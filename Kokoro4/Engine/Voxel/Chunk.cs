using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public class Chunk<T> where T : struct, IEquatable<T>
    {
        class voxelEntry
        {
            public byte x;
            public byte y;
            public T id;
            public byte len;
        }

        private List<voxelEntry>[] voxels;
        private T[] cache;
        private int cache_base;
        private int prev_decomp_z;

        public byte Side { get; private set; }
        public ushort Height { get; private set; }

        public T DefaultID = default(T);

        //GPU data cache is a ~2GB buffer
        //No memory used on empty spaces
        //Start positions represented as a single byte value

        public Chunk(T[] cache, byte side = 16, ushort height = 256, int cache_base = 0)
        {
            Side = side;
            Height = height;
            prev_decomp_z = -1;
            this.cache = cache;
            this.cache_base = cache_base;

            var len = side * side;
            if (cache.Length < len)
                throw new ArgumentException("Cache length too small.");

            voxels = new List<voxelEntry>[height];
            for (int i = 0; i < height; i++)
                voxels[i] = new List<voxelEntry>();
        }

        private void Compress(int z)
        {
            voxels[z].Clear();

            T prevID_b = cache[cache_base];
            ushort runCount_b = 0;

            for (byte y = 0; y < Side; y++)
            {
                List<voxelEntry> ids = new List<voxelEntry>();
                T prevID = cache[cache_base + y * Side];
                byte runCount = 0;
                for (byte x = 0; x < Side; x++)
                {
                    if (cache[cache_base + y * Side + x].Equals(prevID)) runCount++;
                    else
                    {
                        if (!prevID.Equals(DefaultID))
                            ids.Add(new voxelEntry()
                            {
                                x = x,
                                y = y,
                                id = prevID,
                                len = runCount
                            });

                        prevID = cache[cache_base + y * Side + x];
                        runCount = 1;
                    }
                }

                //Pass this run along to the parent
                if (prevID.Equals(prevID_b))
                {
                    runCount_b += runCount;
                }
                else
                {
                    if (!prevID.Equals(DefaultID))
                        voxels[z].Add(new voxelEntry()
                        {
                            x = 0,
                            y = y,
                            id = prevID,
                            len = runCount
                        });

                    if (ids.Count != 0)
                        voxels[z].AddRange(ids);
                }
            }
        }

        private void Decompress(int z)
        {
            if (prev_decomp_z == z)
                return;
            prev_decomp_z = z;

            for (int i = 0; i < Side * Side; i++) cache[cache_base + i] = DefaultID;
            for (int i = 0; i < voxels[z].Count; i++)
            {
                var x_base = voxels[z][i].x;
                var y_base = voxels[z][i].y;
                var id = voxels[z][i].id;
                var cnt = voxels[z][i].len;

                var idx_base = cache_base + y_base * Side + x_base;
                for (int j = 0; j < cnt; j++)
                    cache[idx_base + j] = id;
            }
        }

        public T this[byte x, byte y, int z]
        {
            get
            {
                int idx_base = y * Side + x;
                if (voxels[z].Count > 0)
                    for (int i = voxels[z].Count; i >= 0; i--)
                    {
                        var x_base = voxels[z][i].x;
                        var y_base = voxels[z][i].y;
                        var id = voxels[z][i].id;
                        var cnt = voxels[z][i].len;
                        var idx_base_off = cache_base + y_base * Side + x_base;

                        if (idx_base >= idx_base_off && idx_base < idx_base_off + cnt)
                            return id;
                    }

                return DefaultID;
            }
            set
            {
                int idx_base = y * Side + x;
                if (voxels[z].Count > 0)
                    for (int i = voxels[z].Count; i >= 0; i--)
                    {
                        var x_base = voxels[z][i].x;
                        var y_base = voxels[z][i].y;
                        var id = voxels[z][i].id;
                        var cnt = voxels[z][i].len;
                        var idx_base_off = cache_base + y_base * Side + x_base;

                        if (id.Equals(value)) continue;

                        if (idx_base >= idx_base_off && idx_base < idx_base_off + cnt)
                        {
                            //Split this entry
                            if (cnt == 1)
                                voxels[z][i].id = value;
                            else
                            {
                                byte front_cnt = (byte)(idx_base - idx_base_off);
                                byte back_cnt = (byte)(cnt - 1 - front_cnt);

                                if (front_cnt == 0 && back_cnt > 0)
                                    voxels[z][i--] = new voxelEntry()
                                    {
                                        x = (byte)((x + 1) % Side),
                                        y = (byte)(y + (x + 1 > Side ? 1 : 0)),
                                        id = id,
                                        len = back_cnt
                                    };
                                else if (back_cnt == 0 && front_cnt > 0)
                                    voxels[z][i] = new voxelEntry()
                                    {
                                        x = x_base,
                                        y = y_base,
                                        id = id,
                                        len = front_cnt
                                    };
                                else
                                {
                                    voxels[z][i] = new voxelEntry()
                                    {
                                        x = x_base,
                                        y = y_base,
                                        id = id,
                                        len = front_cnt
                                    };

                                    voxels[z].Insert(i + 1, new voxelEntry()
                                    {
                                        x = (byte)((x + 1) % Side),
                                        y = (byte)(y + (x + 1 > Side ? 1 : 0)),
                                        id = id,
                                        len = back_cnt
                                    });
                                }

                                //Merge the current front entry with the previous one if possible
                                if (i > 0 && front_cnt > 0 && voxels[z][i - 1].id.Equals(id))
                                {
                                    //Check if the previous entry is right before the current entry + is a full set of runs
                                    if (voxels[z][i - 1].x == 0 && (idx_base_off - voxels[z][i - 1].len) / Side == voxels[z][i - 1].y && (voxels[z][i].len + voxels[z][i - 1].len) % Side == 0)
                                    {
                                        //Merge the front entries
                                        voxels[z][i - 1].len += voxels[z][i].len;
                                        voxels[z].RemoveAt(i);
                                    }
                                }

                                //Merge the current rear entry with the next one if possible
                                if (i < voxels[z].Count - 1 && back_cnt > 0 && voxels[z][i + 1].id.Equals(id))
                                {
                                    //Check if the next entry is right after the current entry + is a full set of runs
                                    if (voxels[z][i].x == 0 && (idx_base_off + voxels[z][i].len) / Side == voxels[z][i + 1].y && (voxels[z][i + 1].len + voxels[z][i].len) % Side == 0)
                                    {
                                        //Merge the rear entries
                                        voxels[z][i].len += voxels[z][i + 1].len;
                                        voxels[z].RemoveAt(i + 1);
                                    }
                                }

                                voxels[z].Insert(i + 1, new voxelEntry()
                                {
                                    x = x,
                                    y = y,
                                    id = value,
                                    len = 1
                                });
                            }
                            return;
                        }
                    }

                //If the optimized insertion fails, easier to just unpack and repack
                Decompress(z);
                cache[cache_base + idx_base] = value;
                Compress(z);
            }
        }

    }
}

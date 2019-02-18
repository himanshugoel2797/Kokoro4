using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics.Collision
{
    public class BroadphaseCollisionDetector
    {
        private List<int>[] Sweeps;
        private PhysicsWorld parent;

        public BroadphaseCollisionDetector(PhysicsWorld parent)
        {
            this.parent = parent;

            Sweeps = new List<int>[3];
            for (int i = 0; i < Sweeps.Length; i++)
                Sweeps[i] = new List<int>();
        }

        public void AddObject(int id)
        {
            for (int i = 0; i < Sweeps.Length; i++)
                Sweeps[i].Add(id);
        }

        private void InsertionSort(List<int> list, int idx)
        {
            for (int i = 0; i < list.Count; i++)
                if (!parent.PhysicsObjects.ContainsKey(list[i]))
                {
                    list.RemoveAt(i);
                    i--;
                }

            for (int i = 1; i < list.Count; i++)
                for (int j = i; j > 0 && parent.PhysicsObjects[list[j - 1]].Bounds.Min[idx] > parent.PhysicsObjects[list[j]].Bounds.Min[idx]; j--)
                {
                    int tmp = list[j];
                    list[j] = list[j - 1];
                    list[j - 1] = tmp;
                }
        }

        public void Update()
        {
            HashSet<long> Pairs = new HashSet<long>();
            HashSet<int> ActiveObjects = new HashSet<int>();

            //Sort and find all pairs
            for (int i = 0; i < Sweeps.Length; i++)
            {
                if (Sweeps[i].Count == 0)
                    continue;

                InsertionSort(Sweeps[i], i);

                List<int> Active = new List<int>();

                //Find all pairs
                //TODO: For every following axis, only check previously active objects - this will significantly reduce the number of checks done for highly overlapping axis'

                Active.Add(Sweeps[i][0]);
                for (int k = 1; k < Sweeps[i].Count; k++)
                    for (int j = 0; j < Active.Count; j++)
                    {
                        var sweep_bound = parent.PhysicsObjects[Sweeps[i][k]].Bounds;
                        var active_bound = parent.PhysicsObjects[Active[j]].Bounds;

                        if (active_bound.Max[i] < sweep_bound.Min[i])
                        {
                            Active.RemoveAt(j);
                            j--;

                            Active.Add(Sweeps[i][k]);
                        }
                        else
                        {
                            ActiveObjects.Add(Sweeps[i][k]);
                            ActiveObjects.Add(Active[j]);

                            Pairs.Add(((uint)Sweeps[i][k] << 32) | ((uint)Active[j] & 0xffffffff));
                        }
                    }
            }
        }
    }
}

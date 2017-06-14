using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class TextureCache
    {
        public int CacheSize { get; private set; }
        public Texture Cache { get; private set; }
        
        Dictionary<int, int> TextureAges;
        Dictionary<int, int> TextureTags;
        IOrderedEnumerable<KeyValuePair<int, int>> sortedDictionary;

        public TextureCache(int cacheSize, int w, int h, int levels, PixelFormat fmt, PixelInternalFormat iFmt, PixelType t)
        {
            TextureAges = new Dictionary<int, int>();
            TextureTags = new Dictionary<int, int>();

            CacheSize = cacheSize;
            Cache = new Texture();

            FramebufferTextureSource[] rawTex = new FramebufferTextureSource[CacheSize];
            for (int i = 0; i < rawTex.Length; i++)
            {
                rawTex[i] = new FramebufferTextureSource(w, h, levels)
                {
                    InternalFormat = iFmt,
                    PixelType = t
                };
            }

            ArrayTextureSource arrayTex = new ArrayTextureSource(w, h, CacheSize, levels, fmt, t, rawTex);

            //Setup all the layers
            for (int i = 0; i < rawTex.Length; i++)
            {
                Cache.SetData(arrayTex, 0);
                TextureAges[i] = 0;
                TextureTags[i] = -1;
            }
        }

        public bool Use(int idx, int tag)
        {
            if (idx < 0)
                return false;

            if (TextureTags[idx] != tag)
                return false;

            TextureAges[idx]++;
            sortedDictionary = TextureAges.OrderBy(kvp => kvp.Value);
            return true;
        }
        
        public int Allocate(int tag)
        {
            if (sortedDictionary == null) sortedDictionary = TextureAges.OrderBy(kvp => kvp.Value);
            int idx = sortedDictionary.First().Key;
            TextureTags[idx] = tag;
            return idx;
        }
    }
}

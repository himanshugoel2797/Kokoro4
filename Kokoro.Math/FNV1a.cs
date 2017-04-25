using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Math
{
    public sealed class FNV1a : HashAlgorithm
    {
        private const uint FnvPrime = unchecked(16777619);

        private const uint FnvOffsetBasis = unchecked(2166136261);

        private uint hash;

        public FNV1a()
        {
            this.Reset();
        }

        public override void Initialize()
        {
            this.Reset();
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            for (var i = ibStart; i < cbSize; i++)
            {
                unchecked
                {
                    this.hash ^= array[i];
                    this.hash *= FnvPrime;
                }
            }
        }

        protected override byte[] HashFinal()
        {
            return BitConverter.GetBytes(this.hash);
        }

        private void Reset()
        {
            this.hash = FnvOffsetBasis;
        }

        public long CalculateHash(float a, float b)
        {
            List<byte> n = new List<byte>();
            n.AddRange(BitConverter.GetBytes(a));
            n.AddRange(BitConverter.GetBytes(b));

            Reset();
            HashCore(n.ToArray(), 0, n.Count);
            long r = hash;
            Reset();
            return r;
        }
    }
}

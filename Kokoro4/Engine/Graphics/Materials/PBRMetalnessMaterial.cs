using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Materials
{
    public class PBRMetalnessMaterial : Material
    {
        public Texture Albedo { get; set; }
        public Texture MetalRoughnessDerivative { get; set; }

        public TextureSampler AlbedoSampler { get; set; }
        public TextureSampler MetalRoughnessDerivativeSampler { get; set; }

        public override int PropertyCount => 2;

        public override int PropertySize => 16;

        public PBRMetalnessMaterial(string name) : base(name)
        {
            TypeIndex = 0;
        }

        public override void Dispose()
        {

        }

        public override byte[] GetProperty(int idx)
        {
            if(idx == 0)
            {
                var handle0 = Albedo.GetHandle(AlbedoSampler);
                return BitConverter.GetBytes(handle0);
            }
            if(idx == 1)
            {
                var handle1 = MetalRoughnessDerivative.GetHandle(MetalRoughnessDerivativeSampler);
                return BitConverter.GetBytes(handle1);
            }

            throw new ArgumentOutOfRangeException();
        }

        public override void MakeResident()
        {
            Albedo.GetHandle(AlbedoSampler).SetResidency(Residency.Resident);
            MetalRoughnessDerivative.GetHandle(MetalRoughnessDerivativeSampler).SetResidency(Residency.Resident);
        }

        public override void MakeNonResident()
        {
            Albedo.GetHandle(AlbedoSampler).SetResidency(Residency.NonResident);
            MetalRoughnessDerivative.GetHandle(MetalRoughnessDerivativeSampler).SetResidency(Residency.NonResident);
        }
    }
}

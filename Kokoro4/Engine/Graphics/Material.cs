using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class Material : EngineRenderable
    {
        public bool Transparent { get; set; }
        public ShaderProgram Shader { get; private set; }
        public bool DepthWrite { get; private set; }
        public BlendFactor Src { get; private set; }
        public BlendFactor Dst { get; private set; }
        public CullFaceMode CullMode { get; private set; }

        public MaterialParameters GlobalParameters { get; private set; }
        public ParameterInterface GlobalParameterInterface { get; private set; }
        public ParameterInterface PerDrawParameterInterface { get; private set; }

        //TODO:
        //Allow specification of parameter interfaces
        //Parse the shader parameter usage statements before compilation and substitute them with parameter fetching code based on parameter interface specification
        //

        //Each object specifies its material parameters, which can then be grouped together into various buffers based on usage
        //Generate parameter blocks automatically by parsing shader parameter usage 
        //RenderQueues bucket based on Material and batch together material parameters for draws

        public Material(string name)
        {
            this.Name = name;
        }

        public void AddShaderSource(ShaderSource src)
        {

        }

        public void SetGlobalParameterInterface(ParameterInterface i)
        {
            GlobalParameterInterface = i;
        }

        public void SetPerDrawParameterInterface(ParameterInterface i)
        {
            PerDrawParameterInterface = i;
        }

        public void Link()
        {

        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

using Kokoro4.ContentProcessor.Material;
using Kokoro4.ContentProcessor.Mesh;
using Kokoro4.ContentProcessor.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace Kokoro4.ContentPipeline
{
    enum ProcessType
    {
        None,
        Mesh,
        Scene,
        Material,
        Texture,
        Sound
    }

    class Program
    {
        static void Main(string[] args)
        {
            ProcessType pType = ProcessType.None;

            switch (args[0])
            {
                case "-mesh":
                    pType = ProcessType.Mesh;
                    break;
                case "-scene":
                    pType = ProcessType.Scene;
                    break;
                case "-tex":
                    pType = ProcessType.Texture;
                    break;
                case "-mat":
                    pType = ProcessType.Material;
                    break;
                case "-snd":
                    pType = ProcessType.Sound;
                    break;
            }

            switch (pType)
            {
                case ProcessType.Mesh:
                    MeshProcessor.Preprocess("", 1, "");
                    break;
                case ProcessType.Scene:

                    break;
                case ProcessType.Material:
                    MaterialProcessor.Preprocess("", "", "");
                    break;
                case ProcessType.Texture:
                    TextureProcessor.Preprocess(args);
                    break;
                case ProcessType.Sound:

                    break;
                default:
                    WriteLine("Usage: Kokoro4.ContentPipeline [process type] [options]");
                    WriteLine("process type: -mesh -scene -tex -mat -snd");
                    WriteLine("-mesh options:");
                    WriteLine(MeshProcessor.GetHelp());
                    //WriteLine("-scene options:");
                    //WriteLine(SceneProcessor.GetHelp());
                    WriteLine("-tex options:");
                    WriteLine(TextureProcessor.GetHelp());
                    WriteLine("-mat options:");
                    WriteLine(MaterialProcessor.GetHelp());
                    //WriteLine("-snd options:");
                    //WriteLine(SoundProcessor.GetHelp());
                    break;
            }
        }
    }
}

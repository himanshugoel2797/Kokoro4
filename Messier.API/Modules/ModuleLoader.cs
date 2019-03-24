using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Modules
{
    public class ModuleLoader
    {
        private static string[] ModulePaths;
        private static bool[] ModulesLoaded;
        private static List<Assembly> assemblies;
        private static List<ModuleDesc> moduleDescs;

        public static IReadOnlyList<Assembly> LoadedModules { get => assemblies; }
        public static IReadOnlyList<ModuleDesc> ModuleDescs { get => moduleDescs; }

        public static void Setup()
        {
            ModulePaths = Directory.GetFiles("Modules", "*.dll", SearchOption.AllDirectories);
            ModulesLoaded = new bool[ModulePaths.Length];
            assemblies = new List<Assembly>();
            moduleDescs = new List<ModuleDesc>();

            var domain = AppDomain.CurrentDomain;
            domain.AssemblyResolve += Domain_AssemblyResolve;
        }

        private static Assembly LoadAssembly(int i)
        {
            var asm = Assembly.LoadFile(Path.GetFullPath(ModulePaths[i]));

            //Find and initialize the ModuleDesc
            var descs = asm.GetExportedTypes().Where(a => a.IsSubclassOf(typeof(ModuleDesc)));
            foreach (Type t in descs)
            {
                var mdesc = (ModuleDesc)t.InvokeMember("", BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, null, null);
                moduleDescs.Add(mdesc);
            }

            ModulesLoaded[i] = true;
            assemblies.Add(asm);
            return asm;
        }

        private static Assembly Domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            for (int i = 0; i < ModulePaths.Length; i++)
                if (!ModulesLoaded[i] && Path.GetFileNameWithoutExtension(ModulePaths[i]) == args.Name)
                    return LoadAssembly(i);
            return null;
        }

        public static void LoadAll()
        {
            for (int i = 0; i < ModulePaths.Length; i++)
                if (!ModulesLoaded[i])
                    try
                    {
                        var asm = LoadAssembly(i);
                    }
                    catch (BadImageFormatException)
                    {
                        //Not a .NET assembly
                        Logger.Error($"Bad Image Format: {ModulePaths[i]}");
                    }
                    catch (Exception)
                    {
                        throw new Exception($"Failed to find a dependency required by {Path.GetFileNameWithoutExtension(ModulePaths[i])}.");
                    }

            var mdesc_not_initable = new bool[moduleDescs.Count];
            int initCnt = 0, initTgt = ModuleDescs.Count;

            while (initCnt < initTgt)
                for (int i = 0; i < ModuleDescs.Count; i++)
                    if (!ModuleDescs[i].Initialized && !mdesc_not_initable[i])
                        try
                        {
                            Logger.Log($"Initializing {ModuleDescs[i].Name}.");
                            ModuleDescs[i].Initialize();
                            if (ModuleDescs[i].Initialized)
                            {
                                Logger.Log("Initialized.");
                                initCnt++;
                            }
                            else
                            {
                                Logger.Log("Initialization Failed.");
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log("Skipping, Initialization Exception:");
                            Logger.Error(e.Message);
                            initTgt--;

                            mdesc_not_initable[i] = true;
                        }

            int rmCnt = 0;
            for (int i = 0; i < mdesc_not_initable.Length; i++)
                if (mdesc_not_initable[i])
                {
                    moduleDescs.RemoveAt(i - rmCnt);
                    rmCnt++;
                }
        }
    }
}

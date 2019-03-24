using Messier.Base.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Modules
{
    public class Configuration
    {
        #region JSON Structure
        /*Load json file
        Read key value pairs, create instances of the specified types and setup their values to the specified keys
        {
            "type" : "",
            "name" : "",
            "entries" : [
                { "key" : "", "s_value" : "" },
                { "key" : "", "o_value" : { "type" : "", "entries" : [  ] } },
                { "key" : "", "s_value" : "" },
                { "key" : "", "s_value" : "" },
                { "key" : "", "s_value" : "" },
            ]
        }
        */

        struct ConfigurationKeyValuePairs
        {
            public string key;

            public string s_value;
            public ConfigurationEntry o_value;
        }

        class ConfigurationEntry
        {
            public string type;
            public string name;
            public ConfigurationKeyValuePairs[] entries;
        }
        #endregion

        private static IConfigurable Load(ConfigurationEntry entry)
        {
            foreach (Assembly asm in ModuleLoader.LoadedModules)
            {
                var type = asm.GetType(entry.type, false);
                if (type != null)
                {
                    //Make sure that this type is configurable
                    if (type.GetInterface(nameof(IConfigurable), false) == null)
                        continue;

                    //Create a new instance of the type and populate it
                    var obj = type.InvokeMember("", BindingFlags.CreateInstance, null, null, null);
                    var iface = (IConfigurable)obj;
                    iface.Name = entry.name;

                    for (int i = 0; i < entry.entries.Length; i++)
                        try
                        {
                            if (entry.entries[i].o_value != null)
                                iface.Populate(entry.entries[i].key, Load(entry.entries[i].o_value));
                            else
                                iface.Populate(entry.entries[i].key, entry.entries[i].s_value);
                        }catch(Exception e)
                        {
                            Logger.Log($"Key-Value load error for {entry.entries[i].key}");
                            Logger.Error(e.Message);
                        }

                    return iface;
                }
            }

            throw new ConfigurationLoadException($"Could not find the associated module for {entry.type}");
        }

        public static IConfigurable LoadFile(string path)
        {
            Logger.Log($"Loading configuration at {path}");
            return Load(FileManager.ReadAllText(path));
        }

        public static IConfigurable Load(string s)
        {
            ConfigurationEntry entry = null;

            JsonSerializer serializer = new JsonSerializer();
            using (var s_reader = new StringReader(s))
            using (JsonTextReader reader = new JsonTextReader(s_reader))
                try
                {
                    entry = (ConfigurationEntry)serializer.Deserialize(reader, typeof(ConfigurationEntry));
                }
                catch (JsonSerializationException)
                {
                    return null;
                }

            if (entry != null)
                return Load(entry);

            throw new Exception("Json is malformed.");
        }
    }
}

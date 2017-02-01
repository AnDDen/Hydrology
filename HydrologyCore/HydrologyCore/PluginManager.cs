using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HydrologyCore
{
    public class PluginManager
    {
        private IDictionary<string, Type> algorithmTypes;
        public IDictionary<string, Type> AlgorithmTypes { get { return algorithmTypes; } }

        private static PluginManager instance;
        public static PluginManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PluginManager();
                return instance;
            }
        }

        private PluginManager()
        {
            algorithmTypes = new Dictionary<string, Type>();
        }

        public void LoadPlugins(string pluginRelativeDir, string assemblyPattern)
        {
            string pluginDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + pluginRelativeDir;
            string[] files = Directory.GetFiles(pluginDir);
            string[] assemblyFiles = Directory.GetFiles(pluginDir, assemblyPattern);

            foreach (string assemblyFile in assemblyFiles)
            {
                LoadPlugin(assemblyFile);
            }
        }

        public void LoadPlugin(string file)
        {
            try
            {
                Assembly assembly = Assembly.LoadFile(file);
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.GetInterfaces().Contains(typeof(IAlgorithm)) && type.IsClass && !type.IsAbstract && CheckAlgorithmType(type))
                    {
                        algorithmTypes[type.Name] = type;
                    }
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Error loading plugin {0}", file);
            }
        }

        private bool CheckAlgorithmType(Type algorithmType)
        {
            // todo: добавить проверку на правильность типа (наличие необходимых атрибутов, правильность типов входных/выходных данных)
            return true;
        }
    }
}

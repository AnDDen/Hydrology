using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;
using System.IO;
using System.Reflection;
using System.Data;

namespace HydrologyCore
{
    public class Core
    {
        private const string pluginRelativeDir = @"\Algorithms",
                             assemblyPattern = "*.dll";

        private IList<Type> algorithmTypes;

        public IList<Type> AlgorithmTypes
        {
            get { return algorithmTypes; }
        }

        public Core()
        {
            algorithmTypes = new List<Type>();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            string pluginDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + pluginRelativeDir;
            string[] files = Directory.GetFiles(pluginDir);
            string[] assemblyFiles = Directory.GetFiles(pluginDir, assemblyPattern);

            foreach (string assemblyFile in assemblyFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(assemblyFile);
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.GetInterfaces().Contains(typeof(IAlgorithm)) && type.IsClass && !type.IsAbstract)
                        {
                            algorithmTypes.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error loading plugin {0}", assemblyFile);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.IO;
using System.Reflection;
using System.Data;
using HydrologyCore.ExperimentNodes;

namespace HydrologyCore
{
    public class Core
    {
        private const string pluginRelativeDir = @"\Algorithms",
                             assemblyPattern = "*.dll";

        private IDictionary<string, Type> algorithmTypes;

        public IDictionary<string, Type> AlgorithmTypes
        {
            get { return algorithmTypes; }
        }

        public Core()
        {
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            algorithmTypes = new Dictionary<string, Type>();

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
                            algorithmTypes[type.Name] = type;
                        }
                    }
                }
                catch (Exception)
                {
                    Console.Error.WriteLine("Error loading plugin {0}", assemblyFile);
                }
            }
        }

        public AlgorithmNode Algorithm(string name)
        {
            if (algorithmTypes.ContainsKey(name))
                return new AlgorithmNode(algorithmTypes[name]);
            throw new ApplicationException("Can not find algorithm " + name);
        }

        public RunProcessNode RunProcess(string name)
        {
            return new RunProcessNode(name);
        }

        public LoopNode Loop(string name, IList<IExperimentNode> body, string loopVar, double initValue, double endValue, double step)
        {
            return new LoopNode(body, name, loopVar, initValue, endValue, step);
        }
    }
}

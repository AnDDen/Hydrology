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
        private const string pluginRelativeDir = @"\Plugins",
                             assemblyPattern = ".dll";

        private IList<IAlgorithm> algorithms;

        public IList<IAlgorithm> Algorithms
        {
            get { return algorithms; }
        }

        public IList<Experiment> Experiments { get; set; }

        public Core()
        {
            algorithms = new List<IAlgorithm>();
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            string pluginDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + pluginRelativeDir;
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
                            algorithms.Add((IAlgorithm)Activator.CreateInstance(type));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error loading plugin {0}", assemblyFile);
                }
            }
        }

        public DataSet RunExperiment(Experiment experiment, DataSet data)
        {
            experiment.Init(data);
            experiment.Run();
            return experiment.Result;
        }
    }
}

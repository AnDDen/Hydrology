using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.IO;
using System.Reflection;
using System.Data;
using HydrologyCore.Experiment.Nodes;
using HydrologyCore.Data;

namespace HydrologyCore
{
    public class Core
    {
        private static Core instance;
        public static Core Instance
        {
            get
            {
                if (instance == null)
                    instance = new Core();
                return instance;
            }
        }

        private const string pluginRelativeDir = @"\Algorithms", assemblyPattern = "*.dll";

        private Core()
        {
            PluginManager.Instance.LoadPlugins(pluginRelativeDir, assemblyPattern);
        }

        public Experiment.Experiment NewExperiment()
        {
            return new Experiment.Experiment();
        }
    }
}

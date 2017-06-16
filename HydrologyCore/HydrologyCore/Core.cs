﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.IO;
using System.Reflection;
using System.Data;
using System.ComponentModel;

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
            
        }

        public void LoadPlugins()
        {
            PluginManager.Instance.LoadPlugins(pluginRelativeDir, assemblyPattern);
        }

        public Experiment.Experiment NewExperiment()
        {
            return new Experiment.Experiment();
        }

        public void UpdateWorker(BackgroundWorker worker, string nodeName)
        {
            if (worker != null)
            {
                if (worker.WorkerReportsProgress)
                {
                    worker.ReportProgress(0, nodeName);
                }
            }
        }

        public bool CheckWorkerCancel(BackgroundWorker worker)
        {
            if (worker != null)
            {
                return worker.CancellationPending;
            }
            return false;
        }
    }
}

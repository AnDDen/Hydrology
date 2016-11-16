using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;

namespace HydrologyCore
{
    public class RunProcessNode : IExperimentNode
    {
        public IExperimentNode Next { get; set; }
        public IExperimentNode Prev { get; set; }

        public string ProcessName { get; set; }

        public DataSet Results
        {
            get { return null; }
        }

        public string Name { get; set; }
        public string SaveResultsFolder { get; set; }

        public bool IsSaveResults { get { return false; } set { } }
        public string SaveResultsPath { get; set; }

        public bool IsStoreInContext { get { return false; } }

        public RunProcessNode(string processName)
        {
            ProcessName = processName;
        }

        public void Run(IContext ctx = null)
        {
            Process process = new Process();
            process.StartInfo.FileName = ProcessName;
            process.Start();
        }
                
        public void SaveResults() { }        
    }
}

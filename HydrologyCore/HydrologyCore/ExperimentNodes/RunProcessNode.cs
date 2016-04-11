using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
{
    public class RunProcessNode : IExperimentNode
    {
        public IExperimentNode Next { get; set; }
        public IExperimentNode Prev { get; set; }

        public string ProcessName { get; set; }

        public RunProcessNode(string processName)
        {
            ProcessName = processName;
        }

        public void Run()
        {
            Process process = new Process();
            process.StartInfo.FileName = ProcessName;
            process.Start();
        }
    }
}

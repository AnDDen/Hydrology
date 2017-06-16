using HydrologyCore.Context;
using HydrologyCore.Events;
using HydrologyCore.Experiment.Nodes;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace HydrologyCore.Experiment
{
    public class Experiment
    {
        public Block Block { get; set; }

        private string path;
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public string Name { get; set; }

        public event NodeExecutionStartEventHandler NodeExecutionStart;
        public event NodeStatusChangedEventHandler NodeStatusChanged;

        public event ExperimentExecutedEventHandler ExperimentExecuted;

        protected void InvokeExperimentExecuted(IContext ctx)
        {
            ExperimentExecuted?.Invoke(this, new ExperimentExecutedEventArgs(ctx));
        }

        public Experiment()
        {
            Block = new Block(null, null);
            Block.ExecutionStart += (sender, e) => { NodeExecutionStart?.Invoke(sender, e); };
            Block.StatusChanged += (sender, e) => { NodeStatusChanged?.Invoke(sender, e); };
            Name = "Experiment.$?NOW?$";
        }

        private string ReplaceMacros(string str)
        {
            string res = str;
            if (res.Contains("$?NOW?$"))
                res = res.Replace("$?NOW?$", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            if (res.Contains("$?USERNAME?$"))
                res = res.Replace("$?USERNAME?$", Environment.UserName);
            return res;
        }

        private string GetPathPrefix()
        {
            string res = "";
            if (!string.IsNullOrWhiteSpace(path))
            {
                res += path;
                if (res.Last() != '/')
                    res += "/";
            }
            res += ReplaceMacros(Name);
            if (res.Last() == '/')
                res = res.Substring(0, res.Length - 1);
            return res;
        }

        public void Run(BackgroundWorker worker)
        {
            IContext ctx = new BlockContext(Block, null);
            try
            {
                Block.PathPrefix = GetPathPrefix();
                if (!Directory.Exists(Block.PathPrefix))
                {
                    try
                    {
                        Directory.CreateDirectory(Block.PathPrefix);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Unable to create directory {Block.PathPrefix} for experiment", e);
                    }
                }          
                Block.Run(ctx, worker);
                Core.Instance.UpdateWorker(worker, null);
            }
            finally
            {
                InvokeExperimentExecuted(ctx);
            }
        }
    }
}

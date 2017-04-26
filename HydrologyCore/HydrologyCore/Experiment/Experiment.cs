using HydrologyCore.Context;
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
        
        public Experiment()
        {
            Block = new Block(null, null);
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
            var count = Block.TotalNodeCount() * 2;
            int current = 0;
            IContext ctx = new BlockContext(Block, null);
            Block.Run(ctx, worker, count, ref current);
            Core.Instance.UpdateWorker(worker, 1, 1, null);
        }
    }
}

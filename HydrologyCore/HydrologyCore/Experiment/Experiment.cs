using HydrologyCore.Experiment.Nodes;
using System;
using System.ComponentModel;

namespace HydrologyCore.Experiment
{
    public class Experiment
    {
        public NodeContainer NodeContainer { get; set; }

        public string Path
        {
            get { return NodeContainer.Path; }
            set { NodeContainer.Path = value; }
        }
        
        public Experiment()
        {
            NodeContainer = new NodeContainer();
        }

        public void Run()
        {
            NodeContainer.Run();
        }

        public void Run(BackgroundWorker worker)
        {
            var count = NodeContainer.TotalNodeCount() * 2;
            int current = 0;
            NodeContainer.Run(worker, count, ref current);
            Core.Instance.UpdateWorker(worker, 1, 1, null);
        }

        public AbstractNode ResolveNode(string name)
        {
            return NodeContainer.ResolveNode(name);
        }
    }
}

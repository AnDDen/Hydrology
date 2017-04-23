using HydrologyCore.Experiment.Nodes;
using System;
using System.ComponentModel;

namespace HydrologyCore.Experiment
{
    public class Experiment
    {
        public Block Block { get; set; }

        public string Path
        {
            get { return Block.Path; }
            set { Block.Path = value; }
        }
        
        public Experiment()
        {
            Block = new Block(null, null);
        }

        public void Run()
        {
            Context ctx = new Context();
            Block.Run(ctx);
        }

        public void Run(BackgroundWorker worker)
        {
            var count = Block.TotalNodeCount() * 2;
            int current = 0;
            Context ctx = new Context();
            Block.Run(ctx, worker, count, ref current);
            Core.Instance.UpdateWorker(worker, 1, 1, null);
        }
    }
}

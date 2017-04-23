using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using static System.Convert;
using static System.Math;

namespace HydrologyCore.Experiment.Nodes
{
    // todo : переделать в блок + алгоритм для переменной цикла
    public class LoopBlock : Block
    {
        public double Value { get; set; }

        public LoopBlock(string name, Block parent) : base(name, parent)
        {
        }

        public override void Run(Context ctx)
        {
            /* for (int i  = 0; i < values.Length; i++)
            {
                Value = values[i];
                base.Run(ctx);
            } */
        }

        public override int TotalNodeCount()
        {
            return base.TotalNodeCount();
        }
    }
}

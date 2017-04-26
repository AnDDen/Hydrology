using HydrologyCore.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using static System.Convert;
using static System.Math;

namespace HydrologyCore.Experiment.Nodes
{
    public class LoopBlock : Block
    {
        public LoopPortNode LoopPortNode { get; set; }

        public override string Path => (PathPrefix ?? "") + Parent?.Path + "/" + Name ?? "" + " [" + currentValue + "]";

        private object currentValue;

        public LoopBlock(string name, Block parent) : base(name, parent)
        {
        }

        public override void Run(IContext ctx, BackgroundWorker worker, int count, ref int current)
        {
            Debug.Assert(ctx is LoopContext);
            LoopContext loopCtx = ctx as LoopContext;
            object varArray = ctx.GetPortValue(LoopPortNode.BlockPort);
            Array array = (Array) varArray;
            for (int i = 0; i < array.Length; i++)
            {
                currentValue = array.GetValue(i);
                loopCtx.NextIteration(currentValue);
                base.Run(ctx, worker, count, ref current);
            }
        }

        public override int TotalNodeCount()
        {
            return base.TotalNodeCount();
        }
    }
}

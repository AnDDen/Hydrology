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

        public override string GetPath() => (PathPrefix ?? "") + Parent?.GetPath() + "/" + (Name ?? "") + " [" + currentValue + "]";

        private object currentValue;

        public LoopBlock(string name, Block parent) : base(name, parent)
        {
        }

        public override void Run(IContext ctx, BackgroundWorker worker)
        {
            try
            {
                InvokeExecutionStart(ctx);
                Debug.Assert(ctx is LoopContext);
                LoopContext loopCtx = ctx as LoopContext;
                object varArray = ctx.GetPortValue(LoopPortNode.BlockPort);
                Array array = (Array)varArray;
                for (int i = 0; i < array.Length; i++)
                {
                    if (worker.CancellationPending)
                        return;
                    currentValue = array.GetValue(i);
                    loopCtx.NextIteration(currentValue);
                    ChangeStatus(ctx, ExecutionStatus.NEXT_ITER, null);
                    base.Run(ctx, worker);
                }
                ChangeStatus(ctx, ExecutionStatus.SUCCESS, null);
            }
            catch (Exception e)
            {
                ChangeStatus(ctx, ExecutionStatus.ERROR, e);
                throw e;
            }
        }
    }
}

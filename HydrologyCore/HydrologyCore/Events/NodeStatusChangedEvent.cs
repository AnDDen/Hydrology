using HydrologyCore.Context;
using HydrologyCore.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Events
{
    public class NodeStatusChangedEventArgs
    {
        public IContext Context { get; private set; }

        public IRunable Node => Context.Owner;
        public ExecutionStatus Status => Context.Status;
        public Exception Error => Context.Error;

        public NodeStatusChangedEventArgs(IContext ctx)
        {
            Context = ctx;
        }

        public NodeStatusChangedEventArgs(IContext ctx, IRunable node)
        {
            Context = ctx.GetContext(node);
        }
    }

    public delegate void NodeStatusChangedEventHandler(object sender, NodeStatusChangedEventArgs e);
}

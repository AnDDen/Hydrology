using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Context
{
    public class BlockContext : IContext
    {
        public IRunable Owner { get; }
        public BlockContext ParentContext { get; }

        public IDictionary<Port, object> Inputs { get; } = new Dictionary<Port, object>();
        public IDictionary<Port, object> Outputs { get; } = new Dictionary<Port, object>();

        public virtual IList<IContext> Children { get; } = new List<IContext>();

        public BlockContext(Block owner, BlockContext parentCtx)
        {
            Owner = owner;
            ParentContext = parentCtx;
            foreach (var node in owner.Nodes)
            {
                if (node is Block)
                {
                    if (node is LoopBlock)
                        Children.Add(new LoopContext(node as LoopBlock, this));
                    else
                        Children.Add(new BlockContext(node as Block, this));
                }
                else
                    Children.Add(new NodeContext(node, this));
            }
        }

        protected void SetInput(Port port, object value) => Inputs.Add(port, value);

        protected object GetInput(Port port) => Inputs.ContainsKey(port) ? Inputs[port] : null;

        protected void SetOutput(Port port, object value) => Outputs.Add(port, value);

        protected object GetOutput(Port port) => Outputs.ContainsKey(port) ? Outputs[port] : null;

        public object GetPortValue(Port port)
        {
            if (port.Owner == Owner)
            {
                return Outputs.ContainsKey(port) ? Outputs[port] :
                Inputs.ContainsKey(port) ? Inputs[port] :
                null;
            }
            else
            {
                IContext ctx = Children.FirstOrDefault(c => c.Owner == Owner);
                if (ctx != null)
                {
                    return ctx.GetPortValue(port);
                }
            }
            return null;
        }

        public void SetPortValue(Port port, object value)
        {
            if (port.Owner == Owner)
            {
                if (port.Owner.InPorts.Contains(port))
                    SetInput(port, value);
                else if (port.Owner.OutPorts.Contains(port))
                    SetOutput(port, value);
            }
            else
            {
                IContext ctx = Children.FirstOrDefault(c => c.Owner == Owner);
                if (ctx != null)
                {
                    ctx.SetPortValue(port, value);
                }
            }
        }

        public IContext GetContext(IRunable node)
        {
            if (Owner == node)
                return this;
            foreach (var child in Children)
            {
                var ctx = child.GetContext(node);
                if (ctx != null)
                    return ctx;
            }
            return null;
        }
    }
}

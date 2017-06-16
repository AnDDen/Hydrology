using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Context
{
    public class LoopContext : BlockContext
    {
        public object CurrentVarValue { get; private set; }

        public IDictionary<object, IList<IContext>> IterationContexts { get; } = new Dictionary<object, IList<IContext>>();

        private IList<IContext> children = new List<IContext>();

        public override IList<IContext> Children => CurrentVarValue != null ? IterationContexts[CurrentVarValue] : children;

        public LoopContext(LoopBlock owner, BlockContext parentCtx) : base(owner, parentCtx)
        {
        }

        public void NextIteration(object varValue)
        {
            IterationContexts.Add(varValue, new List<IContext>());
            CurrentVarValue = varValue;
        }

        public object GetPortValue(object varValue, Port port)
        {
            if (port.Owner == Owner)
            {
                return Outputs.ContainsKey(port) ? Outputs[port] :
                Inputs.ContainsKey(port) ? Inputs[port] :
                null;
            }
            else
            {
                IContext ctx = IterationContexts[varValue].FirstOrDefault(c => c.Owner == Owner);
                if (ctx != null)
                {
                    return ctx.GetPortValue(port);
                }
            }
            return null;
        }

        public void SetPortValue(object varValue, Port port, object value)
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
                IContext ctx = IterationContexts[varValue].FirstOrDefault(c => c.Owner == Owner);
                if (ctx != null)
                {
                    ctx.SetPortValue(port, value);
                }
            }
        }
    }
}

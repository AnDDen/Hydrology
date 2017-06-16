using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HydrologyCore.Experiment;

namespace HydrologyCore.Context
{
    public class NodeContext : IContext
    {
        public IRunable Owner { get; }
        public BlockContext ParentContext { get; }

        public IDictionary<Port, object> Inputs { get; } = new Dictionary<Port, object>();
        public IDictionary<Port, object> Outputs { get; } = new Dictionary<Port, object>();

        public ExecutionStatus Status { get; set; } = ExecutionStatus.EXECUTING;
        public Exception Error { get; set; } = null;

        public NodeContext(IRunable owner, BlockContext parentCtx)
        {
            Owner = owner;
            ParentContext = parentCtx;
        }

        private void SetInput(Port port, object value) => Inputs.Add(port, value);

        private object GetInput(Port port) => Inputs.ContainsKey(port) ? Inputs[port] : null;

        private void SetOutput(Port port, object value) => Outputs.Add(port, value);

        private object GetOutput(Port port) => Outputs.ContainsKey(port) ? Outputs[port] : null;

        public object GetPortValue(Port port) =>
            Outputs.ContainsKey(port) ? Outputs[port] :
            Inputs.ContainsKey(port) ? Inputs[port] :
            null;

        public void SetPortValue(Port port, object value)
        {
            if (port.Owner.InPorts.Contains(port))
                SetInput(port, value);
            else if (port.Owner.OutPorts.Contains(port))
                SetOutput(port, value);
        }

        public IContext GetContext(IRunable node)
        {
            if (Owner == node)
                return this;
            return null;
        }

        public void SetStatus(IRunable node, ExecutionStatus status)
        {
            if (Owner != node)
                return;
            Status = status;
        }

        public void SetError(IRunable node, Exception error)
        {
            if (Owner != node)
                return;
            Error = error;
        }
    }
}

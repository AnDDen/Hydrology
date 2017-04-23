using HydrologyCore.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
{
    public class NodeContext
    {
        public IRunable Node { get; }
        private IDictionary<Port, object> inputs = new Dictionary<Port, object>();
        private IDictionary<Port, object> outputs = new Dictionary<Port, object>();

        public NodeContext(IRunable node)
        {
            Node = node;
        }

        private void AddInput(Port port, object value) => inputs.Add(port, value);

        private object GetInput(Port port) => inputs.ContainsKey(port) ? inputs[port] : null;

        private void AddOutput(Port port, object value) => outputs.Add(port, value);

        private object GetOutput(Port port) => outputs.ContainsKey(port) ? outputs[port] : null;

        public object GetPortValue(Port port) =>
            outputs.ContainsKey(port) ? outputs[port] :
            inputs.ContainsKey(port) ? inputs[port] :
            null;

        public void AddPortValue(Port port, object value)
        {
            if (port.Owner.InPorts.Contains(port))
                AddInput(port, value);
            else if (port.Owner.OutPorts.Contains(port))
                AddInput(port, value);
        }
    }
}

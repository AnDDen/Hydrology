using HydrologyCore.Experiment;
using System.Collections.Generic;

namespace HydrologyCore
{
    public class Context
    {
        private Dictionary<IRunable, NodeContext> nodeContexts = new Dictionary<IRunable, NodeContext>();

        public object GetPortValue(Port port) =>
            nodeContexts.ContainsKey(port.Owner) ? nodeContexts[port.Owner].GetPortValue(port) : null;

        public T GetPortValue<T>(Port port) => (T)GetPortValue(port);

        public void AddPortValue(Port port, object value)
        {
            if (!nodeContexts.ContainsKey(port.Owner))
                nodeContexts.Add(port.Owner, new NodeContext(port.Owner));
            nodeContexts[port.Owner].AddPortValue(port, value);
        }
    }
}

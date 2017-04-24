using HydrologyCore.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment.Nodes
{
    public class OutPortNode : PortNode
    {
        public OutPortNode(string name, string description, DataType dataType, Type elementType, Block parent)
            : base(name, description, dataType, elementType, parent)
        {
            InPorts.Add(Port);
        }

        public override void Run(IContext ctx)
        {
            ctx.SetPortValue(BlockPort, ctx.GetPortValue(Port));
        }
    }
}

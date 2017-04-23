using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment.Nodes
{
    public class InPortNode : PortNode
    {
        public InPortNode(string name, string description, DataType dataType, Type elementType, Block parent) 
            : base(name, description, dataType, elementType, parent)
        {
            OutPorts.Add(Port);
        }

        public override void Run(Context ctx)
        {
            ctx.AddPortValue(Port, ctx.GetPortValue(BlockPort));
        }
    }
}

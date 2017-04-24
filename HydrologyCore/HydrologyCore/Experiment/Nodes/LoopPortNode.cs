using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HydrologyCore.Context;

namespace HydrologyCore.Experiment.Nodes
{
    public class LoopPortNode : InPortNode
    {
        public override DataType DataType
        {
            get { return Port.DataType; }
            set { }
        }

        public LoopPortNode(string name, string description, Type elementType, Block parent) 
            : base(name, description, DataType.VALUE, elementType, parent)
        {
            Port.DataType = DataType.VALUE;
            BlockPort.DataType = DataType.ARRAY;
        }

        public override void Run(IContext ctx)
        {
            ctx.SetPortValue(Port, (ctx as LoopContext).CurrentVarValue);
        }
    }
}

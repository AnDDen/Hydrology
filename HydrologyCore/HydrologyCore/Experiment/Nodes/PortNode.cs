using HydrologyCore.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment.Nodes
{
    public abstract class PortNode : AbstractNode
    {
        public Port Port { get; }
        public Port BlockPort { get; }

        public override string Name
        {
            get { return Port.Name; }
            set
            {
                if (Port != null)
                {
                    Port.Name = value;
                    Port.DisplayedName = value;
                    BlockPort.Name = value;
                    BlockPort.DisplayedName = value;
                    InvokeNameChanged(Port.Name);
                }
            }
        }

        public string Description
        {
            get { return Port.Description; }
            set
            {
                Port.Description = value;
                BlockPort.Description = value;
            }
        }

        public Type ElementType
        {
            get { return Port.ElementType; }
            set
            {
                Port.ElementType = value;
                BlockPort.ElementType = value;
            }
        }

        public virtual DataType DataType
        {
            get { return Port.DataType; }
            set
            {
                Port.DataType = value;
                BlockPort.DataType = value;
            }
        }

        public PortNode(string name, string description, DataType dataType, Type elementType, Block parent) : base(name, parent)
        {
            Port = new Port(this, name, description, dataType, elementType);
            BlockPort = new Port(parent, name, description, dataType, elementType);
        }
    }
}

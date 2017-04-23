using HydrologyCore.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HydrologyCore.Experiment.Nodes
{
    public abstract class AbstractNode : IRunable
    {
        public event NameChangedEventHandler NameChanged;

        protected string name;
        public virtual string Name
        {
            get { return name; }
            set
            {
                name = value;
                InvokeNameChanged(name);
            }
        }

        protected void InvokeNameChanged(string name)
        {
            NameChanged?.Invoke(this, new NameChangedEventArgs(name));
        }

        public Block Parent { get; set; }

        protected List<Port> inPorts = new List<Port>();
        public IList<Port> InPorts => inPorts;

        protected List<Port> outPorts = new List<Port>();
        public IList<Port> OutPorts => outPorts;

        public AbstractNode(string name, Block parent)
        {
            Name = name;
            Parent = parent;
        }

        public abstract void Run(Context ctx);
        
        public virtual void Run(Context ctx, BackgroundWorker worker, int count, ref int current)
        {
            Core.Instance.UpdateWorker(worker, ++current, count, Name);
            Run(ctx);
            Core.Instance.UpdateWorker(worker, ++current, count, null);
        }
    }
}

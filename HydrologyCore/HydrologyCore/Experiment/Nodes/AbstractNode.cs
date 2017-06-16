using HydrologyCore.Context;
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

        public event NodeExecutionStartEventHandler ExecutionStart;
        public event NodeStatusChangedEventHandler StatusChanged;

        protected void InvokeExecutionStart(IContext ctx)
        {
            ctx.SetStatus(this, ExecutionStatus.EXECUTING);
            ExecutionStart?.Invoke(this, new NodeStatusChangedEventArgs(ctx, this));
        }

        protected void ChangeStatus(IContext ctx, ExecutionStatus status, Exception error)
        {
            if (ctx.GetContext(this).Status != ExecutionStatus.WARNING || status != ExecutionStatus.SUCCESS)
                ctx.SetStatus(this, status);
            ctx.SetError(this, error);
            StatusChanged?.Invoke(this, new NodeStatusChangedEventArgs(ctx, this));
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

        public abstract void Run(IContext ctx);
        
        public virtual void Run(IContext ctx, BackgroundWorker worker)
        {
            Core.Instance.UpdateWorker(worker, Name);
            try
            {
                InvokeExecutionStart(ctx);

                Run(ctx);

                ChangeStatus(ctx, ExecutionStatus.SUCCESS, null);
            }
            catch (Exception e)
            {
                ChangeStatus(ctx, ExecutionStatus.ERROR, e);
                throw e;
            }
        }
    }
}

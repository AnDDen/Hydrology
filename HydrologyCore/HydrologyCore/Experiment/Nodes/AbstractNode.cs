using HydrologyCore.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment.Nodes
{
    public abstract class AbstractNode
    {
        public string Name { get; set; }

        protected List<AbstractNode> previous;
        public IList<AbstractNode> Previous { get { return previous; } }

        protected List<AbstractNode> next;
        public IList<AbstractNode> Next { get { return next; } }

        public NodeContainer NodeContainer { get; set; }

        protected Dictionary<string, VariableInfo> outputInfo;
        public IDictionary<string, VariableInfo> OutputInfo { get { return outputInfo; } }

        protected Dictionary<string, object> output;
        public IDictionary<string, object> Output { get { return output; } }

        public virtual object GetVarValue(string name)
        {
            var key = outputInfo.First(x => x.Value.Name == name).Key;
            return output[key];
        }

        public virtual bool DependsOn(AbstractNode node) { return false; }

        public AbstractNode(string name, NodeContainer nodeContainer)
        {
            Name = name;
            NodeContainer = nodeContainer;
            previous = new List<AbstractNode>();
            next = new List<AbstractNode>();
            outputInfo = new Dictionary<string, VariableInfo>();
            output = new Dictionary<string, object>();
        }
        
        public abstract void Run();
    }
}

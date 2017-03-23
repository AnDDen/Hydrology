using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;

namespace HydrologyCore.Data
{
    public class VariableValue
    {
        public VariableType VariableType { get; set; }
        private string value;
        private AbstractNode node;
        private string refVarName;

        public string Value { get { return value; } }
        public AbstractNode RefNode { get { return node; } }
        public string RefVarName { get { return refVarName; } }

        public VariableValue(VariableType varType, string value)
        {
            VariableType = varType;
            this.value = value;
        }
        
        public VariableValue(VariableType varType) : this(varType, "") { }

        private Tuple<string, string> GetReferenceInfo(string path)
        {
            switch (VariableType)
            {
                case VariableType.VALUE:
                    {
                        return new Tuple<string, string>(null, null);
                    }
                case VariableType.REFERENCE:
                case VariableType.LOOP:
                    {
                        int pos = path.LastIndexOf('/');
                        if (pos < 0)
                        {
                            return new Tuple<string, string>(path, null);
                        }
                        else
                        {
                            return new Tuple<string, string>(path.Substring(0, pos - 1), path.Substring(pos + 1));
                        }
                    }
            }
            return null;            
        }

        public void SetValue(AbstractNode thisNode, VariableType type, string value)
        {
            switch (VariableType)
            {
                case VariableType.VALUE:
                    {
                        this.value = value;
                        break;
                    }
                case VariableType.REFERENCE:
                    {
                        int pos = value.LastIndexOf('/');
                        string nodeName = value.Substring(0, pos);
                        node = thisNode.NodeContainer.RootContainer.ResolveNode(nodeName);
                        refVarName = value.Substring(pos + 1);
                        break;
                    }
                case VariableType.LOOP:
                    {
                        node = thisNode.NodeContainer.RootContainer.ResolveNode(value);
                        break;
                    }
            }
        }

        public string GetValueAsString()
        {
            switch (VariableType)
            {
                case VariableType.VALUE:
                    {
                        return value;
                    }
                case VariableType.REFERENCE:
                    {
                        return node.Name + "/" + refVarName;
                    }
                case VariableType.LOOP:
                    {
                        return node.Name;
                    }
            }
            return null;
        }

        public object GetValue(NodeContainer nodeContainer, Type type)
        {
            switch (VariableType)
            {
                case VariableType.VALUE:
                    {
                        return Convert.ChangeType(value, type);
                    }
                case VariableType.REFERENCE:
                    {
                        return node.GetVarValue(refVarName);
                    }
                case VariableType.LOOP:
                    {
                        return (node as LoopNode).Value;
                    }
            }
            return null;
        }
    }
}

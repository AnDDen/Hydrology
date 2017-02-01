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
        public string Value { get; set; }

        public VariableValue(VariableType varType, string value)
        {
            VariableType = varType;
            Value = value;
        }
        
        public VariableValue(VariableType varType) : this(varType, "") { }

        public Tuple<string, string> GetReferenceInfo()
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
                        int pos = Value.IndexOf('/');
                        if (pos < 0)
                        {
                            return new Tuple<string, string>(Value, null);
                        }
                        else
                        {
                            return new Tuple<string, string>(Value.Substring(0, pos - 1), Value.Substring(pos + 1));
                        }
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
                        return Convert.ChangeType(Value, type);
                    }
                case VariableType.REFERENCE:
                    {
                        var refInfo = GetReferenceInfo();
                        var node = nodeContainer.FindNode(refInfo.Item1);
                        return node.GetVarValue(refInfo.Item2);
                    }
                case VariableType.LOOP:
                    {
                        var refInfo = GetReferenceInfo();
                        var node = nodeContainer.FindNode(refInfo.Item1);
                        return (node as LoopNode).Value;
                    }
            }
            return null;
        }
    }
}

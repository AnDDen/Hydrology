using HydrologyCore.Data;
using HydrologyCore.Experiment.Nodes;
using System.Collections.ObjectModel;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class InputParameter
    {
        private VariableInfo varInfo;        
        public VariableInfo VarInfo { get { return varInfo; } }

        public string Name { get; set; }

        public VariableValue VarValue { get; set; }

        private AbstractNode node;

        public string Value
        {
            get { return VarValue.GetValueAsString(); }
            set { VarValue.SetValue(node, VarValue.VariableType, value); }
        }

        public string VarType
        {
            get
            {
                return VariableTypeHelper.VariableTypeToString(VarValue.VariableType);
            }
            set
            {
                VarValue.VariableType = VariableTypeHelper.StringToVariableType(value);
            }
        }

        private ObservableCollection<string> varTypes;

        public ObservableCollection<string> VarTypes
        {
            get
            {
                return varTypes;
            }
        }

        public InputParameter(AbstractNode node, string name, VariableInfo varInfo)
        {
            this.node = node;
            Name = name;
            varTypes = new ObservableCollection<string>(VariableTypeHelper.GetAllVariableTypeStrings());
            this.varInfo = varInfo;
            VarValue = new VariableValue(VariableType.VALUE);
        }
    }
}
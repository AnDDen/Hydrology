using HydrologyCore.Data;
using System.Collections.ObjectModel;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class InputParameter
    {
        private VariableInfo varInfo;        
        public VariableInfo VarInfo { get { return varInfo; } }

        public VariableValue VarValue { get; set; }

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

        public InputParameter()
        {
            varTypes = new ObservableCollection<string>(VariableTypeHelper.GetAllVariableTypeStrings());
        }
    }
}
using HydrologyCore.Data;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class OutputParameter
    {
        public VariableInfo VarInfo { get; set; }

        public string Name { get; set; }

        public bool IsSaveToFile { get; set; }

        public OutputParameter(string name, VariableInfo varInfo)
        {
            Name = name;
            VarInfo = varInfo;
            IsSaveToFile = true;
        }
    }
}
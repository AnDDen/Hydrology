using HydrologyCore.Data;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class OutputParameter
    {
        public VariableInfo VarInfo { get; set; }
        public bool IsSaveToFile { get; set; }

        public OutputParameter(VariableInfo varInfo, bool isSaveToFile)
        {
            VarInfo = varInfo;
            IsSaveToFile = isSaveToFile;
        }

        public OutputParameter() : this(null, false) { }
    }
}
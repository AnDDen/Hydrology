using HydrologyCore.Experiment;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class OutputParameter
    {
        public Port Port { get; set; }

        public bool IsSaveToFile { get; set; }

        public OutputParameter(Port port, bool saveToFile)
        {
            Port = port;
            IsSaveToFile = saveToFile;
        }
    }
}
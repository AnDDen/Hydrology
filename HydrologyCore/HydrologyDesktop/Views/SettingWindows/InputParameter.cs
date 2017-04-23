using HydrologyCore.Experiment;
using System.Windows;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class InputParameter
    {
        public Port Port { get; }

        public bool IsRef { get; set; }

        public Visibility ShowValue => IsRef ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ShowDescription => string.IsNullOrWhiteSpace(Port.Description) ? Visibility.Collapsed : Visibility.Visible;

        public string Value { get; set; }

        public IRunable Node { get; }

        public InputParameter(IRunable node, Port port, bool isRef, string value)
        {
            Node = node;
            Port = port;
            IsRef = isRef;
            Value = value;
        }
    }
}
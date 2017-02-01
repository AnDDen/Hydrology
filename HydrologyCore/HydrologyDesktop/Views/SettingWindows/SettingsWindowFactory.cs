using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class SettingsWindowHelper
    {
        public static SettingsWindow CreateSettingWindow(Type nodeType, Type algorithmType, Window owner)
        {
            SettingsWindow window = null;
            if (nodeType == typeof(AlgorithmNode))
                window = new AlgorithmSettingsWindow();

            if (window != null)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            return window;
        }
    }
}

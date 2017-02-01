using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HydrologyDesktop.Views.SettingWindows
{
    public class SettingsWindow : Window
    {
        public AbstractNode Node { get; set; }
        public AbstractNode CreateNode()
        {
            return null;
        }

        public SettingsWindow()
        {

        }
    }
}

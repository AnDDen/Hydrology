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

        public virtual AbstractNode GetNode()
        {
            return Node;
        }

        public virtual bool Validate()
        {
            return true;
        }

        public SettingsWindow()
        {

        }

        public string NodeName
        {
            get { return Node.Name; }
            set { Node.Name = value; }
        }

        protected void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
                DialogResult = true;
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для InitSettingsWindow.xaml
    /// </summary>
    public partial class RunProcessSettingsWindow : Window
    {
        private string processName;
        public string ProcessName
        {
            get { return processName; }
            set {
                processName = value;
                nameTextBox.Text = processName;
                fileDialog.FileName = processName;
            }
        }

        OpenFileDialog fileDialog;

        public RunProcessSettingsWindow(OpenFileDialog fileDialog)
        {
            InitializeComponent();
            this.fileDialog = fileDialog;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            fileDialog.FileName = processName;

            if (!Directory.Exists(fileDialog.InitialDirectory) || fileDialog.InitialDirectory == "")
            {
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            }
            bool? ok = fileDialog.ShowDialog();
            if (ok.HasValue && ok.Value)
            {
                ProcessName = fileDialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}

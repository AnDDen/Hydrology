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
    public partial class InitSettingsWindow : Window
    {
        private string initPath;
        public string InitPath
        {
            get { return initPath; }
            set { 
                initPath = value;
                folderTextBox.Text = initPath;
                folderDialog.SelectedPath = initPath;
            }
        }

        System.Windows.Forms.FolderBrowserDialog folderDialog;

        public InitSettingsWindow(System.Windows.Forms.FolderBrowserDialog folderDialog)
        {
            InitializeComponent();
            this.folderDialog = folderDialog;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            folderDialog.SelectedPath = folderTextBox.Text;
            if (!Directory.Exists(folderDialog.SelectedPath) || folderDialog.SelectedPath == "")
            {
                folderDialog.SelectedPath = Directory.GetCurrentDirectory();
            }
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            initPath = folderTextBox.Text;
            DialogResult = true;
        }
    }
}

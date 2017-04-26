using HydrologyCore.Experiment;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HydrologyDesktop.Views.SettingWindows
{
    /// <summary>
    /// Логика взаимодействия для ExperimentSettingsWindow.xaml
    /// </summary>
    public partial class ExperimentSettingsWindow : Window
    {
        public string PathName { get; set; }

        public string DirName { get; set; }

        private FolderBrowserDialog folderDialog;

        public ExperimentSettingsWindow(string pathName, string dirName)
        {
            PathName = pathName;
            DirName = dirName;

            InitializeComponent();

            folderDialog = new FolderBrowserDialog();
        }

        public ExperimentSettingsWindow(Experiment experiment) : this(experiment.Path, experiment.Name) { }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PathName))
                folderDialog.SelectedPath = Directory.GetCurrentDirectory();
            else
                folderDialog.SelectedPath = System.IO.Path.GetFullPath(PathName);
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = folderDialog.SelectedPath;
                PathName = GetRelativePath(path);
                pathTextBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateTarget();
            }
        }

        public string GetRelativePath(string path)
        {
            string folder = Directory.GetCurrentDirectory();

            if (!folder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                folder += System.IO.Path.DirectorySeparatorChar;
            }

            Uri pathUri = new Uri(path);
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }
    }
}

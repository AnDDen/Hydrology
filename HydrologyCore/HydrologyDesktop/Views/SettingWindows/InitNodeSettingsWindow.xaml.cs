using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HydrologyDesktop.Views.SettingWindows
{
    /// <summary>
    /// Логика взаимодействия для InitNodeSettingsWindow.xaml
    /// </summary>
    public partial class InitNodeSettingsWindow : SettingsWindow
    {
        public ObservableCollection<FileParemeter> Files { get; set; }

        public InitNodeSettingsWindow(NodeContainer container) 
            : this(new InitNode("", container)) { }

        public InitNodeSettingsWindow(AbstractNode node)
        {
            Node = node;

            InitFiles();

            InitializeComponent();
        }

        public void InitFiles()
        {
            Files = new ObservableCollection<FileParemeter>();
            var initNode = Node as InitNode;
            foreach (var f in initNode.Files)
            {
                if (Files.FirstOrDefault(x => x.FilePath == f.Key) == null)
                    Files.Add(new FileParemeter(f.Key, f.Value));
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "CSV таблицы (*.csv)|*.csv";
            fileDialog.CheckFileExists = true;
            fileDialog.Multiselect = true;
            bool? dialogResult = fileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                foreach (var path in fileDialog.FileNames)
                {
                    string relativePath = GetRelativePath(path);
                    if (Files.FirstOrDefault(x => x.FilePath == relativePath) == null)
                        Files.Add(new FileParemeter(relativePath, ""));
                }
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

        public override AbstractNode GetNode()
        {
            var initNode = Node as InitNode;
            initNode.Files.Clear();
            foreach (var f in Files)
            {
                initNode.Files.Add(f.FilePath, f.VarName);
            }
            return Node;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            FileParemeter fileParam = button.DataContext as FileParemeter;
            Files.Remove(fileParam);
        }
    }
}

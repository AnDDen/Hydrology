using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для AlgorithmSettingsWindow.xaml
    /// </summary>
    public partial class AlgorithmSettingsWindow : SettingsWindow
    {
        private string nodeName;
        private string algorithmType;
        private ObservableCollection<InputParameter> inputParameters;
        private ObservableCollection<OutputParameter> outputParameters;

        public AlgorithmSettingsWindow()
        {
            InitializeComponent();
        }
    }
}

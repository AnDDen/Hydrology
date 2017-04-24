using HydrologyCore;
using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
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
    /// Логика взаимодействия для LoopPortNode.xaml
    /// </summary>
    public partial class LoopPortNodeSettingsWindow : SettingsWindow
    {
        public ObservableCollection<string> DataTypes => new ObservableCollection<string>(UIConsts.DATA_TYPE_NAMES.Values);
        public ObservableCollection<string> ElementTypes => new ObservableCollection<string>(UIConsts.ELEMENT_TYPE_NAMES.Values);

        public PortNode PortNode => Node as PortNode;

        public Port Port { get; }

        public string PortName
        {
            get { return Port.Name; }
            set { Port.Name = value; }
        }

        public LoopPortNodeSettingsWindow(IRunable node)
        {
            Node = node;
            Port = new Port(PortNode.Port.Owner, PortNode.Port.Name, PortNode.Port.Description, PortNode.Port.DataType, PortNode.Port.ElementType);

            InitializeComponent();

            SetValues();
        }

        private void SetValues()
        {
            if (Port.ElementType != null)
                ElementTypeComboBox.SelectedItem = UIConsts.ELEMENT_TYPE_NAMES[Port.ElementType];
            else
                ElementTypeComboBox.SelectedItem = UIConsts.ELEMENT_TYPE_NAMES[typeof(double)];
        }

        public override IRunable GetNode()
        {
            PortNode.Name = Port.Name;
            PortNode.ElementType = Port.ElementType;

            return Node;
        }

        private void ElementTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string elementTypeStr = ElementTypeComboBox.SelectedItem.ToString();
            Type elementType = UIConsts.ELEMENT_TYPE_NAMES.FirstOrDefault(x => x.Value == elementTypeStr).Key;
            Port.ElementType = elementType;
        }
    }
}

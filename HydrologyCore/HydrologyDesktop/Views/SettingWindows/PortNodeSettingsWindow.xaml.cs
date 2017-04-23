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
    /// Логика взаимодействия для PortNodeSettingsWindow.xaml
    /// </summary>
    public partial class PortNodeSettingsWindow : SettingsWindow
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

        public string PortDescription
        {
            get { return Port.Description; }
            set { Port.Description = value; }
        }

        public PortNodeSettingsWindow(HydrologyCore.Experiment.Block container, bool isInPort) 
            : this(isInPort 
                  ? (PortNode)(new InPortNode("", "", DataType.VALUE, typeof(int), container)) 
                  : new OutPortNode("", "", DataType.VALUE, typeof(int), container))
        {
        }

        public PortNodeSettingsWindow(IRunable node)
        {
            Node = node;
            Port = new Port(PortNode.Port.Owner, PortNode.Port.Name, PortNode.Port.Description, PortNode.Port.DataType, PortNode.Port.ElementType);

            InitializeComponent();

            SetValues();
        }

        private void SetValues()
        {
            DataTypeComboBox.SelectedItem = UIConsts.DATA_TYPE_NAMES[Port.DataType];
            if (Port.DataType == DataType.DATASET || Port.DataType == DataType.DATATABLE)
            {
                ElementTypeComboBox.IsEnabled = false;
            }
            else
            {
                ElementTypeComboBox.IsEnabled = true;                
            }

            if (Port.ElementType != null)
                ElementTypeComboBox.SelectedItem = UIConsts.ELEMENT_TYPE_NAMES[Port.ElementType];
            else
                ElementTypeComboBox.SelectedItem = UIConsts.ELEMENT_TYPE_NAMES[typeof(int)];
        }

        public override IRunable GetNode()
        {
            PortNode.Name = Port.Name;
            PortNode.Description = Port.Description;
            PortNode.DataType = Port.DataType;
            PortNode.ElementType = Port.ElementType;

            return Node;
        }

        private void DataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dataTypeStr = DataTypeComboBox.SelectedItem.ToString();
            DataType dataType = UIConsts.DATA_TYPE_NAMES.FirstOrDefault(x => x.Value == dataTypeStr).Key;
            Port.DataType = dataType;
            if (dataType == DataType.DATASET || dataType == DataType.DATATABLE)
            {
                ElementTypeComboBox.IsEnabled = false;
                Port.ElementType = null;
            }
            else
            {
                ElementTypeComboBox.IsEnabled = true;
            }
        }

        private void ElementTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string elementTypeStr = ElementTypeComboBox.SelectedItem.ToString();
            Type elementType = UIConsts.ELEMENT_TYPE_NAMES.FirstOrDefault(x => x.Value == elementTypeStr).Key;
            Port.ElementType = elementType;
        }
    }
}

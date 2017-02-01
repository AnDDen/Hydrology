using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HydrologyDesktop.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public event EventHandler<EventArgs> SettingsButtonClick;
        public event EventHandler<EventArgs> EditButtonClick;

        public bool ShowEditButton { get; set; }

        private AbstractNode node;

        public string NodeName
        {
            get { return node.Name; }
            set { node.Name = value; }
        }

        public string NodeType { get; set; }        

        public NodeControl(AbstractNode node)
        {
            InitializeComponent();
            this.node = node;
            ShowEditButton = false;
            if (node is AlgorithmNode)
            {
                var p = node as AlgorithmNode;
                NodeType = p.DisplayedTypeName;
                AdditionalInfo.Visibility = Visibility.Visible;
                AdditionalInfo.Children.Add(new Label() { Content = "Параметры алгоритма", FontWeight = FontWeights.Bold });
                foreach (var param in p.InputValues)
                {
                    string name = param.Key;
                    string displayedName = p.InputInfo[name].Name;
                    var varType = param.Value.VariableType;
                    string valueTypeStr = VariableTypeHelper.VariableTypeToString(varType);
                    string value = param.Value.Value;
                    AdditionalInfo.Children.Add(new Label()
                    {
                        Content = string.Format("{0} = [{1}] {2}", displayedName, valueTypeStr, value)
                    });
                }
            }
            else if (node is LoopNode)
            {
                var p = node as LoopNode;
                NodeType = UIConsts.LOOP_NODE_NAME;
                AdditionalInfo.Visibility = Visibility.Visible;
                AdditionalInfo.Children.Add(new Label()
                {
                    Content = string.Format("{0}..{1}; {2}", p.FromValue, p.ToValue, p.Step)
                });
                ShowEditButton = true;
            }
            else if (node is InitNode)
            {
                NodeType = UIConsts.INIT_NODE_NAME;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditButtonClick != null)
                EditButtonClick.Invoke(this, e);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsButtonClick != null)
                SettingsButtonClick.Invoke(this, e);
        }
    }
}

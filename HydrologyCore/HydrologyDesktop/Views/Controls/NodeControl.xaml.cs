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
using System.Xml.Linq;

namespace HydrologyDesktop.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        public event EventHandler<EventArgs> SettingsButtonClick;
        public event EventHandler<EventArgs> EditButtonClick;

        public Visibility ShowEditButton { get; set; }

        private AbstractNode node;

        public AbstractNode Node { get { return node; } }

        public string NodeName
        {
            get { return node.Name; }
            set { node.Name = value; }
        }

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (selected)
                {
                    Border.BorderThickness = new Thickness(3);
                }
                else
                {
                    Border.BorderThickness = new Thickness(1);
                }
            }
        }

        public string NodeType
        {
            get
            {
                if (node is AlgorithmNode)
                {
                    var p = node as AlgorithmNode;
                    return p.DisplayedTypeName;
                }
                else if (node is LoopNode)
                {
                    return UIConsts.LOOP_NODE_NAME;
                }
                else if (node is InitNode)
                {
                    return UIConsts.INIT_NODE_NAME;
                }
                return "";
            }
        }    

        public NodeControl(AbstractNode node)
        {
            this.node = node;
            InitializeComponent();
            EditButton.Visibility = Visibility.Collapsed;
            if (node is AlgorithmNode)
            {
                var p = node as AlgorithmNode;
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
                AdditionalInfo.Visibility = Visibility.Visible;
                AdditionalInfo.Children.Add(new Label()
                {
                    Content = string.Format("{0}..{1}; {2}", p.FromValue, p.ToValue, p.Step)
                });
                EditButton.Visibility = Visibility.Visible;
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

using HydrologyCore.Experiment;
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
using System.Windows.Shapes;

namespace HydrologyDesktop.Views.SettingWindows
{
    /// <summary>
    /// Логика взаимодействия для LoopNodeSettingsWindow.xaml
    /// </summary>
    public partial class LoopNodeSettingsWindow : SettingsWindow
    {
        public string StartValue { get; set; }

        public string EndValue { get; set; }

        public string Step { get; set; }

        public LoopNodeSettingsWindow(NodeContainer container) 
            : this(new LoopNode("", container)) { }

        public LoopNodeSettingsWindow(AbstractNode node)
        {
            Node = node;
            LoopNode loopNode = Node as LoopNode;
            StartValue = loopNode.FromValue.ToString();
            EndValue = loopNode.ToValue.ToString();
            Step = loopNode.Step.ToString();

            InitializeComponent();
        }

        public override bool Validate()
        {            
            double res = 0;
            if (!double.TryParse(StartValue, out res))
            {
                MessageBox.Show("Начальное значение имеет некорректный формат", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!double.TryParse(EndValue, out res))
            {
                MessageBox.Show("Конечное значение имеет некорректный формат", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!double.TryParse(Step, out res))
            {
                MessageBox.Show("Шаг имеет некорректный формат", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public override AbstractNode GetNode()
        {
            var loopNode = Node as LoopNode;
            loopNode.FromValue = double.Parse(StartValue);
            loopNode.ToValue = double.Parse(EndValue);
            loopNode.Step = double.Parse(Step);
            return Node;
        }
    }
}

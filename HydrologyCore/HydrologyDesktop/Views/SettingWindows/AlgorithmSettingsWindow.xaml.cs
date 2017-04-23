using CoreInterfaces;
using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        public string AlgorithmType => (Node as AlgorithmNode).DisplayedTypeName;

        private AlgorithmNode AlgNode => Node as AlgorithmNode;

        public ObservableCollection<InputParameter> InputParams { get; set; }
        public ObservableCollection<OutputParameter> OutputParams { get; set; }

        public AlgorithmSettingsWindow(Type algorithmType, HydrologyCore.Experiment.Block container) 
            : this(new AlgorithmNode("", algorithmType, container)) { }

        public AlgorithmSettingsWindow(IRunable node)
        {
            Node = node;
            InputParams = new ObservableCollection<InputParameter>(AlgNode.Parameters.Select(p => new InputParameter(node, p, p.Displayed, AlgNode.GetPortValue(p)?.ToString())));
            OutputParams = new ObservableCollection<OutputParameter>(AlgNode.SaveToFile.Select(p => new OutputParameter(p.Key, p.Value)));
            InitializeComponent();
        }

        public override IRunable GetNode()
        {
            ApplyParams();
            return Node;
        }

        public void ApplyParams()
        {
            foreach (InputParameter p in InputParams)
            {
                p.Port.Displayed = p.IsRef;
                if (!p.IsRef)
                    AlgNode.SetPortValue(p.Port, p.Value);
            }

            foreach (OutputParameter p in OutputParams)
            {
                AlgNode.SetSaveToFile(p.Port, p.IsSaveToFile);
            }
        }
    }
}

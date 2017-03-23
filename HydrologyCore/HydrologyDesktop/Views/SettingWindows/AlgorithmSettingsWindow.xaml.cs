using CoreInterfaces;
using HydrologyCore.Data;
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
        public string AlgorithmType
        {
            get
            {
                return (Node as AlgorithmNode).DisplayedTypeName;
            }
        }

        public ObservableCollection<InputParameter> InputParams { get; set; }
        public ObservableCollection<OutputParameter> OutputParams { get; set; }

        public AlgorithmSettingsWindow(Type algorithmType, NodeContainer container) 
            : this(new AlgorithmNode("", algorithmType, container)) { }

        public AlgorithmSettingsWindow(AbstractNode node)
        {
            Node = node;

            InitParams();

            InitializeComponent();
        }

        public void InitParams()
        {
            InputParams = new ObservableCollection<InputParameter>();
            var inputs = (Node as AlgorithmNode).InputInfo;
            var inputValues = (Node as AlgorithmNode).InputValues;
            foreach (var input in inputs)
            {
                var varValue = (inputValues.ContainsKey(input.Key)) ? inputValues[input.Key] : new VariableValue(VariableType.VALUE);
                InputParams.Add(new InputParameter(Node, input.Key, input.Value) { VarValue = varValue });
            }

            OutputParams = new ObservableCollection<OutputParameter>();
            var outputs = (Node as AlgorithmNode).OutputInfo;
            var outputSaveToFile = (Node as AlgorithmNode).SaveToFile;
            foreach (var output in outputs)
            {
                var value = (outputSaveToFile.ContainsKey(output.Key)) ? outputSaveToFile[output.Key] : true;
                OutputParams.Add(new OutputParameter(output.Key, output.Value) { IsSaveToFile = value });
            }
        }

        public void ApplyParams()
        {
            foreach (var input in InputParams)
            {
                (Node as AlgorithmNode).InputValues[input.Name] = input.VarValue;
            }

            foreach (var output in OutputParams)
            {
                (Node as AlgorithmNode).SaveToFile[output.Name] = output.IsSaveToFile;
            }
        }

        public override AbstractNode GetNode()
        {
            ApplyParams();
            return Node;
        }
    }
}

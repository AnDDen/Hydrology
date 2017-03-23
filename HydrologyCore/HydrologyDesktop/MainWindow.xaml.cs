using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HydrologyCore;
using CoreInterfaces;
using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Globalization;
using HydrologyCore.Experiment.Nodes;
using HydrologyDesktop.Views.SettingWindows;
using HydrologyDesktop.Views.Controls;
using HydrologyCore.Experiment;
using HydrologyCore.Data;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string NODE_DRAG_DATA_OBJECT = "NodeDescriptor";
        
        public Tuple<Type, Type> INIT_NODE_DESCRIPTOR
        {
            get { return new Tuple<Type, Type>(typeof(InitNode), null); }
        }

        public Tuple<Type, Type> LOOP_NODE_DESCRIPTOR
        {
            get { return new Tuple<Type, Type>(typeof(LoopNode), null); }
        }

        private IDictionary<LoopNode, NodeContainerGraph> nodeContainerGraphs;

        private Experiment experiment;
        private NodeContainerGraph root;

        private NodeContainerGraph nodeContainer;

        public NodeContainerGraph NodeContainer
        {
            get { return nodeContainer; }
            set
            {
                nodeContainer = value;
                SetNodeContainerName();
                SetBackButtonVisibility();
            }
        }

        private bool isMove;
        private Vector delta;

        private Point startPoint;
        public bool IsDragDrop { get; set; }

        private UIElement selected;
        public UIElement Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public void SetNodeContainerName()
        {
            if (NodeContainer != null && NodeContainer.NodeContainer.LoopNode != null)
                NodeContainerName.Text = NodeContainer.NodeContainer.LoopNode.Name;
            else
                NodeContainerName.Text = UIConsts.EXPERIMENT_NAME;
        }

        public void SetBackButtonVisibility()
        {
            if (NodeContainer != null && NodeContainer.NodeContainer.Parent != null)
                BackButton.Visibility = Visibility.Visible;
            else
                BackButton.Visibility = Visibility.Collapsed;
        }

        public System.Windows.Forms.FolderBrowserDialog FolderDialog { get; set; }
        public OpenFileDialog FileDialog { get; set; }

        

        private SaveFileDialog saveFileDialog = new SaveFileDialog()
        {
            Filter = "Experiment Files (*.xml)|*.xml",
            AddExtension = true
        };

        private OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Filter = "Experiment Files (*.xml)|*.xml",
            AddExtension = true
        };

        public MainWindow()
        {
            Core.Instance.LoadPlugins();
            InitializeComponent();
            Init();
            NewExperiment();
        }        

        private void Init()
        {
            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            FileDialog = new OpenFileDialog() { Multiselect = false };

            // add algorithm buttons to toolkit
            IDictionary<string, Type> algTypes = PluginManager.Instance.AlgorithmTypes;
            foreach (var type in algTypes)
            {
                var button = CreateAlgorithmButton(type.Key, type.Value);
                AlgorithmButtonsPanel.Children.Add(button);
            }
        }

        void NewExperiment()
        {
            experiment = Core.Instance.NewExperiment();

            root = new NodeContainerGraph(experiment.NodeContainer, null);
            nodeContainerGraphs = new Dictionary<LoopNode, NodeContainerGraph>();
            NodeContainer = root;
            NodeContainer.DisplayOnCanvas(Canvas);
        }

        public Button CreateAlgorithmButton(string typeName, Type type)
        {
            Button button = new Button();
            button.Tag = new Tuple<Type, Type>(typeof(AlgorithmNode), type);
            var attr = type.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
            button.Content = attr.Name;

            button.Style = (Style)this.FindResource("AlgButtonStyle");

            button.PreviewMouseDown += Button_PreviewMouseDown;
            button.PreviewMouseUp += Button_PreviewMouseUp;
            button.PreviewMouseMove += Button_PreviewMouseMove;

            return button;
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsDragDrop = false;
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && !IsDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IsDragDrop = true;
                    Button btn = (Button)sender;
                    DataObject dragData = new DataObject(NODE_DRAG_DATA_OBJECT, btn.Tag);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (IsDragDrop)
            {
                var nodeDescriptor = (Tuple<Type, Type>)e.Data.GetData(NODE_DRAG_DATA_OBJECT);

                Point pos = e.GetPosition(Canvas);
                IsDragDrop = false;

                CreateNode(nodeDescriptor.Item1, nodeDescriptor.Item2, pos);
            }
        }

        public void CreateNode(Type nodeType, Type algType, Point pos)
        {
            var window = SettingsWindowHelper.CreateSettingWindow(nodeType, algType, NodeContainer, this);
            bool? dialogResult = window.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                CreateNodeControl(window.GetNode(), pos);
            }
        }

        public void CreateNodeControl(AbstractNode node, Point pos)
        {
            NodeControl nodeControl = new NodeControl(node);

            nodeControl.MouseLeftButtonDown += NodeControl_MouseLeftButtonDown;
            nodeControl.MouseLeftButtonUp += NodeControl_MouseLeftButtonUp;
            nodeControl.MouseMove += NodeControl_MouseMove;
            nodeControl.LostMouseCapture += NodeControl_LostMouseCapture;

            nodeControl.SettingsButtonClick += NodeControl_SettingsClicked;
            nodeControl.EditButtonClick += NodeControl_EditButtonClick;

            Canvas.Children.Add(nodeControl);
            Canvas.SetLeft(nodeControl, pos.X);
            Canvas.SetTop(nodeControl, pos.Y);

            if (nodeControl.Node is LoopNode)
            {
                var loopNode = nodeControl.Node as LoopNode;
                nodeContainerGraphs.Add(loopNode, new NodeContainerGraph(loopNode.LoopBody, NodeContainer) { Parent = nodeContainer });
            }

            NodeContainer.AddNode(nodeControl);
        }

        public void NodeControl_LostMouseCapture(object sender, MouseEventArgs e)
        {
            isMove = false;
        }

        object moveControl = null;

        public void NodeControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (moveControl == null && sender is NodeControl)
            {
                NodeControl node = sender as NodeControl;
                isMove = true;
                Point pos = e.GetPosition(Canvas);
                delta = new Vector(pos.X - Canvas.GetLeft(node), pos.Y - Canvas.GetTop(node));
                node.CaptureMouse();
                moveControl = node;
            }
        }

        public void NodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMove = false;
            moveControl = null;
            (sender as UserControl).ReleaseMouseCapture();
        }

        public void NodeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove && moveControl == sender)
            {
                NodeControl node = sender as NodeControl;
                Point pos = e.GetPosition(Canvas);
                Canvas.SetLeft(node, pos.X - delta.X);
                Canvas.SetTop(node, pos.Y - delta.Y);
            }
        }

        public void NodeControl_SettingsClicked(object sender, EventArgs e)
        {
            if (sender is NodeControl)
            {
                var nodeControl = sender as NodeControl;
                var window = SettingsWindowHelper.CreateSettingWindowForNode(nodeControl.Node, this);
                bool? dialogResult = window.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    window.GetNode().NodeContainer.SetNodeOrder();

                }
            }
        }

        public void NodeControl_EditButtonClick(object sender, EventArgs e)
        {
            if (sender is NodeControl)
            {
                var nodeControl = sender as NodeControl;
                if (nodeControl.Node is LoopNode)
                {
                    var loopNode = nodeControl.Node as LoopNode;
                    NodeContainer = nodeContainerGraphs[loopNode];
                    NodeContainer.DisplayOnCanvas(Canvas);
                }
            }
        }

        // execution
        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            ExecutorWindow experimentWindow = new ExecutorWindow(experiment)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            experimentWindow.ShowDialog(); 
        }        

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selected = null;
            for (int i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Canvas.Children[i];
                if (child is NodeControl)
                {
                    NodeControl node = child as NodeControl;
                    if (node.IsMouseOver)
                    {
                        node.Selected = true;
                        selected = child;
                    }
                    else
                    {
                        node.Selected = false;
                    }
                }
            }
        }

        // todo : arrows
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
        }

        // todo : arrows
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }
        
        // todo : delete node on 'Del' key press
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            NewExperiment();
        }

        private void LoadExperimentGraph(string path)
        {
            XDocument xDocument = XDocument.Load(path);
            NewExperiment();
            LoadExperimentGraph(xDocument.Root);
            LoadExperimentInputs(xDocument.Root);
        }

        // todo : check
        private void LoadExperimentGraph(XElement nodes)
        {
            // read all nodes
            foreach (XElement element in nodes.Elements("node"))
            {
                double x = double.Parse(element.Attribute("x").Value, CultureInfo.InvariantCulture);
                double y = double.Parse(element.Attribute("y").Value, CultureInfo.InvariantCulture);
                AbstractNode node = null;
                if (element.Element("algorithm") != null)
                {
                    // algorithm node
                    XElement algorithm = element.Element("algorithm");
                    string name = algorithm.Attribute("name").Value;
                    string type = algorithm.Attribute("type").Value;
                    node = new AlgorithmNode(name, PluginManager.Instance.AlgorithmTypes[type], NodeContainer.NodeContainer);
                    if (algorithm.Element("outputs") != null)
                    {
                        foreach (XElement output in algorithm.Element("outputs").Elements("output"))
                        {
                            (node as AlgorithmNode).SaveToFile[output.Attribute("name").Value] =
                                output.Attribute("saveToFile").Value.ToLower() == "yes";
                        }
                    }
                }
                else if (element.Element("init") != null)
                {
                    // init node
                    node = new InitNode(element.Element("init").Attribute("name").Value, NodeContainer.NodeContainer);
                    foreach (XElement file in element.Element("init").Element("files").Elements("file"))
                    {
                        (node as InitNode).Files.Add(file.Attribute("path").Value, file.Attribute("var").Value);
                    }
                }
                else if (element.Element("loop") != null)
                {
                    // loop node
                    XElement algorithm = element.Element("loop");
                    string name = algorithm.Attribute("name").Value;
                    double from = double.Parse(algorithm.Attribute("from").Value, CultureInfo.InvariantCulture);
                    double to = double.Parse(algorithm.Attribute("to").Value, CultureInfo.InvariantCulture);
                    double step = double.Parse(algorithm.Attribute("step").Value, CultureInfo.InvariantCulture);
                    node = new LoopNode(name, NodeContainer.NodeContainer) { FromValue = from, ToValue = to, Step = step };
                }

                if (node != null)
                    CreateNodeControl(node, new Point(x, y));

                if (node is LoopNode)
                {
                    NodeContainer = nodeContainerGraphs[node as LoopNode];

                    // read loop body
                    LoadExperimentGraph(element.Element("loop").Element("loopbody"));

                    // return previous NodeContainer
                    if (NodeContainer.Parent != null)
                    {
                        NodeContainer = NodeContainer.Parent;
                        NodeContainer.DisplayOnCanvas(Canvas);
                    }
                }
            }
        }

        // todo : check
        private void LoadExperimentInputs(XElement nodes)
        {
            // read all nodes
            foreach (XElement element in nodes.Elements("node"))
            {
                if (element.Element("algorithm") != null)
                {
                    // algorithm node
                    XElement algorithm = element.Element("algorithm");
                    string name = algorithm.Attribute("name").Value;
                    AlgorithmNode node = experiment.ResolveNode(name) as AlgorithmNode;
                    if (algorithm.Element("inputs") != null)
                    {
                        foreach (XElement input in algorithm.Element("inputs").Elements("input"))
                        {
                            VariableType varType = (VariableType)Enum.Parse(typeof(VariableType), input.Attribute("varType").Value);
                            node.InputValues[input.Attribute("name").Value] =
                                new VariableValue(varType);
                            node.InputValues[input.Attribute("name").Value].SetValue(node, varType, input.Attribute("varValue").Value);
                        }
                    }
                }
                else if (element.Element("loop") != null)
                {
                    // loop
                    LoadExperimentInputs(element.Element("loop").Element("loopbody"));
                }
            }
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {            
            bool? res = openFileDialog.ShowDialog();
            if (res.HasValue && res.Value)
            {
                // Open experiment graph
                LoadExperimentGraph(openFileDialog.FileName);
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            bool? res = saveFileDialog.ShowDialog();
            if (res.HasValue && res.Value)
            {
                XDocument xDoc = new XDocument();
                xDoc.Add(root.ToXml(nodeContainerGraphs));
                xDoc.Save(saveFileDialog.FileName);
            }
        }

        private void BackLoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (NodeContainer.Parent != null)
            {
                NodeContainer = NodeContainer.Parent;
                NodeContainer.DisplayOnCanvas(Canvas);
            }
        }
    }
}

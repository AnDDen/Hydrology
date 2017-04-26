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
using HydrologyDesktop.XmlHelpers;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string NODE_DRAG_DATA_OBJECT = "NodeDescriptor";

        public Tuple<Type, Type> BLOCK_NODE_DESCRIPTOR => new Tuple<Type, Type>(typeof(Block), null);
        public Tuple<Type, Type> LOOP_NODE_DESCRIPTOR => new Tuple<Type, Type>(typeof(LoopBlock), null);
        public Tuple<Type, Type> INIT_NODE_DESCRIPTOR => new Tuple<Type, Type>(typeof(InitNode), null);
        public Tuple<Type, Type> IN_PORT_NODE_DESCRIPTOR => new Tuple<Type, Type>(typeof(InPortNode), null);
        public Tuple<Type, Type> OUT_PORT_NODE_DESCRIPTOR => new Tuple<Type, Type>(typeof(OutPortNode), null);      

        private IDictionary<Block, NodeContainerGraph> blockGraphs;

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
                SetBlockButtonsVisibility();
            }
        }

        private bool isMove;
        private Vector delta;

        private Arrow arrow = null;
        private bool isArrow = false;
        private bool isArrowStart;

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
            if (NodeContainer != null && NodeContainer.Block != null)
                NodeContainerName.Text = NodeContainer.Block.Name;
            else
                NodeContainerName.Text = UIConsts.EXPERIMENT_NAME;
        }

        public void SetBlockButtonsVisibility()
        {
            if (NodeContainer != null && NodeContainer.Block.Parent != null)
            {
                BackButton.Visibility = Visibility.Visible;
                SettingsBtn.Visibility = Visibility.Visible;
            }
            else
            {
                BackButton.Visibility = Visibility.Collapsed;
                SettingsBtn.Visibility = Visibility.Collapsed;
            }
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

            root = new NodeContainerGraph(experiment.Block, null);
            blockGraphs = new Dictionary<Block, NodeContainerGraph>();
            blockGraphs.Add(experiment.Block, root);
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
            if (window != null)
            {
                bool? dialogResult = window.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    CreateNodeControl(window.GetNode(), pos);
                }
            }
        }

        public NodeControl CreateNodeControl(IRunable node, Point pos)
        {
            NodeControl nodeControl = new NodeControl(node);

            nodeControl.MouseLeftButtonDown += NodeControl_MouseLeftButtonDown;
            nodeControl.MouseLeftButtonUp += NodeControl_MouseLeftButtonUp;
            nodeControl.MouseMove += NodeControl_MouseMove;
            nodeControl.LostMouseCapture += NodeControl_LostMouseCapture;

            nodeControl.ButtonClick += NodeControl_ButtonClick;
            
            NodeContainer.AddNode(nodeControl);
            Canvas.Children.Add(nodeControl);
            Canvas.SetLeft(nodeControl, pos.X);
            Canvas.SetTop(nodeControl, pos.Y);

            if (nodeControl.Node is Block)
            {
                var block = nodeControl.Node as Block;
                blockGraphs.Add(block, new NodeContainerGraph(block, NodeContainer) { Parent = nodeContainer });
                NodeContainer = blockGraphs[block];
                NodeContainer.DisplayOnCanvas(Canvas);
                if (block is LoopBlock)
                {
                    LoopPortNode loopPortNode = new LoopPortNode(block.Name + "Var", "Переменная цикла", typeof(double), block);
                    (block as LoopBlock).LoopPortNode = loopPortNode;
                    CreateNodeControl(loopPortNode, new Point(20, 20));
                }
            }

            return nodeControl;
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
                double nodeX = Canvas.GetLeft(node);
                double nodeY = Canvas.GetTop(node);
                var inPortPos = node.FindInPort(e.GetPosition(node));
                if (inPortPos != null)
                {
                    if (inPortPos.Item1.GetConnections().Count > 0)
                        return;
                    Point pos = inPortPos.Item2;
                    arrow = new Arrow(nodeX + pos.X, nodeY + pos.Y);
                    NodeContainer.AddArrow(arrow);
                    Canvas.Children.Add(arrow);
                    arrow.To = inPortPos.Item1;
                    arrow.ArrowCapMouseDown += Arrow_ArrowCapMouseDown;
                    isArrow = true;
                    isArrowStart = false;
                    //arrow.CaptureMouse();
                }
                else
                {
                    var outPortPos = node.FindOutPort(e.GetPosition(node));
                    if (outPortPos != null)
                    {
                        Point pos = outPortPos.Item2;
                        arrow = new Arrow(nodeX + pos.X, nodeY + pos.Y);
                        arrow.From = outPortPos.Item1;
                        NodeContainer.AddArrow(arrow);
                        Canvas.Children.Add(arrow);
                        arrow.ArrowCapMouseDown += Arrow_ArrowCapMouseDown;
                        isArrow = true;
                        isArrowStart = true;
                        //arrow.CaptureMouse();
                    }
                    else
                    {
                        isMove = true;
                        Point pos = e.GetPosition(Canvas);
                        delta = new Vector(pos.X - Canvas.GetLeft(node), pos.Y - Canvas.GetTop(node));
                        node.CaptureMouse();
                        moveControl = node;
                    }
                }
            }
        }

        public void NodeControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMove)
            {
                isMove = false;
                moveControl = null;
                (sender as UserControl).ReleaseMouseCapture();
            }
        }

        public void NodeControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove && moveControl == sender)
            {
                NodeControl node = sender as NodeControl;

                double x = Canvas.GetLeft(node);
                double y = Canvas.GetTop(node);

                Point pos = e.GetPosition(Canvas);
                Canvas.SetLeft(node, pos.X - delta.X);
                Canvas.SetTop(node, pos.Y - delta.Y);

                double dx = Canvas.GetLeft(node) - x;
                double dy = Canvas.GetTop(node) - y;

                foreach (var arrow in NodeContainer.Arrows.Where(arrow => arrow.From.Owner == node.Node))
                    arrow.P1 = arrow.P1 + new Vector(dx, dy);

                foreach (var arrow in NodeContainer.Arrows.Where(arrow => arrow.To.Owner == node.Node))
                    arrow.P2 = arrow.P2 + new Vector(dx, dy);
            }
        }

        public void UpdateArrows(NodeControl node)
        {
            double nodeX = Canvas.GetLeft(node);
            double nodeY = Canvas.GetTop(node);
            Vector r = new Vector(nodeX, nodeY);
            var arrowsToUpdate = NodeContainer.Arrows.Where(a => node.Node.OutPorts.Contains(a.From) | node.Node.InPorts.Contains(a.To)).ToList();
            foreach (Arrow a in arrowsToUpdate)
            {
                if (node.Node.OutPorts.Contains(a.From))
                {
                    if (a.From.Displayed)
                        a.P1 = node.GetOutPortPoint(a.From) + r;
                    else
                    {
                        Canvas.Children.Remove(a);
                        NodeContainer.RemoveArrow(a);
                        NodeContainer.RemoveArrowFromModel(a);
                    }
                }
                else
                {
                    if (a.To.Displayed)
                        a.P2 = node.GetInPortPoint(a.To) + r;
                    else
                    {
                        Canvas.Children.Remove(a);
                        NodeContainer.RemoveArrow(a);
                        NodeContainer.RemoveArrowFromModel(a);
                    }
                }
            }
        }

        public void NodeControl_ButtonClick(object sender, EventArgs e)
        {
            if (sender is NodeControl)
            {
                var nodeControl = sender as NodeControl;
                if (nodeControl.Node is Block)
                {
                    var block = nodeControl.Node as Block;
                    NodeContainer = blockGraphs[block];
                    NodeContainer.DisplayOnCanvas(Canvas);
                }
                else
                {
                    var window = SettingsWindowHelper.CreateSettingWindowForNode(nodeControl.Node, this);
                    bool? dialogResult = window.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        window.GetNode();
                    }

                    nodeControl.LoadPorts(); // updates ports
                    nodeControl.UpdateLayout();
                    UpdateArrows(nodeControl);
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

            if (selected == null)
            {
                Arrow arr = FindNearestArrow(e.GetPosition(Canvas));
                if (arr != null)
                {
                    arrow = arr;
                    NodeContainer.RemoveArrowFromModel(arrow);
                    arrow.From = null;
                    arrow.P1 = e.GetPosition(Canvas);
                    isArrow = true;
                    isArrowStart = false;
                }
            }
        }

        // todo : arrows
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isArrow && arrow != null)
            {
                if (isArrowStart)
                {
                    arrow.P2 = e.GetPosition(Canvas);
                }
                else
                {
                    arrow.P1 = e.GetPosition(Canvas);
                }
            }
        }

        const double ARROW_SEARCH_RADIUS = 10;

        private Arrow FindNearestArrow(Point p)
        {
            IDictionary<Arrow, double> arrows = new Dictionary<Arrow, double>();

            foreach (UIElement child in Canvas.Children)
            {
                if (child is Arrow)
                {
                    Arrow a = child as Arrow;
                    double l = Math.Sqrt((a.X1 - p.X) * (a.X1 - p.X) + (a.Y1 - p.Y) * (a.Y1 - p.Y));
                    double d = Math.Abs((a.Y2 - a.Y1) * p.X - (a.X2 - a.X1) * p.Y + a.X2 * a.Y1 - a.Y2 * a.X1) /
                        Math.Sqrt((a.Y2 - a.Y1) * (a.Y2 - a.Y1) + (a.X2 - a.X1) * (a.X2 - a.X1));
                    if (l < ARROW_SEARCH_RADIUS)
                        arrows.Add(a, d);
                }
            }

            if (arrows.Count == 0)
                return null;

            double min = arrows.Values.Min();
            return arrows.Keys.FirstOrDefault(a => arrows[a] == min);
        }

        // todo : arrows
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NodeControl node = null;

            for (int i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Canvas.Children[i];
                if (child is NodeControl && (child as NodeControl).IsNodeMouseOver(e.GetPosition(Canvas)))
                {
                    node = child as NodeControl;
                    break;
                }
            }

            if (isArrow)
            {
                #region isArrow
                if (node == null)
                {
                    NodeContainer.RemoveArrow(arrow);
                    Canvas.Children.Remove(arrow);
                    isArrow = false;
                    arrow = null;
                    return;
                }

                double nodeX = Canvas.GetLeft(node);
                double nodeY = Canvas.GetTop(node);

                var inPortPos = node.FindInPort(e.GetPosition(node));
                if (inPortPos != null)
                {
                    if (inPortPos.Item1.GetConnections().Count > 0)
                    {
                        NodeContainer.RemoveArrow(arrow);
                        Canvas.Children.Remove(arrow);
                    }
                    else if (isArrowStart && inPortPos.Item1.Owner != arrow.From.Owner)
                    {
                        arrow.To = inPortPos.Item1;
                        arrow.P2 = inPortPos.Item2 + new Vector(nodeX, nodeY);
                        NodeContainer.AddArrowToModel(arrow);
                    }
                    else
                    {
                        NodeContainer.RemoveArrow(arrow);
                        Canvas.Children.Remove(arrow);
                    }
                }
                else
                {
                    var outPortPos = node.FindOutPort(e.GetPosition(node));
                    if (outPortPos != null)
                    {
                        if (!isArrowStart && outPortPos.Item1.Owner != arrow.To.Owner)
                        {
                            arrow.From = outPortPos.Item1;
                            arrow.P1 = outPortPos.Item2 + new Vector(nodeX, nodeY);
                            NodeContainer.AddArrowToModel(arrow);
                        }
                        else
                        {
                            NodeContainer.RemoveArrow(arrow);
                            Canvas.Children.Remove(arrow);
                        }
                    }
                    else
                    {
                        NodeContainer.RemoveArrow(arrow);
                        Canvas.Children.Remove(arrow);
                    }
                }

                isArrow = false;
                arrow = null;
                #endregion isArrow
            }
        }

        private void Arrow_ArrowCapMouseDown(object sender, MouseButtonEventArgs e)
        {
            arrow = sender as Arrow;
            NodeContainer.RemoveArrowFromModel(arrow);
            arrow.To = null;            
            arrow.P2 = e.GetPosition(Canvas);
            isArrow = true;
            isArrowStart = true;
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                NodeControl node = selected as NodeControl;
                if (!(node.Node is LoopPortNode))
                {
                    foreach (var arrow in NodeContainer.Arrows.Where(a => a.From.Owner == node.Node | a.To.Owner == node.Node).ToList())
                    {
                        NodeContainer.RemoveArrow(arrow);
                        NodeContainer.RemoveArrowFromModel(arrow);
                        Canvas.Children.Remove(arrow);
                    }
                    NodeContainer.RemoveNode(node);
                    Canvas.Children.Remove(node);
                }
            }
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            NewExperiment();
        }

        private void LoadExperimentGraph(string path)
        {
            XDocument xDocument = XDocument.Load(path);
            NewExperiment();
            var experimentElement = xDocument.Root;
            experiment.Path = experimentElement.Attribute("path").Value;
            LoadBlock(experimentElement.Element("block"));
        }

        private void LoadBlock(XElement blockElement)
        {
            foreach (var nodeElement in blockElement.Element("nodes").Elements("node"))
            {
                IRunable modelNode = null;
                NodeControl nodeControl = null;
                string name = nodeElement.Attribute("name").Value;
                double x = Convert.ToDouble(nodeElement.Attribute("x").Value, CultureInfo.InvariantCulture);
                double y = Convert.ToDouble(nodeElement.Attribute("y").Value, CultureInfo.InvariantCulture);
                if (nodeElement.Attribute("type") != null)
                {
                    Type nodeType = Type.GetType(nodeElement.Attribute("type").Value);
                    
                    if (nodeType == typeof(InitNode))
                        modelNode = new InitNode(name, NodeContainer.Block);
                    if (nodeType == typeof(InPortNode))
                        modelNode = new InPortNode(name, "", DataType.VALUE, typeof(int), NodeContainer.Block);
                    if (nodeType == typeof(OutPortNode))
                        modelNode = new OutPortNode(name, "", DataType.VALUE, typeof(int), NodeContainer.Block);
                    if (nodeType == typeof(LoopBlock))
                        modelNode = new LoopBlock(name, NodeContainer.Block);
                    if (nodeType == typeof(Block))
                        modelNode = new Block(name, NodeContainer.Block);

                    if (nodeType != typeof(LoopPortNode))
                    {
                        nodeControl = CreateNodeControl(modelNode, new Point(x, y));
                    }
                    else
                    {
                        var blockLoopPortNode = (NodeContainer.Block as LoopBlock).LoopPortNode;
                        var loopPortNodeControl = NodeContainer.NodeControls.FirstOrDefault(c => c.Node == blockLoopPortNode);
                        Canvas.SetLeft(loopPortNodeControl, x);
                        Canvas.SetTop(loopPortNodeControl, y);
                        blockLoopPortNode.Name = name;
                        modelNode = blockLoopPortNode;
                    }

                    if (modelNode is Block)
                    {
                        LoadBlock(nodeElement.Element("block"));
                        GoToParentBlock();
                    }
                    else
                    {
                        XmlSettingsDeserializer.DeserializeSettings(modelNode, nodeElement.Element("settings"));                        
                    }
                }
                else
                {
                    // Algorithm
                    Type algType = PluginManager.Instance.FindByAssemblyQualifiedName(nodeElement.Attribute("algType").Value);
                    modelNode = new AlgorithmNode(name, algType, NodeContainer.Block);
                    nodeControl = CreateNodeControl(modelNode, new Point(x, y));
                    XmlSettingsDeserializer.DeserializeSettings(modelNode, nodeElement.Element("settings"));
                }
            }

            foreach (NodeControl node in NodeContainer.NodeControls)
            {
                node.LoadPorts();
                node.UpdateLayout();
            }

            // todo: rework to storing as connection between ports
            foreach (var arrowElement in blockElement.Element("arrows").Elements("arrow"))
            {
                double x1 = Convert.ToDouble(arrowElement.Attribute("x1").Value, CultureInfo.InvariantCulture);
                double y1 = Convert.ToDouble(arrowElement.Attribute("y1").Value, CultureInfo.InvariantCulture);
                double x2 = Convert.ToDouble(arrowElement.Attribute("x2").Value, CultureInfo.InvariantCulture);
                double y2 = Convert.ToDouble(arrowElement.Attribute("y2").Value, CultureInfo.InvariantCulture);
                Point p1 = new Point(x1, y1);
                Point p2 = new Point(x2, y2);

                Port start = null, end = null;
                foreach (NodeControl node in NodeContainer.NodeControls)
                {
                    Vector v = new Vector(Canvas.GetLeft(node), Canvas.GetTop(node));
                    if (start == null)
                        start = node.FindOutPortByPoint(p1, v, 1);
                    if (end == null)
                        end = node.FindInPortByPoint(p2, v, 1);
                    if (start != null && end != null)
                        break;
                }

                if (start != null && end != null)
                {
                    Arrow arrow = new Arrow(x1, y1, x2, y2);
                    arrow.From = start;
                    arrow.To = end;
                    arrow.ArrowCapMouseDown += Arrow_ArrowCapMouseDown;
                    Canvas.Children.Add(arrow);
                    NodeContainer.AddArrow(arrow);
                    NodeContainer.AddArrowToModel(arrow);
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
                new XmlSerializer(blockGraphs, experiment).Serialize().Save(saveFileDialog.FileName);
            }
        }

        private void GoToParentBlock()
        {
            if (NodeContainer.Parent != null)
            {
                NodeContainer = NodeContainer.Parent;
                NodeContainer.DisplayOnCanvas(Canvas);

                foreach (NodeControl node in NodeContainer.NodeControls)
                {
                    node.LoadPorts();
                    node.UpdateLayout();
                    UpdateArrows(node);
                }
            }
        }

        private void BackLoopButton_Click(object sender, RoutedEventArgs e)
        {
            GoToParentBlock();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = SettingsWindowHelper.CreateSettingWindowForNode(NodeContainer.Block, this);
            bool? dialogResult = window.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                window.GetNode();
                SetNodeContainerName();
            }
        }
    }
}

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
using HydrologyDesktop.Controls;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Globalization;

namespace HydrologyDesktop
{
    public enum EditorMode {
        Pointer,
        Arrow
    }

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Core hydrologyCore;

        private ExperimentGraph experimentGraph;

        //private IList<NodeControl> nodes;
        //private IList<Arrow> arrows;

        private bool isMove;
        private Vector delta;

        private Point startPoint;
        private bool isDragDrop;

        private EditorMode mode = EditorMode.Pointer;
        private Arrow editingArrow = null;

        private UIElement selected;

        private System.Windows.Forms.FolderBrowserDialog folderDialog;
        private OpenFileDialog fileDialog;

        private SaveFileDialog saveFileDialog = new SaveFileDialog()
        {
            Filter = "Experiment Graph Files (*.xml)|*.xml",
            AddExtension = true
        };

        private OpenFileDialog openFileDialog = new OpenFileDialog()
        {
            Filter = "Experiment Graph Files (*.xml)|*.xml",
            AddExtension = true
        };

        public MainWindow()
        {
            InitializeComponent();
            Init();
            NewExperiment();
        }

        private void Init()
        {
            hydrologyCore = new Core();
            IList<Type> algTypes = hydrologyCore.AlgorithmTypes.Values.ToList();
            folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            fileDialog = new OpenFileDialog() { Multiselect = false };

            // add algorithm buttons to toolkit
            foreach (Type type in algTypes)
            {
                Button b = new Button();
                b.Tag = type.Name;
                var attr = type.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
                b.Content = attr.Name;
                b.Style = (Style)this.FindResource("AlgButtonStyle");
                b.Click += algButton_Click;
                b.PreviewMouseDown += Button_PreviewMouseDown;
                b.PreviewMouseMove += algButton_PreviewMouseMove;
                b.PreviewMouseUp += Button_PreviewMouseUp;
                AlgorithmButtonsPanel.Children.Add(b);
            }
        }

        void NewExperiment()
        {
            Canvas.Children.Clear();
            experimentGraph = new ExperimentGraph();
            //nodes = new List<NodeControl>();
            //arrows = new List<Arrow>();
        }

        void AddAlgNode(string algName, double x = 0, double y = 0)
        {
            Type algType = hydrologyCore.AlgorithmTypes[algName];

            DataTable paramsTable = new DataTable();
            paramsTable.Columns.Add("Name");
            paramsTable.Columns.Add("Value");

            foreach (ParameterAttribute attr in algType.GetCustomAttributes<ParameterAttribute>())
            {
                DataRow row = paramsTable.NewRow();
                row["Name"] = attr.Name;
                row["Value"] = attr.DefaultValue;
                paramsTable.Rows.Add(row);
            }

            var algSettings = new AlgorithmSettings(folderDialog, paramsTable, algType) 
            { 
                Owner = this, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner 
            };
            bool? dialogResult = algSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                AddNode(algType, algSettings.Path, algSettings.ParamsTable, x, y);
            }
        }

        void AddNode(Type algType, string initPath, DataTable paramTable, double x = 0, double y = 0)
        {
            var attr = algType.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
            NodeControl node = new AlgorithmNodeControl()
            {
                NodeName = attr.Name,
                AlgorithmType = algType,
                InitPath = initPath,
                ParamsTable = paramTable
            };
            node.Style = (Style)this.FindResource("NodeStyle");
            node.SettingsButtonClick += node_SettingsClicked;
            node.MouseLeftButtonDown += node_MouseLeftButtonDown;
            node.MouseLeftButtonUp += node_MouseLeftButtonUp;
            node.MouseMove += node_MouseMove;
            node.LostMouseCapture += node_LostMouseCapture;

            Canvas.Children.Add(node);
            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
            experimentGraph.Nodes.Add(node);
        }

        void node_LostMouseCapture(object sender, MouseEventArgs e)
        {
            isMove = false;
        }

        void node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {  
            switch (mode) 
            { 
                case EditorMode.Pointer:
                    if (sender is NodeControl) 
                    {
                        NodeControl node = sender as NodeControl;
                        isMove = true;                    
                        Point pos = e.GetPosition(Canvas);
                        delta = new Vector(pos.X - Canvas.GetLeft(node), pos.Y - Canvas.GetTop(node));
                        node.CaptureMouse();
                    }
                    break;
                case EditorMode.Arrow:
                    if (sender is NodeControl && editingArrow == null) 
                    {
                        NodeControl node = sender as NodeControl;
                        Point p = node.FindAttachPoint(e.GetPosition(node));
                        p.X = p.X * 1.0 / node.ActualWidth;
                        p.Y = p.Y * 1.0 / node.ActualHeight;
                        editingArrow = new Arrow(node, p, e.GetPosition(Canvas));
                        Canvas.Children.Add(editingArrow);
                    }
                    break;
            }
        }

        void node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMove = false;
            (sender as UserControl).ReleaseMouseCapture();
        }

        void node_MouseMove(object sender, MouseEventArgs e)
        {
            switch (mode)
            {
                case EditorMode.Pointer:
                    {
                        if (isMove)
                        {
                            NodeControl node = sender as NodeControl;
                            Point pos = e.GetPosition(Canvas);
                            Canvas.SetLeft(node, pos.X - delta.X);
                            Canvas.SetTop(node, pos.Y - delta.Y);

                            foreach (Arrow arrow in experimentGraph.Arrows)
                            {
                                if (arrow.From == node || arrow.To == node)
                                {
                                    arrow.Draw();
                                }
                            }
                        }
                        break;
                    }
                case EditorMode.Arrow:
                    break;
            }
            
        }

        void node_SettingsClicked(object sender, EventArgs e)
        {
            AlgorithmNodeControl node = (AlgorithmNodeControl)sender;
            var algSettings = new AlgorithmSettings(folderDialog, node.ParamsTable, node.AlgorithmType) 
            { 
                Owner = this, 
                Path = node.InitPath, 
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner 
            };
            bool? dialogResult = algSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                node.InitPath = algSettings.Path;
                node.ParamsTable = algSettings.ParamsTable;
            }
        }

        void algButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string algName = btn.Content.ToString();

            AddAlgNode(algName);
        }

        private void algButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && !isDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragDrop = true;
                    Button btn = (Button)sender;
                    DataObject dragData = new DataObject("NodeNameFormat", btn.Tag);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IDictionary<NodeControl, IList<NodeControl>> ascendents = new Dictionary<NodeControl, IList<NodeControl>>();
            IList<NodeControl> left = new List<NodeControl>();

            foreach (NodeControl node in experimentGraph.Nodes) {
                ascendents.Add(node, new List<NodeControl>());
                left.Add(node);
            }
            
            foreach (Arrow arrow in experimentGraph.Arrows)
            {
                if (arrow.From != null && arrow.To != null)
                    ascendents[arrow.To].Add(arrow.From);
            }

            IList<NodeControl> res = new List<NodeControl>();

            Queue<NodeControl> q = new Queue<NodeControl>();
            for (int i = 0; i < left.Count; i++)
            {
                NodeControl node = left[i];
                if (ascendents[node].Count == 0)
                {
                    q.Enqueue(node);
                    left.Remove(node);
                    i--;
                }
            }

            while (q.Count > 0)
            {
                NodeControl p = q.Dequeue();
                res.Add(p);

                for (int i = 0; i < left.Count; i++)
                {
                    NodeControl node = left[i];
                    ascendents[node].Remove(p);
                    if (ascendents[node].Count == 0)
                    {
                        q.Enqueue(node);
                        left.Remove(node);
                        i--;
                    }
                }
            }

            ExecutorWindow experimentWindow = new ExecutorWindow(res, hydrologyCore)
            {
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
            };
            experimentWindow.ShowDialog(); 
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("NodeFormat"))
            {
                NodeControl node = (NodeControl) e.Data.GetData("NodeFormat");
                if (node is InitNodeControl)
                {
                    var initSettings = new InitSettingsWindow(folderDialog) 
                    { 
                        Owner = this, 
                        InitPath = (node as InitNodeControl).InitPath,
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner 
                    };
                    bool? dialogResult = initSettings.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        (node as InitNodeControl).InitPath = initSettings.InitPath;
                        Canvas.Children.Add(node);
                        Point pos = e.GetPosition(Canvas);
                        Canvas.SetLeft(node, pos.X);
                        Canvas.SetTop(node, pos.Y);
                        experimentGraph.Nodes.Add(node);
                    }
                    isDragDrop = false;
                }
                else if (node is RunProcessNodeControl)
                {
                    var runProcessSettings = new RunProcessSettingsWindow(fileDialog)
                    {
                        ProcessName = (node as RunProcessNodeControl).ProcessName,
                        Owner = this,
                        WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                    };
                    bool? dialogResult = runProcessSettings.ShowDialog();
                    if (dialogResult.HasValue && dialogResult.Value)
                    {
                        (node as RunProcessNodeControl).ProcessName = runProcessSettings.ProcessName;
                        Canvas.Children.Add(node);
                        Point pos = e.GetPosition(Canvas);
                        Canvas.SetLeft(node, pos.X);
                        Canvas.SetTop(node, pos.Y);
                        experimentGraph.Nodes.Add(node);
                    }
                    isDragDrop = false;
                }
            }

            if (e.Data.GetDataPresent("NodeNameFormat")) 
            {
                String nodeName = e.Data.GetData("NodeNameFormat").ToString();
                Point pos = e.GetPosition(Canvas);
                AddAlgNode(nodeName, pos.X, pos.Y);
                isDragDrop = false;
            }
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragDrop = false;
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
                        node.Thickness = new Thickness(3);
                        selected = child;
                    }
                    else
                    {
                        node.Thickness = new Thickness(1);
                    }
                }
                else if (child is Arrow)
                {
                    Arrow arrow = child as Arrow;
                    if (arrow.IsMouseOver)
                    {
                        arrow.AttachPointsVisibility = Visibility.Visible;
                        selected = arrow;
                    }
                    else
                    {
                        arrow.AttachPointsVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        private void initButton_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && !isDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragDrop = true;

                    Button btn = (Button)sender;

                    InitNodeControl node = new InitNodeControl() { CornerRadius = new CornerRadius(10) };
                    node.NodeName = "Инициализация";
                    node.Style = (Style)this.FindResource("NodeStyle");
                    node.MouseLeftButtonDown += node_MouseLeftButtonDown;
                    node.MouseLeftButtonUp += node_MouseLeftButtonUp;
                    node.MouseMove += node_MouseMove;
                    node.LostMouseCapture += node_LostMouseCapture;
                    node.SettingsButtonClick += initNode_SettingsButtonClick;

                    DataObject dragData = new DataObject("NodeFormat", node);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        private void initNode_SettingsButtonClick(object sender, EventArgs e)
        {
            InitNodeControl node = sender as InitNodeControl;
            var initSettings = new InitSettingsWindow(folderDialog) 
            { 
                Owner = this, 
                InitPath = node.InitPath, 
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner 
            };
            bool? dialogResult = initSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                node.InitPath = initSettings.InitPath;
            }
        }

        private void PointerBtn_Click(object sender, RoutedEventArgs e)
        {
            mode = EditorMode.Pointer;
            PointerBtn.Background = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            PointerBtn.Foreground = Brushes.White;
            ArrowBtn.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            ArrowBtn.Foreground = Brushes.Black;
        }

        private void ArrowBtn_Click(object sender, RoutedEventArgs e)
        {
            mode = EditorMode.Arrow;
            ArrowBtn.Background = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            ArrowBtn.Foreground = Brushes.White;
            PointerBtn.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            PointerBtn.Foreground = Brushes.Black;
        }

        private void Arrow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Arrow arrow = sender as Arrow;
            if (arrow.AttachEllipse != null)
            {
                arrow.MoveEllipse = arrow.AttachEllipse;
                editingArrow = arrow;
                arrow.AttachEllipse.CaptureMouse();
            }
        }

        private void Arrow_MouseMove(object sender, MouseEventArgs e)
        {
            Arrow arrow = sender as Arrow;
            if (arrow.MoveEllipse != null && editingArrow == arrow)
            {
                if (arrow.MoveEllipse == arrow.pointFrom)
                {
                    arrow.From = null;
                    arrow.Start = e.GetPosition(Canvas);
                }
                else
                {
                    arrow.To = null;
                    arrow.End = e.GetPosition(Canvas) - new Vector(0, 2);
                }
            }
        }

        private void Arrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Arrow arrow = sender as Arrow;
            Point p = e.GetPosition(Canvas);

            if (arrow.MoveEllipse != null && editingArrow == arrow)
            {
                foreach (UIElement element in Canvas.Children)
                {
                    if (element is NodeControl)
                    {
                        NodeControl node = element as NodeControl;
                        double x = Canvas.GetLeft(node);
                        double y = Canvas.GetTop(node);

                        if (p.X >= x && p.X <= x + node.ActualWidth && p.Y >= y && p.Y <= y + node.ActualHeight)
                        {
                            Point relative = node.FindAttachPoint(e.GetPosition(node));
                            relative.X = relative.X * 1.0 / node.ActualWidth;
                            relative.Y = relative.Y * 1.0 / node.ActualHeight;

                            if (arrow.MoveEllipse == arrow.pointFrom)
                            {
                                arrow.From = node;
                                arrow.StartRelative = relative;
                            }
                            else if (arrow.MoveEllipse == arrow.pointTo)
                            {
                                arrow.To = node;
                                arrow.EndRelative = relative;
                            }
                            arrow.Draw();
                            arrow.MoveEllipse = null;
                            break;
                        }
                    }
                }
                
                editingArrow = null;
                arrow.pointTo.ReleaseMouseCapture();
                arrow.pointFrom.ReleaseMouseCapture();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mode == EditorMode.Arrow && editingArrow != null)
            {
                editingArrow.End = e.GetPosition(Canvas) - new Vector(0, 2);
            } 
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mode == EditorMode.Arrow && editingArrow != null)
            {
                Point pos = e.GetPosition(Canvas);
                bool found = false;
                for (int i = Canvas.Children.Count - 1; i >= 0 && !found; i--)
                {
                    UIElement child = Canvas.Children[i];
                    if (child is NodeControl)
                    {
                        NodeControl node = child as NodeControl;
                        double x = Canvas.GetLeft(node), y = Canvas.GetTop(node);
                        if (experimentGraph.Arrows.FirstOrDefault((a) => { return a.From == editingArrow.From && a.To == node; }) == null)
                        {
                            if (pos.X >= x - 4 && pos.X <= x + node.ActualWidth + 4 && pos.Y >= y - 4 && pos.Y <= y + node.ActualHeight + 4)
                            {
                                editingArrow.To = node;
                                if (node.AttachEllipse != null)
                                {
                                    Point p = node.AttachPoint;
                                    p.X = p.X / node.ActualWidth;
                                    p.Y = p.Y / node.ActualHeight;
                                    editingArrow.EndRelative = p;
                                }
                                else
                                {
                                    Point p = node.FindAttachPoint(e.GetPosition(node));
                                    p.X = p.X / node.ActualWidth;
                                    p.Y = p.Y / node.ActualHeight;
                                    editingArrow.EndRelative = p;
                                }

                                editingArrow.MouseDown += Arrow_MouseDown;
                                editingArrow.MouseMove += Arrow_MouseMove;
                                editingArrow.MouseUp += Arrow_MouseUp;

                                experimentGraph.Arrows.Add(editingArrow);
                                editingArrow = null;
                                found = true;
                            }
                        }                        
                    }
                }
                if (!found)
                {
                    Canvas.Children.Remove(editingArrow);
                    editingArrow = null;
                }
            }
        }

        private void exportButton_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed && !isDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    isDragDrop = true;

                    Button btn = (Button)sender;

                    RunProcessNodeControl node = new RunProcessNodeControl() { CornerRadius = new CornerRadius(10) };
                    node.NodeName = "Запуск приложения";
                    node.Style = (Style)this.FindResource("NodeStyle");
                    node.MouseLeftButtonDown += node_MouseLeftButtonDown;
                    node.MouseLeftButtonUp += node_MouseLeftButtonUp;
                    node.MouseMove += node_MouseMove;
                    node.LostMouseCapture += node_LostMouseCapture;
                    node.SettingsButtonClick += exportNode_SettingsButtonClick;

                    DataObject dragData = new DataObject("NodeFormat", node);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        void exportNode_SettingsButtonClick(object sender, EventArgs e)
        {
            RunProcessNodeControl node = sender as RunProcessNodeControl;
            var runProcessSettings = new RunProcessSettingsWindow(fileDialog)
            {
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                ProcessName = node.ProcessName
            };
            bool? dialogResult = runProcessSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                node.ProcessName = runProcessSettings.ProcessName;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && selected != null)
            {
                Canvas.Children.Remove(selected);
                if (selected is Arrow)
                {
                    experimentGraph.Arrows.Remove(selected as Arrow);     
                }
                else if (selected is NodeControl)
                {
                    NodeControl node = selected as NodeControl;
                    experimentGraph.Nodes.Remove(node);

                    var arrs = experimentGraph.Arrows.Where((a) => { return a.From == node || a.To == node; }).ToList();

                    for (int i = 0; i < arrs.Count(); i++)
                    {
                        Arrow a = arrs[i];
                        Canvas.Children.Remove(a);
                        experimentGraph.Arrows.Remove(a);
                    }
                }

                selected = null;
            }
        }

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            NewExperiment();
        }

        private void LoadExperimentGraph(string path)
        {
            NewExperiment();
            XDocument xDocument = XDocument.Load(path);
            foreach (XElement node in xDocument.Element("experiment").Element("nodes").Elements("node"))
            {
                NodeControl nodeControl;
                switch (node.Attribute("type").Value)
                {
                    case "init":
                        nodeControl = new InitNodeControl() { CornerRadius = new CornerRadius(10) };
                        (nodeControl as InitNodeControl).InitPath = node.Element("initpath").Value;
                        nodeControl.NodeName = "Инициализация";
                        nodeControl.SettingsButtonClick += initNode_SettingsButtonClick;
                        break;
                    case "runprocess":
                        nodeControl = new RunProcessNodeControl() { CornerRadius = new CornerRadius(10) };
                        (nodeControl as RunProcessNodeControl).ProcessName = node.Element("processname").Value;
                        nodeControl.NodeName = "Запуск приложения";
                        nodeControl.SettingsButtonClick += exportNode_SettingsButtonClick;
                        break;
                    case "algorithm":
                        nodeControl = new AlgorithmNodeControl();
                        (nodeControl as AlgorithmNodeControl).InitPath = node.Element("initpath").Value;
                        (nodeControl as AlgorithmNodeControl).AlgorithmType = hydrologyCore.AlgorithmTypes[node.Element("algorithmtype").Value];
                        DataTable paramTable = new DataTable();
                        paramTable.Columns.Add("Name");
                        paramTable.Columns.Add("Value");
                        foreach (XElement param in node.Element("params").Elements("param"))
                        {
                            DataRow row = paramTable.NewRow();
                            row["Name"] = param.Attribute("name").Value;
                            row["Value"] = param.Attribute("value").Value;
                            paramTable.Rows.Add(row);
                        }
                        (nodeControl as AlgorithmNodeControl).ParamsTable = paramTable;
                        var attr = (nodeControl as AlgorithmNodeControl).AlgorithmType.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
                        nodeControl.NodeName = attr.Name;
                        nodeControl.SettingsButtonClick += node_SettingsClicked;
                        break;
                    default:
                        nodeControl = new NodeControl();
                        break;
                }
                nodeControl.Style = (Style)this.FindResource("NodeStyle");
                nodeControl.MouseLeftButtonDown += node_MouseLeftButtonDown;
                nodeControl.MouseLeftButtonUp += node_MouseLeftButtonUp;
                nodeControl.MouseMove += node_MouseMove;
                nodeControl.LostMouseCapture += node_LostMouseCapture;

                Canvas.Children.Add(nodeControl);
                Canvas.SetLeft(nodeControl, double.Parse(node.Attribute("x").Value, CultureInfo.InvariantCulture));
                Canvas.SetTop(nodeControl, double.Parse(node.Attribute("y").Value, CultureInfo.InvariantCulture));
                experimentGraph.Nodes.Add(nodeControl);
            }

            Canvas.UpdateLayout();

            foreach (XElement arrow in xDocument.Element("experiment").Element("arrows").Elements("arrow"))
            {
                var arr = new Arrow(
                    experimentGraph.Nodes[int.Parse(arrow.Element("from").Attribute("id").Value)],
                    new Point(double.Parse(arrow.Element("from").Attribute("x").Value, CultureInfo.InvariantCulture),
                              double.Parse(arrow.Element("from").Attribute("y").Value, CultureInfo.InvariantCulture)),
                    experimentGraph.Nodes[int.Parse(arrow.Element("to").Attribute("id").Value)],
                    new Point(double.Parse(arrow.Element("to").Attribute("x").Value, CultureInfo.InvariantCulture),
                              double.Parse(arrow.Element("to").Attribute("y").Value, CultureInfo.InvariantCulture))
                );
                arr.MouseDown += Arrow_MouseDown;
                arr.MouseMove += Arrow_MouseMove;
                arr.MouseUp += Arrow_MouseUp;
                Canvas.Children.Add(arr);
                experimentGraph.Arrows.Add(arr);
                arr.Draw();
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
                experimentGraph.ToXml().Save(saveFileDialog.FileName);
            }
        }
    }
}

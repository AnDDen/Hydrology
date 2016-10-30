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
        public Core HydrologyCore;

        private ExperimentGraph experimentGraph;

        //private IList<NodeControl> nodes;
        //private IList<Arrow> arrows;

        private bool isMove;
        private Vector delta;

        private Point startPoint;
        public bool IsDragDrop { get; set; }

        private EditorMode mode = EditorMode.Pointer;
        public EditorMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        private Arrow editingArrow = null;
        public Arrow EditingArrow
        {
            get { return editingArrow; }
            set { editingArrow = value; }
        }

        private UIElement selected;
        public UIElement Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public System.Windows.Forms.FolderBrowserDialog FolderDialog { get; set; }
        public OpenFileDialog FileDialog { get; set; }

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
            HydrologyCore = new Core();
            IList<Type> algTypes = HydrologyCore.AlgorithmTypes.Values.ToList();
            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            FileDialog = new OpenFileDialog() { Multiselect = false };

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

        void AddAlgNode(string algName, double x = 0, double y = 0, LoopControl loop = null)
        {
            Type algType = HydrologyCore.AlgorithmTypes[algName];

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

            var algSettings = new AlgorithmSettings(FolderDialog, paramsTable, algType) 
            { 
                Owner = this, 
                WindowStartupLocation = WindowStartupLocation.CenterOwner 
            };
            if (ViewedLoop != null)
                algSettings.ParentLoop = ViewedLoop;
            bool? dialogResult = algSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                AddNode(algType, algSettings.Path, algSettings.ParamsTable, x, y, loop, algSettings.VarLoop);
            }
        }

        void AddNode(Type algType, string initPath, DataTable paramTable, double x = 0, double y = 0, LoopControl loop = null, Dictionary<string, LoopControl> varLoop = null)
        {
            var attr = algType.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
            NodeControl node = new AlgorithmNodeControl()
            {
                NodeName = attr.Name,
                AlgorithmType = algType,
                InitPath = initPath,
                ParamsTable = paramTable,
                VarLoop = varLoop
            };
            node.Style = (Style)this.FindResource("NodeStyle");
            node.SettingsButtonClick += node_SettingsClicked;
            node.MouseLeftButtonDown += node_MouseLeftButtonDown;
            node.MouseLeftButtonUp += node_MouseLeftButtonUp;
            node.MouseMove += node_MouseMove;
            node.LostMouseCapture += node_LostMouseCapture;

            Canvas.Children.Add(node);
            experimentGraph.Nodes.Add(node);

            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
        }

        public void node_LostMouseCapture(object sender, MouseEventArgs e)
        {
            isMove = false;
        }

        object moveControl = null;

        public void node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {  
            switch (mode) 
            { 
                case EditorMode.Pointer:
                    if (moveControl == null && sender is BaseNodeControl) 
                    {
                        BaseNodeControl node = sender as BaseNodeControl;
                        isMove = true;                    
                        Point pos = e.GetPosition(ViewedLoop == null ? Canvas : ViewedLoop.Canvas);
                        delta = new Vector(pos.X - Canvas.GetLeft(node), pos.Y - Canvas.GetTop(node));
                        node.CaptureMouse();
                        moveControl = node;
                    }
                    break;
                case EditorMode.Arrow:
                    if (sender is BaseNodeControl && editingArrow == null) 
                    {
                        BaseNodeControl node = sender as BaseNodeControl;
                        Point p = node.FindAttachPoint(e.GetPosition(node));
                        p.X = p.X * 1.0 / node.ActualWidth;
                        p.Y = p.Y * 1.0 / node.ActualHeight;
                        if (ViewedLoop == null)
                        {
                            editingArrow = new Arrow(node, p, e.GetPosition(Canvas));
                            Canvas.Children.Add(editingArrow);
                        }
                        else
                        {
                            editingArrow = new Arrow(node, p, e.GetPosition(ViewedLoop.Canvas));
                            ViewedLoop.Canvas.Children.Add(editingArrow);
                        }
                    }
                    break;
            }
        }

        public void node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMove = false;
            moveControl = null;
            (sender as UserControl).ReleaseMouseCapture();
        }

        public void node_MouseMove(object sender, MouseEventArgs e)
        {
            switch (mode)
            {
                case EditorMode.Pointer:
                    {
                        if (isMove && moveControl == sender)
                        {
                            BaseNodeControl node = sender as BaseNodeControl;
                            Point pos = e.GetPosition(ViewedLoop == null ? Canvas : ViewedLoop.Canvas);
                            Canvas.SetLeft(node, pos.X - delta.X);
                            Canvas.SetTop(node, pos.Y - delta.Y);

                            if (ViewedLoop == null)
                            {
                                foreach (Arrow arrow in experimentGraph.Arrows)
                                {
                                    if (arrow.From == node || arrow.To == node)
                                    {
                                        arrow.Draw();
                                    }
                                }
                            }
                            else
                            {
                                foreach (Arrow arrow in ViewedLoop.LoopBody.Arrows)
                                {
                                    if (arrow.From == node || arrow.To == node)
                                    {
                                        arrow.Draw();
                                    }
                                }
                            }
                        }
                        break;
                    }
                case EditorMode.Arrow:
                    break;
            }
            
        }

        public void node_SettingsClicked(object sender, EventArgs e)
        {
            AlgorithmNodeControl node = (AlgorithmNodeControl)sender;
            var algSettings = new AlgorithmSettings(FolderDialog, node.ParamsTable, node.AlgorithmType) 
            { 
                Owner = this, 
                Path = node.InitPath, 
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner 
            };
            if (ViewedLoop != null)
                algSettings.ParentLoop = ViewedLoop;
            try
            {
                bool? dialogResult = algSettings.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    node.InitPath = algSettings.Path;
                    node.ParamsTable = algSettings.ParamsTable;
                    node.VarLoop = algSettings.VarLoop;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Произошла ошибка во время сохранения настроек: {0}", ex.Message),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (e.LeftButton == MouseButtonState.Pressed && !IsDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IsDragDrop = true;
                    Button btn = (Button)sender;
                    DataObject dragData = new DataObject("NodeNameFormat", btn.Tag);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExecutorWindow experimentWindow = new ExecutorWindow(experimentGraph, HydrologyCore)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            experimentWindow.ShowDialog(); 
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (IsDragDrop)
            { 
                if (e.Data.GetDataPresent("NodeFormat"))
                {
                    BaseNodeControl node = (BaseNodeControl)e.Data.GetData("NodeFormat");
                    if (node is InitNodeControl)
                    {
                        var initSettings = new InitSettingsWindow(FolderDialog)
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
                        IsDragDrop = false;
                    }
                    else if (node is RunProcessNodeControl)
                    {
                        var runProcessSettings = new RunProcessSettingsWindow(FileDialog)
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
                        IsDragDrop = false;
                    }
                    else if (node is LoopControl)
                    {
                        var loopSettings = new LoopSettingsWindow()
                        {
                            Owner = this,
                            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner
                        };
                        bool? dialogResult = loopSettings.ShowDialog();
                        if (dialogResult.HasValue && dialogResult.Value)
                        {
                            (node as LoopControl).VarName = loopSettings.VarName;
                            (node as LoopControl).StartValue = loopSettings.StartValue;
                            (node as LoopControl).EndValue = loopSettings.EndValue;
                            (node as LoopControl).Step = loopSettings.Step;

                            Canvas.Children.Add(node);
                            Point pos = e.GetPosition(Canvas);
                            Canvas.SetLeft(node, pos.X);
                            Canvas.SetTop(node, pos.Y);
                            experimentGraph.Nodes.Add(node);
                        }
                        IsDragDrop = false;
                    }
                }

                if (e.Data.GetDataPresent("NodeNameFormat"))
                {
                    String nodeName = e.Data.GetData("NodeNameFormat").ToString();
                    Point pos = e.GetPosition(Canvas);
                    AddAlgNode(nodeName, pos.X, pos.Y);
                    IsDragDrop = false;
                }
            }
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsDragDrop = false;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            selected = null;
            for (int i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Canvas.Children[i];
                if (child is BaseNodeControl)
                {
                    BaseNodeControl node = child as BaseNodeControl;
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

            if (e.LeftButton == MouseButtonState.Pressed && !IsDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IsDragDrop = true;

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
            var initSettings = new InitSettingsWindow(FolderDialog) 
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

        private void loopButton_PreviewMouseMove(object sender, MouseEventArgs e)
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

                    LoopControl node = new LoopControl(this);
                    node.MouseLeftButtonDown += node_MouseLeftButtonDown;
                    node.MouseLeftButtonUp += node_MouseLeftButtonUp;
                    node.MouseMove += node_MouseMove;
                    node.LostMouseCapture += node_LostMouseCapture;
                    node.SettingsButtonClick += loopNode_SettingsButtonClick;

                    DataObject dragData = new DataObject("NodeFormat", node);
                    DragDrop.DoDragDrop(btn, dragData, DragDropEffects.Move);
                }
            }
        }

        private void loopNode_SettingsButtonClick(object sender, EventArgs e)
        {
            LoopControl node = sender as LoopControl;
            var loopSettings = new LoopSettingsWindow()
            {
                Owner = this,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                VarName = node.VarName,
                StartValue = node.StartValue,
                EndValue = node.EndValue,
                Step = node.Step
            };
            bool? dialogResult = loopSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                node.VarName = loopSettings.VarName;
                node.StartValue = loopSettings.StartValue;
                node.EndValue = loopSettings.EndValue;
                node.Step = loopSettings.Step;
            }
        }

        private void PointerBtn_Click(object sender, RoutedEventArgs e)
        {
            mode = EditorMode.Pointer;
            PointerBtn.Tag = "Selected";
            ArrowBtn.Tag = "";
        }

        private void ArrowBtn_Click(object sender, RoutedEventArgs e)
        {
            mode = EditorMode.Arrow;
            PointerBtn.Tag = "";
            ArrowBtn.Tag = "Selected";
        }

        public void Arrow_MouseDown(object sender, MouseButtonEventArgs e)
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
                    if (element is BaseNodeControl)
                    {
                        BaseNodeControl node = element as BaseNodeControl;
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
                    if (child is BaseNodeControl)
                    {
                        BaseNodeControl node = child as BaseNodeControl;
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

            if (e.LeftButton == MouseButtonState.Pressed && !IsDragDrop)
            {
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IsDragDrop = true;

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
            var runProcessSettings = new RunProcessSettingsWindow(FileDialog)
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
            if (ViewedLoop == null)
            {
                if (e.Key == Key.Delete && selected != null)
                {
                    if (selected is Arrow)
                    {
                        Canvas.Children.Remove(selected);
                        experimentGraph.Arrows.Remove(selected as Arrow);
                    }
                    else if (selected is BaseNodeControl && !(selected is StartLoopNodeControl))
                    {
                        Canvas.Children.Remove(selected);
                        BaseNodeControl node = selected as BaseNodeControl;
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
            else
            {
                if (e.Key == Key.Delete && selected != null)
                {                   
                    if (selected is Arrow)
                    {
                        ViewedLoop.Canvas.Children.Remove(selected);
                        ViewedLoop.LoopBody.Arrows.Remove(selected as Arrow);
                    }
                    else if (selected is BaseNodeControl && !(selected is StartLoopNodeControl))
                    {
                        ViewedLoop.Canvas.Children.Remove(selected);
                        BaseNodeControl node = selected as BaseNodeControl;

                        ViewedLoop.LoopBody.Nodes.Remove(node);

                        var arrs = ViewedLoop.LoopBody.Arrows.Where((a) => { return a.From == node || a.To == node; }).ToList();

                        for (int i = 0; i < arrs.Count(); i++)
                        {
                            Arrow a = arrs[i];
                            ViewedLoop.Canvas.Children.Remove(a);
                            ViewedLoop.LoopBody.Arrows.Remove(a);
                        }
                    }

                    selected = null;
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
            LoadExperimentGraph(xDocument.Root);
        }

        private void LoadExperimentGraph(XElement root, LoopControl loop = null)
        {
            if (loop == null)
                NewExperiment();
            else {
                loop.LoopBody = new ExperimentGraph();
                loop.Canvas.Children.Clear();
            }
            
            foreach (XElement node in root.Element("nodes").Elements("node"))
            {
                BaseNodeControl nodeControl;
                switch (node.Attribute("type").Value)
                {
                    case "init":
                        nodeControl = new InitNodeControl() { CornerRadius = new CornerRadius(10) };
                        (nodeControl as InitNodeControl).InitPath = node.Element("initpath").Value;
                        (nodeControl as NodeControl).NodeName = "Инициализация";
                        (nodeControl as NodeControl).SettingsButtonClick += initNode_SettingsButtonClick;
                        break;
                    case "runprocess":
                        nodeControl = new RunProcessNodeControl() { CornerRadius = new CornerRadius(10) };
                        (nodeControl as RunProcessNodeControl).ProcessName = node.Element("processname").Value;
                        (nodeControl as NodeControl).NodeName = "Запуск приложения";
                        (nodeControl as NodeControl).SettingsButtonClick += exportNode_SettingsButtonClick;
                        break;
                    case "algorithm":
                        nodeControl = new AlgorithmNodeControl();
                        (nodeControl as AlgorithmNodeControl).InitPath = node.Element("initpath").Value;
                        (nodeControl as AlgorithmNodeControl).AlgorithmType = HydrologyCore.AlgorithmTypes[node.Element("algorithmtype").Value];
                        (nodeControl as AlgorithmNodeControl).VarLoop = new Dictionary<string, LoopControl>();

                        var varNames = new Dictionary<string, LoopControl>();
                        LoopControl p = loop;
                        while (p != null)
                        {
                            varNames.Add(p.VarName, p);
                            p = p.PreviousLoop;
                        }

                        DataTable paramTable = new DataTable();
                        paramTable.Columns.Add("Name");
                        paramTable.Columns.Add("Value");
                        foreach (XElement param in node.Element("params").Elements("param"))
                        {
                            DataRow row = paramTable.NewRow();
                            row["Name"] = param.Attribute("name").Value;
                            row["Value"] = param.Attribute("value").Value;
                            paramTable.Rows.Add(row);

                            if (row["Value"].ToString()[0] == '{')
                            {
                                string varName = row["Value"].ToString();
                                varName = varName.Trim(' ', '{', '}');
                                if (varNames.Keys.Contains(varName))
                                {
                                    (nodeControl as AlgorithmNodeControl).VarLoop.Add(row["Name"].ToString(), varNames[varName]);
                                }
                            }
                        }
                        (nodeControl as AlgorithmNodeControl).ParamsTable = paramTable;
                        var attr = (nodeControl as AlgorithmNodeControl).AlgorithmType.GetCustomAttribute(typeof(NameAttribute)) as NameAttribute;
                        (nodeControl as NodeControl).NodeName = attr.Name;
                        (nodeControl as NodeControl).SettingsButtonClick += node_SettingsClicked;
                        break;
                    case "loopstart":
                        nodeControl = new StartLoopNodeControl() { Width = 30, Height = 30 };
                        break;
                    case "loop":
                        nodeControl = new LoopControl(this) { PreviousLoop = loop };
                        (nodeControl as LoopControl).VarName = node.Element("loopparams").Attribute("varname").Value;
                        (nodeControl as LoopControl).StartValue = 
                            double.Parse(node.Element("loopparams").Attribute("start").Value, CultureInfo.InvariantCulture);
                        (nodeControl as LoopControl).EndValue = 
                            double.Parse(node.Element("loopparams").Attribute("end").Value, CultureInfo.InvariantCulture);
                        (nodeControl as LoopControl).Step = 
                            double.Parse(node.Element("loopparams").Attribute("step").Value, CultureInfo.InvariantCulture);
                        LoadExperimentGraph(node.Element("loopbody"), nodeControl as LoopControl);
                        break;
                    default:
                        nodeControl = new NodeControl();
                        break;
                }
                if (!(nodeControl is StartLoopNodeControl))
                    nodeControl.Style = (Style)this.FindResource("NodeStyle");
                nodeControl.MouseLeftButtonDown += node_MouseLeftButtonDown;
                nodeControl.MouseLeftButtonUp += node_MouseLeftButtonUp;
                nodeControl.MouseMove += node_MouseMove;
                nodeControl.LostMouseCapture += node_LostMouseCapture;
                
                Canvas.SetLeft(nodeControl, double.Parse(node.Attribute("x").Value, CultureInfo.InvariantCulture));
                Canvas.SetTop(nodeControl, double.Parse(node.Attribute("y").Value, CultureInfo.InvariantCulture));

                if (loop == null)
                {
                    Canvas.Children.Add(nodeControl);
                    experimentGraph.Nodes.Add(nodeControl);
                }
                else
                {
                    loop.Canvas.Children.Add(nodeControl);
                    loop.LoopBody.Nodes.Add(nodeControl);
                }
            }

            if (loop == null)
            {
                Canvas.UpdateLayout();
            }
            else
            {
                loop.Canvas.UpdateLayout();
            }

            foreach (XElement arrow in root.Element("arrows").Elements("arrow"))
            {
                Arrow arr;
                if (loop == null)
                {
                    arr = new Arrow(
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
                    Canvas.UpdateLayout();
                }
                else
                {
                    arr = new Arrow(
                    loop.LoopBody.Nodes[int.Parse(arrow.Element("from").Attribute("id").Value)],
                    new Point(double.Parse(arrow.Element("from").Attribute("x").Value, CultureInfo.InvariantCulture),
                              double.Parse(arrow.Element("from").Attribute("y").Value, CultureInfo.InvariantCulture)),
                    loop.LoopBody.Nodes[int.Parse(arrow.Element("to").Attribute("id").Value)],
                    new Point(double.Parse(arrow.Element("to").Attribute("x").Value, CultureInfo.InvariantCulture),
                              double.Parse(arrow.Element("to").Attribute("y").Value, CultureInfo.InvariantCulture))
                    );

                    arr.MouseDown += Arrow_MouseDown;
                    arr.MouseMove += loop.Arrow_MouseMove;
                    arr.MouseUp += loop.Arrow_MouseUp;

                    loop.Canvas.Children.Add(arr);
                    loop.LoopBody.Arrows.Add(arr);
                    loop.Canvas.UpdateLayout();
                }

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

        private LoopControl viewedLoop = null;
        public LoopControl ViewedLoop
        {
            get { return viewedLoop; }
            set
            {
                viewedLoop = value;
                if (viewedLoop != null)
                {
                    LoopView.Visibility = Visibility.Visible;
                    LoopViewGrid.Children.Clear();
                    LoopViewGrid.Children.Add(viewedLoop.Canvas);
                    viewedLoop.Canvas.UpdateLayout();
                    foreach (var child in viewedLoop.Canvas.Children)
                        if (child is Arrow)
                            (child as Arrow).Draw();
                }
                else
                {
                    LoopView.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CloseLoopButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewedLoop.PreviousLoop != null)
            {
                ViewedLoop = ViewedLoop.PreviousLoop;
            }
            else
            {
                ViewedLoop = null;                
            }
        }
    }
}

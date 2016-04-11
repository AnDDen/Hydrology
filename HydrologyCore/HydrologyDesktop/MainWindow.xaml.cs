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
using HydrologyCore;
using CoreInterfaces;
using CsvParser;
using System.IO;
using System.Data;
using System.Threading;
using System.Reflection;
using HydrologyDesktop.Controls;

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

        private IList<NodeControl> nodes;
        private IList<Arrow> arrows;

        private bool isMove;
        private Vector delta;

        private Point startPoint;
        private bool isDragDrop;

        private EditorMode mode = EditorMode.Pointer;
        private Arrow editingArrow = null;

        private System.Windows.Forms.FolderBrowserDialog folderDialog;
        private Microsoft.Win32.OpenFileDialog fileDialog;

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
            fileDialog = new Microsoft.Win32.OpenFileDialog() { Multiselect = false };

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
            nodes = new List<NodeControl>();
            arrows = new List<Arrow>();
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
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner 
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
            NodeControl node = new AlgorithmNodeControl { NodeName = attr.Name, AlgorithmType = algType, InitPath = initPath, ParamsTable = paramTable };
            node.Style = (Style)this.FindResource("NodeStyle");
            node.SettingsButtonClick += node_SettingsClicked;
            node.MouseLeftButtonDown += node_MouseLeftButtonDown;
            node.MouseLeftButtonUp += node_MouseLeftButtonUp;
            node.MouseMove += node_MouseMove;
            node.LostMouseCapture += node_LostMouseCapture;
            node.MouseEnter += node_MouseEnter;
            node.MouseLeave += node_MouseLeave;

            Canvas.Children.Add(node);
            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
            nodes.Add(node);
        }

        void node_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is NodeControl && mode == EditorMode.Arrow)
            {
                (sender as NodeControl).AttachPointsVisibility = System.Windows.Visibility.Visible;
            }
        }

        void node_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is NodeControl && mode == EditorMode.Arrow)
            {
                (sender as NodeControl).AttachPointsVisibility = System.Windows.Visibility.Hidden;
            }
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
                        if (node.AttachEllipse != null)
                        {
                            Point p = node.AttachPoint;
                            editingArrow = new Arrow(node, p, e.GetPosition(Canvas));
                            Canvas.Children.Add(editingArrow);
                        }
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

                            foreach (Arrow arrow in arrows)
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
                    if (sender is NodeControl)
                    {
                        (sender as NodeControl).AttachPointsVisibility = System.Windows.Visibility.Visible;
                    }
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

            foreach (NodeControl node in nodes) {
                ascendents.Add(node, new List<NodeControl>());
                left.Add(node);
            }
            
            foreach (Arrow arrow in arrows)
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

            if (q.Count == 0)
            {
                // error !!!!
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

            foreach (NodeControl node in nodes)
            {
                if (ascendents[node].Count != 0)
                {
                    // error !!!!
                    break;
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
                        nodes.Add(node);
                    }
                    isDragDrop = false;
                }
                else
                {
                    Canvas.Children.Add(node);
                    Point pos = e.GetPosition(Canvas);
                    Canvas.SetLeft(node, pos.X);
                    Canvas.SetTop(node, pos.Y);
                    isDragDrop = false;
                    nodes.Add(node);
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
            selectControl(e.GetPosition(Canvas));
        }

        private void selectControl(Point pos)
        {
            bool isSelected = false;
            for (int i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Canvas.Children[i];
                if (child is NodeControl)
                {
                    NodeControl node = child as NodeControl;
                    double x = Canvas.GetLeft(node), y = Canvas.GetTop(node);
                    if (pos.X >= x && pos.X <= x + node.Width &&
                        pos.Y >= y && pos.Y <= y + node.Height && !isSelected)
                    {
                        (child as NodeControl).Thickness = new Thickness(3);
                        isSelected = true;
                    }
                    else
                    {
                        (child as NodeControl).Thickness = new Thickness(1);
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
                    node.MouseEnter += node_MouseEnter;
                    node.MouseLeave += node_MouseLeave;

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

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mode == EditorMode.Arrow && editingArrow != null)
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
                        if (arrows.FirstOrDefault((a) => { return a.From == editingArrow.From && a.To == node; }) == null)
                        {
                            if (pos.X >= x - 4 && pos.X <= x + node.Width + 4 && pos.Y >= y - 4 && pos.Y <= y + node.Height + 4)
                            {
                                editingArrow.To = node;
                                if (node.AttachEllipse != null)
                                {
                                    editingArrow.EndRelative = node.AttachPoint;
                                }
                                else
                                {
                                    editingArrow.EndRelative = node.FindAttachPoint(e.GetPosition(node));
                                }
                                arrows.Add(editingArrow);
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
                    node.MouseEnter += node_MouseEnter;
                    node.MouseLeave += node_MouseLeave;

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
    }
}

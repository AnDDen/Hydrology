using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Reflection;

namespace HydrologyDesktop.Controls
{
    /// <summary>
    /// Логика взаимодействия для LoopControl.xaml
    /// </summary>
    public partial class LoopControl : BaseNodeControl
    {
        private string varName;
        private double startValue;
        private double endValue;
        private double step;

        private double runValue;

        private ExperimentGraph loopBody;
        public ExperimentGraph LoopBody
        {
            get { return loopBody; }
            set { loopBody = value; }
        }

        public LoopControl PreviousLoop { get; set; }
        public Canvas Canvas { get; set; }
        public MainWindow Window { get; set; }

        public string VarName
        {
            get
            {
                return varName;
            }

            set
            {
                varName = value;
                VarNameLbl.Content = varName;
            }
        }
        public double StartValue
        {
            get
            {
                return startValue;
            }

            set
            {
                startValue = value;
                StartValueLbl.Content = startValue.ToString();
            }
        }
        public double EndValue
        {
            get
            {
                return endValue;
            }

            set
            {
                endValue = value;
                EndValueLbl.Content = endValue.ToString();
            }
        }
        public double Step
        {
            get
            {
                return step;
            }

            set
            {
                step = value;
                StepLbl.Content = step.ToString();
            }
        }

        public override Thickness Thickness
        {
            get { return border.BorderThickness; }
            set { border.BorderThickness = value; }
        }

        public LoopControl() : this(null) { }

        public LoopControl(MainWindow window) : base()
        {
            InitializeComponent();

            Window = window;
            loopBody = new ExperimentGraph();

            Canvas = new Canvas()
            {
                AllowDrop = true,
                ClipToBounds = true,
                Style = (Style)this.FindResource("CanvasStyle")
            };

            Canvas.Drop += CanvasDrop;
            Canvas.MouseDown += Canvas_MouseDown;
            Canvas.MouseMove += Canvas_MouseMove;
            Canvas.MouseUp += Canvas_MouseUp;

            StartLoopNodeControl startNode = new StartLoopNodeControl() { Height = 30, Width = 30 };
            Canvas.SetTop(startNode, 50);
            Canvas.SetLeft(startNode, 50);

            startNode.MouseLeftButtonDown += Window.node_MouseLeftButtonDown;
            startNode.MouseLeftButtonUp += Window.node_MouseLeftButtonUp;
            startNode.MouseMove += Window.node_MouseMove;
            startNode.LostMouseCapture += Window.node_LostMouseCapture;

            Canvas.Children.Add(startNode);
            loopBody.Nodes.Add(startNode);
        }

        public double ResetValue()
        {
            runValue = startValue;
            return runValue;
        }
        public double StepValue()
        {
            runValue += step;
            return runValue;
        }
        public bool IsLoop()
        {
            return runValue <= endValue;
        }
        public double RunValue { get { return runValue; } }

        private void CanvasDrop(object sender, DragEventArgs e)
        {
            if (Window.IsDragDrop)
            {
                if (e.Data.GetDataPresent("NodeFormat"))
                {
                    BaseNodeControl node = (BaseNodeControl)e.Data.GetData("NodeFormat");
                    if (node is InitNodeControl)
                    {
                        var initSettings = new InitSettingsWindow(Window.FolderDialog)
                        {
                            Owner = Window,
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
                            loopBody.Nodes.Add(node);
                        }
                        Window.IsDragDrop = false;
                    }
                    else if (node is RunProcessNodeControl)
                    {
                        var runProcessSettings = new RunProcessSettingsWindow(Window.FileDialog)
                        {
                            ProcessName = (node as RunProcessNodeControl).ProcessName,
                            Owner = Window,
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
                            loopBody.Nodes.Add(node);
                        }
                        Window.IsDragDrop = false;
                    }
                    else if (node is LoopControl)
                    {
                        var loopSettings = new LoopSettingsWindow()
                        {
                            Owner = Window,
                            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                        };
                        bool? dialogResult = loopSettings.ShowDialog();
                        if (dialogResult.HasValue && dialogResult.Value)
                        {
                            (node as LoopControl).VarName = loopSettings.VarName;
                            (node as LoopControl).StartValue = loopSettings.StartValue;
                            (node as LoopControl).EndValue = loopSettings.EndValue;
                            (node as LoopControl).Step = loopSettings.Step;
                            (node as LoopControl).PreviousLoop = this;
                            (node as LoopControl).Window = Window;

                            Canvas.Children.Add(node);
                            Point pos = e.GetPosition(Canvas);
                            Canvas.SetLeft(node, pos.X);
                            Canvas.SetTop(node, pos.Y);
                            loopBody.Nodes.Add(node);
                        }
                        Window.IsDragDrop = false;
                    }
                }

                if (e.Data.GetDataPresent("NodeNameFormat"))
                {
                    String nodeName = e.Data.GetData("NodeNameFormat").ToString();
                    Point pos = e.GetPosition(Canvas);
                    AddAlgNode(nodeName, pos.X, pos.Y);
                    Window.IsDragDrop = false;
                }
            }
        }

        void AddAlgNode(string algName, double x = 0, double y = 0, LoopControl loop = null)
        {
            Type algType = Window.HydrologyCore.AlgorithmTypes[algName];

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

            var algSettings = new AlgorithmSettings(Window.FolderDialog, paramsTable, algType)
            {
                Owner = Window,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            algSettings.ParentLoop = this;
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
            node.Style = (Style)Window.FindResource("NodeStyle");
            node.SettingsButtonClick += Window.node_SettingsClicked;
            node.MouseLeftButtonDown += Window.node_MouseLeftButtonDown;
            node.MouseLeftButtonUp += Window.node_MouseLeftButtonUp;
            node.MouseMove += Window.node_MouseMove;
            node.LostMouseCapture += Window.node_LostMouseCapture;

            Canvas.Children.Add(node);
            loopBody.Nodes.Add(node);

            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Window.Mode == EditorMode.Arrow && Window.EditingArrow != null)
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
                        if (loopBody.Arrows.FirstOrDefault((a) => { return a.From == Window.EditingArrow.From && a.To == node; }) == null)
                        {
                            if (pos.X >= x - 4 && pos.X <= x + node.ActualWidth + 4 && pos.Y >= y - 4 && pos.Y <= y + node.ActualHeight + 4)
                            {
                                Window.EditingArrow.To = node;
                                if (node.AttachEllipse != null)
                                {
                                    Point p = node.AttachPoint;
                                    p.X = p.X / node.ActualWidth;
                                    p.Y = p.Y / node.ActualHeight;
                                    Window.EditingArrow.EndRelative = p;
                                }
                                else
                                {
                                    Point p = node.FindAttachPoint(e.GetPosition(node));
                                    p.X = p.X / node.ActualWidth;
                                    p.Y = p.Y / node.ActualHeight;
                                    Window.EditingArrow.EndRelative = p;
                                }

                                Window.EditingArrow.MouseDown += Window.Arrow_MouseDown;
                                Window.EditingArrow.MouseMove += Arrow_MouseMove;
                                Window.EditingArrow.MouseUp += Arrow_MouseUp;

                                loopBody.Arrows.Add(Window.EditingArrow);

                                Window.EditingArrow = null;
                                found = true;
                            }
                        }
                    }
                }
                if (!found)
                {
                    Canvas.Children.Remove(Window.EditingArrow);
                    Window.EditingArrow = null;
                }
            }
        }

        public void Arrow_MouseMove(object sender, MouseEventArgs e)
        {
            Arrow arrow = sender as Arrow;
            if (arrow.MoveEllipse != null && Window.EditingArrow == arrow)
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

        public void Arrow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Arrow arrow = sender as Arrow;
            Point p = e.GetPosition(Canvas);

            if (arrow.MoveEllipse != null && Window.EditingArrow == arrow)
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

                Window.EditingArrow = null;
                arrow.pointTo.ReleaseMouseCapture();
                arrow.pointFrom.ReleaseMouseCapture();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Window.Mode == EditorMode.Arrow && Window.EditingArrow != null)
            {
                Window.EditingArrow.End = e.GetPosition(Canvas) - new Vector(0, 2);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window.Selected = null;
            for (int i = Canvas.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Canvas.Children[i];
                if (child is BaseNodeControl)
                {
                    BaseNodeControl node = child as BaseNodeControl;
                    if (node.IsMouseOver)
                    {
                        node.Thickness = new Thickness(3);
                        Window.Selected = child;
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
                        Window.Selected = arrow;
                    }
                    else
                    {
                        arrow.AttachPointsVisibility = Visibility.Hidden;
                    }
                }
            }
        }

        public event EventHandler<EventArgs> SettingsButtonClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsButtonClick != null)
                SettingsButtonClick.Invoke(this, e);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Window.ViewedLoop = this;
        }

        private Ellipse attachEllipse = null;

        public override Ellipse AttachEllipse
        {
            get { return attachEllipse; }
            set { attachEllipse = value; }
        }

        public override Point AttachPoint
        {
            get
            {
                if (attachEllipse != null)
                {
                    Point pos = attachEllipse.TransformToAncestor(this).Transform(new Point(0, 0));
                    pos += new Vector(4, 4);
                    return pos;
                }
                return new Point(0, 0);
            }
        }

        public override Point FindAttachPoint(Point p)
        {
            Point attach = new Point(0, 0);
            double min = 0;
            bool first = true;
            foreach (UIElement element in attachPoints.Children)
            {
                if (element is Ellipse)
                {
                    Ellipse ellipse = element as Ellipse;
                    Point pos = ellipse.TransformToAncestor(this).Transform(new Point(0, 0));
                    pos += new Vector(4, 4);
                    var delta = pos - p;
                    if (first || delta.Length <= min)
                    {
                        min = delta.Length;
                        attach = pos;
                        first = false;
                    }
                }
            }
            return attach;
        }

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            attachEllipse = ellipse;
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(244, 244, 245));
            attachEllipse = null;
        }

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(112, 112, 112));
            attachEllipse = ellipse;
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(244, 244, 245));
            attachEllipse = null;
        }
    }
}

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
        private const string IN_PORT_STYLE_NAME = "InPortPathStyle";
        private const string OUT_PORT_STYLE_NAME = "OutPortPathStyle";

        public event EventHandler<EventArgs> ButtonClick;

        public Visibility ShowEditButton { get; set; }

        public IRunable Node { get; }

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

        public IDictionary<Port, Path> InPorts { get; } = new Dictionary<Port, Path>();
        public IDictionary<Port, Path> OutPorts { get; } = new Dictionary<Port, Path>();

        public string NodeType
        {
            get
            {
                if (Node is AlgorithmNode)
                {
                    return (Node as AlgorithmNode).DisplayedTypeName;
                }
                else if (Node is LoopBlock)
                {
                    return UIConsts.LOOP_NODE_NAME;
                }
                else if (Node is Block)
                {
                    return UIConsts.BLOCK_NODE_NAME;
                }
                else if (Node is InitNode)
                {
                    return UIConsts.INIT_NODE_NAME;
                }
                else if (Node is InPortNode)
                {
                    if (Node is LoopPortNode)
                        return UIConsts.LOOP_PORT_NODE_NAME;
                    return UIConsts.IN_PORT_NODE_NAME;
                }
                else if (Node is OutPortNode)
                {
                    return UIConsts.OUT_PORT_NODE_NAME;
                }
                return "";
            }
        }

        public NodeControl(IRunable node)
        {
            Node = node;            
            InitializeComponent();
            NameLabel.Content = node.Name;
            Node.NameChanged += (sender, e) =>
            {
                NameLabel.Content = e.Name;
            };
            LoadPorts();
        }

        public void LoadPorts()
        {
            LoadInPorts();
            LoadOutPorts();
        }

        public StackPanel CreateToolTip(Port p)
        {
            StackPanel toolTip = new StackPanel();
            // name
            toolTip.Children.Add(new Label() {
                Content = p.DisplayedName,
                FontWeight = FontWeights.Bold
            });
            // datatype
            toolTip.Children.Add(new Label() {
                Content = UIConsts.GetTypeName(p.DataType, p.ElementType),
                FontStyle = FontStyles.Italic,
                FontSize = 10 });
            // description
            if (!string.IsNullOrWhiteSpace(p.Description))
                toolTip.Children.Add(new Label() {
                    Content = p.Description,
                    FontSize = 10
                });
            return toolTip;
        }

        public void LoadInPorts()
        {
            InPortsControl.Children.Clear();
            foreach (Port p in Node.InPorts)
            {
                if (!InPorts.ContainsKey(p))
                    InPorts[p] = new Path() { Style = this.FindResource(IN_PORT_STYLE_NAME) as Style };

                InPorts[p].ToolTip = CreateToolTip(p);

                if (!p.Displayed)
                    InPorts[p].Visibility = Visibility.Collapsed;
                else
                    InPorts[p].Visibility = Visibility.Visible;

                InPortsControl.Children.Add(InPorts[p]);
            }
        }

        public void LoadOutPorts()
        {
            OutPortsControl.Children.Clear();
            foreach (Port p in Node.OutPorts)
            {
                if (!OutPorts.ContainsKey(p))
                    OutPorts[p] = new Path() { Style = this.FindResource(OUT_PORT_STYLE_NAME) as Style };

                OutPorts[p].ToolTip = CreateToolTip(p);

                if (!p.Displayed)
                    OutPorts[p].Visibility = Visibility.Collapsed;
                else 
                    OutPorts[p].Visibility = Visibility.Visible;

                OutPortsControl.Children.Add(OutPorts[p]);
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonClick != null)
                ButtonClick.Invoke(this, e);
        }

        public Tuple<Port, Point> FindInPort(Point point)
        {
            Port p = FindPort(point, InPorts);
            if (p != null)
            {
                return new Tuple<Port, Point>(p, GetInPortPoint(p));
            }
            return null;
        }

        public Point GetInPortPoint(Port port)
        {
            Path path = InPorts[port];
            var pos = GetPosition(path);
            Point start = new Point(pos.X, pos.Y + path.ActualHeight / 2);
            return start;
        }

        public Tuple<Port, Point> FindOutPort(Point point)
        {
            Port p = FindPort(point, OutPorts);
            if (p != null)
            {
                return new Tuple<Port, Point>(p, GetOutPortPoint(p));
            }
            return null;
        }

        public Point GetOutPortPoint(Port port)
        {
            Path path = OutPorts[port];
            var pos = GetPosition(path);
            Point end = new Point(pos.X + path.ActualWidth, pos.Y + path.ActualHeight / 2);
            return end;
        }

        private Port FindPort(Point point, IDictionary<Port, Path> ports)
        {
            return ports.Keys.FirstOrDefault(p =>
            {
                var path = ports[p];
                var pos = GetPosition(path);
                double x = pos.X;
                double y = pos.Y;
                double w = path.ActualWidth;
                double h = path.ActualHeight;                

                return (point.X >= x && point.X <= x + w) && (point.Y >= y && point.Y <= y + h);
            });
        }

        private Point GetPosition(Visual element)
        {
            var positionTransform = element.TransformToAncestor(this);
            var areaPosition = positionTransform.Transform(new Point(0, 0));

            return areaPosition;
        }

        public bool IsNodeMouseOver(Point p)
        {
            return p.X >= Canvas.GetLeft(this) && p.X <= Canvas.GetLeft(this) + ActualWidth
                && p.Y >= Canvas.GetTop(this) && p.Y <= Canvas.GetTop(this) + ActualHeight;
        }
    }
}

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

namespace HydrologyDesktop.Controls
{
    /// <summary>
    /// Логика взаимодействия для Arrow.xaml
    /// </summary>
    public partial class Arrow : UserControl
    {
        private Point start, end, startRelative, endRelative;
        private double length = 100, angle = 0;

        public NodeControl From { get; set; }
        public NodeControl To { get; set; }

        public Point StartRelative
        {
            get { return startRelative; }
            set { 
                startRelative = value;
                start = new Point(Canvas.GetLeft(From) + startRelative.X, Canvas.GetTop(From) + startRelative.Y);
                Draw(); 
            }
        }

        public Point EndRelative
        {
            get { return endRelative; }
            set { 
                endRelative = value;
                end = new Point(Canvas.GetLeft(To) + endRelative.X, Canvas.GetTop(To) + endRelative.Y);
                Draw();
            }
        }

        public Point Start
        {
            get { return start; }
            set { start = value; Draw(); }
        }

        public Point End
        {
            get { return end; }
            set { end = value; Draw(); }
        }

        public void Draw()
        {
            if (From != null)
            {
                start = new Point(Canvas.GetLeft(From) + startRelative.X, Canvas.GetTop(From) + startRelative.Y);
            }

            if (To != null)
            {
                end = new Point(Canvas.GetLeft(To) + endRelative.X, Canvas.GetTop(To) + endRelative.Y);
            }

            Canvas.SetLeft(this, start.X);
            Canvas.SetTop(this, start.Y);

            length = Math.Sqrt((end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y));
            angle = Math.Sign(end.Y - start.Y) * Math.Acos((end.X - start.X) / length) * 180 / Math.PI;

            ArrowLine.X2 = length;
            ArrowCap.Data = Geometry.Parse(string.Format("M {0},-5 L {1},0 L {2},5", (length - 15).ToString().Replace(",", "."),
                length.ToString().Replace(",", "."), (length - 15).ToString().Replace(",", ".") ));
            RotateTransform transform = ArrowCanvas.RenderTransform as RotateTransform;
            if (transform == null)
            {
                transform = new RotateTransform();
                ArrowCanvas.RenderTransform = transform;
            }
            transform.Angle = angle;
        }

        public Arrow(NodeControl from, Point startRelative, NodeControl to, Point endRelative)
        {
            InitializeComponent();

            From = from;
            To = to;
            StartRelative = startRelative;
            EndRelative = endRelative;
        }

        public Arrow(NodeControl from, Point startRelative, Point end)
        {
            InitializeComponent();

            From = from;
            StartRelative = startRelative;

            End = end;
        }
    }
}

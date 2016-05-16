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

        public BaseNodeControl From { get; set; }
        public BaseNodeControl To { get; set; }

        public Point StartRelative
        {
            get { return startRelative; }
            set { 
                startRelative = value;
                start = new Point(Canvas.GetLeft(From) + startRelative.X * From.ActualWidth, Canvas.GetTop(From) + startRelative.Y * From.ActualHeight);
                Draw(); 
            }
        }

        public Point EndRelative
        {
            get { return endRelative; }
            set { 
                endRelative = value;
                end = new Point(Canvas.GetLeft(To) + endRelative.X * To.ActualWidth, Canvas.GetTop(To) + endRelative.Y * To.ActualHeight);
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

        public Visibility AttachPointsVisibility
        {
            get { return attachPoints.Visibility; }
            set { attachPoints.Visibility = value; }
        }

        private Ellipse attachEllipse = null;
        public Ellipse AttachEllipse
        {
            get { return attachEllipse; }
            set { attachEllipse = value; }
        }

        public Ellipse MoveEllipse { get; set; }

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

        public void Draw()
        {
            if (From != null)
            {
                start = new Point(Canvas.GetLeft(From) + startRelative.X * From.ActualWidth, Canvas.GetTop(From) + startRelative.Y * From.ActualHeight);
            }

            if (To != null)
            {
                end = new Point(Canvas.GetLeft(To) + endRelative.X * To.ActualWidth, Canvas.GetTop(To) + endRelative.Y * To.ActualHeight);
            }

            Canvas.SetLeft(this, start.X);
            Canvas.SetTop(this, start.Y);

            length = Math.Sqrt((end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y));
            angle = Math.Sign(end.Y - start.Y) * Math.Acos((end.X - start.X) / length) * 180 / Math.PI;

            ArrowLine.X2 = length;
            HiddenLine.X2 = length;
            pointTo.Margin = new Thickness(length - 4, -4, -length + 4, 4);
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

        public Arrow(BaseNodeControl from, Point startRelative, BaseNodeControl to, Point endRelative)
        {
            InitializeComponent();

            From = from;
            To = to;
            StartRelative = startRelative;
            EndRelative = endRelative;
        }

        public Arrow(BaseNodeControl from, Point startRelative, Point end)
        {
            InitializeComponent();

            From = from;
            StartRelative = startRelative;

            End = end;
        }
    }
}

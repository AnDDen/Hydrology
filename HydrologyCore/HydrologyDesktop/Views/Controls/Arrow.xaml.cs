using HydrologyCore.Experiment;
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

namespace HydrologyDesktop.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для Arrow.xaml
    /// </summary>
    public partial class Arrow : UserControl
    {
        private double x1, y1, x2, y2;

        public Port From { get; set; }
        public Port To { get; set; }

        public double X1
        {
            get { return x1; }
            set
            {
                x1 = value;
                Draw();
            }
        }

        public double Y1
        {
            get { return y1; }
            set
            {
                y1 = value;
                Draw();
            }
        }

        public double X2
        {
            get { return x2; }
            set
            {
                x2 = value;
                Draw();
            }
        }

        public double Y2
        {
            get { return y2; }
            set
            {
                y2 = value;
                Draw();
            }
        }

        public Point P1
        {
            get { return new Point(x1, y1); }
            set
            {
                x1 = value.X;
                y1 = value.Y;
                Draw();
            }
        }

        public Point P2
        {
            get { return new Point(x2, y2); }
            set
            {
                x2 = value.X;
                y2 = value.Y;
                Draw();
            }
        }

        public event EventHandler<MouseButtonEventArgs> ArrowCapMouseDown;

        private void Cap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ArrowCapMouseDown != null)
                ArrowCapMouseDown.Invoke(this, e);
        }

        public Arrow(double x1, double y1, double x2, double y2)
        {
            InitializeComponent();
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            Draw();
        }

        public Arrow(double x, double y) : this(x, y, x, y) { }

        public void Draw()
        {
            Canvas.SetLeft(this, x1);
            Canvas.SetTop(this, y1);            

            double l = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            double l_lenght = l - 20;
            double alpha = Math.Atan((y2 - y1) / (x2 - x1)) * 180 / Math.PI;

            if (x2 < x1)
                alpha = 180 + alpha;

            Grid.Width = l;
            Line.X2 = l_lenght;

            RotateTransform rotate = new RotateTransform();
            rotate.Angle = alpha;
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(rotate);
            Grid.RenderTransform = transform;
        }
    }
}

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
    /// Логика взаимодействия для NodeControl.xaml
    /// </summary>
    public partial class NodeControl : UserControl
    {
        private string nodeName = "";
        public string NodeName
        {
            get
            {
                return nodeName;
            }
            set
            {
                nodeName = value;
                nodeNameLbl.Content = nodeName;
            }
        }

        private Ellipse attachEllipse = null;

        public Ellipse AttachEllipse
        {
            get { return attachEllipse; }
            set { attachEllipse = value; }
        }

        public Point AttachPoint
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

        public Point FindAttachPoint(Point p)
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

        public Brush BorderColor
        {
            get { return border.BorderBrush; }
            set { border.BorderBrush = value; }
        }

        public Thickness Thickness
        {
            get { return border.BorderThickness; }
            set { border.BorderThickness = value; }
        }

        public CornerRadius CornerRadius
        {
            get { return border.CornerRadius; }
            set { border.CornerRadius = value; }
        }

        public event EventHandler<EventArgs> SettingsButtonClick;

        public NodeControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SettingsButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            ellipse.Fill = new SolidColorBrush(Color.FromRgb(112,112,112));
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

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

namespace HydrologyDesktopControls
{
    /// <summary>
    /// Логика взаимодействия для ArrowControl.xaml
    /// </summary>
    public partial class ArrowControl : UserControl
    {
        private double length = 100, angle = 0;

        public double Length
        {
            get { return length; }
            set { length = value; Draw(); }
        }
        public double Angle
        {
            get { return angle; }
            set { angle = value; Draw(); }
        }

        public void Draw()
        {
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

        public ArrowControl()
        {
            InitializeComponent();
            Draw();
        }
    }
}

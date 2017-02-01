using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace HydrologyDesktop.Controls
{
    public class BaseNodeControl : UserControl
    {
        public AbstractNode Node { get; set; }

        public virtual Thickness Thickness { get; set; }

        public virtual Point FindAttachPoint(Point pos) { return pos; }

        public virtual Ellipse AttachEllipse { get; set; }
        public virtual Point AttachPoint { get; set; }

        public BaseNodeControl()
        {
        }
    }
}

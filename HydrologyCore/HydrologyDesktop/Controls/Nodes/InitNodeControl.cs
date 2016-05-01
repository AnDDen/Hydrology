using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop.Controls
{
    public class InitNodeControl : NodeControl
    {
        public string InitPath { get; set; }

        public InitNodeControl() : base()
        {
            paramsExpander.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}

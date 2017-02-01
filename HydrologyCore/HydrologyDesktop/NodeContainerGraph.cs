using HydrologyCore.Experiment;
using HydrologyDesktop.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop
{
    public class NodeContainerGraph
    {
        public NodeContainerGraph Parent { get; set; }

        public NodeContainer NodeContainer { get; set; }

        private IList<NodeControl> nodeControls;

        public NodeContainerGraph()
        {
            
        }
    }
}

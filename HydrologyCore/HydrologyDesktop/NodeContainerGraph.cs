using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using HydrologyDesktop.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace HydrologyDesktop
{
    public class NodeContainerGraph
    {
        public NodeContainer NodeContainer { get; set; }

        public IList<NodeControl> NodeControls { get; set; }

        public NodeContainerGraph Parent { get; set; }

        public NodeContainerGraph(NodeContainer nodeContainer, NodeContainerGraph parent)
        {
            NodeContainer = nodeContainer;
            NodeControls = new List<NodeControl>();
        }

        public void AddNode(NodeControl node)
        {
            NodeControls.Add(node);
            NodeContainer.AddNode(node.Node);
        }

        public void RemoveNode(NodeControl node)
        {
            NodeControls.Remove(node);
            NodeContainer.RemoveNode(node.Node);
        }

        public void DisplayOnCanvas(Canvas canvas)
        {
            canvas.Children.Clear();
            foreach (var nodeControl in NodeControls)
                canvas.Children.Add(nodeControl);
        }

        public XElement ToXml(IDictionary<LoopNode, NodeContainerGraph> nodeContainerGraphs)
        {
            XElement elem = new XElement("nodes");
            foreach (NodeControl node in NodeControls)
            {               
                XElement nodeElem = node.Node.ToXml();

                if (node.Node is LoopNode)
                {
                    nodeElem.Add(new XElement("loopbody", 
                        nodeContainerGraphs[node.Node as LoopNode].ToXml(nodeContainerGraphs)));
                }
                elem.Add(new XElement("node",
                    new XAttribute("x", Canvas.GetLeft(node)),
                    new XAttribute("y", Canvas.GetTop(node)),
                    nodeElem
                ));
            }
            return elem;
        }
    }
}

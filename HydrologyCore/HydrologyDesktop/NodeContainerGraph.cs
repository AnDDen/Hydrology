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
        public Block Block { get; set; }

        public IList<NodeControl> NodeControls { get; } = new List<NodeControl>();

        public IList<Arrow> Arrows { get; } = new List<Arrow>();

        public NodeContainerGraph Parent { get; set; }

        public NodeContainerGraph(Block block, NodeContainerGraph parent)
        {
            Block = block;
            Parent = parent;
        }

        public void AddNode(NodeControl node)
        {
            NodeControls.Add(node);
            Block.AddNode(node.Node);
        }

        public void RemoveNode(NodeControl node)
        {
            NodeControls.Remove(node);
            Block.RemoveNode(node.Node);
        }

        public void AddArrow(Arrow arrow)
        {
            Arrows.Add(arrow);
        }

        public void AddArrowToModel(Arrow arrow)
        {
            Block.Connections.Add(new Connection(arrow.From, arrow.To));
        }

        public void RemoveArrowFromModel(Arrow arrow)
        {
            Block.Connections.Remove(Block.Connections.SingleOrDefault(c => c.From == arrow.From && c.To == arrow.To));
        }

        public void RemoveArrow(Arrow arrow)
        {
            Arrows.Remove(arrow);
        }

        public void DisplayOnCanvas(Canvas canvas)
        {
            canvas.Children.Clear();
            foreach (var nodeControl in NodeControls)
            {
                canvas.Children.Add(nodeControl);
            }
            foreach (var arrow in Arrows)
            {
                canvas.Children.Add(arrow);
            }
        }

        public XElement ToXml(IDictionary<LoopBlock, NodeContainerGraph> nodeContainerGraphs)
        {
            XElement elem = new XElement("nodes");
            /* foreach (NodeControl node in NodeControls)
            {               
                XElement nodeElem = node.Node.ToXml();

                if (node.Node is LoopBlock)
                {
                    nodeElem.Add(new XElement("loopbody", 
                        nodeContainerGraphs[node.Node as LoopBlock].ToXml(nodeContainerGraphs)));
                }
                elem.Add(new XElement("node",
                    new XAttribute("x", Canvas.GetLeft(node)),
                    new XAttribute("y", Canvas.GetTop(node)),
                    nodeElem
                ));
            } */
            return elem;
        }
    }
}

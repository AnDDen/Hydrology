using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using HydrologyDesktop.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;

namespace HydrologyDesktop.XmlHelpers
{
    public class XmlSerializer
    {
        private IDictionary<Block, NodeContainerGraph> blockGraphs;
        private Experiment experiment;

        public XmlSerializer(IDictionary<Block, NodeContainerGraph> blockGraphs, Experiment experiment)
        {
            this.blockGraphs = blockGraphs;
            this.experiment = experiment;
        }

        public XDocument Serialize()
        {
            XElement expNode = new XElement("experiment", 
                new XAttribute("path", experiment.Path ?? ""),
                new XAttribute("dirName", experiment.Name ?? ""),
                SerializeBlock(experiment.Block));
            return new XDocument(expNode);
        }

        private XElement SerializeBlock(Block blockNode)
        {
            NodeContainerGraph nodeContainer = blockGraphs[blockNode];
            XElement block = new XElement("block");
            XElement nodes = new XElement("nodes");
            block.Add(nodes);
            foreach (var node in nodeContainer.NodeControls)
                nodes.Add(SerializeNode(node));
            XElement arrows = new XElement("arrows");
            block.Add(arrows);
            foreach (var arrow in nodeContainer.Arrows)
                arrows.Add(SerializeArrow(arrow));
            return block;
        }

        private XElement SerializeNode(NodeControl node)
        {
            IRunable modelNode = node.Node;
            string name = modelNode.Name;
            Type type = modelNode.GetType();
            Type algType = null;
            if (modelNode is AlgorithmNode)
                algType = (modelNode as AlgorithmNode).AlgorithmType;
            double x = Canvas.GetLeft(node);
            double y = Canvas.GetTop(node);

            XElement element = new XElement("node",
                new XAttribute("name", name),
                algType == null ? new XAttribute("type", type.AssemblyQualifiedName) : new XAttribute("algType", algType.AssemblyQualifiedName),
                new XAttribute("x", x),
                new XAttribute("y", y)
            );

            if (modelNode is Block)
            {
                element.Add(SerializeBlock(modelNode as Block));
            }
            else
            {
                if (modelNode is InitNode)
                    element.Add(SerializeInitNode(modelNode as InitNode));
                else if (modelNode is PortNode)
                    element.Add(SerializePortNode(modelNode as PortNode));
                else if (modelNode is AlgorithmNode)
                    element.Add(SerializeAlgorithmNode(modelNode as AlgorithmNode));
            }
            return element;
        }

        private XElement SerializeInitNode(InitNode node)
        {
            XElement settings = new XElement("settings");
            XElement files = new XElement("files");
            settings.Add(files);
            foreach (var file in node.Files) {
                files.Add(new XElement("file",
                    new XAttribute("path", file.Key),
                    new XAttribute("name", file.Value)
                ));
            }
            return settings;
        }

        private XElement SerializePortNode(PortNode node)
        {
            XElement settings = new XElement("settings", 
                new XElement("description", node.Description),
                new XElement("dataType", node.DataType),
                new XElement("elementType", node.ElementType?.AssemblyQualifiedName)
            );
            return settings;
        }

        private XElement SerializeAlgorithmNode(AlgorithmNode node)
        {
            XElement settings = new XElement("settings");
            XElement inputs = new XElement("inputs");
            settings.Add(inputs);
            foreach (var param in node.ValueParams)
            {
                inputs.Add(new XElement("input",
                    new XAttribute("name", param.Key.Name),
                    new XAttribute("value", param.Value),
                    new XAttribute("isRef", param.Key.Displayed))
                );
            }
            XElement outputs = new XElement("outputs");
            settings.Add(outputs);
            foreach (var param in node.SaveToFile)
            {
                outputs.Add(new XElement("output",
                    new XAttribute("name", param.Key.Name),
                    new XAttribute("saveToFile", param.Value))
                );
            }
            return settings;
        }

        private XElement SerializeArrow(Arrow arrow)
        {
            return new XElement("arrow",
                new XAttribute("x1", arrow.X1),
                new XAttribute("y1", arrow.Y1),
                new XAttribute("x2", arrow.X2),
                new XAttribute("y2", arrow.Y2)
            );
        }
    } 
}

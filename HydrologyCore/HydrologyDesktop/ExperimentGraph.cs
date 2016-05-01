using HydrologyDesktop.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using HydrologyCore;

namespace HydrologyDesktop
{
    public class ExperimentGraph
    {
        public IList<NodeControl> Nodes { get; set; }
        public IList<Arrow> Arrows { get; set; }

        public ExperimentGraph()
        {
            Nodes = new List<NodeControl>();
            Arrows = new List<Arrow>();
        }

        public XDocument ToXml()
        {
            XDocument xDocument = new XDocument();

            // root element
            XElement experiment = new XElement("experiment");

            // <nodes> ... </nodes>
            XElement nodes = new XElement("nodes");
            for (int i = 0; i < Nodes.Count; i++)
            {
                // <node type = "..." id = "..." x = "..." y = "..."> ... </node>
                XElement node = new XElement("node");

                string nodeType = "";
                if (Nodes[i] is InitNodeControl)
                    nodeType = "init";
                else if (Nodes[i] is AlgorithmNodeControl)
                    nodeType = "algorithm";
                else if (Nodes[i] is RunProcessNodeControl)
                    nodeType = "runprocess";

                node.Add(new XAttribute("type", nodeType));
                node.Add(new XAttribute("id", i));
                node.Add(new XAttribute("x", Canvas.GetLeft(Nodes[i])));
                node.Add(new XAttribute("y", Canvas.GetTop(Nodes[i])));

                switch (nodeType)
                {
                    case "init":
                        node.Add(new XElement("initpath", (Nodes[i] as InitNodeControl).InitPath));
                        break;
                    case "runprocess":
                        node.Add(new XElement("processname", (Nodes[i] as RunProcessNodeControl).ProcessName));
                        break;
                    case "algorithm":
                        {
                            node.Add(new XElement("initpath", (Nodes[i] as AlgorithmNodeControl).InitPath));
                            node.Add(new XElement("algorithmtype", (Nodes[i] as AlgorithmNodeControl).AlgorithmType.Name));
                            XElement ps = new XElement("params");
                            foreach (DataRow row in (Nodes[i] as AlgorithmNodeControl).ParamsTable.Rows)
                            {
                                ps.Add(new XElement("param",
                                    new XAttribute("name", row["Name"]),
                                    new XAttribute("value", row["Value"])));
                            }
                            node.Add(ps);
                            break;
                        }
                }

                nodes.Add(node);
            }

            // <arrows> ... </arrows>
            XElement arrows = new XElement("arrows");
            for (int i = 0; i < Arrows.Count; i++)
            {
                // <arrow> <from id="" x="" y="" /> <to id="" x="" y="" /> </arrow>
                arrows.Add(new XElement("arrow",
                    new XElement("from",
                        new XAttribute("id", Nodes.IndexOf(Arrows[i].From)),
                        new XAttribute("x", Arrows[i].StartRelative.X),
                        new XAttribute("y", Arrows[i].StartRelative.Y)
                    ), 
                    new XElement("to",
                        new XAttribute("id", Nodes.IndexOf(Arrows[i].To)),
                        new XAttribute("x", Arrows[i].EndRelative.X),
                        new XAttribute("y", Arrows[i].EndRelative.Y)
                    )
                ));
            }

            experiment.Add(nodes);
            experiment.Add(arrows);
            xDocument.Add(experiment);
            return xDocument;
        }
    }
}

using HydrologyCore;
using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HydrologyDesktop.XmlHelpers
{
    public class XmlSettingsDeserializer
    {
        public static void DeserializeSettings(IRunable node, XElement settings)
        {
            if (node is InitNode)
                DeserializeInitNodeSettings(node as InitNode, settings);
            else if (node is PortNode)
                DeserializePortNodeSettings(node as PortNode, settings);
            else if (node is AlgorithmNode)
                DeserializeAlgorithmNodeSettings(node as AlgorithmNode, settings);
        }

        private static void DeserializeInitNodeSettings(InitNode node, XElement settings)
        {
            node.ClearFiles();
            foreach (var element in settings.Element("files").Elements("file"))
            {
                node.AddFile(element.Attribute("path").Value, element.Attribute("name").Value);
            }
        }

        private static void DeserializePortNodeSettings(PortNode node, XElement settings)
        {
            node.Description = settings.Element("description").Value;
            node.DataType = (DataType) Enum.Parse(typeof(DataType), settings.Element("dataType").Value);
            node.ElementType = Type.GetType(settings.Element("elementType").Value);
        }

        private static void DeserializeAlgorithmNodeSettings(AlgorithmNode node, XElement settings)
        {
            foreach (var input in settings.Element("inputs").Elements("input"))
            {
                Port port = node.ValueParams.Keys.FirstOrDefault(p => p.Name == input.Attribute("name").Value);
                port.Displayed = Convert.ToBoolean(input.Attribute("isRef").Value);
                if (!port.Displayed)
                    node.SetPortValue(port, input.Attribute("value").Value);
            }

            foreach (var output in settings.Element("outputs").Elements("output"))
            {
                Port port = node.OutPorts.FirstOrDefault(p => p.Name == output.Attribute("name").Value);
                node.SetSaveToFile(port, Convert.ToBoolean(output.Attribute("saveToFile").Value));
            }
        }
    }
}

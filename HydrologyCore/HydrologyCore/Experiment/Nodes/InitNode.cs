using CoreInterfaces;
using CsvParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Linq;

namespace HydrologyCore.Experiment.Nodes
{
    public class InitNode : AbstractNode
    {
        // key = filePath, value = var name
        private Dictionary<string, string> files = new Dictionary<string, string>();
        public IDictionary<string, string> Files => files;

        public InitNode(string name, Block parent) : base(name, parent) { }

        public override void Run(Context ctx)
        {
            foreach (var file in files.Keys)
            {
                IReader reader = new CSVParser();
                DataTable table = reader.Read(file);
                table.TableName = files[file];
                // put in context
                Port port = outPorts.Find(p => p.Name == files[file]);
                ctx.AddPortValue(port, table);
            }
        }

        public void AddFile(string name, string path)
        {
            files.Add(path, name);
            outPorts.Add(new Port(this, name, null, typeof(DataTable)));
        }

        public void DeleteFile(string name)
        {
            files.Remove(name);
            outPorts.RemoveAll(p => p.Name == name);
        }

        public void ClearFiles()
        {
            files.Clear();
            outPorts.Clear();
        }
    }
}

using CoreInterfaces;
using CsvParser;
using HydrologyCore.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace HydrologyCore.Experiment.Nodes
{
    public class InitNode : AbstractNode
    {
        // key = filePath, value = var name
        private Dictionary<string, string> files;
        public IDictionary<string, string> Files { get { return files; } }

        public InitNode(string name, NodeContainer nodeContainer) : base(name, nodeContainer)
        {
            files = new Dictionary<string, string>();
        }

        public override void Run()
        {
            output = new Dictionary<string, object>();
            foreach (var file in files.Keys)
            {
                IReader reader = new CSVParser();
                DataTable table = reader.Read(file);
                table.TableName = files[file];
                output.Add(files[file], table);
            }
        }
    }
}

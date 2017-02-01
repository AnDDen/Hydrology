using System.Collections.Generic;
using System.Data;

namespace HydrologyCore.Experiment.Nodes
{
    public class LoopNode : AbstractNode
    {
        public double Value { get; set; }

        public double FromValue { get; set; }
        public double ToValue { get; set; }
        public double Step { get; set; }

        public NodeContainer LoopBody { get; set; }

        Dictionary<double, Dictionary<string, IDictionary<string, object>>> results;

        public LoopNode(string name, NodeContainer nodeContainer) : base(name, nodeContainer)
        {
            LoopBody = new NodeContainer()
            {
                Parent = nodeContainer,
                Path = nodeContainer.Path + "/" + name
            };
        }

        public override bool DependsOn(AbstractNode node)
        {
            return LoopBody.DependsOn(node);
        }

        public override object GetVarValue(string name)
        {
            DataTable table = new DataTable(name);
            int pos = name.IndexOf("/");
            var nodeName = name.Substring(0, pos - 1);
            var varName = name.Substring(pos + 1);
            table.Columns.Add("Key");
            table.Columns.Add("Value");
            foreach (var res in results)
            {
                var row = table.NewRow();
                row["Key"] = res.Key;
                row["Value"] = res.Value[nodeName][varName];
                table.Rows.Add(row);
            }
            return table;
        }

        public override void Run()
        {
            results = new Dictionary<double, Dictionary<string, IDictionary<string, object>>>();
            for (double Value = FromValue; Value <= ToValue; Value += Step)
            {
                LoopBody.Run();
                var currentResults = new Dictionary<string, IDictionary<string, object>>();
                foreach (var node in LoopBody.Nodes)
                    currentResults.Add(node.Name, node.Output);
                results.Add(Value, currentResults);
            }
        }
    }
}

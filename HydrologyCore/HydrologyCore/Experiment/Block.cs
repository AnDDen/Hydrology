using HydrologyCore.Events;
using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment
{
    public class Block : IRunable
    {
        protected List<IRunable> nodes = new List<IRunable>();
        public IList<IRunable> Nodes => nodes;

        protected List<Connection> connections = new List<Connection>();
        public IList<Connection> Connections => connections;

        public IList<Port> InPorts => nodes.Where(x => x is InPortNode).Select(x => (x as InPortNode).BlockPort).ToList();

        public IList<Port> OutPorts => nodes.Where(x => x is OutPortNode).Select(x => (x as OutPortNode).BlockPort).ToList();

        public string Path { get; set; }

        public Block Parent { get; set; }

        public Block RootContainer
        {
            get
            {
                Block p = this;
                while (p.Parent != null)
                    p = p.Parent;
                return p;
            }
        }

        public event NameChangedEventHandler NameChanged;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NameChanged?.Invoke(this, new NameChangedEventArgs(name));
            }
        }

        public Block(string name, Block parent)
        {
            this.name = name;
            Parent = parent;
            if (parent != null && name != null)
                Path = parent.Path + "/" + name;
        }

        public virtual void Run(Context ctx)
        {
            IDictionary<IRunable, List<IRunable>> graph = GenerateExecutionGraph();      
            while (graph.Keys.Count != 0)
            {
                var forExecute = graph.Where(x => x.Value.Count == 0).Select(x => x.Key);
                foreach (var node in forExecute)
                {
                    var ports = Connections.Where(c => c.To.Owner == node).ToDictionary(c => c.To, c => c.From);
                    foreach (var p in ports)
                    {
                        ctx.AddPortValue(p.Key, ctx.GetPortValue(p.Value));
                    }

                    node.Run(ctx);

                    graph.Remove(node);
                    foreach (var pair in graph)
                        pair.Value.Remove(node);
                }
            }
        }

        public virtual void Run(Context ctx, BackgroundWorker worker, int count, ref int current)
        {
            IDictionary<IRunable, List<IRunable>> graph = GenerateExecutionGraph();
            while (graph.Keys.Count != 0)
            {
                var forExecute = graph.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
                foreach (var node in forExecute)
                {
                    var ports = Connections.Where(c => c.To.Owner == node).ToDictionary(c => c.To, c => c.From);
                    foreach (var p in ports)
                    {
                        ctx.AddPortValue(p.Key, ctx.GetPortValue(p.Value));
                    }

                    node.Run(ctx, worker, count, ref current);

                    graph.Remove(node);
                    foreach (var pair in graph)
                        if (pair.Value.Contains(node))
                            pair.Value.Remove(node);
                }
            }
        }

        private IDictionary<IRunable, List<IRunable>> GenerateExecutionGraph() =>
            nodes.ToDictionary(r => r, r => Connections.Where(c => c.From.Owner == r).Select(c => c.To.Owner).ToList());

        public Block AddNode(IRunable node)
        {
            nodes.Add(node);
            return this;
        }

        public Block RemoveNode(IRunable node)
        {
            nodes.Remove(node);
            return this;
        }
        
        public IRunable FindNode(string name) => nodes.FirstOrDefault(x => x.Name == name);

        public virtual int TotalNodeCount()
        {
            int res = nodes.Count;
            var blocks = nodes.Where(x => x is Block);
            foreach (var block in blocks)
            {
                res += (block as Block).TotalNodeCount();
            }
            return res;
        }

        private bool DFS(IDictionary<IRunable, List<IRunable>> graph, IRunable v, Dictionary<IRunable, int> color)
        {
            color[v] = 1;
            foreach (var u in graph[v])
            {
                if (color[u] == 0)
                {
                    bool f = DFS(graph, u, color);
                    if (f) return true;
                }
                else if (color[u] == 1)
                    return true;
            }
            color[v] = 2;
            return false;
        }

        public bool CheckForCycles(IDictionary<IRunable, List<IRunable>> graph)
        {
            Dictionary<IRunable, int> color = new Dictionary<IRunable, int>();
            foreach (var node in nodes)
                color[node] = 0;
            foreach (var node in nodes)
            {
                bool f = DFS(graph, node, color);
                if (f) return true;
            }
            return false;
        }

        public bool CheckForCycles() => CheckForCycles(GenerateExecutionGraph());
    }
}

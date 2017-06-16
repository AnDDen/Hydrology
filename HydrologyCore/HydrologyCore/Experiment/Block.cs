using HydrologyCore.Context;
using HydrologyCore.Converters;
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

        public string PathPrefix { get; set; }

        public virtual string GetPath() => (PathPrefix ?? "") + Parent?.GetPath() + "/" + Name ?? "";

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

        public event NodeExecutionStartEventHandler ExecutionStart;
        public event NodeStatusChangedEventHandler StatusChanged;

        protected void InvokeExecutionStart(IContext ctx)
        {
            ctx.SetStatus(this, ExecutionStatus.EXECUTING);
            ExecutionStart?.Invoke(this, new NodeStatusChangedEventArgs(ctx, this));
        }

        protected void ChangeStatus(IContext ctx, ExecutionStatus status, Exception error)
        {
            if (ctx.GetContext(this).Status != ExecutionStatus.WARNING || status != ExecutionStatus.SUCCESS)
                ctx.SetStatus(this, status);
            ctx.SetError(this, error);
            StatusChanged?.Invoke(this, new NodeStatusChangedEventArgs(ctx, this));
        }

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
        }

        public virtual void Run(IContext ctx, BackgroundWorker worker)
        {
            if (!(this is LoopBlock))
                InvokeExecutionStart(ctx);
            try
            {
                IDictionary<IRunable, List<IRunable>> graph = GenerateExecutionGraph();
                while (graph.Keys.Count != 0)
                {
                    if (worker.CancellationPending)
                        return;
                    var forExecute = graph.Where(x => x.Value.Count == 0).Select(x => x.Key).ToList();
                    foreach (var node in forExecute)
                    {
                        if (worker.CancellationPending)
                            return;

                        (ctx as BlockContext).CreateContextForNode(node);

                        var ports = Connections.Where(c => c.To.Owner == node).ToDictionary(c => c.To, c => c.From);
                        foreach (var p in ports)
                        {
                            if (p.Key.DataType != p.Value.DataType)
                                throw new Exception($"Connected ports {p.Key.Owner.Name}.{p.Key.Name} and {p.Value.Owner.Name}.{p.Value.Name} have different data types");
                            object value = ctx.GetPortValue(p.Value);
                            IConverter converter = ConvertersFactory.GetConverter(p.Key.DataType);
                            object converted = converter.Convert(value, p.Value.ElementType, p.Key.ElementType);
                            ctx.SetPortValue(p.Key, converted);
                        }

                        IContext nodeCtx = ctx;
                        if (node is Block)
                            nodeCtx = ctx.GetContext(node);

                        node.Run(nodeCtx, worker);

                        graph.Remove(node);
                        foreach (var pair in graph)
                            while (pair.Value.Contains(node))
                                pair.Value.Remove(node);
                    }
                }
                if (!(this is LoopBlock))
                    ChangeStatus(ctx, ExecutionStatus.SUCCESS, null);
            }
            catch (Exception e)
            {
                if (!(this is LoopBlock))
                    ChangeStatus(ctx, ExecutionStatus.ERROR, e);
                throw e;
            }
        }

        private IDictionary<IRunable, List<IRunable>> GenerateExecutionGraph() =>
            nodes.ToDictionary(r => r, r => Connections.Where(c => c.To.Owner == r).Select(c => c.From.Owner).ToList());

        private IDictionary<IRunable, List<IRunable>> ExecutionGraphForCheck() =>
            nodes.ToDictionary(r => r, r => Connections.Where(c => c.From.Owner == r).Select(c => c.To.Owner).ToList());

        public Block AddNode(IRunable node)
        {
            nodes.Add(node);
            node.ExecutionStart += (sender, e) => { ExecutionStart?.Invoke(sender, e); };
            node.StatusChanged += (sender, e) => { StatusChanged?.Invoke(sender, e); };
            return this;
        }

        public Block RemoveNode(IRunable node)
        {
            nodes.Remove(node);
            return this;
        }
        
        public IRunable FindNode(string name) => nodes.FirstOrDefault(x => x.Name == name);

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

        public bool CheckForCycles() => CheckForCycles(ExecutionGraphForCheck());
    }
}

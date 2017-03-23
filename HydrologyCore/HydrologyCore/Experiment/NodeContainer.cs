using HydrologyCore.Experiment.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment
{
    public class NodeContainer
    {
        private List<AbstractNode> nodes;
        public IList<AbstractNode> Nodes { get { return nodes; } }

        public string Path { get; set; }

        public NodeContainer Parent { get; set; }

        public LoopNode LoopNode { get; set; }

        public NodeContainer RootContainer
        {
            get
            {
                NodeContainer p = this;
                while (p.Parent != null)
                    p = p.Parent;
                return p;
            }
        }

        public NodeContainer()
        {
            nodes = new List<AbstractNode>();
        }
                
        public void Run()
        {
            List<AbstractNode> left = new List<AbstractNode>(nodes);
            while (left.Count != 0)
            {
                var forExecute = left.Where((x) => left.Intersect(x.Previous).Count() == 0);
                foreach (var node in forExecute)
                {
                    node.Run();
                    left.Remove(node);
                }
            }
        }

        public void Run(BackgroundWorker worker, int count, ref int current)
        {
            List<AbstractNode> left = new List<AbstractNode>(nodes);
            while (left.Count != 0)
            {
                var forExecute = new List<AbstractNode>(left.Where((x) => left.Intersect(x.Previous).Count() == 0));
                foreach (var node in forExecute)
                {
                    if (Core.Instance.CheckWorkerCancel(worker))
                        return;
                    node.Run(worker, count, ref current);
                    left.Remove(node);
                }
            }
        }

        public void SetNodeOrder()
        {
            foreach (var node in nodes)
            {
                node.Next.Clear();
                node.Previous.Clear();
            }

            foreach (var node in nodes)
            {
                foreach (var next in GetNextNodes(node))
                {
                    node.Next.Add(next);
                    next.Previous.Add(node);
                }
            }
        }

        public bool DependsOn(AbstractNode node)
        {
            foreach (var p in nodes)
                if (p != node && p.DependsOn(node))
                    return true;
            return false;
        }

        public List<AbstractNode> GetNextNodes(AbstractNode node)
        {
            List<AbstractNode> res = new List<AbstractNode>();
            foreach (var p in nodes)
                if (p != node && p.DependsOn(node))
                    res.Add(p);
            return res;
        }

        public NodeContainer AddNode(AbstractNode node)
        {
            nodes.Add(node);
            return this;
        }

        public NodeContainer RemoveNode(AbstractNode node)
        {
            nodes.Remove(node);
            // todo : remove references
            return this;
        }

        public AbstractNode FindNode(string name)
        {
            AbstractNode node = nodes.Find((x) => x.Name == name);
            if (node == null)
                node = Parent.FindNode(name);
            return node;
        }

        public int TotalNodeCount()
        {
            int res = nodes.Count;
            var loops = nodes.Where(x => x is LoopNode);
            foreach (var loop in loops)
            {
                res += (loop as LoopNode).LoopBody.TotalNodeCount();
            }
            return res;
        }

        private bool DFS(AbstractNode v, Dictionary<AbstractNode, int> color)
        {
            color[v] = 1;
            foreach (var u in v.Next)
            {
                if (color[u] == 0)
                {
                    bool f = DFS(u, color);
                    if (f) return true;
                }
                else if (color[u] == 1)
                    return true;
            }
            color[v] = 2;
            return false;
        }

        public bool CheckForCycles()
        {
            Dictionary<AbstractNode, int> color = new Dictionary<AbstractNode, int>();
            foreach (var node in nodes)
                color[node] = 0;
            foreach (var node in nodes)
            {
                bool f = DFS(node, color);
                if (f) return true;
            }
            return false;
        }

        public AbstractNode ResolveNode(string name)
        {
            AbstractNode node = nodes.Find(x => x.Name == name);
            if (node != null)
                return node;
            else 
                foreach (var loop in nodes.Where(x => x is LoopNode)) {
                    node = (loop as LoopNode).LoopBody.ResolveNode(name);
                    if (node != null)
                        return node;
                }
            return null;
        }
    }
}

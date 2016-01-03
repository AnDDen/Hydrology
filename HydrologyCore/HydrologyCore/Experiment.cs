using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;
using System.Data;
using System.IO;
using CsvParser;

namespace HydrologyCore
{
    public class Experiment
    {
        private IList<AlgorithmNode> algorithms = new List<AlgorithmNode>();
        public IList<AlgorithmNode> Algorithms { get { return algorithms; } }

        public Experiment()
        {
            this.algorithms = new List<AlgorithmNode>();
        }

        public Experiment(IList<AlgorithmNode> algorithms)
            : this()
        {            
            foreach (AlgorithmNode alg in algorithms)
                this.algorithms.Add(alg);
        }

        private IList<AlgorithmNode> TopologicalSort(IList<AlgorithmNode> nodes)
        {
            IList<AlgorithmNode> sorted = new List<AlgorithmNode>();

            ISet<AlgorithmNode> s = new HashSet<AlgorithmNode>();
            var a = new Dictionary<AlgorithmNode, IList<AlgorithmNode>>();

            foreach (AlgorithmNode node in nodes)
            {
                a[node] = new List<AlgorithmNode>(node.Prev);
                if (a[node].Count == 0)
                    s.Add(node);
            }

            while (s.Count > 0)
            {
                AlgorithmNode node = s.First();
                s.Remove(node);
                sorted.Add(node);
                foreach (AlgorithmNode child in node.Next)
                {
                    a[child].Remove(node);
                    if (a[child].Count == 0)
                        s.Add(child);
                }
            }

            return sorted;
        }

        public void Run()
        {
            var ctx = new Context();
            IList<AlgorithmNode> algs = TopologicalSort(algorithms);
            foreach (AlgorithmNode alg in algs) {
                alg.Init();
                alg.Run(ctx);
                alg.WriteToFile();
                ctx.History.Add(alg);
            }
        }

        public Experiment StartFrom(string initFolder)
        {
            //read data from folder and store in the context as initial data
            return this;
        }

        public Experiment Then(AlgorithmNode node)
        {
            //link to prev node
            return this;
        }

    }
}

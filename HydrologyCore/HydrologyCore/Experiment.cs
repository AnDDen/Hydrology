using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;
using System.Data;

namespace HydrologyCore
{
    public class Experiment
    {
        private IList<IAlgorithm> algorithms;
        private DataSet result;

        public Experiment(params IAlgorithm[] algorithms)
        {
            this.algorithms = new List<IAlgorithm>();
            foreach (IAlgorithm alg in algorithms)
            {
                this.algorithms.Add(alg);
            } 
        }

        public void Init(DataSet data)
        {
            algorithms[0].Init(data);
        }

        public void Run()
        {
            for (int i = 0; i < algorithms.Count; i++) {
                algorithms[i].Run();
                DataSet res = algorithms[i].Results;
                if (i < algorithms.Count - 1)
                    algorithms[i + 1].Init(res);
                else result = res;
            }
        }

        public DataSet Result
        {
            get { return result; }
        }
    }
}

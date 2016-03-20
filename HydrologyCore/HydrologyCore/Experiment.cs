using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Data;
using System.IO;
using CsvParser;

namespace HydrologyCore
{
    public class Experiment
    {
        private IList<AlgorithmNode> algorithms;
        public IList<AlgorithmNode> Algorithms { get { return algorithms; } }

        private Context context;

        private string outDir;

        public delegate void ExperimentStatusHandler(string message);

        public event ExperimentStatusHandler AlgorithmChanged;

        public Experiment()
        {
            algorithms = new List<AlgorithmNode>();
            context = new Context();
        }

        public void Run()
        {
            outDir = string.Format("Experiment.{0}", DateTime.Now.ToString("yyyyMMdd-HHmmss"));

            foreach (AlgorithmNode alg in algorithms) {
                if (AlgorithmChanged != null)
                    AlgorithmChanged(alg.Name);

                alg.Init();
                alg.Run(context);

                string algOutDir = outDir + "/" + alg.Name;
                if (Directory.Exists(algOutDir))
                {
                    int i = 2;
                    while (Directory.Exists(algOutDir + i.ToString())) i++;
                    algOutDir = algOutDir + i.ToString();
                }

                alg.WriteToFile(algOutDir);
                context.History.Add(alg);
            }
        }

        public Experiment StartFrom(string initFolder)
        {
            //read data from folder and store in the context as initial data
            string[] files = Directory.GetFiles(initFolder, "*.csv");
            DataSet data = new DataSet();
            for (int i = 0; i < files.Length; i++)
            {
                IReader reader = new CSVParser();
                DataTable table = reader.Read(files[i]);
                string name = files[i].Substring(0, files[i].Length - 4);
                name = name.Substring(name.LastIndexOf(initFolder) + initFolder.Length + 1);
                table.TableName = name;
                data.Tables.Add(table);
            }
            context.InitialData = data;

            return this;
        }

        public Experiment Then(AlgorithmNode node)
        {
            //link to prev node
            algorithms.Add(node);
            if (algorithms.Count > 1)
                AlgorithmNode.Connect(algorithms[algorithms.Count - 2], algorithms[algorithms.Count - 1]);

            return this;
        }
    }
}

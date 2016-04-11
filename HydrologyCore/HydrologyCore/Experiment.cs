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
        private IList<IExperimentNode> nodes;
        public IList<IExperimentNode> Nodes { get { return nodes; } }

        private Context context;

        private string outDir;

        public delegate void ExperimentStatusHandler(string message);

        public event ExperimentStatusHandler AlgorithmChanged;

        public Experiment()
        {
            nodes = new List<IExperimentNode>();
            context = new Context();
            context.InitialData = new DataSet();
        }

        public void Run()
        {
            outDir = string.Format("Experiment.{0}", DateTime.Now.ToString("yyyyMMdd-HHmmss"));

            foreach (IExperimentNode node in nodes)
            {
                if (node is AlgorithmNode)
                {
                    var alg = node as AlgorithmNode;
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
                else if (node is RunProcessNode)
                {
                    var runProc = node as RunProcessNode;
                    runProc.Run();
                }
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

        private void Connect(IExperimentNode node1, IExperimentNode node2)
        {
            node1.Next = node2;
            node2.Prev = node1;
        }

        public Experiment Then(IExperimentNode node)
        {
            //link to prev node
            nodes.Add(node);
            if (nodes.Count > 1)
                Connect(nodes[nodes.Count - 2], nodes[nodes.Count - 1]);

            return this;
        }
    }
}

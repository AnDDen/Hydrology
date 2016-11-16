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

        public string OutDir { get; set; }

        public delegate void ExperimentStatusHandler(IExperimentNode node);


        private string initFolder;

        public Experiment(string outDir)
        {
            nodes = new List<IExperimentNode>();
            initFolder = null;
            OutDir = outDir;
        }

        public Experiment() : this(string.Format("Experiment.{0}", DateTime.Now.ToString("yyyyMMdd-HHmmss"))) { }

        public void Run()
        {
            Context context = new Context();
            if (initFolder != null)
                context.InitialData = InitData(initFolder);
            foreach (IExperimentNode node in nodes)
            {
                if (CurrentNodeChanged != null)
                    CurrentNodeChanged(node);

                if (node.IsSaveResults)
                {
                    string nodeOutDir = OutDir + "/" + node.Name;
                    if (Directory.Exists(nodeOutDir))
                    {
                        int i = 2;
                        while (Directory.Exists(nodeOutDir + i.ToString())) i++;
                        nodeOutDir = nodeOutDir + i.ToString();
                    }

                    node.SaveResultsPath = nodeOutDir;
                }

                node.Run(context);

                if (node.IsSaveResults)
                    node.SaveResults();
                
                if (node.IsStoreInContext)
                    context.History.Add(node);
            }
        }

        public Experiment StartFrom(string initFolder)
        {
            this.initFolder = initFolder;
            return this;
        }

        private DataSet InitData(string initFolder)
        {
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
            return data;
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

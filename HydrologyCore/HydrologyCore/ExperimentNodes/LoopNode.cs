using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.ExperimentNodes
{
    public class LoopNode : IExperimentNode
    {
        public IExperimentNode Next { get; set; }
        public IExperimentNode Prev { get; set; }

        private IList<IExperimentNode> body;
        public IList<IExperimentNode> Body { get { return body; } }

        public string Name { get; set; }
        public bool IsSaveResults { get { return true; } set { } }
        public string SaveResultsPath { get; set; }
        public string SaveResultsFolder { get; set; }

        public bool IsStoreInContext { get { return true; } }

        public DataSet results;
        public DataSet Results { get { return results; } }

        public string LoopVar { get; set; }
        public double InitValue { get; set; }
        public double EndValue { get; set; }
        public double Step { get; set; }

        public LoopNode(IList<IExperimentNode> body, string loopVar, double initValue, double endValue, double step)
        {
            this.body = body;
            Name = string.Format("Loop {0} = {1}..{2}, {3}", loopVar, initValue, endValue, step);
            SaveResultsFolder = string.Format("Loop {0}", loopVar);
            LoopVar = loopVar;
            InitValue = initValue;
            EndValue = endValue;
            Step = step;
        }

        public void Run(IContext ctx)
        {
            results = new DataSet();
            DataTable table = new DataTable(Name);
            table.Columns.Add("value");
            foreach (IExperimentNode node in body)
            {
                if (node.IsStoreInContext)
                    table.Columns.Add(node.Name, typeof(DataSet));
            }
            for (double i = InitValue; i <= EndValue; i += Step)
            {
                string outDir = string.Format("{0}/{1}={2}", SaveResultsPath, LoopVar, i);
                Directory.CreateDirectory(outDir);
                DataRow row = table.NewRow();
                row["value"] = i;

                ReplaceParamValue(LoopVar, i);

                foreach (IExperimentNode node in body)
                {
                    if (node.IsSaveResults)
                    {
                        string nodeOutDir = outDir + "/" + node.Name;
                        if (Directory.Exists(nodeOutDir))
                        {
                            int j = 2;
                            while (Directory.Exists(nodeOutDir + j.ToString())) j++;
                            nodeOutDir = nodeOutDir + j.ToString();
                        }

                        node.SaveResultsPath = nodeOutDir;
                    }

                    node.Run(ctx);

                    if (node.IsSaveResults)
                        node.SaveResults();

                    if (node.IsStoreInContext)
                    {
                        row[node.Name] = node.Results;
                    }                    
                }

                RestoreParamsTable(LoopVar);
                table.Rows.Add(row);
            }
            results.Tables.Add(table);
        }        

        public void SaveResults() { }    
        
        public void ReplaceParamValue(string loopVar, object value)
        {
            foreach (IExperimentNode node in body)
            {
                if (node is AlgorithmNode)
                    (node as AlgorithmNode).ReplaceParamValue(loopVar, value);
                else if (node is LoopNode)
                    (node as LoopNode).ReplaceParamValue(loopVar, value);
            }
        }

        public void RestoreParamsTable(string loopVar)
        {
            foreach (IExperimentNode node in body)
            {
                if (node is AlgorithmNode)
                    (node as AlgorithmNode).RestoreParamsTable(loopVar);
                else if (node is LoopNode)
                    (node as LoopNode).RestoreParamsTable(loopVar);
            }
        }
    }
}

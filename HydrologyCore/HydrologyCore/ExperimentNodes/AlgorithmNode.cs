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
    public class AlgorithmNode : IExperimentNode
    {
        public IAlgorithm Algorithm { get; set; }
        public IExperimentNode Next { get; set; }
        public IExperimentNode Prev { get; set; }

        public string Name { get; set; }

        public bool IsSaveResults { get; set; }
        public string SaveResultsPath { get; set; }

        public bool IsStoreInContext { get { return true; } }

        private DataSet data;
        private DataTable initialParams;
        private string initPath;

        public AlgorithmNode(Type algorithmType)
        {
            Algorithm = (IAlgorithm)Activator.CreateInstance(algorithmType);
            Name = algorithmType.Name;
            Next = null;
            Prev = null;
            data = new DataSet();
            IsSaveResults = true;
            initPath = null;
        }

        private void WriteData(DataSet data, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            IWriter writer = new CSVWriter();
            foreach (DataTable dt in data.Tables)
                writer.Write(dt, string.Format("{0}/{1}.csv", outputDir, dt.TableName));
        }

        public DataSet Results
        {
            get { return Algorithm.Results; }
        }

        public void Run(IContext context)
        {
            InitData();
            Algorithm.Init(data);
            Algorithm.Run(context);
        }

        public void SaveResults()
        {
            WriteData(Algorithm.Results, SaveResultsPath);
        }

        public AlgorithmNode InitFromFolder(string path)
        {
            initPath = path;

            if (!string.IsNullOrEmpty(path))
            {
                string[] files = Directory.GetFiles(initPath, "*.csv");
                for (int i = 0; i < files.Length; i++)
                {
                    IReader reader = new CSVParser();
                    DataTable table = reader.Read(files[i]);
                    string name = files[i].Substring(0, files[i].Length - 4);
                    name = name.Substring(name.LastIndexOf(initPath) + initPath.Length + 1);
                    table.TableName = name;
                    data.Tables.Add(table);
                }
            }

            return this;
        }

        private void InitData()
        {
            //// todo : put logic from InitFromFolder here
        }

        public AlgorithmNode SetParams(DataTable paramsTbl)
        {
            if (data.Tables["params"] != null)
                data.Tables.Remove("params");
            paramsTbl.TableName = "params";
            data.Tables.Add(paramsTbl);
            initialParams = data.Tables["params"];
            return this;
        }

        public void ReplaceParamValue(string varName, object value)
        {
            if (data.Tables.Contains("params"))
            {
                DataTable paramsTbl = data.Tables["params"].Copy();

                foreach (DataRow r in paramsTbl.Rows)
                {
                    if (r["Value"].ToString().Trim(' ', '{', '}').Equals(varName))
                        r["Value"] = value;
                }

                data.Tables.Remove("params");
                data.Tables.Add(paramsTbl);
            }
        }

        public void RestoreParamsTable(string varName)
        {
            DataTable paramsTbl = data.Tables["params"].Copy();
            for (int i = 0; i < paramsTbl.Rows.Count; i++)
                if (initialParams.Rows[i]["Value"].ToString().Trim(' ', '{', '}').Equals(varName))
                    paramsTbl.Rows[i]["Value"] = initialParams.Rows[i]["Value"];
            data.Tables.Remove("params");
            data.Tables.Add(paramsTbl);
        }
    }
}

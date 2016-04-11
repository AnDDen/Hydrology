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

        private string name;
        public string Name { get { return name; } }

        private DataSet data;

        public AlgorithmNode(Type algorithmType)
        {
            Algorithm = (IAlgorithm)Activator.CreateInstance(algorithmType);
            name = algorithmType.Name;
            Next = null;
            Prev = null;
            data = new DataSet();
        }

        private void WriteData(DataSet data, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            IWriter writer = new CSVWriter();
            foreach (DataTable dt in data.Tables)
                writer.Write(dt, string.Format("{0}/{1}.csv", outputDir, dt.TableName));
        }

        public void Init()
        {
            Algorithm.Init(data);
        }

        public DataSet Results
        {
            get { return Algorithm.Results; }
        }

        public void Run(Context context)
        {
            Algorithm.Run(context);
        }

        public void WriteToFile(string path)
        {
            WriteData(Algorithm.Results, path);
        }

        public AlgorithmNode InitFromFolder(string path)
        {
            string[] files = Directory.GetFiles(path, "*.csv");
            for (int i = 0; i < files.Length; i++)
            {
                IReader reader = new CSVParser();
                DataTable table = reader.Read(files[i]);
                string name = files[i].Substring(0, files[i].Length - 4);
                name = name.Substring(name.LastIndexOf(path) + path.Length);
                table.TableName = name;
                data.Tables.Add(table);
            }
            return this;
        }
    }
}

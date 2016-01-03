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
    public class AlgorithmNode
    {
        public IAlgorithm Algorithm { get; set; }
        private IList<AlgorithmNode> next;
        private IList<AlgorithmNode> prev;
        public IList<AlgorithmNode> Next { get { return next; } }
        public IList<AlgorithmNode> Prev { get { return prev; } }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        private DataSet data;

        public AlgorithmNode(IAlgorithm algorithm, string inputPath, string outputPath)
        {
            Algorithm = algorithm;
            next = new List<AlgorithmNode>();
            prev = new List<AlgorithmNode>();
            InputPath = inputPath;
            OutputPath = outputPath;
            data = new DataSet();
        }

        public AlgorithmNode(Type algorithmType, string inputPath, string outputPath)
        {
            Algorithm = (IAlgorithm)Activator.CreateInstance(algorithmType);
            next = new List<AlgorithmNode>();
            prev = new List<AlgorithmNode>();
            InputPath = inputPath;
            OutputPath = outputPath;
            data = new DataSet();
        }

        private void LoadData()
        {
            string[] files = Directory.GetFiles(InputPath, "*.csv");
            data = new DataSet();
            for (int i = 0; i < files.Length; i++)
            {
                CSVParser parser = new CSVParser();
                DataTable table = parser.Parse(files[i]);
                string name = files[i].Substring(0, files[i].Length - 4);
                name = name.Substring(name.LastIndexOf(InputPath) + InputPath.Length);
                table.TableName = name;
                data.Tables.Add(table);
            }
        }

        private void WriteData(DataSet data, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            foreach (DataTable dt in data.Tables)
                CSVWriter.Write(dt, string.Format("{0}/{1}.csv", outputDir, dt.TableName));
        }

        public void Init()
        {
            LoadData();
            foreach (AlgorithmNode node in Prev)
            {
                foreach (DataTable table in node.Results.Tables)
                    data.Tables.Add(table.Copy());
            }
            Algorithm.Init(data);
        }

        public DataSet Results
        {
            get { return Algorithm.Results; }
        }

        public void Run(Context ctx)
        {
            Algorithm.Run(ctx);
        }

        public void WriteToFile()
        {
            WriteData(Algorithm.Results, OutputPath);
        }

        public static void Connect(AlgorithmNode from, AlgorithmNode to)
        {
            from.Next.Add(to);
            to.Prev.Add(from);
        }

        public AlgorithmNode InitFromFolder(string p)
        {
            return this;
        }
    }
}

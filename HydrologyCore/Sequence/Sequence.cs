using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Data;
using System.Reflection;
using System.Globalization;

namespace Sequence
{
    [Parameter("eps", 1E-5, typeof(Double))]
    [Parameter("ETA", 2.0, typeof(Double))]
    [Parameter("CV", 0.8, typeof(Double))]
    [Parameter("N", 50000, typeof(Int32))]
    [Parameter("r", 0.109, typeof(Double))]
    [Name("Генерация последовательности стока")]
    public class FlowSequenceGeneration : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;
        Random Rand;

        public void Init(DataSet data)
        {
            this.data = data;
            Rand = new Random();
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();
            DataTable kTable = new DataTable() { TableName = "FlowSequence" };
            kTable.Columns.Add("K");

            DataTable paramsTable = data.Tables["\\params"];

            var attrs = typeof(FlowSequenceGeneration).GetCustomAttributes<ParameterAttribute>();

            double eps = (double)attrs.First((param) => { return param.Name == "eps"; }).DefaultValue;
            int n = (int)attrs.First((param) => { return param.Name == "N"; }).DefaultValue;
            double r = (double)attrs.First((param) => { return param.Name == "r"; }).DefaultValue;
            double cv = (double)attrs.First((param) => { return param.Name == "CV"; }).DefaultValue;
            double eta = (double)attrs.First((param) => { return param.Name == "ETA"; }).DefaultValue;

            foreach(DataRow row in paramsTable.Rows) {
                switch (row["Name"].ToString())
                {
                    case "eps":
                        eps = double.Parse(row["Value"].ToString(), CultureInfo.InvariantCulture);
                        break;
                    case "N": 
                        n = int.Parse(row["Value"].ToString()); 
                        break;
                    case "r": 
                        r = double.Parse(row["Value"].ToString(), CultureInfo.InvariantCulture); 
                        break;
                    case "CV":
                        cv = double.Parse(row["Value"].ToString(), CultureInfo.InvariantCulture); 
                        break;
                    case "ETA":
                        eta = double.Parse(row["Value"].ToString(), CultureInfo.InvariantCulture); 
                        break;
                }
            }

            DataTable optSource = ctx.InitialData.Tables["optsource_big"];

            double[] kArray = Sequence(optSource, new Statistics(), n, r, cv, eta,eps);

            for (int i = 0; i < kArray.Length; i++)
            {
                DataRow row = kTable.NewRow();
                row["K"] = kArray[i];
                kTable.Rows.Add(row);
            }

            resultSet.Tables.Add(kTable);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

        //столбец по cv и eta
        public double[] BigOptSource(DataTable optSource, double Cv, double Eta)
        {
            int group = 1 + (int)((Eta - 1.0) * 2.0); //выбираем группу
            int c1 = (int)(Cv * 10); //выбираем колонку в группе
            int column = 0; //нужная колонка в таблице
            column = 1 + (group - 1) * 10 + c1;
            return TableValues(optSource, column);
        }
        //значения из  столбца source
        public double[] TableValues(DataTable optSource, int col)
        {
            int i = 0;
            DataColumn column = optSource.Columns[col];
            double[] x = new double[optSource.Rows.Count];
            foreach (DataRow row in optSource.Rows)
            {
                x[i] = Convert.ToDouble(row[column]);
                i++;
            }
            return x;
        }
        //последовательность для заданного N
        double[] Sequence(DataTable optSource, Statistics stat, int N, double r, double Cv, double Eta,double eps)
        {
            double r0 = stat.r0(r, Cv,eps);
            List<double> P = new List<double>(N);
            Double[] m_vK = new Double[N];
            Double[] x = BigOptSource(optSource, Cv, Eta);
            int NSrc = x.Length;
            int index;
            double next, dP;
            for (int i = 0; i < N; ++i)
            {
                next = (i == 0 ? 0.5 : stat.nextP(P[i - 1], r0, Rand.NextDouble(),eps));
                P.Add(next);
                index = (int)(next * NSrc);
                dP = next * NSrc - index;
                next = (index == 0 ? x[1] : x[index]) * dP + (index >= NSrc - 1 ? 0 : x[index + 1]) * (1 - dP);
                if (next < 0) next = 0;
                m_vK[i] = next;
            }

            return m_vK;
        }
    }
}

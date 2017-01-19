using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Reflection;
using System.Data;
using Statistics;
using Helpers;

namespace Transformation
{
    [Parameter("Table Name", "FlowSequence", typeof(string))]
    [Parameter("Size", 1000, typeof(int))]
    [Parameter("Start", 0, typeof(int))]
    [Parameter("Lambda", 0.1, typeof(double))]
    [Parameter("TransformationType", 1, typeof(int))]
    [Parameter("Save", "0", typeof(string))]
    [Name("Модификация")]
    public class Modification : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;
        private Statistic stat;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();

            DataTable sequenceTable = new DataTable() { TableName = "ModifiedSequence" };
            sequenceTable.Columns.Add("X");

            var h = new ParamsHelper(data.Tables["params"]);
            var attrs = typeof(Modification).GetCustomAttributes<ParameterAttribute>();

            var tableName = h.GetValue<string>("Table Name");
            var size = h.GetValue<int>("Size");
            var start = h.GetValue<int>("Start");
            var lambda = h.GetValue<double>("Lambda");
            var type = h.GetValue<int>("TransformationType");

            DataTable src = ctx.Data.Tables[tableName];
            if (src == null)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < ctx.Data.Tables.Count; i++)
                {
                    sb.AppendFormat(sb.Length > 0 ? "{0}, " : "{0}", ctx.Data.Tables[i].TableName);                   
                }

                throw new Exception(string.Format("No table '{0}' found in the input dataset. There is only '{1}' tables found", tableName, sb.ToString()));
            }

            double[] series = GetTableValues(src, 0);

            //разбиение
            var N = series.Length;
            stat = new Statistic(N);

            List<double> modified = new List<double>();
            int negCount = 0;
            transform(series, modified, start, Math.Min(start + size, N), type, lambda, out negCount);

            for (int j = 0; j < modified.Count; j++)
            {
                DataRow row = sequenceTable.NewRow();
                row["X"] = modified[j];                
                sequenceTable.Rows.Add(row);
            }
            resultSet.Tables.Add(sequenceTable);
        }


        public double[] GetTableValues(DataTable X_Table, int col)
        {
            int i = 0;
            DataColumn column = X_Table.Columns[col];
            double[] x = new double[X_Table.Rows.Count];
            foreach (DataRow row in X_Table.Rows)
            {
                x[i] = Convert.ToDouble(row[column]);
                if (i < X_Table.Rows.Count) i++;
            }
            return x;
        }


        public List<double> transform(double[] series, List<double> mods, int i0, int n_, int index, double lambda, out int negCount)
        {
            double Fx = 0;
            double middle = stat.means(series, i0, n_);
            negCount = 0;

            for (int i = i0; i < n_; ++i)
            {
                if (index == 0)
                    Fx = 1;     //const
                else if (index == 1)
                    Fx = (double)(i + 1) / n_;   //linear 1
                                                 //else if (index == 2)
                                                 //Fx = -(double)(i + 1) / n_;      //linear 2
                else if (index == 2)
                    Fx = (double)((double)(i + 1) / (double)n_) * (double)((double)(i + 1) / (double)n_);   //parabola 1
                else if (index == 3)
                    Fx = (double)(1.0 - (double)(i + 1) / (double)n_) * (double)(1.0 - (double)(i + 1) / (double)n_);      //parabola 2 

                double val = series[i] - Fx * lambda * middle;
                if (val >= 0)
                    mods.Add(val);
                else
                {
                    negCount = negCount + 1;
                    mods.Add(0);
                }
            }
            return mods;
        }

        public DataSet Results
        {
            get { return resultSet; }
        }
    }
}

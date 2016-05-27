using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Reflection;

namespace UniformityCheck
{
    [Parameter("n", 0.4, typeof(double))]
    [Parameter("size", 1000, typeof(int))]
    [Parameter("lam1", 0.1, typeof(double))]
    [Parameter("lam2", 0.2, typeof(double))]
    [Parameter("lam3", 0.3, typeof(double))]
    [Parameter("lam4", 0.4, typeof(double))]
    [Parameter("lam5", 0.5, typeof(double))]
    [Name("Модификация")]
    public class Transformation : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;
        public Statistic stat = new Statistic();
        public int index_size = 5;
        public int lam_len = 5;
        public double[,] modifiers1;
        public double[,] modifiers2;
        public double[,] summods;
        public double[, ,] params1;
        //public double[, ,] params2;
        public int N;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();
            DataTable kTable = new DataTable() { TableName = "Parameters" };
            kTable.Columns.Add("average");
            kTable.Columns.Add("deviation");
            kTable.Columns.Add("variation");
            kTable.Columns.Add("asymmetry");
            kTable.Columns.Add("correlation");
            kTable.Columns.Add("eta");

            DataTable sequenceTable = new DataTable() { TableName = "ModifiedSequence" };
            sequenceTable.Columns.Add("const");
            sequenceTable.Columns.Add("linear-1");
            sequenceTable.Columns.Add("linear-2");
            sequenceTable.Columns.Add("parabola-1");
            sequenceTable.Columns.Add("parabola-2");

            DataTable paramsTable = data.Tables["params"];
            var attrs = typeof(Transformation).GetCustomAttributes<ParameterAttribute>();
            
            double[] lambda_items = new double[lam_len];
            double n = 0;
            int size = 0;
            /*double n = (int)attrs.First((param) => { return param.Name == "n"; }).DefaultValue;
            int size = (int)attrs.First((param) => { return param.Name == "size"; }).DefaultValue;
            lambda_items[0] = (int)attrs.First((param) => { return param.Name == "lam1"; }).DefaultValue;
            lambda_items[1] = (int)attrs.First((param) => { return param.Name == "lam2"; }).DefaultValue;
            lambda_items[2] = (int)attrs.First((param) => { return param.Name == "lam3"; }).DefaultValue;
            lambda_items[3] = (int)attrs.First((param) => { return param.Name == "lam4"; }).DefaultValue;
            lambda_items[4] = (int)attrs.First((param) => { return param.Name == "lam5"; }).DefaultValue;*/

            foreach (DataRow row in paramsTable.Rows)
            {
                switch (row["Name"].ToString())
                {
                    case "n":
                        n = double.Parse(row["Value"].ToString());
                        break;
                    case "size":
                        size = int.Parse(row["Value"].ToString());
                        break;
                    case "lam1":
                        lambda_items[0] = double.Parse(row["Value"].ToString());
                        break;
                    case "lam2":
                        lambda_items[1] = double.Parse(row["Value"].ToString());
                        break;
                    case "lam3":
                        lambda_items[2] = double.Parse(row["Value"].ToString());
                        break;
                    case "lam4":
                        lambda_items[3] = double.Parse(row["Value"].ToString());
                        break;
                    case "lam5":
                        lambda_items[4] = double.Parse(row["Value"].ToString());
                        break;
                }
            }

            DataTable series_Table = ctx.GetData("FlowSequenceGeneration").Tables["FlowSequence"];

            double[] series = new double[size];
            series = GetTableValues(series_Table, 0);


            params1 = new double[index_size, lam_len, 6];

            //разбиение
            N = series.Length;
            int n1 = Convert.ToInt32(N * n);
            int n2 = N - n1;
            List<double> mods1 = new List<double>();
            List<double> mods2 = new List<double>();
            int negCount = 0;

            for (int index = 0; index < index_size; index++)
            {
                for (int i = 0; i < lam_len; i++)
                {
                    double lambda = lambda_items[i];
                    mods1 = transform(series, mods1, 0, n1, index, lambda, negCount);
                    mods2 = transform(series, mods2, n1, N, index, lambda, negCount);
                }

            }
            modifiers1 = new double[index_size, mods1.Count / index_size];
            modifiers2 = new double[index_size, mods2.Count / index_size];
            getModifiers(modifiers1, mods1, mods1.Count / index_size);
            getModifiers(modifiers2, mods2, mods2.Count / index_size);
            summods = new double[index_size, modifiers1.GetLength(1) + modifiers2.GetLength(1)];

            double[] tmpMods1 = new double[n1];
            double[] tmpMods2 = new double[n2];

            for (int index = 0; index < index_size; index++)
            {
                for (int j = 0; j < lam_len; j++)
                {
                    tmpMods1 = tempMods(modifiers1, index, j);
                    tmpMods2 = tempMods(modifiers2, index, j);
                    summods = sumMods(summods, tmpMods1, tmpMods2, index, j);
                }
            }

            for (int index = 0; index < index_size; index++)
                for (int j = 0; j < lam_len; j++)
                {
                    params1[index, j, 0] = stat.mean2(summods, index, j, lam_len);
                    params1[index, j, 1] = stat.deviation2(summods, index, j, lam_len);
                    params1[index, j, 2] = stat.variation2(summods, index, j, lam_len);
                    params1[index, j, 3] = stat.skewness2(summods, index, j, lam_len);
                    params1[index, j, 4] = stat.correlation2(summods, index, j, lam_len);
                    params1[index, j, 5] = params1[index, j, 3] == 0 ? 0 : params1[index, j, 3] / params1[index, j, 2];
                }

            SaveParameters(kTable, lambda_items, series);

            for (int j = 0; j < summods.GetLength(1) / lam_len; j++)
            {
                DataRow row = sequenceTable.NewRow();
                row["const"] = summods[0, j];
                row["linear-1"] = summods[1, j];
                row["linear-2"] = summods[2, j];
                row["parabola-1"] = summods[3, j];
                row["parabola-2"] = summods[4, j];
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


        public List<double> transform(double[] series, List<double> mods, int i0, int n_, int index, double lambda, int negCount)
        {
            double Fx = 0;
            double middle = stat.means(series, i0, n_);

            for (int i = i0; i < n_; ++i)
            {
                if (index == 0)
                    Fx = 1;     //const
                else if (index == 1)
                    Fx = (double)(i + 1) / n_;   //linear 1
                else if (index == 2)
                    Fx = -(double)(i + 1) / n_;      //linear 2
                else if (index == 3)
                    Fx = (double)((i + 1) / n_) * (double)((i + 1) / n_);   //parabola 1
                else if (index == 4)
                    Fx = -(double)((i + 1) / n_) * (double)((i + 1) / n_);      //parabola 2 

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


        public double[,] getModifiers(double[,] modifiers, List<double> mods, int n_)
        {
            int k = 0;
            for (int j = 0; j < index_size; j++)
            {
                for (int i = 0; i < n_; i++)
                {
                    modifiers[j, i] = mods[k];
                    k++;
                }
            }
            return modifiers;
        }


        public double[] tempMods(double[,] mods, int index, int j)
        {
            double[] tempMods = new double[mods.GetLength(1) / lam_len];
            int k = 0;
            for (int i = mods.GetLength(1) / lam_len * j; i < mods.GetLength(1) / lam_len * (j + 1); i++)
            {
                tempMods[k] = mods[index, i];
                k++;
            }
            return tempMods;
        }


        public double[,] sumMods(double[,] summods, double[] modfs1, double[] modfs2, int index, int j)
        {
            int k = j * 1000;
            for (int i = 0; i < modfs1.Length; i++)
            {
                summods[index, k] = modfs1[i];
                k++;
            }
            for (int i = 0; i < modfs2.Length; i++)
            {
                summods[index, k] = modfs2[i];
                k++;
            }
            return summods;
        }


        public void SaveParameters(DataTable kTable, double[] lambda_items, double[] series)
        {
            //параметры для входной последовательности 1000 (для lambda = 0).
            double seriesMean = stat.mean(series);
            double seriesDev = stat.deviation(series);
            double seriesVariation = stat.variation(series);
            double seriesSkew = stat.skewness(series);
            double seriesCorr = stat.correlation(series);
            double seriesEta = seriesSkew / seriesVariation;

            DataRow row1 = kTable.NewRow();
            row1["average"] = 0.0;
            kTable.Rows.Add(row1);

            row1 = kTable.NewRow();
            row1["average"] = seriesMean;
            row1["deviation"] = seriesDev;
            row1["variation"] = seriesVariation;
            row1["asymmetry"] = seriesSkew;
            row1["correlation"] = seriesCorr;
            row1["eta"] = seriesEta;
            kTable.Rows.Add(row1);

            row1 = kTable.NewRow();
            row1["average"] = lambda_items[0];
            kTable.Rows.Add(row1);

            for (int j = 0; j < lam_len; j++)
            {
                for (int index = 0; index < index_size; index++)
                {
                    DataRow row = kTable.NewRow();
                    row["average"] = params1[index, j, 0];
                    row["deviation"] = params1[index, j, 1];
                    row["variation"] = params1[index, j, 2];
                    row["asymmetry"] = params1[index, j, 3];
                    row["correlation"] = params1[index, j, 4];
                    row["eta"] = params1[index, j, 5];
                    kTable.Rows.Add(row);
                }
                if (j < lam_len - 1)
                {
                    DataRow row2 = kTable.NewRow();
                    row2["average"] = lambda_items[j + 1];
                    kTable.Rows.Add(row2);
                }
            }
            resultSet.Tables.Add(kTable);
        }


        public DataSet Results
        {
            get { return resultSet; }
        }
    }
}

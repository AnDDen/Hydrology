using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Reflection;

namespace RepresentationCheck
{
    [Name("Рассчет коэффициентов")]
    public class RepresentationCheck : IAlgorithm
    {
        public DataSet Data { get; set; }

        [Input("n", 20)]
        public int N { get; set; }

        [Input("FlowSequenceGeneration")]
        public DataTable X_Table { get; set; }

        [Output("Результат")]
        public DataSet ResultSet { get; set; }

        public void Run()
        {
            ResultSet = new DataSet();
            DataTable kTable = new DataTable() { TableName = "Coefficient" };
            kTable.Columns.Add("Period");
            kTable.Columns.Add("K_srednee");
            kTable.Columns.Add("K_sigma");
            kTable.Columns.Add("K_cv");
            kTable.Columns.Add("K_cs");
            kTable.Columns.Add("K_eta");           

            List<double> k1 = CoefAvg(X_Table, new statistics(), N);
            List<double> k2 = CoefSigma(X_Table, new statistics(), N);
            List<double> k3 = Coef_Cv(X_Table, new statistics(), N);
            List<double> k4 = Coef_Cs(X_Table, new statistics(), N);
            List<double> k5 = Coef_Eta(X_Table, new statistics(), N);
            for (int i = 0; i < k1.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["Period"] = i + ".." + (i + N);
                row["K_srednee"] = k1[i];
                row["K_sigma"] = k2[i];
                row["K_cv"] = k3[i];
                row["K_cs"] = k4[i];
                row["K_eta"] = k5[i];
                kTable.Rows.Add(row);
            }
            ResultSet.Tables.Add(kTable);
        }

        //значения из  столбца source
        public double[] TableValues(DataTable X_Table, int col)
        {
            int i = 0;
            DataColumn column = X_Table.Columns[col];
            double[] x = new double[X_Table.Rows.Count];
            foreach (DataRow row in X_Table.Rows)
            {
                x[i] = Convert.ToDouble(row[column]);
                if (i<X_Table.Rows.Count) i++;
            }
            return x;
        }
        List<double> CoefAvg(DataTable X_Table, statistics stat, int n)
        {
            double[] x = TableValues(X_Table, 0);
            List<double> k1 = new List<double>();
            double avg = stat.Average(x, 0, x.Length);
            for (int i = 0; i <= x.Length - n; i++)
            {
                k1.Add(stat.Average(x, i, n) / avg);
            }
            return k1;

        }
        List<double> CoefSigma(DataTable X_Table, statistics stat, int n)
        {
            double[] x = TableValues(X_Table, 0);
            List<double> k2 = new List<double>();
            double sigma = stat.Sigma(x, 0, x.Length);
            for (int i = 0; i <= x.Length - n; i++)
            {
                k2.Add(stat.Sigma(x, i, n ) / sigma);
            }
            return k2;

        }
        List<double> Coef_Cv(DataTable X_Table, statistics stat, int n)
        {
            double[] x = TableValues(X_Table, 0);
            List<double> k3 = new List<double>();
            double cv = stat.Variation(x, 0, x.Length);
            for (int i = 0; i <= x.Length - n; i++)
            {
                k3.Add(stat.Variation(x, i, n) / cv);
            }
            return k3;

        }
        List<double> Coef_Cs(DataTable X_Table, statistics stat, int n)
        {
            double[] q = new double[n];
            double[] x = TableValues(X_Table, 0);
            List<double> k4 = new List<double>();
            double cs = stat.Asimmetria(x, 0, x.Length);
            for (int i = 0; i <= x.Length - n; i++)
            {
                k4.Add(stat.Asimmetria(x, i, n ) / cs);
            }
            return k4;

        }
        List<double> Coef_Eta(DataTable X_Table, statistics stat, int n)
        {
            double[] q = new double[n];
            double[] x = TableValues(X_Table, 0);
            List<double> k5 = new List<double>();
            double cv = stat.Variation(x, 0, x.Length);
            double cs = stat.Asimmetria(x, 0, x.Length);
            double eta = cs / cv;
            // int index = 0; 
            for (int i = 0; i <= x.Length - n; i++)
            {
                k5.Add(stat.Asimmetria(x, i, n ) / stat.Variation(x, i, n ) / eta);
            }
            return k5;

        }

        [Parameter("resN", 500, typeof(Int32))]
        [Name("Вероятности превышения")]
        public class Probability : IAlgorithm
        {
            private DataSet data;
            private DataSet resultSet;
            
            public void Init(DataSet data)
            {
                this.data = data;
            }

            public void Run(IContext ctx)
            {
                resultSet = new DataSet();
                DataTable pTable = new DataTable() { TableName = "Probability" };
                DataTable K_Table = ctx.Data.Tables["Coefficient"];
                pTable.Columns.Add("k_srednee");
                pTable.Columns.Add("k_sigma");
                pTable.Columns.Add("k_cv");
                pTable.Columns.Add("k_cs");
                pTable.Columns.Add("k_eta");
                RepresentationCheck represent = new RepresentationCheck();
                DataTable paramsTable = data.Tables["params"];
                var attrs = typeof(Probability).GetCustomAttributes<ParameterAttribute>();

                int resN = (int)attrs.First((param) => { return param.Name == "resN"; }).DefaultValue;

                foreach (DataRow row in paramsTable.Rows)
                {
                    switch (row["Name"].ToString())
                    {
                        case "resN":
                            resN = int.Parse(row["Value"].ToString());
                            break;
                    }
                }
            
                for (int i = 0; i < resN; i++)
                {
                    DataRow row = pTable.NewRow();
                    pTable.Rows.Add(row);
                }

                for (int type_k = 1; type_k < 6; type_k++)
                {
                    statistics stat = new statistics();
                    double[] k = represent.TableValues(K_Table, type_k);
                    k = stat.sort_shell(k, k.Length);
                    for (int i = 0; i < resN; i++)
                    {
                        pTable.Rows[i][type_k-1] = k[(int)Math.Round((double)i * ((double)(k.Length - 1)) / ((double)(resN - 1)))];                      
                    }
                }

            resultSet.Tables.Add(pTable);
            }
            
            public DataSet Results
            {
                get { return resultSet; }
            }


        }

        [Parameter("n", 20, typeof(Int32))]
        [Name("Стандарты")]
        public class Standarts : IAlgorithm
        {
            private DataSet data;
            private DataSet resultSet;
            public void Init(DataSet data)
            {
                this.data = data;
            }
            
            public void Run(IContext ctx)
            {
                resultSet = new DataSet();
                DataTable K_Table = ctx.GetData("RepresentationCheck").Tables["Coefficient"];
                DataTable SigmaTable = new DataTable() { TableName = "Standarts" };
                SigmaTable.Columns.Add("n");
                SigmaTable.Columns.Add("Sigma_k_srednee");
                SigmaTable.Columns.Add("Sigma_k_sigma");
                SigmaTable.Columns.Add("Sigma_k_cv");
                SigmaTable.Columns.Add("Sigma_k_cs");
                SigmaTable.Columns.Add("Sigma_k_eta");
                DataTable paramsTable = data.Tables["params"];
                var attrs = typeof(Standarts).GetCustomAttributes<ParameterAttribute>();

                int n = (int)attrs.First((param) => { return param.Name == "n"; }).DefaultValue;

                foreach (DataRow row in paramsTable.Rows)
                {
                    switch (row["Name"].ToString())
                    {
                        case "n":
                            n = int.Parse(row["Value"].ToString());
                            break;
                    }
                }
                RepresentationCheck represent = new RepresentationCheck();
                statistics stat = new statistics();
                DataRow rows = SigmaTable.NewRow();
                rows["n"] = n;
                double[] k = represent.TableValues(K_Table, 1);
                double sigma = stat.standart(k);
                rows["Sigma_k_srednee"] = sigma;
                k = represent.TableValues(K_Table, 2);
                sigma = stat.standart(k);
                rows["Sigma_k_sigma"] = sigma;
                k = represent.TableValues(K_Table, 3);
                sigma = stat.standart(k);
                rows["Sigma_k_cv"] = sigma;
                k = represent.TableValues(K_Table, 4);
                sigma = stat.standart(k);
                rows["Sigma_k_cs"] = sigma;
                k = represent.TableValues(K_Table, 5);
                sigma = stat.standart(k);
                rows["Sigma_k_eta"] = sigma;
                SigmaTable.Rows.Add(rows);
                resultSet.Tables.Add(SigmaTable);

            }

            public DataSet Results
            {
                get { return resultSet; }
            }
        }
    }
}



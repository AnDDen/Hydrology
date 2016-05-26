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
    [Parameter("n", 20, typeof(Int32))]
    [Name("Расчет коэффициентов")]
    public class RepresentationCheck : IAlgorithm
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
            DataTable kTable = new DataTable() { TableName = "Coefficient" };
            kTable.Columns.Add("K_average");
            kTable.Columns.Add("K_dev");
            kTable.Columns.Add("K_variation");
            kTable.Columns.Add("K_asymmetry");
            kTable.Columns.Add("K_eta");

            DataTable paramsTable = data.Tables["params"];
            var attrs = typeof(RepresentationCheck).GetCustomAttributes<ParameterAttribute>();

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
            
            DataTable X_Table = ctx.Data.Tables["FlowSequence"];

            List<double> k1 = CoefAvg(X_Table, new statistics(), n);
            List<double> k2 = CoefSigma(X_Table, new statistics(), n);
            List<double> k3 = Coef_Cv(X_Table, new statistics(), n);
            List<double> k4 = Coef_Cs(X_Table, new statistics(), n);
            List<double> k5 = Coef_Eta(X_Table, new statistics(), n);
            for (int i = 0; i < k1.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["average"] = k1[i];
              //  kTable.Rows.Add(row);
                row["deviation"] = k2[i];
              //  kTable.Rows.Add(row);
                row["variation"] = k3[i];
             //   kTable.Rows.Add(row);
                row["asymmetry"] = k4[i];
             //   kTable.Rows.Add(row);
                row["eta"] = k5[i];
                kTable.Rows.Add(row);
            }
            resultSet.Tables.Add(kTable);
        }

        public DataSet Results
        {
            get { return resultSet; }
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
            double cs = stat.Assimetria(x, 0, x.Length);
            for (int i = 0; i <= x.Length - n; i++)
            {
                k4.Add(stat.Assimetria(x, i, n ) / cs);
            }
            return k4;

        }
        List<double> Coef_Eta(DataTable X_Table, statistics stat, int n)
        {
            double[] q = new double[n];
            double[] x = TableValues(X_Table, 0);
            List<double> k5 = new List<double>();
            double cv = stat.Variation(x, 0, x.Length);
            double cs = stat.Assimetria(x, 0, x.Length);
            double eta = cs / cv;
            // int index = 0; 
            for (int i = 0; i <= x.Length - n; i++)
            {
                k5.Add(stat.Assimetria(x, i, n ) / stat.Variation(x, i, n ) / eta);
            }
            return k5;

        }
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
                pTable.Columns.Add("k1");
                pTable.Columns.Add("k2");
                pTable.Columns.Add("k3");
                pTable.Columns.Add("k4");
                pTable.Columns.Add("k5");
                RepresentationCheck represent = new RepresentationCheck();
                int resN = 100;
                for (int i = 0; i < resN; i++)
                {
                    DataRow row = pTable.NewRow();
                    pTable.Rows.Add(row);
                }

                for (int type_k = 0; type_k < 5; type_k++)
                {
                    statistics stat = new statistics();
                    double[] k = represent.TableValues(K_Table, type_k);
                    k = stat.sort_shell(k, k.Length);
                    for (int i = 0; i < resN; i++)
                    {
                        pTable.Rows[i][type_k] = k[(int)Math.Round((double)i * ((double)(k.Length - 1)) / ((double)(resN - 1)))];                      
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
        [Parameter("i", 0, typeof(Int32))]
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
                DataTable K_Table = ctx.Data.Tables["Coefficient"];
                DataTable SigmaTable = new DataTable() { TableName = "Standarts" };
                SigmaTable.Columns.Add("n");
                SigmaTable.Columns.Add("Sigma_k1");
                SigmaTable.Columns.Add("Sigma_k2");
                SigmaTable.Columns.Add("Sigma_k3");
                SigmaTable.Columns.Add("Sigma_k4");
                SigmaTable.Columns.Add("Sigma_k5");
                DataTable paramsTable = data.Tables["params"];
                var attrs = typeof(Standarts).GetCustomAttributes<ParameterAttribute>();

                int n = (int)attrs.First((param) => { return param.Name == "n"; }).DefaultValue;
                int i = (int)attrs.First((param) => { return param.Name == "i"; }).DefaultValue;

                foreach (DataRow row in paramsTable.Rows)
                {
                    switch (row["Name"].ToString())
                    {
                        case "n":
                            n = int.Parse(row["Value"].ToString());
                            break;
                        case "i":
                            i = int.Parse(row["Value"].ToString());
                            break;

                    }
                }
                RepresentationCheck represent = new RepresentationCheck();
                statistics stat = new statistics();
                SigmaTable.Rows[i][0] = n;
                for (int type_k = 0; type_k < 5; type_k++)
                {
                    double[] k = represent.TableValues(K_Table, type_k);
                    double sigma = stat.standart(k);

                    SigmaTable.Rows[i][type_k+1] = sigma;
                }

            }

            public DataSet Results
            {
                get { return resultSet; }
            }
        }
    }
}



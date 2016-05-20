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
            kTable.Columns.Add("K1");
            kTable.Columns.Add("K2");
            kTable.Columns.Add("K3");
            kTable.Columns.Add("K4");
            kTable.Columns.Add("K5");

            DataTable paramsTable = data.Tables["\\params"];
            int N = 20;
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
            
            DataTable X_Table = ctx.InitialData.Tables["FlowSequence"];

            List<double> k1 = CoefAvg(X_Table, new statistics(), N);

            for (int i = 0; i < k1.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["k1"] = k1[i];
                kTable.Rows.Add(row);
            }
            List<double> k2 = CoefSigma(X_Table, new statistics(), N);

            for (int i = 0; i < k2.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["k2"] = k2[i];
                kTable.Rows.Add(row);
            }
            List<double> k3 = Coef_Cv(X_Table, new statistics(), N);

            for (int i = 0; i < k3.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["k3"] = k3[i];
                kTable.Rows.Add(row);
            }
            List<double> k4 = Coef_Cs(X_Table, new statistics(), N);

            for (int i = 0; i < k4.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["k4"] = k4[i];
                kTable.Rows.Add(row);
            }
            List<double> k5 = Coef_Eta(X_Table, new statistics(), N);

            for (int i = 0; i < k5.Count; i++)
            {
                DataRow row = kTable.NewRow();
                row["k5"] = k5[i];
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
        [Parameter("type_k", 0, typeof(Int32))]

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
                DataTable K_Table = ctx.InitialData.Tables["Coefficient"];
                pTable.Columns.Add("p");
            //   pTable.Columns.Add("k");
                RepresentationCheck represent = new RepresentationCheck();

                DataTable paramsTable = data.Tables["\\params"];

                var attrs = typeof(Probability).GetCustomAttributes<ParameterAttribute>();

                int type_k = (int)attrs.First((param) => { return param.Name == "type_k"; }).DefaultValue;

                foreach (DataRow row in paramsTable.Rows)
                {
                    switch (row["Name"].ToString())
                    {
                        case "type_k":
                            type_k = int.Parse(row["Value"].ToString());
                            break;
                    }
                }
                statistics stat = new statistics();
                double [] k = represent.TableValues(K_Table,type_k);
                k = stat.sort_shell(k, k.Length);
                double [] p = stat.probability(k);
                for (int i = 0; i < p.Length; i++)
                       {
                           DataRow row = pTable.NewRow();
                           row["p"] = p[i];
                           pTable.Rows.Add(row);
                       }
        /*        for (int i = 0; i < k.Length; i++)
            {
                DataRow row = pTable.NewRow();
                row["k"] = k[i];
                pTable.Rows.Add(row);
            }*/
            resultSet.Tables.Add(pTable);
            }
            
            public DataSet Results
            {
                get { return resultSet; }
            }


        }

        [Parameter("type_k", 0, typeof(Int32))]
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
                DataTable K_Table = ctx.InitialData.Tables["Coefficient"];
                DataTable SigmaTable = new DataTable() { TableName = "Standarts" };
                SigmaTable.Columns.Add("K");
                SigmaTable.Columns.Add("Sigma");
                DataTable paramsTable = data.Tables["\\params"];
                var attrs = typeof(Probability).GetCustomAttributes<ParameterAttribute>();
                int type_k = (int)attrs.First((param) => { return param.Name == "type_k"; }).DefaultValue;
                RepresentationCheck represent = new RepresentationCheck();
                statistics stat = new statistics();
                foreach (DataRow row in paramsTable.Rows)
                {
                    switch (row["Name"].ToString())
                    {
                        case "type_k":
                            type_k = int.Parse(row["Value"].ToString());
                            break;
                    }
                }
                double[] k = represent.TableValues(K_Table, type_k);
                double sigma = stat.standart(k);

            }

            public DataSet Results
            {
                get { return resultSet; }
            }
        }
    }
}



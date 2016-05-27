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
    [Parameter("r", 0.1, typeof(double))]
    [Parameter("size", 1000, typeof(int))]
    [Name("Однородность")]
    public class Student : IAlgorithm
    {

        private DataSet data;
        private DataSet resultSet;
        public double[] t;
        public int index_size = 5;
        public int lam_len = 5;
        public int size;
        public double n, r;
        public double[,] series;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();
            DataTable tTable = new DataTable() { TableName = "t" };
            tTable.Columns.Add("const");
            tTable.Columns.Add("linear-1");
            tTable.Columns.Add("linear-2");
            tTable.Columns.Add("parabola-1");
            tTable.Columns.Add("parabola-2");

            DataTable paramsTable = data.Tables["params"];
            var attrs = typeof(Transformation).GetCustomAttributes<ParameterAttribute>();

            n = 0;
            size = 0;
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
                    case "r":
                        r = double.Parse(row["Value"].ToString());
                        break;
                    case "size":
                        size = int.Parse(row["Value"].ToString());
                        break;
                }
            }

            DataTable series_Table = ctx.Data.Tables["ModifiedSequence"];
            double[,] series = new double[index_size, size];
            for (int index = 0; index < index_size; index++)
                series = GetTableValues(series_Table, series, index);


            t = new double[index_size];
            int n1 = Convert.ToInt32(n * size);
            int n2 = size - n1;
            double[] mods1 = new double[n1 / lam_len];
            double[] mods2 = new double[n2 / lam_len];
            //double[,] t_r = new double[11,index_size];      //0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0

            for (int index = 0; index < index_size; index++)
                t[index] = getT(series, n1, index);

                DataRow row1 = tTable.NewRow();
                row1["const"] = t[0];
                row1["linear-1"] = t[1];
                row1["linear-2"] = t[2];
                row1["parabola-1"] = t[3];
                row1["parabola-2"] = t[4];
                tTable.Rows.Add(row1);
                resultSet.Tables.Add(tTable);

            //получили, например, для r=0.1 различные t при разбиении 50/50, различных формах кривых и лямбдах.
            //потом можно взять для r=0.1 разбиение 40/60, 20/80...
            //а затем брать разные r
        }


        public double[,] GetTableValues(DataTable X_Table, double[,] x, int col)
        {
            int i = 0;
            DataColumn column = X_Table.Columns[col];
            //double[,] x = new double[index_size, size];
            foreach (DataRow row in X_Table.Rows)
            {
                x[col, i] = Convert.ToDouble(row[column]);
                if (i < size) i++;
            }
            return x;
        }


        public double [] tempMods(double[,] mods, double[] tempMods, int i0, int n_, int index)
        {
            int k = 0;
            for (int i = i0; i < n_; i++)
                {
                    tempMods[k] = mods[index, i];
                    k++;
                }
            return tempMods;
        }

        public double getT(double[,] series, int n1, int index)
        {
            double t = 0; double u = 0;
            double[] dx;
            double[] dy;
            double mx, my, sigma_x, sigma_y;

            dx = new double[n1];
            dy = new double[size-n1];

            mx = 0; //х среднее
            for (int i = 0; i < n1; i++)
                mx += series[index, i];
            mx /= n1;

            my = 0; //у среднее
            for (int i = n1; i < size; i++)
                my += series[index, i];
            my /= size-n1;

            sigma_x = 0; //sigma_x^2
            for (int i = 0; i < n1; i++)
                sigma_x += Math.Pow(series[index, i] - mx, 2);
            sigma_x /= n1;

            sigma_y = 0; //sigma_y^2
            for (int i = n1; i < size; i++)
                sigma_y += Math.Pow(series[index, i] - mx, 2);
            sigma_y /= size-n1;

            t = Math.Abs(mx - my);
            t /= Math.Sqrt((n1 - 1) * sigma_x + (size - n1 - 1) * sigma_y);
            u = n1 * (size-n1);
            u *= (size - 2);
            u /= size;
            t *= Math.Sqrt(u);
            return t;
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

    }
}

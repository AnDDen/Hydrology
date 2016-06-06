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
        public double[,] t;
        public int index_size = 4;
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
            //tTable.Columns.Add("linear-2");
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
            double[,] series = new double[index_size, size*lam_len];
            for (int index = 0; index < index_size; index++)
                series = GetTableValues(series_Table, series, index);


            t = new double[index_size, lam_len];
            int n1 = Convert.ToInt32(n * size * lam_len);
            int n2 = size * lam_len - n1;
            double[] mods1 = new double[n1 / lam_len];
            double[] mods2 = new double[n2 / lam_len];
            //double[,] t_r = new double[11,index_size];      //0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0

            //for (int index = 0; index < index_size; index++)
                //t[index] = getT(series, n1, index);

            for (int index = 0; index < index_size; index++)
            {
                for (int j = 0; j < lam_len; j++)
                {
                    mods1 = tempMods(series, mods1, j * n1 / lam_len, (j + 1) * n1 / lam_len, index);
                    mods2 = tempMods(series, mods2, j * n2 / lam_len, (j + 1) * n2 / lam_len, index);
                    t[index, j] = getT(mods1, mods2);
                }
            }

            for (int j = 0; j < lam_len; j++)
            {
                DataRow row1 = tTable.NewRow();
                row1["const"] = t[0, j];
                row1["linear-1"] = t[1, j];
                //row1["linear-2"] = t[2, j];
                row1["parabola-1"] = t[2, j];
                row1["parabola-2"] = t[3, j];
                tTable.Rows.Add(row1);
                
            }

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
                if (i < size*lam_len) i++;
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

        public double getT(double[] mods1, double[] mods2)
        {
            double t = 0; double u = 0;
            double[] dx;
            double[] dy;
            double mx, my, sigma_x, sigma_y;
            int n1 = mods1.Length;
            int n2 = mods2.Length;

            dx = new double[n1];
            dy = new double[n2];

            mx = 0; //х среднее
            for (int i = 0; i < n1; i++)
                mx += mods1[i];
            mx /= n1;

            my = 0; //у среднее
            for (int i = 0; i < n2; i++)
                my += mods2[i];
            my /= n2;

            sigma_x = 0; //sigma_x^2
            for (int i = 0; i < n1; i++)
                sigma_x += Math.Pow(mods1[i] - mx, 2);
            sigma_x /= n1;

            sigma_y = 0; //sigma_y^2
            for (int i = 0; i < n2; i++)
                sigma_y += Math.Pow(mods2[i] - mx, 2);
            sigma_y /= n2;

            t = Math.Abs(mx - my);
            t /= Math.Sqrt((n1 - 1) * sigma_x + (n2 - 1) * sigma_y);
            u = n1 * n2;
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreInterfaces;
using System.Reflection;
using Helpers;

namespace UniformityCheck
{
    [Parameter("Source1", "FlowSequence", typeof(string))]
    [Parameter("Source2", "ModifiedSequence", typeof(string))]
    [Name("Однородность по критерию Стьюдента")]
    public class StudentUniformity : IAlgorithm
    {

        private DataSet data;
        private DataSet resultSet;
        public double[,] t;
        public int index_size = 4;
        public int lam_len = 6;
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
            tTable.Columns.Add("StudentStatistics");
            //FlowSequence
            //ModifiedSequence
            double[] sequence1 = null;
            double[] sequence2 = null;

            DataTable paramsTable = data.Tables["params"];
            var attrs = typeof(StudentUniformity).GetCustomAttributes<ParameterAttribute>();

            string src1 = (string)attrs.First((param) => { return param.Name == "Source1"; }).DefaultValue;
            string src2 = (string)attrs.First((param) => { return param.Name == "Source2"; }).DefaultValue;

            var h = new ParamsHelper(data.Tables["params"]);
            var tableName = h.GetValue<string>("Source1");
            if (!string.IsNullOrEmpty(tableName))
                src1 = tableName;
            tableName = h.GetValue<string>("Source2");
            if (!string.IsNullOrEmpty(tableName))
                src2 = tableName;

            DataTable dtSrc1 = ctx.FindInHistory(src1);
            if (dtSrc1 == null)
                throw new Exception("В предыдущих алгоритмах таблица " + src1 + " не найдена.");
            DataTable dtSrc2 = ctx.FindInHistory(src2);
            if (dtSrc2 == null)
                throw new Exception("В предыдущих алгоритмах таблица " + src2 + " не найдена.");

            var n = Math.Min(dtSrc1.Rows.Count, dtSrc2.Rows.Count);
            sequence1 = new double[dtSrc1.Rows.Count];
            sequence2 = new double[dtSrc2.Rows.Count];
            GetTableValues(dtSrc1, sequence1, 0, dtSrc1.Rows.Count);
            GetTableValues(dtSrc2, sequence2, 0, dtSrc2.Rows.Count);
            var res = getT(sequence1, sequence2, dtSrc1.Rows.Count + dtSrc2.Rows.Count);

            var r = tTable.NewRow();
            r[0] = res;

            tTable.Rows.Add(r);

            resultSet.Tables.Add(tTable);
        }


        public void GetTableValues(DataTable X_Table, double[] x, int col, int n)
        {
            int i = 0;
            DataColumn column = X_Table.Columns[col];
            //double[,] x = new double[index_size, size];
            foreach (DataRow row in X_Table.Rows)
            {
                if (i == n)
                    return;
                x[i] = Convert.ToDouble(row[column]);
                i++;
            }
        }

        public double getT(double[] mods1, double[] mods2, int size)
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
                sigma_y += Math.Pow(mods2[i] - my, 2);
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

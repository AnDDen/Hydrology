using CoreInterfaces;
using Helpers;
using Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transformation
{
    [Parameter("Table Name", "FlowSequence", typeof(string))]
    [Parameter("SplitPart", 0.3, typeof(double))]
    [Name("Разбиение")]
    class Split : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            System.Diagnostics.Trace.WriteLine("Split run");

            resultSet = new DataSet();

            DataTable table1 = new DataTable() { TableName = "Split1" };
            table1.Columns.Add("X");

            DataTable table2 = new DataTable() { TableName = "Split2" };
            table2.Columns.Add("X");



            var h = new ParamsHelper(data.Tables["params"]);
            var tableName = h.GetValue<string>("Table Name");
            //var attrs = typeof(Split).GetCustomAttributes<ParameterAttribute>();
            DataTable src = ctx.Data.Tables[tableName];

            if (src == null)
            {
                throw new Exception(string.Format("Алгоритм источник не имеет таблицы {0}", tableName));
            }

            var splitPart = h.GetValue<double>("SplitPart");

            int cnt = (int)(src.Rows.Count * splitPart);
            int i = 0;
            foreach (DataRow r in src.Rows)
            {
                if (i < cnt)
                {
                    DataRow rr = table1.NewRow();
                    rr[0] = r[0];
                    table1.Rows.Add(rr);
                }
                else
                {
                    DataRow rr = table2.NewRow();
                    rr[0] = r[0];
                    table2.Rows.Add(rr);
                }
                i++;               
            }

            resultSet.Tables.Add(table1);
            resultSet.Tables.Add(table2);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

    }
}

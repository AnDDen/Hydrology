using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TestAlgorithms
{
    [Name("TEST 1")]
    [Parameter("start", 0, typeof(int))]
    [Parameter("end", 4, typeof(int))]
    [Parameter("step", 1, typeof(int))]
    public class TestAlgorithm1 : IAlgorithm
    {
        private DataSet results;

        private string[] values = new string[5];
        private int start = 0, end = 4, step = 1;

        [Argument]
        public int Start { get; set; }

        [Argument(4)]
        public int End { get; set; }

        [Argument(1)]
        public int Step { get; set; }

        public DataSet Results
        {
            get { return results; }
        }

        public void Init(DataSet data)
        {
            DataTable tbl = data.Tables["big_optsource"];
            for (int i = 0; i < values.Length; i++)
                values[i] = tbl.Rows[0][i].ToString();

            DataTable p = data.Tables["params"];
            foreach (DataRow row in p.Rows)
            {
                switch (row["Name"].ToString())
                {
                    case "start":
                        start = int.Parse(row["Value"].ToString());
                        break;
                    case "end":
                        end = int.Parse(row["Value"].ToString());
                        break;
                    case "step":
                        step = int.Parse(row["Value"].ToString());
                        break;
                }
            }
        }

        public void Run(IContext ctx)
        {
            results = new DataSet();
            DataTable table = new DataTable("TEST_1_OUT");
            for (int i = start; i <= end; i += step)
            {
                table.Columns.Add("col" + i.ToString());
            }
            DataRow row = table.NewRow();
            for (int i = start; i <= end; i += step)
            {             
                row["col" + i.ToString()] = values[i];
            }
            table.Rows.Add(row);
            results.Tables.Add(table);
        }
    }
}

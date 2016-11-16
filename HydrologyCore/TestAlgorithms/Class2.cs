using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("TEST LOOP")]
    [Parameter("LoopName", "", typeof(string))]
    [Parameter("NodeName", "", typeof(string))]
    [Parameter("TableName", "", typeof(string))]
    public class TestAlgorithm2 : IAlgorithm
    {
        private DataSet results;

        private string loopName;
        private string nodeName;
        private string tableName;

        public DataSet Results
        {
            get { return results; }
        }

        public void Init(DataSet data)
        {
            DataTable p = data.Tables["params"];
            foreach (DataRow row in p.Rows)
            {
                switch (row["Name"].ToString())
                {
                    case "LoopName":
                        loopName = row["Value"].ToString();
                        break;
                    case "NodeName":
                        nodeName = row["Value"].ToString();
                        break;
                    case "TableName":
                        tableName = row["Value"].ToString();
                        break;
                }
            }
        }

        public void Run(IContext ctx)
        {
            results = new DataSet();
            DataTable table = new DataTable("TEST_LOOP_RESULTS_OUT");
            table.Columns.Add("n");
            table.Columns.Add("value");
            table.Columns.Add("col_count");
            var tbl = ctx.GetData(loopName).Tables[0];
            DataRow r = table.NewRow();
            r["n"] = -1;
            r["value"] = tbl.Rows.Count;
            table.Rows.Add(r);
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                string varVal = tbl.Rows[i]["value"].ToString();
                DataSet nodeResult = (DataSet)tbl.Rows[i][nodeName];
                DataTable t = nodeResult.Tables[tableName];
                int colCount = t.Columns.Count;
                DataRow row = table.NewRow();
                row["n"] = i;
                row["value"] = varVal;
                row["col_count"] = colCount;
                table.Rows.Add(row);
            }
            results.Tables.Add(table);
        }
    }
}

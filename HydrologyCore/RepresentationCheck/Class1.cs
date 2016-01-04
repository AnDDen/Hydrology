using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlgorithmInterface;

namespace RepresentationCheck
{
    public class RepresentationCheck : IAlgorithm
    {
        DataSet data;
        DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();
            DataTable table = new DataTable();
            table.Columns.Add("col1");
            table.Columns.Add("col2");
            table.Columns.Add("col3");
            DataRow row = table.NewRow();
            row["col1"] = 10;
            row["col2"] = 15;
            row["col3"] = 17.5;
            table.Rows.Add(row);
            resultSet.Tables.Add(table);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }
    }

    public class Statistics : IAlgorithm
    {
        DataSet data;
        DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            resultSet = new DataSet();
            DataTable table = new DataTable();
            table.Columns.Add("col1");
            table.Columns.Add("col2");
            DataRow row = table.NewRow();
            row["col1"] = 14;
            row["col2"] = 12;
            table.Rows.Add(row);
            resultSet.Tables.Add(table);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }
    }
}

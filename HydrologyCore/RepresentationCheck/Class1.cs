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
    [Parameter("CS", 1.0, typeof(Double))]
    [Parameter("CV", 2.0, typeof(Double))]
    [Parameter("N", 1000, typeof(Int32))]
    [Name("Репрезентативность")]
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
            var d = ctx.Data.Tables[0];
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

    [Name("Статистика")]
    public class Statistics : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;
        public IDictionary<string, object> Parameters { get; set; }

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

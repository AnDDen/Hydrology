using CoreInterfaces;
using System.Linq;
using System.Data;
using Statistics;
using Helpers;
using System;

namespace Transformation
{ 
    [Parameter("Table Name", "", typeof(string))]
    [Parameter("Destination", 0, typeof(int))]
      [Parameter("Save", "0", typeof(string))]
  [Name("СортВсехКолонок")]
    class SortEachColumn : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            System.Diagnostics.Trace.WriteLine("SortEachColumn run");

            resultSet = new DataSet();


            var h = new ParamsHelper(data.Tables["params"]);
            var tableName = h.GetValue<string>("Table Name");
            var dest = h.GetValue<int>("Destination");
            //var attrs = typeof(Split).GetCustomAttributes<ParameterAttribute>();
            DataTable src = ctx.Data.Tables[tableName];
                
            if (src == null)
            {
                throw new Exception(string.Format("Алгоритм источник не имеет таблицы {0}", tableName));
            }

            DataTable table = src.Copy();
            table.TableName = "SortEachColumn";

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sortColumn(table, i, dest);
            }

            resultSet.Tables.Add(table);
        }

        private void sortColumn(DataTable table, int col, int dest)
        {
            if (table.Rows.Count == 0)
                return;
            object[] arr = new object[table.Rows.Count];
                 
            for (int i = 0; i < table.Rows.Count; i++)
            {
                arr[i] = table.Rows[i][col];
            }
            if (dest == 0)
            {
                arr = arr.OrderBy(a => a).ToArray();
            }
            else
            {
                arr = arr.OrderByDescending(a => a).ToArray();
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i][col] = arr[i];
            }
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

    }
}

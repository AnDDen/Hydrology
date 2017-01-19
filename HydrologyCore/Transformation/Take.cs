using CoreInterfaces;
using System.Reflection;
using System.Data;
using Statistics;
using Helpers;
using System;

namespace Transformation
{ 
    [Parameter("Table Name", "FlowSequence", typeof(string))]
    [Parameter("GroupSize", 20, typeof(int))]
    [Parameter("Group", 1, typeof(double))]
    [Parameter("Save", "0", typeof(string))]
    [Name("Взять")]
    class Take : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            System.Diagnostics.Trace.WriteLine("Take run");

            resultSet = new DataSet();

            DataTable table1 = new DataTable() { TableName = "Take" };
            table1.Columns.Add("X");


            var h = new ParamsHelper(data.Tables["params"]);
            var tableName = h.GetValue<string>("Table Name");
            var groupSize = h.GetValue<int>("GroupSize");
            var group = h.GetValue<int>("Group");
            //var attrs = typeof(Split).GetCustomAttributes<ParameterAttribute>();
            DataTable src = ctx.Data.Tables[tableName];

            if (src == null)
            {
                throw new Exception(string.Format("Алгоритм источник не имеет таблицы {0}", tableName));
            }

            var splitPart = h.GetValue<double>("SplitPart");

            for (int i = (group - 1) * groupSize; (i < group * groupSize) && (i < src.Rows.Count); i++)
            {
                DataRow rr = table1.NewRow();
                rr[0] = src.Rows[i][0];
                table1.Rows.Add(rr);
            }

            resultSet.Tables.Add(table1);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

    }
}

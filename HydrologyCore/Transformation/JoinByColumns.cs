﻿using CoreInterfaces;
using System.Reflection;
using System.Data;
using Statistics;
using Helpers;
using System;

namespace Transformation
{
    [Parameter("Algorythm", "StudentUniformity", typeof(string))]
    [Parameter("Table Name", "t", typeof(string))]
    [Parameter("Start", "0", typeof(int))]
    [Parameter("Count", "1000", typeof(int))]
    [Parameter("Save", "1", typeof(string))]
    [Name("Соед по колонкам")]
    class JoinByColumns : IAlgorithm
    {
        private DataSet data;
        private DataSet resultSet;

        public void Init(DataSet data)
        {
            this.data = data;
        }

        public void Run(IContext ctx)
        {
            System.Diagnostics.Trace.WriteLine("JoinByRows run");

            resultSet = new DataSet();


            var h = new ParamsHelper(data.Tables["params"]);
            var tableName = h.GetValue<string>("Table Name");
            var algorythm = h.GetValue<string>("Algorythm");
            var start = h.GetValue<int>("Start");
            var count = h.GetValue<int>("Count");

            DataTable table = new DataTable() { TableName = "JoinByColumns" };
            var loopTable = ctx.Data.Tables[0];

            int columnShift = 0;

            for (var i = 0; i <  loopTable.Rows.Count; i++)
            {
                var ds = loopTable.Rows[i][algorythm] as DataSet;
                var t = ds.Tables[tableName];
                if (t == null)
                {
                    throw new Exception(string.Format("Последний алгоритм цикла {0} не имеет таблицы {1}", loopTable.TableName, tableName));
                }

                for (int ii = 0; ii < t.Columns.Count; ii++)
                {
                    table.Columns.Add(new DataColumn(loopTable.Rows[i]["value"] + " " + t.Columns[ii].ColumnName, t.Columns[ii].DataType));
                }

                for (int k = 0; k < t.Rows.Count; k++)
                {
                    DataRow rr = null;
                    if (table.Rows.Count <= k)
                    {
                        rr = table.NewRow();
                        table.Rows.Add(rr);
                    }
                    else
                    {
                        rr = table.Rows[k];
                    }

                    for (int ii = 0; ii < t.Columns.Count; ii++)
                    {
                        rr[ii + columnShift] = t.Rows[k][ii];
                    }
                }

                columnShift += t.Columns.Count;
            }

            resultSet.Tables.Add(table);
        }

        public DataSet Results
        {
            get { return resultSet; }
        }

    }
}

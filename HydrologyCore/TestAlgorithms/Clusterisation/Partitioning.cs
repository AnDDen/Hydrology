using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms.Clusterisation
{
    [Name("Разбиение")]
    public class Partitioning : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Output("Количество групп")]
        public int N { get; set; }

        [Output("Размеченная таблица")]
        public DataTable ResultTable { get; set; }        

        public void Run()
        {
            ResultTable = Table.Copy();
            ResultTable.Columns.Add("cluster");
            N = ResultTable.Rows.Count;
            for (int i = 0; i < N; i++)
            {
                ResultTable.Rows[i]["cluster"] = i;
            }

        }
    }
}

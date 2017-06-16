using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Создание таблицы")]
    public class NewTable : IAlgorithm
    {
        [Input("Имя таблицы")]
        public string Name { get; set; }

        [Input("Имена столбцов", Description = "Имена столбцов через запятую")]
        public string ColNames { get; set; }

        [Output("Таблица")]
        public DataTable Table { get; set; }

        public void Run()
        {
            string[] colNames = ColNames.Split(',').Select(x => x.Trim()).ToArray();
            int N = colNames.Count();
            Table = new DataTable(Name);
            for (int i = 0; i < colNames.Count(); i++)
                Table.Columns.Add(colNames[i]);
        }
    }
}

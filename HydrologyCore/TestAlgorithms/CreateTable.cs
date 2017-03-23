using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Создание таблицы")]
    public class CreateTable : IAlgorithm
    {
        [Input("Имя таблицы")]
        public string Name { get; set; }

        [Input("Число столбцов")]
        public int ColCount { get; set; }

        [Input("Число строк")]
        public int RowCount { get; set; }

        [Output("Таблица")]
        public DataTable Table { get; set; }

        public void Run()
        {
            Table = new DataTable(Name);
            for (int i = 1; i <= ColCount; i++)
                Table.Columns.Add("col " + i);
            for (int j = 0; j < RowCount; j++)
                Table.Rows.Add(Table.NewRow());
        }
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Новая строка таблицы")]
    public class NewRow : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Output("Результирующая таблица")]
        public DataTable ResultTable { get; set; }

        [Output("Номер строки")]
        public int RowNum { get; set; }

        public void Run()
        {
            Table.Rows.Add(Table.NewRow());
            ResultTable = Table;
            RowNum = Table.Rows.Count - 1;
        }
    }
}

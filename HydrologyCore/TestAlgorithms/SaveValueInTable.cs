using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Запись числа в таблицу")]
    public class SaveValueInTable : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Input("Номер строки")]
        public int RowNum { get; set; }

        [Input("Номер столбца")]
        public int ColNum { get; set; }

        [Input("Значение")]
        public double Value { get; set; }

        [Output("Измененная таблица")]
        public DataTable OutTable { get; set; }

        public void Run()
        {
            Table.Rows[RowNum][ColNum] = Value;
            OutTable = Table;
        }
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Конвертер DataTable в массив")]
    public class DataTableArrayConverter : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Input("Номер столбца", 0)]
        public int ColumnIndex { get; set; }

        [Output("Массив")]
        public double[] Array { get; set; }

        public void Run()
        {
            Array = new double[Table.Rows.Count];
            for (int i = 0; i < Table.Rows.Count; i++)
            {
                Array[i] = double.Parse(Table.Rows[i][ColumnIndex].ToString());
            }
        }
    }
}

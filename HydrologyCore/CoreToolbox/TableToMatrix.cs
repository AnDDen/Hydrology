using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Конвертация таблицы в матрицу")]
    public class TableToMatrix : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Output("Матрица")]
        public double[,] Matrix { get; set; }

        public void Run()
        {
            Matrix = new double[Table.Rows.Count, Table.Columns.Count];
            for (int i = 0; i < Table.Rows.Count; i++)
                for (int j = 0; j < Table.Columns.Count; j++)
                {
                    Matrix[i, j] = Convert.ToDouble(Table.Rows[i][j], System.Globalization.CultureInfo.InvariantCulture);
                }
        }
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Сохранение таблицы")]
    public class SaveTable : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Output("Таблица")]
        public DataTable OutTable { get; set; }

        public void Run()
        {
            OutTable = Table.Copy();
        }
    }
}

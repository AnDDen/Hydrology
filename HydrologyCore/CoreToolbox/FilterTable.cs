using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Фильтрация таблицы")]
    public class FilterTable : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Input("Фильтр")]
        public string Filter { get; set; }

        [Output("Отфильтровання таблица")]
        public DataTable Filtered { get; set; }

        public void Run()
        {
            Filtered = Table.Select(Filter).CopyToDataTable();
        }
    }
}

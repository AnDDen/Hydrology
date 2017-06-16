using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms.Clusterisation
{
    [Name("Выделение группы из выборки")]
    public class GetCluster : IAlgorithm
    {
        [Input("Номер группы")]
        public int I { get; set; }

        [Input("Выборка")]
        public DataTable Table { get; set; }        

        [Output("Группа")]
        public DataTable Result { get; set; }

        public void Run()
        {
            var rows = Table.Select($"cluster = '{I}'");
            Result = rows.CopyToDataTable();
        }
    }
}

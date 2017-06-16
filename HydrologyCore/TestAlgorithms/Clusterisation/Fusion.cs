using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms.Clusterisation
{
    [Name("Слияние")]
    public class Fusion : IAlgorithm
    {
        [Input("Таблица")]
        public DataTable Table { get; set; }

        [Input("Номер первой группы")]
        public int C1 { get; set; }

        [Input("Номер второй группы")]
        public int C2 { get; set; }

        [Output("Результирующая таблица")]
        public DataTable ResultTable { get; set; }

        public void Run()
        {
            int c = Math.Min(C1, C2);
            int cm = Math.Max(C1, C2);

            ResultTable = Table;
            foreach (DataRow r in ResultTable.Rows)
            {
                int cluster = Convert.ToInt32(r["cluster"]);
                if (cluster == cm)
                    r["cluster"] = c;
                else if (cluster > cm)
                    r["cluster"] = cluster - 1;
            }
        }
    }
}

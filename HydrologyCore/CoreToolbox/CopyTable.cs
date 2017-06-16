using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Копирование таблицы")]
    public class CopyTable : IAlgorithm
    {
        public DataTable Table { get; set; }

        public DataTable Result { get; set; }

        public void Run()
        {
            Result = Table.Copy();
        }
    }
}

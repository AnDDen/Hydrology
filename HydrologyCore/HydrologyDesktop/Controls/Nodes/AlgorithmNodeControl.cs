using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyDesktop.Controls
{
    public class AlgorithmNodeControl : NodeControl
    {
        public Type AlgorithmType { get; set; }

        public DataTable ParamsTable { get; set; }

        public string InitPath { get; set; }
    }
}

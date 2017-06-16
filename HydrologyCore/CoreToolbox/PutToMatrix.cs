using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Положить в матрицу")]
    class PutToMatrix : IAlgorithm
    {
        [Input("Строка")]
        public int I { get; set; }

        [Input("Столбец")]
        public int J { get; set; }

        [Input("Значение")]
        public double Value { get; set; }

        [Input("Матрица")]
        public double[,] Matrix { get; set; }

        [Output("Результирующая матрица")]
        public double[,] Result { get; set; }

        public void Run()
        {
            Result = Matrix;
            Result[I, J] = Value;
        }
    }
}

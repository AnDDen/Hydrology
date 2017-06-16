using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Создание матрицы вещественных чисел")]
    public class CreateMatrix : IAlgorithm
    {
        [Input("Число строк")]
        public int N { get; set; }

        [Input("Число столбцов")]
        public int M { get; set; }

        [Output("Матрица")]
        public double[,] Matrix { get; set; }

        public void Run()
        {
            Matrix = new double[N, N];            
        }
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Поэлементарная операция")]
    class MatrixElementOperations : IAlgorithm
    {
        [Input("Матрица")]
        public double[,] A { get; set; }

        [Input("Второй операнд")]
        public double B { get; set; }

        [Input("Операция", Description = "+ - * /")]
        public string Operation { get; set; }

        [Output("Результат")]
        public double[,] C { get; set; }

        public void Run()
        {
            switch (Operation)
            {
                case "+":
                    C = ForEachElement(A, x => x + B);
                    return;
                case "-":
                    C = ForEachElement(A, x => x - B);
                    return;
                case "*":
                    C = ForEachElement(A, x => x * B);
                    return;
                case "/":
                    C = ForEachElement(A, x => x / B);
                    return;
            }
        }

        private double[,] ForEachElement(double[,] matrix, Func<double, double> func)
        {
            double[,] result = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    result[i, j] = func(matrix[i, j]);
            return result; 
        }
    }
}

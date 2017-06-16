using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Минимальный/максимальный элемент матрицы")]
    public class MatrixOperation : IAlgorithm
    {
        [Input("Матрица")]
        public double[,] Matrix { get; set; }

        [Input("Операция")]
        public string Operation { get; set; }

        [Output("Результат")]
        public double Result { get; set; }

        [Output("Строка")]
        public int Row { get; set; }

        [Output("Столбец")]
        public int Column { get; set; }

        public void Run()
        {
            bool hasResult = false;
            for (int i = 0; i < Matrix.GetLength(0); i++)
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (!hasResult)
                    {
                        Result = Matrix[i, j];
                        Row = i;
                        Column = j;
                        hasResult = true;
                    }
                    else
                    {
                        bool check = false;
                        switch (Operation.ToLower())
                        {
                            case "min":
                                check = Matrix[i, j] < Result;
                                break;
                            case "max":
                                check = Matrix[i, j] > Result;
                                break;
                        }

                        if (check)
                        {
                            Result = Matrix[i, j];
                            Row = i;
                            Column = j;
                        }
                    }
                }
            
            
        }
    }
}

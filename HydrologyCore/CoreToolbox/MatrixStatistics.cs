using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Характеристика матрицы")]
    public class MatrixStatistics : IAlgorithm
    {
        [Input("Матрица")]
        public double[,] Matrix { get; set; }

        [Input("Статистическая характеристика")]
        public string Param { get; set; }

        [Input("Измерение матрицы")]
        public int Dimention { get; set; }

        [Output("Результат")]
        public double[] Result { get; set; }

        public void Run()
        {
            switch (Param)
            {
                case "mean":
                    Result = GetValue(Matrix, Dimention, ArrayStatistics.Mean);
                    break;
                case "std":
                    Result = GetValue(Matrix, Dimention, ArrayStatistics.Std);
                    break;
                case "variation":
                    Result = GetValue(Matrix, Dimention, ArrayStatistics.Variation);
                    break;
                case "skewness":
                    Result = GetValue(Matrix, Dimention, ArrayStatistics.Skewness);
                    break;
                case "eta":
                    Result = GetValue(Matrix, Dimention, x => ArrayStatistics.Skewness(x) / ArrayStatistics.Variation(x));
                    break;
                default:
                    throw new ArgumentException($"Статистическая характеристика {Param ?? ""} не поддерживается");
            }
        }

        public delegate double Func(double[] array);

        public double[] GetValue(double[,] m, int d, Func f)
        {
            int N = m.GetLength(d);
            long M = m.LongLength / N;
            double[] res = new double[N];
            for (int i = 0; i < N; i++)
            {
                double[] arr = new double[M];
                for (int j = 0; j < M; j++)
                    arr[j] = d == 0 ? m[i, j] : m[j, i];
                res[i] = f(arr);
            }
            return res;
        }
    }
}

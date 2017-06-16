using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Характеристика массива")]
    public class ArrayStatistics : IAlgorithm
    {
        [Input("Массив")]
        public double[] Array { get; set; }

        [Input("Статистическая характеристика")]
        public string Param { get; set; }

        [Output("Результат")]
        public double Result { get; set; }

        public void Run()
        {
            switch (Param)
            {
                case "mean":
                    Result = Mean(Array);
                    break;
                case "std":
                    Result = Std(Array);
                    break;
                case "variation":
                    Result = Variation(Array);
                    break;
                case "skewness":
                    Result = Skewness(Array);
                    break;
                case "eta":
                    Result = Skewness(Array) / Variation(Array);
                    break;
                default:
                    throw new ArgumentException($"Статистическая характеристика {Param ?? ""} не поддерживается");
            }
        }

        public static double Mean(double[] array)
        {
            return array.Sum() / array.Length;
        }

        public static double MomentK(double[] array, int k)
        {
            double mean = Mean(array);
            return Mean(array.Select(x => Math.Pow(x - mean, k)).ToArray());
        }

        public static double Std(double[] array)
        {
            int N = array.Length;
            return Math.Sqrt(MomentK(array, 2) * N / (N - 1));
        }

        public static double Variation(double[] array)
        {
            return Std(array) / Mean(array);
        }

        public static double Skewness(double[] array)
        {
            return MomentK(array, 3) / Math.Pow(Std(array), 3);
        }
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Модификация стока")]
    public class ModifyFlow : IAlgorithm
    {
        [Input("Исходная последовательность")]
        public double[,] OriginalFlow { get; set; }

        [Input("Изъятие")]
        public double Lambda { get; set; }

        [Input("Сценарий модификации")]
        public string Scenario { get; set; }

        [Output("Модифицированная последовательность")]
        public double[,] ResultFlow { get; set; }

        public void Run()
        {
            switch (Scenario)
            {
                case "const":
                    ResultFlow = Modify(OriginalFlow, Lambda, (q, i, N, l) => l * q);
                    break;
                case "line":
                    ResultFlow = Modify(OriginalFlow, Lambda, (q, i, N, l) => i / N * l * q);
                    break;
                case "parabola1":
                    ResultFlow = Modify(OriginalFlow, Lambda, (q, i, N, l) => (i / N) * (i / N) * l * q);
                    break;
                case "parabola2":
                    ResultFlow = Modify(OriginalFlow, Lambda, (q, i, N, l) => (1 - (1 - i / N) * (1 - i / N)) * l * q);
                    break;
                default:
                    throw new ArgumentException($"Сценарий модификации {Scenario ?? ""} не поддерживается");
            }
        }

        public delegate double ModifierFunc(double x, double i, double N, double lambda);

        public double[,] Modify(double[,] original, double lambda, ModifierFunc modifier)
        {
            int N = original.GetLength(0);
            int M = original.GetLength(1);
            double avg = original.Cast<double>().Sum() / original.Length;
            double[,] result = new double[N, M];
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    result[i, j] = original[i, j] - modifier(avg, i, N, lambda);
                    if (result[i, j] < 0)
                        result[i, j] = 0;
                }
            }
            return result;
        }
    }
}

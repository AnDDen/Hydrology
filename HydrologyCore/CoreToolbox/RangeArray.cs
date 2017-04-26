using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Последовательность", "Массивы")]
    public class RangeArray : IAlgorithm
    {
        [Input("Начальное значение")]
        public double StartValue { get; set; }

        [Input("Конечное значение")]
        public double EndValue { get; set; }

        [Input("Шаг")]
        public double Step { get; set; }

        [Output("Массив")]
        public double[] Array { get; set; }

        public void Run()
        {
            int n = (int)Math.Truncate((EndValue - StartValue) / Step) + 1;
            Array = new double[n];
            for (int i = 0; i < n; i++)
                Array[i] = StartValue + i * Step;
        }
    }
}

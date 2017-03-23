using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace TestAlgorithms
{
    [Name("Среднее арифметическое")]
    public class ArrayMeanAlgorithm : IAlgorithm
    {
        [Input("Массив")]
        public double[] Array { get; set; }

        [Output("Среднее")]
        public double Mean { get; set; }

        public void Run()
        {
            double sum = 0;
            foreach (var a in Array)
                sum += a;
            Mean = sum / Array.Length;
        }
    }
}

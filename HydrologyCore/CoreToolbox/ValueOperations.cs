using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    [Name("Арифметическая операция")]
    class ValueOperations : IAlgorithm
    {
        [Input("Первый операнд")]
        public double A { get; set; }

        [Input("Второй операнд")]
        public double B { get; set; }

        [Input("Операция", Description = "+ - * /")]
        public string Operation { get; set; }

        [Output("Результат")]
        public double C { get; set; }

        public void Run()
        {
            switch (Operation)
            {
                case "+":
                    C = A + B;
                    return;
                case "-":
                    C = A - B;
                    return;
                case "*":
                    C = A * B;
                    return;
                case "/":
                    C = A / B;
                    return;
            }
        }
    }
}

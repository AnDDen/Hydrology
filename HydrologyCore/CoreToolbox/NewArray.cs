using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreToolbox
{
    // todo : in core implement support for generic types

    public class NewArray<T> : IAlgorithm
    {
        [Input("N", "Размер создаваемого массива")]
        public int N { get; set; }

        [Output("Массив", "Пустой массив заданного размера N")]
        public T[] Array { get; private set; }

        public void Run()
        {
            Array = new T[N];
        }
    }

    [Name("Пустой массив целых чисел", "Массивы")]
    public class NewIntArray : NewArray<int>
    {        
    }

    [Name("Пустой массив вещественных чисел", "Массивы")]
    public class NewDoubleArray : NewArray<double>
    {
    }

    [Name("Пустой массив строк", "Массивы")]
    public class NewStringArray : NewArray<string>
    {        
    }
}

using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CoreToolbox
{
    [Name("Массив целых чисел", "Массивы")]
    public class ParseIntArrayFromString : IAlgorithm
    {
        [Input("Описание массива", Description = "Массив, заданный в формате [elem1 elem2 ... elemN]")]
        public string String { get; set; }

        [Output("Массив")]
        public int[] Array { get; set; }

        public void Run()
        {
            if ((String.First() != '[') && (String.Last() != ']'))
                throw new ArgumentException("Описание массива имеет неверный формат");

            string str = String.Substring(1, String.Length - 2);
            string[] strElements = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Array = new int[strElements.Length];
            for (int i = 0; i < strElements.Length; i++)
            {
                try
                {
                    Array[i] = Convert.ToInt32(strElements[i]);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Элемент массива " + strElements[i] + " не является целым числом", e);
                }
            }
        }

        [Name("Массив вещественных чисел", "Массивы")]
        public class ParseDoubleArrayFromString : IAlgorithm
        {
            [Input("Описание массива", Description = "Массив, заданный в формате [elem1 elem2 ... elemN]")]
            public string String { get; set; }

            [Output("Массив")]
            public double[] Array { get; set; }

            public void Run()
            {
                if ((String.First() != '[') && (String.Last() != ']'))
                    throw new ArgumentException("Описание массива имеет неверный формат");

                string str = String.Substring(1, String.Length - 2);
                string[] strElements = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Array = new double[strElements.Length];
                for (int i = 0; i < strElements.Length; i++)
                {
                    try
                    {
                        Array[i] = Convert.ToDouble(strElements[i], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("Элемент массива " + strElements[i] + " не является вещественным числом", e);
                    }
                }
            }
        }

        [Name("Массив строк", "Массивы")]
        public class ParseStringArrayFromString : IAlgorithm
        {
            [Input("Описание массива", Description = "Массив, заданный в формате [\"elem1\" \"elem2\" ... \"elemN\"]")]
            public string String { get; set; }

            [Output("Массив")]
            public string[] Array { get; set; }

            public void Run()
            {
                if ((String.First() != '[') || (String.Last() != ']'))
                    throw new ArgumentException("Описание массива имеет неверный формат");

                string str = String.Substring(1, String.Length - 2).Trim();

                string pattern = "((\"([^\"]|(?<=\\\\)\")*\")\\s*)*";
                Regex regex = new Regex(pattern);
                if (!regex.IsMatch(str))
                    throw new ArgumentException("Описание массива имеет неверный формат");

                string elementPattern = "\"(([^\"]|(?<=\\\\)\")*)\"";
                Regex elementRegex = new Regex(elementPattern);
                Match match = elementRegex.Match(str);

                List<string> elements = new List<string>();

                while (match.Success)
                {
                    elements.Add(match.Groups[1].Value);
                    match = match.NextMatch();
                }

                Array = elements.ToArray();
            }
        }
    }
}

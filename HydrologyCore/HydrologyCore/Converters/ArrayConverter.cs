using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public class ArrayConverter : IConverter
    {
        public object Convert(object value, Type inElementType, Type outElementType)
        {
            Array arr = (Array)value;
            Array outArr = Array.CreateInstance(outElementType, arr.Length);
            IConverter valueConverter = new ValueConverter();
            for (int i = 0; i < arr.Length; i++)
                outArr.SetValue(valueConverter.Convert(arr.GetValue(i), inElementType, outElementType), i);
            return outArr;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public class ValueConverter : IConverter
    {
        public object Convert(object value, Type inElementType, Type outElementType)
        {
            // If inElementType == outElementType, do nothing
            if (inElementType == outElementType)
                return value;

            try
            {
                if (inElementType == typeof(int))
                {
                    if (outElementType == typeof(double))
                        return System.Convert.ToDouble(value);
                    else if (outElementType == typeof(string))
                        return value.ToString();
                }
                else if (inElementType == typeof(double))
                {
                    if (outElementType == typeof(int))
                        return System.Convert.ToInt32(value);
                    else if (outElementType == typeof(string))
                        return value.ToString();
                }
                else if (inElementType == typeof(string))
                {
                    string str = value as string;
                    if (outElementType == typeof(int))
                        return System.Convert.ToInt32(str);
                    else if (outElementType == typeof(double))
                        return System.Convert.ToDouble(str, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e)
            {
                throw new ConvertException($"Error converting {value} to {outElementType.Name}", e);
            }

            return value;
        }
    }
}

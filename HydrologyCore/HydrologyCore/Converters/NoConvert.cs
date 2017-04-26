using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public class NoConvert : IConverter
    {
        // for DataTable and DataSet we don't need to convert element types 

        public object Convert(object value, Type inElementType, Type outElementType)
        {
            return value;
        }
    }
}

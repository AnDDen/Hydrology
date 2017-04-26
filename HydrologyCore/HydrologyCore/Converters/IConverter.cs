using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public interface IConverter
    {
        object Convert(object value, Type inElementType, Type outElementType);
    }
}

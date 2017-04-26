using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public class ConvertersFactory
    {
        public static IConverter GetConverter(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.VALUE:
                    return new ValueConverter();
                case DataType.ARRAY:
                    return new ArrayConverter();
                case DataType.MATRIX:
                    return new MatrixConverter();
                case DataType.DATATABLE:
                case DataType.DATASET:
                default:
                    return new NoConvert();
            }            
        }
    }
}

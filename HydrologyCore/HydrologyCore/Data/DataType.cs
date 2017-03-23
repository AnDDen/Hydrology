using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Data
{
    public enum DataType
    {
        VALUE,
        ARRAY,
        MATRIX,
        DATATABLE,
        DATASET
    }

    public static class DataTypeHelpers
    {
        public static DataType GetDataType(Type type)
        {
            if (type == typeof(System.Data.DataSet))
                return DataType.DATASET;
            if (type == typeof(System.Data.DataTable))
                return DataType.DATATABLE;
            if (type.IsArray)
            {
                var elemType = type.GetElementType();
                if (elemType.IsArray)
                    return DataType.MATRIX;
                else
                    return DataType.ARRAY;
            }
            if (type.IsValueType || type == typeof(string))
                return DataType.VALUE;
            throw new ArgumentException(string.Format("There is no supported DataType for type {0}", type.FullName));
        }
    }
}

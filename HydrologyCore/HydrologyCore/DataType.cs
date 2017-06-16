using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
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
                if (type.GetArrayRank() == 2)
                    return DataType.MATRIX;
                else
                    return DataType.ARRAY;
            }
            if (type.IsValueType || type == typeof(string))
                return DataType.VALUE;
            throw new ArgumentException(string.Format("There is no supported DataType for type {0}", type.FullName));
        }

        public static Type GetType(DataType dataType, Type elementType)
        {
            switch (dataType)
            {
                case DataType.VALUE:
                    return elementType;
                case DataType.ARRAY:
                    return elementType.MakeArrayType();
                case DataType.MATRIX:
                    return elementType.MakeArrayType(2);
                case DataType.DATATABLE:
                    return typeof(System.Data.DataTable);
                case DataType.DATASET:
                    return typeof(System.Data.DataSet);
            }
            return null;
        }
    }
}

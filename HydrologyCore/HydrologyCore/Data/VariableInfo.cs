using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HydrologyCore.Data
{
    public class VariableInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public DataType DataType { get; set; }
        public Type BaseType { get; set; }
        public Type Type { get; set; }
        
        public VariableInfo(string name, string description, Type type)
        {
            Name = name;
            Description = description;
            Type = type;
            DataType = DataTypeHelpers.GetDataType(type);
            if (DataType == DataType.ARRAY)
                BaseType = type.GetElementType();
            else if (DataType == DataType.MATRIX)
                BaseType = type.GetElementType().GetElementType();
            else
                BaseType = type;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreInterfaces
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]

    public class ParameterAttribute : System.Attribute
    {
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public Type ValueType { get; set; }

        public ParameterAttribute(string name, object defaultValue, Type valueType)
        {
            Name = name;
            DefaultValue = defaultValue;
            ValueType = valueType;
        }
    }
}

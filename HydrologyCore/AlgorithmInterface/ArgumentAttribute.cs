using System;

namespace CoreInterfaces
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArgumentAttribute : Attribute
    {
        public object DefaultValue { get; set; }

        public ArgumentAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public ArgumentAttribute() : this(null) { }
    }
}

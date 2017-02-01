using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreInterfaces
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute
    {
        public string DisplayedName { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }

        public InputAttribute(string displayedName, string description, object defaultValue)
        {
            DisplayedName = displayedName;
            Description = description;
            DefaultValue = defaultValue;
        }

        public InputAttribute(string displayedName, object defaultValue) : this(displayedName, "", defaultValue) { }

        public InputAttribute(string displayedName) : this(displayedName, null) { }
    }
}

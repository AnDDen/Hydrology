using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreInterfaces
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NameAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string Package { get; set; }

        public NameAttribute(string name, string package = null)
        {
            Name = name;
            Package = package;
        }
    }
}

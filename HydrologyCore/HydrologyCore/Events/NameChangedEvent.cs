using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Events
{
    public class NameChangedEventArgs
    {
        public string Name { get; set; }
        public NameChangedEventArgs(string name)
        {
            Name = name;
        }
    }

    public delegate void NameChangedEventHandler(object sender, NameChangedEventArgs e);
}

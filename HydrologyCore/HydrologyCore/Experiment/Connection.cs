using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment
{
    public class Connection
    {
        public Port From { get; set; }
        public Port To { get; set; }
        
        public Connection(Port from, Port to)
        {
            From = from;
            To = to;
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Converters
{
    public class ConvertException : Exception
    {
        public ConvertException() : base() { }
        public ConvertException(string message) : base(message) { }
        public ConvertException(string message, Exception e) : base(message, e) { }
    }
}

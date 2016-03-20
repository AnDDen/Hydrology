using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreInterfaces
{
    public interface IWriter
    {
        void Write(DataTable table, string path);
    }
}

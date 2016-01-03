using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace AlgorithmInterface
{
    public interface IAlgorithm
    {
        void Init (DataSet data);
        void Run(IContext ctx);
        DataSet Results { get; }
    }
}

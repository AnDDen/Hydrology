using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
{
    public interface IExperimentNode
    {
        IExperimentNode Next { get; set; }
        IExperimentNode Prev { get; set; }
    }
}

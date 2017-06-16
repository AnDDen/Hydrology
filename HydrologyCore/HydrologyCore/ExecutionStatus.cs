using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore
{
    public enum ExecutionStatus
    {
        EXECUTING,
        NEXT_ITER,
        ERROR,
        WARNING,
        SUCCESS
    }
}

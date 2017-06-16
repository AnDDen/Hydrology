using HydrologyCore.Context;
using HydrologyCore.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Experiment
{
    public interface IRunable
    {
        string Name { get; set; }
        Block Parent { get; set; }

        IList<Port> InPorts { get; }
        IList<Port> OutPorts { get; }

        void Run(IContext ctx, BackgroundWorker worker);

        event NameChangedEventHandler NameChanged;

        event NodeExecutionStartEventHandler ExecutionStart;
        event NodeStatusChangedEventHandler StatusChanged;
    }
}

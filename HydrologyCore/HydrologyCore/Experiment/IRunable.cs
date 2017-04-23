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

        void Run(Context ctx);
        void Run(Context ctx, BackgroundWorker worker, int count, ref int current);

        event NameChangedEventHandler NameChanged;
    }
}

using HydrologyCore.Context;
using HydrologyCore.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Events
{
    public delegate void NodeExecutionStartEventHandler(object sender, NodeStatusChangedEventArgs e);
}

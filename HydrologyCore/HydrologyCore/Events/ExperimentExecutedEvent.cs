using HydrologyCore.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Events
{
        public class ExperimentExecutedEventArgs
        {
            public IContext Context { get; private set; }
           
            public ExecutionStatus Status => Context.Status;
            public Exception Error => Context.Error;

            public ExperimentExecutedEventArgs(IContext ctx)
            {
                Context = ctx;
            }
            
        }

        public delegate void ExperimentExecutedEventHandler(object sender, ExperimentExecutedEventArgs e);
}

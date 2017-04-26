﻿using HydrologyCore.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrologyCore.Context
{
    public interface IContext
    {
        IRunable Owner { get; }
        BlockContext ParentContext { get; }

        IDictionary<Port, object> Inputs { get; }
        IDictionary<Port, object> Outputs { get; }

        object GetPortValue(Port port);
        void SetPortValue(Port port, object value);

        IContext GetContext(IRunable node);
    }
}
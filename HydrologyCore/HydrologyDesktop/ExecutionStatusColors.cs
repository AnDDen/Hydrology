using HydrologyCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HydrologyDesktop
{
    public class ExecutionStatusColors
    {
        public static Brush EXECUTING_COLOR_BRUSH = Brushes.LightGreen;
        public static Brush SUCCESS_COLOR_BRUSH = Brushes.LightGreen;
        public static Brush WARNING_COLOR_BRUSH = Brushes.LightYellow;
        public static Brush ERROR_COLOR_BRUSH = Brushes.LightPink;

        public static Brush GetBrush(ExecutionStatus status)
        {
            switch (status)
            {
                case ExecutionStatus.SUCCESS:
                    return SUCCESS_COLOR_BRUSH;
                case ExecutionStatus.EXECUTING:
                case ExecutionStatus.NEXT_ITER:
                    return EXECUTING_COLOR_BRUSH;
                case ExecutionStatus.WARNING:
                    return WARNING_COLOR_BRUSH;
                case ExecutionStatus.ERROR:
                    return ERROR_COLOR_BRUSH;
            }
            return null;
        }
    }
}

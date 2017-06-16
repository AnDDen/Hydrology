using HydrologyCore;
using HydrologyCore.Context;
using HydrologyCore.Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace HydrologyDesktop.Views
{
    public class ExecListItem
    {
        private double fullWidth;

        private int totalCount;
        public int Total
        {
            get { return totalCount; }
            set { totalCount = value; }
        }

        private int count;
        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                currentWidth = fullWidth * (count > totalCount ? totalCount : count) * 1.0 / totalCount;
            }
        }            

        private ExecutionStatus status;
        public ExecutionStatus Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    color = ExecutionStatusColors.GetBrush(status);
                }                
            }
        }

        private double currentWidth;
        public double CurrentWidth
        {
            get { return currentWidth; }
            set { }
        }

        private Brush color;
        public Brush Color
        {
            get { return color; }
            set { }
        }
        
        public Thickness Padding { get; set; }

        public IRunable Node { get; set; }

        public ExecListItem(IContext ctx, double fullWidth, int total = 1)
        {
            Node = ctx.Owner;

            this.fullWidth = fullWidth;
            var p = ctx.Owner;
            int i = 0;
            while (p.Parent != null)
            {
                i++;
                p = p.Parent;
            }

            Padding = new Thickness(i * 20, 5, 20, 5);
            totalCount = total;
            Count = 0;
            Status = ctx.Status;
           
        }
    }
}

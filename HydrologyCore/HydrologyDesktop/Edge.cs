using HydrologyDesktopControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HydrologyDesktop
{
    public class Edge
    {
        public AlgNodeControl NodeFrom { get; set; }
        public AlgNodeControl NodeTo { get; set; }

        public ArrowControl Arrow { get; set; }

        private Point start, end;
        public Point Start
        {
            get { return start; }
            set
            {
                start = value;
                CalcArrow();
            }
        }
        public Point End
        {
            get { return end; }
            set
            {
                end = value;
                CalcArrow();
            }
        }

        private void CalcArrow()
        {
            Arrow.Length = Math.Sqrt((end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y));
            Arrow.Angle = Math.Sign(end.Y - start.Y) * Math.Acos((end.X - start.X) / Arrow.Length) * 180 / Math.PI;

            var transform = Arrow.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                Arrow.RenderTransform = transform;
            }
            transform.X = start.X;
            transform.Y = start.Y;
        }

        public bool IsValid()
        {
            return NodeFrom != null && NodeTo != null;
        }

        public Edge(AlgNodeControl nodeFrom, AlgNodeControl nodeTo)
        {
            NodeFrom = nodeFrom;
            NodeTo = nodeTo;
            Arrow = new ArrowControl();
        }
    }
}

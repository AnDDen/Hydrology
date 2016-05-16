using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace HydrologyDesktop.Controls
{
    public class LoopNodeControl : NodeControl
    {
        private string varName;
        private double startValue;
        private double endValue;
        private double step;

        public string VarName
        {
            get
            {
                return varName;
            }

            set
            {
                varName = value;
                (paramsPanel.Children[0] as Label).Content = string.Format("Переменная: {0}", varName);
            }
        }
        public double StartValue
        {
            get
            {
                return startValue;
            }

            set
            {
                startValue = value;
                (paramsPanel.Children[1] as Label).Content = string.Format("От {0} до {1}", startValue, endValue);
            }
        }
        public double EndValue
        {
            get
            {
                return endValue;
            }

            set
            {
                endValue = value;
                (paramsPanel.Children[1] as Label).Content = string.Format("От {0} до {1}", startValue, endValue);
            }
        }
        public double Step
        {
            get
            {
                return step;
            }

            set
            {
                step = value;
                (paramsPanel.Children[2] as Label).Content = string.Format("Шаг = {0}", step);
            }
        }

        public LoopNodeControl() : base()
        {
            paramsExpander.Visibility = Visibility.Visible;
            border.Visibility = Visibility.Collapsed;
            borderLoop.Visibility = Visibility.Visible;

            nodeNameLbl.Content = "Цикл";

            paramsPanel.Children.Clear();
            paramsPanel.Children.Add(new Label { Content = "Переменная: ..." });
            paramsPanel.Children.Add(new Label { Content = "От ... до ..." });
            paramsPanel.Children.Add(new Label { Content = "Шаг = ..." });
        }
    }
}

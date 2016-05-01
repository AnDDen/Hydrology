using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HydrologyDesktop.Controls
{
    public class AlgorithmNodeControl : NodeControl
    {
        public Type AlgorithmType { get; set; }

        private DataTable paramsTable;
        public DataTable ParamsTable
        {
            get
            {
                return paramsTable;
            }
            set
            {
                paramsTable = value;
                if (paramsTable.Rows.Count == 0)
                {
                    paramsExpander.Visibility = Visibility.Collapsed;
                }
                else
                {
                    paramsExpander.Visibility = Visibility.Visible;
                    paramsPanel.Children.Clear();
                    foreach (DataRow row in paramsTable.Rows)
                    {
                        paramsPanel.Children.Add(
                            new Label()
                            {
                                Content = row[0].ToString() + " = " + row[1].ToString()
                            }
                        );
                    }
                }
            }
        }

        public string InitPath { get; set; }

        public AlgorithmNodeControl() : base()
        {
            paramsExpander.Visibility = Visibility.Visible;
        }
    }
}

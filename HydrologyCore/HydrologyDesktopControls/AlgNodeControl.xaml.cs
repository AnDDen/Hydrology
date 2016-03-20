using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для AlgNodeControl.xaml
    /// </summary>
    public partial class AlgNodeControl : UserControl
    {
        private string algorithmName;
        public string AlgorithmName
        {
            get
            {
                return algorithmName;
            }
            set
            {
                algorithmName = value;
                algNameLlb.Content = algorithmName;
            }
        }

        public DataTable ParamsTable { get; set; }

        public string InitPath { get; set; }

        public event EventHandler<EventArgs> SettingsClicked;

        public AlgNodeControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsClicked != null)
            {
                SettingsClicked(this, EventArgs.Empty);
            }
        }
    }
}

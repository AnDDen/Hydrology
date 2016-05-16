using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Shapes;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для InitSettingsWindow.xaml
    /// </summary>
    public partial class LoopSettingsWindow : Window
    {
        private string varName;
        private double startValue;
        private double endValue;
        private double step;

        public string VarName
        {
            get { return varName; }
            set { 
                varName = value;
                varTextBox.Text = varName;
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
                startTextBox.Text = startValue.ToString();
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
                endTextBox.Text = endValue.ToString();
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
                stepTextBox.Text = step.ToString();
            }
        }

        public LoopSettingsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            varName = varTextBox.Text;
            try
            {
                startValue = double.Parse(startTextBox.Text, CultureInfo.InvariantCulture);
                endValue = double.Parse(endTextBox.Text, CultureInfo.InvariantCulture);
                step = double.Parse(stepTextBox.Text, CultureInfo.InvariantCulture);
                DialogResult = true;
            }
            catch
            {
                MessageBox.Show("Некорректный формат входных данных.");
            }            
        }
    }
}

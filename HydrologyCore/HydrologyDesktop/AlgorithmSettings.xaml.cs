using System;
using System.Collections.Generic;
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
using System.Data;
using CoreInterfaces;
using CsvParser;
using System.Reflection;

namespace HydrologyDesktop
{

    /// <summary>
    /// Логика взаимодействия для AlgorithmSettings.xaml
    /// </summary>
    public partial class AlgorithmSettings : Window
    {
        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                folderTextBox.Text = path;
                folderDialog.SelectedPath = path;
            }
        }

        private Type algType;
        public Type AlgType
        {
          get { return algType; }
          set { algType = value; }
        }

        private DataTable paramsTable;
        public DataTable ParamsTable
        {
            get { return paramsTable; }
            set
            {
                paramsTable = value;
                paramsGrid.DataContext = paramsTable.DefaultView;
            }
        }

        System.Windows.Forms.FolderBrowserDialog folderDialog;

        public AlgorithmSettings()
        {
            InitializeComponent();

            ParamsTable = new DataTable();
            ParamsTable.Columns.Add("Name");
            ParamsTable.Columns.Add("Value");

            folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        }

        public AlgorithmSettings(DataTable paramsTable, Type algType)
        {
            InitializeComponent();

            ParamsTable = paramsTable;
            AlgType = algType;

            folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderTextBox.Text = folderDialog.SelectedPath;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            path = folderTextBox.Text;

            if (ParamsValid())
                DialogResult = true;
            else
                MessageBox.Show("Один или несколько параметров имеют неверный формат", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool ParamsValid()
        {
            var attrs = algType.GetCustomAttributes<ParameterAttribute>();
            foreach (DataRow row in paramsTable.Rows)
            {
                var attr = attrs.First((x) => { return x.Name == row["Name"].ToString(); });
                object value;
                if (row["Value"] == null) return false;
                try
                {
                    value = Convert.ChangeType(row["Value"], attr.ValueType);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

    }
}

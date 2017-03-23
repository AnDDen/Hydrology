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
using HydrologyDesktop.Controls;
using System.Collections.ObjectModel;

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

        public Dictionary<string, LoopControl> VarLoop { get; set; }

        public Dictionary<string, LoopControl> VarNames { get; set; }

        private LoopControl parentLoop;
        public LoopControl ParentLoop
        {
            get
            {
                return parentLoop;
            }

            set
            {
                parentLoop = value;
                VarNames.Clear();
                LoopControl p = parentLoop;
                while (p != null)
                {
                    VarNames.Add(p.VarName, p);
                    p = p.PreviousLoop;
                }
                
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

        public AlgorithmSettings(System.Windows.Forms.FolderBrowserDialog folderDialog)
        {
            InitializeComponent();

            ParamsTable = new DataTable();
            ParamsTable.Columns.Add("Name");
            ParamsTable.Columns.Add("Value");

            //folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.folderDialog = folderDialog;

            VarNames = new Dictionary<string, LoopControl>();
        }

        public AlgorithmSettings(System.Windows.Forms.FolderBrowserDialog folderDialog, DataTable paramsTable, Type algType)
        {
            InitializeComponent();

            ParamsTable = paramsTable;
            AlgType = algType;

            //folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.folderDialog = folderDialog;

            VarNames = new Dictionary<string, LoopControl>();
            VarLoop = new Dictionary<string, LoopControl>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            folderDialog.SelectedPath = folderTextBox.Text;
            if (!Directory.Exists(folderDialog.SelectedPath) || folderDialog.SelectedPath == "")
            {
                folderDialog.SelectedPath = Directory.GetCurrentDirectory();
            }
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
        }

        private bool ParamsValid()
        {
            var attrs = algType.GetCustomAttributes<ParameterAttribute>();
            foreach (DataRow row in paramsTable.Rows)
            {
                var name = row["Name"].ToString();
                var svalue = row["Value"] == null ? null : row["Value"].ToString();
                var attr = attrs.FirstOrDefault((x) => { return x.Name == name; });
                if (attr == null)
                    throw new ArgumentException(string.Format(@"Алгоритм {0} не имеет параметра {1}. 
                        Исправьте файл настроек, удалив лишний параметр или подгрузите другую версию алгоритма, который поддерживает параметр {1}.", algType.Name, name));
                object value;
                if (svalue == null)
                {
                    MessageBox.Show(string.Format("Параметр {0} имеет неверный формат", name),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else if (svalue[0] == '{')
                {
                    string varName = svalue.Trim(' ', '{', '}');
                    if (VarNames.Keys.Contains(varName))
                    {
                        VarLoop.Add(svalue, VarNames[varName]);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Переменная {0}, заданная для параметра {1}, не найдена", varName, name),
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    try
                    {
                        value = Convert.ChangeType(row["Value"], attr.ValueType);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(string.Format("Параметр {0} имеет неверный формат", name),
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            return true;
        }

    }
}

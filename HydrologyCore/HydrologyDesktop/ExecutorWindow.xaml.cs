using CoreInterfaces;
using CsvParser;
using HydrologyCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для RunExperimentWindow.xaml
    /// </summary>
    /// 

    public partial class ExecutorWindow : Window
    {
        private Experiment experiment;

        IList<AlgNodeControl> nodes;
        Core hydrologyCore;

        BackgroundWorker exec;

        public ExecutorWindow(IList<AlgNodeControl> nodes, Core hydrologyCore)
        {
            InitializeComponent();

            this.nodes = nodes;
            this.hydrologyCore = hydrologyCore;

            experiment = new Experiment();

            RunExecutor();            
        }

        public void RunExecutor()
        {
            execBar.Value = 0;

            exec = new BackgroundWorker();
            exec.DoWork += exec_DoWork;
            exec.RunWorkerCompleted += exec_RunWorkerCompleted;
            exec.ProgressChanged += exec_ProgressChanged;
            exec.WorkerReportsProgress = true;
            exec.WorkerSupportsCancellation = true;

            exec.RunWorkerAsync();
        }

        void exec_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            execBar.Value = e.ProgressPercentage;
        }

        void exec_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
                MessageBox.Show("Эксперимент выполнен", "", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        void exec_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Dispatcher.Invoke(() => { statusLbl.Content = "Загрузка исходных данных"; }, System.Windows.Threading.DispatcherPriority.Normal);
            int n = nodes.Count + 1;
            double percentInc = 100.0 / n;
            double percent = 0;
            experiment.StartFrom("experiment/initial");
            
            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
            percent += percentInc;
            worker.ReportProgress((int)percent);

            for (int i = 0; i < nodes.Count; i++)
            {
                string path = nodes[i].InitPath;
                if (Directory.Exists(nodes[i].InitPath))
                {
                    // write params to file
                    DataTable algParams = nodes[i].ParamsTable;
                    if (path[path.Length - 1] != '/') path += "/";
                    path += "params.csv";
                    IWriter writer = new CSVWriter();
                    writer.Write(algParams, path);

                    experiment.Then(hydrologyCore.Algorithm(nodes[i].AlgorithmName).InitFromFolder(nodes[i].InitPath));

                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return;
                    }
                    percent += percentInc;
                    worker.ReportProgress((int)percent);
                }
                else
                {
                    MessageBox.Show(string.Format("Путь {0}, указанный для алгоритма {1} не существует", nodes[i].InitPath, nodes[i].AlgorithmName),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (worker.CancellationPending == true)
            {
                e.Cancel = true;
                return;
            }
            Dispatcher.Invoke(() => { statusLbl.Content = "Выполнение алгоритма"; }, System.Windows.Threading.DispatcherPriority.Normal);
            percent = 0;
            worker.ReportProgress((int)percent);

            experiment.Run();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            exec.CancelAsync();
            statusLbl.Content = "Завершение текущей операции";
            ((Button)sender).IsEnabled = false;
        }
    }
}

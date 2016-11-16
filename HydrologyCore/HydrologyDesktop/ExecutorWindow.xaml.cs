using CoreInterfaces;
using CsvParser;
using HydrologyCore;
using HydrologyDesktop.Controls;
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
        private ExperimentGraph experimentGraph;
        private double percent = 0;

        private Core hydrologyCore;

        private BackgroundWorker backgroundWorker;

        public ExecutorWindow(ExperimentGraph experimentGraph, Core hydrologyCore)
        {
            InitializeComponent();

            this.experimentGraph = experimentGraph;
            this.hydrologyCore = hydrologyCore;

            RunExecutor();            
        }

        public void PrepareInitNode(InitNodeControl node, Experiment experiment)
        {
            experiment.StartFrom(node.InitPath);
        }
        public void PrepareRunProcessNode(RunProcessNodeControl node, Experiment experiment)
        {
            experiment.Then(hydrologyCore.RunProcess(node.ProcessName));
        }
        public void PrepareAlgorithmNode(AlgorithmNodeControl node, Experiment experiment)
        {
            DataTable algParams = node.ParamsTable.Copy();

            foreach (DataRow row in algParams.Rows)
            {
                if (node.VarLoop.Keys.Contains(row["Name"].ToString()))
                    row["Value"] = node.VarLoop[row["Name"].ToString()].RunValue;
            }

            if (string.IsNullOrEmpty(node.InitPath))
            {
                experiment.Then(hydrologyCore.Algorithm(node.AlgorithmType.Name).SetParams(algParams));
            } 
            else
            {
                if (Directory.Exists(node.InitPath))
                {
                    experiment.Then(hydrologyCore.Algorithm(node.AlgorithmType.Name).InitFromFolder(node.InitPath).SetParams(algParams));
                }
                else
                {
                    MessageBox.Show(string.Format("Путь {0}, указанный для алгоритма {1} не существует", node.InitPath, node.AlgorithmType.Name),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        public void PrepareLoop(LoopControl loop, Experiment experiment, BackgroundWorker worker, DoWorkEventArgs e, double p)
        {
            var chain = loop.LoopBody.CreateExecutionChain();

            double percentInc = 100.0 * p / chain.Count;
            percent += percentInc / 2;
            worker.ReportProgress((int)percent);

            for (loop.ResetValue(); loop.IsLoop(); loop.StepValue())
            {
                for (int i = 0; i < chain.Count; i++)
                {
                    if (chain[i] is InitNodeControl)
                        PrepareInitNode(chain[i] as InitNodeControl, experiment);
                    else if (chain[i] is AlgorithmNodeControl)
                        PrepareAlgorithmNode(chain[i] as AlgorithmNodeControl, experiment);
                    else if (chain[i] is RunProcessNodeControl)
                        PrepareRunProcessNode(chain[i] as RunProcessNodeControl, experiment);
                    else if (chain[i] is LoopControl)
                        PrepareLoop(chain[i] as LoopControl, experiment, worker, e, percentInc);

                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return;
                    }
                    percent += percentInc;
                    worker.ReportProgress((int)percent);
                }

                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                percent += percentInc;
                worker.ReportProgress((int)percent);
            }
        }
        public void PrepareExperiment(BackgroundWorker worker, DoWorkEventArgs e)
        {
            experiment = new Experiment();

            var chain = experimentGraph.CreateExecutionChain();

            double percentInc = 100.0 / chain.Count;
            percent += percentInc / 2;
            worker.ReportProgress((int)percent);

            for (int i = 0; i < chain.Count; i++)
            {
                if (chain[i] is InitNodeControl)
                    PrepareInitNode(chain[i] as InitNodeControl, experiment);
                else if (chain[i] is AlgorithmNodeControl)
                    PrepareAlgorithmNode(chain[i] as AlgorithmNodeControl, experiment);
                else if (chain[i] is RunProcessNodeControl)
                    PrepareRunProcessNode(chain[i] as RunProcessNodeControl, experiment);
                else if (chain[i] is LoopControl)
                    PrepareLoop(chain[i] as LoopControl, experiment, worker, e, percentInc);

                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                percent += percentInc;
                worker.ReportProgress((int)percent);
            }
        }

        public void Run(BackgroundWorker worker, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() => { statusLbl.Content = "Подготовка эксперимента"; }, System.Windows.Threading.DispatcherPriority.Normal);

            PrepareExperiment(worker, e);

            Dispatcher.Invoke(() => { statusLbl.Content = "Выполнение эксперимента"; }, System.Windows.Threading.DispatcherPriority.Normal);
            percent = 0;
            worker.ReportProgress((int)percent);

            experiment.Run();
        }

        public void RunExecutor()
        {
            execBar.Value = 0;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += exec_DoWork;
            backgroundWorker.RunWorkerCompleted += exec_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += exec_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            backgroundWorker.RunWorkerAsync();
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
            Run(sender as BackgroundWorker, e);
        }
        public void Cancel()
        {
            backgroundWorker.CancelAsync();
            statusLbl.Content = "Завершение текущей операции";
            cancelBtn.IsEnabled = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }
    }
}

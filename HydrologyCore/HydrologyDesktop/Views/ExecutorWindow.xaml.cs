using HydrologyCore;
using HydrologyCore.Context;
using HydrologyCore.Events;
using HydrologyCore.Experiment;
using HydrologyCore.Experiment.Nodes;
using HydrologyDesktop.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Threading;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для RunExperimentWindow.xaml
    /// </summary>
    /// 

    public partial class ExecutorWindow : Window
    {
        private Experiment experiment;

        private BackgroundWorker backgroundWorker;

        public ObservableCollection<ExecListItem> ExecListItems { get; set; }


        public ExecutorWindow(Experiment experiment)
        {
            InitializeComponent();

            this.experiment = experiment;

            ExecListItems = new ObservableCollection<ExecListItem>();

            experiment.NodeExecutionStart += Experiment_NodeExecutionStart;
            experiment.NodeStatusChanged += Experiment_NodeStatusChanged;

            RunExecutor();
        }
        
        private void Experiment_NodeStatusChanged(object sender, NodeStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Node.Parent == null || e.Node is PortNode || !(e.Node is LoopBlock))
                    return;
                var node = ExecListItems.FirstOrDefault(x => x.Node == e.Node);
                if (node != null && e.Node is LoopBlock) {
                    node.Status = e.Status;
                    node.Count = node.Count + 1;
                    if (node.Status == ExecutionStatus.NEXT_ITER)
                    {
                        ResetChildren(node, e.Context);
                    }
                    UpdateBinding();
                }                
            });
        }

        private void Experiment_NodeExecutionStart(object sender, NodeStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Node.Parent == null || e.Node is PortNode || !(e.Node is LoopBlock))
                    return;
                var node = ExecListItems.FirstOrDefault(x => x.Node == e.Node);
                if (node == null)
                {
                    if (e.Node is LoopBlock)
                    {
                        var loop = e.Node as LoopBlock;
                        Array varArray = (Array)e.Context.GetPortValue(loop.LoopPortNode.BlockPort);
                        int length = varArray.Length + 1;
                        var item = new ExecListItem(e.Context, list.ActualWidth, length);
                        item.Count = 1;
                        ExecListItems.Add(item);
                    }
                    else
                    {
                        ExecListItems.Add(new ExecListItem(e.Context, list.ActualWidth));
                    }
                }
                else
                {
                    if (e.Node is LoopBlock)
                    {
                        var loop = e.Node as LoopBlock;
                        Array varArray = (Array)e.Context.GetPortValue(loop.LoopPortNode.BlockPort);
                        int length = varArray.Length + 1;
                        node.Total = length;
                        node.Count = 1;
                    }
                }
                UpdateBinding();
            });
        }

        private void UpdateBinding()
        {
            list.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();
            list.Items.Refresh();
        }

        private void ResetChildren(ExecListItem item, IContext ctx)
        {
            foreach (var it in ExecListItems.Where(x => x.Node.Parent == item.Node))
            {
                it.Status = ExecutionStatus.EXECUTING;
                it.Count = 0;
                ResetChildren(it, ctx);
            }
        }

        public void RunExecutor()
        {
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
            if (e.UserState != null)
                statusLbl.Content = "Выполняется узел " + e.UserState.ToString();
            else
                statusLbl.Content = "";
        }

        void exec_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Выполнение эксперимента отменено пользователем", "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else if (e.Error != null)
            {
                MessageBox.Show("При выполнении эксперимента произошла ошибка", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Эксперимент выполнен успешно", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        void exec_DoWork(object sender, DoWorkEventArgs e)
        {            
            experiment.Run(backgroundWorker);
            if (backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        public void Cancel()
        {
            backgroundWorker.CancelAsync();
            statusLbl.Content = "Выполнение отменено пользователем. Идет завершение текущей операции";
            cancelBtn.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (backgroundWorker.IsBusy)
            {
                e.Cancel = true;
                Cancel();
            }
        }
    }
}

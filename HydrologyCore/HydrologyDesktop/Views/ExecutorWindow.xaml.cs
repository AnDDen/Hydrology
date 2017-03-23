using HydrologyCore.Experiment;
using System.ComponentModel;
using System.Windows;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для RunExperimentWindow.xaml
    /// </summary>
    /// 

    public partial class ExecutorWindow : Window
    {
        private Experiment experiment;
        private double percent = 0;

        private BackgroundWorker backgroundWorker;

        public ExecutorWindow(Experiment experiment)
        {
            InitializeComponent();

            this.experiment = experiment;

            RunExecutor();            
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
            }
            else if (e.Error != null)
            {
                MessageBox.Show("При выполнении эксперимента произошла ошибка", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Эксперимент выполнен успешно", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Close();
        }

        void exec_DoWork(object sender, DoWorkEventArgs e)
        {
            experiment.Run(backgroundWorker);
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
    }
}

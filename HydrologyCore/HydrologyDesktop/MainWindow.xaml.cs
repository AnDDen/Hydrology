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
using HydrologyCore;
using CoreInterfaces;
using CsvParser;
using System.IO;
using System.Data;
using System.Threading;
using System.Reflection;

namespace HydrologyDesktop
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Core hydrologyCore;

        private IList<AlgNodeControl> nodes;
        private IList<Edge> edges;

        protected bool isDrag;
        private Point clickPos;

        private Thread runExperimentThread;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            NewExperiment();
        }

        private void Init()
        {
            hydrologyCore = new Core();
            IList<string> algNames = hydrologyCore.AlgorithmTypes.Keys.ToList();

            foreach (string alg in algNames)
            {
                Button b = new Button();
                b.Content = alg;
                b.Style = (Style)this.FindResource("AlgButtonStyle");
                b.Click += algButton_Click;
                AlgorithmButtonsPanel.Children.Add(b);
            }

            edges = new List<Edge>();
        }

        void NewExperiment()
        {
            nodes = new List<AlgNodeControl>();
        }

        void AddNode(string algName, string initPath, DataTable paramTable)
        {
            AlgNodeControl node = new AlgNodeControl { AlgorithmName = algName, InitPath = initPath, ParamsTable = paramTable };
            node.Style = (Style)this.FindResource("AlgNodeStyle");
            node.SettingsClicked += node_SettingsClicked;
            node.MouseLeftButtonDown += node_MouseLeftButtonDown;
            node.MouseLeftButtonUp += node_MouseLeftButtonUp;
            node.MouseMove += node_MouseMove;
            node.LostMouseCapture += node_LostMouseCapture;
            node.Loaded += node_Loaded;
            ExperimentPanel.Children.Add(node);
            nodes.Add(node);
        }

        void node_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (Edge edge in GetNodeEdges((AlgNodeControl)sender))
            {
                UpdateEdge(edge);
            }
        }

        void AddEdge(AlgNodeControl nodeFrom, AlgNodeControl nodeTo)
        {
            Edge edge = new Edge(nodeFrom, nodeTo);
            edges.Add(edge);
            EdgesPanel.Children.Add(edge.Arrow);
        }

        void UpdateEdge(Edge edge)
        {
            AlgNodeControl nodeFrom = edge.NodeFrom;
            AlgNodeControl nodeTo = edge.NodeTo;
            Point startPoint = nodeFrom.TransformToAncestor(this).Transform(new Point(-150, -25))
                + new Vector(nodeFrom.Width, nodeFrom.Height / 2);
            Point endPoint = nodeTo.TransformToAncestor(this).Transform(new Point(-150, -25))
                + new Vector(0, nodeTo.Height / 2);
            edge.Start = startPoint;
            edge.End = endPoint;
        }

        void node_LostMouseCapture(object sender, MouseEventArgs e)
        {
            isDrag = false;
        }

        void node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDrag = true;
            AlgNodeControl node = sender as AlgNodeControl;
            var transform = node.RenderTransform as TranslateTransform;           
            clickPos = e.GetPosition(this);
            if (transform != null)
                clickPos -= new Vector(transform.X, transform.Y);
            node.CaptureMouse();
        }

        void node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrag = false;
            (sender as AlgNodeControl).ReleaseMouseCapture();
        }

        void node_MouseMove(object sender, MouseEventArgs e)
        {
            AlgNodeControl node = sender as AlgNodeControl;
            if (node != null && isDrag)
            {
                Point currentPos = e.GetPosition(this.Parent as UIElement);
                var transform = node.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    node.RenderTransform = transform;
                }
                transform.X = currentPos.X - clickPos.X;
                transform.Y = currentPos.Y - clickPos.Y;

                foreach (Edge edge in GetNodeEdges(node))
                    UpdateEdge(edge);
            }
        }

        void node_SettingsClicked(object sender, EventArgs e)
        {
            AlgNodeControl node = (AlgNodeControl)sender;
            var algSettings = new AlgorithmSettings(node.ParamsTable, hydrologyCore.AlgorithmTypes[node.AlgorithmName]) { Owner = this, Path = node.InitPath };
            bool? dialogResult = algSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                node.InitPath = algSettings.Path;
                node.ParamsTable = algSettings.ParamsTable;
            }
        }

        void algButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string algName = btn.Content.ToString();

            Type algType = hydrologyCore.AlgorithmTypes[algName];

            DataTable paramsTable = new DataTable();
            paramsTable.Columns.Add("Name");
            paramsTable.Columns.Add("Value");

            foreach (ParameterAttribute attr in algType.GetCustomAttributes<ParameterAttribute>())
            {
                DataRow row = paramsTable.NewRow();
                row["Name"] = attr.Name;
                row["Value"] = attr.DefaultValue;
                paramsTable.Rows.Add(row);
            }

            var algSettings = new AlgorithmSettings(paramsTable, algType) { Owner = this };
            bool? dialogResult = algSettings.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                AddNode(algName, algSettings.Path, algSettings.ParamsTable);
                if (nodes.Count > 1)
                    AddEdge(nodes[nodes.Count - 2], nodes[nodes.Count - 1]);
            }
        }

        IEnumerable<Edge> GetNodeEdges(AlgNodeControl node)
        {
            return edges.Where((x) => { return x.NodeFrom == node || x.NodeTo == node; });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExecutorWindow experimentWindow = new ExecutorWindow(nodes, hydrologyCore);
            experimentWindow.ShowDialog();
            /*
            runExperimentThread = new Thread(RunExperiment);
            runExperimentThread.Start();   */  
        }

        private void RunExperiment()
        {
            Experiment experiment = new Experiment();
            experiment.StartFrom("experiment/initial");

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
                }
                else
                {
                    MessageBox.Show(string.Format("Путь {0}, указанный для алгоритма {1} не существует", nodes[i].InitPath, nodes[i].AlgorithmName),
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            experiment.Run();
            MessageBox.Show("Эксперимент выполнен", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

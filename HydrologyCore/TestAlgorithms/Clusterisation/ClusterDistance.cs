using CoreInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAlgorithms
{
    [Name("Расстояние между кластерами")]
    public class ClusterDistance : IAlgorithm
    {
        [Input("Первая группа")]
        public DataTable Table1 { get; set; }

        [Input("Вторая группа")]
        public DataTable Table2 { get; set; }

        [Input("Вариант расстояния между кластерами")]
        public string DistType { get; set; }

        [Input("Метрика расстояния")]
        public string Metric { get; set; }

        [Output("Расстояние между группами")]
        public double Distance { get; set; }

        public void Run()
        {
            int n1 = Table1.Rows.Count;
            int n2 = Table2.Rows.Count;

            double[,] d = new double[n1, n2];
            for (int i = 0; i < n1; i++)
            {
                DataRow row1 = Table1.Rows[i];
                double x1 = Convert.ToDouble(row1["x"], System.Globalization.CultureInfo.InvariantCulture);
                double y1 = Convert.ToDouble(row1["y"], System.Globalization.CultureInfo.InvariantCulture);
                for (int j = 0; j < n2; j++)
                {
                    DataRow row2 = Table2.Rows[j];
                    double x2 = Convert.ToDouble(row2["x"], System.Globalization.CultureInfo.InvariantCulture);
                    double y2 = Convert.ToDouble(row2["y"], System.Globalization.CultureInfo.InvariantCulture);

                    d[i, j] = CalculateDistance(x1, y1, x2, y2);
                }
            }

            switch (DistType.ToLower())
            {
                case "nearest":
                    Distance = d.Cast<double>().Min();
                    break;
                case "farthest":
                    Distance = d.Cast<double>().Max();
                    break;
                case "mean":
                    Distance = d.Cast<double>().Sum() / (n1 * n2);
                    break;
            }
        }

        public double CalculateDistance(double x1, double y1, double x2, double y2)
        {
            switch (Metric.ToLower()) {
                case "euclidean":
                    return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
            }
            throw new ArgumentException($"Неизвестная метрика {Metric}");
        }
    }
}

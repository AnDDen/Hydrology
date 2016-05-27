using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniformityCheck
{
    public class Statistic
    {
        public double means(double[] series, int i0, int n_)
        {
            double result = 0;
            int n = series.Length;
            for (int i = i0; i < n_; ++i)
                result += series[i];

            return result / n_;
        }

        /*************************************************************************/

        // Среднее выборочное
        public double mean(double[] series)
        {
            double result = 0;
            int n = series.Length;
            for (int i = 0; i < series.Length; ++i)
                result += series[i];

            return result / series.Length;
        }

        // Выборочная дисперсия
        public double dispersion(double[] series)
        {
            double result = 0,
                mid = mean(series);

            for (int i = 0; i < series.Length; ++i)
                result += (series[i] - mid) * (series[i] - mid);

            return result / series.Length;
        }

        // Среднеквадратическое отклонение
        public double deviation(double[] series)
        {
            double d = dispersion(series);
            return Math.Sqrt(d);
        }

        // Ассиметрия
        public double skewness(double[] series)
        {
            int N = series.Length;
            double dev = deviation(series),
                result = 0,
                mid = mean(series);

            for (int i = 0; i < N; ++i)
                result += (series[i] - mid) * (series[i] - mid) * (series[i] - mid);

            return result / (dev * dev * dev) * (double)N / ((double)(N - 1) * (double)(N - 2));
        }

        // Вариация
        public double variation(double[] series)
        {
            double m = mean(series);
            return Math.Abs(m) < 0.001 ? 0 : deviation(series) / m;
        }

        // Корреляция
        public double correlation(double[] series)
        {
            double result = 0,
                middle1,
                middle2,
                second;
            int N = series.Length;

            middle1 = middle2 = mean(series);
            middle1 = (middle1 * N - series[N - 1]) / (N - 1);
            middle2 = (middle2 * N - series[0]) / (N - 1);

            for (int i = 0; i < N - 1; ++i)
                result += (series[i] - middle1) * (series[i + 1] - middle2);

            second = 0;
            for (int i = 0; i < N - 1; ++i)
                second += (series[i] - middle1) * (series[i] - middle1);

            result /= Math.Sqrt(second);

            second = 0;
            for (int i = 1; i < N; ++i)
                second += (series[i] - middle2) * (series[i] - middle2);

            return result / Math.Sqrt(second);
        }

        /***************************************************************************************/

        // Среднее выборочное для двумерного
        public double mean2(double[,] series, int index, int j, int lam_len)
        {
            double result = 0;
            for (int i = j*1000; i < (j+1)*1000; ++i)
                result += series[index, i];

            return result / 1000;
        }

        // Выборочная дисперсия
        public double dispersion2(double[,] series, int index, int j, int lam_len)
        {
            double result = 0,
                mid = mean2(series, index, j, lam_len);

            for (int i = j * 1000; i < (j + 1) * 1000; ++i)
                result += (series[index, i] - mid) * (series[index, i] - mid);

            return result / 1000;
        }

        // Среднеквадратическое отклонение
        public double deviation2(double[,] series, int index, int j, int lam_len)
        {
            double d = dispersion2(series, index, j, lam_len);
            return Math.Sqrt(d);
        }

        // Ассиметрия
        public double skewness2(double[,] series, int index, int j, int lam_len)
        {
            //int N = series.GetLength(1);
            double dev = deviation2(series, index, j, lam_len);
            double result = 0;
            double mid = mean2(series, index, j, lam_len);

            for (int i = j * 1000; i < (j + 1) * 1000; ++i)
                result += (series[index, i] - mid) * (series[index, i] - mid) * (series[index, i] - mid);

            return result / (dev * dev * dev) * 1000 / (999 * 998);
        }

        // Вариация
        public double variation2(double[,] series, int index, int j, int lam_len)
        {
            double m = mean2(series, index, j, lam_len);
            return Math.Abs(m) < 0.001 ? 0 : deviation2(series, index, j, lam_len) / m;
        }

        // Корреляция
        public double correlation2(double[,] series, int index, int j, int lam_len)
        {
            double result = 0,
                middle1,
                middle2,
                second;
            //int N = series.GetLength(1);

            middle1 = middle2 = mean2(series, index, j, lam_len);
            middle1 = (middle1 * lam_len - series[index, 999]) / (999);
            middle2 = (middle2 * lam_len - series[index, 0]) / (999);

            for (int i = j * 1000; i < (j + 1) * 1000 - 1; ++i)
                result += (series[index, i] - middle1) * (series[index, i + 1] - middle2);

            second = 0;
            for (int i = j * 1000; i < (j + 1) * 1000 - 1; ++i)
                second += (series[index, i] - middle1) * (series[index, i] - middle1);

            result /= Math.Sqrt(second);

            second = 0;
            for (int i = j * 1000 + 1; i < (j + 1) * 1000; ++i)
                second += (series[index, i] - middle2) * (series[index, i] - middle2);

            return result / Math.Sqrt(second);
        }

    }
}

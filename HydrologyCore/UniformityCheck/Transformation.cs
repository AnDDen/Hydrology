using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniformityCheck
{
    class Transformation
    {
        public static int index_size = 5;
        public int lam_len;
        public double[,] modifiers1;
        public double[,] modifiers2;
        public double[,,] params1;
        public double[,,] params2;
        public int N;
        public Statistic stat = new Statistic();

        public Transformation(double[] series, double[] lambda_items,  double n, double autocorr)
        {
            lam_len = lambda_items.Length;
            params1 = new double[index_size, lam_len, 6];
            params2 = new double[index_size, lam_len, 6];

            //параметры для входной последовательности 1000 (для lambda = 0).
            double m_vkMean = stat.mean(series);
            double m_vkDev = stat.deviation(series);
            double m_vkVariation = stat.variation(series);
            double m_vkSkew = stat.skewness(series);
            double m_vkCorr = stat.correlation(series);
            double m_vkEta = m_vkSkew / m_vkVariation;

            //разбиение
            N = series.Length;          
            int n1 = Convert.ToInt32(N * n);
            int n2 = N - n1;
            List<double> mods1 = new List<double>();
            List<double> mods2 = new List<double>();
            int negCount = 0;

            for (int index = 0; index < index_size; index++)
            {
                for (int i = 0; i < lam_len; i++)
                {
                    double lambda = lambda_items[i];
                    mods1 = transform(series, mods1, 0, n1, index, lambda, negCount);//, udm);
                    mods2 = transform(series, mods2, n1, N, index, lambda, negCount);//, udm);
                }

            }
            modifiers1 = new double[index_size, mods1.Count / index_size];
            modifiers2 = new double[index_size, mods2.Count / index_size];
            getModifiers(modifiers1, mods1, mods1.Count / index_size);
            getModifiers(modifiers2, mods2, mods2.Count / index_size);

            for (int index = 0; index < index_size; index++)                 
            {
                for (int j = 0; j < lam_len; j++)
                {
                    calcParameters(params1, tempMods(modifiers1, index, j), index, j);
                    calcParameters(params2, tempMods(modifiers2, index, j), index, j);
                }
            }

            //Student stud = new Student(modifiers1, modifiers2, autocorr, lam_len);
        }


        public List<double> transform(double[] series, List<double> mods, int i0, int n_, int index, double lambda, int negCount)//, double[] udm)
        {
            int idx;
            double Fx = 0;

            double middle = stat.means(series, i0, n_);

            for (int i = i0; i < n_; ++i)
            {
                if (index == 0)
                    Fx = 1;     //const
                else if (index == 1)
                    Fx = (double)(i + 1) / n_;   //linear 1
                else if (index == 2)
                    Fx = -(double)(i + 1) / n_;      //linear 2
                else if (index == 3)
                    Fx = (double)((i + 1) / n_) * (double)((i + 1) / n_);   //parabola 1
                else if (index == 4)
                    Fx = -(double)((i + 1) / n_) * (double)((i + 1) / n_);      //parabola 2 

                double val = series[i] - Fx * lambda * middle;
                if (val >= 0)
                    mods.Add(val);
                else
                {
                    negCount = negCount + 1;
                    mods.Add(0);
                }
            }
            return mods;
        }


        public double[,] getModifiers(double[,] modifiers, List<double> mods, int n_)
        {
            int k = 0;
            for (int j = 0; j < index_size; j++)
            {
                for (int i = 0; i < n_; i++)
                {
                    modifiers[j, i] = mods[k];
                    k++;
                }
            }
            return modifiers;
        }


        public double[] tempMods(double[,] mods, int index, int j)
        {
            double[] tempMods = new double[mods.Length];
            int k = 0;
            for (int i = mods.GetLength(1) / lam_len * j; i < mods.GetLength(1) / lam_len * (j + 1); i++)
            {
                tempMods[k] = mods[index, i];
                k++;
            }
            return tempMods;
        }


        public void calcParameters(double[,,] mParams, double[] series, int  index, int j)
        {
            mParams[index, j, 0] = stat.mean(series);
            mParams[index, j, 1] = stat.deviation(series);
            mParams[index, j, 2] = stat.variation(series);
            mParams[index, j, 3] = stat.skewness(series);
            mParams[index, j, 4] = stat.correlation(series);
            mParams[index, j, 5] = mParams[index, j, 3] == 0 ? 0 : mParams[index, j, 4] / mParams[index, j, 3];
        }


        public Transformation()
        {

        }
    }
}

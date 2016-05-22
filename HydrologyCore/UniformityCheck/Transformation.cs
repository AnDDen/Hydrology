using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniformityCheck
{
    class Transformation
    {
        public double[,] modifiers1;
        public double[,] modifiers2;
        public double[] params1 = new double[6];
        public double[] params2 = new double[6];
        public int N;
        public Statistic stat = new Statistic();


        public Transformation(double[] series, double[] lambda_items, double[] udm, double n, double autocorr)
        {
            //параметры для входной последовательности 1000 (для статистики).
            double m_vkMean = stat.mean(series);
            double m_vkDev = stat.deviation(series);
            double m_vkVariation = stat.variation(series);
            double m_vkSkew = stat.skewness(series);
            double m_vkCorr = stat.correlation(series);
            double m_vkEta = m_vkSkew / m_vkVariation;

            //разбиение
            N = series.Length;          
            int n1 = Convert.ToInt32(N * n);
            //int n2 = series.Length - n1;
            List<double> mods1 = new List<double>();
            List<double> mods2 = new List<double>();
            int negCount = 0;

            for (int index = 0; index < 6; index++)
            {
                for (int i = 0; i < lambda_items.Length; i++)
                {
                    double lambda = lambda_items[i];
                    mods1 = transform(series, mods1, 0, n1, index, lambda, negCount, udm);
                    mods2 = transform(series, mods2, n1, N, index, lambda, negCount, udm);
                }

            }
            modifiers1 = new double [6, mods1.Count/6];
            modifiers2 = new double [6, mods2.Count/6];
            getModifiers(modifiers1, mods1, mods1.Count/6);
            getModifiers(modifiers2, mods2, mods2.Count/6);

            for (int index = 0; index < 6; index++)                 
            {
                calcParameters(params1, tempMods(modifiers1, index));
                calcParameters(params2, tempMods(modifiers2, index));
            }

            Student stud = new Student(modifiers1, modifiers2, autocorr, lambda_items.Length);
        }


        public List<double> transform(double[] series, List<double> mods, int i0, int n_, int index, double lambda, int negCount, double[] udm)
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
                else if (index == 5)
                {
                    idx = Convert.ToInt32(i / n_ * 50);
                    if (idx < 0 || idx >= udm.Length)
                        Fx = 0;
                    else
                        Fx = udm[idx];
                }

                //double l = Fx*lambda*middle;
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
            for (int j = 0; j < 6; j++)
            {
                for (int i = 0; i < n_; i++)
                {
                    modifiers[j, i] = mods[k];
                    k++;
                }
            }
            return modifiers;
        }


        public double[] tempMods(double[,] mods, int index)
        {
            int length = mods.GetLength(1);
            double[] tmpMods = new double[length];
            for (int i = 0; i < mods.GetLength(1); i++)
            {
                tmpMods[i] = mods[index, i];
            }
            return tmpMods;
        }


        public void calcParameters(double[] mParams, double[] series)
        {
            mParams[0] = stat.mean(series);
            mParams[1] = stat.deviation(series);
            mParams[2] = stat.variation(series);
            mParams[3] = stat.skewness(series);
            mParams[4] = stat.correlation(series);
            mParams[5] = mParams[3] == 0 ? 0 : mParams[4] / mParams[3];
        }


        public Transformation()
        {

        }
    }
}

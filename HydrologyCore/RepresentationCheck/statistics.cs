using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepresentationCheck
{
    public class statistics
    {
       // public statistics(double [] x)
      //  {
             
      //  }

        //выборочное среднее
        public double Average(Double[] x, int i0, int n)
        {
            double s = 0;
            int ik = i0 + n ;
            for (int i = i0; i < ik; i++)
                s += x[i];
            return s / n;
        }
        public double Average(double [] k)
        {
            double s = 0;
            for (int i = 0; i < k.Length; i++)
                s += k[i];
            return s / k.Length;
        }


        //среднеквадратичное отклонение
        public double Sigma(Double[] x, int i0, int n)
        {
            int ik;
            double s, avr;
            if (n == 1) return 0;
            else
            {
                avr = Average(x, i0, n);
                s = 0;
                ik = i0 + n ;
                for (int i = i0; i < ik; i++)
                    s += Math.Pow(x[i] - avr, 2);
                return Math.Sqrt(s / (n - 1));
            }
        }

        //Коэффициент вариации
        public double Variation(Double[] x, int i0, int n)
        {
            double s = Sigma(x, i0, n), avr = Average(x, i0, n);
            if (avr != 0)
                return s / avr;
            else
                return 0;
        }

        //коэффициент ассиметрии
        public double Assimetria(Double[] x, int i0, int n)
        {
            double sig, avr, s = 0;
            int ik, i;
            if ((n == 1) | (n == 2)) return 0;
            else
            {
                avr = Average(x, i0, n);
                sig = Sigma(x, i0, n);
                ik = i0 + n - 1;
                for (i = i0; i <= ik; i++)
                    s += Math.Pow(x[i] - avr, 3);
                return s * n / ((n - 1) * (n - 2) * Math.Pow(sig, 3));
            }
        }

        //Коэффициент корреляции
        public double Correlation(Double[] x, int i0, int n)
        {
            double avr1 = Average(x, i0, n - 1),
                avr2 = Average(x, i0 + 1, n),
                s1 = 0, s2 = 0, s3 = 0;
            int ik = i0 + n - 2, i;
            for (i = i0; i <= ik; i++)
            {
                s1 += (x[i] - avr1) * (x[i + 1] - avr2);
                s2 += Math.Pow(x[i] - avr1, 2);
                s3 += Math.Pow(x[i + 1] - avr2, 2);
            }
            return s1 / Math.Pow(s2 * s3, 2);
        }
        //стандарты
        public double standart(double [] k)
        {
            double s = 0;
            double avg_k = Average(k);
            for (int i = 0; i < k.Length; i++)
                s += Math.Pow(k[i] - avg_k,2);
            return Math.Sqrt(s / k.Length);

        }
        public double[] probability(double[] k)
        {
            int N = k.Length;
            double[] p = new double[N];
            for (int i = 0; i < N; i++)
                p[i] = (double)(i + 1) / (double)(N + 1);
            return p;
        }
        

        //сортировка
         public double [] sort_shell(double [] a,  int N)             
        {
            double tmp;
            for (int gap = N / 2; gap > 0; gap /= 2)
                for (int i = gap; i < N; i++)
                    for (int j = i - gap; j >= 0 && (a[j + gap] > a[j]); j -= gap)
                    {
                        tmp = a[j];
                        a[j] = a[j+gap];
                        a[j+gap] = tmp;
                    }
            return a;
        }      

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sequence
{
    public class Statistics
    {

        //корни уравнения n-ой степени
        static List<double> roots(List<double> factors, double eps)
        {
            if (factors.Count == 2)
            {
                List<double> f = new List<double>();
                f.Add(-factors[1] / factors[0]);
                return f;
            }
            else
                if (factors.Count == 3)
                    return roots2(factors,eps);
                else
                {
                    List<double> result = new List<double>(),
                        new_factors = new List<double>(),
                         _roots = new List<double>();

                    // Новые коэффициенты
                    for (int i = 0; i < factors.Count - 1; ++i)
                        new_factors.Add(factors[i] * (factors.Count - 1 - i));
                    _roots = roots(new_factors,eps);
                    // Если нет корней
                    if (_roots.Count == 0)
                        _roots.Add(0);

                    // Слева от минимального экстремума
                    if (Math.Abs(polynom(factors, _roots[0])) > eps)
                        if (polynom(factors, _roots[0] - 100) * polynom(factors, _roots[0]) < 0)
                            result.Add(find(factors, _roots[0] - 100, _roots[0],eps));
                    // Между соседними экстремумами
                    for (int i = 0; i < _roots.Count - 1; ++i)
                        if (Math.Abs(polynom(factors, _roots[i])) < eps)
                            result.Add(_roots[i]);
                        else
                            if (Math.Abs(polynom(factors, _roots[i + 1])) > eps && polynom(factors, _roots[i]) * polynom(factors, _roots[i + 1]) < 0)
                                result.Add(find(factors, _roots[i], _roots[i + 1],eps));
                    // Справа от максимального экстремума
                    if (Math.Abs(polynom(factors, _roots[_roots.Count - 1])) > eps)
                        if (polynom(factors, _roots[_roots.Count - 1] + 100) * polynom(factors, _roots[_roots.Count - 1]) < 0)
                            result.Add(find(factors, _roots[_roots.Count - 1], _roots[_roots.Count - 1] + 100,eps));
                    return result;
                }
        }

        // бинарный поиск нуля
        static double find(List<double> factors, double left, double right, double eps)
        {
            double center,
                left_result = polynom(factors, left),
                center_result;
            int i=0;
            do
            {
                center = (left + right) / 2;
                center_result = polynom(factors, center);
                if (left_result < 0 && center_result > 0 || left_result > 0 && center_result < 0)
                    right = center;
                else
                {
                    left = center;
                    left_result = center_result;
                }
                i++;
            }
            while(Math.Abs(center_result) > eps);
            return center;
        }
        //корни квадратного уравнения
        static List<double> roots2(List<double> factors, double eps)
        {
            List<double> result = new List<double>();
            Double D = factors[1] * factors[1] - 4 * factors[0] * factors[2];

            if (Math.Abs(D) < eps)
                result.Add(-factors[1] / (2 * factors[0]));
            else
                if (D > 0)
                {
                    Double x1 = (-factors[1] - Math.Sqrt(D)) / (2 * factors[0]),
                        x2 = (-factors[1] + Math.Sqrt(D)) / (2 * factors[0]);
                    if (x1 < x2)
                    {
                        result.Add(x1);
                        result.Add(x2);
                    }
                    else
                    {
                        result.Add(x2);
                        result.Add(x1);
                    }
                }

            return result;
        }

        // Значение полинома
        static double polynom(List<double> factors, double arg)
        {
            double res = 0;
            for (int i = 0; i < factors.Count; ++i)
                res = res * arg + factors[i];
            return res;
        }

        public double r0(double r, double cv, double eps)
        {
            List<double> factors, _roots;
            double result = 0;
            factors = new List<double> { 3.0 / Math.PI, -0.12 * Math.Pow(cv, 3.0 / 2.0), 0, 0, -r };
            _roots = roots(factors,eps);

            bool stop = false;
            for (int i = 0; i < _roots.Count && !stop; ++i)
            {
                stop = _roots[i] > 0;
                if (stop)
                {
                    double root = _roots[i];
                    result = Math.Pow(root, 4);
                }
            }

            return result;
        }
        // Следующее P
        public double nextP(double P, double r0, double delta, double eps)
        {
            double A = 3.0 * r0 * (2.0 * P - 1),
                B = 5.0 / 2.0 * r0 * r0 * (3.0 * (2.0 * P - 1) * (2.0 * P - 1.0) - 1.0),
                C = 7.0 * r0 * r0 * r0 * (2.0 * P - 1.0) * (10.0 * P * (P - 1.0) + 1.0),
                D = 9.0 * r0 * r0 * r0 * r0 * (10.0 * P * (P - 1.0) * (7.0 * P * (P - 1.0) + 2.0) + 1.0),
                E = 1.0 - delta;

            List<double> factors,
                _roots;
            double result = 0;
            factors = new List<double>();
            factors.Add(14.0 * D);
            factors.Add(5.0 * C - 35.0 * D);
            factors.Add(2.0 * B - 10 * C + 30.0 * D);
            factors.Add(A - 3.0 * B + 6.0 * C - 10.0 * D);
            factors.Add(1.0 - A + B - C + D);
            factors.Add(-E);
            while (Math.Abs(factors[0]) < eps)
            {
                factors.Remove(factors[0]);
                factors.Remove(factors[1]);
            }
            _roots = roots(factors,eps);
            bool stop = false;
            for (int i = 0; i < _roots.Count && !stop; ++i)
            {
                stop = _roots[i] > 0 && _roots[i] < 1;
                if (stop)
                    result = _roots[i];
            }
            return result;
        }
        //выборочное среднее
        public double Average(Double[] x, int i0, int n)
        {
            double s = 0;
            int ik = i0 + n;
            for (int i = i0; i < ik; i++)
                s += x[i];
            return s / n;
        }
        public double Average(List <double> k)
        {
            double s = 0;
            for (int i = 0; i < k.Count; i++)
                s += k[i];
            return s / k.Count;
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
                ik = i0 + n;
                for (int i = i0; i < ik; i++)
                    s += Math.Pow(x[i] - avr, 2);
                return Math.Sqrt(s / n);
            }
        }
        public double Sigma(List <double> x)
        {
            double s, avr;
            avr = Average(x);
            s = 0;
            for (int i = 0; i < x.Count; i++)
               s += Math.Pow(x[i] - avr, 2);
            return Math.Sqrt(s / x.Count);
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
        public double Asimmetria(Double[] x, int i0, int n)
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
                return s * (double)n / ((double)(n - 1) * (double)(n - 2) * Math.Pow(sig, 3));
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
    }
}

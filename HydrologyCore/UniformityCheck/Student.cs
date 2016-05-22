using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniformityCheck
{
    public class Student
    {
        public double[,] t;

        public Student(double[,] modifiers1, double[,] modifiers2, double autocorr, int lam_len)
        {
            t = new double[6,lam_len];
            int n1 = modifiers1.GetLength(1);
            int n2 = modifiers2.GetLength(1);
            double[] mods1 = new double[n1 / lam_len];
            double[] mods2 = new double[n2 / lam_len];
            //double[,] t_r = new double[11,6];      //0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0

            for (int index = 0; index < 6; index++)
            {
                for (int j = 0; j < lam_len; j++)
                {
                    mods1 = tempMods(modifiers1, mods1, n1, index, j, lam_len);
                    mods2 = tempMods(modifiers2, mods2, n2, index, j, lam_len);
                    t[index, j] = getT(mods1, n1 / lam_len, mods2, n2 / lam_len);
                }
            }
            //получили, например, для r=0.1 различные t при разбиении 50/50, различных формах кривых и лямбдах.
            //потом можно взять для r=0.1 разбиение 40/60, 20/80...
            //а затем брать разные r
        }

        public double [] tempMods(double[,] mods, double[] tempMods, int n_, int index, int j, int lam_len)
        {
            int k = 0;
            for (int i = n_ / lam_len * j; i < n_ / lam_len * (j + 1); i++)
                {
                    tempMods[k] = mods[index, i];
                    k++;
                }
            return tempMods;
        }

        public double getT(double[] mods1, int n1, double[] mods2, int n2)
        {
            double t = 0; double u = 0;
            double[] dx;
            double[] dy;
            double mx, my, sigma_x, sigma_y;

            dx = new double[n1];
            dy = new double[n2];

            mx = 0; //х среднее
            for (int i = 0; i < n1; i++)
                mx += mods1[i];
            mx /= n1;

            my = 0; //у среднее
            for (int i = 0; i < n2; i++)
                my += mods2[i];
            my /= n2;

            sigma_x = 0; //sigma_x^2
            for (int i = 0; i < n1; i++)
                sigma_x += Math.Pow(mods1[i] - mx, 2);
            sigma_x /= n1;

            sigma_y = 0; //sigma_y^2
            for (int i = 0; i < n2; i++)
                sigma_y += Math.Pow(mods2[i] - mx, 2);
            sigma_y /= n2;

            t = Math.Abs(mx - my);
            t /= Math.Sqrt((n1 - 1) * sigma_x + (n2 - 1) * sigma_y);
            u = n1 * n2;
            u *= (n1 + n2 - 2);
            u /= (n1 + n2);
            t *= Math.Sqrt(u);
            return t;
        }

    }
}

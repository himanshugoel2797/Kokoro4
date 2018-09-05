using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Texture
{
    class SphericalHarmonics
    {
        private object EnvMapLock;
        //private double[][] OutputData;
        //private object[] OutputLocks;
        //private int[] WriteCount;

        private double Sample(double theta, double phi, int col)
        {
            lock (EnvMapLock)
            {
                int x = (int)(phi / (2 * Math.PI) * EnvMap.Width);//(int)(Math.Sin(theta) * Math.Cos(phi) * EnvMap.Width / 2) + EnvMap.Width / 2;
                int y = (int)(theta / Math.PI * EnvMap.Height);//(int)(Math.Sin(theta) * Math.Sin(phi) * EnvMap.Height / 2) + EnvMap.Height / 2;

                if (col == 0)
                    return EnvMap.GetPixel(x, y).R / 255.0f;
                else if (col == 1)
                    return EnvMap.GetPixel(x, y).G / 255.0f;
                else
                    return EnvMap.GetPixel(x, y).B / 255.0f;
            }
        }

        /*private void Store(int x, int y, double theta, double phi, double r_d, double g_d, double b_d)
        {
            //int x = 0, y = 0, w = 0;
            //lock (EnvMapLock)
            //{
            //    x = (int)(phi / (2 * Math.PI) * OutputMap.Width);// Math.Sin(theta) * Math.Cos(phi) * OutputMap.Width / 2) + OutputMap.Width / 2;
            //    y = (int)(theta / Math.PI * OutputMap.Height);// Math.Sin(theta) * Math.Sin(phi) * OutputMap.Height / 2) + OutputMap.Height / 2;
            //    w = OutputMap.Width;
            //}
            int w = 512;

            try
            {
                lock (OutputLocks[y * w + x])
                {
                    OutputData[0][y * w + x] += r_d;
                    OutputData[1][y * w + x] += g_d;
                    OutputData[2][y * w + x] += b_d;

                    WriteCount[y * w + x]++;
                }
            }
            catch (Exception) { }
        }*/

        private double Factorial(int v)
        {
            if (v <= 1)
                return 1;

            return Factorial(v - 1) * v;
        }

        private double DFactorial(int v)
        {
            if (v <= 1)
                return 1;

            return DFactorial(v - 2) * v;
        }

        private double P(int l, int m, double theta)
        {
            if (l == 0 && m == 0)
                return 1;
            else if (l == 1)
            {
                if (m == 0)
                    return Math.Cos(theta);
                else if (m == 1)
                    return -Math.Sin(theta);
            }
            else if (l == 2)
            {
                if (m == 0)
                    return 0.5f * (3 * Math.Cos(theta) * Math.Cos(theta) - 1);
                else if (m == 1)
                    return -3 * Math.Sin(theta) * Math.Cos(theta);
                else if (m == 2)
                    return 3 * Math.Sin(theta) * Math.Sin(theta);
            }
            else if (l == 3)
            {
                if (m == 0)
                    return 0.5f * Math.Cos(theta) * (5 * Math.Cos(theta) * Math.Cos(theta) - 3);
                else if (m == 1)
                    return -3.0f / 2.0f * Math.Sin(theta) * (5 * Math.Cos(theta) * Math.Cos(theta) - 1);
                else if (m == 2)
                    return 15 * Math.Sin(theta) * Math.Sin(theta) * Math.Cos(theta);
                else if (m == 3)
                    return -15 * Math.Sin(theta) * Math.Sin(theta) * Math.Sin(theta);
            }

            throw new Exception();
        }

        private double P_recur(int l, int m, double x)
        {
            if (l == 0 && m == 0)
                return 1;

            if (Math.Abs(m) > l)
                return 0;

            if (m < 0)
            {
                m = -m;
                return Math.Pow(-1, m) * Factorial(l - m) / Factorial(l + m) * P_recur(l, m, x);
            }

            if (l == m)
            {
                return Math.Pow(-1, l) * DFactorial(2 * l - 1) * Math.Pow((1 - x * x), l * 0.5f);
            }
            else if (l == m + 1)
            {
                return x * (2 * m + 1) * P_recur(m, m, x);
            }
            else
            {
                return ((2.0f * (l - 1.0f) + 1.0f) * x * P_recur(l - 1, m, x) - ((l - 1.0f) + m) * P_recur(l - 2, m, x)) / ((l - 1.0f) - m + 1.0f);
            }
        }

        private double K(int l, int m)
        {
            return Math.Sqrt((2 * l + 1) / (4 * Math.PI) * Factorial(l - m) / Factorial(l + m));
        }

        private double Y(int l, int m, double theta, double phi)
        {
            double k = K(l, Math.Abs(m));
            double p = P_recur(l, Math.Abs(m), Math.Cos(theta));
            if (m < 0)
                return Math.Pow(-1, m) * Math.Sqrt(2) * k * Math.Sin(Math.Abs(m) * phi) * p;
            else if (m > 0)
                return Math.Pow(-1, m) * Math.Sqrt(2) * k * Math.Cos(m * phi) * p;

            return k * p;
        }

        double MonteCarlo(int l, int m, int col, int phi_sample_cnt, int theta_sample_cnt)
        {
            Random rng = new Random(0);

            double net_sample = 0;

            for (int i = 0; i < phi_sample_cnt; i++)
            {
                //phi  0->2Pi
                double phi = rng.NextDouble() * (Math.PI * 2);

                for (int j = 0; j < theta_sample_cnt; j++)
                {
                    //theta  0->Pi
                    double theta = rng.NextDouble() * Math.PI;
                    //double theta = Math.Acos(2 * rng.NextDouble() - 1);

                    //Obtain a sample
                    double sample = Sample(theta, phi, col) * Y(l, m, theta, phi) * Math.Sin(theta);

                    //sample /= Math.Cos(theta);
                    //sample /= Math.PI;
                    net_sample += sample;
                }
            }

            net_sample = net_sample * (2 * Math.PI / phi_sample_cnt) * (Math.PI / theta_sample_cnt);
            return net_sample;
        }

        double a_s(int l)
        {
            if (l == 0)
                return Math.PI;

            if (l == 1)
                return (2.0f * Math.PI) / 3.0f;

            if (l % 2 == 1)
                return 0;

            return 2 * Math.PI * Math.Pow(-1, l * 0.5f - 1) / ((l + 2) * (l - 1)) * Factorial(l) / (Math.Pow(2, l) * Math.Pow(Factorial(l / 2), 2));
        }

        /*void Render(double[][] coeff, int l, int off)
        {
            int w = 0, h = 0;
            lock (EnvMapLock)
            {
                w = 512;
                h = 256;
            }

            for (int m = -l; m <= l; m++)
            {
                coeff[0][m + l + off] *= a_s(l);
                coeff[1][m + l + off] *= a_s(l);
                coeff[2][m + l + off] *= a_s(l);
            }

            //var a_s = new double[] { Math.PI, 2.0f * Math.PI / 3.0f, Math.PI / 4, 0 };
            Parallel.For(0, h, (a) =>
            //for (int a = 0; a < w; a++)
            {
                double theta = a * Math.PI / h;
                for (int b = 0; b < w; b++)
                {
                    double phi = b * (2 * Math.PI) / w;

                    //Console.WriteLine(phi);

                    double res_r = 0;
                    double res_g = 0;
                    double res_b = 0;
                    for (int m = -l; m <= l; m++)
                    {
                        var y = Y(l, m, theta, phi);// * a_s(l);


                        res_r += coeff[0][m + l + off] * y;
                        res_g += coeff[1][m + l + off] * y;
                        res_b += coeff[2][m + l + off] * y;
                    }

                    Store(b, a, theta, phi, res_r, res_g, res_b);
                }
            }
            );
        }*/

        Bitmap OutputMap;
        Bitmap EnvMap;
        double[][] coeff;
        public void Compute(Bitmap envmap, int l = 9)
        {
            //EnvMap = (Bitmap)Image.FromFile("grace_probe_original.png");
            EnvMap = envmap;
            EnvMapLock = new object();

            //OutputMap = new Bitmap(outputWidth, outputHeight);
            //OutputLocks = new object[OutputMap.Width * OutputMap.Height];
            //OutputData = new double[3][];
            //WriteCount = new int[OutputMap.Width * OutputMap.Height];


            /*for (int i = 0; i < 3; i++)
                OutputData[i] = new double[OutputMap.Width * OutputMap.Height];

            for (int i = 0; i < OutputLocks.Length; i++)
                OutputLocks[i] = new object();
            */
            coeff = new double[3][];
            int off = 0;

            //Console.WriteLine(DateTime.Now.ToLongTimeString());

            for (int i0 = 0; i0 <= l; i0++)
            {
                Parallel.For(-i0, i0 + 1, (m) =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (coeff[i] == null) coeff[i] = new double[(l + 1) * (l + 1)];
                        coeff[i][m + i0 + off] = MonteCarlo(i0, m, i, 500, 400);
                    }
                });

                off += (2 * i0 + 1);
            }

            /*off = 0;
            for (int i0 = 0; i0 <= l; i0++)
            {
                Render(coeff, i0, off);
                off += (2 * i0 + 1);
            }

            Console.WriteLine(DateTime.Now.ToLongTimeString());

            for (int i = 0; i < OutputData[0].Length; i++)
            {
                int x = i % OutputMap.Width;
                int y = i / OutputMap.Width;

                int r = (int)(OutputData[0][i] / (Math.PI) * 255.0f);
                int g = (int)(OutputData[1][i] / (Math.PI) * 255.0f);
                int b = (int)(OutputData[2][i] / (Math.PI) * 255.0f);

                r = Math.Min(255, Math.Max(0, r));
                g = Math.Min(255, Math.Max(0, g));
                b = Math.Min(255, Math.Max(0, b));

                OutputMap.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
            OutputMap.Save("output.png"); */

            /*StreamWriter writer = new StreamWriter("coeffs.txt");
            for (int i = 0; i < coeff.Length; i++)
            {
                for (int j = 0; j < coeff[i].Length; j++)
                    writer.WriteLine(coeff[i][j]);

                writer.WriteLine();
            }
            writer.Close();
            Console.ReadLine();*/
        }

        public void Save(string file)
        {
            StreamWriter writer = new StreamWriter(file);
            for (int i = 0; i < coeff.Length; i++)
            {
                for (int j = 0; j < coeff[i].Length; j++)
                    writer.WriteLine(coeff[i][j]);

                writer.WriteLine();
            }
            writer.Close();
        }
    }
}

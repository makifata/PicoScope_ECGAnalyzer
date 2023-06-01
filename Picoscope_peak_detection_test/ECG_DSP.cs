using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MDataplot;

namespace ECG_DSP
{
    class ECG_DSP
    {
        //IIR low pass filter (Four stage low pass filter) -fast
        public WAVE LowPassFilter(WAVE inwave, int fsmpl, double fcut)
        {
            int length_1 = inwave.ValidLength + 100;
            Double[] X = new Double[length_1];
            Double[] Y = new Double[length_1];
            int i;

            for (i = 0; i < length_1; i++)
            {
                X[i] = 0;
                Y[i] = 0;
                if (i < inwave.ValidLength)
                {
                    X[i] = inwave[i].Y;
                }
            }

            //Calculate filter coefficients recursive form
            double x = Math.Exp(-2.0 * Math.PI * fcut / (double)fsmpl);
            double A0 = Math.Pow((1 - x), 4);
            double B1 = 4 * x;
            double B2 = -6 * x * x;
            double B3 = 4 * x * x * x;
            double B4 = -x * x * x * x;

            for (i = 4; i < length_1; i++)
            {
                Y[i] = X[i] * A0 + Y[i - 1] * B1 + Y[i - 2] * B2 + Y[i - 3] * B3 + Y[i - 4] * B4;
            }

            WAVE result = new WAVE(inwave.ValidLength);
            for (i = 0; i < inwave.ValidLength; i++)
            {
                result.AddPoint(inwave[i].X, Y[i]);
            }
            return result;

        }

        //Notch filter (IIR / Recursive filter)
        public WAVE notchfilter(WAVE inwave, int fsmpl, double fcnt, int bw)
        {
            int length_1 = inwave.ValidLength + 100;
            Double[] X = new Double[length_1];
            Double[] Y = new Double[length_1];
            int i;

            for (i = 0; i < length_1; i++)
            {
                X[i] = 0;
                Y[i] = 0;
                if (i < inwave.ValidLength)
                {
                    X[i] = inwave[i].Y;
                }
            }

            Double K, R;
            Double f = fcnt / (Double)fsmpl;            //fraction of sampling frequency
            R = 1 - 3 * (Double)bw/(Double)fsmpl;       //fraction of sampling frequency
            K = (1 - 2 * R * Math.Cos(2 * Math.PI * f) + R * R) / (2 - 2 * Math.Cos(2 * Math.PI * f));

            double A0 = K;
            double A1 = -2 * K * Math.Cos(2 * Math.PI * f);
            double A2 = K;
            double B1 = 2 * R * Math.Cos(2 * Math.PI * f);
            double B2 = -R * R;

            for (i = 2; i < length_1; i++)
            {
                Y[i] = X[i] * A0 + X[i - 1] * A1 + X[i - 2] * A2
                                 + Y[i - 1] * B1 + Y[i - 2] * B2;
            }

            WAVE result = new WAVE(inwave.ValidLength);
            for (i = 0; i < inwave.ValidLength; i++)
            {
                result.AddPoint(inwave[i].X, Y[i]);
            }
            return result;
        }

        public double[] PeakDetection(WAVE inwave, double fsmpl)
        {
            int i;
            //double[] sig = new Double[inwave.ValidLength];
            int N = inwave.ValidLength;

            WAVE result = new WAVE(N);
            List<double> sig = new List<double>();
            List<double> time = new List<double>();

            //create second derivertive.
            for (i = 0; i < N; i++)
            {
                time.Add(inwave[i].X);

                if (i != 0 && i != N - 1)
                {
                    sig.Add(inwave[i + 1].Y - 2 * inwave[i].Y + inwave[i - 1].Y);
                }else
                {
                    sig.Add(0.0);
                }
            }

            double average = Average(sig.ToArray());
            double stddev = StandardDeviation(sig.ToArray());

            ////removing
            //for (i = 0; i < N; i++)
            //{
            //    if (Math.Abs((sig[i]-average)) <stddev*1.5)
            //    {
            //        sig[i] = 0;
            //    }
            //}

            Boolean peak = true;
            List<double> peakpos = new List<double>();
            int ptwindow = (int)((double)fsmpl / 3.66); //3.66beat/sec is maximum HR.

            peakpos.Clear();
            while(peak)
            {
                double min = sig.Min();
                if(Math.Abs(average-min) < stddev * 1.5) { peak = false;}
                else
                {
                    int detected = sig.IndexOf(min);


                    peakpos.Add(time[detected]);
                    if (detected > ptwindow /2 && detected < sig.Count - ptwindow / 2) {
                        sig.RemoveRange (detected - ptwindow / 2, ptwindow );
                        time.RemoveRange(detected - ptwindow / 2, ptwindow );
                    }else if(detected < ptwindow / 2 && sig.Count>ptwindow)
                    {
                        sig.RemoveRange (0, detected + ptwindow / 2);
                        time.RemoveRange(0, detected + ptwindow / 2);
                    }else  if (detected + ptwindow / 2 > sig.Count && sig.Count > ptwindow)
                    {
                        sig.RemoveRange (detected - ptwindow / 2, sig.Count-detected);
                        time.RemoveRange(detected - ptwindow / 2, time.Count - detected);
                    }
                    else
                    {
                        peak = false;
                    }

                }
               
            }

            double[] arr_res = peakpos.ToArray();
            Array.Sort(arr_res);
            return arr_res;
        }

        public double Average(WAVE inwave)
        {
            double result = 0;
            int N = inwave.ValidLength;
            for (int i=0;i< N; i++)
            {
                result += inwave[i].Y;
            }
            return result / N;
        }

        public double Average(double[] inwave)
        {
            double result = 0;
            int N = inwave.Length;
            for (int i = 0; i < N; i++)
            {
                result += inwave[i];
            }
            return result / N;
        }

        public double StandardDeviation(WAVE inwave)
        {
            double dev = 0;
            double mean = Average(inwave);
            int N = inwave.ValidLength;

            for (int i = 0; i < N; i++)
            {
                dev = dev + (inwave[i].Y - mean) * (inwave[i].Y - mean);
            }
            return Math.Sqrt(dev / (N - 1));


        }

        public double StandardDeviation(double[] inwave)
        {
            double dev = 0;
            double mean = Average(inwave);
            int N = inwave.Length;

            for (int i = 0; i < N; i++)
            {
                dev = dev + (inwave[i] - mean) * (inwave[i] - mean);
            }
            return Math.Sqrt(dev / (N - 1));


        }


        public WAVE FFT(WAVE inwave, int fsmpl)
        {
            int i = 0;
            int N = inwave.ValidLength;
            double[] sig = new Double[N];
            for (i = 0; i < N; i++) sig[i] = inwave[i].Y;
            /* Forward FFT ***********************************/
            //ALGLIB FFT needs their complex structure.
            alglib.complex[] intermidiate;
            alglib.fftr1d(sig, out intermidiate);

            WAVE res = new WAVE(intermidiate.Length/2);
            for(i=0;i<intermidiate.Length/2;i++)
            {
                double abscomplex = Math.Sqrt(intermidiate[i].x * intermidiate[i].x + intermidiate[i].y * intermidiate[i].y);
                double freq = (double)fsmpl / (double)N * i;
                res.AddPoint(freq, 20*Math.Log10(abscomplex));
            }
            return res;
        }

        //filter using FFT. can be achieved by recursive filter 
        public WAVE testfilter(WAVE inwave, int fsmpl, double fcut)
        {
            int i = 0;

            double[] sig = new Double[inwave.ValidLength];
            int N = inwave.ValidLength;

            for (i = 0; i < N; i++) sig[i] = inwave[i].Y;

            /* Forward FFT ***********************************/
            //ALGLIB FFT needs their complex structure.
            alglib.complex[] intermidiate;
            alglib.fftr1d(sig, out intermidiate);

            for (i = 0; i < N; i++)
            {
                if ((double)fsmpl / N * i > fcut)
                {
                    alglib.complex H = new alglib.complex(0, 0);
                    intermidiate[i] *= H;

                }
            }

            /* Invert FFT ***********************************/
            double[] simresult = new double[N];
            alglib.fftr1dinv(intermidiate, out simresult);

            WAVE result = new WAVE(N);
            for (i = 0; i < N; i++)
            {
                result.AddPoint(inwave[i].X, simresult[i]);
            }
            return result;
        }
    }
}

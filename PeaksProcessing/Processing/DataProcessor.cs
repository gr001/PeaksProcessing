using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Processing
{
    public struct Bin
    {
        public double Min, Max;
    }
    class DataProcessor
    {
        static double Sqr(double value)
        {
            return value * value;
        }

        static double ArcLength(double [] data, int iStart, int iEnd)
        {
            double length = 0;
            for(int i=iStart; i<iEnd; i++)
            {
                length += Math.Sqrt(1 + Sqr(data[i] - data[i + 1]));
            }

            return length;
        }

        public static double [] CalculateCurvatures(double [] data)
        {
            double[] curvatures = new double[data.Length];

            for(int i=0; i<data.Length; i++)
            {
                for (int k = 1; k < data.Length / 10; k++)
                {
                    if ((i-(k+1)) < 0 || i + k + 1 >= data.Length)
                        break;

                    double d1 = Math.Sqrt(Sqr(i - k - (i + k)) + Sqr(data[i - k] - data[i + k]));
                    double d2 = Math.Sqrt(Sqr(i - (k + 1) - (i + (k + 1))) + Sqr(data[i - (k + 1)] - data[i + (k + 1)]));

                    double d3 = Math.Sqrt(Sqr(i - (i + k)) + Sqr(data[i] - data[i + k]));
                    double d4 = Math.Sqrt(Sqr(i - (i + k + 1)) + Sqr(data[i] - data[i + k + 1]));

                    double d5 = Math.Sqrt(Sqr(i - (i - k)) + Sqr(data[i] - data[i - k]));
                    double d6 = Math.Sqrt(Sqr(i - (i - (k + 1))) + Sqr(data[i] - data[i - (k + 1)]));

                    if (d1 >= d2 || d3 >= d4 || d5 >= d6)
                        continue;
                    else
                    {
                        double[] selectedPoints = new double[2 * k + 1];
                        int iPoint = 0;
                        for (int j = i - k; j <= i + k; j++)
                            selectedPoints[iPoint++] = data[j];

                        double length = 0;
                        double dx = 0, dy = 0;

                        for (int j = 0; j <= k-1; j++)
                        {
                            double lj = ArcLength(data, i - k + j, i + 1 + j);
                            length += lj;

                            double betaj = ArcLength(data, i, i + 1 + j);
                            dx += ((lj - betaj) * (i + 1 + j) + betaj * (i - k + j) - lj * i) / lj;
                            dy += ((lj - betaj) * data[i + 1 + j] + betaj * data[i - k + j] - lj * data[i]) / lj;
                        }

                        length /= k;

                        curvatures[i] = Math.Sqrt(dx * dx + dy * dy)*6/(length*length*length);
                            break;
                    }
                }
            }

            return curvatures;
        }
        public static double [] ComputeDiff(double [] data)
        {
            if (data == null || data.Length == 1)
                return null;

            double[] retData = new double[data.Length - 1];

            for (int i = 0; i < data.Length-1; i++)
                retData[i] = data[i+1] - data[i];

            return retData;
        }

        public static double? MAD(double [] data)
        {
            if (data == null)
                return null;

            double [] dataCopy = new double[data.Length];
            Array.Copy(data, dataCopy, data.Length);

            Array.Sort<double>(dataCopy);

            double median = dataCopy[data.Length / 2];

            for(int i=0; i<dataCopy.Length; i++)
                dataCopy[i] = Math.Abs(dataCopy[i] - median);

            Array.Sort<double>(dataCopy);

            return dataCopy[dataCopy.Length/2];
        }

        public static int[] ComputeHistogram(double[] data, int binsCount, out Bin[] bins)
        {
            bins = null;

            if (binsCount < 0)
                return null;

            double maxValue = double.MinValue;
            double minValue = double.MaxValue;

            foreach (var item in data)
            {
                maxValue = Math.Max(maxValue, item);
                minValue = Math.Min(minValue, item);
            }

            double a = (maxValue == minValue) ? 0 : (binsCount - 1) / (maxValue - minValue);
            double b = -a * minValue;

            int[] histogram = new int[binsCount];

            foreach (var item in data)
                ++histogram[(int)(a * item + b)];

            bins = new Bin[binsCount];
            double delta = (maxValue - minValue)/ binsCount;

            for (int i = 0; i < binsCount;i++)
            {
                bins[i].Min = minValue + i * delta;
                bins[i].Max = minValue + (i+1) * delta;
            }

            return histogram;
        }

        public delegate double DataTransform(int index, double value);
        public static double[] Transform(double[] data, DataTransform func)
        {
            if (data == null || func == null)
                return null;

            double[] retData = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
                retData[i] = func(i, data[i]);

            return retData;
        }


        //public static TVal[] Threshold<TVal>(TVal [] data, TVal threshold)
        //{
        //    if (data == null)
        //        return null;

        //    TVal[] retData = new TVal[data.Length];

        //    for(int i=0; i<data.Length; i++)
        //    {
        //        if (data[i] > threshold)
        //            retData[i] = data[i];
        //        else
        //            retData[i] = default(TVal);
        //    }

        //    return retData;
        //}

        public static int [] ThresholdIndices(IList<double> data, double threshold)
        {
            if (data == null)
                return null;

            List<int> indices = new List<int>(64);

            for(int i=0; i<data.Count; i++)
            {
                if (data[i] > threshold)
                    indices.Add(i);
            }

            return indices.ToArray();
        }

        public static double Sigma(IList<double> data)
        {
            if (data != null)
                return Sigma(data, data.Count);
            else
                return -1;
        }

        public static double Sigma(IList<double> data, int pointsCount)
        { 
            if (data == null)
                return -1;

            pointsCount = Math.Min(pointsCount, data.Count);

            if (pointsCount < 2)
                return -1;

            double average = data.Average();

            double sqrError = 0;
            for(int i=0; i< pointsCount; i++)
            {
                double val = data[i] - average;
                sqrError += val * val;
            }

            sqrError = Math.Sqrt(sqrError / (pointsCount -1));

            return sqrError;
        }

    }
}

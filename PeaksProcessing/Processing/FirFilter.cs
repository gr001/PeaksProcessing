using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Processing
{
    public enum FilterType
    {
        LowPass,
        HighPass
    }

    public sealed class FirFilterKernelDescription
    {
        public FirFilterKernelDescription()
        {
            Offset = 0;
        }

        public double[] Kernel;
        public int Offset;
    }

    public sealed class FirFilter
    {
        int m_kernelSize;
        FirFilterKernelDescription m_kernelDescription;

        FirFilter()
        {
            m_kernelSize = 0;
        }

        FirFilter(FirFilterKernelDescription descr)
        {
            m_kernelSize = descr.Kernel.Length;
            m_kernelDescription = descr;
        }

        public static FirFilter Create(double cutoffFrequency, double samplingPeriod, FilterType filterType)
        {
            var kernel = CreateGaussianFilterKernel(cutoffFrequency, samplingPeriod, filterType);

            if (kernel != null)
                return new FirFilter(kernel);

            return null;
        }

        static FirFilterKernelDescription CreateGaussianFilterKernel(double cutoffFrequency, double samplingPeriod, FilterType filterType)
        {
            double alpha = Math.Sqrt(Math.Log(2.0) / Math.PI);

            int windowLength = (int)Math.Ceiling(1.0 / (cutoffFrequency* samplingPeriod));

            FirFilterKernelDescription ret = new FirFilterKernelDescription();
            ret.Kernel = new double[2 * windowLength + 1];

            double normalizationConstant = 0;
            double evalConst = -Math.PI * Math.Pow(samplingPeriod * cutoffFrequency / alpha, 2);

            for (int i = -windowLength; i <= windowLength; i++)
                normalizationConstant += Math.Exp(evalConst * i * i);

            normalizationConstant = 1.0 / normalizationConstant;

            ret.Offset = windowLength;

            //we use spectral inversion to create a high-pass filter
            double sign = (filterType == FilterType.LowPass) ? 1 : -1;

            for (int i = 0; i <= windowLength; i++)
            {
                double value = sign * normalizationConstant * Math.Exp(evalConst * i * i);
                ret.Kernel[i + ret.Offset] = value;
                ret.Kernel[ret.Offset - i] = value;
            }

            if (filterType == FilterType.HighPass)
                ret.Kernel[ret.Offset] += 1;

            return ret;
        }

        public void FilterInPlace(IList<double> values)
        {
            if (m_kernelSize <= 0 && values == null)
                return;

            int currentWritePosition = 0;
            int currentReadPosition = 0;

            double[] history = new double[m_kernelSize];

            int readIndex = 0;
            for (; readIndex < values.Count && currentWritePosition <= m_kernelDescription.Offset; readIndex++)
                history[currentWritePosition++] = values[readIndex];

            currentWritePosition = m_kernelDescription.Offset;

            for (int writeIndex = 0; writeIndex < values.Count; writeIndex++)
            {
                double result = 0;
                for (int i = -m_kernelDescription.Offset; i < m_kernelSize - m_kernelDescription.Offset; i++)
                    result += m_kernelDescription.Kernel[i + m_kernelDescription.Offset] * history[(m_kernelSize + currentReadPosition - i) % m_kernelSize];

                currentWritePosition = (++currentWritePosition) % m_kernelSize;
                currentReadPosition = (++currentReadPosition) % m_kernelSize;

                if (readIndex == values.Count)
                    history[currentWritePosition] = 0;
                else
                {
                    history[currentWritePosition] = values[readIndex];
                    ++readIndex;
                }

                values[writeIndex] = result;
            }
        }
    };
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Settings
{
    public enum PeakDetectionModes
    {
        FirstPeak,
        MaximumPeak
    }

    public sealed class GlobalDetectionSettings : Notifiable
    {
        public PeakDetectionModes PeakDetectionMode
        {
            get { return m_peakDetectionMode; }
            set
            {
                if (m_peakDetectionMode != value)
                {
                    m_peakDetectionMode = value;
                    NotifyPropertyChanged();
                }
            }
        }
        PeakDetectionModes m_peakDetectionMode = PeakDetectionModes.FirstPeak;

        public bool InvertedSignal
        {
            get { return m_invertedSignal; }
            set
            {
                if (m_invertedSignal != value)
                {
                    m_invertedSignal = value;
                    NotifyPropertyChanged();
                }
            }
        }
        bool m_invertedSignal = false;
    }
}

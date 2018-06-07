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
        public event Action PeakDetectionModeChanged;

        public PeakDetectionModes PeakDetectionMode
        {
            get { return m_peakDetectionMode; }
            set
            {
                if (m_peakDetectionMode != value)
                {
                    m_peakDetectionMode = value;
                    var handler = PeakDetectionModeChanged;
                    if (handler != null)
                        handler();

                    NotifyPropertyChanged();
                }
            }
        }

        PeakDetectionModes m_peakDetectionMode = PeakDetectionModes.FirstPeak;
    }
}

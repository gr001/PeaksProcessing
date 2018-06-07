using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Settings
{
    public sealed class PeakDetectionSettings : Notifiable
    {
        public event Action Changed;
        public event Action<double> CalculatedThresholdChanged;

        bool m_sendChangedEvent = true;
        public int BaseLineSamplesCount
        {
            get { return m_baseLineSamplesCount; }
            set
            {
                if (m_baseLineSamplesCount != value)
                {
                    m_baseLineSamplesCount = value;
                    NotifyPropertyChanged();
                    Fire_Changed();
                }
            }
        }
        int m_baseLineSamplesCount = 200;
        public double BaseLineDurationMs { get { return 50; } }

        public int EndSampleOffsetBeforeArtefact
        {
            get { return m_endSampleOffsetBeforeArtefact; }
            set
            {
                if (m_endSampleOffsetBeforeArtefact != value)
                {
                    m_endSampleOffsetBeforeArtefact = value;
                    NotifyPropertyChanged();
                    Fire_Changed();
                }
            }
        }

        internal void Set(PeakDetectionSettings detectionSettings, bool sendChangedEvent)
        {
            m_sendChangedEvent = sendChangedEvent;

            this.BaseLineSamplesCount = detectionSettings.BaseLineSamplesCount;
            this.CalculatedThreshold = detectionSettings.CalculatedThreshold;
            this.EndSampleOffsetBeforeArtefact = detectionSettings.EndSampleOffsetBeforeArtefact;
            this.MaxSlopeAfterPeak = detectionSettings.MaxSlopeAfterPeak;
            this.MaxSlopeBeforePeak = detectionSettings.MaxSlopeBeforePeak;
            this.BaseLineValue = detectionSettings.BaseLineValue;
            this.PeakThresholdRelativeToBaseline = detectionSettings.PeakThresholdRelativeToBaseline;

            m_sendChangedEvent = true;


        }

        int m_endSampleOffsetBeforeArtefact = 20;

        public double PeakThresholdRelativeToBaseline
        {
            get { return m_peakThresholdRelativeToBaseline; }
            set
            {
                if (m_peakThresholdRelativeToBaseline != value)
                {
                    m_peakThresholdRelativeToBaseline = value;
                    NotifyPropertyChanged();
                    Fire_Changed();
                }
            }
        }
        double m_peakThresholdRelativeToBaseline = 0.1;

        public double? MaxSlopeBeforePeak
        {
            get { return m_maxSlopeBeforePeak; }
            set
            {
                if (m_maxSlopeBeforePeak != value)
                {
                    m_maxSlopeBeforePeak = value;
                    NotifyPropertyChanged();
                    Fire_Changed();
                }
            }
        }
        double? m_maxSlopeBeforePeak;

        public double? MaxSlopeAfterPeak
        {
            get { return m_maxSlopeAfterPeak; }
            set
            {
                if (m_maxSlopeAfterPeak != value)
                {
                    m_maxSlopeAfterPeak = value;
                    NotifyPropertyChanged();
                    Fire_Changed();
                }
            }
        }
        double? m_maxSlopeAfterPeak;

        public double CalculatedThreshold
        {
            get { return m_calculatedThreshold; }
            internal set
            {
                if (m_calculatedThreshold != value)
                {
                    m_calculatedThreshold = value;

                    var handler = CalculatedThresholdChanged;
                    if (handler != null)
                        handler(value);

                    NotifyPropertyChanged();
                }
            }
        }

        public double BaseLineValue { get; internal set; }

        double m_calculatedThreshold;

        void Fire_Changed()
        {
            if (m_sendChangedEvent)
            {
                var handler = Changed;
                if (handler != null)
                    handler();
            }
        }

    }
}

using Bridge;
using PeaksProcessing.Data;
using PeaksProcessing.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing.Processing
{
    public sealed class FirstPeakDetector : PeaksDetectorBase
    {
        FirstPeakDetectionSettings m_firstPeakDetectionSettings;

        internal FirstPeakDetector(DataItem item, FirstPeakDetectionSettings settings) : base(item, settings)
        {
            m_firstPeakDetectionSettings = settings;
        }

        internal static void DetectPeaks(DataItem item)
        {
            if (item == null || item.Recording == null && item.Recording.Channels == null)
                return;

            var settings = item.PeakDetectionSettings as FirstPeakDetectionSettings;
            if (settings == null)
            {
                System.Diagnostics.Debug.Assert(false);
                return;
            }

            FirstPeakDetector detector = new FirstPeakDetector(item, settings);

            if (!detector.Initialize())
                return;

            detector.LocateFirstPeaks();
        }

        void LocateFirstPeaks()
        {
            //or use MAD??

            m_firstPeakDetectionSettings.CalculatedThreshold = m_firstPeakDetectionSettings.BaseLineValue +
                2 * m_firstPeakDetectionSettings.BaseLineStd +
                m_firstPeakDetectionSettings.PeakThresholdRelativeToBaseline;

            LocateFirstPeaks(m_firstPeakDetectionSettings.CalculatedThreshold);
        }

        void LocateFirstPeaks(double peakThreshold)
        {
            m_dataItem.LocatedPeaks = null;

            if (!m_dataItem.IsAnalyzed)
                return;

            //get peaks
            List<PeakInfo> locatedPeaks = new List<PeakInfo>(1024);
            foreach (var sweep in m_dataItem.Sweeps)
            {
                var stimulus = sweep.StimulusInfo;
                int maxIndex = Math.Min(stimulus.PeakSample + m_dataItem.FoundPeriodInSamples.Value / 2, m_points.Length - 1);

                int minIndex = stimulus.PeakSample;

                for (int j = stimulus.PeakSample + 2; j < maxIndex; j++)
                {
                    if (m_points[j] < peakThreshold)
                    {
                        minIndex = j;
                        break;
                    }
                }

                int startPointIndex = -1;
                for (int j = minIndex; j < maxIndex; j++)
                {
                    if (m_points[j] >= peakThreshold)
                    {
                        startPointIndex = j;
                        break;
                    }
                }

                if (startPointIndex < 0)
                    continue;

                for (int j = startPointIndex; j < maxIndex; j++)
                {
                    if (m_points[j] > peakThreshold && m_diffs[j] < 0)
                    {
                        double maxDiff = m_diffs[j];
                        for (int k = j - 1; k != minIndex; k--)
                        {
                            if (m_diffs[k] > maxDiff)
                                maxDiff = m_diffs[k];

                            if (m_diffs[k] < 0)
                                break;
                        }

                        if (m_firstPeakDetectionSettings.MaxSlopeBeforePeak.HasValue)
                        {
                            if (maxDiff > m_firstPeakDetectionSettings.MaxSlopeBeforePeak)
                                continue;
                        }

                        double minDiff = m_diffs[j];
                        for (int k = j; k != minIndex; k++)
                        {
                            if (m_diffs[k] < minDiff)
                                minDiff = m_diffs[k];

                            if (m_diffs[k] > 0)
                                break;
                        }

                        locatedPeaks.Add(new PeakInfo() { Pos = new Point(j * m_dataItem.Recording.Xscale, m_points[j]), Sample = j, MaxDiffBeforePeak = maxDiff, MinDiffAfterPeak = minDiff, ArtifactPeakSample = stimulus.PeakSample });
                        break;
                    }
                }
            }

            m_dataItem.LocatedPeaks = locatedPeaks.ToArray();
        }
    }
}

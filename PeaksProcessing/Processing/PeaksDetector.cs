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
    class PeaksDetector
    {
        DataItem m_dataItem;
        PeakDetectionSettings m_settings;
        PeakDetectionModes m_peakDetectionMode;
        double[] m_points, m_diffs, m_filteredPoints;
        double m_filteredSignalNoiseSigma;

        internal PeaksDetector(DataItem item, PeakDetectionSettings settings, PeakDetectionModes peakDetectionMode)
        {
            m_dataItem = item;
            m_settings = settings;
            m_peakDetectionMode = peakDetectionMode;
        }

        internal static void DetectPeaks(DataItem item, PeakDetectionModes peakDetectionMode)
        {
            if (item == null || item.Recording == null && item.Recording.Channels == null)
                return;

            var detectionSettings = DeepCopyExtensions.DeepCopyByExpressionTrees.DeepCopyByExpressionTree(item.PeakDetectionSettings);
            item.Reset();

            PeaksDetector detector = new PeaksDetector(item, detectionSettings, peakDetectionMode);
            detector.DetectPeaksImpl();

            item.PeakDetectionSettings.Set(detectionSettings, false);
        }

        void DetectPeaksImpl()
        {
            var points = m_dataItem.CreateContinuousSignal();
            if (points == null)
                return;

            var diffs = DataProcessor.ComputeDiff(points.ToArray());
            if (diffs == null)
                return;

            double samplingPeriod = 1.0 / (m_dataItem.Recording.SamplingFrequency * 1000);
            double cutoffFrequency = 300;

            //bool isLowPass = false;
            var filter = PeaksProcessing.Processing.FirFilter.Create(cutoffFrequency, samplingPeriod, FilterType.HighPass);

            m_filteredPoints = new double[points.Length];
            Array.Copy(points, m_filteredPoints, points.Length);

            filter.FilterInPlace(m_filteredPoints);

            var mad = DataProcessor.MAD(m_filteredPoints);

            if (!mad.HasValue)
                return;

            m_filteredSignalNoiseSigma = 1.4826 * mad.Value;

            m_points = points;
            m_diffs = diffs;

            if (!LocateArtifacts2())
                return;

            //m_settings.BaseLineSamplesCount = m_dataItem.FoundPeriodInSamples.Value / 5;
            m_settings.BaseLineSamplesCount = (int)(m_settings.BaseLineDurationMs / m_dataItem.Recording.Xscale);

            //calculate baseline statistics
            List<double> baseLineSamples = new List<double>();
            int baseLineStartIndex = Math.Max(m_dataItem.Sweeps[0].StimulusInfo.StartSample - m_settings.BaseLineSamplesCount - m_settings.EndSampleOffsetBeforeArtefact, 0);
            int endSample = Math.Max(1, m_dataItem.Sweeps[0].StimulusInfo.StartSample - m_settings.EndSampleOffsetBeforeArtefact);

            for (int i = baseLineStartIndex; i < endSample; i++)
                baseLineSamples.Add(m_points[i]);

            if (baseLineSamples.Count < 4)
                return;

            var std = DataProcessor.Sigma(baseLineSamples);
            var mean = baseLineSamples.Average();

            m_settings.BaseLineValue = mean;
            m_settings.CalculatedThreshold = mean + 2 * std + m_settings.PeakThresholdRelativeToBaseline;

            LocatePeaks(m_settings.CalculatedThreshold);
        }

        private void LocatePeaks(double peakThreshold)
        {
            if (m_peakDetectionMode == PeakDetectionModes.FirstPeak)
                LocateFirstPeaks(peakThreshold);
            else if (m_peakDetectionMode == PeakDetectionModes.MaximumPeak)
                LocateMaximumPeaks(peakThreshold);
        }

        void LocateMaximumPeaks(double peakThreshold)
        {
            m_dataItem.LocatedPeaks = null;

            if (!m_dataItem.IsAnalyzed)
                return;

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

                double maxValue = double.MinValue;
                int maxValueSample = -1;

                for (int j = minIndex; j < sweep.EndSample; j++)
                {
                    if (m_points[j] >= peakThreshold)
                    {
                        while (j < sweep.EndSample && m_points[j] >= peakThreshold)
                        {
                            if (m_points[j] > maxValue)
                            {
                                maxValue = m_points[j];
                                maxValueSample = j;
                            }
                            j++;
                        }
                        break;
                    }
                }

                if (maxValueSample >= 0)
                    locatedPeaks.Add(new PeakInfo() { Pos = new Point(maxValueSample * m_dataItem.Recording.Xscale, m_points[maxValueSample]), Sample = maxValueSample, ArtifactPeakSample = stimulus.PeakSample });
            }

            m_dataItem.LocatedPeaks = locatedPeaks.ToArray();
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

                        if (m_settings.MaxSlopeBeforePeak.HasValue)
                        {
                            if (maxDiff > m_settings.MaxSlopeBeforePeak)
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

        private void LocateArtifactsPeaks(StimulusInfo[] stimuli)
        {
            if (!m_dataItem.IsAnalyzed)
                return;

            foreach (var stimulus in stimuli)
            {
                double maxPeak = double.MinValue;
                int peakSample = -1;

                for (int i = stimulus.StartSample; i < stimulus.StartSample + m_dataItem.FoundPeriodInSamples.Value / 10; i++)
                {
                    if (m_points[i] > maxPeak)
                    {
                        maxPeak = m_points[i];
                        peakSample = i;
                    }
                }

                stimulus.PeakSample = peakSample;
                stimulus.Peak = new Point(peakSample * m_dataItem.Recording.Xscale, maxPeak);
            }
        }

        bool LocateArtifacts2()
        {
            //locate peaks
            var thresholdHigh = m_filteredPoints.Max();// m_diffs.Max();// * 0.5;
            var thresholdLow = m_filteredSignalNoiseSigma * 4;// m_diffs.Average((item) => Math.Abs(item));
            //var thresholdLow = DataProcessor.MAD(m_diffs).Value * 4;
            double threshold = (thresholdLow + thresholdHigh) * 0.5;
            //double threshold = m_filteredSignalNoiseSigma * 6;
            //adaptive threshold - median, aproximate max peaks???

            if (this.m_dataItem.ArtifactsThreshold.HasValue)
                threshold = this.m_dataItem.ArtifactsThreshold.Value;
            else
                this.m_dataItem.ArtifactsThreshold = threshold;

            var indices = DataProcessor.ThresholdIndices(m_diffs, threshold);
            if (indices.Length < 3)
                return false;

            List<Tuple<int, int>> intervals = new List<Tuple<int, int>>();
            var refIndex = indices[0];

            for (int i = 0; i < indices.Length - 1; i++)
            {
                if (indices[i + 1] - indices[i] > 1)
                {
                    intervals.Add(new Tuple<int, int>(refIndex, indices[i]));
                    refIndex = indices[i + 1];
                }
            }

            intervals.Add(new Tuple<int, int>(refIndex, indices[indices.Length-1]));

            if (intervals.Count < 2)
                return false;

            List<int> foundArtifactsPeaksSamples = new List<int>(intervals.Count);

            foreach (var interval in intervals)
            {
                int maxValueIndex = interval.Item1;
                double maxValue = m_points[interval.Item1];

                for (int i = interval.Item1 + 1; i <= interval.Item2; i++)
                {
                    if (m_points[i] > maxValue)
                    {
                        maxValue = m_points[i];
                        maxValueIndex = i;
                    }
                }

                foundArtifactsPeaksSamples.Add(maxValueIndex);
            }

            StimulusInfo[] stimuli = new StimulusInfo[foundArtifactsPeaksSamples.Count];

            for (int i = 0; i < foundArtifactsPeaksSamples.Count; i++)
            {
                var sample = foundArtifactsPeaksSamples[i];
                //  artifactsPeaksSamples[i].Sample = sample;
                //artifactsPeaksSamples[i].Value = m_points[sample];

                stimuli[i] = new StimulusInfo() { PeakSample = sample, Peak = new Point(sample * m_dataItem.Recording.Xscale, m_points[sample]) };
            }

            HashSet<int> invalidArtifacts = new HashSet<int>();
            for (int iArtifact = 0; iArtifact < foundArtifactsPeaksSamples.Count; iArtifact++)
            {
                //find minimum
                int iDiffSampleMin = Math.Max(foundArtifactsPeaksSamples[iArtifact] - 30, 0);

                var lineStart = this.m_points[iDiffSampleMin];
                var lineEnd = this.m_points[foundArtifactsPeaksSamples[iArtifact]];


                double m = (lineEnd - lineStart) / (foundArtifactsPeaksSamples[iArtifact] - iDiffSampleMin);
                double k = lineStart - m * iDiffSampleMin;
                double distMax = double.MinValue;
                int sampleMin = -1;

                for (int i = iDiffSampleMin; i < foundArtifactsPeaksSamples[iArtifact]; i++)
                {
                    double dist = Math.Abs(k + m * i - m_points[i]) / Math.Sqrt(1 + m * m);

                    if (dist > distMax)
                    {
                        distMax = dist;
                        sampleMin = i;
                    }
                }

                if (iArtifact == 0 || stimuli[iArtifact - 1].StartSample != sampleMin)
                {
                    //  foundArtifactsPeaks[iArtifact] = sampleMin;

                    stimuli[iArtifact].StartSample = sampleMin;
                    stimuli[iArtifact].Start = new Point(sampleMin * m_dataItem.Recording.Xscale, m_points[sampleMin]);
                }
                else
                    invalidArtifacts.Add(iArtifact);
            }

            Sweep[] sweeps = new Sweep[stimuli.Length];

            for (int i = 0; i < stimuli.Length; i++)
            {
                var stimul = stimuli[i];
                int endSample = (i == stimuli.Length - 1) ? m_points.Length - 1 : stimuli[i + 1].StartSample - 1;
                sweeps[i] = new Sweep(this.m_dataItem, stimul.StartSample, endSample, stimul);
            }

            m_dataItem.Sweeps = sweeps;
            //m_dataItem.Stimuli = stimuli;
            //m_dataItem.StimuliStarts = artifactsStarts;
            m_dataItem.FoundPeriodInSamples = intervals[1].Item1 - intervals[0].Item1;

            //LocateArtifactsPeaks(stimuli);
            return true;
        }


        bool LocateArtifacts()
        {
            //locate peaks
            var thresholdHigh = m_filteredPoints.Max();// m_diffs.Max();// * 0.5;
            var thresholdLow = m_filteredSignalNoiseSigma * 4;// m_diffs.Average((item) => Math.Abs(item));
            double threshold = (thresholdLow + thresholdHigh) * 0.5;

            int peaksCount = 0;
            int foundPeriodInSamples = -1;
            List<int> foundArtifactsPeaksSamples = new List<int>();

            for (int iTest = 0; iTest < 100; iTest++)
            {
                //var indices = DataProcessor.ThresholdIndices(m_diffs, threshold);

                var indices = DataProcessor.ThresholdIndices(m_filteredPoints, threshold);

                if (indices.Length > 300)
                {
                    threshold = (threshold + thresholdHigh) * 0.5;
                    continue;
                }

                //calculate distances between peaks
                List<DistanceEx> distances = new List<DistanceEx>(indices.Length * 2);

                for (int j = 0; j < indices.Length; j++)
                {
                    for (int k = j + 1; k < indices.Length; k++)
                    {
                        var distance = indices[k] - indices[j];

                        if (distance > 10)
                            distances.Add(new DistanceEx { Dist = distance, Index1 = indices[j], Index2 = indices[k] });
                    }
                }

                distances.Sort(new Comparison<DistanceEx>((item1, item2) =>
                {
                    if (item1.Dist < item2.Dist)
                        return -1;
                    if (item1.Dist == item2.Dist)
                        return 0;
                    return 1;
                }));

                List<int> peaks = new List<int>();

                foreach (var distanceRef in distances)
                {
                    int peaksCountTmp = 0;
                    peaks.Clear();
                    peaks.Add(distanceRef.Index1);
                    peaks.Add(distanceRef.Index2);

                    foreach (var distance in distances)
                    {
                        if ((distance.Dist % distanceRef.Dist) < 50)
                        {
                            peaksCountTmp++;
                            peaks.Add(distance.Index1);
                            peaks.Add(distance.Index2);
                        }
                    }
                    if (peaksCountTmp > peaksCount)
                    {
                        peaksCount = peaksCountTmp;
                        foundPeriodInSamples = distanceRef.Dist;
                        foundArtifactsPeaksSamples = peaks;
                        peaks = new List<int>();
                    }
                }

                if (foundPeriodInSamples <= 0)
                {
                    threshold = (threshold + thresholdLow) * 0.5;
                    continue;
                }


                //check if we are missing a peak
                HashSet<int> set = new HashSet<int>(foundArtifactsPeaksSamples);

                foundArtifactsPeaksSamples = new List<int>(set);
                foundArtifactsPeaksSamples.Sort();

                bool missingPeak = false;
                for (int i = 0; i < foundArtifactsPeaksSamples.Count - 1; i++)
                {
                    if ((foundArtifactsPeaksSamples[i + 1] - foundArtifactsPeaksSamples[i] - foundPeriodInSamples) > foundPeriodInSamples / 2)
                    {
                        missingPeak = true;
                        threshold = (threshold + thresholdHigh) * 0.5;
                        break;
                    }
                }

                if (!missingPeak)
                {
                    //SampleValue[] artifactsPeaksSamples = new SampleValue[foundArtifactsPeaks.Count];

                    HashSet<int> filteredArtifactsPeaksSamples = new HashSet<int>();

                    for (int i = 0; i < foundArtifactsPeaksSamples.Count; i++)
                    {
                        var sample = foundArtifactsPeaksSamples[i];
                        var sampleValue = m_points[sample];

                        for (int j = sample + 1; j < m_points.Length; j++)
                        {
                            if (m_points[j] >= sampleValue)
                            {
                                sampleValue = m_points[j];
                                sample = j;
                            }
                            else
                            {
                                filteredArtifactsPeaksSamples.Add(sample);
                                break;
                            }
                        }
                    }

                    foundArtifactsPeaksSamples = filteredArtifactsPeaksSamples.ToList();
                    foundArtifactsPeaksSamples.Sort();

                    //select the highest peaks so the distance between them is the foundPeriodInSamples

                    List<Point> foundArtifacts = new List<Point>(foundArtifactsPeaksSamples.Count);
                    foreach (var item in foundArtifactsPeaksSamples)
                        foundArtifacts.Add(new Point(item, m_points[item]));

                    foundArtifactsPeaksSamples.Clear();
                    foreach (var item in foundArtifacts.OrderByDescending(item => item.Y))
                    {
                        foundArtifactsPeaksSamples.Add((int)item.X);

                        if (foundArtifactsPeaksSamples.Count > 2)
                        {
                            foundArtifactsPeaksSamples.Sort();
                            bool wrongPeakAdded = false;

                            for (int i = 1; i < foundArtifactsPeaksSamples.Count; i++)
                            {
                                double dist = (foundArtifactsPeaksSamples[i] - foundArtifactsPeaksSamples[i - 1]);

                                if (dist < 50)
                                {
                                    foundArtifactsPeaksSamples.Remove((int)item.X);
                                    wrongPeakAdded = true;
                                    break;
                                }
                            }

                            if (wrongPeakAdded)
                                break;
                        }
                    }

                    ////new code
                    foundArtifactsPeaksSamples = new List<int>();

                    StimulusInfo[] stimuli = new StimulusInfo[foundArtifactsPeaksSamples.Count];

                    for (int i = 0; i < foundArtifactsPeaksSamples.Count; i++)
                    {
                        var sample = foundArtifactsPeaksSamples[i];
                        //  artifactsPeaksSamples[i].Sample = sample;
                        //artifactsPeaksSamples[i].Value = m_points[sample];

                        stimuli[i] = new StimulusInfo() { PeakSample = sample, Peak = new Point(sample * m_dataItem.Recording.Xscale, m_points[sample]) };
                    }

                    HashSet<int> invalidArtifacts = new HashSet<int>();
                    for (int iArtifact = 0; iArtifact < foundArtifactsPeaksSamples.Count; iArtifact++)
                    {
                        //find minimum
                        int iDiffSampleMin = Math.Max(foundArtifactsPeaksSamples[iArtifact] - 30, 0);

                        var lineStart = this.m_points[iDiffSampleMin];
                        var lineEnd = this.m_points[foundArtifactsPeaksSamples[iArtifact]];


                        double m = (lineEnd - lineStart) / (foundArtifactsPeaksSamples[iArtifact] - iDiffSampleMin);
                        double k = lineStart - m * iDiffSampleMin;
                        double distMax = double.MinValue;
                        int sampleMin = -1;

                        for (int i = iDiffSampleMin; i < foundArtifactsPeaksSamples[iArtifact]; i++)
                        {
                            double dist = Math.Abs(k + m * i - m_points[i]) / Math.Sqrt(1 + m * m);

                            if (dist > distMax)
                            {
                                distMax = dist;
                                sampleMin = i;
                            }
                        }

                        if (iArtifact == 0 || stimuli[iArtifact - 1].StartSample != sampleMin)
                        {
                            //  foundArtifactsPeaks[iArtifact] = sampleMin;

                            stimuli[iArtifact].StartSample = sampleMin;
                            stimuli[iArtifact].Start = new Point(sampleMin * m_dataItem.Recording.Xscale, m_points[sampleMin]);
                        }
                        else
                            invalidArtifacts.Add(iArtifact);
                    }

                    //m_dataItem.ArtifactsPeaksSamples = artifactsPeaksSamples;

                    //find the stimuli starts - it is when the diff is minimum

                    //for (int iArtifact = 0; iArtifact < foundArtifactsPeaks.Count; iArtifact++)
                    //{
                    //    //find minimum
                    //    int iDiffSampleMin = Math.Max(foundArtifactsPeaks[iArtifact] - (foundPeriodInSamples >> 1), 0);

                    //    double minDiff = m_diffs[iDiffSampleMin];
                    //    int sampleMin = -1;
                    //    for (int iDiffSample = iDiffSampleMin; iDiffSample < foundArtifactsPeaks[iArtifact]; iDiffSample++)
                    //    {
                    //        var diff = m_diffs[iDiffSample];
                    //        if (diff < minDiff)
                    //        {
                    //            minDiff = diff;
                    //            sampleMin = iDiffSample;
                    //        }
                    //    }

                    //    if (iArtifact == 0 || stimuli[iArtifact - 1].StartSample != sampleMin)
                    //    {
                    //        //  foundArtifactsPeaks[iArtifact] = sampleMin;

                    //        stimuli[iArtifact].StartSample = sampleMin;
                    //        stimuli[iArtifact].Start = new Point(sampleMin * m_dataItem.Recording.Xscale, m_points[sampleMin]);
                    //    }
                    //    else
                    //        invalidArtifacts.Add(iArtifact);
                    //}

                    if (invalidArtifacts.Count != 0)
                    {
                        List<StimulusInfo> stimuliTmp = new List<StimulusInfo>();

                        for (int i = 0; i < stimuli.Length; i++)
                        {
                            if (!invalidArtifacts.Contains(i))
                                stimuliTmp.Add(stimuli[i]);
                        }
                        stimuli = stimuliTmp.ToArray();
                    }
                    //SampleValue[] artifactsStarts = new SampleValue[foundArtifactsPeaks.Count];

                    //for (int i = 0; i < foundArtifactsPeaks.Count; i++)
                    //{
                    //    var sample = foundArtifactsPeaks[i];
                    //    artifactsStarts[i].Sample = sample;
                    //    artifactsStarts[i].Value = m_points[sample];
                    //}

                    Sweep[] sweeps = new Sweep[stimuli.Length];

                    for (int i = 0; i < stimuli.Length; i++)
                    {
                        var stimul = stimuli[i];
                        int endSample = (i == stimuli.Length - 1) ? m_points.Length - 1 : stimuli[i + 1].StartSample - 1;
                        sweeps[i] = new Sweep(this.m_dataItem, stimul.StartSample, endSample, stimul);
                    }

                    m_dataItem.Sweeps = sweeps;
                    //m_dataItem.Stimuli = stimuli;
                    //m_dataItem.StimuliStarts = artifactsStarts;
                    m_dataItem.FoundPeriodInSamples = foundPeriodInSamples;

                    LocateArtifactsPeaks(stimuli);
                    return true;
                }
            }

            return false;
        }
    }
}

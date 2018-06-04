using PeaksProcessing.Data;
using PeaksProcessing.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing.ViewModel
{
    public sealed class DataItemViewModel : Notifiable
    {
        public event Action CurrentPeakIndexChanged;

        internal DataItemViewModel(DataItem item)
        {
            this.DataItem = item;
            item.LocatedPeaksUpdated += OnLocatedPeaksUpdated;
        }

        private void OnLocatedPeaksUpdated()
        {
            this.CurrentPeakIndex = -1;
            RefreshSelectedPeakInfo();
        }

        public DataItem DataItem { get; }
        public SelectedPeakInfo SelectedPeakInfo
        {
            get { return m_selectedPeakInfo; }
            set
            {
                if (m_selectedPeakInfo != value)
                {
                    m_selectedPeakInfo = value;
                    NotifyPropertyChanged();
                }
            }
        }
        SelectedPeakInfo m_selectedPeakInfo;

        public int CurrentPeakIndex
        {
            get { return m_currentPeakIndex; }
            set
            {
                if (m_currentPeakIndex != value)
                {
                    m_currentPeakIndex = value;
                    RefreshSelectedPeakInfo();

                    var handler = CurrentPeakIndexChanged;
                    if (handler != null)
                        handler();
                }
            }
        }
        int m_currentPeakIndex = -1;

        internal void MoveToPrevPeak()
        {
            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            this.CurrentPeakIndex = Math.Max(this.CurrentPeakIndex - 1, 0);
        }

        internal void MoveToNextPeak()
        {
            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            this.CurrentPeakIndex = Math.Min(this.CurrentPeakIndex + 1, this.DataItem.LocatedPeaks.Length - 1);
        }

        internal void FindPrevPeak()
        {
            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            if (this.CurrentPeakIndex < 0)
                return;

            var currentPeak = this.DataItem.LocatedPeaks[this.CurrentPeakIndex];
            int currentPeakSample = currentPeak.Sample;

            int? nearestStimulusIndex = this.DataItem.GetNearestStimulusIndex(currentPeak);

            --currentPeakSample;

            var nearestStimulusSample = this.DataItem.Sweeps[nearestStimulusIndex.Value].StimulusInfo;

            if (nearestStimulusSample.StartSample > currentPeakSample)
            {
                nearestStimulusIndex = nearestStimulusIndex.Value - 1;
                nearestStimulusSample = this.DataItem.Sweeps[nearestStimulusIndex.Value].StimulusInfo;
            }

            var contSignal = this.DataItem.CreateContinuousSignal();
            var diffSignal = DataProcessor.ComputeDiff(contSignal);

            var prevDiffValue = diffSignal[currentPeakSample];
            for (int i = currentPeakSample - 1; i > nearestStimulusSample.StartSample; i--)
            {
                var currentDiffValue = diffSignal[i];

                if (prevDiffValue < 0 && currentDiffValue >= 0)
                {
                    ++i;
                    this.DataItem.LocatedPeaks[this.CurrentPeakIndex].Sample = i;
                    this.DataItem.LocatedPeaks[this.CurrentPeakIndex].Pos = new Point(i * this.DataItem.Recording.Xscale, contSignal[i]);
                    this.RefreshSelectedPeakInfo();
                    return;
                }
                prevDiffValue = currentDiffValue;
            }
        }

        internal void FindNextPeak()
        {
            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            if (this.CurrentPeakIndex < 0)
                return;

            var currentPeak = this.DataItem.LocatedPeaks[this.CurrentPeakIndex];
            int currentPeakSample = currentPeak.Sample;

            int? nearestStimulusIndex = this.DataItem.GetNearestStimulusIndex(currentPeak);

            if (nearestStimulusIndex.HasValue)
            {
                ++currentPeakSample;
                var nearestStimulus = this.DataItem.Sweeps[nearestStimulusIndex.Value].StimulusInfo;
                int endSample = nearestStimulus.StartSample;

                if (nearestStimulus.StartSample < currentPeakSample)
                {
                    nearestStimulusIndex = nearestStimulusIndex.Value + 1;

                    if (nearestStimulusIndex >= this.DataItem.Sweeps.Length)
                        endSample = this.DataItem.SamplesCount - 1;
                    else
                        endSample = this.DataItem.Sweeps[nearestStimulusIndex.Value].StimulusInfo.StartSample;
                }

                var contSignal = this.DataItem.CreateContinuousSignal();
                var diffSignal = DataProcessor.ComputeDiff(contSignal);

                var prevDiffValue = diffSignal[currentPeakSample];
                for (int i = currentPeakSample + 1; i < endSample; i++)
                {
                    var currentDiffValue = diffSignal[i];

                    if (prevDiffValue > 0 && currentDiffValue <= 0)
                    {
                        this.DataItem.LocatedPeaks[this.CurrentPeakIndex].Sample = i;
                        this.DataItem.LocatedPeaks[this.CurrentPeakIndex].Pos = new Point(i * this.DataItem.Recording.Xscale, contSignal[i]);
                        this.RefreshSelectedPeakInfo();
                        return;
                    }

                    prevDiffValue = currentDiffValue;
                }
            }
        }

        void RefreshSelectedPeakInfo()
        {
            SelectedPeakInfo selectedPeakInfo = null;

            if (this.DataItem.LocatedPeaks != null &&
                m_currentPeakIndex >= 0 && m_currentPeakIndex < this.DataItem.LocatedPeaks.Length)
            {
                var peakInfo = this.DataItem.LocatedPeaks[m_currentPeakIndex];

                int? nearestStimulusSampleIndex = this.DataItem.GetNearestStimulusIndex(peakInfo);

                selectedPeakInfo = new SelectedPeakInfo();
                selectedPeakInfo.PeakInfo = peakInfo;

                if (nearestStimulusSampleIndex.HasValue)
                    selectedPeakInfo.DistToStimulusMs = selectedPeakInfo.PeakInfo.Pos.X - this.DataItem.Sweeps[nearestStimulusSampleIndex.Value].StimulusInfo.Start.X;
            }

            this.SelectedPeakInfo = selectedPeakInfo;
        }
        internal void AddPeakAtPosition(double timeMs)
        {
            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            int? sweepIndex = this.DataItem.GetSweepIndex(timeMs);

            if (sweepIndex.HasValue)
            {
                var sweep = this.DataItem.Sweeps[sweepIndex.Value];
                var locatedPeaks = this.DataItem.LocatedPeaks.ToList();

                var locatedPeak = locatedPeaks.FirstOrDefault(item => sweep.StartSample <= item.Sample && item.Sample <= sweep.EndSample);
                bool peakExisted = true;

                if (locatedPeak == null)
                {
                    //insert a new peak
                    locatedPeak = new Data.PeakInfo();
                    locatedPeak.ArtifactPeakSample = sweep.StimulusInfo.PeakSample;
                    peakExisted = false;
                }

                int sampleIndex = (int)(timeMs / this.DataItem.Recording.Xscale);
                var sampleValue = this.DataItem.GetSampleValueAt(sampleIndex);

                if (sampleValue.HasValue)
                {
                    locatedPeak.Pos = new Point(sampleIndex * this.DataItem.Recording.Xscale, sampleValue.Value);
                    locatedPeak.Sample = sampleIndex;

                    if (!peakExisted)
                        locatedPeaks.Add(locatedPeak);

                    locatedPeaks.Sort(new Comparison<Data.PeakInfo>((item1, item2) =>
                    {
                        if (item1.Pos.X < item2.Pos.X)
                            return -1;
                        if (item1.Pos.X == item2.Pos.X)
                            return 0;
                        return 1;
                    }));

                    this.DataItem.LocatedPeaks = locatedPeaks.ToArray();

                    for (int i = 0; i < this.DataItem.LocatedPeaks.Length; i++)
                    {
                        if (this.DataItem.LocatedPeaks[i] == locatedPeak)
                        {
                            this.CurrentPeakIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        internal void RemovePeak(PeakInfo peakInfo)
        {
            if (peakInfo == null)
                return;

            if (!this.DataItem.IsAnalyzed || this.DataItem.LocatedPeaks == null)
                return;

            var locatedPeaks = this.DataItem.LocatedPeaks.ToList();
            locatedPeaks.Remove(peakInfo);

            this.DataItem.LocatedPeaks = locatedPeaks.ToArray();
        }

        internal int? GetNearestStimulusIndex(PeakInfo peakInfo)
        {
            int? nearestStimulusStart = null;

            if (peakInfo == null || !this.DataItem.IsAnalyzed)
                return nearestStimulusStart;

            int distMin = int.MaxValue;

            for (int i = 0; i < this.DataItem.Sweeps.Length; i++)
            {
                var dist = Math.Abs(this.DataItem.Sweeps[i].StimulusInfo.StartSample - peakInfo.Sample);

                if (dist < distMin)
                {
                    distMin = dist;
                    nearestStimulusStart = i;
                }
            }

            return nearestStimulusStart;
        }
    }
}

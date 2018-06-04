using Bridge;
using PeaksProcessing.Data;
using PeaksProcessing.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing
{
    public sealed class DataItem : Notifiable
    {
        public event Action LocatedPeaksUpdated;
        public event Action SweepsChanged;
        public event Action ArtifactThresholdChanged;

        internal DataItem(RecordingNet recording)
        {
            this.Recording = recording;
            this.PeakDetectionSettings.Changed += OnPeakDetectionSettings_Changed;
        }

        public bool IsSaved
        {
            get { return m_isSaved; }
            private set
            {
                if (value != m_isSaved)
                {
                    m_isSaved = value;
                    NotifyPropertyChanged();
                }
            }
        }
        bool m_isSaved;

        public RecordingNet Recording { get; }

        public double? GetSampleValueAt(int sample)
        {
            int currentSample = 0;
            foreach (var serie in this.Recording.Channels[0].Sections)
            {
                foreach (var point in serie.Data)
                {
                    if (currentSample == sample)
                        return point;

                    ++currentSample;
                }
            }

            return null;
        }

        public double[] CreateContinuousSignal()
        {
            //double x = 0;

            List<double> points = new List<double>(1024);
            //List<double> xs = new List<double>(1024);

            foreach (var serie in this.Recording.Channels[0].Sections)
            {
                foreach (var point in serie.Data)
                {
                    points.Add(point);
                    //      xs.Add(x);
                    //    x +=  serie.Xscale;
                }
            }

            if (points.Count < 1000)
                return null;

            this.SamplesCount = points.Count;
            return points.ToArray();
        }

        internal void SaveResults(string folderPath)
        {
            try
            {
                StringBuilder fileContent = new StringBuilder(2048);

                if (this.IsAnalyzed)
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(this.Recording.FilePath);

                    var outFilePath = System.IO.Path.Combine(folderPath, filename + ".csv");

                    fileContent.Clear();
                    fileContent.AppendLine("Stimulus position [ms], Peak position [ms], Peak offset [ms], Peak amplitude relative to baseline");

                    //ukladat prazdny radek pouze se stimulus time kdyz se nic nenajde
                    
                    foreach (var peakInfo in this.LocatedPeaks)
                    {
                        int? nearestStimulusIndex = GetNearestStimulusIndex(peakInfo);

                        if (nearestStimulusIndex.HasValue)
                        {
                            double stimulusTime = this.Sweeps[nearestStimulusIndex.Value].StimulusInfo.Start.X;
                            double distToStimulusMs = peakInfo.Pos.X - stimulusTime;
                            
                            fileContent.AppendFormat($"{stimulusTime}, {peakInfo.Pos.X}, {distToStimulusMs}, {peakInfo.Pos.Y - this.PeakDetectionSettings.BaseLineValue}\n");
                        }
                    }

                    File.WriteAllText(outFilePath, fileContent.ToString());
                    this.IsSaved = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save results failed:\n\nException: " + ex.ToString());
            }
        }

        private void OnPeakDetectionSettings_Changed()
        {
            Processing.PeaksDetector.DetectPeaks(this, Settings.Settings.Instance.GlobalDetectionSettings.PeakDetectionMode);
        }

        //public double[] Xs { get; private set; }

        //public SampleValue[] ArtifactsPeaksSamples
        //{
        //    get { return m_artifactsPeaksSamples; }
        //    internal set
        //    {
        //        if (m_artifactsPeaksSamples != value)
        //        {
        //            m_artifactsPeaksSamples = value;
        //            var handler = ArtifactsPeaksChanged;
        //            if (handler != null)
        //                handler();
        //        }
        //    }
        //}
        //SampleValue[] m_artifactsPeaksSamples;

        //public SampleValue[] StimuliStarts
        //{
        //    get { return m_stimuliStarts; }
        //    internal set
        //    {
        //        if (m_stimuliStarts != value)
        //        {
        //            m_stimuliStarts = value;
        //            var handler = StimuliStartsChanged;
        //            if (handler != null)
        //                handler();
        //        }
        //    }
        //}
        //SampleValue[] m_stimuliStarts;

        public Sweep[] Sweeps
        {
            get { return m_sweeps; }
            internal set
            {
                if (m_sweeps != value)
                {
                    m_sweeps = value;
                    var handler = SweepsChanged;
                    if (handler != null)
                        handler();
                }
            }
        }
        Sweep[] m_sweeps;

        //public StimulusInfo[] Stimuli
        //{
        //    get { return m_stimuli; }
        //    internal set
        //    {
        //        if (m_stimuli != value)
        //        {
        //            m_stimuli = value;
        //            var handler = StimuliStartsChanged;
        //            if (handler != null)
        //                handler();
        //        }
        //    }
        //}
        //StimulusInfo [] m_stimuli;

        public double? ArtifactsThreshold
        {
            get { return m_artifactThreshold; }
            set
            {
                if (m_artifactThreshold != value)
                {
                    bool recalculateSweeps = m_artifactThreshold.HasValue;
                    m_artifactThreshold = value;
                    NotifyPropertyChanged();

                    if (recalculateSweeps)
                        Processing.PeaksDetector.DetectPeaks(this, Settings.Settings.Instance.GlobalDetectionSettings.PeakDetectionMode);
                }
            }
        }
        double? m_artifactThreshold;

        public int? FoundPeriodInSamples
        {
            get { return m_foundPeriodInSamples; }
            set
            {
                if (m_foundPeriodInSamples != value)
                {
                    m_foundPeriodInSamples = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("IsAnalyzed");
                }
            }
        }
        int? m_foundPeriodInSamples;

        public PeakInfo[] LocatedPeaks
        {
            get { return m_locatedPeaks; }
            set
            {
                if (m_locatedPeaks != value)
                {
                    m_locatedPeaks = value;
                    NotifyPropertyChanged();

                    var handler = LocatedPeaksUpdated;
                    if (handler != null)
                        handler();

                }
            }
        }
        PeakInfo [] m_locatedPeaks;

        public PeakDetectionSettings PeakDetectionSettings { get; } = new PeakDetectionSettings();

        public bool IsAnalyzed
        {
            get { return m_foundPeriodInSamples.HasValue; }
        }

        public void Reset()
        {
            this.FoundPeriodInSamples = null;
            this.Sweeps = null;
            this.LocatedPeaks = null;
        }

        public int SamplesCount { get; private set; }

        internal int? GetNearestStimulusIndex(PeakInfo peakInfo)
        {
            return peakInfo != null ? GetNearestStimulusIndex(peakInfo.Pos.X) : null;
        }

        internal int? GetNearestStimulusIndex(double timeInMs)
        {
            int? nearestStimulusIndex = null;

            if (this.Sweeps != null)
            {
                double distMin = double.MaxValue;

                for (int i = 0; i < this.Sweeps.Length; i++)
                {
                    var dist = timeInMs - this.Sweeps[i].StimulusInfo.Start.X;

                    if (dist >= 0 && dist < distMin)
                    {
                        distMin = dist;
                        nearestStimulusIndex = i;
                    }
                }
            }

            return nearestStimulusIndex;
        }        

        internal int? GetSweepIndex(double timeMs)
        {
            for(int i=0; i<this.Sweeps.Length; i++)
            {
                if (this.Sweeps[i].IsTimeInside(timeMs))
                    return i;
            }

            return null;
        }
    }
}

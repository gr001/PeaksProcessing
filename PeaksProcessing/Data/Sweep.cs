using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.Data
{
    public sealed class Sweep
    {
        public Sweep(DataItem item, int startSample, int endSample, StimulusInfo stimulus)
        {
            this.StartSample = startSample;
            this.Start = startSample * item.Recording.Xscale;

            this.EndSample = endSample;
            this.End = endSample * item.Recording.Xscale;

            this.DataItem = item;

            this.StimulusInfo = stimulus;
        }
        public DataItem DataItem { get; }

        public double Start { get; }
        public double End { get; }
        public int StartSample { get; }
        public int EndSample { get; }

        public StimulusInfo StimulusInfo { get; }

        public bool IsSampleInside(int sample)
        {
            return StartSample <= sample && sample <= EndSample;
        }

        internal bool IsTimeInside(double timeMs)
        {
            return Start <= timeMs && timeMs <= End;
        }
    }
}

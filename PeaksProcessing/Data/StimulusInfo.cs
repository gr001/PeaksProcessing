using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing.Data
{
    public sealed class StimulusInfo
    {
        public Point Start { get; set; }
        public int StartSample { get; set; }

        public Point Peak { get; set; }
        public int PeakSample { get; set; }
    }
}

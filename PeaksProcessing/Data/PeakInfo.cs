using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PeaksProcessing.Data
{
    public class PeakInfo
    {
        public Point Pos { get; set; }
        public double MaxDiffBeforePeak { get; set; }
        public double MinDiffAfterPeak { get; set; }
        public int Sample { get; set; }

        public int ArtifactPeakSample { get; set; }
    }
}

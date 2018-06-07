using PeaksProcessing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeaksProcessing.ViewModel
{
    public class SelectedPeakInfo
    {
        public PeakInfo PeakInfo { get; set; }
        public double DistToStimulusMs { get; set; }
    }
}

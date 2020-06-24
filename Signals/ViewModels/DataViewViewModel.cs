using Signals.Models;
using System.Collections.Generic;

namespace Signals
{
    public class DataViewViewModel
    {
        public int? Frequency { get; set; }
        public int? Amplitude { get; set; }
        public int? Phase { get; set; }
        public int PointCount { get; set; }
        public int SampleRate { get; set; }
        public double SamplePeriod { get; set; }
        public string PlotName { get; set; }
        public List<DataViewModel> DataViewModels { get; set; }
    }
}

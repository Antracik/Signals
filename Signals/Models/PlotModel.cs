using System;
using System.Collections.Generic;
using System.Text;
using ScottPlot;

namespace Signals.Models
{
    public class PlotModel
    { 
        public PlottableSignal Plot { get; set; }
        public bool Visible { get; set; } = true;
        public string Name => Plot.label;
        public int? Frequency { get; set; }
        public int? Phase { get; set; }
        public int? Amplitude { get; set; }
        public int PointCount => Plot.GetPointCount();
        public double SampleRate => Plot.sampleRate;
        public double SamplePeriod => Plot.samplePeriod;
    }
}

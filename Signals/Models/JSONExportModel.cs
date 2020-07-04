using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Signals.Models
{
    public class JSONPlotModel
    {
        public string Name { get; set; }
        public int? Frequency { get; set; }
        public int? Phase { get; set; }
        public int? Amplitude { get; set; }
        public int PointCount { get; set; }
        public double SampleRate { get; set; }
        public double SamplePeriod { get; set; }
        public double[] Data { get; set; }

    }
}

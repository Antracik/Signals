using System;
using System.Collections.Generic;
using System.Text;
using ScottPlot;

namespace Signals.Models
{
    public class PlotModel
    { 
        public PlottableScatter Plot { get; set; }
        public bool Visible { get; set; } = true;

        public string Name => Plot.label;
        public int PointCount => Plot.pointCount;
    }
}

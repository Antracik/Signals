using Signals.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Signals.ViewModels
{
    public class FFTAnalysisViewModel
    {
        public string PlotName { get; set; }
        public List<FFTAnalysisModel> FFTAnalysisModels { get; set; }
    }
}

using Signals.Models;
using System.Collections.Generic;

namespace Signals.ViewModels
{
    public class FFTAnalysisViewModel
    {
        public string PlotName { get; set; }
        public List<FFTAnalysisModel> FFTAnalysisModels { get; set; }
    }
}

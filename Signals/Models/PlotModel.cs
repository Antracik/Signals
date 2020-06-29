﻿using ScottPlot;
using System.ComponentModel;

namespace Signals.Models
{
    public class PlotModel : INotifyPropertyChanged
    {
        public PlottableSignal Plot { get; set; }

        public WpfPlot ParentPlot { get; set; }

        private bool _visible = true;

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    Plot.visible = value;
                    ParentPlot.Render();
                    OnPropertyChanged(nameof(Visible));
                }
            }
        }


        public string Name => Plot.label;
        public int? Frequency { get; set; }
        public int? Phase { get; set; }
        public int? Amplitude { get; set; }
        public int PointCount => Plot.GetPointCount();
        public double SampleRate => Plot.sampleRate;
        public double SamplePeriod => Plot.samplePeriod;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using MathNet.Numerics;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Signals.Models
{
    public class AddWaveModel : BaseModel
    {
        public WpfPlot ParentPlot { get; set; }

        private string _plotName = "NewCustomPlot";

        public string PlotName
        {
            get { return _plotName; }
            set
            {
                if (_plotName != value)
                {
                    _plotName = value;
                    OnPropertyChanged("PlotName");
                    UpdatePlot();
                }
            }
        }

        private int _sampleRate = 1000;

        public int SampleRate
        {
            get { return _sampleRate; }
            set
            {
                if (_sampleRate != value)
                {
                    _sampleRate = value;
                    OnPropertyChanged("SampleRate");
                    UpdatePlot();
                }
            }
        }

        private int _pointCount = 1000;

        public int PointCount
        {
            get { return _pointCount; }
            set
            {
                if (_pointCount != value)
                {
                    _pointCount = value;
                    OnPropertyChanged("PointCount");
                    UpdatePlot();
                }
            }
        }

        private int _frequency = 20;

        public int Frequency
        {
            get { return _frequency; }
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    OnPropertyChanged("Frequency");
                    UpdatePlot();
                }
            }
        }


        private int _amplitude = 10;

        public int Amplitude
        {
            get { return _amplitude; }
            set
            {
                if (_amplitude != value)
                {
                    _amplitude = value;
                    OnPropertyChanged("Amplitude");
                    UpdatePlot();
                }
            }
        }

        private int _phase = 0;

        public int Phase
        {
            get { return _phase; }
            set
            {
                if (_phase != value)
                {
                    _phase = value;
                    OnPropertyChanged("Phase");
                    UpdatePlot();
                }
            }
        }

        private void UpdatePlot()
        {
            ParentPlot.plt.Clear();


            _pointCount = _pointCount > 0 ? _pointCount : 1;

            ParentPlot.plt.PlotSignal(Generate.Sinusoidal(_pointCount, _sampleRate, _frequency, _amplitude, phase: _phase), _sampleRate, label: _plotName);
            
            ParentPlot.Render(true);
        }

        
    }
}

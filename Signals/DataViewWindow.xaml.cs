using ScottPlot;
using Signals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Signals
{
    /// <summary>
    /// Interaction logic for DataViewWindow.xaml
    /// </summary>
    public partial class DataViewWindow : Window
    {
        private DataViewViewModel Instance { get; set; }

        public DataViewWindow()
        {
            InitializeComponent();
        }

        public DataViewWindow(PlotModel model)
        {
            InitializeComponent();

            Instance = new DataViewViewModel
            {
                Amplitude = model.Amplitude,
                Frequency = model.Frequency,
                Phase = model.Phase,
                PlotName = model.Name,
                PointCount = model.PointCount,
                SamplePeriod = model.SamplePeriod,
                SampleRate = (int)model.SampleRate,
                DataViewModels = Utility.ExtractDataModel(model).ToList()
            };

            DataContext = Instance;
        }
    }
}

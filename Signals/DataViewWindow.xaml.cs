using Signals.Models;
using System.Linq;
using System.Windows;

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

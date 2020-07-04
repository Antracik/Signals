
using ScottPlot;
using Signals.Models;
using Signals.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Signals
{
    /// <summary>
    /// Interaction logic for AddWaveWindow.xaml
    /// </summary>
    public partial class AddWaveWindow : Window
    {
        private AddWaveViewModel Instance { get; set; }

        public PlotModel ResultPlot { get; private set; }

        public AddWaveWindow()
        {
            InitializeComponent();

            AddWavePlot.ContextMenu = null;

            Instance = new AddWaveViewModel
            {
                WaveModel = new AddWaveModel
                {
                    ParentPlot = AddWavePlot
                },


                RandomWaveCommand = new GenerateRandomWaveCommand(),
                AddWaveCommand = new AddWaveCommand(AddWaveAndExit)
            };

            AddWavePlot.plt.PlotSignal(MathNet.Numerics.Generate.Sinusoidal(Instance.WaveModel.PointCount, Instance.WaveModel.SampleRate, Instance.WaveModel.Frequency, Instance.WaveModel.Amplitude, phase: Instance.WaveModel.Phase), Instance.WaveModel.SampleRate, label: Instance.WaveModel.PlotName);
            
            DataContext = Instance;
            
            AddWavePlot.Render(true);
        }

        private void AddWaveAndExit()
        {
            if (string.IsNullOrWhiteSpace(Instance.WaveModel.PlotName))
                Instance.WaveModel.PlotName = "NewCustomWave";

            var model = Instance.WaveModel;
            ResultPlot = new PlotModel
            {
                Amplitude = model.Amplitude,
                Phase = model.Phase,
                Frequency = model.Frequency,
                Plot = (PlottableSignal)Instance.WaveModel.ParentPlot.plt.GetPlottables().First()
            };

            DialogResult = true;
            Close();
        }

        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Utility.IsTextAllowed(e.Text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!Utility.IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text;
                if (text.Contains(" "))
                    text = text.Replace(" ", "");

                if (text.Length > 1)
                    text = text.TrimStart('0');

                if (string.IsNullOrWhiteSpace(text))
                    text = "0";

                textBox.Text = text;
            }
        }
    }
}

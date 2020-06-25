using MathNet.Numerics.IntegralTransforms;
using ScottPlot;
using Signals.Models;
using Signals.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#pragma warning disable CS0618 
namespace Signals
{
    /// <summary>
    /// Interaction logic for FFTAnalysis.xaml
    /// </summary>
    public partial class FFTAnalysisWindow : Window
    {

        private FFTAnalysisViewModel Instance { get; set; }
        //Not sure if I can bind these to XAML in ScottPlot so for now use them as they are
        private PlottableSignalConst<double> frequencyDomainPlot;
        private PlottableScatter plottableScatter;
        private PlottableText plottableText;
        private double[] xs;

        public FFTAnalysisWindow()
        {
            InitializeComponent();
        }
        public FFTAnalysisWindow(PlotModel model)
        {
            InitializeComponent();


            var magYs = PrepareFFT(model.Plot.ys, model.Plot.GetPointCount(), (int)model.Plot.sampleRate);
            Instance = new FFTAnalysisViewModel
            {
                FFTAnalysisModels = Utility.FitFFTModel(magYs).ToList(),
                PlotName = model.Name
            };

            DataContext = Instance;
            FFTPlot.plt.Ticks(useExponentialNotation: false, useMultiplierNotation: false, useOffsetNotation: false, rulerModeX: true, rulerModeY: true, logScaleX: true);
            FFTPlot.plt.Title($"FFT Analysis of {Instance.PlotName}");
            FFTPlot.plt.XLabel("Frequency (Hz)", fontSize: 18, bold: true);
            FFTPlot.plt.YLabel("Magnitude", fontSize: 18, bold: true);
            frequencyDomainPlot = FFTPlot.plt.PlotSignalConst(magYs);
            plottableText = FFTPlot.plt.PlotText("", .0, .0);
            plottableScatter = FFTPlot.plt.PlotPoint(.0, .0);
            xs = DataGen.Consecutive(magYs.Length);
            FFTPlot.MouseMove += OnMouseMove_Plot;
            FFTPlot.Render();
        }

        private double[] PrepareFFT(double[] dataY, int sampleSize, int sampleRate)
        {
            //find if the sample size is bigger than the sample rate, if it is then we ignore the extra data
            if (sampleSize > sampleRate)
                sampleSize = sampleRate;

            //find if the sample rate is bigger than the supplied samples and later pad with zeros if needed
            int difference = 0;

            if (sampleRate > sampleSize)
                difference = sampleRate - sampleSize;

            Complex[] samples = new Complex[sampleSize + difference];
            var tempPlotYs = dataY;

            for (int i = 0; i < sampleSize; i++)
                samples[i] = new Complex(tempPlotYs[i], 0d);

            //pad with zeroz if needed
            if (difference > 0)
                for (int i = samples.Length - 1; i < difference; i++)
                    samples[i] = 0;

            Fourier.Forward(samples, FourierOptions.NoScaling);
            double[] magYs = new double[samples.Length / 2];
            //double[] angleYs = new double[samples.Length / 2];

            for (int i = 0; i < samples.Length / 2; i++)
            {
                magYs[i] = (2.0 / sampleSize) * samples[i].Magnitude;
                //angleYs[i] = samples[i].Phase;
            }

            return magYs;
        }

        private void OnMouseMove_Plot(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) return;
            if (e.RightButton == MouseButtonState.Pressed) return;
            if (e.MiddleButton == MouseButtonState.Pressed) return;

            // determine where the mouse is in coordinate space
            var (xPoint, yPoint) = FFTPlot.GetMouseCoordinates();
            var pos = new Point(xPoint, yPoint);

            // determine which point is closest to the mouse
            int closestIndex = 0;
            double closestDistance = double.PositiveInfinity;
            for (int i = 0; i < frequencyDomainPlot.ys.Length; i++)
            {
                double dx = xs[i] - pos.X;
                double dy = frequencyDomainPlot.ys[i] - pos.Y;
                double distance = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                if (distance < closestDistance)
                {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }

            if (closestDistance < 1)
            {
                double x = xs[closestIndex];
                double y = frequencyDomainPlot.ys[closestIndex];

                plottableScatter.visible = true;
                plottableScatter.xs[0] = x;
                plottableScatter.ys[0] = y;

                plottableText.visible = true;
                plottableText.text = $"  X({Math.Round(x, 3)}), Y({Math.Round(y, 3)})";
                plottableText.x = x;
                plottableText.y = y;
            }
            else
            {
                plottableScatter.visible = false;
                plottableText.visible = false;
            }

            FFTPlot.Render();
        }

    }
}

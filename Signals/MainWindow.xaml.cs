using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MathNet.Numerics;
using ScottPlot;
using Signals.Models;
using MathNet.Numerics.IntegralTransforms;
using static Signals.Utility;
using static ScottPlot.DataGen;
using Color = System.Drawing.Color;
using Window = System.Windows.Window;
using System.Numerics;
#pragma warning disable CS0618 
namespace Signals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private values

        private const int pointCount = 100;
        private const int sampleRate = 500;
        private Point mousePosition;
        private PlottableVLine _plottedLine1;
        private PlottableVLine _plottedLine2;
        private PlottableHSpan _plottedSpan;
        private MenuItem _line;
        private MenuItem _clearLines;
        private MenuItem _openWindow;
        private ContextMenu _plotMenu;
        private ObservableCollection<PlotModel> plots = new ObservableCollection<PlotModel>();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializePlot();
        }

        public MainWindow(ObservableCollection<PlotModel> plots)
        {
            InitializeComponent();
            this.plots = plots;
            foreach (var plotModel in this.plots)
            {
                var plot = plotModel.Plot;
                SinePlot.plt.PlotSignal(plot.ys, plot.sampleRate);
            }
            InitializePlot(true);
        }

        private void InitializePlot(bool skipDemoSineWave = false)
        {
            if (!skipDemoSineWave)
            {
                var waveList = new List<(double[] ys,int frequency,int amplitude,int phase)>();

                int waveCount = 3;

                for (int i = 0; i < waveCount; i++)
                {
                    var temp = GenerateSinusoidal(pointCount, sampleRate);
                    waveList.Add(temp);
                }

                waveList = waveList.OrderBy(x => x.frequency).ToList();

                for (int i = 0; i < waveCount; i++)
                {
                    plots.Add(new PlotModel { Plot = SinePlot.plt.PlotSignal(waveList[i].ys, sampleRate, label: $"Inner Wave{i}"), Amplitude = waveList[i].amplitude, Frequency = waveList[i].frequency, Phase = waveList[i].phase });
                }

                var combined = CombineSinusodial(waveList.Select(x => x.ys));
                plots.Add(new PlotModel { Plot = SinePlot.plt.PlotSignal(combined, sampleRate, label: "ExampleSignal") });
            }

            #region SetupPlot
            _line = new MenuItem();
            _clearLines = new MenuItem();
            _openWindow = new MenuItem();
            _plotMenu = SinePlot.ContextMenu;
            _plottedLine1 = SinePlot.plt.PlotVLine(0, Color.Red, lineStyle: LineStyle.DashDotDot, lineWidth: 2.5);
            _plottedLine2 = SinePlot.plt.PlotVLine(0, Color.Red, lineStyle: LineStyle.DashDotDot, lineWidth: 2.5);
            _plottedSpan = SinePlot.plt.PlotHSpan(0, 0, Color.Gold);
            SinePlot.plt.PlotHLine(0D, lineStyle: LineStyle.DashDot);
            SinePlot.plt.PlotVLine(0D, lineStyle: LineStyle.DashDot);

            _line.Click += MenuItemSelect_Click;
            _clearLines.Click += MenuItemClearSelect_Click;
            _openWindow.Click += MenuItemOpenSelection_Click;

            SinePlot.plt.Title("Signals");
            SinePlot.plt.XLabel("Time (s)");
            SinePlot.plt.YLabel("Amplitude");
            _clearLines.Header = "Clear Selection";
            _openWindow.Header = "Open Selection in new window";

            _plotMenu.Items.Add(_line);
            _plotMenu.Items.Add(_openWindow);
            _plotMenu.Items.Add(_clearLines);

            _plottedLine1.visible = false;
            _plottedLine2.visible = false;
            _plottedSpan.visible = false;
            #endregion

            SinePlot.Render();
        }

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PlotDataGrid.ItemsSource = plots;
        }

        private void MenuItemClearSelect_Click(object sender, RoutedEventArgs e)
        {
            _plottedLine1.visible = _plottedLine2.visible = _plottedSpan.visible = false;
            SinePlot.Render();
        }

        private void MenuItemOpenSelection_Click(object sender, RoutedEventArgs e)
        {
            var maxPoint = Math.Max(_plottedLine1.position, _plottedLine2.position);
            var minPoint = Math.Min(_plottedLine1.position, _plottedLine2.position);
            ObservableCollection<PlotModel> newCollection = new ObservableCollection<PlotModel>();
            foreach (var plotModel in plots)
            {
                if (plotModel.Visible)
                {
                    var plot = plotModel.Plot;
                    var xs = DataGen.Consecutive(plot.GetPointCount(), plot.samplePeriod);
                    int indexMax = (int)(xs.First(x => x.AlmostEqual(maxPoint)) * plot.sampleRate);
                    int indexMin = (int)(xs.First(x => x.AlmostEqual(minPoint)) * plot.sampleRate);
                    var ys = plot.ys.Skip(indexMin).Take(indexMax - indexMin).ToArray();
                    if (ys.Length > 0)
                    {
                        var temp = new PlottableSignal(ys, plot.sampleRate, plot.xOffset, plot.yOffset, plot.color, plot.lineWidth, plot.markerSize, plot.label, plot.useParallel, null, ys.Length - 1);
                        newCollection.Add(new PlotModel { Plot = temp, Visible = true });
                    }
                }
            }

            MainWindow newWindow = new MainWindow(newCollection);
            newWindow.Show();
        }

        private void MenuItemSelect_Click(object sender, RoutedEventArgs e)
        {
            List<(int index, double value)> closestValues = new List<(int index, double value)>();
            foreach (var plotModel in plots)
            {
                if (plotModel.Visible)
                {
                    var tempPlt = plotModel.Plot;
                    var xs = DataGen.Consecutive(tempPlt.GetPointCount(), tempPlt.samplePeriod);
                    closestValues.Add(FindClosestXIndex(xs, mousePosition.X));
                }
            }

            if (closestValues.Any())
            {
                var xIndex = closestValues.Max(x => x.value);

                if (!_plottedLine1.visible)
                {
                    _plottedLine1.position = xIndex;
                    _plottedLine1.visible = true;
                }
                else
                {
                    _plottedLine2.position = xIndex;
                    _plottedLine2.visible = true;
                }

                if (_plottedLine1.visible && _plottedLine2.visible)
                {
                    _plottedSpan.position1 = _plottedLine1.position;
                    _plottedSpan.position2 = _plottedLine2.position;
                    _plottedSpan.visible = true;
                }

                SinePlot.Render();
            }
        }

        private void PlotMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            mousePosition = SinePlot.mouseCoordinates;

            _line.Header = _plottedLine1.visible
                ? "Select to here..."
                : "Select from here...";

            _line.Visibility = _plottedLine1.visible && _plottedLine2.visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            _clearLines.Visibility = _plottedLine1.visible || _plottedLine2.visible
                ? Visibility.Visible
                : Visibility.Collapsed;

            _openWindow.Visibility = _plottedLine1.visible && _plottedLine2.visible
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void Visible_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                selectedItem.Visible = false;
                SinePlot.plt.GetPlottables().Find(x => x.GetLegendItems()[0].label == selectedItem.Name).visible = false;
                SinePlot.Render();
            }
        }

        private void Visible_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                selectedItem.Visible = true;
                SinePlot.plt.GetPlottables().Find(x => x.GetLegendItems()[0].label == selectedItem.Name).visible = true;
                SinePlot.Render();
            }
        }
        private void DataGridMenuItemFFT_Clicked(object sender, RoutedEventArgs e)
        {
            var test = (PlotModel)PlotDataGrid.SelectedItem;
            if (test == null)
                return;

            var FFTWindow = new FFTAnalysis(test.Plot);
            FFTWindow.Show();
        }

        #endregion
    }
}

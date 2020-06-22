using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics;
using ScottPlot;
using Signals.Models;
using static Signals.Utility;
using Color = System.Drawing.Color;
using Window = System.Windows.Window;

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
        private const int sampleRate = 100;
        private Point mousePosition;
        private PlottableVLine _plottedLine1;
        private PlottableVLine _plottedLine2;
        private PlottableHSpan _plottedSpan;
        private MenuItem _line;
        private MenuItem _clearLines;
        private MenuItem _openWindow;
        private ContextMenu _plotMenu;
        private ObservableCollection<PlotModel> _plots = new ObservableCollection<PlotModel>();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitializePlot();
        }

        public MainWindow(ObservableCollection<PlotModel> plots)
        {
            InitializeComponent();
            _plots = new ObservableCollection<PlotModel>();
            foreach (var plotModel in plots)
            {
                var temp = new PlotModel { Plot = SinePlot.plt.PlotSignal(plotModel.Plot.ys, plotModel.Plot.sampleRate, label: plotModel.Name), Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase };
                _plots.Add(temp);
            }
            InitializePlot(true);
        }

        public MainWindow(PlotModel plotModel)
        {
            InitializeComponent();
            _plots = new ObservableCollection<PlotModel>();
            var temp = new PlotModel { Plot = SinePlot.plt.PlotSignal(plotModel.Plot.ys, plotModel.Plot.sampleRate, label: plotModel.Name), Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase };
            _plots.Add(temp);

            InitializePlot(true);
        }



        private void InitializePlot(bool skipDemoSineWave = false)
        {
            if (!skipDemoSineWave)
            {
                var waveList = new List<(double[] ys, int frequency, int amplitude, int phase)>();

                int waveCount = 3;

                for (int i = 0; i < waveCount; i++)
                {
                    var temp = GenerateSinusoidal(pointCount, sampleRate);
                    waveList.Add(temp);
                }

                waveList = waveList.OrderBy(x => x.frequency).ToList();

                for (int i = 0; i < waveCount; i++)
                {
                    _plots.Add(new PlotModel { Plot = SinePlot.plt.PlotSignal(waveList[i].ys, sampleRate, label: $"Harmonic{i}"), Amplitude = waveList[i].amplitude, Frequency = waveList[i].frequency, Phase = waveList[i].phase });
                }

                var combined = CombineSinusodial(waveList.Select(x => x.ys));
                _plots.Add(new PlotModel { Plot = SinePlot.plt.PlotSignal(combined, sampleRate, label: "CombinedSignal") });
            }

            #region SetupPlot
            _line = new MenuItem();
            _clearLines = new MenuItem();
            _openWindow = new MenuItem();

            //_plotMenu = SinePlot.ContextMenu;
            _plotMenu = new ContextMenu();

            _plottedLine1 = SinePlot.plt.PlotVLine(0, Color.Red, lineStyle: LineStyle.DashDotDot, lineWidth: 2.5);
            _plottedLine2 = SinePlot.plt.PlotVLine(0, Color.Red, lineStyle: LineStyle.DashDotDot, lineWidth: 2.5);
            _plottedSpan = SinePlot.plt.PlotHSpan(0, 0, Color.Gold);
            SinePlot.plt.PlotHLine(0D, lineStyle: LineStyle.DashDot);
            SinePlot.plt.PlotVLine(0D, lineStyle: LineStyle.DashDot);

            _plotMenu.Opened += PlotMenu_OnOpened;
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

            SinePlot.ContextMenu = _plotMenu;
            #endregion

            SinePlot.Render();
        }

        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PlotDataGrid.ItemsSource = _plots;
            PlotDataGrid.IsSynchronizedWithCurrentItem = true;
            PlotDataGrid.UnselectAll();
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
            foreach (var plotModel in _plots)
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
                        var temp = new PlottableSignal(ys, plot.sampleRate, plot.xOffset, plot.yOffset, plot.color, plot.lineWidth, plot.markerSize, plot.label, null, ys.Length - 1, LineStyle.Dash, useParallel: false);
                        newCollection.Add(new PlotModel { Plot = temp, Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase });
                    }
                }
            }

            MainWindow newWindow = new MainWindow(newCollection);
            newWindow.Show();
        }

        private void MenuItemSelect_Click(object sender, RoutedEventArgs e)
        {
            List<(int index, double value)> closestValues = new List<(int index, double value)>();
            foreach (var plotModel in _plots)
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
            var (x, y) = SinePlot.GetMouseCoordinates();
            mousePosition = new Point(x, y);

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

        private void DataGridMenuItemFFT_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            var FFTWindow = new FFTAnalysis(selectedItem.Plot);
            FFTWindow.Show();
        }

        private void ToggleVisibility(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                var temp = SinePlot.plt.GetPlottables().FirstOrDefault(x => x == selectedItem.Plot);
                if (temp != null)
                    temp.visible ^= true;

                SinePlot.Render();
            }
        }

        private void DataGridMenuItemOpenInNewWindow_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            var window = new MainWindow(selectedItem);
            window.Show();
        }

        #endregion
    }
}

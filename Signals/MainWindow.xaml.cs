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
using static ScottPlot.DataGen;
using Color = System.Drawing.Color;
using Window = System.Windows.Window;

namespace Signals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private values

        private bool valueChanging = false;
        private const int pointCount = 100;
        private PlottableScatter _plt;
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
                SinePlot.plt.PlotScatter(plot.xs, plot.ys, plot.color, plot.lineWidth, plot.markerSize, plot.label,
                    plot.errorX, plot.errorY, plot.errorLineWidth, plot.errorCapSize, plot.markerShape, plot.lineStyle);
            }
            InitializePlot(true);
        }

        private void InitializePlot(bool skipDemoSineWave = false)
        {
            double[] sineWave = Array.Empty<double>();
            double[] consecutive = Array.Empty<double>();
            if (!skipDemoSineWave)
            {
                sineWave = Sin(pointCount);
                consecutive = Consecutive(pointCount);
                var temp = GenerateRandomScatterPoints().ToList();
                //for (int i = 0; i < temp.Count; i++)
                //{
                //    plots.Add(new PlotModel { Plot = SinePlot.plt.PlotScatter(temp[i].xs, temp[i].ys, markerShape: MarkerShape.filledCircle, label: $"Wave{i + 1}"), Visible = true });
                //}
            }
            else
            {
                sineWave = Sin(pointCount);
                consecutive = Consecutive(pointCount);
            }
            _plt = SinePlot.plt.PlotScatter(consecutive, sineWave, markerShape: MarkerShape.filledCircle, label: "DemoPlot");
            plots.Add(new PlotModel { Plot = _plt, Visible = _plt.visible });

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

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!valueChanging)
            {
                valueChanging = true;
                _plt.ys = Generate.Sinusoidal(pointCount, pointCount, OscSlider.Value, MulSlider.Value, phase: PhaseSlider.Value, mean: OffSlider.Value);
                //_plt.ys = Sin(_plt.ys.Length, OscSlider.Value, OffSlider.Value, MulSlider.Value,
                //    PhaseSlider.Value);
                _plottedLine1.visible = _plottedLine2.visible = _plottedSpan.visible = false;
                SinePlot.Render();
                valueChanging = false;
            }
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PlotDataGrid.ItemsSource = plots;
            OscSlider.ValueChanged += Slider_OnValueChanged;
            OffSlider.ValueChanged += Slider_OnValueChanged;
            MulSlider.ValueChanged += Slider_OnValueChanged;
            PhaseSlider.ValueChanged += Slider_OnValueChanged;
        }

        private void MenuItemClearSelect_Click(object sender, RoutedEventArgs e)
        {
            _plottedLine1.visible = _plottedLine2.visible = _plottedSpan.visible = false;
            SinePlot.Render();
        }

        private void MenuItemOpenSelection_Click(object sender, RoutedEventArgs e)
        {
            int maxPoint = (int)Math.Max(_plottedLine1.position, _plottedLine2.position);
            int minPoint = (int)Math.Min(_plottedLine1.position, _plottedLine2.position);

            ObservableCollection<PlotModel> newCollection = new ObservableCollection<PlotModel>();
            foreach (var plotModel in plots)
            {
                if (plotModel.Visible)
                {
                    var ys = plotModel.Plot.ys.Skip(minPoint).Take(maxPoint - minPoint).ToArray();
                    var xs = plotModel.Plot.xs.Skip(minPoint).Take(maxPoint - minPoint).ToArray();
                    if (ys.Length > 0 && xs.Length > 0)
                    {
                        var plot = plotModel.Plot;
                        var temp = new PlottableScatter(xs, ys, plot.color, plot.lineWidth, plot.markerSize, plot.label,
                            plot.errorX, plot.errorY, plot.errorLineWidth, plot.errorCapSize, plot.stepDisplay,
                            plot.markerShape, plot.lineStyle);
                        newCollection.Add(new PlotModel {Plot = temp, Visible = true});
                    }
                }
            }

            MainWindow newWindow = new MainWindow(newCollection);
            newWindow.Show();
        }

        private void MenuItemSelect_Click(object sender, RoutedEventArgs e)
        {
            var mouseLoc = SinePlot.mouseCoordinates;

            List<(int index, double value)> closestValues = new List<(int index, double value)>();
            foreach (var plotModel in plots)
            {
                if (plotModel.Visible)
                    closestValues.Add(FindClosestXIndex(plotModel.Plot.xs, mouseLoc.X));
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
                SinePlot.plt.GetPlottables().Find(x => x.label == selectedItem.Name).visible = false;
                SinePlot.Render();
            }
        }

        private void Visible_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem != null)
            {
                selectedItem.Visible = true;
                SinePlot.plt.GetPlottables().Find(x => x.label == selectedItem.Name).visible = true;
                SinePlot.Render();
            }
        }

        #region Utility
        private IEnumerable<(double[] xs, double[] ys)> GenerateRandomScatterPoints()
        {
            Random rand = new Random();
            List<(double[] xs, double[] ys)> list = new List<(double[] xs, double[] ys)>();
            for (int i = 0; i < 3; i++)
            {
                int randomCount = rand.Next(100, 1000);
                var xs = Consecutive(randomCount);
                var ys = Sin(randomCount, rand.Next(1, 3));

                list.Add((xs, ys));
            }

            return list;
        }
        private (int index, double value) FindClosestXIndex(double[] arr, double target)
        {
            int n = arr.Length;

            if (target <= arr[0])
                return (0, arr[0]);
            if (target >= arr[n - 1])
                return (n - 1, arr[n - 1]);

            int i = 0, j = n, mid = 0;
            while (i < j)
            {
                mid = (i + j) / 2;

                if (Math.Abs(arr[mid] - target) < 0.000000001)
                    return (mid, arr[mid]);

                double temp;
                if (target < arr[mid])
                {
                    if (mid > 0 && target > arr[mid - 1])
                    {
                        temp = GetClosest(arr[mid - 1], arr[mid], target);
                        if (Math.Abs(temp - arr[mid - 1]) < 0.000000001)
                            return (mid - 1, arr[mid - 1]);
                        else
                            return (mid, arr[mid]);
                    }

                    j = mid;
                }

                else
                {
                    if (mid < n - 1 && target < arr[mid + 1])
                    {
                        temp = GetClosest(arr[mid], arr[mid + 1], target);
                        if (Math.Abs(temp - arr[mid]) < 0.000000001)
                            return (mid, arr[mid]);
                        else
                            return (mid + 1, arr[mid + 1]);
                    }
                    i = mid + 1;
                }
            }

            return (mid, arr[mid]);
        }

        private double GetClosest(double val1, double val2, double target)
        {
            if (target - val1 >= val2 - target)
                return val2;

            return val1;
        }


        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics;
using Microsoft.Win32;
using Newtonsoft.Json;
using ScottPlot;
using Signals.Models;
using static Signals.Utility;
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
        private const int pointCount = 1000;
        private const int sampleRate = 1000;
        private Point mousePosition;
        private PlottableVLine _plottedLine1;
        private PlottableVLine _plottedLine2;
        private PlottableHSpan _plottedSpan;
        private MenuItem _line;
        private MenuItem _clearLines;
        private MenuItem _openWindow;
        private MenuItem _saveImage;
        private ContextMenu _plotMenu;
        private readonly ObservableCollection<PlotModel> _plots = new ObservableCollection<PlotModel>();

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
                var temp = new PlotModel { ParentPlot = SinePlot, Plot = SinePlot.plt.PlotSignal(plotModel.Plot.ys, plotModel.Plot.sampleRate, label: plotModel.Name), Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase };
                _plots.Add(temp);
            }
            InitializePlot(true);
        }

        public MainWindow(PlotModel plotModel)
        {
            InitializeComponent();
            _plots = new ObservableCollection<PlotModel>();
            var temp = new PlotModel { ParentPlot = SinePlot, Plot = SinePlot.plt.PlotSignal(plotModel.Plot.ys, plotModel.Plot.sampleRate, label: plotModel.Name), Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase };
            _plots.Add(temp);

            InitializePlot(true);
        }

        private void InitializePlot(bool skipDemoSineWave = false)
        {
            #region Demo Wave
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
                    _plots.Add(new PlotModel { ParentPlot = SinePlot, Plot = SinePlot.plt.PlotSignal(waveList[i].ys, sampleRate, label: $"Harmonic{i}"), Amplitude = waveList[i].amplitude, Frequency = waveList[i].frequency, Phase = waveList[i].phase });
                }

                var combined = CombineSinusoidal(waveList.Select(x => x.ys));
                _plots.Add(new PlotModel { ParentPlot = SinePlot, Plot = SinePlot.plt.PlotSignal(combined, sampleRate, label: "CombinedSignal") });
            }

            #endregion

            #region SetupPlot
            _line = new MenuItem();
            _clearLines = new MenuItem();
            _openWindow = new MenuItem();
            _saveImage = new MenuItem();

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
            _saveImage.Click += SavePlotAsImage;

            SinePlot.plt.Title("Signals");
            SinePlot.plt.XLabel("Time (s)");
            SinePlot.plt.YLabel("Amplitude");
            _clearLines.Header = "Clear Selection";
            _openWindow.Header = "Open Selection in new window";
            _saveImage.Header = "Save Image";

            _plotMenu.Items.Add(_line);
            _plotMenu.Items.Add(_openWindow);
            _plotMenu.Items.Add(_clearLines);
            _plotMenu.Items.Add(new Separator());
            _plotMenu.Items.Add(_saveImage);

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

        private void SavePlotAsImage(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog
            {
                FileName = "WavePlot.png",
                Filter = "PNG Files (*.png)|*.png;*.png" +
                         "|JPG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                         "|BMP Files (*.bmp)|*.bmp;*.bmp" +
                         "|TIF files (*.tif, *.tiff)|*.tif;*.tiff" +
                         "|All files (*.*)|*.*"
            };
            if (savefile.ShowDialog() == true)
                SinePlot.plt.SaveFig(savefile.FileName);
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
                    int indexMax = (int)(xs.FirstOrDefault(x => x.AlmostEqual(maxPoint)) * plot.sampleRate);
                    if (indexMax == 0)
                        indexMax = xs.Length;
                    indexMax += 1; //offset index by 1

                    int indexMin = (int)(xs.FirstOrDefault(x => x.AlmostEqual(minPoint)) * plot.sampleRate);

                    indexMax = Math.Max(indexMin, indexMax);
                    indexMin = Math.Min(indexMin, indexMax);

                    var ys = plot.ys.Skip(indexMin).Take(indexMax - indexMin).ToArray();
                    if (ys.Length > 0)
                    {
                        var temp = new PlottableSignal(ys, plot.sampleRate, plot.xOffset, plot.yOffset, plot.color, plot.lineWidth, plot.markerSize, plot.label, null, ys.Length - 1, LineStyle.Dash, useParallel: false);
                        newCollection.Add(new PlotModel { ParentPlot = SinePlot, Plot = temp, Visible = true, Amplitude = plotModel.Amplitude, Frequency = plotModel.Frequency, Phase = plotModel.Phase });
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

            var FFTWindow = new FFTAnalysisWindow(selectedItem);
            FFTWindow.Show();
        }


        private void DataGridMenuItemOpenInNewWindow_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            var window = new MainWindow(selectedItem);
            window.Show();
        }

        private void DataGridMenuItemRemove_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            _plots.Remove(selectedItem);
            SinePlot.plt.Clear(plotToRemove => plotToRemove == selectedItem.Plot);

            SinePlot.Render();
        }

        private void DataGridMenuItemViewValues_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            var dataViewWindow = new DataViewWindow(selectedItem);

            dataViewWindow.Show();
        }

        private void AddCustomWave(object sender, RoutedEventArgs e)
        {
            var addCustomWaveWindow = new AddWaveWindow();

            if (addCustomWaveWindow.ShowDialog() == true)
            {
                var resultPlot = addCustomWaveWindow.ResultPlot;
                resultPlot.ParentPlot = SinePlot;
                _plots.Add(resultPlot);
                SinePlot.plt.Add(resultPlot.Plot);
            }

            SinePlot.Render();
        }

        private void DataGridMenuItemExportJSON_Clicked(object sender, RoutedEventArgs e)
        {
            var selectedItem = (PlotModel)PlotDataGrid.SelectedItem;
            if (selectedItem == null)
                return;

            SaveFileDialog savefile = new SaveFileDialog
            {
                FileName = "WavePlot.json",
                Filter = "JSON file (*.json)|*.json" +
                         "| All Files (*.*)|*.*"

            };

            if (savefile.ShowDialog() == true)
            {
                JSONPlotModel jsonModel = new JSONPlotModel
                {
                    Amplitude = selectedItem.Amplitude,
                    Data = selectedItem.Plot.ys,
                    Frequency = selectedItem.Frequency,
                    Name = selectedItem.Name,
                    Phase = selectedItem.Phase,
                    PointCount = selectedItem.PointCount,
                    SamplePeriod = selectedItem.SamplePeriod,
                    SampleRate = selectedItem.SampleRate
                };

                using StreamWriter file = File.CreateText(savefile.FileName);
                new JsonSerializer().Serialize(file, jsonModel);
            }

        }
        private void ReadFromJsonFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON file (*.json)|*.json" +
                         "| All Files (*.*)|*.*"
            };

            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    using StreamReader file = File.OpenText(openFileDialog.FileName);

                    JSONPlotModel deserializedPlot = (JSONPlotModel)new JsonSerializer().Deserialize(file, typeof(JSONPlotModel));

                    if (string.IsNullOrWhiteSpace(deserializedPlot.Name))
                        deserializedPlot.Name = openFileDialog.SafeFileName.Substring(0, openFileDialog.SafeFileName.IndexOf('.'));


                    if (deserializedPlot.Data == null)
                        throw new ArgumentException();

                    if (deserializedPlot.PointCount == 0)
                        deserializedPlot.PointCount = deserializedPlot.Data.Length;

                    if (deserializedPlot.SampleRate == 0)
                        deserializedPlot.SampleRate = deserializedPlot.Data.Length;

                    var plot = SinePlot.plt.PlotSignal(deserializedPlot.Data, deserializedPlot.SampleRate, label: deserializedPlot.Name);

                    _plots.Add(new PlotModel
                    {
                        ParentPlot = SinePlot,
                        Amplitude = deserializedPlot.Amplitude,
                        Frequency = deserializedPlot.Frequency,
                        Phase = deserializedPlot.Phase,
                        Plot = plot
                    });

                    SinePlot.Render();
                }
            }
            catch
            {
                MessageBox.Show("There was a problem reading the json file", "Error Reading From FIle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void ExitProgram(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}

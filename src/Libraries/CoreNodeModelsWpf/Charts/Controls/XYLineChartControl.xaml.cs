using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Linq;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for XYLineChartControl.xaml
    /// </summary>
    public partial class XYLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();
        private readonly XYLineChartNodeModel model;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public XYLineChartControl(XYLineChartNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(XYLineChartNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var seriesRange = DefaultSeries();

                XYLineChart.Series.AddRange(seriesRange);
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries();

                    XYLineChart.Series.AddRange(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as XYLineChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    XYLineChart.Series.Clear();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        var seriesRange = DefaultSeries();
                        XYLineChart.Series.AddRange(seriesRange);
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);
                        XYLineChart.Series.AddRange(seriesRange);

                    }
                });
            }
        }

        private LineSeries[] DefaultSeries()
        {
            var defaultXValues = new double[][]
                {
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 0, 1, 2, 3 }
                };

            var defaultYValues = new double[][]
            {
                    new double[]{ 0, 1, 2, 3 },
                    new double[]{ 1, 2, 3, 4 },
                    new double[]{ 2, 3, 4, 5 }
            };
            List<string> labels = new List<string> { "Plot 1", "Plot 2", "Plot 3" };
            LineSeries[] seriesRange = new LineSeries[defaultXValues.Length];

            for (var i = 0; i < defaultXValues.Length; i++)
            {
                ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                for (int j = 0; j < defaultXValues[i].Length; j++)
                {
                    points.Add(new ObservablePoint
                    {
                        X = defaultXValues[i][j],
                        Y = defaultYValues[i][j]
                    });
                }

                seriesRange[i] = new LineSeries
                {
                    Title = labels[i],
                    Values = points,
                    Fill = Brushes.Transparent
                };
            }

            return seriesRange;
        }

        private List<LineSeries> UpdateSeries(XYLineChartNodeModel model = null)
        {
            var seriesRange = new List<LineSeries>();
            if(model == null)
            {
                model = this.model;
            }
            if(model.Labels != null && model.Labels.Any()
             && model.XValues != null && model.XValues.Any()
             && model.YValues != null && model.YValues.Any()
             && model.Colors != null && model.Colors.Any())
            {
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                    for (int j = 0; j < model.XValues[i].Count; j++)
                    {
                        points.Add(new ObservablePoint
                        {
                            X = model.XValues[i][j],
                            Y = model.YValues[i][j]
                        });
                    }

                    seriesRange.Add(new LineSeries
                    {
                        Title = model.Labels[i],
                        Values = points,
                        Stroke = model.Colors[i],
                        StrokeThickness = 2.0,
                        Fill = Brushes.Transparent
                    });
                }
            }

            return seriesRange;
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= inputGrid.MinWidth && xAdjust >= MIN_WIDTH)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight && xAdjust >= MIN_HEIGHT)
                {
                    Height = yAdjust;
                }
            }
        }

        /// <summary>
        /// Unsubscribes from ViewModel events
        /// </summary>
        private void Unload(object sender, RoutedEventArgs e)
        {
            this.model.PropertyChanged -= NodeModel_PropertyChanged;
            Unloaded -= Unload;
        }
    }
}

using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for XYLineChartControl.xaml
    /// </summary>
    public partial class XYLineChartControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public XYLineChartControl(XYLineChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(XYLineChartNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
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
                        Values = points,
                        Fill = Brushes.Transparent
                    };
                }

                XYLineChart.Series.AddRange(seriesRange);
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected && model.InPorts[3].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    LineSeries[] seriesRange = new LineSeries[model.Labels.Count];

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

                        seriesRange[i] = new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = points,
                            Stroke = model.Colors[i],
                            StrokeThickness = 2.0,
                            Fill = Brushes.Transparent,
                        };
                    }

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
                    LineSeries[] seriesRange = new LineSeries[model.Labels.Count];

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

                        seriesRange[i] = new LineSeries
                        {
                            Title = model.Labels[i],
                            Values = points,
                            Stroke = model.Colors[i],
                            StrokeThickness = 2.0,
                            Fill = Brushes.Transparent
                            //PointGeometrySize = 0
                        };
                    }

                    XYLineChart.Series.Clear();
                    XYLineChart.Series.AddRange(seriesRange);
                });
            }
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= inputGrid.MinWidth)
                {
                    Width = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight)
                {
                    Height = yAdjust;
                }
            }
        }
    }
}

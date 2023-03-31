using System;
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
    public partial class HeatSeriesControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();
        private readonly HeatSeriesNodeModel model;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public HeatSeriesControl(HeatSeriesNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(HeatSeriesNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var chartValues = DefaultValues();

                HeatSeriesUI.Series.Add(new HeatSeries()
                {
                    Values = chartValues,
                    DrawsHeatRange = false,
                });
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected && model.InPorts[3].IsConnected)
            {
                if (model.XLabels.Count == model.Values.Count && model.XLabels.Count > 0)
                {
                    UpdateValues(model);
                }
            }
        }
        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var model = sender as HeatSeriesNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        var chartValues = DefaultValues();

                        HeatSeriesUI.Series.Add(new HeatSeries()
                        {
                            Values = chartValues,
                            DrawsHeatRange = false,
                        });
                    }
                    else
                    {
                        UpdateValues(model);
                    }
                });
            }
        }

        private ChartValues<HeatPoint> DefaultValues()
        {
            // X - Products
            var XLabels = new[]
            {
                    "Item-1",
                    "Item-2",
                    "Item-3",
                    "Item-4",
                    "Item-5"
                };

            // Y - Day of the week
            var YLabels = new[]
            {
                    "Monday",
                    "Tuesday",
                    "Wednesday",
                    "Thursday",
                    "Friday",
                    "Saturday",
                    "Sunday"
                };

            // Value for each product on every day of the week
            var chartValues = new ChartValues<HeatPoint>();

            for (var i = 0; i < XLabels.Length; i++)
            {
                for (var j = 0; j < YLabels.Length; j++)
                {
                    chartValues.Add(new HeatPoint(i, j, rnd.Next(0, 10)));
                }
            }

            XAxis.Labels = XLabels;
            YAxis.Labels = YLabels;

            return chartValues;
        }

        private void UpdateValues(HeatSeriesNodeModel model)
        {
            var chartValues = new ChartValues<HeatPoint>();
            HeatSeriesUI.Series.Clear();

            if (model == null)
            {
                model = this.model;
            }

            var colors = new GradientStopCollection();

            if (model.XLabels != null && model.XLabels.Any()
             && model.YLabels != null && model.YLabels.Any()
             && model.Values != null && model.Values.Any()
             && model.Colors != null && model.Colors.Any())
            {
                for (var i = 0; i < model.XLabels.Count; i++)
                {
                    for (var j = 0; j < model.YLabels.Count; j++)
                    {
                        chartValues.Add(new HeatPoint(i, j, model.Values[i][j]));
                    }
                }

                colors = BuildColors(model);

                XAxis.Labels = model.XLabels;
                YAxis.Labels = model.YLabels;
            }

            HeatSeriesUI.Series.Add(new HeatSeries()
            {
                Values = chartValues,
                DrawsHeatRange = false,
                GradientStopCollection = colors,
                PointGeometry = DefaultGeometries.Square
            });
        }

        private GradientStopCollection BuildColors(HeatSeriesNodeModel model)
        {
            var colors = new GradientStopCollection();

            // If provided with a single color create range from transparent white to color
            if (model.Colors.Count == 1)
            {
                colors.Add(new GradientStop()
                {
                    Offset = 0,
                    Color = Color.FromArgb(255, 255, 255, 255)
                });

                colors.Add(new GradientStop()
                {
                    Offset = 1,
                    Color = model.Colors[0]
                });
            }

            // If provided with several colors create a range for provided colors
            else if (model.Colors.Count > 1)
            {
                var count = model.Colors.Count;

                for (var i = 0; i < count; i++)
                {
                    colors.Add(new GradientStop()
                    {
                        Offset = i / (count - 1),
                        Color = model.Colors[i]
                    });
                }
            }

            return colors;
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

                if (yAdjust >= inputGrid.MinHeight && yAdjust >= MIN_HEIGHT)
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

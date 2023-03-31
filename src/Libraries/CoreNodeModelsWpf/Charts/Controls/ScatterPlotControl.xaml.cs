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
    /// Interaction logic for ScatterPlotControl.xaml
    /// </summary>
    public partial class ScatterPlotControl : UserControl, INotifyPropertyChanged
    {
        private Random rnd = new Random();
        private readonly ScatterPlotNodeModel model;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ScatterPlotControl(ScatterPlotNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(ScatterPlotNodeModel model)
        {
            // Load sample data if any ports are not connected
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
            {
                var plots = DefaultSeries();
                ScatterPlot.Series.AddRange(plots);
            }
            // Else load input data
            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.XValues.Count && model.XValues.Count == model.YValues.Count && model.Labels.Count > 0)
                {
                    var plots = UpdateSeries();                    
                    ScatterPlot.Series.AddRange(plots);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var model = sender as ScatterPlotNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    ScatterPlot.Series.Clear();

                    // Load sample data if any ports are not connected
                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected && !model.InPorts[3].IsConnected)
                    {
                        var plots = DefaultSeries();
                        ScatterPlot.Series.AddRange(plots);
                    }
                    else
                    {
                        var plots = UpdateSeries(model);
                        ScatterPlot.Series.AddRange(plots);
                    }
                });
            }
        }

        private ScatterSeries [] DefaultSeries()
        {
            var ValuesA = new ChartValues<ObservablePoint>();
            var ValuesB = new ChartValues<ObservablePoint>();
            var ValuesC = new ChartValues<ObservablePoint>();

            for (var i = 0; i < 20; i++)
            {
                ValuesA.Add(new ObservablePoint(rnd.NextDouble() * 10, rnd.NextDouble() * 10));
                ValuesB.Add(new ObservablePoint(rnd.NextDouble() * 10, rnd.NextDouble() * 10));
                ValuesC.Add(new ObservablePoint(rnd.NextDouble() * 10, rnd.NextDouble() * 10));
            }

            var plot1 = new ScatterSeries { Title = "Plot 1", Values = ValuesA };
            var plot2 = new ScatterSeries { Title = "Plot 2", Values = ValuesB };
            var plot3 = new ScatterSeries { Title = "Plot 3", Values = ValuesC };

            var plots = new ScatterSeries[] { plot1, plot2, plot3 };

            return plots;
        }

        private List<ScatterSeries> UpdateSeries(ScatterPlotNodeModel model = null)
        {
            var plots = new List<ScatterSeries>();

            if(model == null)
            {
                model = this.model;
            }

            if (model.Labels != null && model.Labels.Any()
             && model.XValues != null && model.XValues.Any()
             && model.YValues != null && model.YValues.Any()
             && model.Colors != null && model.Colors.Any())
            {
                // For each set of points
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();

                    // For each x-value list
                    for (int j = 0; j < model.XValues[i].Count; j++)
                    {
                        points.Add(new ObservablePoint
                        {
                            X = model.XValues[i][j],
                            Y = model.YValues[i][j]
                        });
                    }

                    plots.Add(new ScatterSeries
                    {
                        Title = model.Labels[i],
                        Values = points,
                        Fill = model.Colors[i]
                    });
                }
            }

            return plots;
        }

        private void ThumbResizeThumbOnDragDeltaHandler(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = ActualHeight + e.VerticalChange;
            var xAdjust = ActualWidth + e.HorizontalChange;

            if (this.Parent.GetType() == typeof(Grid))
            {
                var inputGrid = this.Parent as Grid;

                if (xAdjust >= MIN_WIDTH/*inputGrid.MinWidth*/ )
                {
                    Width = xAdjust;
                }

                if (yAdjust >= MIN_HEIGHT/*inputGrid.MinHeight*/)
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

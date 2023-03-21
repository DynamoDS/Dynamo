using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Web.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace CoreNodeModelsWpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl, INotifyPropertyChanged
    {
        //private Func<ChartPoint, string> PointLabel { get; set; }
        private Random rnd = new Random();
        private readonly PieChartNodeModel model;

        private double MIN_WIDTH = 300;
        private double MIN_HEIGHT = 300;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(PieChartNodeModel model)
        {
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                var seriesRange = DefaultSeries();

                PieChart.Series.AddRange(seriesRange);
            }

            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = UpdateSeries(model);

                    PieChart.Series.AddRange(seriesRange);
                }
            }
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as PieChartNodeModel;

                // Invoke on UI thread
                this.Dispatcher.Invoke(() =>
                {
                    PieChart.Series.Clear();

                    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                    {
                        var seriesRange = DefaultSeries();

                        PieChart.Series.AddRange(seriesRange);
                    }
                    else
                    {
                        var seriesRange = UpdateSeries(model);

                        PieChart.Series.AddRange(seriesRange);
                    }
                });
            }
        }

        private PieSeries[] DefaultSeries()
        {
            var series = new PieSeries[]
            {
                new PieSeries { Title = "Item1", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ },
                new PieSeries { Title = "Item2", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ },
                new PieSeries { Title = "Item3", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ },
            };

            return series;
        }

        private List<PieSeries> UpdateSeries(PieChartNodeModel model)
        {
            var seriesRange = new List<PieSeries>();

            if (model == null)
            {
                model = this.model;
            }

            if (model.Labels != null && model.Labels.Any()
             && model.Values != null && model.Values.Any()
             && model.Colors != null && model.Colors.Any())
            {
                for (var i = 0; i < model.Labels.Count; i++)
                {
                    seriesRange.Add(new PieSeries
                    {
                        Title = model.Labels[i],
                        Values = new ChartValues<double> { model.Values[i] },
                        Fill = model.Colors[i],
                        DataLabels = true,
                        //LabelPoint = PointLabel
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
                    Height = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight && xAdjust >= MIN_HEIGHT)
                {
                    Width = yAdjust;
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

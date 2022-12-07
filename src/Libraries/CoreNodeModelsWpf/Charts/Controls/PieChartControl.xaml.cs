using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LiveCharts;
using LiveCharts.Wpf;
using System.ComponentModel;

namespace CoreNodeModels.Charts.Controls
{
    /// <summary>
    /// Interaction logic for PieChartControl.xaml
    /// </summary>
    public partial class PieChartControl : UserControl, INotifyPropertyChanged
    {
        //private Func<ChartPoint, string> PointLabel { get; set; }
        private Random rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PieChartControl(PieChartNodeModel model)
        {
            InitializeComponent();

            model.PropertyChanged += NodeModel_PropertyChanged;

            //PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            BuildUI(model);

            DataContext = this;
        }

        private void BuildUI(PieChartNodeModel model)
        {
            if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
            {
                var seriesRange = new PieSeries[]
                {
                    new PieSeries { Title = "Item1", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ },
                    new PieSeries { Title = "Item2", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ },
                    new PieSeries { Title = "Item3", Values = new ChartValues<double> { 100.0 }, DataLabels = true/*, LabelPoint = PointLabel*/ }
                };

                PieChart.Series.AddRange(seriesRange);
            }

            else if (model.InPorts[0].IsConnected && model.InPorts[1].IsConnected && model.InPorts[2].IsConnected)
            {
                if (model.Labels.Count == model.Values.Count && model.Labels.Count > 0)
                {
                    var seriesRange = new PieSeries[model.Labels.Count];

                    for (var i = 0; i < model.Labels.Count; i++)
                    {
                        seriesRange[i] = new PieSeries
                        {
                            Title = model.Labels[i],
                            Values = new ChartValues<double> { model.Values[i] },
                            Fill = model.Colors[i],
                            DataLabels = true,
                            //LabelPoint = PointLabel
                        };
                    }

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
                    var seriesRange = new PieSeries[nodeModel.Labels.Count];

                    for (var i = 0; i < nodeModel.Labels.Count; i++)
                    {
                        seriesRange[i] = new PieSeries
                        {
                            Title = nodeModel.Labels[i],
                            Fill = nodeModel.Colors[i],
                            //StrokeThickness = 0,
                            Values = new ChartValues<double> { nodeModel.Values[i] },
                            DataLabels = true,
                            //LabelPoint = PointLabel
                        };
                    }

                    PieChart.Series.Clear();
                    PieChart.Series.AddRange(seriesRange);
                });
            }
        }

        /*
        private void Chart_OnDataClick(object sender, ChartPoint chartpoint)
        {
            var chart = (PieChart)chartpoint.ChartView;

            // Clear selected slice
            foreach (PieSeries series in chart.Series)
            {
                series.PushOut = 0;
            }

            var selectedSeries = (PieSeries)chartpoint.SeriesView;
            selectedSeries.PushOut = 8;
        }
        */

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
                    Height = xAdjust;
                }

                if (yAdjust >= inputGrid.MinHeight)
                {
                    Width = yAdjust;
                    Height = yAdjust;
                }
            }
        }
    }
}

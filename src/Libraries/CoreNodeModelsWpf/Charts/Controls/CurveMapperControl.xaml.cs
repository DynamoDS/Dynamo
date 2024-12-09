using CoreNodeModelsWpf.Charts;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dynamo.Wpf.Charts.Controls
{
    /// <summary>
    /// Interaction logic for CurveMapperControl.xaml
    /// </summary>
    public partial class CurveMapperControl : UserControl, INotifyPropertyChanged
    {
        private readonly CurveMapperNodeModel model;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CurveMapperControl(CurveMapperNodeModel model)
        {
            InitializeComponent();

            this.model = model;
            // ip comment : build this
            //this.model.PropertyChanged += NodeModel_PropertyChanged;
            this.Unloaded += Unload;


            // ip comment : build this
            //BuildUI(model);

            DataContext = this;
        }

        private void NodeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DataUpdated")
            {
                var nodeModel = sender as CurveMapperNodeModel;

                // ip comment : build this ? do we need SkiaSharpView/libSkiaSharp ?
                //// Invoke on UI thread
                //this.Dispatcher.Invoke(() =>
                //{
                //    PieChart.Series = Enumerable.Empty<ISeries>();

                //    if (!model.InPorts[0].IsConnected && !model.InPorts[1].IsConnected && !model.InPorts[2].IsConnected)
                //    {
                //        var seriesRange = DefaultSeries();

                //        PieChart.Series = PieChart.Series.Concat(seriesRange);
                //    }
                //    else
                //    {
                //        var seriesRange = UpdateSeries(model);

                //        PieChart.Series = PieChart.Series.Concat(seriesRange);
                //    }
                //});
            }
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
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

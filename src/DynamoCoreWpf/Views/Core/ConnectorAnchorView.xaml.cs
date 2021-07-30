using Dynamo.ViewModels;
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
using System.Windows.Threading;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for ConnectorAnchorView.xaml
    /// </summary>
    public partial class ConnectorAnchorView : UserControl
    {

        private DispatcherTimer timerOn;

        /// <summary>
        /// Access to the ConnectorAnchorView model which parents this view
        /// and which needs to be notified when the mouse leaves this views 
        /// as it then proceeds to dispose of itself.
        /// </summary>
        public ConnectorAnchorViewModel ViewModel { get; set; }

        public ConnectorAnchorView()
        {
            InitializeComponent();
            this.Loaded += InitializeCommands;
        }

        private void InitializeCommands(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as ConnectorAnchorViewModel;

            if (timerOn != null)
                ForceTimerOff(timerOn);

            StartTimer(timerOn, new TimeSpan(0, 0, 0, 0, 500));
        }

        private void StartTimer(DispatcherTimer timer, TimeSpan timeSpan)
        {
            timer = new DispatcherTimer();
            timer.Interval = timeSpan;
            timer.Start();

                timer.Tick += TimerDoneShow;
        }

        private void TimerDoneShow(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer is null) { return; }
            timer.Tick -= TimerDoneShow;

            FlipOn();
            ForceTimerOff(timer);
        }

        private void ForceTimerOff(DispatcherTimer timer)
        {
            timer.Stop();
            timer = null;
        }
        private void FlipOn()
        {
            ViewModel.CanDisplayIcons = true;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.CanDisplayIcons = false;
            ViewModel.DisposeViewModel();
        }
    }
}

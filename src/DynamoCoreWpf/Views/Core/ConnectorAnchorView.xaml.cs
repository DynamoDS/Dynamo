using Dynamo.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Dynamo.Controls
{
    /// <summary>
    /// Interaction logic for ConnectorAnchorView. Basically this class 
    /// controls some of the interaction logic for the icons that 
    /// appear above a connector when one hovers over it. Specifically the 
    /// 'watch' and the 'pin' icons. Each of them triggers the action 
    /// of placing said element.
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
        /// <summary>
        /// Initializes datacontext and timer after the appearance of the
        /// 'anchor' (plus sign) icon, and the subsequent appearance
        /// of both the 'watch' and 'pin' icons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitializeCommands(object sender, RoutedEventArgs e)
        {
            ViewModel = this.DataContext as ConnectorAnchorViewModel;

            if (timerOn != null)
                ForceTimerOff(timerOn);

            StartTimer(timerOn, new TimeSpan(0, 0, 0, 0, 500));
        }

        /// <summary>
        /// Starts timer.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="timeSpan"></param>
        private void StartTimer(DispatcherTimer timer, TimeSpan timeSpan)
        {
            timer = new DispatcherTimer();
            timer.Interval = timeSpan;
            timer.Start();
            timer.Tick += TimerDoneShow;
        }

        /// <summary>
        /// Handles the termination of the timer.
        /// In this case it turns a flag on which
        /// indicates the xaml that the other icons should appear.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerDoneShow(object sender, EventArgs e)
        {
            var timer = sender as DispatcherTimer;
            if (timer is null) { return; }
            timer.Tick -= TimerDoneShow;

            FlipOn();
            ForceTimerOff(timer);
        }

        /// <summary>
        /// Stops timer.
        /// </summary>
        /// <param name="timer"></param>
        private void ForceTimerOff(DispatcherTimer timer)
        {
            timer.Stop();
            timer = null;
        }
        /// <summary>
        /// Simple function to turn flag on.
        /// </summary>
        private void FlipOn()
        {
            ViewModel.CanDisplayIcons = true;
        }

        /// <summary>
        /// Turns flag indicating that icons should be displayed to 'false'.
        /// It also raises an event telling the viewmodel which hosts this 
        /// view to dispose of this view and itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.CanDisplayIcons = false;
            ViewModel.RequestDisposeViewModel();
        }

        /// <summary>
        /// Swaps icon for a 'preview' version of the icon on hover.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWatchIconHover(object sender, MouseEventArgs e)
        {
            ViewModel.SwitchWatchPreviewOn();
        }
        /// <summary>
        /// Swaps icon for a rest-state version of the icon on uhover.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWatchIconUnhover(object sender, MouseEventArgs e)
        {
            ViewModel.SwitchWatchPreviewOff();
        }
        /// <summary>
        /// Swaps icon for a 'preview' version of the icon on hover.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPinIconHover(object sender, MouseEventArgs e)
        {
            ViewModel.SwitchPinPreviewOn();
        }
        /// <summary>
        /// Swaps icon for a rest-state version of the icon on uhover.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPinIconUnhover(object sender, MouseEventArgs e)
        {
            ViewModel.SwitchPinPreviewOff();
        }
    }
}

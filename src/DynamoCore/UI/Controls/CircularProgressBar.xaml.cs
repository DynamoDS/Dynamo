//http://www.codeproject.com/Articles/49853/Better-WPF-Circular-Progress-Bar
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Dynamo.UI.Controls
{
    public partial class CircularProgressBar
    {
        #region Properties
        private readonly DispatcherTimer dispatcherTimer;       
        #endregion

        #region Constructor
        public CircularProgressBar()
        {
            InitializeComponent();

            dispatcherTimer = new DispatcherTimer(
                DispatcherPriority.Background, Dispatcher);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 75);
        }
        #endregion

        #region Private Methods
        private void Start()
        {                      
            dispatcherTimer.Tick += OnDispatcherTimerTick;
            dispatcherTimer.Start();
        }

        private void Stop()
        {
            dispatcherTimer.Stop();
            Mouse.OverrideCursor = Cursors.Arrow;
            dispatcherTimer.Tick -= OnDispatcherTimerTick;
        }

        private void OnDispatcherTimerTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            SetPosition(C0, offset, 0.0, step);
            SetPosition(C1, offset, 1.0, step);
            SetPosition(C2, offset, 2.0, step);
            SetPosition(C3, offset, 3.0, step);
            SetPosition(C4, offset, 4.0, step);
            SetPosition(C5, offset, 5.0, step);
            SetPosition(C6, offset, 6.0, step);
            SetPosition(C7, offset, 7.0, step);
            SetPosition(C8, offset, 8.0, step);
        }

        private void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50.0
                + Math.Sin(offset + posOffSet * step) * 50.0);

            ellipse.SetValue(Canvas.TopProperty, 50
                + Math.Cos(offset + posOffSet * step) * 50.0);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void OnVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;
            if (isVisible)
                Start();
            else
                Stop();
           
        }
        #endregion
    }
}
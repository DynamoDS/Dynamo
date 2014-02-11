using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.Utilities
{
    /// <summary>
    /// For double clicks
    /// 
    /// Origin: http://stackoverflow.com/questions/1821076/wpf-stackpanel-with-click-and-doubleclick
    /// </summary>
    public class MouseClickManager
    {
        private event MouseButtonEventHandler _click;
        private event MouseButtonEventHandler _doubleClick;

        public event MouseButtonEventHandler Click
        {
            add { _click += value; }
            remove { _click -= value; }
        }

        public event MouseButtonEventHandler DoubleClick
        {
            add { _doubleClick += value; }
            remove { _doubleClick -= value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MouseClickManager"/> is clicked.
        /// </summary>
        /// <value><c>true</c> if clicked; otherwise, <c>false</c>.</value>
        private bool Clicked { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>The timeout.</value>
        public int DoubleClickTimeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseClickManager"/> class.
        /// </summary>
        /// <param name="control">The control.</param>
        public MouseClickManager(int doubleClickTimeout)
        {
            Clicked = false;
            DoubleClickTimeout = doubleClickTimeout;
        }

        /// <summary>
        /// Handles the click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        public void HandleClick(object sender, MouseButtonEventArgs e)
        {
            lock (this)
            {
                if (Clicked)
                {
                    Clicked = false;
                    OnDoubleClick(sender, e);
                }
                else
                {
                    Clicked = true;
                    ParameterizedThreadStart threadStart = ResetThread;
                    Thread thread = new Thread(threadStart);
                    thread.Start(e);
                }
            }
        }

        /// <summary>
        /// Resets the thread.
        /// </summary>
        /// <param name="state">The state.</param>
        private void ResetThread(object state)
        {
            Thread.Sleep(DoubleClickTimeout);

            lock (this)
            {
                if (Clicked)
                {
                    Clicked = false;
                    OnClick(this, (MouseButtonEventArgs)state);
                }
            }
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (_click != null)
            {
                if (sender is Control)
                {
                    (sender as Control).Dispatcher.BeginInvoke(_click, sender, e);
                }
            }
        }

        /// <summary>
        /// Called when [double click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_doubleClick != null)
            {
                _doubleClick(sender, e);
            }
        }
    }
}

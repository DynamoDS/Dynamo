using System;
using System.Windows;
using Dynamo.Logging;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public class ConnectorContextMenuViewModel : ViewModelBase
    {
        #region Properties
        private Point currentPosition;
        private bool isCollapsed;
        private ConnectorViewModel ViewModel { get; set; }

        /// <summary>
        /// Controls connector visibility: on/off. When wire is off, additional styling xaml turns off tooltips.
        /// </summary>
        public bool IsCollapsed
        {
            get => isCollapsed;
            set
            {
                if (isCollapsed == value)
                {
                    return;
                }

                isCollapsed = value;
                RaisePropertyChanged(nameof(IsCollapsed));
            }
        }

        /// <summary>
        /// Location of this control.
        /// </summary>
        public Point CurrentPosition
        {
            get
            {
                return currentPosition;
            }
            set
            {
                currentPosition = value;
                RaisePropertyChanged(nameof(CurrentPosition));
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectorViewModel"></param>
        public ConnectorContextMenuViewModel(ConnectorViewModel connectorViewModel)
        {
            ViewModel = connectorViewModel;
            InitCommands();
        }

        /// <summary>
        /// Calls for ConnectorViewModel to dispose this instance.
        /// </summary>
        internal void RequestDisposeViewModel()
        {
            OnRequestDispose(this, EventArgs.Empty);
        }
        #region Events
        public event EventHandler RequestDispose;
        public virtual void OnRequestDispose(Object sender, EventArgs e)
        {
            RequestDispose(this, e);
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
        }
        #region Commands

        /// <summary>
        /// Alerts ConnectorViewModel to hide/show the connector.
        /// </summary>
        public DelegateCommand HideConnectorSurrogateCommand { get; set; }

        /// <summary>
        /// Alerts ConnectorViewModel select connected nodes.
        /// </summary>
        public DelegateCommand SelectConnectedSurrogateCommand { get; set; }
        /// <summary>
        /// Alerts ConnectorViewModel to break the current connection.
        /// </summary>
        public DelegateCommand BreakConnectionsSurrogateCommand { get; set; }
        /// <summary>
        /// Alerts ConnectorViewModel to pin the connector
        /// </summary>
        public DelegateCommand PinConnectedSurrogateCommand { get; set; }
        /// <summary>
        /// Alerts ConnectorViewModel to focus the view on start node
        /// </summary>
        public DelegateCommand GoToStartNodeCommand { get; set; }
        /// <summary>
        /// Alerts ConnectorViewModel to focus the view on end node
        /// </summary>
        public DelegateCommand GoToEndNodeCommand { get; set; }

        private void InitCommands()
        {
            HideConnectorSurrogateCommand = new DelegateCommand(HideConnectorSurrogateCommandExecute, x => true);
            SelectConnectedSurrogateCommand = new DelegateCommand(SelectConnectedSurrogateCommandExecute, x => true);
            BreakConnectionsSurrogateCommand = new DelegateCommand(BreakConnectionsSurrogateCommandExecute, x => true);
            PinConnectedSurrogateCommand = new DelegateCommand(PinConnectedSurrogateCommandExecute, x => true);
            GoToStartNodeCommand = new DelegateCommand(GoToStartNodeCommandExecute, x => true);
            GoToEndNodeCommand = new DelegateCommand(GoToEndNodeCommandExecute, x => true);
        }

        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void BreakConnectionsSurrogateCommandExecute(object obj)
        {
            ViewModel.BreakConnectionCommand.Execute(null);
            // Track break connection event, this is distinguished with break connections from input/output port.
            // So sending connector and number of connector as 1
            Analytics.TrackEvent(Actions.Break, Categories.ConnectorOperations, "Connector", 1);
        }

        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void SelectConnectedSurrogateCommandExecute(object obj)
        {
            ViewModel.SelectConnectedCommand.Execute(null);
            // Track select connected nodes event
            Analytics.TrackEvent(Actions.Select, Categories.ConnectorOperations, "SelectConnected");
        }
        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void HideConnectorSurrogateCommandExecute(object obj)
        {
            // Track Show or hide connected nodes event
            if (ViewModel.IsHidden)
            {
                Analytics.TrackEvent(Actions.Show, Categories.ConnectorOperations, "Connector", 1);
            }
            else
            {
                Analytics.TrackEvent(Actions.Hide, Categories.ConnectorOperations, "Connector", 1);
            }
            ViewModel.ShowhideConnectorCommand.Execute(null);
        }

        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void PinConnectedSurrogateCommandExecute(object obj)
        {
            ViewModel.PinConnectorCommand.Execute(null);
            // Track pin connected nodes event
            Analytics.TrackEvent(Actions.Pin, Categories.ConnectorOperations, "PinWire");
        }

        /// <summary>
        /// Executes the start node command on connector view model
        /// </summary>
        /// <param name="obj"></param>
        private void GoToStartNodeCommandExecute(object obj)
        {
            ViewModel.GoToStartNodeCommand.Execute(null);
        }

        /// <summary>
        /// Executes the end node command on connector view model
        /// </summary>
        /// <param name="obj"></param>
        private void GoToEndNodeCommandExecute(object obj)
        {
            ViewModel.GoToEndNodeCommand.Execute(null);
        }

        #endregion


    }
}

using System.Windows;
using System.Windows.Threading;
using Dynamo.Core;
using Dynamo.UI.Commands;
using System;

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
        /// Alerts ConnectorViewModel select connnected nodes.
        /// </summary>
        public DelegateCommand SelectConnectedSurrogateCommand { get; set; }
        /// <summary>
        /// Alets ConnectorViewModel to break the current connection.
        /// </summary>
        public DelegateCommand BreakConnectionsSurrogateCommand { get; set; }

        private void InitCommands()
        {
            HideConnectorSurrogateCommand = new DelegateCommand(HideConnectorSurrogateCommandExecute, x => true);
            SelectConnectedSurrogateCommand = new DelegateCommand(SelectConnectedSurrogateCommandExecute, x => true);
            BreakConnectionsSurrogateCommand = new DelegateCommand(BreakConnectionsSurrogateCommandExecute, x => true);
        }

        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void BreakConnectionsSurrogateCommandExecute(object obj)
        {
            ViewModel.BreakConnectionCommand.Execute(null);
        }
        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void SelectConnectedSurrogateCommandExecute(object obj)
        {
            ViewModel.SelectConnectedCommand.Execute(null);
        }
        /// <summary>
        /// Request disposal of this viewmodel after command has run.
        /// </summary>
        /// <param name="obj"></param>
        private void HideConnectorSurrogateCommandExecute(object obj)
        {
            ViewModel.HideConnectorCommand.Execute(null);
        }

        #endregion


    }
}

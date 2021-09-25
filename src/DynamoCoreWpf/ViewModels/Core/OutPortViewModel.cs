using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    public partial class OutPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand _breakConnectionsCommand;
        private DelegateCommand _hideConnectionsCommand;
        private DelegateCommand portMouseLeftButtonOnContextCommand;

        private bool _showContextMenu;
        private bool areConnectorsHidden;
        private string showHideWiresButtonContent = "";
        private bool hideWiresButtonEnabled;

        /// <summary>
        /// Sets the condensed styling on Code Block output ports.
        /// This is used to style the output ports on Code Blocks to be smaller.
        /// </summary>
        public bool IsPortCondensed
        {
            get
            {
                return this.PortModel.Owner is CodeBlockNodeModel && PortType == PortType.Output;
            }
        }

        /// <summary>
        /// If should display Use Levels popup menu. 
        /// </summary>
        public bool ShowContextMenu
        {
            get
            {
                return _showContextMenu;
            }
            set
            {
                _showContextMenu = value;
                RaisePropertyChanged("ShowContextMenu");
            }
        }

        /// <summary>
        /// The visibility of Use Levels menu.
        /// </summary>
        public Visibility UseContextMenuVisibility
        {
            get
            {
                if (IsPortCondensed)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Shows or hides the Break Connections, Hide Wires and UnhideWires buttons in the node chevron popup menu.
        /// </summary>
        public bool OutputPortConnectionsButtonsVisible
        {
            get => _port.PortType == PortType.Output;
        }

        /// <summary>
        /// Enables or disables the Break Connections button on the node output port context menu.
        /// </summary>
        public bool OutputPortBreakConnectionsButtonEnabled
        {
            get => OutputPortConnectionsButtonsVisible && IsConnected;
        }

        /// <summary>
        /// Determines whether the output port button says 'Hide Wires' or 'Show Wires'
        /// </summary>
        public string ShowHideWiresButtonContent
        {
            get => showHideWiresButtonContent;
            set
            {
                showHideWiresButtonContent = value;
                RaisePropertyChanged(nameof(ShowHideWiresButtonContent));
            }
        }

        /// <summary>
        /// Sets the visibility of the connectors from the port. This will overwrite the 
        /// individual visibility of the connectors. However when visibility is controlled 
        /// from the connector, that connector's visibility will overwrite its previous state.
        /// In order to overwrite visibility of all connectors associated with a port, us this 
        /// flag again.
        /// </summary>
        public bool SetConnectorsVisibility
        {
            get => areConnectorsHidden;
            set
            {
                areConnectorsHidden = value; 
                RaisePropertyChanged(nameof(SetConnectorsVisibility));
            }
        }

        /// <summary>
        /// Enables or disables the Hide Wires button on the node output port context menu.
        /// </summary>
        public bool HideWiresButtonEnabled
        {
            get => hideWiresButtonEnabled;
            set
            {
                hideWiresButtonEnabled = value; 
                RaisePropertyChanged(nameof(HideWiresButtonEnabled));
            }
        }

        /// <summary>
        /// Takes care of the multiple UI concerns when dealing with the Unhide/Hide Wires button
        /// on the output port's context menu.
        /// </summary>
        private void RefreshHideWiresButton()
        {
            HideWiresButtonEnabled = _port.Connectors.Count > 0;
            SetConnectorsVisibility = CheckIfConnectorsAreHidden();

            ShowHideWiresButtonContent = SetConnectorsVisibility
                ? Wpf.Properties.Resources.ShowWiresPopupMenuItem
                : Wpf.Properties.Resources.HideWiresPopupMenuItem;

            RaisePropertyChanged(nameof(ShowHideWiresButtonContent));
        }

        #endregion

        public OutPortViewModel(NodeViewModel node, PortModel port) :base(node, port)
        {
            _node.PropertyChanged += _node_PropertyChanged;
            _port.PropertyChanged += _port_PropertyChanged;
            RefreshHideWiresButton();
        }

        public override void Dispose()
        {
            _port.PropertyChanged -= _port_PropertyChanged;
            _node.PropertyChanged -= _node_PropertyChanged;
            base.Dispose();
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            return new OutPortViewModel(_node, portModel);
        }

        void _node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NodeViewModel.ZIndex):
                    RefreshHideWiresButton();
                    break;
            }
        }

        void _port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsConnected":
                    RaisePropertyChanged(nameof(OutputPortBreakConnectionsButtonEnabled));
                    RefreshHideWiresButton();
                    break;
            }
        }

        /// <summary>
        /// Used by the 'Break Connection' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        public DelegateCommand BreakConnectionsCommand
        {
            get
            {
                if (_breakConnectionsCommand == null)
                {
                    _breakConnectionsCommand = new DelegateCommand(BreakConnections);
                }
                return _breakConnectionsCommand;
            }
        }

        /// <summary>
        /// Used by the 'Break Connection' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        public DelegateCommand HideConnectionsCommand
        {
            get
            {
                if (_hideConnectionsCommand == null)
                {
                    _hideConnectionsCommand = new DelegateCommand(HideConnections);
                }
                return _hideConnectionsCommand;
            }
        }

        /// <summary>
        /// Used by the 'Break Connection' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        /// <param name="parameter"></param>
        private void BreakConnections(object parameter)
        {
            for (int i = _port.Connectors.Count - 1; i >= 0; i--)
            {
                // Attempting to get the relevant ConnectorViewModel via matching GUID
                ConnectorViewModel connectorViewModel = _node.WorkspaceViewModel.Connectors
                    .FirstOrDefault(x => x.ConnectorModel.GUID == _port.Connectors[i].GUID);

                if (connectorViewModel == null) continue;

                connectorViewModel.BreakConnectionCommand.Execute(null);
            }
        }

        /// <summary>
        /// Used by the 'Hide Wires' button in the node output context menu.
        /// Turns of the visibility of any connections this port has.
        /// </summary>
        /// <param name="parameter"></param>
        private void HideConnections(object parameter)
        {
            for (int i = _port.Connectors.Count - 1; i >= 0; i--)
            {
                // Attempting to get the relevant ConnectorViewModel via matching GUID
                ConnectorViewModel connectorViewModel = _node.WorkspaceViewModel.Connectors
                    .FirstOrDefault(x => x.ConnectorModel.GUID == _port.Connectors[i].GUID);

                if (connectorViewModel == null) continue;

                connectorViewModel.HideConnectorCommand.Execute(!SetConnectorsVisibility);
            }
            RefreshHideWiresButton();
        }

        /// <summary>
        /// Returns true if they are hidden.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfConnectorsAreHidden()
        {
            if (_port.Connectors.Count < 1 || _node.WorkspaceViewModel.Connectors.Count < 1) return false;

            // Attempting to get a relevant ConnectorViewModel via matching NodeModel GUID
            ConnectorViewModel connectorViewModel = _node.WorkspaceViewModel.Connectors
                .FirstOrDefault(x => x.Nodevm.NodeModel.GUID == _port.Owner.GUID);

            if (connectorViewModel == null) return false;
            return connectorViewModel.IsCollapsed;
        }

        public DelegateCommand MouseLeftButtonDownOnContextCommand
        {
            get
            {
                if (portMouseLeftButtonOnContextCommand == null)
                    portMouseLeftButtonOnContextCommand = new DelegateCommand(OnMouseLeftButtonDownOnContext, CanConnect);

                return portMouseLeftButtonOnContextCommand;
            }
        }

        /// <summary>
        /// Handles the Mouse left button down on the level.
        /// </summary>
        /// <param name="parameter"></param>
        private void OnMouseLeftButtonDownOnContext(object parameter)
        {
            ShowContextMenu = true;
        }
    }
}

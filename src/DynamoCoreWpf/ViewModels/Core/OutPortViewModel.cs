using System;
using System.Linq;
using System.Windows;
using Dynamo.Graph.Nodes;
using Dynamo.UI.Commands;

namespace Dynamo.ViewModels
{
    public partial class OutPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand breakConnectionsCommand;
        private DelegateCommand hideConnectionsCommand;
        private DelegateCommand portMouseLeftButtonOnContextCommand;

        private bool showContextMenu;
        private bool areConnectorsHidden;
        private string showHideWiresButtonContent = "";
        private bool hideWiresButtonEnabled;

        /// <summary>
        /// Sets the condensed styling on Code Block output ports.
        /// This is used to style the output ports on Code Blocks to be smaller.
        /// </summary>
        public bool IsPortCondensed => this.PortModel.Owner is CodeBlockNodeModel;

        /// <summary>
        /// If should display Use Levels popup menu. 
        /// </summary>
        public bool ShowContextMenu
        {
            get => showContextMenu;
            set
            {
                showContextMenu = value;
                RaisePropertyChanged(nameof(ShowContextMenu));
            }
        }

        /// <summary>
        /// The visibility of Use Levels menu.
        /// </summary>
        public Visibility UseContextMenuVisibility
        {
            get
            {
                if (IsPortCondensed  || PortName.Equals(">"))
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
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
            HideWiresButtonEnabled = port.Connectors.Count > 0;
            SetConnectorsVisibility = CheckIfConnectorsAreHidden();

            ShowHideWiresButtonContent = SetConnectorsVisibility
                ? Wpf.Properties.Resources.ShowWiresPopupMenuItem
                : Wpf.Properties.Resources.HideWiresPopupMenuItem;

            RaisePropertyChanged(nameof(ShowHideWiresButtonContent));
        }

        #endregion

        public OutPortViewModel(NodeViewModel node, PortModel port) :base(node, port)
        {
            node.PropertyChanged += NodePropertyChanged;
            port.PropertyChanged += PortPropertyChanged;
            RefreshHideWiresButton();
        }

        public override void Dispose()
        {
            port.PropertyChanged -= PortPropertyChanged;
            node.PropertyChanged -= NodePropertyChanged;
            base.Dispose();
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            return new OutPortViewModel(node, portModel);
        }

        private void NodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(NodeViewModel.ZIndex):
                    RefreshHideWiresButton();
                    break;
            }
        }

        private void PortPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsConnected):
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
            get { return breakConnectionsCommand ?? (breakConnectionsCommand = new DelegateCommand(BreakConnections)); }
        }

        /// <summary>
        /// Used by the 'Break Connection' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        public DelegateCommand HideConnectionsCommand
        {
            get { return hideConnectionsCommand ?? (hideConnectionsCommand = new DelegateCommand(HideConnections)); }
        }

        /// <summary>
        /// Used by the 'Break Connection' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        /// <param name="parameter"></param>
        private void BreakConnections(object parameter)
        {
            for (var i = port.Connectors.Count - 1; i >= 0; i--)
            {
                // Attempting to get the relevant ConnectorViewModel via matching GUID
                ConnectorViewModel connectorViewModel = node.WorkspaceViewModel.Connectors
                    .FirstOrDefault(x => x.ConnectorModel.GUID == port.Connectors[i].GUID);

                if (connectorViewModel == null)
                {
                    continue;
                }

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
            for (var i = port.Connectors.Count - 1; i >= 0; i--)
            {
                // Attempting to get the relevant ConnectorViewModel via matching GUID
                var connectorViewModel = node.WorkspaceViewModel.Connectors
                    .FirstOrDefault(x => x.ConnectorModel.GUID == port.Connectors[i].GUID);

                if (connectorViewModel == null)
                {
                    continue;
                }

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
            if (port.Connectors.Count < 1 || node.WorkspaceViewModel.Connectors.Count < 1) return false;

            // Attempting to get a relevant ConnectorViewModel via matching NodeModel GUID
            ConnectorViewModel connectorViewModel = node.WorkspaceViewModel.Connectors
                .FirstOrDefault(x => x.Nodevm.NodeModel.GUID == port.Owner.GUID);

            if (connectorViewModel == null)
            {
                return false;
            }

            return connectorViewModel.IsHidden;
        }

        /// <summary>
        /// Handles the Mouse left button click on the node context menu button.
        /// </summary>
        public DelegateCommand MouseLeftButtonDownOnContextCommand
        {
            get
            {
                return portMouseLeftButtonOnContextCommand ?? (portMouseLeftButtonOnContextCommand =
                    new DelegateCommand(OnMouseLeftButtonDownOnContext, CanConnect));
            }
        }

        private void OnMouseLeftButtonDownOnContext(object parameter)
        {
            ShowContextMenu = true;
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DSCore;
using Dynamo.Graph.Nodes;
using Dynamo.Logging;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.UI.Commands;
using ProtoCore.Utils;
using UI.Prompts;
using Color = System.Windows.Media.Color;

namespace Dynamo.ViewModels
{
    public partial class OutPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand breakConnectionsCommand;
        private DelegateCommand hideConnectionsCommand;
        private DelegateCommand editPortPropertiesCommand;
        private DelegateCommand portMouseLeftButtonOnContextCommand;

        private SolidColorBrush portValueMarkerColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

        internal static SolidColorBrush PortValueMarkerGrey = new SolidColorBrush(Color.FromRgb(153, 153, 153));
        internal static int valueMarkerWidthWithFunction = 5;
        internal static int valueMarkerWidthWithoutFunction = 0;

        private bool showContextMenu;
        private bool areConnectorsHidden;
        private string showHideWiresButtonContent = "";
        private bool hideWiresButtonEnabled;
        private bool renameNodeButtonEnabled;
        private bool portDefaultValueMarkerVisible;
        private int valueMarkerWidth;

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
        internal bool AreConnectorsHidden
        {
            get => areConnectorsHidden;
            set
            {
                areConnectorsHidden = value;
                RaisePropertyChanged(nameof(AreConnectorsHidden));
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
        ///  Enables or disables the Rename Node button on the node output port context menu.
        /// </summary>
        public bool RenameNodeButtonEnabled
        {
            get => renameNodeButtonEnabled;
            set
            {
                renameNodeButtonEnabled = value;
                RaisePropertyChanged(nameof(RenameNodeButtonEnabled));
            }
        }

        /// <summary>
        /// Sets the color of the small rectangular marker on each input port.
        /// </summary>
        public SolidColorBrush PortValueMarkerColor
        {
            get => portValueMarkerColor;
            set
            {
                portValueMarkerColor = value;
                RaisePropertyChanged(nameof(PortValueMarkerColor));
            }
        }


        public bool PortDefaultValueMarkerVisible
        {
            get => portDefaultValueMarkerVisible;
            set
            {
                portDefaultValueMarkerVisible = value;
                RaisePropertyChanged(nameof(PortDefaultValueMarkerVisible));
            }
        }

        /// <summary>
        /// Returns the width from the ValueMarkerColumn
        /// </summary>
        public int ValueMarkerWidth
        {
            get => valueMarkerWidth;
        }

        /// <summary>
        /// Returns the width to assign to the ValueMarkerColumn when is function state
        /// </summary>
        public int ValueMarkerWidthWithFunction
        {
            get => valueMarkerWidthWithFunction;
        }

        /// <summary>
        /// Returns the width to assign to the ValueMarkerColumn when is not function state
        /// </summary>
        public int ValueMarkerWidthWithoutFunction
        {
            get => valueMarkerWidthWithoutFunction;
        }

        /// <summary>
        /// Takes care of the multiple UI concerns when dealing with the Unhide/Hide Wires button
        /// on the output port's context menu.
        /// </summary>
        internal void RefreshHideWiresState()
        {
            HideWiresButtonEnabled = port.Connectors.Count > 0;
            AreConnectorsHidden = CheckIfConnectorsAreHidden();

            ShowHideWiresButtonContent = AreConnectorsHidden
                ? Wpf.Properties.Resources.ShowWiresPopupMenuItem
                : Wpf.Properties.Resources.HideWiresPopupMenuItem;

            RaisePropertyChanged(nameof(ShowHideWiresButtonContent));
        }

        internal void EnableRenamePort()
        {
            RenameNodeButtonEnabled = true;
            RaisePropertyChanged(nameof(RenameNodeButtonEnabled));
        }

        #endregion

        public OutPortViewModel(NodeViewModel node, PortModel port) :base(node, port)
        {
            port.PropertyChanged += PortPropertyChanged;
            node.NodeModel.PropertyChanged += NodeModel_PropertyChanged;

            RefreshHideWiresState();
            if (node.NodeModel is PythonNodeModels.PythonNode)
            {
                EnableRenamePort();
            }
        }

        public override void Dispose()
        {
            port.PropertyChanged -= PortPropertyChanged; 
            node.NodeModel.PropertyChanged -= NodeModel_PropertyChanged;

            base.Dispose();
        }

        private void NodeModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(node.NodeModel.CachedValue):
                    RefreshPortColors();
                    break;
            }
        }

        private void PortPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(UsingDefaultValue):
                    RefreshPortColors();
                    break;
                case nameof(IsConnected):
                    RefreshInputPorts();
                    break;
            }
        }

        private void RefreshInputPorts()
        {
            foreach (var portViewModel in node.InPorts)
            {
                var inPort = (InPortViewModel)portViewModel;
                inPort.RefreshInputPortsByOutputConnectionChanged(port.IsConnected);
            }
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            portModel.IsProxyPort = true;
            return new OutPortViewModel(node, portModel);
        }

        /// <summary>
        /// Used by the 'Break Connections' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        public DelegateCommand BreakConnectionsCommand
        {
            get { return breakConnectionsCommand ?? (breakConnectionsCommand = new DelegateCommand(BreakConnections)); }
        }

        /// <summary>
        /// Used by the 'Show/Hide Wires' button in the node output context menu.
        /// Hides or Shows any connections this port has.
        /// </summary>
        public DelegateCommand HideConnectionsCommand
        {
            get { return hideConnectionsCommand ?? (hideConnectionsCommand = new DelegateCommand(HideConnections)); }
        }

        /// <summary>
        /// Used by the 'Edit Port Properties' button in the node output context menu.
        /// Triggers the Port Properties Panel
        /// </summary>
        public DelegateCommand EditPortPropertiesCommand
        {
            get
            {
                return editPortPropertiesCommand ??
                       (editPortPropertiesCommand = new DelegateCommand(EditPortProperties)); 
            }
        }

        /// <summary>
        /// Used by the 'Break Connections' button in the node output context menu.
        /// Removes any current connections this port has.
        /// </summary>
        /// <param name="parameter"></param>
        private void BreakConnections(object parameter)
        {
            // Send analytics data ahead of the actual break operation so connector count is still accurate
            Analytics.TrackEvent(Actions.Break, Categories.ConnectorOperations, port.PortType.ToString(), port.Connectors.Count);
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
        /// Used by the 'Hide Wires' / 'Show Wires' button in the node output context menu.
        /// Flips of the visibility of any connections this port has.
        /// </summary>
        /// <param name="parameter"></param>
        private void HideConnections(object parameter)
        {
            foreach(var connector in port.Connectors)
            {
                // Attempting to get the relevant ConnectorViewModel via matching GUID
                var connectorViewModel = node.WorkspaceViewModel.Connectors
                    .FirstOrDefault(x => x.ConnectorModel.GUID == connector.GUID);
                connectorViewModel?.ShowhideConnectorCommand.Execute(!AreConnectorsHidden);
            }
            if (AreConnectorsHidden)
            {
                Analytics.TrackEvent(Actions.Show, Categories.ConnectorOperations, port.PortType.ToString(), port.Connectors.Count);
            }
            else
            {
                Analytics.TrackEvent(Actions.Hide, Categories.ConnectorOperations, port.PortType.ToString(), port.Connectors.Count);
            }
            RefreshHideWiresState();
        }

        /// <summary>
        /// Used by the 'Edit Port Properties' button in the node output context menu.
        /// Triggers the Port Properties Panel
        /// </summary>
        private void EditPortProperties(object parameter)
        {
            var wsViewModel = node.WorkspaceViewModel;

            // Hide the popup, we no longer need it
            wsViewModel.OnRequestPortContextMenu(ShowHideFlags.Show, this);

            var dialog = new PortPropertiesEditPrompt()
            {
                DescriptionInput = { Text = port.ToolTip },
                nameBox = { Text = port.Name },
                PortType = PortType.Output,
                OutPortNames = ListOutportNames(this.NodeViewModel.OutPorts),
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            port.Name = dialog.PortName;
            port.ToolTip = dialog.Description;

            RaisePropertyChanged(nameof(PortName));
        }

        private List<string> ListOutportNames(ObservableCollection<PortViewModel> outPorts)
        {
            return outPorts.Where(x => !x.PortName.Equals(this.PortName)).Select(x => x.PortName).ToList();
        }

        /// <summary>
        /// Returns true if they are hidden.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfConnectorsAreHidden()
        {
            if (port.Connectors.Count < 1 || node.WorkspaceViewModel.Connectors.Count < 1) return false;

            foreach (var connector in port.Connectors)
            {
                if (connector.IsHidden)
                {
                    return true;
                }
            }

            return false;
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

        protected override void RefreshPortColors()
        {
            PortDefaultValueMarkerVisible = node.NodeModel.CachedValue != null && node.NodeModel.CachedValue.IsFunction && !node.IsWatchNode;
            valueMarkerWidth = PortDefaultValueMarkerVisible ? valueMarkerWidthWithFunction : valueMarkerWidthWithoutFunction;
        }
    }
}

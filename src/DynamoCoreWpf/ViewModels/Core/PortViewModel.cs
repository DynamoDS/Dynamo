using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel : ViewModelBase
    {
        #region Properties/Fields

        private readonly PortModel _port;
        private readonly NodeViewModel _node;
        private DelegateCommand _useLevelsCommand;
        private DelegateCommand _keepListStructureCommand;
        private DelegateCommand _breakConnectionsCommand;
        private DelegateCommand _hideConnectionsCommand;
        private const double autocompletePopupSpacing = 2.5;
        private SolidColorBrush portBorderBrushColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
        private SolidColorBrush portBackgroundColor = new SolidColorBrush(Color.FromArgb(0, 60, 60, 60));
        internal bool inputPortDisconnectedByConnectCommand = false;
        private bool _showUseLevelMenu;
        private bool areConnectorsHidden;
        private string showHideWiresButtonContent = "";
        private bool hideWiresButtonEnabled;

        /// <summary>
        /// Port model.
        /// </summary>
        public PortModel PortModel
        {
            get { return _port; }
        }

        /// <summary>
        /// The content of tooltip.
        /// </summary>
        public string ToolTipContent
        {
            get { return _port.ToolTip; }
        }

        /// <summary>
        /// Port name.
        /// </summary>
        public string PortName
        {
            get { return GetPortDisplayName(_port.Name); }
        }

        /// <summary>
        /// Port type.
        /// </summary>
        public PortType PortType
        {
            get { return _port.PortType; }
        }


        /// <summary>
        /// If port is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _node.IsSelected; }
        }

        /// <summary>
        /// If port is connected.
        /// </summary>
        public bool IsConnected
        {
            get => _port.IsConnected;
        }

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
        /// If port is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return _port.IsEnabled; }
        }

        /// <summary>
        /// The height of port.
        /// </summary>
        public double Height
        {
            get { return _port.Height; }
        }

        /// <summary>
        /// The center point of port.
        /// </summary>
        public Point Center
        {
            get { return _port.Center.AsWindowsType(); }
        }

        /// <summary>
        /// The state of host node.
        /// </summary>
        public ElementState State
        {
            get { return _node.State; }
        }

        /// <summary>
        /// Returns whether this port has a default value that can be used.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValue != null; }
        }
        
        /// <summary>
        /// Returns whether the port is using its default value, or whether this been disabled
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set
            {
                _port.UsingDefaultValue = value;
                RaisePropertyChanged(nameof(UsingDefaultValueMarkerVisibile));
            }
        }

        /// <summary>
        /// IsHitTestVisible property gets a value that declares whether 
        /// a Snapping rectangle can possibly be returned as a hit test result.
        /// When FirstActiveConnector is not null, Snapping rectangle handles click events.
        /// When FirstActiveConnector is null, Snapping rectangle does not handle click events 
        /// and user can "click though invisible snapping area".
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return _node.WorkspaceViewModel.FirstActiveConnector != null; }
        }

        /// <summary>
        /// The margin thickness of port view.
        /// </summary>
        public System.Windows.Thickness MarginThickness
        {
            get { return _port.MarginThickness.AsWindowsType(); }
        }

        public PortEventType EventType { get; set; }

        /// <summary>
        /// If should display Use Levels popup menu. 
        /// </summary>
        public bool ShowUseLevelMenu
        {
            get
            {
                return _showUseLevelMenu;
            }
            set
            {
                _showUseLevelMenu = value;
                RaisePropertyChanged("ShowUseLevelMenu");
            }
        }

        /// <summary>
        /// If UseLevel is enabled on this port.
        /// </summary>
        public bool UseLevels
        {
            get { return _port.UseLevels; }
        }

        /// <summary>
        /// Determines whether or not the UseLevelsSpinner is visible on the port.
        /// </summary>
        public Visibility UseLevelSpinnerVisible
        {
            get
            {
                if (PortType == PortType.Output) return Visibility.Collapsed;
                if (UseLevels) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// Determines whether the blue marker appears beside an input port, indicating
        /// the default value for this port is being used.
        /// </summary>
        public bool UsingDefaultValueMarkerVisibile
        {
            get => PortType == PortType.Input && UsingDefaultValue && DefaultValueEnabled;
        }

        /// <summary>
        /// If should keep list structure on this port.
        /// </summary>
        public bool ShouldKeepListStructure
        {
            get { return _port.KeepListStructure; }
        }

        /// <summary>
        /// Levle of list.
        /// </summary>
        public int Level
        {
            get { return _port.Level; }
            set
            {
                ChangeLevel(value);
            }
        }

        /// <summary>
        /// The visibility of Use Levels menu.
        /// </summary>
        public Visibility UseLevelVisibility
        {
            get
            {
                if (_node.ArgumentLacing != LacingStrategy.Disabled)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        internal NodeViewModel NodeViewModel
        {
            get => _node;
        }
        
        /// <summary>
        /// Shows or hides the Use Levels and Keep List Structure checkboxes
        /// in the node chevron popup menu.
        /// </summary>
        public bool UseLevelCheckBoxVisibility
        {
            get => _port.PortType == PortType.Input;
        }

        /// <summary>
        /// Shows or hides the Use Default Value checkbox in the node chevron popup menu.
        /// </summary>
        public bool UseDefaultValueCheckBoxVisibility
        {
            get => _port.PortType == PortType.Input && DefaultValueEnabled;
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
                ? Properties.Resources.UnhideWiresPopupMenuItem
                : Properties.Resources.HideWiresPopupMenuItem;

            RaisePropertyChanged(nameof(ShowHideWiresButtonContent));
        }

        /// <summary>
        /// Sets the color of the port's border brush
        /// </summary>
        public SolidColorBrush PortBorderBrushColor
        {
            get => portBorderBrushColor;
            set
            {
                portBorderBrushColor = value;
                RaisePropertyChanged(nameof(PortBorderBrushColor));
            }
        }

        /// <summary>
        /// Sets the color of the port's background - affected by multiple factors such as
        /// MouseOver, IsConnected, Node States (active, inactie, frozen 
        /// </summary>
        public SolidColorBrush PortBackgroundColor
        {
            get => portBackgroundColor;
            set
            {
                portBackgroundColor = value;
                RaisePropertyChanged(nameof(PortBackgroundColor));
            }
        }

        #endregion

        #region events
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event EventHandler MouseLeftButtonDown;
        public event EventHandler MouseLeftButtonDownOnLevel;
        #endregion

        public PortViewModel(NodeViewModel node, PortModel port)
        {
            _node = node;
            _port = port;

            _port.PropertyChanged += _port_PropertyChanged;
            _node.PropertyChanged += _node_PropertyChanged;
            _node.WorkspaceViewModel.PropertyChanged += Workspace_PropertyChanged;

            RefreshPortColors();
            RefreshHideWiresButton();
        }

        public override void Dispose()
        {
            _port.PropertyChanged -= _port_PropertyChanged;
            _node.PropertyChanged -= _node_PropertyChanged;
            _node.WorkspaceViewModel.PropertyChanged -= Workspace_PropertyChanged;
        }

        internal PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            return new PortViewModel(_node, portModel);
        }

        /// <summary>
        /// Sets up the node autocomplete window to be placed relative to the node.
        /// </summary>
        /// <param name="popup">Node autocomplete popup.</param>
        internal void SetupNodeAutocompleteWindowPlacement(Popup popup)
        {
            _node.OnRequestAutoCompletePopupPlacementTarget(popup);
            popup.CustomPopupPlacementCallback = PlaceAutocompletePopup;
        }

        private CustomPopupPlacement[] PlaceAutocompletePopup(Size popupSize, Size targetSize, Point offset)
        {
            var zoom = _node.WorkspaceViewModel.Zoom;

            double x;
            var scaledSpacing = autocompletePopupSpacing * targetSize.Width / _node.ActualWidth;
            if (PortModel.PortType == PortType.Input)
            {
                // Offset popup to the left by its width from left edge of node and spacing.
                x = -scaledSpacing - popupSize.Width;
            }
            else
            {
                // Offset popup to the right by node width and spacing from left edge of node.
                x = scaledSpacing + targetSize.Width;
            }
            // Offset popup down from the upper edge of the node by the node header and corresponding to the respective port.
            // Scale the absolute heights by the target height (passed to the callback) and the actual height of the node.
            var scaledHeight = targetSize.Height / _node.ActualHeight;
            var absoluteHeight = NodeModel.HeaderHeight + PortModel.Index * PortModel.Height;
            var y = absoluteHeight * scaledHeight;

            var placement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None);

            return new[] { placement };
        }

        private void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ActiveConnector":
                    RaisePropertyChanged("IsHitTestVisible");
                    break;
            }
        }

        void _node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    RaisePropertyChanged("IsSelected");
                    break;
                case "State":
                    RaisePropertyChanged("State");
                    break;
                case "ToolTipContent":
                    RaisePropertyChanged("ToolTipContent");
                    break;
            }
        }

        void _port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ToolTip":
                    RaisePropertyChanged("ToolTipContent");
                    break;
                case "PortType":
                    RaisePropertyChanged("PortType");
                    break;
                case "PortName":
                    RaisePropertyChanged("PortName");
                    break;
                case "IsConnected":
                    RaisePropertyChanged(nameof(IsConnected));
                    RaisePropertyChanged(nameof(OutputPortBreakConnectionsButtonEnabled));
                    RefreshPortColors();
                    RefreshHideWiresButton();
                    break;
                case "IsEnabled":
                    RaisePropertyChanged("IsEnabled");
                    break;
                case "Center":
                    RaisePropertyChanged("Center");
                    break;
                case "DefaultValue":
                    RaisePropertyChanged("DefaultValue");
                    break;
                case "UsingDefaultValue":
                    RaisePropertyChanged("UsingDefaultValue");
                    RefreshPortColors();
                    break;
                case "MarginThickness":
                    RaisePropertyChanged("MarginThickness");
                    break;
                case "UseLevels":
                    RaisePropertyChanged("UseLevels");
                    break;
                case "Level":
                    RaisePropertyChanged("Level");
                    break;
                case "KeepListStructure":
                    RaisePropertyChanged("ShouldKeepListStructure");
                    RefreshPortColors();
                    break;
            }
        }

        /// <summary>
        /// UseLevels command
        /// </summary>
        public DelegateCommand UseLevelsCommand
        {
            get
            {
                if (_useLevelsCommand == null)
                {
                    _useLevelsCommand = new DelegateCommand(UseLevel, p => true);
                }
                return _useLevelsCommand;
            }
        }

        private void UseLevel(object parameter)
        {
            var useLevel = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, _node.NodeLogic.GUID, "UseLevels", string.Format("{0}:{1}", _port.Index, useLevel));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
            RaisePropertyChanged(nameof(UseLevelSpinnerVisible));
        }

        /// <summary>
        /// ShouldKeepListStructure command
        /// </summary>
        public DelegateCommand KeepListStructureCommand
        {
            get
            {
                if (_keepListStructureCommand == null)
                {
                    _keepListStructureCommand = new DelegateCommand(KeepListStructure, p => true);
                }
                return _keepListStructureCommand;
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

        private void KeepListStructure(object parameter)
        {
            bool keepListStructure = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, _node.NodeLogic.GUID, "KeepListStructure", string.Format("{0}:{1}", _port.Index, keepListStructure));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        private void ChangeLevel(int level)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                            Guid.Empty, _node.NodeLogic.GUID, "ChangeLevel", string.Format("{0}:{1}", _port.Index, level));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
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

                connectorViewModel.HideConnectorCommand.Execute(SetConnectorsVisibility);
            }
            RefreshHideWiresButton();
        }

        private bool CheckIfConnectorsAreHidden()
        {
            if (_port.Connectors.Count < 1 || _node.WorkspaceViewModel.Connectors.Count < 1) return false;

            // Attempting to get a relevant ConnectorViewModel via matching NodeModel GUID
            ConnectorViewModel connectorViewModel = _node.WorkspaceViewModel.Connectors
                .FirstOrDefault(x => x.Nodevm.NodeModel.GUID == _port.Owner.GUID);

            if (connectorViewModel == null) return false;
            return !connectorViewModel.IsDisplayed;
        }


        private void Connect(object parameter)
        {
            DynamoViewModel dynamoViewModel = this._node.DynamoViewModel;
            WorkspaceViewModel workspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;
            workspaceViewModel.HandlePortClicked(this);
        }

        private bool CanConnect(object parameter)
        {
            return true;
        }

        // Handler to invoke node Auto Complete
        private void AutoComplete(object parameter)
        {
            var wsViewModel = _node.WorkspaceViewModel;
            wsViewModel.NodeAutoCompleteSearchViewModel.PortViewModel = this;

            // If the input port is disconnected by the 'Connect' command while triggering Node AutoComplete, undo the port disconnection.
            if (this.inputPortDisconnectedByConnectCommand)
            {
                wsViewModel.DynamoViewModel.Model.CurrentWorkspace.Undo();
            }

            // Bail out from connect state
            wsViewModel.CancelActiveState();
            wsViewModel.OnRequestNodeAutoCompleteSearch(ShowHideFlags.Show);
        }

        private bool CanAutoComplete(object parameter)
        {
            DynamoViewModel dynamoViewModel = _node.DynamoViewModel;
            // If the feature is enabled from Dynamo experiment setting and if user interaction is on input port.
            return dynamoViewModel.EnableNodeAutoComplete;
        }

        /// <summary>
        /// Handles the Mouse enter event on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseEnter(object parameter)
        {
            MouseEnter?.Invoke(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse leave on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeave(object parameter)
        {
            MouseLeave?.Invoke(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse left button down on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeftButtonDown(object parameter)
        {
            MouseLeftButtonDown?.Invoke(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse left button down on the level.
        /// </summary>
        /// <param name="parameter"></param>
        private void OnMouseLeftButtonDownOnLevel(object parameter)
        {
            ShowUseLevelMenu = true;
        }

        /// <summary>
        /// Handle the Mouse left from Use Level popup.
        /// </summary>
        /// <param name="parameter"></param>
        private void OnMouseLeftUseLevel(object parameter)
        {
            ShowUseLevelMenu = false;
        }

        /// <summary>
        /// Handles the logic for updating the PortBackgroundColor and PortBackgroundBrushColor
        /// </summary>
        private void RefreshPortColors()
        {
            // The visual appearance of ports can be affected by many different logical states
            // Inputs have more display styles than outputs
            if (_port.PortType == PortType.Input)
            {
                // Special case for keeping list structure visual appearance
                if (_port.UseLevels && _port.KeepListStructure && _port.IsConnected)
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(94, 165, 196));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
                }

                // Port has a default value, shows blue marker
                else if (UsingDefaultValue && DefaultValueEnabled)
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 90, 99));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
                }
                else
                {
                    // Port isn't connected and has no default value (or isn't using it)
                    if (!_port.IsConnected)
                    {
                        PortBackgroundColor = new SolidColorBrush(Color.FromRgb(107, 67, 67));
                        PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(244, 134, 134));
                    }
                    // Port is connected and has no default value
                    else
                    {
                        PortBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 90, 99));
                        PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
                    }
                }
            }
            // It's an output port, which either displays a connected style or a disconnected style
            else
            {
                if (_port.IsConnected)
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(70, 90, 99));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(106, 192, 231));
                }
                else
                {
                    PortBackgroundColor = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    PortBorderBrushColor = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                }
            }
        }

        /// <summary>
        /// Replaces the old PortNameConverter.
        /// Ports without names are generally converter chevrons i.e. '>'. However, if an output
        /// port is displaying its context menu chevron AND has no name (e.g. the Function node)
        /// the output port is renamed in order to avoid confusing the user with double chevrons.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetPortDisplayName(string value)
        {
            if (value is string && !string.IsNullOrEmpty(value as string))
            {
                return value as string;
            }
            if (_node.ArgumentLacing != LacingStrategy.Disabled)
            {
                switch (_port.PortType)
                {
                    case PortType.Input:
                        return Properties.Resources.InputPortAlternativeName;
                    case PortType.Output:
                        return Properties.Resources.OutputPortAlternativeName;
                }
            }
            return ">";
        }
    }
}

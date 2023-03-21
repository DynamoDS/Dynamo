using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.UI.Commands;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Port View Model
    /// </summary>
    public partial class PortViewModel : ViewModelBase
    {
        #region Properties/Fields

        protected readonly PortModel port;
        protected readonly NodeViewModel node;
        private DelegateCommand useLevelsCommand;
        private DelegateCommand keepListStructureCommand;
        private bool showUseLevelMenu;
        private const double autocompletePopupSpacing = 2.5;
        internal bool inputPortDisconnectedByConnectCommand = false;
        protected static readonly SolidColorBrush PortBackgroundColorPreviewOff = new SolidColorBrush(Color.FromRgb(102, 102, 102));
        protected static readonly SolidColorBrush PortBackgroundColorDefault = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        protected static readonly SolidColorBrush PortBorderBrushColorDefault = new SolidColorBrush(Color.FromRgb(161, 161, 161));
        private SolidColorBrush portBorderBrushColor = PortBorderBrushColorDefault;
        private SolidColorBrush portBackgroundColor = PortBackgroundColorDefault;
        /// <summary>
        /// Port model.
        /// </summary>
        public PortModel PortModel
        {
            get { return port; }
        }

        /// <summary>
        /// The content of tooltip.
        /// </summary>
        public string ToolTipContent
        {
            get { return port.ToolTip; }
        }

        /// <summary>
        /// Port name.
        /// </summary>
        public string PortName
        {
            get { return GetPortDisplayName(port.Name); }
        }

        /// <summary>
        /// Port type.
        /// </summary>
        public PortType PortType
        {
            get { return port.PortType; }
        }


        /// <summary>
        /// If port is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return node.IsSelected; }
        }

        /// <summary>
        /// If port is connected.
        /// </summary>
        public bool IsConnected
        {
            get => port.IsConnected;
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
            get { return port.IsEnabled; }
        }

        /// <summary>
        /// The height of port.
        /// </summary>
        public double Height
        {
            get { return port.Height; }
        }

        /// <summary>
        /// The center point of port.
        /// </summary>
        public Point Center
        {
            get { return port.Center.AsWindowsType(); }
        }

        /// <summary>
        /// The state of host node.
        /// </summary>
        public ElementState State
        {
            get { return node.State; }
        }

        /// <summary>
        /// Returns whether this port has a default value that can be used.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool DefaultValueEnabled
        {
            get { return port.DefaultValue != null; }
        }
        
        /// <summary>
        /// Returns whether the port is using its default value, or whether this been disabled
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool UsingDefaultValue
        {
            get { return port.UsingDefaultValue; }
            set
            {
                port.UsingDefaultValue = value;
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
            get { return node.WorkspaceViewModel.FirstActiveConnector != null; }
        }

        /// <summary>
        /// The margin thickness of port view.
        /// </summary>
        public System.Windows.Thickness MarginThickness
        {
            get { return port.MarginThickness.AsWindowsType(); }
        }

        public PortEventType EventType { get; set; }

        /// <summary>
        /// If should display Use Levels popup menu. 
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool ShowUseLevelMenu
        {
            get
            {
                return showUseLevelMenu;
            }
            set
            {
                showUseLevelMenu = value;
                RaisePropertyChanged(nameof(ShowUseLevelMenu));
            }
        }

        /// <summary>
        /// If UseLevel is enabled on this port.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool UseLevels
        {
            get { return port.UseLevels; }
        }

        /// <summary>
        /// If should keep list structure on this port.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool ShouldKeepListStructure
        {
            get { return port.KeepListStructure; }
        }

        /// <summary>
        /// Levle of list.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public int Level
        {
            get { return port.Level; }
            set
            {
                ChangeLevel(value);
            }
        }

        /// <summary>
        /// The visibility of Use Levels menu.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public Visibility UseLevelVisibility
        {
            get
            {
                if (node.ArgumentLacing != LacingStrategy.Disabled)
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
            get => node;
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
            this.node = node;
            this.port = port;

            this.port.PropertyChanged += PortPropertyChanged;
            this.node.PropertyChanged += NodePropertyChanged;
            this.node.WorkspaceViewModel.PropertyChanged += WorkspacePropertyChanged;

            RefreshPortColors();
        }

        public override void Dispose()
        {
            port.PropertyChanged -= PortPropertyChanged;
            node.PropertyChanged -= NodePropertyChanged;
            node.WorkspaceViewModel.PropertyChanged -= WorkspacePropertyChanged;
        }

        internal virtual PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            portModel.IsProxyPort = true;
            return new PortViewModel(node, portModel);
        }

        /// <summary>
        /// Sets up the node autocomplete window to be placed relative to the node.
        /// </summary>
        /// <param name="popup">Node autocomplete popup.</param>
        internal void SetupNodeAutocompleteWindowPlacement(Popup popup)
        {
            node.OnRequestAutoCompletePopupPlacementTarget(popup);
            popup.CustomPopupPlacementCallback = PlaceAutocompletePopup;
        }

        /// <summary>
        /// Sets up the PortContextMenu window to be placed relative to the port.
        /// </summary>
        /// <param name="popup">Node context menu popup.</param>
        internal void SetupPortContextMenuPlacement(Popup popup)
        {
            node.OnRequestPortContextMenuPlacementTarget(popup);
            popup.CustomPopupPlacementCallback = PlacePortContextMenu;
        }

        private CustomPopupPlacement[] PlaceAutocompletePopup(Size popupSize, Size targetSize, Point offset)
        {
            var zoom = node.WorkspaceViewModel.Zoom;

            double x;
            var scaledSpacing = autocompletePopupSpacing * targetSize.Width / node.ActualWidth;
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
            var scaledHeight = targetSize.Height / node.ActualHeight;
            var absoluteHeight = NodeModel.HeaderHeight + (PortModel.Index * PortModel.Height);
            var y = absoluteHeight * scaledHeight;

            var placement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None);

            return new[] { placement };
        }

        private CustomPopupPlacement[] PlacePortContextMenu(Size popupSize, Size targetSize, Point offset)
        {
            // The actual zoom here is confusing
            // What matters is the zoom factor measured from the scaled : unscaled node size
            var zoom = node.WorkspaceViewModel.Zoom;

            double x;
            var scaledWidth = autocompletePopupSpacing * targetSize.Width / node.ActualWidth;

            if (PortModel.PortType == PortType.Input)
            {
                // Offset popup to the left by its width from left edge of node and spacing.
                x = -scaledWidth - popupSize.Width;
            }
            else
            {
                // Offset popup to the right by node width and spacing from left edge of node.
                x = scaledWidth + targetSize.Width;
            }
            // Important - while zooming in and out, Node elements are scaled, while popup is not
            // Calculate absolute popup halfheight to deduct from the overal y pos
            // Then add the header, port height and port index position
            var popupHeightOffset = - popupSize.Height * 0.5;
            var headerHeightOffset = 2 * NodeModel.HeaderHeight * zoom;
            var portHalfHeight = PortModel.Height * 0.5 * zoom;
            var rowOffset = PortModel.Index * (1.5 * PortModel.Height) * zoom;

            var y = popupHeightOffset + headerHeightOffset + portHalfHeight + rowOffset;

            var placement = new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None);

            return new[] { placement };
        }

        private void WorkspacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ActiveConnector":
                    RaisePropertyChanged(nameof(IsHitTestVisible));
                    break;
                default:
                    break;
            }
        }

        private void NodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsSelected):
                    RaisePropertyChanged(nameof(IsSelected));
                    break;
                case nameof(State):
                    RaisePropertyChanged(nameof(State));
                    RefreshPortColors();
                    break;
                case nameof(ToolTipContent):
                    RaisePropertyChanged(nameof(ToolTipContent));
                    break;
                case nameof(node.IsVisible):
                    RefreshPortColors();
                    break;
                case nameof(node.NodeModel.CachedValue):
                    RefreshPortColors();
                    break;
            }
        }

        private void PortPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ToolTip":
                    RaisePropertyChanged(nameof(ToolTipContent));
                    break;
                case nameof(PortType):
                    RaisePropertyChanged(nameof(PortType));
                    break;
                case nameof(PortName):
                    RaisePropertyChanged(nameof(PortName));
                    break;
                case nameof(IsConnected):
                    RaisePropertyChanged(nameof(IsConnected));
                    RefreshPortColors();
                    break;
                case nameof(IsEnabled):
                    RaisePropertyChanged(nameof(IsEnabled));
                    break;
                case nameof(Center):
                    RaisePropertyChanged(nameof(Center));
                    break;
                case nameof(MarginThickness):
                    RaisePropertyChanged(nameof(MarginThickness));
                    break;
                case nameof(UsingDefaultValue):
                    RefreshPortColors();
                    break;
            }
        }

        /// <summary>
        /// UseLevels command
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public DelegateCommand UseLevelsCommand
        {
            get
            {
                if (useLevelsCommand == null)
                {
                    useLevelsCommand = new DelegateCommand(null, p => true);
                }
                return useLevelsCommand;
            }
        }

        /// <summary>
        /// ShouldKeepListStructure command
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public DelegateCommand KeepListStructureCommand
        {
            get
            {
                if (keepListStructureCommand == null)
                {
                    keepListStructureCommand = new DelegateCommand(null, p => true);
                }
                return keepListStructureCommand;
            }
        }

        //Todo remove in 2.13
        private void ChangeLevel(int level)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, node.NodeLogic.GUID, "ChangeLevel", string.Format("{0}:{1}", port.Index, level));

            node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }
        
        private void Connect(object parameter)
        {
            DynamoViewModel dynamoViewModel = this.node.DynamoViewModel;
            WorkspaceViewModel workspaceViewModel = dynamoViewModel.CurrentSpaceViewModel;
            workspaceViewModel.HandlePortClicked(this);
        }

        protected bool CanConnect(object parameter)
        {
            return true;
        }

        // Handler to invoke node Auto Complete
        private void AutoComplete(object parameter)
        {
            var wsViewModel = node.WorkspaceViewModel;
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

        private void NodePortContextMenu(object obj)
        {
            // If this port does not display a Chevron button to open the context menu and it doesn't
            // have a default value then using right-click to open the context menu should also do nothing.
            // Added check for Python node model (allow input context menu for rename)
            if (obj is InPortViewModel inPortViewModel &&
                inPortViewModel.UseLevelVisibility == Visibility.Collapsed &&
                !inPortViewModel.DefaultValueEnabled &&
                !(inPortViewModel.NodeViewModel.NodeModel is PythonNodeModels.PythonNode)) return;
            
            var wsViewModel = node.WorkspaceViewModel;
            
            wsViewModel.CancelActiveState();
            wsViewModel.OnRequestPortContextMenu(ShowHideFlags.Show, this);
        }

        private bool CanAutoComplete(object parameter)
        {
            DynamoViewModel dynamoViewModel = node.DynamoViewModel;
            // If user trying to trigger Node AutoComplete from proxy ports, display notification
            // telling user it is not available that way
            if (port.IsProxyPort)
            {
                dynamoViewModel.MainGuideManager.CreateRealTimeInfoWindow(Wpf.Properties.Resources.NodeAutoCompleteNotAvailableForCollapsedGroups);
            }
            // If the feature is enabled from Dynamo experiment setting and if user interaction is not on proxy ports.
            return dynamoViewModel.EnableNodeAutoComplete && !port.IsProxyPort;
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
        protected virtual void RefreshPortColors()
        {
            PortBackgroundColor = PortBackgroundColorDefault;
            PortBorderBrushColor = PortBorderBrushColorDefault;
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
            if (node.ArgumentLacing != LacingStrategy.Disabled)
            {
                switch (port.PortType)
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

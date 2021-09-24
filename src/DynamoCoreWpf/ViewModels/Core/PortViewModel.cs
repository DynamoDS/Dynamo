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
    public partial class PortViewModel : ViewModelBase
    {
        #region Properties/Fields

        protected readonly PortModel _port;
        protected readonly NodeViewModel _node;
        private DelegateCommand _useLevelsCommand;
        private DelegateCommand _keepListStructureCommand;
        private bool _showUseLevelMenu;
        private const double autocompletePopupSpacing = 2.5;
        internal bool inputPortDisconnectedByConnectCommand = false;
        protected static SolidColorBrush portBorderBrushColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
        protected static SolidColorBrush portBackgroundColor = new SolidColorBrush(Color.FromArgb(0, 60, 60, 60));

        protected static readonly SolidColorBrush PortBackgroundColorDefault = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        protected static readonly SolidColorBrush PortBorderBrushColorDefault = new SolidColorBrush(Color.FromRgb(161, 161, 161));

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
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValue != null; }
        }
        
        /// <summary>
        /// Returns whether the port is using its default value, or whether this been disabled
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set
            {
                _port.UsingDefaultValue = value;
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
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
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
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        public bool UseLevels
        {
            get { return _port.UseLevels; }
        }

        /// <summary>
        /// If should keep list structure on this port.
        /// </summary>
        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
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
        }

        public override void Dispose()
        {
            _port.PropertyChanged -= _port_PropertyChanged;
            _node.PropertyChanged -= _node_PropertyChanged;
            _node.WorkspaceViewModel.PropertyChanged -= Workspace_PropertyChanged;
        }

        internal virtual PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            throw new Exception("Don't do this");
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
                    RefreshPortColors();
                    break;
                case "IsEnabled":
                    RaisePropertyChanged("IsEnabled");
                    break;
                case "Center":
                    RaisePropertyChanged("Center");
                    break;
                case "MarginThickness":
                    RaisePropertyChanged("MarginThickness");
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
                if (_useLevelsCommand == null)
                {
                    _useLevelsCommand = new DelegateCommand(null, p => true);
                }
                return _useLevelsCommand;
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
                if (_keepListStructureCommand == null)
                {
                    _keepListStructureCommand = new DelegateCommand(null, p => true);
                }
                return _keepListStructureCommand;
            }
        }

        [Obsolete("This method will be removed in Dynamo 3.0 - please use the InPortViewModel")]
        private void ChangeLevel(int level)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, _node.NodeLogic.GUID, "ChangeLevel", string.Format("{0}:{1}", _port.Index, level));

            _node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        private void Connect(object parameter)
        {
            DynamoViewModel dynamoViewModel = this._node.DynamoViewModel;
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

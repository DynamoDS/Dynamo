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
    public partial class InPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand _useLevelsCommand;
        private DelegateCommand _keepListStructureCommand;
        private DelegateCommand portMouseLeftButtonOnLevelCommand;

        private bool _showUseLevelMenu;

        private static SolidColorBrush portValueMarkerColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
        private static SolidColorBrush PortValueMarkerBlue = new SolidColorBrush(Color.FromRgb(106, 192, 231));
        private static SolidColorBrush PortValueMarkerRed = new SolidColorBrush(Color.FromRgb(235, 85, 85));

        private static readonly SolidColorBrush PortBackgroundColorKeepListStructure = new SolidColorBrush(Color.FromRgb(83, 126, 145));
        private static readonly SolidColorBrush PortBorderBrushColorKeepListStructure = new SolidColorBrush(Color.FromRgb(168, 181, 187));

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
            }
        }

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

        // <summary>
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

        /// <summary>
        /// Sets the color of the use levels popup in the input port context menu.
        /// This changes when the Keep List Structure option is activated and the port
        /// is connected, upon which it turns blue.
        /// </summary>
        public SolidColorBrush UseLevelsMenuColor
        {
            get
            {
                return ShouldKeepListStructure && _port.IsConnected
                    ? new SolidColorBrush(Color.FromArgb(255, 60, 60, 60))
                    : new SolidColorBrush(Color.FromArgb(255, 83, 83, 83));
            }
        }

        #endregion

        public InPortViewModel(NodeViewModel node, PortModel port) : base(node, port)
        {
            _port.PropertyChanged += _port_PropertyChanged;
        }

        public override void Dispose()
        {
            _port.PropertyChanged -= _port_PropertyChanged;
            base.Dispose();
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            return new InPortViewModel(_node, portModel);
        }

        void _port_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DefaultValue":
                    RaisePropertyChanged("DefaultValue");
                    break;
                case "UsingDefaultValue":
                    RaisePropertyChanged("UsingDefaultValue");
                    RefreshPortColors();
                    break;
                case "UseLevels":
                    RaisePropertyChanged("UseLevels");
                    RaisePropertyChanged(nameof(UseLevelsMenuColor));
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

        public DelegateCommand MouseLeftButtonDownOnLevelCommand
        {
            get
            {
                if (portMouseLeftButtonOnLevelCommand == null)
                    portMouseLeftButtonOnLevelCommand = new DelegateCommand(OnMouseLeftButtonDownOnLevel, CanConnect);

                return portMouseLeftButtonOnLevelCommand;
            }
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
        /// Handles the logic for updating the PortBackgroundColor and PortBackgroundBrushColor
        /// </summary>
        protected override void RefreshPortColors()
        {
            // The visual appearance of ports can be affected by many different logical states
            // Inputs have more display styles than outputs

            // Special case for keeping list structure visual appearance
            if (_port.UseLevels && _port.KeepListStructure && _port.IsConnected)
            {
                PortValueMarkerColor = PortValueMarkerBlue;
                PortBackgroundColor = PortBackgroundColorKeepListStructure;
                PortBorderBrushColor = PortBorderBrushColorKeepListStructure;
            }
            // Port has a default value, shows blue marker
            else if (UsingDefaultValue && DefaultValueEnabled)
            {
                PortValueMarkerColor = PortValueMarkerBlue;
                PortBackgroundColor = PortBackgroundColorDefault;
                PortBorderBrushColor = PortBorderBrushColorDefault;
            }
            // Port isn't connected and has no default value (or isn't using it)
            else
            {
                PortValueMarkerColor = !_port.IsConnected ? PortValueMarkerRed : PortValueMarkerBlue;
                PortBackgroundColor = PortBackgroundColorDefault;
                PortBorderBrushColor = PortBorderBrushColorDefault;
            }

            RaisePropertyChanged(nameof(UseLevelsMenuColor));
        }
    }
}

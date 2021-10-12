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

        private DelegateCommand useLevelsCommand;
        private DelegateCommand keepListStructureCommand;
        private DelegateCommand portMouseLeftButtonOnLevelCommand;

        private bool showUseLevelMenu;

        private SolidColorBrush portValueMarkerColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));
        private static SolidColorBrush PortValueMarkerBlue = new SolidColorBrush(Color.FromRgb(106, 192, 231));
        private static SolidColorBrush PortValueMarkerRed = new SolidColorBrush(Color.FromRgb(235, 85, 85));

        private static readonly SolidColorBrush PortBackgroundColorKeepListStructure = new SolidColorBrush(Color.FromRgb(83, 126, 145));
        private static readonly SolidColorBrush PortBorderBrushColorKeepListStructure = new SolidColorBrush(Color.FromRgb(168, 181, 187));

        /// <summary>
        /// Returns whether this port has a default value that can be used.
        /// </summary>
        public bool DefaultValueEnabled => port.DefaultValue != null;

        /// <summary>
        /// Returns whether the port is using its default value, or whether this been disabled
        /// </summary>
        public bool UsingDefaultValue
        {
            get => port.UsingDefaultValue;
            set
            {
                port.UsingDefaultValue = value;
            }
        }

        /// <summary>
        /// If should display Use Levels popup menu. 
        /// </summary>
        public bool ShowUseLevelMenu
        {
            get => showUseLevelMenu;
            set
            {
                showUseLevelMenu = value;
                RaisePropertyChanged(nameof(ShowUseLevelMenu));
            }
        }

        /// <summary>
        /// If UseLevel is enabled on this port.
        /// </summary>
        public bool UseLevels => port.UseLevels;

        /// <summary>
        /// Determines whether or not the UseLevelsSpinner is visible on the port.
        /// </summary>
        public Visibility UseLevelSpinnerVisible
        {
            get
            {
                if (PortType == PortType.Output)
                {
                    return Visibility.Collapsed;
                }

                if (UseLevels)
                {
                    return Visibility.Visible;
                }

                return Visibility.Hidden;
            }
        }

        /// <summary>
        /// If should keep list structure on this port.
        /// </summary>
        public bool ShouldKeepListStructure => port.KeepListStructure;

        /// <summary>
        /// Levle of list.
        /// </summary>
        public int Level
        {
            get => port.Level;
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
                if (node.ArgumentLacing != LacingStrategy.Disabled)
                {
                    return Visibility.Visible;
                }

                return Visibility.Collapsed;
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

        #endregion

        public InPortViewModel(NodeViewModel node, PortModel port) : base(node, port)
        {
            port.PropertyChanged += PortPropertyChanged;
        }

        public override void Dispose()
        {
            port.PropertyChanged -= PortPropertyChanged;
            base.Dispose();
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            return new InPortViewModel(node, portModel);
        }

        private void PortPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(port.DefaultValue):
                    RaisePropertyChanged(nameof(port.DefaultValue));
                    break;
                case nameof(UsingDefaultValue):
                    RaisePropertyChanged(nameof(UsingDefaultValue));
                    RefreshPortColors();
                    break;
                case nameof(UseLevels):
                    RaisePropertyChanged(nameof(UseLevels));
                    break;
                case nameof(Level):
                    RaisePropertyChanged(nameof(Level));
                    break;
                case nameof(KeepListStructure):
                    RaisePropertyChanged(nameof(ShouldKeepListStructure));
                    RefreshPortColors();
                    break;
            }
        }

        /// <summary>
        /// UseLevels command
        /// </summary>
        public DelegateCommand UseLevelsCommand
        {
            get { return useLevelsCommand ?? (useLevelsCommand = new DelegateCommand(UseLevel, p => true)); }
        }

        private void UseLevel(object parameter)
        {
            var useLevel = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, node.NodeLogic.GUID, "UseLevels", string.Format("{0}:{1}", port.Index, useLevel));
            
            node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
            RaisePropertyChanged(nameof(UseLevelSpinnerVisible));
        }

        /// <summary>
        /// ShouldKeepListStructure command
        /// </summary>
        public DelegateCommand KeepListStructureCommand
        {
            get
            {
                return keepListStructureCommand ??
                       (keepListStructureCommand = new DelegateCommand(KeepListStructure, p => true));
            }
        }

        private void KeepListStructure(object parameter)
        {
            var keepListStructure = (bool)parameter;
            var command = new DynamoModel.UpdateModelValueCommand(
                Guid.Empty, node.NodeLogic.GUID, "KeepListStructure", string.Format("{0}:{1}", port.Index, keepListStructure));
            
            node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        private void ChangeLevel(int level)
        {
            var command = new DynamoModel.UpdateModelValueCommand(
                            Guid.Empty, node.NodeLogic.GUID, "ChangeLevel", string.Format("{0}:{1}", port.Index, level));

            node.WorkspaceViewModel.DynamoViewModel.ExecuteCommand(command);
        }

        /// <summary>
        /// Handles the Mouse left button click on the node levels context menu button.
        /// </summary>
        public DelegateCommand MouseLeftButtonDownOnLevelCommand
        {
            get
            {
                return portMouseLeftButtonOnLevelCommand ?? (portMouseLeftButtonOnLevelCommand =
                    new DelegateCommand(OnMouseLeftButtonDownOnLevel, CanConnect));
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
            // Special case for keeping list structure visual appearance
            if (port.UseLevels && port.KeepListStructure && port.IsConnected)
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
                PortValueMarkerColor = port.IsConnected ? PortValueMarkerBlue : PortValueMarkerRed;
                PortBackgroundColor = PortBackgroundColorDefault;
                PortBorderBrushColor = PortBorderBrushColorDefault;
            }
        }
    }
}

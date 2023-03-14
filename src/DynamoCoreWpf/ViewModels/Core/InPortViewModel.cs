using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.UI;
using Dynamo.UI.Commands;
using ProtoCore.Utils;
using UI.Prompts;

namespace Dynamo.ViewModels
{
    public partial class InPortViewModel : PortViewModel
    {
        #region Properties/Fields

        private DelegateCommand useLevelsCommand;
        private DelegateCommand keepListStructureCommand;
        private DelegateCommand portMouseLeftButtonOnLevelCommand;
        private DelegateCommand editPortPropertiesCommand;

        private bool showUseLevelMenu;
        private bool isPythonNodePort;

        private SolidColorBrush portValueMarkerColor = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

        private bool portDefaultValueMarkerVisible;
        private bool isFunctionNode;

        internal static SolidColorBrush PortValueMarkerBlue = new SolidColorBrush(Color.FromRgb(106, 192, 231));
        internal static SolidColorBrush PortValueMarkerRed = new SolidColorBrush(Color.FromRgb(235, 85, 85));
        private static SolidColorBrush PortValueMarkerGrey = new SolidColorBrush(Color.FromRgb(153, 153, 153));

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
        /// Returns whether the port belongs to a Python node
        /// </summary>
        public bool IsPythonNodePort
        {
            get => isPythonNodePort;
            set
            {
                isPythonNodePort = value;
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

        /// <summary>
        /// Determines whether the blue circular marker to the left of this port is visible.
        /// This indicates whether the port has a default value which is in use.
        /// </summary>
        public bool PortDefaultValueMarkerVisible
        {
            get => portDefaultValueMarkerVisible;
            set
            {
                portDefaultValueMarkerVisible = value;
                RaisePropertyChanged(nameof(PortDefaultValueMarkerVisible));
            }
        }

        #endregion

        public InPortViewModel(NodeViewModel node, PortModel port) : base(node, port)
        {
            port.PropertyChanged += PortPropertyChanged;
            node.NodeModel.PropertyChanged += NodeModel_PropertyChanged;

            RefreshPortDefaultValueMarkerVisible();
            IsPythonNodePort = node.NodeModel is PythonNodeModels.PythonNode;
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

        public override void Dispose()
        {
            port.PropertyChanged -= PortPropertyChanged;
            node.NodeModel.PropertyChanged -= NodeModel_PropertyChanged;

            base.Dispose();
        }

        internal override PortViewModel CreateProxyPortViewModel(PortModel portModel)
        {
            portModel.IsProxyPort = true;
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
                    RefreshPortDefaultValueMarkerVisible();
                    break;
                case nameof(DefaultValueEnabled):
                    RefreshPortDefaultValueMarkerVisible();
                    break;
                case nameof(UseLevels):
                    RaisePropertyChanged(nameof(UseLevels));
                    break;
                case nameof(Level):
                    RaisePropertyChanged(nameof(Level));
                    break;
                case nameof(KeepListStructure):
                    RaisePropertyChanged(nameof(ShouldKeepListStructure));
                    break;
                case nameof(IsConnected):
                    RaisePropertyChanged(nameof(IsConnected));
                    RefreshAllInportsColors();
                    break;
                case nameof(IsPythonNodePort):
                    RaisePropertyChanged(nameof(IsPythonNodePort));
                    break;
            }
        }

        private void RefreshAllInportsColors()
        {
            foreach (InPortViewModel port in node.InPorts)
            {
                port.RefreshInputColors();
            }
        }

        internal void RefreshInputColors()
        {
            RefreshPortColors();
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
                OutPortNames = ListOutportNames(this.NodeViewModel.InPorts),
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
        /// Handles the Mouse left button down on the level.
        /// </summary>
        /// <param name="parameter"></param>
        private void OnMouseLeftButtonDownOnLevel(object parameter)
        {
            ShowUseLevelMenu = true;
        }

        private void RefreshPortDefaultValueMarkerVisible()
        {
            PortDefaultValueMarkerVisible = UsingDefaultValue && DefaultValueEnabled;
        }

        /// <summary>
        /// Handles the logic for updating the PortBackgroundColor and PortBackgroundBrushColor
        /// </summary>
        protected override void RefreshPortColors()
        {
            //This variable checks if the node is a function class
            var isCachedValueNull = node.NodeModel.CachedValue == null || node.NodeModel.CachedValue.Data == null;
            isFunctionNode = isCachedValueNull && node.NodeModel.IsPartiallyApplied 
                || !isCachedValueNull && node.NodeModel.CachedValue.IsFunction;

            if (isFunctionNode)
            {
                if (node.NodeModel.AreAllOutputsConnected && node.NodeModel.AreAllInputsDisconnected)
                {
                    PortValueMarkerColor = PortValueMarkerGrey;
                    PortBackgroundColor = PortBackgroundColorDefault;
                    PortBorderBrushColor = PortBorderBrushColorDefault;
                }
                else
                {
                    SetupDefaultPortColorValues();
                }
            }
            else
            {
                SetupDefaultPortColorValues();
            }
        }

        internal void RefreshInputPortsByOutputConnectionChanged(bool isOutputConnected)
        {
            if (node.NodeModel.AreAllInputsDisconnected && isOutputConnected)
            {
                PortValueMarkerColor = PortValueMarkerGrey;
            }
            else
            {
                SetupDefaultPortColorValues();
            }
        }

        private void SetupDefaultPortColorValues()
        {
            
            // Special case for keeping list structure visual appearance
            if (port.UseLevels && port.KeepListStructure && port.IsConnected)
            {
                PortValueMarkerColor = PortValueMarkerBlue;
                PortBackgroundColor = PortBackgroundColorKeepListStructure;
                PortBorderBrushColor = PortBorderBrushColorKeepListStructure;
            }
            // Port has a default value, shows blue marker
            else if ((UsingDefaultValue && DefaultValueEnabled) || !isFunctionNode && !node.IsWatchNode)
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

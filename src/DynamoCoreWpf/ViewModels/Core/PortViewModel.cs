using System;
using System.Windows;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
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
            get { return _port.Name; }
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
            get { return _port.Owner.InPorts[_port.Index].IsConnected; }
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
        /// If should use default value on this port.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValue != null; }
        }

        /// <summary>
        /// If default value is being used on this port.
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set { _port.UsingDefaultValue = value; }
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

        private bool _showUseLevelMenu;

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
                if (_node.ArgumentLacing != LacingStrategy.Disabled && PortType == PortType.Input)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
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
        }

        public override void Dispose()
        {
            _port.PropertyChanged -= _port_PropertyChanged;
            _node.PropertyChanged -= _node_PropertyChanged;
            _node.WorkspaceViewModel.PropertyChanged -= Workspace_PropertyChanged;
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
                    RaisePropertyChanged("IsConnected");
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

        /// <summary>
        /// Handles the Mouse enter event on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseEnter(object parameter)
        {
            if (MouseEnter != null)
                MouseEnter(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse leave on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeave(object parameter)
        {
            if (MouseLeave != null)
                MouseLeave(parameter, null);
        }

        /// <summary>
        /// Handles the Mouse left button down on the port
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OnRectangleMouseLeftButtonDown(object parameter)
        {
            if (MouseLeftButtonDown != null)
                MouseLeftButtonDown(parameter, null);
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

       
    }
}

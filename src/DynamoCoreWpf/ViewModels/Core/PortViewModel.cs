﻿using System;
using System.Windows;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.ViewModels
{
    public partial class PortViewModel : ViewModelBase
    {

        #region Properties/Fields

        private readonly PortModel _port;
        private readonly NodeViewModel _node;

        public PortModel PortModel
        {
            get { return _port; }
        }

        public string ToolTipContent
        {
            get { return _port.ToolTipContent; }
        }

        public string PortName
        {
            get { return _port.PortName; }
        }

        public PortType PortType
        {
            get { return _port.PortType; }
        }
        
        public bool IsSelected
        {
            get { return _node.IsSelected; }
        }

        public bool IsConnected
        {
            get { return _port.IsConnected; }
        }

        public bool IsEnabled
        {
            get { return _port.IsEnabled; }
        }

        public double Height
        {
            get { return _port.Height; }
        }

        public Point Center
        {
            get { return _port.Center.AsWindowsType(); }
        }

        public ElementState State
        {
            get { return _node.State; }    
        }

        public bool DefaultValueEnabled
        {
            get { return _port.DefaultValueEnabled; }
        }

        public bool UsingDefaultValue
        {
            get { return _port.UsingDefaultValue; }
            set { _port.UsingDefaultValue = value; }
        }

        /// <summary>
        /// IsHitTestVisible property gets a value that declares whether 
        /// a Snapping rectangle can possibly be returned as a hit test result.
        /// When ActiveConnector is not null, Snapping rectangle handles click events.
        /// When ActiveConnector is null, Snapping rectangle does not handle click events 
        /// and user can "click though invisible snapping area".
        /// </summary>
        public bool IsHitTestVisible
        {
            get { return _node.WorkspaceViewModel.ActiveConnector != null; }
        }

        public System.Windows.Thickness MarginThickness
        {
            get { return _port.MarginThickness.AsWindowsType(); }
        }

        public PortEventType EventType { get; set; }

        private bool _showUseLevelMenu;
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
                case "ToolTipContent":
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
                case "DefaultValueEnabled":
                    RaisePropertyChanged("DefaultValueEnabled");
                    break;
                case "UsingDefaultValue":
                    RaisePropertyChanged("UsingDefaultValue");
                    break;
                case "MarginThickness":
                    RaisePropertyChanged("MarginThickness");
                    break;
            }
            
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

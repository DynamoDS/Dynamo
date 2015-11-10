﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Graph.Connectors;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Interaction logic for dynPort.xaml
    /// </summary>
    public enum PortType { Input, Output };

    public class PortModel : ModelBase
    {
        #region events

        /// <summary>
        /// Event triggered when a port is connected.
        /// </summary>
        public event Action<PortModel, ConnectorModel> PortConnected;

        /// <summary>
        /// Event triggered when a port is disconnected.
        /// </summary>
        public event Action<PortModel> PortDisconnected;

        #endregion

        #region private fields
        ObservableCollection<ConnectorModel> connectors = new ObservableCollection<ConnectorModel>();
        private bool usingDefaultValue;
        private PortData portData;
        #endregion

        #region public members

        public ObservableCollection<ConnectorModel> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        public string PortName
        {
            get { return portData.NickName; }
        }

        public string ToolTipContent 
        {
            get 
            {
                string useDefaultArgument = string.Empty;
                if (!UsingDefaultValue && DefaultValueEnabled)
                    useDefaultArgument = " " + Properties.Resources.DefaultValueDisabled;
                return portData.ToolTipString + useDefaultArgument; 
            }
        }

        public PortType PortType
        {
            get; private set;
        }

        public NodeModel Owner
        {
            get; private set;
        }

        public int Index
        {
            get { return Owner.GetPortModelIndex(this); }
        }

        public int LineIndex
        {
            get { return portData.LineIndex; }
        }

        /// <summary>
        /// A flag indicating whether the port is considered connected.
        /// </summary>
        /// 
        [Obsolete("Please use NodeModel.HasConnectedInput instead.")]
        public bool IsConnected
        {
            get; private set;
        }

        /// <summary>
        /// Center is used by connected connectors to update their shape
        /// The "center" of a port is derived from the type of port and
        /// offsets from the node origin based on the port's index in the 
        /// ports collection.
        /// </summary>
        public Point2D Center
        {
            get
            {
                double halfHeight = Height * 0.5;
                const double headerHeight = 25;

                double offset = Owner.GetPortVerticalOffset(this);
                double y = Owner.Y + headerHeight + 5 + halfHeight + offset;

                switch (PortType)
                {
                    case PortType.Input:
                        return new Point2D(Owner.X, y);
                    case PortType.Output:
                        return new Point2D(Owner.X + Owner.Width, y);
                }

                return new Point2D();
            }
        }

        /// <summary>
        /// Controls whether this port is set to use it's default value (true) or yield a closure (false).
        /// </summary>
        public bool UsingDefaultValue
        {
            get { return usingDefaultValue; }
            set
            {
                usingDefaultValue = value; 
                RaisePropertyChanged("UsingDefaultValue");
                RaisePropertyChanged("ToolTipContent");
            }
        }

        /// <summary>
        /// Controls whether the Use Default Value option is available.
        /// </summary>
        public bool DefaultValueEnabled
        {
            get { return portData.DefaultValue != null; }
        }

        /// <summary>
        /// Controls the space between successive output ports
        /// </summary>
        public Thickness MarginThickness
        {
            get; private set;
        }

        public SnapExtensionEdges extensionEdges { get; set; }        
    
        #endregion

        public PortModel(PortType portType, NodeModel owner, PortData data)
        {
            IsConnected = false;
            PortType = portType;
            Owner = owner;

            SetPortData(data);

            MarginThickness = new Thickness(0);
            Height = Math.Abs(data.Height) < 0.001 ? Configurations.PortHeightInPixels : data.Height;
        }

        public void SetPortData(PortData data)
        {
            portData = data;

            UsingDefaultValue = portData.DefaultValue != null;
            RaisePropertyChanged("DefaultValueEnabled");
            RaisePropertyChanged("PortName");
            RaisePropertyChanged("TooltipContent");
        }

        /// <summary>
        /// Deletes all connectors attached to this PortModel.
        /// </summary>
        public void DestroyConnectors()
        {
            if (Owner == null)
                return;

            while (Connectors.Any())
            {
                ConnectorModel connector = Connectors[0];
                connector.Delete();
            }
        }

        public void Connect(ConnectorModel connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(connector);

            IsConnected = true;
        }

        public void Disconnect(ConnectorModel connector, bool silent = false)
        {
            if (!connectors.Contains(connector))
                return;

            //throw the event for a disconnection
            if (!silent)
            {
                OnPortDisconnected();
            }

            connectors.Remove(connector);

            if (connectors.Count == 0)
            {
                IsConnected = false;
            }

            Owner.ValidateConnections();
        }

        /// <summary>
        /// Called when a port is connected.
        /// </summary>
        /// <param name="connector"></param>
        protected virtual void OnPortConnected(ConnectorModel connector)
        {
            if (PortConnected != null)
                PortConnected(this, connector);
        }

        /// <summary>
        /// Called when a port is disconnected.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPortDisconnected()
        {
            if (PortDisconnected != null)
                PortDisconnected(this);
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            // We are not deserializing the ports.
            throw new NotImplementedException();
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            // We are not deserializing the ports.
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PortData
    {
        public string NickName { get; set; }
        public string ToolTipString { get; set; }
        public AssociativeNode DefaultValue { get; set; }
        public int LineIndex { get; set; }

        public double Height { get; set; }

        public PortData(string nickName, string tip) : this(nickName, tip, null) { }

        public PortData(string nickName, string toolTipString, AssociativeNode defaultValue)
        {
            NickName = nickName;
            ToolTipString = toolTipString;
            DefaultValue = defaultValue;
            LineIndex = -1;
            Height = 0;
        }
    }
}

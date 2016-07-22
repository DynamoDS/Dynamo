using System;
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

    /// <summary>
    /// PortModel represents Dynamo ports.
    /// </summary>
    public class PortModel : ModelBase
    {
        #region private fields
        ObservableCollection<ConnectorModel> connectors = new ObservableCollection<ConnectorModel>();
        private bool usingDefaultValue;
        private PortData portData;
        private bool isEnabled = true;
        private bool useLevels = false;
        private bool shouldKeepListStructure = false;
        private int level = 1;
        #endregion

        #region public members

        /// <summary>
        /// Returns the connectors between the specified ports.
        /// </summary>
        public ObservableCollection<ConnectorModel> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        /// <summary>
        /// Name of the port.
        /// </summary>
        public string PortName
        {
            get { return portData.NickName; }
        }

        /// <summary>
        /// Tooltip of the port.
        /// </summary>
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

        /// <summary>
        /// Type of the port.
        /// It can be incoming or outcoming.
        /// </summary>
        public PortType PortType
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the Node.
        /// </summary>
        public NodeModel Owner
        {
            get;
            private set;
        }

        /// <summary>
        /// Index of the port.
        /// </summary>
        public int Index
        {
            get { return Owner.GetPortModelIndex(this); }
        }

        /// <summary>
        /// Returns the LineIndex of that port. The vertical position of PortModel is dependent on LineIndex.
        /// </summary>
        public int LineIndex
        {
            get { return portData.LineIndex; }
        }

        /// <summary>
        /// A flag indicating whether the port is considered connected.
        /// </summary>
        [Obsolete("Please use NodeModel.HasConnectedInput instead.")]
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the port is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
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
        /// Default value for port.
        /// </summary>
        public AssociativeNode DefaultValue
        {
            get { return portData.DefaultValue; }
        }

        /// <summary>
        /// Controls the space between successive output ports
        /// </summary>
        public Thickness MarginThickness
        {
            get;
            private set;
        }

        /// <summary>
        /// Based on extensionEdges port is aligned in UI.
        /// </summary>
        public SnapExtensionEdges extensionEdges { get; set; }

        /// <summary>
        /// List at level.
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                if (level != value)
                {
                    level = value;
                    RaisePropertyChanged("Level");
                }
            }
        }

        /// <summary>
        /// If use level is enabled.
        /// </summary>
        public bool UseLevels
        {
            get
            {
                return useLevels;
            }
            set
            {
                if (useLevels != value)
                {
                    useLevels = value;
                    RaisePropertyChanged("UseLevels");
                }
            }
        }

        /// <summary>
        /// If needs to keep list structure.
        /// </summary>
        public bool ShouldKeepListStructure
        {
            get
            {
                return shouldKeepListStructure;
            }
            set
            {
                if (shouldKeepListStructure != value)
                {
                    shouldKeepListStructure = value;
                    RaisePropertyChanged("ShouldKeepListStructure");
                }
            }
        }

        #endregion

        /// <summary>
        /// Creates PortModel.
        /// </summary>
        /// <param name="portType">Type of the Port</param>
        /// <param name="owner">Parent Node</param>
        /// <param name="data">Information about port</param>
        public PortModel(PortType portType, NodeModel owner, PortData data)
        {
            IsConnected = false;
            PortType = portType;
            Owner = owner;
            UseLevels = false;
            ShouldKeepListStructure = false;
            Level = 2;

            SetPortData(data);

            MarginThickness = new Thickness(0);
            Height = Math.Abs(data.Height) < 0.001 ? Configurations.PortHeightInPixels : data.Height;
        }

        /// <summary>
        /// Sets the port data.
        /// </summary>
        /// <param name="data">The data.</param>
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
        internal void DestroyConnectors()
        {
            if (Owner == null)
                return;

            while (Connectors.Any())
            {
                ConnectorModel connector = Connectors[0];
                connector.Delete();
            }
        }

        internal void Connect(ConnectorModel connector)
        {
            connectors.Add(connector);

            //throw the event for a connection
            OnPortConnected(connector);

            IsConnected = true;
        }

        internal void Disconnect(ConnectorModel connector, bool silent = false)
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
            if (Owner != null)
                Owner.RaisePortConnectedEvent(this, connector);
        }

        /// <summary>
        /// Called when a port is disconnected.
        /// </summary>
        protected virtual void OnPortDisconnected()
        {
            if (Owner != null)
                Owner.RaisePortDisconnectedEvent(this);
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

    /// <summary>
    /// PortData stores information for port. It's used for constructing PortModel.
    /// </summary>
    public class PortData
    {
        /// <summary>
        /// Nickname of the port.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Tooltip of the port.
        /// </summary>
        public string ToolTipString { get; set; }

        /// <summary>
        /// Default value of the port.
        /// </summary>
        public AssociativeNode DefaultValue { get; set; }

        /// <summary>
        /// This property is used in code block nodes.
        /// </summary>
        public int LineIndex { get; set; }

        /// <summary>
        /// Height of the port.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Creates PortData.
        /// </summary>
        /// <param name="nickName">Nickname of the port</param>
        /// <param name="tip">Tooltip of the port</param>
        public PortData(string nickName, string tip) : this(nickName, tip, null) { }

        /// <summary>
        /// Creates PortData.
        /// </summary>
        /// <param name="nickName">Nickname of the port</param>
        /// <param name="toolTipString">Tooltip of the port</param>
        /// <param name="defaultValue">Default value of the port</param>
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

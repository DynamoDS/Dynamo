using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Workspaces;
using Dynamo.Utilities;
using Newtonsoft.Json;
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
        private bool isEnabled = true;
        private bool useLevels = false;
        private bool keepListStructure = false;
        private int level = 1;
        private string toolTip;
        #endregion

        #region public members
        /// <summary>
        /// ID of the PortModel, which is unique within the graph.
        /// </summary>
        [JsonProperty("Id")]
        [JsonConverter(typeof(IdToGuidConverter))]
        public override Guid GUID
        {
            get
            {
                return base.GUID;
            }
            set
            {
                base.GUID = value;
            }
        }

        /// <summary>
        /// Returns the connectors between the specified ports.
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<ConnectorModel> Connectors
        {
            get { return connectors; }
            set { connectors = value; }
        }

        /// <summary>
        /// Name of the port.
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Tooltip of the port.
        /// </summary>
        [JsonProperty("Description")]
        public string ToolTip
        {
            get
            {
                string useDefaultArgument = string.Empty;
                if (!UsingDefaultValue && DefaultValue != null)
                    useDefaultArgument = " " + Properties.Resources.DefaultValueDisabled;
                return toolTip.Contains(Properties.Resources.DefaultValueDisabled)? toolTip: toolTip + useDefaultArgument;
            }
            set
            {
                toolTip = value;
                RaisePropertyChanged("ToolTip");
            }
        }

        /// <summary>
        /// Type of the port.
        /// It can be incoming or outcoming.
        /// </summary>
        [JsonIgnore]
        public PortType PortType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns the Node.
        /// </summary>
        [JsonIgnore]
        public NodeModel Owner
        {
            get;
            internal set;
        }

        /// <summary>
        /// Index of the port.
        /// </summary>
        [JsonIgnore]
        public int Index
        {
            get { return Owner.GetPortModelIndex(this); }
        }

        /// <summary>
        /// Returns the LineIndex of that port. The vertical position of PortModel is dependent on LineIndex.
        /// </summary>
        [JsonIgnore]
        public int LineIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates whether the port is enabled or not.
        /// </summary>
        [JsonIgnore]
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
        [JsonIgnore]
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
        [DefaultValue(true)]            
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsingDefaultValue
        {
            get { return usingDefaultValue; }
            set
            {
                usingDefaultValue = value;
                RaisePropertyChanged("UsingDefaultValue");
                RaisePropertyChanged("ToolTip");
            }
        }

        /// <summary>
        /// Default value for port.
        /// </summary>
        [JsonIgnore]
        public AssociativeNode DefaultValue
        {
            get;
            internal set;
        }

        /// <summary>
        /// Controls the space between successive output ports
        /// </summary>
        [JsonIgnore]
        public Thickness MarginThickness
        {
            get;
            private set;
        }

        /// <summary>
        /// Based on extensionEdges port is aligned in UI.
        /// </summary>
        [JsonIgnore]
        public SnapExtensionEdges extensionEdges { get; set; }

        /// <summary>
        /// The Level at which objects will be
        /// extracted from a nested list. The deepest
        /// level of a nested list is -1.
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
        /// A flag which determines whether this Port will 
        /// extract data from a specific level in a nested list.
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
                    
                    if (!useLevels) KeepListStructure = useLevels;
                    RaisePropertyChanged("UseLevels");
                }
            }
        }

        /// <summary>
        /// A flag which determines whether data from this
        /// node will be re-aligned into the original structure
        /// of the nested list.
        /// </summary>
        public bool KeepListStructure
        {
            get
            {
                return keepListStructure;
            }
            set
            {
                if (keepListStructure != value)
                {
                    keepListStructure = value;
                    RaisePropertyChanged(nameof(KeepListStructure));
                }
            }
        }

        /// <summary>
        /// Returns true if the port has connectors or if the 
        /// default value is enabled and not null. Otherwise, returns false.
        /// </summary>
        [JsonIgnore]
        public bool IsConnected
        {
            get
            {
                return Connectors.Any() || (UsingDefaultValue && DefaultValue != null);
            }
        }
        #endregion

        [JsonConstructor]
        internal PortModel(string name, string toolTip)
        {
            UseLevels = false;
            KeepListStructure = false;
            Level = 2;

            Height = 0.0;
            DefaultValue = null;
            LineIndex = -1;
            this.toolTip = toolTip;
            this.Name = name;

            MarginThickness = new Thickness(0);
            Height = Math.Abs(Height) < 0.001 ? Configurations.PortHeightInPixels : Height;
        }

        /// <summary>
        /// Creates PortModel.
        /// </summary>
        /// <param name="portType">Type of the Port</param>
        /// <param name="owner">Parent Node</param>
        /// <param name="data">Information about port</param>
        public PortModel(PortType portType, NodeModel owner, PortData data)
        {
            PortType = portType;
            Owner = owner;
            UseLevels = false;
            KeepListStructure = false;
            Level = 2;

            Height = data.Height;
            DefaultValue = data.DefaultValue;
            UsingDefaultValue = DefaultValue != null;
            LineIndex = data.LineIndex;
            toolTip = data.ToolTipString;
            Name = data.Name;
            
            MarginThickness = new Thickness(0);
            Height = Math.Abs(data.Height) < 0.001 ? Configurations.PortHeightInPixels : data.Height;
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
        /// Name of the port.
        /// </summary>
        public string Name { get; set; }

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
        /// <param name="name">name of the port</param>
        /// <param name="toolTipString">Tooltip of the port</param>
        public PortData(string name, string toolTipString) : this(name, toolTipString, null) { }

        /// <summary>
        /// Creates PortData.
        /// </summary>
        /// <param name="name">name of the port</param>
        /// <param name="toolTipString">Tooltip of the port</param>
        /// <param name="defaultValue">Default value of the port</param>
        [JsonConstructor]
        public PortData(string name, string toolTipString, AssociativeNode defaultValue)
        {
            Name = name;
            ToolTipString = toolTipString;
            DefaultValue = defaultValue;
            LineIndex = -1;
            Height = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Utilities;
using Newtonsoft.Json;

namespace Dynamo.Graph.Connectors
{
    /// <summary>
    /// Returns the Type of connector.
    /// </summary>
    public enum ConnectorType
    {
        /// <summary>
        /// Displays connectors as bezier curves
        /// </summary>
        BEZIER,

        /// <summary>
        /// Displays connectors as set of connected straight lines
        /// </summary>
        POLYLINE
    };

    /// <summary>
    /// Represents a connector between nodes. Connector can be a bezier or polyline <see cref="ConnectorType"/>.
    /// </summary>
    public class ConnectorModel : ModelBase
    {
        #region properties
        private bool isHidden = false;

        /// <summary>
        /// IsHidden flag controlling the visibility of a connector
        /// </summary>
        public bool IsHidden
        {
            get { return isHidden; }
            set
            {
                isHidden = value;
                RaisePropertyChanged(nameof(IsHidden));
            }
        }
        /// <summary>
        /// Returns start port model.
        /// </summary>
        public PortModel Start { get; private set; }

        /// <summary>
        /// Returns end port model.
        /// </summary>
        public PortModel End { get; private set; }

        /// <summary>
        /// Collection of ConnectorPinModels associated with this Connector.
        /// </summary>
        public ObservableCollection<ConnectorPinModel> ConnectorPinModels { get; private set; }

        /// <summary>
        /// Handles adding a new ConnectorPin to the collection of CollectorPins associated with this connector.
        /// This function is called after deserializing and is responsible for reconstructing the pins corresponding
        /// to each connector.
        /// </summary>
        /// <param name="x">'x' location on the canvas where this new pin will be added.</param>
        /// <param name="y">'y' location on the canvas where this new pin will be added.</param>
        internal void AddPin(double x, double y)
        {
            ConnectorPinModels.Add(new ConnectorPinModel(x, y, Guid.NewGuid(),this.GUID));
        }
        /// <summary>
        /// Handles adding a new ConnectorPin to the collection of ConnectorPins associated with this connector.
        /// This function is called after placing a new watch node in a connector and preserving pin locations during 
        /// the re-wire of affected connectors. It is also called during undo/redo operations.
        /// to each connector.
        /// </summary>
        /// <param name="connectorPinModel"> Previous ConnectorPinModel to be re-added to the collection of pins corresponding
        /// to a connector. </param>
        internal void AddPin(ConnectorPinModel connectorPinModel)
        {
            ConnectorPinModels.Add(connectorPinModel);
        }
        internal void RemovePin(ConnectorPinModel connectorPinModel)
        {
            ConnectorPinModels.Remove(connectorPinModel);
        }

        /// <summary>
        /// ID of the Connector, which is unique within the graph.
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

        #endregion 

        #region constructors

        /// <summary>
        /// Factory method to create a connector.  Checks to make sure that the start and end ports are valid, 
        /// otherwise returns null.
        /// </summary>
        /// <param name="start">The node having port where the connector starts</param>
        /// <param name="end">The node having port where the connector ends</param>
        /// <param name="startIndex">Port index in <paramref name="start"/></param>
        /// <param name="endIndex">Port index in <paramref name="end"/></param>
        /// <param name="guid">Identifier of the new connector</param>
        /// <returns>The valid connector model or null if the connector is invalid</returns>
        internal static ConnectorModel Make(
            NodeModel start, NodeModel end, int startIndex, int endIndex, Guid? guid = null)
        {
            if (start != null && end != null && start != end && startIndex >= 0
                && endIndex >= 0 && start.OutPorts.Count > startIndex
                && end.InPorts.Count > endIndex)
            {
                return new ConnectorModel(start, end, startIndex, endIndex, guid ?? Guid.NewGuid());
            }

            Debug.WriteLine("Could not create a connector between {0} and {1}.", start.Name, end.Name);

            return null;
        }

        /// <summary>
        /// Constructor used when only the start and end <see cref="PortModel"/> are known.
        /// </summary>
        /// <param name="start">The start <see cref="PortModel"/>.</param>
        /// <param name="end">The end <see cref="PortModel"/>.</param>
        /// <param name="guid">The unique identifier for the <see cref="ConnectorModel"/>.</param>
        public ConnectorModel(PortModel start, PortModel end, Guid guid)
        {
            ConnectorPinModels = new ObservableCollection<ConnectorPinModel>();
            Debug.WriteLine("Creating a connector between ports {0}(owner:{1}) and {2}(owner:{3}).", 
                start.GUID, start.Owner == null?"null":start.Owner.Name, end.GUID, end.Owner == null?"null":end.Owner.Name);
            Start = start;
            Start.Connectors.Add(this);
            Connect(end);
            GUID = guid;
        }

        private ConnectorModel(
            NodeModel start, NodeModel end, int startIndex, int endIndex, Guid guid)
        {
            ConnectorPinModels = new ObservableCollection<ConnectorPinModel>();
            GUID = guid;
            Start = start.OutPorts[startIndex];
            PortModel endPort = end.InPorts[endIndex];

            Debug.WriteLine("Creating a connector between ports {0}(owner:{1}) and {2}(owner:{3}).",
                start.GUID, Start.Owner == null ? "null" : Start.Owner.Name, end.GUID, endPort.Owner == null ? "null" : endPort.Owner.Name);
            Start.Connectors.Add(this);
            Connect(endPort);
        }

        #endregion

        #region operators

        /// <summary>
        /// Overload for EQUAL operator.
        /// </summary>
        public static bool operator ==(ConnectorModel lhs, ConnectorModel rhs)
        {
            if (ReferenceEquals(lhs, rhs))
                return true;

            if ((object)lhs == null || (object)rhs == null)
                return false;

            return ((lhs.Start.Owner.GUID == rhs.Start.Owner.GUID)
                && (lhs.End.Owner.GUID == rhs.End.Owner.GUID)
                && (lhs.Start.Index == rhs.Start.Index)
                && (lhs.End.Index == rhs.End.Index));
        }

        /// <summary>
        /// Overload for NOT EQUAL operator.
        /// </summary>
        public static bool operator !=(ConnectorModel lhs, ConnectorModel rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        private void Connect(PortModel p)
        {
            //test if the port that you are connecting too is not the start port or the end port
            //of the current connector
            if (p.Equals(Start) || p.Equals(End))
            {
                return;
            }

            //if the selected connector is also an output connector, return false
            //output ports can't be connected to eachother
            if (p.PortType == PortType.Output)
            {
                return;
            }

            //test if the port that you are connecting to is an input and 
            //already has other connectors
            if (p.PortType == PortType.Input && p.Connectors.Count > 0)
            {
                p.Connectors.Remove(p.Connectors[0]);
            }

            //turn the line solid
            End = p;

            if (End != null)
            {
                p.Connectors.Add(this);
            }

            return;
        }

        /// <summary>
        /// Delete the connector without raising port disconnection events.
        /// </summary>
        internal void Delete()
        {
            if (Start != null && Start.Connectors.Contains(this))
            {
                Start.Connectors.Remove(this);
            }
            if (End != null && End.Connectors.Contains(this))
            {
                End.Connectors.Remove(this);
            }
            OnDeleted();
        }

        #region Serialization/Deserialization Methods
        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case nameof(IsHidden):
                    IsHidden = Convert.ToBoolean(value);
                    break;
                default:
                    break;
            }

            return base.UpdateValueCore(updateValueParams);
        }
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("start", Start.Owner.GUID);
            helper.SetAttribute("start_index", Start.Index);
            helper.SetAttribute("end", End.Owner.GUID);
            helper.SetAttribute("end_index", End.Index);
            helper.SetAttribute(nameof(IsHidden), IsHidden);
            //helper.SetAttribute("portType", ((int) End.PortType));
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var helper = new XmlElementHelper(nodeElement);
            IsHidden = helper.ReadBoolean(nameof(IsHidden));
            //This is now handled via NodeGraph.LoadConnectorFromXml
        }

        #endregion

        /// <summary>
        /// Occurs when deleting connector.
        /// </summary>
        public event Action Deleted;
        protected virtual void OnDeleted()
        {
            var handler = Deleted;
            if (handler != null) handler();
        }
    }
}

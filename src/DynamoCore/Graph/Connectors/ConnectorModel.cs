using System;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;

namespace Dynamo.Graph.Connectors
{
    public enum ConnectorType { BEZIER, POLYLINE };

    public class ConnectorModel : ModelBase
    {
        #region properties

        public PortModel Start { get; private set; }
        public PortModel End { get; private set; }

        #endregion 

        #region constructors

        /// <summary>
        /// Factory method to create a connector.  Checks to make sure that the start and end ports are valid, 
        /// otherwise returns null.
        /// </summary>
        /// <param name="start">The port where the connector starts</param>
        /// <param name="end">The port where the connector ends</param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="guid"></param>
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

            return null;
        }

        private ConnectorModel(
            NodeModel start, NodeModel end, int startIndex, int endIndex, Guid guid)
        {
            GUID = guid;
            Start = start.OutPorts[startIndex];

            PortModel endPort = end.InPorts[endIndex];

            Start.Connect(this);
            Connect(endPort);
        }

        #endregion

        #region operators

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
                p.Disconnect(p.Connectors[0]);
            }

            //turn the line solid
            End = p;

            if (End != null)
            {
                p.Connect(this);
            }

            return;
        }
        
        /// <summary>
        /// Delete the connector.
        /// </summary>
        /// <param name="silent">If silent is true, the start and end ports will be disconnected
        /// without raising port disconnection events.</param>
        internal void Delete(bool silent = false)
        {
            if (Start != null && Start.Connectors.Contains(this))
            {
                Start.Disconnect(this, silent);
            }
            if (End != null && End.Connectors.Contains(this))
            {
                End.Disconnect(this, silent);
            }
            OnDeleted();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("guid", GUID);
            helper.SetAttribute("start", Start.Owner.GUID);
            helper.SetAttribute("start_index", Start.Index);
            helper.SetAttribute("end", End.Owner.GUID);
            helper.SetAttribute("end_index", End.Index);
            //helper.SetAttribute("portType", ((int) End.PortType));
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            //This is now handled via NodeGraph.LoadConnectorFromXml

            /*
            var helper = new XmlElementHelper(element);

            // Restore some information from the node attributes.
            GUID = helper.ReadGuid("guid", GUID);
            Guid startNodeId = helper.ReadGuid("start");
            int startIndex = helper.ReadInteger("start_index");
            Guid endNodeId = helper.ReadGuid("end");
            int endIndex = helper.ReadInteger("end_index");
            var portType = ((PortType)helper.ReadInteger("portType"));

            // Get to the start and end nodes that this connector connects to.
            var startNode = workspaceModel.GetModelInternal(startNodeId) as NodeModel;
            var endNode = workspaceModel.GetModelInternal(endNodeId) as NodeModel;

            pStart = startNode.OutPorts[startIndex];
            PortModel endPort = null;
            if (portType == PortType.Input)
                endPort = endNode.InPorts[endIndex];

            pStart.Connect(this);
            Connect(endPort);*/
        }

        #endregion

        public event Action Deleted;
        protected virtual void OnDeleted()
        {
            var handler = Deleted;
            if (handler != null) handler();
        }
    }

    internal class InvalidPortException : ApplicationException
    {
        private readonly string message;
        public override string Message
        {
            get { return message; }
        }

        public InvalidPortException()
        {
            message = "Connection port is not valid.";
        }
    }
}

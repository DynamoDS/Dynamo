using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Models;
using Dynamo.Nodes;
using Double = System.Double;

namespace Dynamo.Core
{
    public class NodeGraph
    {
        public List<NodeModel> Nodes;
        public List<ConnectorModel> Connectors;
        public List<NoteModel> Notes;

        private static IEnumerable<NodeModel> LoadNodesFromXml(XmlDocument xmlDoc)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes.Count == 0)
                elNodes = xmlDoc.GetElementsByTagName("dynElements");
            XmlNode elNodesList = elNodes[0];

            foreach (XmlNode elNode in elNodesList.ChildNodes)
            {
                XmlAttribute typeAttrib = elNode.Attributes["type"];
                XmlAttribute guidAttrib = elNode.Attributes["guid"];
                XmlAttribute nicknameAttrib = elNode.Attributes["nickname"];
                XmlAttribute xAttrib = elNode.Attributes["x"];
                XmlAttribute yAttrib = elNode.Attributes["y"];
                XmlAttribute isVisAttrib = elNode.Attributes["isVisible"];
                XmlAttribute isUpstreamVisAttrib = elNode.Attributes["isUpstreamVisible"];
                XmlAttribute lacingAttrib = elNode.Attributes["lacing"];

                string typeName = typeAttrib.Value;

                //test the GUID to confirm that it is non-zero
                //if it is zero, then we have to fix it
                //this will break the connectors, but it won't keep
                //propagating bad GUIDs
                var guid = new Guid(guidAttrib.Value);
                if (guid == Guid.Empty)
                {
                    guid = Guid.NewGuid();
                }

                string nickname = nicknameAttrib.Value;

                double x = double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                double y = double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                bool isVisible = true;
                if (isVisAttrib != null)
                    isVisible = isVisAttrib.Value == "true";

                bool isUpstreamVisible = true;
                if (isUpstreamVisAttrib != null)
                    isUpstreamVisible = isUpstreamVisAttrib.Value == "true";

                // Retrieve optional 'function' attribute (only for DSFunction).
                XmlAttribute signatureAttrib = elNode.Attributes["function"];
                var signature = signatureAttrib == null ? null : signatureAttrib.Value;
                
                NodeModel node = null;
                XmlElement dummyElement = null;

                try
                {
                    // The attempt to create node instance may fail due to "type" being
                    // something else other than "NodeModel" derived object type. This 
                    // is possible since some legacy nodes have been made to derive from
                    // "MigrationNode" object type that is not derived from "NodeModel".
                    // 
                    typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                    Type type = Dynamo.Nodes.Utilities.ResolveType(dynamoModel, typeName);
                    if (type != null)
                        node = nodeFactory.CreateNodeInstance(type, nickname, signature, guid);

                    if (node != null)
                    {
                        node.Load(elNode);
                    }
                    else
                    {
                        var e = elNode as XmlElement;
                        dummyElement = MigrationManager.CreateMissingNode(e, 1, 1);
                    }
                }
                catch (UnresolvedFunctionException)
                {
                    // If a given function is not found during file load, then convert the 
                    // function node into a dummy node (instead of crashing the workflow).
                    // 
                    var e = elNode as XmlElement;
                    dummyElement = MigrationManager.CreateUnresolvedFunctionNode(e);
                }

                //=====HOME=====

                // If a custom node fails to load its definition, convert it into a dummy node.
                var function = node as Function;
                if ((function != null) && (function.Definition == null))
                {
                    var e = elNode as XmlElement;
                    dummyElement = MigrationManager.CreateMissingNode(
                        e, node.InPortData.Count, node.OutPortData.Count);
                }

                //==============

                if (dummyElement != null) // If a dummy node placement is desired.
                {
                    // The new type representing the dummy node.
                    typeName = dummyElement.GetAttribute("type");
                    var type = Dynamo.Nodes.Utilities.ResolveType(dynamoModel, typeName);

                    node = NodeFactory.CreateNodeInstance(type, nickname, string.Empty, guid);
                    node.Load(dummyElement);
                }

                node.X = x;
                node.Y = y;

                if (lacingAttrib != null)
                {
                    if (node.ArgumentLacing != LacingStrategy.Disabled)
                    {
                        LacingStrategy lacing;
                        Enum.TryParse(lacingAttrib.Value, out lacing);
                        node.ArgumentLacing = lacing;
                    }
                }

                // This is to fix MAGN-3648. Method reference in CBN that gets 
                // loaded before method definition causes a CBN to be left in 
                // a warning state. This is to clear such warnings and set the 
                // node to "Dead" state (correct value of which will be set 
                // later on with a call to "EnableReporting" below). Please 
                // refer to the defect for details and other possible fixes.
                // 
                if (node.State == ElementState.Warning && (node is CodeBlockNodeModel))
                    node.State = ElementState.Dead; // Condition to fix MAGN-3648


                node.IsVisible = isVisible;
                node.IsUpstreamVisible = isUpstreamVisible;

                yield return node;
            }
        }

        public static NodeGraph LoadFromXml(XmlDocument xmlDoc)
        {
            var nodes = new List<NodeModel>();
            var connectors = new List<NodeModel>();
            var notes = new List<ConnectorModel>();

            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            XmlNodeList cNodes = xmlDoc.GetElementsByTagName("Connectors");
            XmlNodeList nNodes = xmlDoc.GetElementsByTagName("Notes");

            if (elNodes.Count == 0)
                elNodes = xmlDoc.GetElementsByTagName("dynElements");
            if (cNodes.Count == 0)
                cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
            if (nNodes.Count == 0)
                nNodes = xmlDoc.GetElementsByTagName("dynNotes");

            XmlNode elNodesList = elNodes[0];
            XmlNode cNodesList = cNodes[0];
            XmlNode nNodesList = nNodes[0];

            #region Nodes

            

            #endregion

            #region Connectors

            foreach (XmlNode connector in cNodesList.ChildNodes)
            {
                XmlAttribute guidStartAttrib = connector.Attributes[0];
                XmlAttribute intStartAttrib = connector.Attributes[1];
                XmlAttribute guidEndAttrib = connector.Attributes[2];
                XmlAttribute intEndAttrib = connector.Attributes[3];
                XmlAttribute portTypeAttrib = connector.Attributes[4];

                var guidStart = new Guid(guidStartAttrib.Value);
                var guidEnd = new Guid(guidEndAttrib.Value);
                int startIndex = Convert.ToInt16(intStartAttrib.Value);
                int endIndex = Convert.ToInt16(intEndAttrib.Value);
                var portType = ((PortType)Convert.ToInt16(portTypeAttrib.Value));

                //find the elements to connect
                NodeModel start = null;
                NodeModel end = null;

                foreach (NodeModel e in CurrentWorkspace.Nodes)
                {
                    if (e.GUID == guidStart)
                    {
                        start = e;
                    }
                    else if (e.GUID == guidEnd)
                    {
                        end = e;
                    }
                    if (start != null && end != null)
                    {
                        break;
                    }
                }

                var newConnector = CurrentWorkspace.AddConnection(
                    start,
                    end,
                    startIndex,
                    endIndex,
                    Logger,
                    portType);

                OnConnectorAdded(newConnector);
            }

            #endregion

            #region Notes

            if (nNodesList != null)
            {
                foreach (XmlNode note in nNodesList.ChildNodes)
                {
                    XmlAttribute textAttrib = note.Attributes[0];
                    XmlAttribute xAttrib = note.Attributes[1];
                    XmlAttribute yAttrib = note.Attributes[2];

                    string text = textAttrib.Value;
                    double x = Double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
                    double y = Double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

                    // TODO(Ben): Shouldn't we be reading in the Guid 
                    // from file instead of generating a new one here?
                    CurrentWorkspace.AddNote(false, x, y, text, Guid.NewGuid());
                }
            }

            #endregion
            
            //=====HOME=====

            HomeSpace.FileName = xmlPath;

            // Allow live runner a chance to preload trace data from XML.
            var engine = EngineController;
            if (engine != null && (engine.LiveRunnerCore != null))
            {
                var data = Utils.LoadTraceDataFromXmlDocument(xmlDoc);
                CurrentWorkspace.PreloadedTraceData = data;
            }

            //====CUSTOM====

            def.IsBeingLoaded = false;
            def.Compile(this.dynamoModel.EngineController);
            SetFunctionDefinition(def.FunctionId, def);
            ws.WatchChanges = true;
            OnGetDefinitionFromPath(def);

            //==============
        }
    }
}

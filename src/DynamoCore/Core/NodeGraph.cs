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

        private static IEnumerable<NodeModel> LoadNodesFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes.Count == 0)
                elNodes = xmlDoc.GetElementsByTagName("dynElements");
            XmlNode elNodesList = elNodes[0];
            return from XmlNode elNode in elNodesList.ChildNodes select nodeFactory.CreateNodeFromXml(elNode);
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

                foreach (NodeModel e in nodes)
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

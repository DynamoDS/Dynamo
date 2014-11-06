using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Double = System.Double;

namespace Dynamo.Core
{
    public class NodeGraph
    {
        public List<NodeModel> Nodes { get; private set; }
        public List<ConnectorModel> Connectors { get; private set; }
        public List<NoteModel> Notes { get; private set; }

        private NodeGraph() { }

        private static IEnumerable<NodeModel> LoadNodesFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes.Count == 0)
                elNodes = xmlDoc.GetElementsByTagName("dynElements");
            XmlNode elNodesList = elNodes[0];
            return from XmlNode elNode in elNodesList.ChildNodes select nodeFactory.CreateNodeFromXml(elNode);
        }

        private static bool LoadConnectorFromXml(XmlNode connEl, IDictionary<Guid, NodeModel> nodes, out ConnectorModel connector)
        {
            XmlAttribute guidStartAttrib = connEl.Attributes[0];
            XmlAttribute intStartAttrib = connEl.Attributes[1];
            XmlAttribute guidEndAttrib = connEl.Attributes[2];
            XmlAttribute intEndAttrib = connEl.Attributes[3];
            XmlAttribute portTypeAttrib = connEl.Attributes[4];

            var guidStart = new Guid(guidStartAttrib.Value);
            var guidEnd = new Guid(guidEndAttrib.Value);
            int startIndex = Convert.ToInt16(intStartAttrib.Value);
            int endIndex = Convert.ToInt16(intEndAttrib.Value);
            var portType = ((PortType)Convert.ToInt16(portTypeAttrib.Value));

            //find the elements to connect
            NodeModel start;
            if (nodes.TryGetValue(guidStart, out start))
            {
                NodeModel end;
                if (nodes.TryGetValue(guidEnd, out end))
                {
                    connector = ConnectorModel.Make(start, end, startIndex, endIndex, portType);
                    return connector != null;
                }
            }
            connector = null;
            return false;
        }

        private static IEnumerable<ConnectorModel> LoadConnectorsFromXml(XmlDocument xmlDoc, IDictionary<Guid, NodeModel> nodes)
        {
            XmlNodeList cNodes = xmlDoc.GetElementsByTagName("Connectors");
            if (cNodes.Count == 0)
                cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
            XmlNode cNodesList = cNodes[0];

            foreach (XmlNode connector in cNodesList.ChildNodes)
            {
                ConnectorModel c;
                if (LoadConnectorFromXml(connector, nodes, out c))
                    yield return c;
            }
        }

        private static IEnumerable<NoteModel> LoadNotesFromXml(XmlDocument xmlDoc)
        {
            XmlNodeList nNodes = xmlDoc.GetElementsByTagName("Notes");
            if (nNodes.Count == 0)
                nNodes = xmlDoc.GetElementsByTagName("dynNotes");
            XmlNode nNodesList = nNodes[0];

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

                    // TODO(Ben): Shouldn't we be reading in the Guid from file instead of generating a new one here?
                    yield return new NoteModel(x, y, text, Guid.NewGuid());
                }
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeFactory"></param>
        /// <returns></returns>
        public static NodeGraph LoadFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            var nodes = LoadNodesFromXml(xmlDoc, nodeFactory).ToList();
            var connectors = LoadConnectorsFromXml(xmlDoc, nodes.ToDictionary(node => node.GUID)).ToList();
            var notes = LoadNotesFromXml(xmlDoc).ToList();

            return new NodeGraph { Nodes = nodes, Connectors = connectors, Notes = notes };

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

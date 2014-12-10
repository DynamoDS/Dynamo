using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
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
            return from XmlElement elNode in elNodesList.ChildNodes
                   select LoadNodeFromXml(elNode, SaveContext.File, nodeFactory);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="elNode"></param>
        /// <param name="context"></param>
        /// <param name="nodeFactory"></param>
        /// <returns></returns>
        public static NodeModel LoadNodeFromXml(
            XmlElement elNode, SaveContext context, NodeFactory nodeFactory)
        {
            return nodeFactory.CreateNodeFromXml(elNode, context);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="connEl"></param>
        /// <param name="nodes"></param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static bool LoadConnectorFromXml(XmlNode connEl, IDictionary<Guid, NodeModel> nodes, out ConnectorModel connector)
        {
            XmlAttribute guidStartAttrib = connEl.Attributes[0];
            XmlAttribute intStartAttrib = connEl.Attributes[1];
            XmlAttribute guidEndAttrib = connEl.Attributes[2];
            XmlAttribute intEndAttrib = connEl.Attributes[3];
            //XmlAttribute portTypeAttrib = connEl.Attributes[4];

            var guidStart = new Guid(guidStartAttrib.Value);
            var guidEnd = new Guid(guidEndAttrib.Value);
            int startIndex = Convert.ToInt16(intStartAttrib.Value);
            int endIndex = Convert.ToInt16(intEndAttrib.Value);
            //var portType = ((PortType)Convert.ToInt16(portTypeAttrib.Value));

            //find the elements to connect
            NodeModel start;
            if (nodes.TryGetValue(guidStart, out start))
            {
                NodeModel end;
                if (nodes.TryGetValue(guidEnd, out end))
                {
                    connector = ConnectorModel.Make(start, end, startIndex, endIndex);
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public static NoteModel LoadNoteFromXml(XmlNode note)
        {
            XmlAttribute textAttrib = note.Attributes[0];
            XmlAttribute xAttrib = note.Attributes[1];
            XmlAttribute yAttrib = note.Attributes[2];

            string text = textAttrib.Value;
            double x = Double.Parse(xAttrib.Value, CultureInfo.InvariantCulture);
            double y = Double.Parse(yAttrib.Value, CultureInfo.InvariantCulture);

            // TODO(Ben): Shouldn't we be reading in the Guid from file instead of generating a new one here?
            return new NoteModel(x, y, text, Guid.NewGuid());
        }

        private static IEnumerable<NoteModel> LoadNotesFromXml(XmlDocument xmlDoc)
        {
            XmlNodeList nNodes = xmlDoc.GetElementsByTagName("Notes");
            if (nNodes.Count == 0)
                nNodes = xmlDoc.GetElementsByTagName("dynNotes");
            XmlNode nNodesList = nNodes[0];

            return nNodesList != null
                ? nNodesList.ChildNodes.Cast<XmlNode>().Select(LoadNoteFromXml)
                : Enumerable.Empty<NoteModel>();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeFactory"></param>
        /// <returns></returns>
        public static NodeGraph LoadGraphFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            var nodes = LoadNodesFromXml(xmlDoc, nodeFactory).ToList();
            var connectors = LoadConnectorsFromXml(xmlDoc, nodes.ToDictionary(node => node.GUID)).ToList();
            var notes = LoadNotesFromXml(xmlDoc).ToList();

            return new NodeGraph { Nodes = nodes, Connectors = connectors, Notes = notes };
        }
    }
}

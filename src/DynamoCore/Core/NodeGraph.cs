using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Utilities;
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
        ///     Creates and initializes a NodeModel from its Xml representation.
        /// </summary>
        /// <param name="elNode">XmlElement for a NodeModel.</param>
        /// <param name="context">The serialization context for initialization.</param>
        /// <param name="nodeFactory">A NodeFactory, to be used to create the node.</param>
        /// <returns></returns>
        public static NodeModel LoadNodeFromXml(
            XmlElement elNode, SaveContext context, NodeFactory nodeFactory)
        {
            return nodeFactory.CreateNodeFromXml(elNode, context);
        }

        /// <summary>
        ///     Creates and initializes a ConnectorModel from its Xml representation.
        /// </summary>
        /// <param name="connEl">XmlElement for a ConnectorModel.</param>
        /// <param name="nodes">Dictionary to be used for looking up a NodeModel by it's Guid.</param>
        /// <param name="connector"></param>
        /// <returns></returns>
        public static bool LoadConnectorFromXml(XmlElement connEl, IDictionary<Guid, NodeModel> nodes, out ConnectorModel connector)
        {
            var helper = new XmlElementHelper(connEl);

            var guid = helper.ReadGuid("guid", Guid.NewGuid());
            var guidStart = helper.ReadGuid("start");
            var guidEnd = helper.ReadGuid("end");
            int startIndex = helper.ReadInteger("start_index");
            int endIndex = helper.ReadInteger("end_index");

            //find the elements to connect
            NodeModel start;
            if (nodes.TryGetValue(guidStart, out start))
            {
                NodeModel end;
                if (nodes.TryGetValue(guidEnd, out end))
                {
                    connector = ConnectorModel.Make(start, end, startIndex, endIndex, guid);
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

            foreach (XmlElement connector in cNodesList.ChildNodes)
            {
                ConnectorModel c;
                if (LoadConnectorFromXml(connector, nodes, out c))
                    yield return c;
            }
        }

        /// <summary>
        ///     Creates and initializes a NoteModel from its Xml representation.
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
        ///     Loads NodeModels, ConnectorModels, and NoteModels from an XmlDocument.
        /// </summary>
        /// <param name="xmlDoc">An XmlDocument representing a serialized Dynamo workspace.</param>
        /// <param name="nodeFactory">A NodeFactory, used to load and instantiate nodes.</param>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Dynamo.Utilities;
using ProtoCore.AST;
using ProtoCore.Namespace;

namespace Dynamo.Core
{
    public class NodeGraph
    {
        public List<NodeModel> Nodes { get; private set; }
        public List<ConnectorModel> Connectors { get; private set; }
        public List<NoteModel> Notes { get; private set; }
        public List<AnnotationModel> Annotations { get; private set; }
        public ElementResolver ElementResolver { get; private set; }
  
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
        /// <returns>Returns the new instance of ConnectorModel loaded from XmlElement.</returns>
        public static ConnectorModel LoadConnectorFromXml(XmlElement connEl, IDictionary<Guid, NodeModel> nodes)
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
                    return ConnectorModel.Make(start, end, startIndex, endIndex, guid);
                }
            }

            return null;
        }

        private static IEnumerable<ConnectorModel> LoadConnectorsFromXml(XmlDocument xmlDoc, IDictionary<Guid, NodeModel> nodes)
        {
            XmlNodeList cNodes = xmlDoc.GetElementsByTagName("Connectors");
            if (cNodes.Count == 0)
                cNodes = xmlDoc.GetElementsByTagName("dynConnectors");
            XmlNode cNodesList = cNodes[0];

            foreach (XmlElement connector in cNodesList.ChildNodes)
            {
                var c = LoadConnectorFromXml(connector, nodes);
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
            var instance = new NoteModel(0, 0, string.Empty, Guid.NewGuid());
            instance.Deserialize(note as XmlElement, SaveContext.File);
            return instance;
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

        internal static AnnotationModel LoadAnnotationFromXml(XmlNode annotation, IEnumerable<NodeModel> nodes, IEnumerable<NoteModel> notes)
        {
            var instance = new AnnotationModel(nodes,notes);             
            instance.Deserialize(annotation as XmlElement, SaveContext.File);
            return instance;
        }

        private static IEnumerable<AnnotationModel> LoadAnnotationsFromXml(XmlDocument xmlDoc, IEnumerable<NodeModel> nodes,
                                                                                IEnumerable<NoteModel> notes )
        {
            XmlNodeList nNodes = xmlDoc.GetElementsByTagName("Annotations");
            if (nNodes.Count == 0)
                nNodes = xmlDoc.GetElementsByTagName("dynAnnotations");
            XmlNode nNodesList = nNodes[0];
            if (nNodesList != null)
                return from XmlElement annotation in nNodesList.ChildNodes.Cast<XmlNode>() select LoadAnnotationFromXml(annotation, nodes, notes);
            
            return Enumerable.Empty<AnnotationModel>();
        }

        /// <summary>
        ///     Loads NodeModels, ConnectorModels, and NoteModels from an XmlDocument.
        /// </summary>
        /// <param name="xmlDoc">An XmlDocument representing a serialized Dynamo workspace.</param>
        /// <param name="nodeFactory">A NodeFactory, used to load and instantiate nodes.</param>
        /// <param name="elementResolver"></param>
        /// <returns></returns>
        public static NodeGraph LoadGraphFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            var nodes = LoadNodesFromXml(xmlDoc, nodeFactory).ToList();
            var connectors = LoadConnectorsFromXml(xmlDoc, nodes.ToDictionary(node => node.GUID)).ToList();
            var notes = LoadNotesFromXml(xmlDoc).ToList();
            var annotations = LoadAnnotationsFromXml(xmlDoc, nodes, notes).ToList();

            return new NodeGraph { Nodes = nodes, Connectors = connectors, Notes = notes, Annotations =annotations};
        }

    }
}

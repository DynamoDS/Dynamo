using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Utilities;
using ProtoCore.Namespace;

namespace Dynamo.Graph
{
    /// <summary>
    /// This class is used to load workspace from xml file.
    /// </summary>
    public class NodeGraph
    {
        /// <summary>
        /// <see cref="NodeModel"/> loaded from xml.
        /// </summary>
        public List<NodeModel> Nodes { get; private set; }

        /// <summary>
        /// <see cref="ConnectorModel"/> loaded from xml.
        /// </summary>
        public List<ConnectorModel> Connectors { get; private set; }

        /// <summary>
        /// <see cref="NoteModel"/> loaded from xml.
        /// </summary>
        public List<NoteModel> Notes { get; private set; }

        /// <summary>
        /// <see cref="AnnotationModel"/> loaded from xml.
        /// </summary>
        public List<AnnotationModel> Annotations { get; private set; }

        /// <summary>
        /// Partial class name nodes loaded from xml.
        /// E.g. Range turns into DSCoreNodesUI.Range.
        /// </summary>
        public ElementResolver ElementResolver { get; private set; }

        /// <summary>
        /// <see cref="PresetModel"/> loaded from xml.
        /// </summary>
        public List<PresetModel> Presets { get; private set; }
  
        private NodeGraph() { }

        private static IEnumerable<NodeModel> LoadNodesFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory, ElementResolver resolver)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes.Count == 0)
                elNodes = xmlDoc.GetElementsByTagName("dynElements");
            XmlNode elNodesList = elNodes[0];
            return from XmlElement elNode in elNodesList.ChildNodes
                   select LoadNodeFromXml(elNode, SaveContext.File, nodeFactory, resolver);
        }

        /// <summary>
        ///     Creates and initializes a NodeModel from its Xml representation.
        /// </summary>
        /// <param name="elNode">XmlElement for a NodeModel.</param>
        /// <param name="context">The serialization context for initialization.</param>
        /// <param name="nodeFactory">A NodeFactory, to be used to create the node.</param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static NodeModel LoadNodeFromXml(
            XmlElement elNode, SaveContext context, NodeFactory nodeFactory, ElementResolver resolver)
        {
            return nodeFactory.CreateNodeFromXml(elNode, context, resolver);
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
        /// Loads presets from xml file. 
        /// </summary>
        /// <param name="xmlDoc">xml file</param>
        /// <param name="nodesInNodeGraph">node models</param>
        /// <returns>list of presets</returns>
        public static IEnumerable<PresetModel> LoadPresetsFromXml(XmlDocument xmlDoc, IEnumerable<NodeModel> nodesInNodeGraph)
        {
            XmlNodeList PresetsNodes = xmlDoc.GetElementsByTagName("Presets");
            XmlNode presetlist = PresetsNodes[0];
            if (presetlist != null)
            {
                return from XmlElement stateNode in presetlist.ChildNodes
                       select PresetFromXml(stateNode, nodesInNodeGraph);
            }
            return Enumerable.Empty<PresetModel>();
        }

        private static PresetModel PresetFromXml(XmlElement stateNode, IEnumerable<NodeModel> nodesInNodeGraph)
        {
            var instance = new PresetModel(nodesInNodeGraph);
            instance.Deserialize(stateNode, SaveContext.File);
            return instance;
        }

        private static ElementResolver LoadElementResolverFromXml(XmlDocument xmlDoc)
        {
            var nodes = xmlDoc.GetElementsByTagName("NamespaceResolutionMap");

            var resolutionMap = new Dictionary<string, KeyValuePair<string, string>>();
            if (nodes.Count > 0)
            {
                foreach (XmlNode child in nodes[0].ChildNodes)
                {
                    if (child.Attributes != null)
                    {
                        XmlAttribute pName = child.Attributes["partialName"];
                        XmlAttribute rName = child.Attributes["resolvedName"];
                        XmlAttribute aName = child.Attributes["assemblyName"];
                        var kvp = new KeyValuePair<string, string>(rName.Value, aName.Value);
                        resolutionMap.Add(pName.Value, kvp);
                    }
                }
            }

            return new ElementResolver(resolutionMap);
        }

        /// <summary>
        ///     Loads NodeModels, ConnectorModels, and NoteModels from an XmlDocument.
        /// </summary>
        /// <param name="xmlDoc">An XmlDocument representing a serialized Dynamo workspace.</param>
        /// <param name="nodeFactory">A NodeFactory, used to load and instantiate nodes.</param>
        /// <returns></returns>
        public static NodeGraph LoadGraphFromXml(XmlDocument xmlDoc, NodeFactory nodeFactory)
        {
            var elementResolver = LoadElementResolverFromXml(xmlDoc);
            var nodes = LoadNodesFromXml(xmlDoc, nodeFactory, elementResolver).ToList();
            var connectors = LoadConnectorsFromXml(xmlDoc, nodes.ToDictionary(node => node.GUID)).ToList();
            var notes = LoadNotesFromXml(xmlDoc).ToList();
            var annotations = LoadAnnotationsFromXml(xmlDoc, nodes, notes).ToList();
            var presets = LoadPresetsFromXml(xmlDoc,nodes).ToList();

            return new NodeGraph { Nodes = nodes, Connectors = connectors, Notes = notes, Annotations = annotations, Presets = presets, ElementResolver = elementResolver };
        }

    }
}

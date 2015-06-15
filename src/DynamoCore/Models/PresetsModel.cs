using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Nodes;
using System.Xml;
using Dynamo.Interfaces;
using DynamoUtilities;

namespace Dynamo.Models
{
    /// <summary>
    /// a class that holds a set of preset design options states
    /// there is one instance of this class per workspacemodel
    /// </summary>
    public class PresetsModel : ILogSource
    {
        #region private members
        private readonly List<PresetState> presetStates = new List<PresetState>();
        #endregion

        # region properties
        public IEnumerable<PresetState> PresetStates { get { return presetStates; } }

        public const string GuidAttributeName = "guid";
        public const string NameAttributeName = "Name";
        public const string DescriptionAttributeName = "Description";
        public const string NicknameAttributeName = "nickname";

        #endregion

        #region serialization / deserialzation

        //grabbed some methods needed from modelbase for serialization
        protected virtual XmlElement CreateElement(XmlDocument xmlDocument, SaveContext context)
        {
            string typeName = GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            return element;
        }

        /// <summary>
        /// we serialze the presets to xml like a model, but we deserialze them before the workspacemodel is constructed
        /// during save and load of the graph, can just inject this into workspacemodel 
        /// when a new presetState is created we'll serialize all the current nodes into a new xmlelement
        /// but not actually write this xml to a file until the graph is saved
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            var element = CreateElement(xmlDocument, context);
            SerializeCore(element, context);
            return element;
        }

        protected virtual void SerializeCore(System.Xml.XmlElement element, SaveContext context)
        {

            foreach (var state in presetStates)
            {
                var parent = element.OwnerDocument.CreateElement("PresetState");
                element.AppendChild(parent);
                parent.SetAttribute(NameAttributeName, state.Name);
                parent.SetAttribute(DescriptionAttributeName, state.Description);
                parent.SetAttribute(GuidAttributeName, state.Guid.ToString());
                //the states are already serialized
                foreach (var serializedNode in state.SerializedNodes)
                {
                    //need to import the node to cross xml contexts
                    var importNode = parent.OwnerDocument.ImportNode(serializedNode, true);
                    parent.AppendChild(importNode);
                }

            }
        }

        internal static PresetsModel LoadFromXmlPaths(string presetsPath, string dynPath,NodeFactory nodefactory)
        {
            var doc = XmlHelper.CreateDocument("tempworkspace");
            doc.Load(dynPath);
            var graph = NodeGraph.LoadGraphFromXml(doc,nodefactory);

            return LoadFromXml(presetsPath, graph, nodefactory.AsLogger());
        }

        /// <summary>
        /// this is method used to load a state from xml into this presets model collection
        /// </summary>
        private void loadStateFromXml(string name, string description, List<NodeModel> nodes, List<XmlElement> serializednodes, Guid id)
        {
            var loadedState = new PresetState(name, description, nodes, serializednodes, id);
            presetStates.Add(loadedState);
        }

        internal static PresetsModel LoadFromXml(string xmlDocPath, NodeGraph nodegraph, ILogger logger)
        {
            var doc = new XmlDocument();
            doc.Load(xmlDocPath);
            return LoadFromXml(doc, nodegraph, logger);
        }

        internal static PresetsModel LoadFromXml(XmlDocument xmlDoc, NodeGraph nodegraph, ILogger logger)
        {
            var loadedStateSet = new PresetsModel();

            //create a new state inside the set foreach state present in the xmldoc

            foreach (XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                if (element.Name == typeof(PresetsModel).ToString())
                {
                    foreach (XmlElement stateNode in element.ChildNodes)
                    {
                        var name = stateNode.GetAttribute(NameAttributeName);
                        var des = stateNode.GetAttribute(DescriptionAttributeName);
                        var stateguidString = stateNode.GetAttribute(GuidAttributeName);

                        Guid stateID;
                        if (!Guid.TryParse(stateguidString, out stateID))
                        {
                            logger.LogError("unable to parse the GUID for preset state: " + name + ", will atttempt to load this state anyway");
                        }

                        var nodes = new List<NodeModel>();
                        var deserialzedNodes = new List<XmlElement>();
                        //now find the nodes we're looking for by their guids in the loaded nodegraph
                        //it's possible they may no longer be present, and we must not fail to set the

                        //iterate each actual saved nodemodel in each state
                        foreach (XmlElement node in stateNode.ChildNodes)
                        {
                            var nodename = stateNode.GetAttribute(NicknameAttributeName);
                            var guidString = node.GetAttribute(GuidAttributeName);
                            Guid nodeID;
                            if (!Guid.TryParse(guidString, out nodeID))
                            {
                                logger.LogError("unable to parse GUID for node " + nodename);
                                continue;
                            }

                            var nodebyGuid = nodegraph.Nodes.Where(x => x.GUID == nodeID).ToList();
                            if (nodebyGuid.Count > 0)
                            {
                                nodes.Add(nodebyGuid.First());
                                deserialzedNodes.Add(node);
                            }
                            else
                            {   //add the deserialized version anyway so we dont lose this node from all states.
                                deserialzedNodes.Add(node);
                                logger.Log(nodename + nodeID.ToString() + " could not be found in the loaded .dyn");
                            }

                        }

                        loadedStateSet.loadStateFromXml(name, des, nodes, deserialzedNodes, stateID);
                    }
                }
            }
            return loadedStateSet;
        }
        #endregion

        #region public methods
        /// <summary>
        ///  this method creates a new preset state from a set of NodeModels and adds this new state to this presets collection
        /// </summary>
        /// <param name="name">the name of preset state</param>
        /// <param name="description">a description of what the state does</param>
        /// <param name="currentSelection">a set of NodeModels that are to be serialized in this state</param>
        /// <param name="id">a GUID id for the state, if not supplied, a new GUID will be generated, cannot be a duplicate</param>
        public void AddState(string name, string description, IEnumerable<NodeModel> currentSelection, Guid id = new Guid())
        {
            if (currentSelection == null || currentSelection.Count() < 1)
            {
                throw new ArgumentException("currentSelection is empty or null");
            }
            var inputs = currentSelection;

            if (presetStates.Any(x => x.Guid == id))
            {
                throw new ArgumentException("duplicate id in collection");
            }

            var newstate = new PresetState(name, description, inputs, id);
            presetStates.Add(newstate);
        }

        public void ImportStates(PresetsModel presetsCollection)
        {
            presetStates.AddRange(presetsCollection.PresetStates);
        }

        public void RemoveState(PresetState state)
        {
            if (PresetStates.Contains(state))
            {
                presetStates.Remove(state);
            }
        }

        #endregion

        #region ILogSource implementation
        public event Action<ILogMessage> MessageLogged;

        protected void Log(ILogMessage obj)
        {
            var handler = MessageLogged;
            if (handler != null) handler(obj);
        }

        protected void Log(string msg)
        {
            Log(LogMessage.Info(msg));
        }

        protected void Log(string msg, WarningLevel severity)
        {
            switch (severity)
            {
                case WarningLevel.Error:
                    Log(LogMessage.Error(msg));
                    break;
                default:
                    Log(LogMessage.Warning(msg, severity));
                    break;
            }
        }

        #endregion
    }
}

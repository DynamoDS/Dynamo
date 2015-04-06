using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Selection;
using Dynamo.Nodes;
using System.Xml;

namespace Dynamo.Models
{
    /// <summary>
    /// a class that holds a set of preset design options states
    /// there is one instance of this class per workspacemodel
    /// </summary>
    public class PresetsModel
    {
        #region private members
        private readonly List<PresetState> designStates;

        private void LoadStateFromXml(string name, string description, List<NodeModel> nodes, List<XmlElement> serializednodes, Guid id)
        {
            var loadedState = new PresetState(name, description, nodes, serializednodes, id);
            designStates.Add(loadedState);
        }
        #endregion

        # region properties
        public IEnumerable<PresetState> DesignStates { get { return designStates; } }
        #endregion

        #region constructor
        public PresetsModel()
        {
            designStates = new List<PresetState>();
        }
        #endregion

        #region serialization / deserialzation
        // we serialze the presets to xml like a model, but we deserialze them before the workspacemodel is constructed
        //during save and load of the graph, can just inject this into workspacemodel 
        //when a new designstate is created we'll serialize all the current nodes into a new xmlelement
        //but not actually write this xml to a file until the graph is saved

        //grabbed some methods needed from modelbase for serialization
        protected virtual XmlElement CreateElement(XmlDocument xmlDocument, SaveContext context)
        {
            string typeName = GetType().ToString();
            XmlElement element = xmlDocument.CreateElement(typeName);
            return element;
        }

        public XmlElement Serialize(XmlDocument xmlDocument, SaveContext context)
        {
            var element = CreateElement(xmlDocument, context);
            SerializeCore(element, context);
            return element;
        }

        protected virtual void SerializeCore(System.Xml.XmlElement element, SaveContext context)
        {

            foreach (var state in designStates)
            {
                var parent = element.OwnerDocument.CreateElement("PresetState");
                element.AppendChild(parent);
                parent.SetAttribute("Name", state.Name);
                parent.SetAttribute("Description", state.Description);
                parent.SetAttribute("guid", state.Guid.ToString());
                //the states are already serialized
                foreach (var serializedNode in state.SerializedNodes)
                {
                    //need to import the node to cross xml contexts
                    var importNode = parent.OwnerDocument.ImportNode(serializedNode, true);
                    parent.AppendChild(importNode);
                }

            }
        }

        internal static PresetsModel LoadFromXml(XmlDocument xmlDoc, NodeGraph nodegraph)
        {
            var loadedStateSet = new PresetsModel();

            //create a new state inside the set foreach state present in the xmldoc

            foreach (XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                if (element.Name == typeof(PresetsModel).ToString())
                {
                    foreach (XmlElement stateNode in element.ChildNodes)
                    {
                        var name = stateNode.GetAttribute("Name");
                        var des = stateNode.GetAttribute("Description");
                        var stateguidString = stateNode.GetAttribute("guid");

                        Guid stateID;
                        if (!Guid.TryParse(stateguidString, out stateID))
                        {
                            throw new Exception("unable to parse state GUID");
                        }

                        var nodes = new List<NodeModel>();
                        var deserialzedNodes = new List<XmlElement>();
                        //now find the nodes we're looking for by their guids in the loaded nodegraph
                        //it's possible they may no longer be present, and we must not fail to set the
                        //TODO//rest of the nodes but log this to the console.

                        //iterate each actual saved nodemodel in each state
                        foreach (XmlElement node in stateNode.ChildNodes)
                        {
                            var guidString = node.GetAttribute("guid");
                            Guid nodeID;
                            if (!Guid.TryParse(guidString, out nodeID))
                            {
                                throw new Exception("unable to parse GUID");
                            }

                            var nodename = stateNode.GetAttribute("nickname");
                            var nodebyGuid = nodegraph.Nodes.Where(x => x.GUID == nodeID).ToList();
                            if (nodebyGuid.Count > 0)
                            {
                                nodes.Add(nodebyGuid.First());
                                deserialzedNodes.Add(node);
                            }
                            else
                            {
                                //TODO possibly hookup to dynamologger
                                Console.WriteLine(nodename + nodeID.ToString() + " could not be found in the loaded dyn");
                            }

                        }

                        loadedStateSet.LoadStateFromXml(name, des, nodes, deserialzedNodes, stateID);
                    }
                }
            }
            return loadedStateSet;
        }
        #endregion

        #region public methods
        /// <summary>
        /// method to create and add a new state to this presets collection
        /// </summary>
        public void CreateNewState(string name, string description, IEnumerable<NodeModel> currentSelection, Guid id = new Guid())
        {
            var inputs = currentSelection;
            var newstate = new PresetState(name, description, inputs, id);
            designStates.Add(newstate);
        }

        public void RemoveState(PresetState state)
        {
            designStates.Remove(state);
        }

        #endregion

    }
}

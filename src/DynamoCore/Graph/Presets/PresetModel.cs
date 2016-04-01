using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;

namespace Dynamo.Graph.Presets
{
    /// <summary>
    /// This class references a set of nodemodels, and a set of serialized versions of those nodemodels
    /// a client can use this class to store the state of a set of nodes from a graph
    /// </summary>
    public class PresetModel:ModelBase
    {
       
        private List<NodeModel> nodes;
        private List<XmlElement> serializedNodes;

        # region properties

        public string Name { get; private set; }
        public string Description { get; private set; }
       
        /// <summary>
        /// list of nodemodels that this state serializes
        /// </summary>
        public IEnumerable<NodeModel> Nodes { get{return nodes;}}

        /// <summary>
        /// list of serialized nodes
        /// </summary>
        public IEnumerable<XmlElement> SerializedNodes { get { return serializedNodes; }}

       
        public const string GuidAttributeName = "guid";
        public const string NameAttributeName = "Name";
        public const string DescriptionAttributeName = "Description";
        public const string NicknameAttributeName = "nickname";

        #endregion

        #region constructor
        /// <summary>
        /// create a new presetsState, this will serialize all the referenced nodes by calling their serialize method, 
        /// the resulting XML elements will be used to save this state when the presetModel is saved on workspace save
        /// </summary>
        /// <param name="name">name for the state, must not be null </param>
        /// <param name="description">description of the state, can be null</param>
        /// <param name="inputsToSave">set of nodeModels, must not be null</param>
        public PresetModel(string name, string description, IEnumerable<NodeModel> inputsToSave):base()
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (inputsToSave == null || inputsToSave.Count() < 1)
            {
                throw new ArgumentNullException("inputsToSave");
            } 

           
            Name = name;
            Description = description;
            nodes = inputsToSave.ToList();

           
            var tempdoc = new XmlDocument();
            serializedNodes = new List<XmlElement>();
            foreach (var node in Nodes)
            {
                serializedNodes.Add(node.Serialize(tempdoc, SaveContext.Preset));
            }
        }
        
        //this overload is used for loading
        private PresetModel(string name, string description, List<NodeModel> nodes, List<XmlElement> serializedNodes, Guid id)
        {
            Name = name;
            Description = description;
            this.nodes = nodes;
            this.serializedNodes = serializedNodes;
            GUID = id;
        }

        /// <summary>
        /// this overload is used for loading with deserializeCore, we must pass the nodesInTheGraph to the instance of the Preset so that
        /// we can detect missing nodes
        /// </summary>
        /// <param name="nodesInGraph"></param>
 
        internal PresetModel(IEnumerable<NodeModel> nodesInGraph)
        {
            this.nodes = nodesInGraph.ToList();
        }
        #endregion



        #region serialization / deserialzation

        protected override void SerializeCore(System.Xml.XmlElement element, SaveContext context)
        {
            element.SetAttribute(NameAttributeName, this.Name);
            element.SetAttribute(DescriptionAttributeName, this.Description);
            element.SetAttribute(GuidAttributeName, this.GUID.ToString());
            //the states are already serialized
            foreach (var serializedNode in this.SerializedNodes)
            {
                //need to import the node to cross xml contexts
                var importNode = element.OwnerDocument.ImportNode(serializedNode, true);
                element.AppendChild(importNode);
            }
        }

        
        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            var stateName = nodeElement.GetAttribute(NameAttributeName);
            var stateguidString = nodeElement.GetAttribute(GuidAttributeName);
            var stateDescription = nodeElement.GetAttribute(DescriptionAttributeName);

            Guid stateID;
            if (!Guid.TryParse(stateguidString, out stateID))
            {
                this.Log("unable to parse the GUID for preset state: " + stateName + ", will atttempt to load this state anyway");
            }

            var foundNodes = new List<NodeModel>();
            var deserialzedNodes = new List<XmlElement>();
            //now find the nodes we're looking for by their guids in the loaded nodegraph
            //it's possible they may no longer be present, and we must not fail to set the
            //rest of the serialized versions
            //iterate each actual saved nodemodel in the state
            foreach (XmlElement node in nodeElement.ChildNodes)
            {
                var nodename = node.GetAttribute(NicknameAttributeName);
                var guidString = node.GetAttribute(GuidAttributeName);
                Guid nodeID;
                if (!Guid.TryParse(guidString, out nodeID))
                {
                    this.Log("unable to parse GUID for node " + nodename);
                    continue;
                }

                var nodebyGuid = this.nodes.Where(x => x.GUID == nodeID);
                if (nodebyGuid.Count() > 0)
                {
                    foundNodes.Add(nodebyGuid.First());
                    deserialzedNodes.Add(node);
                }
                else
                {   //add the deserialized version anyway so we dont lose this node from all states.
                    deserialzedNodes.Add(node);
                    this.Log(nodename + nodeID.ToString() + " could not be found in the loaded .dyn");
                }
            }

            this.Name = stateName;
            this.GUID = stateID;
            this.Description = stateDescription;
            this.serializedNodes = deserialzedNodes;
            //at the time of deserialization, nodes contains all the nodes in the nodegraph
            //we now replace it with the found nodes that this preset serializes
            this.nodes = foundNodes;
        }
        #endregion
    }
}


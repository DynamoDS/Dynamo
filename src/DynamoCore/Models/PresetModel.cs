using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Services;
using System.Xml;
using Dynamo.Nodes;
using Dynamo.Interfaces;

namespace Dynamo.Models
{
    /// <summary>
    /// This class references a set of nodemodels, and a set of serialized versions of those nodemodels
    /// a client can use this class to store the state of a set of nodes from a graph
    /// </summary>
    public class PresetModel:ModelBase
    {
        
        private readonly List<NodeModel> nodes;
        private readonly List<XmlElement> serializedNodes;

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
        /// the below temp root and doc is to avoid exceptions thrown by the zero touch serialization methods
        /// </summary>
        /// <param name="name">name for the state, must not be null </param>
        /// <param name="description">description of the state, can be null</param>
        /// <param name="inputsToSave">set of nodeModels, must not be null</param>
        /// <param name="id">an id GUID, can be empty GUID</param>
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

        /// <summary>
        /// this method will never be called for a presetModel but needs to exist for modelbase
        /// </summary>
        /// <param name="nodeElement"></param>
        /// <param name="context"></param>
        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            throw new NotImplementedException();
        }

        public static PresetModel LoadFromXml(XmlElement element,IEnumerable<NodeModel> nodesInNodeGraph,ILogger logger)
        {
            return loadFromXmlCore(element, nodesInNodeGraph, logger);
        }


         protected static PresetModel loadFromXmlCore(XmlElement element,IEnumerable<NodeModel> nodesInNodeGraph,ILogger logger)
      {
          var stateName = element.GetAttribute(NameAttributeName);
          var stateguidString = element.GetAttribute(GuidAttributeName);
          var stateDescription = element.GetAttribute(DescriptionAttributeName);

          Guid stateID;
          if (!Guid.TryParse(stateguidString, out stateID))
          {
              logger.LogError("unable to parse the GUID for preset state: " + stateName + ", will atttempt to load this state anyway");
          }

          var nodes = new List<NodeModel>();
          var deserialzedNodes = new List<XmlElement>();
          //now find the nodes we're looking for by their guids in the loaded nodegraph
          //it's possible they may no longer be present, and we must not fail to set the

          //iterate each actual saved nodemodel in the state
          foreach (XmlElement node in element.ChildNodes)
          {
              var nodename = node.GetAttribute(NicknameAttributeName);
              var guidString = node.GetAttribute(GuidAttributeName);
              Guid nodeID;
              if (!Guid.TryParse(guidString, out nodeID))
              {
                  logger.LogError("unable to parse GUID for node " + nodename);
                  continue;
              }

              var nodebyGuid = nodesInNodeGraph.Where(x => x.GUID == nodeID).ToList();
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
          return new PresetModel(stateName, stateDescription, nodes, deserialzedNodes, stateID);

        }

        #endregion
    }
}


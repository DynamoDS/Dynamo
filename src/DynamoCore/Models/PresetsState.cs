using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Services;
using System.Xml;
using Dynamo.Nodes;

namespace Dynamo.Models
{
    /// <summary>
    /// a class that saves the state of a subset of a graph
    /// </summary>
    public class PresetState
    {
        private Guid guid;
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

        /// <summary>
        /// A unique identifier for the state.
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
        }

        #endregion

        #region constructor
        public PresetState(string name, string description, IEnumerable<NodeModel> inputsToSave, Guid id)
        {
            //if we have not supplied a guid at then create a new one
            if (id == Guid.Empty)
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = id;
            }
            
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("preset options state name is null");
            }

            if (inputsToSave == null || inputsToSave.Count() < 1)
            {
                throw new ArgumentNullException("nodes to save are null null");
            } 
  
            Name = name;
            Description = description;
            nodes = inputsToSave.ToList();
            
            // serialize all the nodes by calling their serialize method, 
            // the resulting elements will be used to save this state when 
            // the presetModel is saved on graph save
            // the below temp root and doc is to avoid a exceptions thrown by the zero touch serialization methods
            var tempdoc = new XmlDocument();
            var root = tempdoc.CreateElement("temproot");
            tempdoc.AppendChild(root);
            Dynamo.Nodes.Utilities.SetDocumentXmlPath(tempdoc,"C:/tempdoc" );
            serializedNodes = new List<XmlElement>();
            foreach (var node in Nodes)
            {
                serializedNodes.Add(node.Serialize(tempdoc, SaveContext.File));
            }
        }

        public PresetState(string name, string description, List<NodeModel> nodes, List<XmlElement> serializedNodes, Guid id)
        {
            //TODO null checks
            Name = name;
            Description = description;
            this.nodes = nodes;
            this.serializedNodes = serializedNodes;
            guid = id;
        }
        #endregion
    }
}


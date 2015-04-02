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
    /// a class that saves the state of a graph
    /// </summary>
    public class DesignOptionsState
    {
        private Guid guid;
        private readonly List<NodeModel> nodes;
        private readonly List<XmlElement> serializedNodes;

        # region properties

        public string Name { get; set; }
        public string Description { get; set; }
       
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
        public DesignOptionsState(string name, string description, IEnumerable<NodeModel> inputsToSave, Guid id)
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
                throw new ArgumentNullException("design options state name is null");
            }
            if (inputsToSave == null || inputsToSave.Count() < 1)
            {
                throw new ArgumentNullException("nodes to save are null null");
            }   
            Name = name;
            Description = description;
            nodes = inputsToSave.ToList(); ;
            
            // serialize all the nodes by calling their serialize method, 
            // the resulting elements will be used to save this state when 
            // the designOptionsSet is saved on graph save
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

        public DesignOptionsState(string name, string description, List<NodeModel> nodes, List<XmlElement> serializedNodes, Guid id)
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


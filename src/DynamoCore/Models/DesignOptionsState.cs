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

        # region properties

        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// list of nodemodels that this state serializes
        /// </summary>
        public List<NodeModel> Nodes { get; set; }

        /// <summary>
        /// list of serialized nodes
        /// </summary>
        public List<XmlElement> SerializedNodes { get; set; }

        /// <summary>
        /// A unique identifier for the state.
        /// </summary>
        public Guid Guid
        {
            get { return guid; }
        }

        #endregion

        #region constructor
        public DesignOptionsState(string name, string description, List<NodeModel> inputsToSave, Guid id)
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
            if (inputsToSave == null || inputsToSave.Count < 1)
            {
                throw new ArgumentNullException("nodes to save are null null");
            }   
            Name = name;
            Description = description;
            Nodes = inputsToSave;
            
            // serialize all the nodes by calling their serialize method, 
            // the resulting elements will be used to save this state when 
            // the designOptionsSet is saved on graph save
            var tempdoc = new XmlDocument();
            var root = tempdoc.CreateElement("temproot");
            tempdoc.AppendChild(root);
            Dynamo.Nodes.Utilities.SetDocumentXmlPath(tempdoc,"C:/tempdoc" );
            SerializedNodes = new List<XmlElement>();
            foreach (var node in Nodes)
            {
                SerializedNodes.Add(node.Serialize(tempdoc, SaveContext.File));
            }
        }

        public DesignOptionsState(string name, string description, List<NodeModel> nodes, List<XmlElement> serializedNodes, Guid id)
        {
            //TODO null checks
            Name = name;
            Description = description;
            Nodes = nodes;
            SerializedNodes = serializedNodes;
            guid = id;
        }
        #endregion
    }
}


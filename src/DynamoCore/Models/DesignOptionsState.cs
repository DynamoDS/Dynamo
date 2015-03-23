using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Core;
using Dynamo.Services;
using System.Xml;

namespace Dynamo.Models
{
    /// <summary>
    /// a class that saves the state of a graph
    /// </summary>
    public class DesignOptionsState
    {
      
        # region properties
        public string Name { get; set; }
        public string Description { get; set; }
        public List<NodeModel> Nodes { get; set; }
        public List<XmlElement> SerializedNodes { get; set; }

        #endregion

        #region constructor
        public DesignOptionsState(string name, string description, List<NodeModel> inputsToSave)
        {
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
            SerializedNodes = new List<XmlElement>();
            foreach (var node in Nodes)
            {
                SerializedNodes.Add(node.Serialize(tempdoc, SaveContext.File));
            }
        }
        #endregion
    }
}


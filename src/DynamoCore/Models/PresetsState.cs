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
    /// This class references a set of nodemodels, and a set of serialized versions of those nodemodels
    /// a client can use this class to store the state of a set of nodes from a graph
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
        /// <summary>
        /// create a new presetsState, this will serialize all the referenced nodes by calling their serialize method, 
        /// the resulting XML elements will be used to save this state when the presetModel is saved on workspace save
        /// the below temp root and doc is to avoid exceptions thrown by the zero touch serialization methods
        /// </summary>
        /// <param name="name">name for the state, must not be null </param>
        /// <param name="description">description of the state, can be null</param>
        /// <param name="inputsToSave">set of nodeModels, must not be null</param>
        /// <param name="id">an id GUID, can be empty GUID</param>
        public PresetState(string name, string description, IEnumerable<NodeModel> inputsToSave, Guid id)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (inputsToSave == null || inputsToSave.Count() < 1)
            {
                throw new ArgumentNullException("inputsToSave");
            } 

            //if we have not supplied a guid at construction then create a new one
            if (id == Guid.Empty)
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = id;
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
        public PresetState(string name, string description, List<NodeModel> nodes, List<XmlElement> serializedNodes, Guid id)
        {
            
            Name = name;
            Description = description;
            this.nodes = nodes;
            this.serializedNodes = serializedNodes;
            //if we have not supplied a guid at load then create a new one
            if (id == Guid.Empty)
            {
                guid = Guid.NewGuid();
            }
            else
            {
                guid = id;
            }
            
        }
        #endregion
    }
}


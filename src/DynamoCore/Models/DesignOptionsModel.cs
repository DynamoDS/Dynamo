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
    /// a class that holds a set of design options states
    /// there is one instance of this class per workspacemodel
    /// </summary>
    public class DesignOptionsSetModel : ModelBase
    {
        #region private members
        private List<DesignOptionsState> designStates;
        //private List<String> supportedInputTypes =new List<String>(){("DoubleSlider")};
        #endregion

        # region properties
        public List<DesignOptionsState> DesignStates { get { return designStates;} }
        #endregion

        #region constructor
        public DesignOptionsSetModel()
        {
            designStates = new List<DesignOptionsState>();
        }
        #endregion

        #region serialization / deserialzation
        //current idea is that we serialze and and deserialze the the designoptions set
        //during save and load of the graph, can just inject this into workspacemodel 

        //when a new designstate is created we'll serialize all the current nodes into a new xmlelement
        //but not actually write this xml to a file until the graph is saved
        protected override void SerializeCore(System.Xml.XmlElement element, SaveContext context)
        {

            foreach (var state in designStates)
            {
                var parent = element.OwnerDocument.CreateElement("DesignOptionsState");
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

        protected override void DeserializeCore(System.Xml.XmlElement nodeElement, SaveContext context)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region public methods
        /// <summary>
        /// method to create and add a new state to this design options set
        /// </summary>
        public void CreateNewState(string name, string description, List<NodeModel> currentSelection,Guid id = new Guid())
        {
            //TODO filter current selection down to inputs before passing it on
            //var inputs = currentSelection.Where(x=>supportedInputTypes.Contains(x.GetType())).ToList();
            var inputs = currentSelection;
            //then create a new designstate and store it in the dictionary looked up via it's name
            //TODO should use a GUID instead?
            var newstate = new DesignOptionsState(name, description, inputs,id);
            designStates.Add(newstate);
        }
        public void LoadStateFromXml(string name, string description, List<NodeModel> nodes, List<XmlElement> serializednodes, Guid id)
        {

            var loadedState = new DesignOptionsState(name, description, nodes, serializednodes, id);
            designStates.Add(loadedState);
        }
      
        #endregion




        internal static DesignOptionsSetModel LoadFromXml(XmlDocument xmlDoc, NodeGraph nodegraph)
        {
            var loadedStateSet = new DesignOptionsSetModel();

            //create a new state inside the set foreach state present in the xmldoc

            foreach (XmlElement element in xmlDoc.DocumentElement.ChildNodes)
            {
                if (element.Name == typeof(DesignOptionsSetModel).ToString())
                {
                    foreach (XmlElement stateNode in element.ChildNodes)
                    {
                        var name = stateNode.GetAttribute("Name");
                        var des = stateNode.GetAttribute("Description");
                        var stateguidString = stateNode.GetAttribute("guid");

                        Guid stateID;
                        if (Guid.TryParse(stateguidString, out stateID))
                        {

                        }
                        else
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
                            if (Guid.TryParse(guidString, out nodeID))
                            {

                            }
                            else
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

                 loadedStateSet.LoadStateFromXml(name, des, nodes,deserialzedNodes,stateID);
                    }
                }
            }
            return loadedStateSet;
        }

    }
}

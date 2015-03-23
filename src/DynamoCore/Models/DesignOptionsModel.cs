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
        public void CreateNewState(string name, string description,List<NodeModel> currentSelection)
        {
          //TODO filter current selection down to inputs before passing it on
            //var inputs = currentSelection.Where(x=>supportedInputTypes.Contains(x.GetType())).ToList();
            var inputs = currentSelection;
          //then create a new designstate and store it in the dictionary looked up via it's name
          //TODO should use a GUID instead?
            var newstate = new DesignOptionsState(name, description, inputs);
            designStates.Add(newstate);
        }
        #endregion


       
    }
}

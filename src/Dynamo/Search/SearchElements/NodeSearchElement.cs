//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Windows;
using Dynamo.Commands;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Microsoft.Practices.Prism.Commands;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A search element representing a local node </summary>
    public class NodeSearchElement : SearchElementBase
    {

        #region Properties

        /// <summary>
        /// Guid property </summary>
        /// <value>
        /// The guid used to reference a dynFunction </value>
        public Guid Guid { get; internal set; }

        /// <summary>
        /// Node property </summary>
        /// <value>
        /// The node used to instantiate this object </value>
        public dynNodeModel Node { get; internal set; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        private string _type;
        public override string Type { get { return _type; } }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name { get { return _name; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description { get { return _description; } }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override double Weight { get; set; }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override string Keywords { get; set; }

        /// <summary>
        /// Whether the description of this node should be visible or not
        /// </summary>
        private Visibility _descriptionVisibility = Visibility.Collapsed;
        public Visibility DescriptionVisibility
        {
            get { return _descriptionVisibility; }
            set
            {
                _descriptionVisibility = value;
                RaisePropertyChanged("DescriptionVisibility");
            }
        }

        /// <summary>
        /// Command for toggling visibility
        /// </summary>
        public DelegateCommand ToggleDescriptionVisibilityCommand { get; set; }

        #endregion

        /// <summary>
        /// The class constructor for a built-in type that is already loaded. </summary>
        /// <param name="node">The local node</param>
        public NodeSearchElement(dynNodeModel node)
        {
            ToggleDescriptionVisibilityCommand = new DelegateCommand(ToggleIsVisible);
            this.Node = node;
            this._name = Node.NickName;
            this.Weight = 1;
            this.Keywords = String.Join(" ", node.Tags);
            this._type = "Node";
            this._description = node.Description;
        }

        /// <summary>
        ///     The class constructor - use this constructor for built-in types\
        ///     that are not yet loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="tags"></param>
        public NodeSearchElement(string name, string description, List<string> tags)
        {
            ToggleDescriptionVisibilityCommand = new DelegateCommand(ToggleIsVisible);
            this.Node = null;
            this._name = name;
            this.Weight = 1;
            this.Keywords = String.Join(" ", tags);
            this._type = "Node";
            this._description = description;
        }



        /// <summary>
        ///     The class constructor - use this constructor when for
        ///     custom nodes
        /// </summary>
        /// <param name="name">The name of the custom node</param>
        /// <param name="guid">The unique id for the custom node</param>
        public NodeSearchElement(string name, Guid guid)
        {
            ToggleDescriptionVisibilityCommand = new DelegateCommand(ToggleIsVisible);
            this.Node = null;
            this._name = name;
            this.Weight = 0.9;
            this.Keywords = "";
            this._type = "Custom Node";
            this.Guid = guid;
            this._description = "";
        }

        /// <summary>
        ///     The class constructor - use this constructor when for
        ///     custom nodes
        /// </summary>
        /// <param name="funcDef">The FunctionDefinition for a custom node</param>
        public NodeSearchElement(FunctionDefinition funcDef)
        {
            ToggleDescriptionVisibilityCommand = new DelegateCommand(ToggleIsVisible);
            this.Node = dynSettings.Controller.DynamoViewModel.CreateNode(funcDef.FunctionId.ToString());
            this._name = funcDef.Workspace.Name;
            this.Weight = 1.1;
            this.Keywords = "";
            this._description = "Custom Node";
            this._type = "Custom Node";
        }

        private void ToggleIsVisible()
        {
            if (this.DescriptionVisibility != Visibility.Visible)
            {
                this.DescriptionVisibility = Visibility.Visible;
            }
            else
            {
                this.DescriptionVisibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            //dynSettings.Controller.SearchViewModel.Visible = Visibility.Collapsed;
            string name;

            if (this.Node != null && this.Node is dynFunction)
            {
                name = ((dynFunction)Node).Definition.FunctionId.ToString();
            } 
            else if (this.Guid != Guid.Empty && this._type == "Custom Node") 
            {
                name = this.Guid.ToString();
            }
            else
            {
                name = Name;
            }

            // create node
            var guid = Guid.NewGuid();
            var nodeParams = new Dictionary<string, object>()
                {
                    {"name", name},
                    {"transformFromOuterCanvasCoordinates", true},
                    {"guid", guid}
                };
            dynSettings.Controller.CommandQueue.Enqueue(Tuple.Create<object, object>(dynSettings.Controller.DynamoViewModel.CreateNodeCommand, nodeParams));
            dynSettings.Controller.ProcessCommandQueue();

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                dynSettings.Controller.OnRequestSelect(this, new ModelEventArgs(placedNode, null));
            }
        }

    }

}

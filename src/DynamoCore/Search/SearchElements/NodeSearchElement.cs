using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using String = System.String;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Search.SearchElements
{

    /// <summary>
    /// A search element representing a local node </summary>
    public partial class NodeSearchElement : SearchElementBase, IEquatable<NodeSearchElement>
    {
        private string _fullName ;

        #region Properties

        /// <summary>
        /// Node property </summary>
        /// <value>
        /// The node used to instantiate this object </value>
        public NodeModel Node { get; internal set; }

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

        private bool _searchable = true;
        public override bool Searchable { get { return _searchable; } }

        public void SetSearchable(bool s)
        {
            _searchable = s;
        }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override sealed double Weight { get; set; }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override sealed string Keywords { get; set; }

        /// <summary>
        /// Whether the description of this node should be visible or not
        /// </summary>
        private bool _descriptionVisibility = false;
        public bool DescriptionVisibility
        {
            get { return _descriptionVisibility; }
            set
            {
                _descriptionVisibility = value;
                RaisePropertyChanged("DescriptionVisibility");
            }
        }

        #endregion

        /// <summary>
        ///     The class constructor - use this constructor for built-in types\
        ///     that are not yet loaded.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="tags"></param>
        /// <param name="fullName"></param>
        public NodeSearchElement(string name, string description, IEnumerable<string> tags, string fullName = "")
        {
            this.Node = null;
            this._name = name;
            this.Weight = 1;
            this.Keywords = String.Join(" ", tags);
            this._type = "Node";
            this._description = description;
            this._fullName = fullName;
        }

        public virtual NodeSearchElement Copy()
        {
            var f = new NodeSearchElement(this.Name, this.Description, new List<string>(), this._fullName);
            f.FullCategoryName = this.FullCategoryName;
            return f;
        }

        private void ToggleIsVisible(object parameter)
        {
            if (this.DescriptionVisibility != true)
            {
                this.DescriptionVisibility = true;
            }
            else
            {
                this.DescriptionVisibility = false;
            }
        }

        /// <summary>
        /// Executes the element in search, this is what happens when the user 
        /// hits enter in the SearchView.</summary>
        public override void Execute()
        {
            // create node
            var guid = Guid.NewGuid();
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.CreateNodeCommand(guid, this._fullName, 0, 0, true, true));

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals(obj as NodeSearchElement);
        }

        /// <summary>
        /// Overriding equals, we need to override hashcode </summary>
        /// <returns> A unique hashcode for the object </returns>
        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + this.Name.GetHashCode() + this.Description.GetHashCode();
        }

        public bool Equals(NodeSearchElement other)
        {
            return this.Name == other.Name && this.FullCategoryName == other.FullCategoryName;
        }
    }

}
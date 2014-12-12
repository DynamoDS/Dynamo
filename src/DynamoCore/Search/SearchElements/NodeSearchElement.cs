using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using String = System.String;

namespace Dynamo.Search.SearchElements
{

    /// <summary>
    /// A search element representing a local node </summary>
    public partial class NodeSearchElement : SearchElementBase, IEquatable<NodeSearchElement>
    {

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

        private string _fullName;
        public string FullName { get { return _fullName; } }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description 
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                    return Dynamo.UI.Configurations.NoDescriptionAvailable;

                return _description;
            } 
        }


        public bool HasDescription
        {
            get { return (!string.IsNullOrEmpty(_description)); }
        }

        /// <summary>
        /// Group property </summary>
        /// <value>
        /// Group to which Node belongs to</value>
        private SearchElementGroup _group;
        public SearchElementGroup Group { get { return _group; } }

        /// <summary>
        /// Property specifies how Node was created.
        /// </summary>
        public SearchModel.ElementType ElementType { get; set; }

        private List<Tuple<string, string>> _inputParameters;
        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get
            {
                if (_inputParameters == null)
                {
                    _inputParameters = GenerateInputParameters();
                }
                return _inputParameters;
            }
        }

        private List<string> _outputParameters = new List<String>();

        public List<string> OutputParameters
        {
            get
            {
                return GenerateOutputParameters();
            }
        }

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
        public NodeSearchElement(string name, string description,
                                 IEnumerable<string> tags, SearchElementGroup group,
                                 string fullName = "", string _assembly = "",
                                 IEnumerable<Tuple<string, string>> inputParameters = null,
                                 List<string> outputParameters = null)
        {
            this.Node = null;
            this._name = name;
            this.Weight = 1;
            this.Keywords = String.Join(" ", tags);
            this._type = "Node";
            this._description = description;
            this._fullName = fullName;
            this._group = group;
            if (inputParameters != null)
                this._inputParameters = inputParameters.ToList();
            this._outputParameters = outputParameters;
            this.Assembly = _assembly;
        }

        public virtual NodeSearchElement Copy()
        {
            var f = new NodeSearchElement(this.Name, this.Description, new List<string>(),
                                          this._group, this._fullName, this.Assembly,
                                          this._inputParameters, this._outputParameters);
            f.FullCategoryName = this.FullCategoryName;
            f.ElementType = this.ElementType;
            return f;
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

        /// <summary>
        /// This method is called to obtain the resource name for this NodeSearchElement.
        /// Typical NodeSearchElement includes 'ColorRange' or 'File.Directory'. Since these 
        /// elements do not have overloads, the parameter 'disambiguate' is not checked.
        /// </summary>
        protected override string GetResourceName(ResourceType resourceType, bool disambiguate = false)
        {
            switch (resourceType)
            {
                case ResourceType.SmallIcon: return this._fullName;
                case ResourceType.LargeIcon: return this._fullName;
            }

            throw new InvalidOperationException("Unhandled resourceType");
        }

        protected virtual List<string> GenerateOutputParameters()
        {
            if (_outputParameters == null)
            {
                _outputParameters = new List<String>();
                _outputParameters.Add("none");
            }
            return _outputParameters;
        }


        protected virtual List<Tuple<string, string>> GenerateInputParameters()
        {
            List<Tuple<string, string>> inputPar = new List<Tuple<string, string>>();
            inputPar.Add(Tuple.Create("", "none"));
            return inputPar;
        }
    }
}

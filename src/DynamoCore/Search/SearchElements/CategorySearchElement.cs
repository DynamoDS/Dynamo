namespace Dynamo.Search.SearchElements
{
    public class CategorySearchElement : SearchElementBase
    {
        #region Properties
        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public override string Type
        {
            get { return "Category"; }
        }

        /// <summary>
        /// Name property </summary>
        /// <value>
        /// The name of the node </value>
        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        private string _description;
        public override string Description
        {
            get { return _description; }
        }

        public override bool Searchable { get { return true; } }

        /// <summary>
        /// Keywords property </summary>
        /// <value>
        /// Joined set of keywords </value>
        public override string Keywords { get; set; }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search.  Higher weight means closer to the top. </value>
        public override double Weight { get; set; }


        /// <summary>
        /// NumElements property </summary>
        /// <value>
        /// The number of elements in the category.  Setting also updates the description.
        /// </value>
        private int _numElements;
        public int NumElements
        {
            get
            {
                return _numElements;
            }
            set
            {
                this._description = value + " nodes";
                _numElements = value;
            }
        }

        #endregion

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="name">The name of the namespace (e.g. BestTeamsAtAutodesk.Magneto ) </param>
        public CategorySearchElement(string name)
        {
            _name = name;
            Weight = 1.2;
            Keywords = "";
            NumElements = 0;
            _description = "";
        }
    }
}
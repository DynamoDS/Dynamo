using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    class CategorySearchElement : SearchElementBase
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
        public override string Description
        {
            get { return "Category"; }
        }

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
        #endregion

        /// <summary>
        /// The class constructor. </summary>
        /// <param name="name">The name of the namespace (e.g. BestTeamsAtAutodesk.Magneto ) </param>
        public CategorySearchElement(string name)
        {
            _name = name + ".";
            Weight = 1.2;
            Keywords = "";
        }
        
        /// <summary>
        /// Add the name as the current search text </summary>
        public override void Execute()
        {
            dynSettings.Controller.SearchViewModel.SearchText = Name;
        }

    }
}

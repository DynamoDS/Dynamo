using System;

namespace Dynamo.Search.SearchElements

{
    /// <summary>
    /// A base class for elements found in search </summary>
    public abstract class SearchElementBase : BrowserInternalElement
    {
        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public virtual string CreationName { get { return this.Name; } }

        /// <summary>
        /// Searchable property </summary>
        /// <value>
        /// A bool indicating if the object will appear in searches </value>
        public abstract bool Searchable { get; }

        /// <summary>
        /// Type property </summary>
        /// <value>
        /// A string describing the type of object </value>
        public abstract string Type { get; }

        /// <summary>
        /// Description property </summary>
        /// <value>
        /// A string describing what the node does</value>
        public abstract string Description { get; }

        /// <summary>
        /// Keywords property</summary>
        /// <value>
        /// A set of keywords for the object, joined by spaces</value>
        public abstract string Keywords { get; set; }

        /// <summary>
        /// Weight property </summary>
        /// <value>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results </value>
        public abstract double Weight { get; set; }

        public virtual void Execute()
        {
            this.OnExecuted();
        }

        /// <summary>
        /// Delegate is used in Executed event.
        /// </summary>
        /// <param name="ele">search element</param>
        public delegate void SearchElementHandler(SearchElementBase ele);
        internal event SearchElementHandler Executed;
        protected void OnExecuted()
        {
            if (Executed != null)
            {
                Executed(this);
            }
        }
    }
}

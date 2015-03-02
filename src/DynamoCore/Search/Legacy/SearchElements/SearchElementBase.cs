using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Dynamo.Models;
using Dynamo.Nodes;

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

    /// <summary>
    /// A simple version of the SearchElementBase class needed for sending data to a web client
    /// </summary>
    [DataContract]
    public class LibraryItem
    {
        /// <summary>
        /// Full category name
        /// </summary>
        [DataMember]
        public string Category { get; private set; }

        /// <summary>
        /// Model name in the list of all node models
        /// </summary>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Unique name that is used during node creation
        /// </summary>
        [DataMember]
        public string CreationName { get; private set; }

        /// <summary>
        /// The name that will be displayed on node itself 
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// A string describing what the node does
        /// </summary>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Number defining the relative importance of the element in search. 
        /// Higher = closer to the top of search results
        /// </summary>
        [DataMember]
        public double Weight { get; private set; }

        /// <summary>
        /// This property represents the list of words used for element search.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Keywords { get; set; }

        /// <summary>
        /// This property represents the list of node inputs.
        /// </summary>
        [DataMember]
        public IEnumerable<PortInfo> Parameters { get; set; }

        /// <summary>
        /// This property represents the list of node outputs.
        /// </summary>
        [DataMember]
        public IEnumerable<PortInfo> ReturnKeys { get; set; }


        public LibraryItem(NodeSearchElement node)
        {
            Category = node.FullCategoryName;
            DisplayName = Name = node.Name;
            CreationName = node.CreationName;
            Description = node.Description;
            Weight = node.Weight;
        }

        public struct PortInfo
        {
            /// <summary>
            /// This property represents displayed name of the port.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// This property represents the type of the port. It can contain type name such
            /// as 'String', 'Point', 'Vertex' and other.
            /// It doesn't care about exact type, value can be 'Autodesk.DesignScript.Geometry.Point'
            /// or just 'Point' as well.
            /// This property only help users to understand what kind of value does the port expects.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// This property represents the default value for each of the input parameters.
            /// It can potentially contain primitive value types such as 'int', 'double', 'bool'
            /// and 'string'. If no default value is given to an input parameter, then it has 
            /// the corresponding entry in 'DefaultValues' as 'null'.
            /// </summary>
            public object DefaultValue { get; set; }
        }
    }
}

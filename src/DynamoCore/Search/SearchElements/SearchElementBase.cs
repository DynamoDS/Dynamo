using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Windows.Input;

using Dynamo.DSEngine;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Nodes.Search;
using Dynamo.Utilities;
using System.Runtime.Serialization;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    /// A base class for elements found in search </summary>
    public abstract class SearchElementBase : BrowserInternalElement
    {
        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public virtual string CreatingName { get { return this.Name; } }

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
    public class LibraryItem
    {
        [DataMember]
        public string Category { get; private set; }

        [DataMember]
        public string Type { get; private set; }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string CreatingName { get; private set; }

        [DataMember]
        public string Description { get; private set; }

        [DataMember]
        public bool Searchable { get; private set; }

        [DataMember]
        public double Weight { get; private set; }

        [DataMember]
        public string Keywords { get; private set; }

        [DataMember]
        public IEnumerable<string> Parameters { get; private set; }

        [DataMember]
        public IEnumerable<string> ReturnKeys { get; private set; }
        
        public LibraryItem(SearchElementBase node)
        {
            Category = node.FullCategoryName;
            Type = node.Type;
            Name = node.Name;
            CreatingName = node.CreatingName;
            Description = node.Description;
            Searchable = node.Searchable;
            Weight = node.Weight;
            Keywords = node.Keywords;

            PopulateKeysAndParameters();
        }

        private void PopulateKeysAndParameters()
        {
            var controller = dynSettings.Controller.EngineController;
            var functionItem = (controller.GetFunctionDescriptor(CreatingName));
            if (functionItem != null)
            {
                Parameters = functionItem.Parameters.Select(elem => elem.Name);

                if (functionItem.ReturnKeys != null && functionItem.ReturnKeys.Any())
                {
                    ReturnKeys = functionItem.ReturnKeys;
                }
                else
                {
                    ReturnKeys = new[] { functionItem.Type == FunctionType.Constructor ? functionItem.UnqualifedClassName : functionItem.ReturnType };
                }
            }
            else
            {
                TypeLoadData tld = null;
                
                if (dynSettings.Controller.BuiltInTypesByName.ContainsKey(CreatingName))
                {
                    tld = dynSettings.Controller.BuiltInTypesByName[CreatingName];
                }
                else if (dynSettings.Controller.BuiltInTypesByNickname.ContainsKey(CreatingName))
                {
                    tld = dynSettings.Controller.BuiltInTypesByNickname[CreatingName];
                }

                if (tld != null)
                {
                    ObjectHandle obj = Activator.CreateInstanceFrom(tld.Assembly.Location, tld.Type.FullName);
                    var newEl = (NodeModel)obj.Unwrap();

                    Parameters = newEl.InPorts.Select(elem => elem.PortName);
                    ReturnKeys = newEl.OutPorts.Select(elem => elem.PortName);
                }
                else
                {
                    Parameters = new[] { "Input" };
                    ReturnKeys = new[] { "Output" };
                }
            }
        }
    }
}

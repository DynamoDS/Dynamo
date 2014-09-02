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
        public string DisplayedName { get; private set; }

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
        
        public LibraryItem(SearchElementBase node, DynamoModel dynamoModel)
        {
            Category = node.FullCategoryName;
            Type = node.Type;
            DisplayedName = Name = node.Name;
            CreatingName = node.CreatingName;
            Description = node.Description;
            Searchable = node.Searchable;
            Weight = node.Weight;
            Keywords = node.Keywords;

            PopulateKeysAndParameters(dynamoModel);
        }

        private void PopulateKeysAndParameters(DynamoModel dynamoModel)
        {
            var controller = dynamoModel.EngineController;
            var functionItem = (controller.GetFunctionDescriptor(CreatingName));
            NodeModel newElement = null;
            if (functionItem != null)
            {
                DisplayedName = functionItem.DisplayName;
                if (functionItem.IsVarArg)
                    newElement = new DSVarArgFunction(dynamoModel.CurrentWorkspace, functionItem);
                else
                    newElement = new DSFunction(dynamoModel.CurrentWorkspace, functionItem);
            }
            else
            {
                TypeLoadData tld = null;

                if (dynamoModel.BuiltInTypesByName.ContainsKey(CreatingName))
                {
                    tld = dynamoModel.BuiltInTypesByName[CreatingName];
                }
                else if (dynamoModel.BuiltInTypesByNickname.ContainsKey(CreatingName))
                {
                    tld = dynamoModel.BuiltInTypesByNickname[CreatingName];
                }

                if (tld != null)
                {
                    newElement = (NodeModel)Activator.CreateInstance(tld.Type, dynamoModel.CurrentWorkspace);
                }
            }

            if (newElement != null)
            {
                Parameters = newElement.InPorts.Select(elem => elem.PortName);
                ReturnKeys = newElement.OutPorts.Select(elem => elem.PortName);
            }
            else
            {
                Parameters = new[] { "Input" };
                ReturnKeys = new[] { "Output" };
            }
        }
    }
}

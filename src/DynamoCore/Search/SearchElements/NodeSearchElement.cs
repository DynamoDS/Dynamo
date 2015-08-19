using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dynamo.Annotations;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Search.Interfaces;
using Dynamo.UI;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Base class for all Dynamo Node search elements.
    /// </summary>
    public abstract class NodeSearchElement : ISearchEntry, ISource<NodeModel>
    {
        protected string iconName;

        private readonly HashSet<string> keywords = new HashSet<string>();
        protected readonly List<double> keywordWeights = new List<double>();
        private string description;
        private string userFriendlyName;
        private SearchElementGroup group;
        private string assembly;
        private bool isVisibleInSearch = true;


        public Action VisibilityChanged;
        private void OnVisibilityChanged()
        {
            if (VisibilityChanged != null)
                VisibilityChanged();
        }

        /// <summary>
        ///     Specified whether or not this entry should appear in search.
        /// </summary>
        public bool IsVisibleInSearch
        {
            get { return isVisibleInSearch; }
            set
            {
                if (value.Equals(isVisibleInSearch)) return;

                isVisibleInSearch = value;
                OnVisibilityChanged();
            }
        }

        /// <summary>
        /// The name that is used during node creation
        /// </summary>
        public virtual string CreationName { get { return this.Name; } }

        /// <summary>
        ///     List of nested categories this search element is contained in.
        /// </summary>
        public ICollection<string> Categories
        {
            get { return SplitCategoryName(FullCategoryName).ToList(); }
        }

        public const char CATEGORY_DELIMITER = '.';

        /// <summary>
        ///     Split a category name into individual category names splitting be DEFAULT_DELIMITER
        /// </summary>
        /// <param name="categoryName">The name</param>
        /// <returns>A list of output</returns>
        public static IEnumerable<string> SplitCategoryName(string categoryName)
        {
            if (String.IsNullOrEmpty(categoryName))
                return Enumerable.Empty<string>();

            return
                categoryName.Split(CATEGORY_DELIMITER)
                    .Where(x => x != CATEGORY_DELIMITER.ToString() && !String.IsNullOrEmpty(x));
        }

        /// <summary>
        ///     The full name of entry which consists of category name and entry name.
        /// </summary>
        public string FullName
        {
            get { return FullCategoryName + "." + Name; }
        }

        /// <summary>
        ///     The category name of this node.
        /// </summary>
        public string FullCategoryName
        {
            get;
            set;
        }

        /// <summary>
        ///     The name of this entry in search.
        /// </summary>
        string ISearchEntry.Name
        {
            get { return FullCategoryName + "." + Name; }
        }

        /// <summary>
        ///     The name of this entry as it appears in the library.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// User friendly name, that is used in InCanvasSearch.
        /// </summary>
        public string UserFriendlyName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(userFriendlyName))
                    return userFriendlyName;
                else
                    return Name;
            }
            protected set
            {                
                userFriendlyName = value;                
            }
        }

        /// <summary>
        ///     The search weight of this entry.
        /// </summary>
        public double Weight = 1;

        /// <summary>
        ///     Collection of keywords which can be used to search for this element.
        /// </summary>
        public ICollection<string> SearchKeywords
        {
            get { return keywords; }
        }

        /// <summary>
        ///     Description of the node.
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(description))
                    return Dynamo.UI.Configurations.NoDescriptionAvailable;

                return description;
            }
            set
            {
                description = value;
            }
        }

        public string IconName
        {
            get { return iconName; }
        }

        /// <summary>
        ///     Group to which Node belongs to 
        /// </summary>        
        public SearchElementGroup Group
        {
            get { return group; }
            set
            {
                if (value == group) return;
                group = value;
            }
        }

        /// <summary>
        ///     Group to which Node belongs to 
        /// </summary>        
        public string Assembly
        {
            get
            {
                if (!string.IsNullOrEmpty(assembly))
                    return assembly;

                // If there wasn't any assembly, then it's builtin function, operator or custom node.
                // Icons for these members are in DynamoCore project.
                return Configurations.DefaultAssembly;
            }
            protected set
            {
                assembly = value;
            }
        }

        protected List<Tuple<string, string>> inputParameters;
        public IEnumerable<Tuple<string, string>> InputParameters
        {
            get
            {
                if (!inputParameters.Any())
                    GenerateInputParameters();

                return inputParameters;
            }
        }

        protected List<string> outputParameters;
        public IEnumerable<string> OutputParameters
        {
            get
            {
                if (!outputParameters.Any())
                    GenerateOutputParameters();

                return outputParameters;
            }
        }

        /// <summary>
        ///     Indicates whether it is custom node or zero-touch element.
        ///     And whether this element comes from package or not.
        /// </summary>
        public ElementTypes ElementType
        {
            get;
            protected set;
        }

        /// <summary>
        ///     Event fired when this search element produces a new NodeModel. This typically
        ///     happens when it is selected in the library by the user.
        /// </summary>
        public event Action<NodeModel> ItemProduced;
        protected virtual void OnItemProduced(NodeModel obj)
        {
            var handler = ItemProduced;
            if (handler != null) handler(obj);
        }

        /// <summary>
        ///     Creates a new NodeModel to be inserted into the current Dynamo workspace.
        /// </summary>
        /// <returns></returns>
        protected abstract NodeModel ConstructNewNodeModel();

        /// <summary>
        ///     Produces a new Node, via the ItemProduced event.
        /// </summary>
        public void ProduceNode()
        {
            OnItemProduced(ConstructNewNodeModel());
        }

        public NodeModel CreateNode()
        {
            return ConstructNewNodeModel();
        }

        ICollection<string> ISearchEntry.SearchTags
        {
            get
            {
                return SearchKeywords.ToList();
            }
        }

        IEnumerable<double> ISearchEntry.SearchTagWeights
        {
            get
            {
                return keywordWeights;
            }
        }

        protected virtual IEnumerable<string> GenerateOutputParameters()
        {
            outputParameters.Add("none");
            return outputParameters;
        }

        protected virtual IEnumerable<Tuple<string, string>> GenerateInputParameters()
        {
            inputParameters.Add(Tuple.Create("", "none"));
            return inputParameters;
        }
    }

    public class DragDropNodeSearchElementInfo
    {
        public NodeSearchElement SearchElement { get; private set; }

        public DragDropNodeSearchElementInfo(NodeSearchElement searchElement)
        {
            this.SearchElement = searchElement;
        }
    }
}

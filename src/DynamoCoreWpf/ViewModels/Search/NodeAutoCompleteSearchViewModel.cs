using System.Collections.Generic;
using System.Linq;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Search View Model for Node AutoComplate Search Bar
    /// </summary>
    public class NodeAutoCompleteSearchViewModel : SearchViewModel
    {
        internal PortViewModel PortViewModel { get; set; }

        /// <summary>
        /// Cache of default node suggestions, use it in case where
        /// a. our algorithm does not return sufficient results
        /// b. the results returned by our algorithm will not be useful for user
        /// </summary>
        public IEnumerable<NodeSearchElementViewModel> DefaultSuggestions { get; set; }

        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            // Off load some time consuming operation here
            InitializeDefaultAutoCompleteCandidates();
        }

        internal void InitializeDefaultAutoCompleteCandidates()
        {
            var candidates = new List<NodeSearchElementViewModel>();
            // TODO: These are hard copied all time top 7 nodes placed by customers
            // This should be only served as a temporary default case.
            var queries = new List<string>(){ "Code Block", "Watch", "List.Flatten", "List.Create", "String", "Double", "Python" };
            foreach (var query in queries)
            {
                var foundNode = Search(query).ToList().FirstOrDefault();
                if(foundNode != null)
                {
                    candidates.Add(foundNode);
                }
            }
            DefaultSuggestions = candidates;
        }

        internal void PopulateAutoCompleteCandidates()
        {
            if(PortViewModel == null) return;

            var searchElements = GetMatchingNodes();
            if (searchElements.Count() == 0)
            {
                FilteredResults = DefaultSuggestions;
            }
            else
            {
                FilteredResults = searchElements.Select(e =>
                {
                    var vm = new NodeSearchElementViewModel(e, this);
                    vm.RequestBitmapSource += SearchViewModelRequestBitmapSource;
                    return vm;
                });
            }
        }

        /// <summary>
        /// Returns a collection of node search elements for nodes
        /// that output a type compatible with the port type if it's an input port.
        /// These search elements can belong to either zero touch, NodeModel or Builtin nodes.
        /// This method returns an empty collection if the input port type cannot be inferred or
        /// there are no matching nodes found for the type. Currently the match is an exact match
        /// done including the rank information in the type, e.g. Point[] or var[]..[].
        /// The search elements can be made to appear in the node autocomplete search dialog.
        /// </summary>
        /// <returns>collection of node search elements</returns>
        internal IEnumerable<NodeSearchElement> GetMatchingNodes()
        {
            var elements = new List<NodeSearchElement>();

            var inputPortType = PortViewModel.PortModel.GetInputPortType();
            if (inputPortType == null) return elements;

            var libraryServices = dynamoViewModel.Model.LibraryServices;

            // Builtin functions and zero-touch functions
            // Multi-return ports for these nodes do not contain type information
            // and are therefore skipped.
            var functionGroups = libraryServices.GetAllFunctionGroups();
            var functionDescriptors = functionGroups.SelectMany(fg => fg.Functions).Where(fd => fd.IsVisibleInLibrary);

            foreach (var descriptor in functionDescriptors)
            {
                if (descriptor.ReturnType.ToString() == inputPortType)
                {
                    elements.Add(new ZeroTouchSearchElement(descriptor));
                }
            }

            // NodeModel nodes
            foreach (var element in Model.SearchEntries.OfType<NodeModelSearchElement>())
            {
                if (element.OutputParameters.Any(op => op == inputPortType))
                    elements.Add(element);
            }

            return elements;
        }
    }
}

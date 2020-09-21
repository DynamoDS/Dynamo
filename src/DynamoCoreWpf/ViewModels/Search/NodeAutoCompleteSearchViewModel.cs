using System.Collections.Generic;
using System.Linq;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// Search View Model for Node AutoComplate Search Bar
    /// </summary>
    public class NodeAutoCompleteSearchViewModel : SearchViewModel
    {
        internal PortViewModel PortViewModel { get; set; }

        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            //InitializeDefaultAutoCompleteCandidates();
        }

        internal void InitializeDefaultAutoCompleteCandidates()
        {
            var candidates = new List<NodeSearchElementViewModel>();
            // TODO: These are hard copied all time top 7 nodes placed by customers
            // This should be only served as a temporary default case.
            var queries = new List<string>(){ "Code Block", "Watch", "List Flatten", "List Create", "String", "Double", "Python" };
            foreach (var query in queries)
            {
                var foundNode = Search(query).ToList().FirstOrDefault();
                if(foundNode != null)
                {
                    candidates.Add(foundNode);
                }
            }
            FilteredResults = candidates;
        }

        internal void PopulateAutoCompleteCandidates()
        {
            var searchElements = PortViewModel.GetMatchingNodes();
            FilteredResults = searchElements.Select(e =>
            {
                var vm  = new NodeSearchElementViewModel(e, this);
                vm.RequestBitmapSource += SearchViewModelRequestBitmapSource;
                return vm;
            });
        }
    }
}

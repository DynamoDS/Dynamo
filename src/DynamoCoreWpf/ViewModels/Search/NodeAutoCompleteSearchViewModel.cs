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
        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            InitializeDefaultAutoCompleteCandidates();
        }

        internal void InitializeDefaultAutoCompleteCandidates()
        {
            var candidates = new List<NodeSearchElementViewModel>();
            // TODO: These are hard copied all time top 7 nodes placed by customers
            // This should be only served as default case.
            var foundNodes = Search("Code Block").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("Watch").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("Flatten").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("List Create").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("String").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("Double").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            foundNodes = Search("Python").ToList();
            candidates.Add(foundNodes.FirstOrDefault());
            FilteredResults = candidates;
        }
    }
}

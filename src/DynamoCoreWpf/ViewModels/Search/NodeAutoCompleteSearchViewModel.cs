﻿using System.Collections.Generic;
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
        internal IEnumerable<NodeSearchElementViewModel> DefaultResults { get; set; }

        internal NodeAutoCompleteSearchViewModel(DynamoViewModel dynamoViewModel) : base(dynamoViewModel)
        {
            // Off load some time consuming operation here
            InitializeDefaultAutoCompleteCandidates();
        }

        private void InitializeDefaultAutoCompleteCandidates()
        {
            var candidates = new List<NodeSearchElementViewModel>();
            // TODO: These are basic input types in Dynamo
            // This should be only served as a temporary default case.
            var queries = new List<string>(){"String", "Number Slider", "Integer Slider", "Number", "Boolean" };
            foreach (var query in queries)
            {
                var foundNode = Search(query).FirstOrDefault();
                if(foundNode != null)
                {
                    candidates.Add(foundNode);
                }
            }
            DefaultResults = candidates;
        }

        internal void PopulateAutoCompleteCandidates()
        {
            if(PortViewModel == null) return;

            var searchElements = GetMatchingSearchElements();
            // If node match searchElements found, use default suggestions
            if (!searchElements.Any())
            {
                FilteredResults = DefaultResults;
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
        internal IEnumerable<NodeSearchElement> GetMatchingSearchElements()
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DSCore;
using Dynamo.Controls;
using Dynamo.Search.SearchElements;
using Dynamo.Wpf.ViewModels;
using ProtoCore;
using ProtoCore.Mirror;

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
            // Do nothing for now, but we may off load some time consuming operation here later
        }

        internal void PopulateAutoCompleteCandidates()
        {
            if (PortViewModel == null) return;

            var searchElements = GetMatchingNodes();
            FilteredResults = searchElements.Select(e =>
            {
                var vm = new NodeSearchElementViewModel(e, this);
                //TODO I think this leaks.
                vm.RequestBitmapSource += SearchViewModelRequestBitmapSource;
                return vm;
            });
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

            var ztSearchElements = Model.SearchEntries.OfType<ZeroTouchSearchElement>();

            foreach (var ztSearchElement in ztSearchElements)
            {
                var descriptor = ztSearchElement.Descriptor;
                if ((descriptor.ReturnType.ToString() == inputPortType)
                    || DerivesFrom(inputPortType, descriptor.ReturnType.ToString(), dynamoViewModel.Model.LibraryServices.LibraryManagementCore))
                {
                    elements.Add(ztSearchElement);
                }
            }

            // NodeModel nodes
            foreach (var element in Model.SearchEntries.OfType<NodeModelSearchElement>())
            {
                if (element.OutputParameters.Any(op => op == inputPortType))
                {
                    elements.Add(element);
                }
            }
            //TODO if we intend to cache data instantiate this comparer at construction time
            //and update the typeNameToCompare.
            var comparer = new NodeSearchElementComparer(inputPortType, dynamoViewModel.Model.LibraryServices.LibraryManagementCore);
            //first sort by type distance to input port type
            elements.Sort(comparer);
            //then sort by node library group (create, action, or query node)
            return elements.OrderBy(x =>x.Group);
        }

        /// <summary>
        /// Does typeb derive from typea
        /// </summary>
        /// <param name="typea"></param>
        /// <param name="typeb"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        private bool DerivesFrom(string typea, string typeb, ProtoCore.Core core)
        {
            try
            {
                //TODO mirrors can be cached until new types are imported...
                var mirror1 = new ClassMirror(typea, core);
                var mirror2 = new ClassMirror(typeb, core);


                //TODO as we do this check we can cache the type distance...
                if (mirror2.GetClassHierarchy().Any(x => x.ClassName == mirror1.ClassName))
                {
                    //this is a derived type
                    return true;
                }
                return false;
            }
            catch
            {
                // TODO need to deal with the fact that type[] and type[]..[] are not classes - trying to create these up as ClassMirrors will yield an exception.
                Debug.WriteLine($"failed to create class mirror for {typea} or {typeb} during node autocomplete operation ");
                return false;
            }
        }

        internal class NodeSearchElementComparer : IComparer<NodeSearchElement>
        {
            private string typeNameToCompareTo;
            private ProtoCore.Core core;

            internal NodeSearchElementComparer(string typeNameToCompareTo, ProtoCore.Core core)
            {
                this.typeNameToCompareTo = typeNameToCompareTo;
                this.core = core;
            }

            public int Compare(NodeSearchElement x, NodeSearchElement y)
            {
               return CompareNodeSearchElementByTypeDistance(x, y, typeNameToCompareTo, core);
            }

            private int CompareNodeSearchElementByTypeDistance(NodeSearchElement x, NodeSearchElement y, string typeNameToCompareTo, ProtoCore.Core core)
            {
                /*
                 * -1	x proceeds y.
                 0 x and y are equal
                 1	x is after y.
                 */

                // TODO - for now using OutputParameters.FirstOrDefault() - but this is not ideal. What about
                // nodes which have multiple outputs - might need to iterate over each output param
                // and take the closest match as mechanism for sorting.

                var xTypeName = x.OutputParameters.FirstOrDefault();
                var yTypeName = y.OutputParameters.FirstOrDefault();

                if (xTypeName == yTypeName)
                {
                    return 0;
                }
                // null is further away, so x is at the end of list.
                if (xTypeName == null)
                {
                    return 1;
                }
                // null is further away, so y is at the end of the list.
                if (yTypeName == null)
                {
                    return -1;
                }

                // x proceeds y because it's an exact match
                if (xTypeName == typeNameToCompareTo)
                {
                    return -1;
                }
                // y proceeds x because it's an exact match
                if (yTypeName == typeNameToCompareTo)
                {
                    return 1;
                }

                var xdistance = GetTypeDistance(typeNameToCompareTo, xTypeName, core);
                var ydistance = GetTypeDistance(typeNameToCompareTo, yTypeName, core);

                //if distance of x to currentSelectedType is greater than y distance
                //then x is further away
                if (xdistance > ydistance)
                {
                    return 1;
                }
                if (xdistance == ydistance)
                {
                    return 0;
                }
                // distance2 < distance 1
                // x proceeds y
                return -1;
            }

            /// <summary>
            /// Return the type distance between two type names. 
            /// </summary>
            /// <param name="typea"></param>
            /// <param name="typeb"></param>
            /// <param name="core"></param>
            /// <returns>Will return int.MaxValue if no match can be found.
            /// Otherwise will return the distance between two types in class hierarchy.
            /// Will throw an exception if either type name is undefined.
            ///</returns>
            private static int GetTypeDistance(string typea, string typeb, ProtoCore.Core core)
            {
                //TODO - cache? Turn into params?
                var mirror1 = new ClassMirror(typea, core);
                var mirror2 = new ClassMirror(typeb, core);

                if (mirror1.ClassNodeID == mirror2.ClassNodeID)
                {
                    return 0;
                }

                var heirarchy = mirror2.GetClassHierarchy();
                var dist = 0;
                while (dist < heirarchy.Count())
                {
                    if (heirarchy.ElementAt(dist).ClassName == mirror1.ClassName)
                    {
                        return dist + 1;
                    }
                    dist++;
                }
                //if we can't find a match then dist should indicate that.
                return int.MaxValue;
            }

        }

    }
}

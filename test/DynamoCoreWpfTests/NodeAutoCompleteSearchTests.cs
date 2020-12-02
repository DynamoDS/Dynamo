using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Search.SearchElements;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Views;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class NodeAutoCompleteSearchTests : DynamoTestUIBase
    {

        [NodeDescription("This is test node with multiple output ports and types specified.")]
        [NodeName("node with multi type outputs")]
        [InPortNames("input1", "input2")]
        [InPortTypes("int", "double")]
        [InPortDescriptions("This is input1", "This is input2")]

        [OutPortNames("output1", "output2")]
        [OutPortTypes("Curve", "String")]
        public class MultReturnTypeNode : NodeModel
        {
            public MultReturnTypeNode()
            {
                RegisterAllPorts();
            }
        }

        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FFITarget.dll");
            libraries.Add("ProtoGeometry.dll");
            libraries.Add("DSCoreNodes.dll");
        }

        public override void Open(string path)
        {
            base.Open(path);

            DispatcherUtil.DoEvents();
        }

        public override void Run()
        {
            base.Run();

            DispatcherUtil.DoEvents();
        }

        public override void Start()
        {
            base.Start();
            Model.AddZeroTouchNodesToSearch(Model.LibraryServices.GetAllFunctionGroups());
        }


        [Test]
        public void NodeSuggestions_InputPortZeroTouchNode_AreCorrect()
        {
            Open(@"UI\ffitarget_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("9aeba33453a34c73823976222b44375b").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());

            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("FFITarget.DummyPoint", type);

            port = inPorts[1].PortModel;
            type = port.GetInputPortType();
            Assert.AreEqual("FFITarget.DummyVector", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[1];
            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(5, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName).OrderBy(s => s);
            var nodes = new[] { "FFITarget.FFITarget.DummyVector.DummyVector", "FFITarget.FFITarget.DummyVector.ByCoordinates",
                "FFITarget.FFITarget.DummyVector.ByVector", "FFITarget.FFITarget.DummyVector.Scale", "FFITarget.FFITarget.DummyPoint.DirectionTo" };
            var expectedNodes = nodes.OrderBy(s => s);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(expectedNodes.ElementAt(i), suggestedNodes.ElementAt(i));
            }

            // Show Node AutoCompleteSearchBar
            ViewModel.CurrentSpaceViewModel.OnRequestNodeAutoCompleteSearch(ShowHideFlags.Show);
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsTrue(currentWs.NodeAutoCompleteSearchBar.IsOpen);

            // Hide Node AutoCompleteSearchBar
            ViewModel.CurrentSpaceViewModel.OnRequestNodeAutoCompleteSearch(ShowHideFlags.Hide);
            Assert.IsFalse(currentWs.NodeAutoCompleteSearchBar.IsOpen);
        }

        [Test]
        public void NodeSuggestions_InputPortZeroTouchNodeForProperty_AreCorrect()
        {
            Open(@"UI\ffitarget_property_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("ac15a2a0010a4fef98c13a870ce4c55c").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(1, inPorts.Count());

            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("FFITarget.ClassFunctionality", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[0];
            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(4, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName).OrderBy(s => s);
            var nodes = new[] { "FFITarget.FFITarget.ClassFunctionality.ClassFunctionality",
                "FFITarget.FFITarget.ClassFunctionality.ClassFunctionality",
                "FFITarget.FFITarget.ClassFunctionality.ClassFunctionality",
                "FFITarget.FFITarget.ClassFunctionality.Instance" };
            var expectedNodes = nodes.OrderBy(s => s);
            Assert.IsTrue(expectedNodes.SequenceEqual(suggestedNodes));
        }

        [Test]
        public void NodeSuggestions_GeometryNodes_SortedBy_NodeGroup_CreateActionQuery()
        {
            var type1 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("DummyPoint.DirectionTo")).FirstOrDefault(); //returns a dummyPoint.
            var node = type1.CreateNode();
            ViewModel.ExecuteCommand(new DynamoModel.CreateNodeCommand(
               node, 0, 0, true, false));
            DispatcherUtil.DoEvents();
            var nodeView = NodeViewWithGuid(node.GUID.ToString());
            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = nodeView.ViewModel.InPorts.FirstOrDefault();

            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(SearchElementGroup.Create, suggestions.FirstOrDefault().Group);
            Assert.AreEqual(SearchElementGroup.Action, suggestions.ElementAt(suggestions.Count()/2).Group);
            Assert.AreEqual(SearchElementGroup.Query, suggestions.LastOrDefault().Group);
        }

        [Test]
        public void NodeSuggestions_InputPortBuiltInNode_AreCorrect()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("b6cb6ceb21df4c7fb6b186e6ff399afc").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());

            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("var[]..[]", type);

            port = inPorts[1].PortModel;
            type = port.GetInputPortType();
            Assert.AreEqual("string", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[0];
            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(0, suggestions.Count());
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //we'll compare curve to polyCurve and expect the result to be -1 for curve closer to our input type.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("Curve.Offset")).FirstOrDefault(); //returns a curve.
            var type2 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

            var comparer = new NodeAutoCompleteSearchViewModel.NodeSearchElementComparer(inputType, core);
            Assert.AreEqual(-1, comparer.Compare(type1, type2));
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance_NonExact()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //we'll compare Rect to PolyCurve and expect the result to be 1 for PolyCurve closer to our input type.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("Rectangle.ByWidthLength")).FirstOrDefault(); //returns a Rect.
            var type2 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

            var comparer = new NodeAutoCompleteSearchViewModel.NodeSearchElementComparer(inputType, core);
            Assert.AreEqual(1, comparer.Compare(type1, type2));
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance_MultiReturnNodeModel()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //inject our mock node into search model.
            Model.SearchModel.Add(new NodeModelSearchElement(new TypeLoadData(typeof(MultReturnTypeNode))));

            //we'll compare polyCurve to our mock node and expect the result to be 1 for the mocknode curve output to be closer to our input type.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("MultReturnTypeNode")).FirstOrDefault(); //returns a Curve, and String.
            var type2 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

            var comparer = new NodeAutoCompleteSearchViewModel.NodeSearchElementComparer(inputType, core);
            Assert.AreEqual(-1, comparer.Compare(type1, type2));
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance_MultiReturnNodeModelEqual()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //inject our mock node into search model.
            Model.SearchModel.Add(new NodeModelSearchElement(new TypeLoadData(typeof(MultReturnTypeNode))));

            //we'll compare curve to our mock node and expect the result to be 0 since they both match exactly.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("MultReturnTypeNode")).FirstOrDefault(); //returns a Curve, and String.
            var type2 = Model.SearchModel.SearchEntries.Where(x => x.FullName.Contains("Curve.Offset")).FirstOrDefault(); //returns a Curve.

            var comparer = new NodeAutoCompleteSearchViewModel.NodeSearchElementComparer(inputType, core);
            Assert.AreEqual(0, comparer.Compare(type1, type2));
        }

        [Test]
        public void NodeSuggestionsAreSortedBasedOnGroupAndAlphabetically()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("77aad5875f124bf59a4ece6b30813d3b").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());
            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("DSCore.Color[]", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[0];

            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(6, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName);
            var expectedNodes = new[] { "DSCoreNodes.DSCore.Color.Add",
                "DSCoreNodes.DSCore.Color.ByARGB",
                "DSCoreNodes.DSCore.Color.Divide",
                "DSCoreNodes.DSCore.Color.Multiply",
                "DSCoreNodes.DSCore.ColorRange.GetColorAtParameter",
                "DSCoreNodes.DSCore.IO.Image.Pixels"};

            Assert.IsTrue(expectedNodes.SequenceEqual(suggestedNodes));
            
        }

        [Test]
        public void NodeSuggestions_DefaultSuggestions()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("b6cb6ceb21df4c7fb6b186e6ff399afc").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());
            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("var[]..[]", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[0];

            // Running the default algorithm should return no suggestions
            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(0, suggestions.Count());

            // The initial list will fill the FilteredResults with a few options - all basic input types
            searchViewModel.PopulateAutoCompleteCandidates();
            Assert.AreEqual(5, searchViewModel.FilteredResults.Count());
            Assert.AreEqual("String", searchViewModel.FilteredResults.FirstOrDefault().Name);
        }

        [Test]
        public void SearchNodeAutocompletionSuggestions()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("77aad5875f124bf59a4ece6b30813d3b").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());
            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("DSCore.Color[]", type);

            var searchViewModel = (ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel as NodeAutoCompleteSearchViewModel);
            searchViewModel.PortViewModel = inPorts[0];

            // Get the matching node elements for the specific node port.
            searchViewModel.PopulateAutoCompleteCandidates();

            // Filter the node elements using the search field.
            searchViewModel.SearchAutoCompleteCandidates("ar");
            Assert.AreEqual(2 , searchViewModel.FilteredResults.Count());
        }

        [Test]
        public void NodeSuggestions_SkippedSuggestions()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("1a0f89fdd3ce4214ba81c08934706452").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(1, inPorts.Count());
            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("double", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[0];

            // Running the algorithm against skipped nodes should return no suggestions
            var suggestions = searchViewModel.GetMatchingSearchElements();
            Assert.AreEqual(0, suggestions.Count());

            // The initial list will fill the FilteredResults with a list of default options
            searchViewModel.PopulateAutoCompleteCandidates();
            Assert.AreEqual(2, searchViewModel.FilteredResults.Count());
            Assert.AreEqual("Number Slider", searchViewModel.FilteredResults.FirstOrDefault().Name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Properties;
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

        private readonly List<string> expectedNodes = new List<string> { "ProtoGeometry.Autodesk.DesignScript.Geometry.Arc.ByFillet", "ProtoGeometry.Autodesk.DesignScript.Geometry.Arc.ByFilletTangentToCurve", "ProtoGeometry.Autodesk.DesignScript.Geometry.BoundingBox.ByGeometry", "ProtoGeometry.Autodesk.DesignScript.Geometry.BoundingBox.ByMinimumVolume", "ProtoGeometry.Autodesk.DesignScript.Geometry.Curve.ByBlendBetweenCurves", "ProtoGeometry.Autodesk.DesignScript.Geometry.Line.ByTangency", "ProtoGeometry.Autodesk.DesignScript.Geometry.Mesh.ByGeometry", "ProtoGeometry.Autodesk.DesignScript.Geometry.Plane.ByLineAndPoint", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolyCurve.ByJoinedCurves", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolyCurve.ByThickeningCurveNormal", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolySurface.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolySurface.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolySurface.ByLoftGuides", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolySurface.BySweep", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.ByRevolve", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.BySweep", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.BySweep2Rails", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ByLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ByPatch", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ByRevolve", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ByRuledLoft", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.BySweep", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.BySweep2Rails", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildPipes", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByRevolve", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.DoesIntersect", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.IsAlmostEqualTo", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.DistanceTo", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.Intersect", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.IntersectAll", "ProtoGeometry.Autodesk.DesignScript.Geometry.Curve.Project", "ProtoGeometry.Autodesk.DesignScript.Geometry.Point.Project", "ProtoGeometry.Autodesk.DesignScript.Geometry.Solid.ProjectInputOnto", "ProtoGeometry.Autodesk.DesignScript.Geometry.Surface.ProjectInputOnto", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.Split", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.Trim", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.SerializeAsSAB", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.ClosestPointTo", "ProtoGeometry.Autodesk.DesignScript.Geometry.Curve.Join", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolyCurve.ByGroupedCurves", "ProtoGeometry.Autodesk.DesignScript.Geometry.Curve.SweepAsSolid", "ProtoGeometry.Autodesk.DesignScript.Geometry.Geometry.ExportToSAT", "ProtoGeometry.Autodesk.DesignScript.Geometry.Curve.SweepAsSurface", "ProtoGeometry.Autodesk.DesignScript.Geometry.PolySurface.LocateSurfacesByLine", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreateMatch", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeEdgesAlongCurve", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFacesAlongCurve", "ProtoGeometry.Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices" };

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

        private void ValidateMissingAddedNodes(List<string> nodeNamesResultList, PreferenceSettings prefSettings)
        {
            var namespacesToExcludeFromLibraryStr = "\"" + string.Join("\",", prefSettings.NamespacesToExcludeFromLibrary);
            var missingNodes = new List<string>();

            string nodesMatchingFailText = "Missing nodes";
            if (expectedNodes.Count() > nodeNamesResultList.Count())
            {
                missingNodes = expectedNodes.Except(nodeNamesResultList).ToList();
            }
            else if (expectedNodes.Count() < nodeNamesResultList.Count())
            {
                nodesMatchingFailText = "New Added nodes";
                missingNodes = nodeNamesResultList.Except(expectedNodes).ToList();
            }
            Assert.AreEqual(expectedNodes.Count(), nodeNamesResultList.Count(), string.Format("{0}: {1}\nNamespacesToExcludeFromLibrarySpecified: {2}\n NamespacesToExcludeFromLibrary: {3}",
                                                                                                nodesMatchingFailText,
                                                                                                string.Join(", ", missingNodes),
                                                                                                namespacesToExcludeFromLibraryStr,
                                                                                                prefSettings.NamespacesToExcludeFromLibrarySpecified.ToString()));
        }

        [Test]
        public void NodeSuggestions_CanAutoCompleteInCustomNodeWorkspace()
        {
            Open(@"pkgs\EvenOdd2\dyf\EvenOdd.dyf");
            ViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Pick the % node
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("1ddf4b4cc39f42acadd578db42bcb6d3").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());

            var port = inPorts[0].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("var[]..[]", type);

            port = inPorts[1].PortModel;
            type = port.GetInputPortType();
            Assert.AreEqual("var[]..[]", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[1];
            var suggestions = searchViewModel.GetMatchingSearchElements();
            // No matching search elements should be found
            Assert.AreEqual(0, suggestions.Count());

            // Show Node AutoCompleteSearchBar in custom node workspace
            ViewModel.CurrentSpaceViewModel.OnRequestNodeAutoCompleteSearch(ShowHideFlags.Show);
            DispatcherUtil.DoEvents();
            var currentWs = View.ChildOfType<WorkspaceView>();
            Assert.IsTrue(currentWs.NodeAutoCompleteSearchBar.IsOpen);
        }

        [Test]
        public void NodeSuggestions_CanAutoCompleteOnCustomNodes()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Pick the % node
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("05d82f5627314cc9bf802c5d6d3ed907").ToString());

            var inPorts = nodeView.ViewModel.InPorts;
            Assert.AreEqual(2, inPorts.Count());

            var port = inPorts[1].PortModel;
            var type = port.GetInputPortType();
            Assert.AreEqual("DSCore.Color", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = inPorts[1];

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // The initial list will fill the FilteredResults with a few options - all basic input types
            searchViewModel.PopulateAutoCompleteCandidates();
            Assert.AreEqual(8, searchViewModel.FilteredResults.Count());
        }

        [Test]
        public void NodeSuggestions_CanAutoCompleteOnCustomNodesOutPort()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Pick the % node
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("05d82f5627314cc9bf802c5d6d3ed907").ToString());

            var outPorts = nodeView.ViewModel.OutPorts;
            Assert.AreEqual(2, outPorts.Count());

            var port = outPorts[1].PortModel;
            var type = port.GetOutPortType();
            Assert.AreEqual("Color", type);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = outPorts[1];

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Results will be nodes that take color or color[] etc as params.
            searchViewModel.PopulateAutoCompleteCandidates();
            Assert.AreEqual(10, searchViewModel.FilteredResults.Count());
        }

        [Test]
        public void NodeSuggestions_CanAutoCompleteOnCustomNodesOutPort_WithSpaceInPortName()
        {
            var outputNode = new Dynamo.Graph.Nodes.CustomNodes.Output();
            outputNode.Symbol = "Line Nonsense";
            var cnm = new Dynamo.Graph.Nodes.CustomNodes.Function(
                new CustomNodeDefinition(Guid.NewGuid(), "mock", new List<NodeModel>() { outputNode })
                , "mock", "mock", "mock");
            var cnvm = new NodeViewModel(ViewModel.CurrentSpaceViewModel, cnm);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = cnvm.OutPorts.First();

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Results will be nodes that accept Line as parameter.
            searchViewModel.PopulateAutoCompleteCandidates();
            var nodeNamesResultList = searchViewModel.FilteredResults.Select(x => x.FullName).ToList();

            ValidateMissingAddedNodes(nodeNamesResultList, searchViewModel.dynamoViewModel.PreferenceSettings);
        }
        [Test]
        public void NodeSuggestions_CanAutoCompleteOnCustomNodesOutPort_WithWhiteSpaceStartingPortName()
        {
            var outputNode = new Dynamo.Graph.Nodes.CustomNodes.Output();
            outputNode.Symbol = "   Line Nonsense";
            var cnm = new Dynamo.Graph.Nodes.CustomNodes.Function(
                new CustomNodeDefinition(Guid.NewGuid(), "mock", new List<NodeModel>() { outputNode })
                , "mock", "mock", "mock");
            var cnvm = new NodeViewModel(ViewModel.CurrentSpaceViewModel, cnm);

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = cnvm.OutPorts.First();

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Results will be nodes that accept Line as parameter.
            searchViewModel.PopulateAutoCompleteCandidates();
            var nodeNamesResultList = searchViewModel.FilteredResults.Select(x => x.FullName).ToList();

            ValidateMissingAddedNodes(nodeNamesResultList, searchViewModel.dynamoViewModel.PreferenceSettings);
        }

        [Test]
        public void NodeSuggestions_InputPortZeroTouchNode_AreCorrect()
        {
            Open(@"UI\ffitarget_inputport_suggestion.dyn");
            ViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

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
            DispatcherUtil.DoEvents();
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
            var type1 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("DummyPoint.DirectionTo")).FirstOrDefault(); //returns a dummyPoint.
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
        public void NodeSuggestions_OutputPortBuiltInNode_AreCorrect()
        {
            Open(@"UI\builtin_outputport_suggestion.dyn");

            // Get the node view for a specific node in the graph
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("a3412b9b1de54205a1fe6904697ffd5f").ToString());

            // Get the output port type for the node. 
            var outPorts = nodeView.ViewModel.OutPorts;
            Assert.AreEqual(1, outPorts.Count);

            var port = outPorts[0].PortModel;
            var type = port.GetOutPortType();
            Assert.IsTrue(type.Contains("Line"));

            // Trigger node autocomplete on the output port and verify the results. 
            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = outPorts[0];
            var suggestions = searchViewModel.GetMatchingSearchElements();

            var nodeNamesResultList = suggestions.Select(x => x.FullName).ToList();

            ValidateMissingAddedNodes(nodeNamesResultList, searchViewModel.dynamoViewModel.PreferenceSettings);
        }

        [Test]
        public void NodeSuggestions_MultipleOutputPortCBN_AreCorrect()
        {
            Open(@"UI\builtin_outputport_CBNsuggestion.dyn");

            // Get the output port type for the node. 
            var outPorts = ViewModel.CurrentSpaceViewModel.Nodes.FirstOrDefault().OutPorts;
            Assert.AreEqual(3, outPorts.Count());

            // Setup
            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = outPorts[2];
            searchViewModel.PopulateDefaultAutoCompleteCandidates();
            Assert.AreEqual(3, searchViewModel.FilteredResults.Count());

            // Trigger node autocomplete on the 3rd output port and verify the start port of final connector is expected
            searchViewModel.FilteredResults.FirstOrDefault().CreateAndConnectCommand.Execute(outPorts[2].PortModel);

            // The connector should be from the same index of the original port which triggered node autocomplete
            var connector = ViewModel.CurrentSpaceViewModel.Connectors.FirstOrDefault();
            Assert.AreEqual(outPorts[2].PortModel.Index, connector.ConnectorModel.Start.Index);
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //we'll compare curve to polyCurve and expect the result to be -1 for curve closer to our input type.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("Curve.Offset")).FirstOrDefault(); //returns a curve.
            var type2 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

            var comparer = new NodeAutoCompleteSearchViewModel.NodeSearchElementComparer(inputType, core);
            Assert.AreEqual(-1, comparer.Compare(type1, type2));
        }

        [Test]
        public void NodeSearchElementComparerSortsBasedOnTypeDistance_NonExact()
        {
            var core = Model.LibraryServices.LibraryManagementCore;
            //we'll compare Rect to PolyCurve and expect the result to be 1 for PolyCurve closer to our input type.
            var inputType = "Autodesk.DesignScript.Geometry.Curve";
            var type1 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("Rectangle.ByWidthLength")).FirstOrDefault(); //returns a Rect.
            var type2 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

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
            var type1 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("MultReturnTypeNode")).FirstOrDefault(); //returns a Curve, and String.
            var type2 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("PolyCurve.ByJoinedCurves")).FirstOrDefault(); //returns a polycurve.

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
            var type1 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("MultReturnTypeNode")).FirstOrDefault(); //returns a Curve, and String.
            var type2 = Model.SearchModel.Entries.Where(x => x.FullName.Contains("Curve.Offset")).FirstOrDefault(); //returns a Curve.

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
            Assert.AreEqual(8, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName);
            var expectedNodes = new[] { "DSCoreNodes.DSCore.Color.Add",
                "DSCoreNodes.DSCore.Color.ByARGB",
                "DSCoreNodes.DSCore.Color.Divide",
                "DSCoreNodes.DSCore.Color.Multiply",
                "Core.Color.Color Palette",
                "Core.Color.Color Range",
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

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

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
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Get the matching node elements for the specific node port.
            searchViewModel.PopulateAutoCompleteCandidates();

            // Filter the node elements using the search field.
            searchViewModel.SearchAutoCompleteCandidates("ar");
            Assert.AreEqual(4 , searchViewModel.FilteredResults.Count());
        }

        [Test]
        public void CloseNodeAutocompleteWhenParentNodeDeleted()
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
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // Get the matching node elements for the specific node port.
            searchViewModel.PopulateAutoCompleteCandidates();
            // Show Node AutoCompleteSearchBar
            ViewModel.CurrentSpaceViewModel.OnRequestNodeAutoCompleteSearch(ShowHideFlags.Show);
            //remove the parent node
            searchViewModel.dynamoViewModel.CurrentSpaceViewModel.Model.RemoveAndDisposeNode(nodeView.ViewModel.NodeModel);

            var currentWs = View.ChildOfType<WorkspaceView>();
            //confirm if the AutoCompleteSearchBar is closed.
            Assert.IsFalse(currentWs.NodeAutoCompleteSearchBar.IsOpen);

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

            // Set the suggestion to ObjectType
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.ObjectType;

            // The initial list will fill the FilteredResults with a list of default options
            searchViewModel.PopulateAutoCompleteCandidates();
            Assert.AreEqual(2, searchViewModel.FilteredResults.Count());
            Assert.AreEqual("Number Slider", searchViewModel.FilteredResults.FirstOrDefault().Name);
        }

        [Test]
        public void NoMLAutocompleteRecommendations()
        {
            Open(@"UI\builtin_inputport_suggestion.dyn");

            // Get the node view for a CoreNodeModels.Sequence node
            NodeView nodeView = NodeViewWithGuid(Guid.Parse("17b1efe0cdea4ce48cafc2da689f79a4").ToString());

            var outPorts = nodeView.ViewModel.OutPorts;
            Assert.AreEqual(1, outPorts.Count());

            var searchViewModel = ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel;
            searchViewModel.PortViewModel = outPorts[0];

            // Set the suggestion to ML
            searchViewModel.dynamoViewModel.PreferenceSettings.DefaultNodeAutocompleteSuggestion = NodeAutocompleteSuggestion.MLRecommendation;

            // As there is no authentication in test fixture, no results are shown.
            searchViewModel.PopulateAutoCompleteCandidates();
            searchViewModel.DisplayAutocompleteMLStaticPage = true;
            searchViewModel.DisplayLowConfidence = false;
            searchViewModel.AutocompleteMLTitle = Resources.AutocompleteNoRecommendationsTitle;
            searchViewModel.AutocompleteMLMessage = Resources.AutocompleteNoRecommendationsMessage;
        }
    }
}

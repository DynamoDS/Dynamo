using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Controls;
using DynamoCoreWpfTests.Utility;
using NUnit.Framework;

namespace DynamoCoreWpfTests
{
    class NodeAutoCompleteSearchTests : DynamoTestUIBase
    {
        protected override void GetLibrariesToPreload(List<string> libraries)
        {
            libraries.Add("FunctionObject.ds");
            libraries.Add("BuiltIn.ds");
            libraries.Add("FFITarget.dll");
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

            var searchViewModel = (ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel as NodeAutoCompleteSearchViewModel);
            searchViewModel.PortViewModel = inPorts[1];
            var suggestions = searchViewModel.GetMatchingNodes();
            Assert.AreEqual(5, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName).OrderBy(s => s);
            var nodes = new[] { "FFITarget.FFITarget.DummyVector.DummyVector", "FFITarget.FFITarget.DummyVector.ByCoordinates",
                "FFITarget.FFITarget.DummyVector.ByVector", "FFITarget.FFITarget.DummyVector.Scale", "FFITarget.FFITarget.DummyPoint.DirectionTo" };
            var expectedNodes = nodes.OrderBy(s => s);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(expectedNodes.ElementAt(i), suggestedNodes.ElementAt(i));
            }
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

            var searchViewModel = (ViewModel.CurrentSpaceViewModel.NodeAutoCompleteSearchViewModel as NodeAutoCompleteSearchViewModel);
            searchViewModel.PortViewModel = inPorts[1];
            var suggestions = searchViewModel.GetMatchingNodes();
            Assert.AreEqual(16, suggestions.Count());

            var suggestedNodes = suggestions.Select(s => s.FullName).OrderBy(s => s);
            var nodes = new[]
            {
                "Core.Input.File Path",
                "Core.String.String from Array",
                "Core.String.String from Object",
                "FFITarget.DupTargetTest.Bar",
                "FFITarget.FFITarget.AtLevelTestClass.sumAndConcat",
                "FFITarget.FFITarget.AtLevelTestClass.SumAndConcat",
                "FFITarget.FFITarget.FirstNamespace.AnotherClassWithNameConflict.PropertyA",
                "FFITarget.FFITarget.FirstNamespace.AnotherClassWithNameConflict.PropertyB",
                "FFITarget.FFITarget.FirstNamespace.AnotherClassWithNameConflict.PropertyC",
                "FFITarget.FFITarget.FirstNamespace.ClassWithNameConflict.PropertyA",
                "FFITarget.FFITarget.FirstNamespace.ClassWithNameConflict.PropertyB",
                "FFITarget.FFITarget.FirstNamespace.ClassWithNameConflict.PropertyC",
                "FFITarget.FFITarget.SecondNamespace.ClassWithNameConflict.PropertyD",
                "FFITarget.FFITarget.SecondNamespace.ClassWithNameConflict.PropertyE",
                "FFITarget.FFITarget.SecondNamespace.ClassWithNameConflict.PropertyF",
                "FFITarget.FFITarget.TestData.GetStringValue"
            };
            var expectedNodes = nodes.OrderBy(s => s);
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(expectedNodes.ElementAt(i), suggestedNodes.ElementAt(i));
            }
        }
    }
}

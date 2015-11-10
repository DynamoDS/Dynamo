﻿using System;
using System.Linq;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Nodes;

using NUnit.Framework;

namespace Dynamo
{
    [NodeDescription("This is a test node.")]
    [NodeName("Test Node")]
    public class TestNode : NodeModel
    {
        public TestNode()
        {
            InPortData.Add(new PortData("input A", "This is input A."));
            OutPortData.Add(new PortData("output A", "This is output A."));
            RegisterAllPorts();
        }
    }

    [NodeDescription("This is another test node.")]
    [NodeName("Dummy test Node")]
    [InPortNames("input1", "input2")]
    [InPortTypes("int", "double")]
    [InPortDescriptions("This is input1", "This is input2")]

    [OutPortNames("output1", "output2")]
    [OutPortTypes("foo", "bla")]
    [OutPortDescriptions(typeof(Dynamo.Properties.Resources), "DescriptionResource1")]
    public class DummyNodeModel : NodeModel
    {
        public DummyNodeModel()
        {
            RegisterAllPorts();
        }
    }

    /// <summary>
    /// DerivedTestNode is used to test a node class which derives from
    /// another node class and needs to add nodes.
    /// </summary>
    public class DerivedTestNode : TestNode
    {
        public DerivedTestNode()
        {
            AddPort(PortType.Input, new PortData("input B", "This is input B."), 1);
        }
    }

    [TestFixture]
    class NodeConstructionTests
    {
        [Test]
        public void TestNodeHasToolTipsOnInputPorts()
        {
            var node = new TestNode();
            Assert.AreEqual(node.InPorts[0].ToolTipContent, "This is input A.");
        }

        [Test]
        public void TestNodeHasToolTipsOnOutputPorts()
        {
            var node = new TestNode();
            Assert.AreEqual(node.OutPorts[0].ToolTipContent, "This is output A.");
        }

        [Test]
        public void DerivedTestNodeHasToolTipsOnInputPorts()
        {
            var node = new DerivedTestNode();
            Assert.AreEqual(node.InPorts[1].ToolTipContent, "This is input B.");
        }

        [Test]
        public void TestNodeCanLoadInputPortsFromAttributes()
        {
            var node = new DummyNodeModel();
            Assert.AreEqual(2, node.InPorts.Count);

            Assert.AreEqual("input1", node.InPorts[0].PortName);
            Assert.AreEqual("input2", node.InPorts[1].PortName);

            Assert.AreEqual("This is input1", node.InPorts[0].ToolTipContent);
            Assert.AreEqual("This is input2", node.InPorts[1].ToolTipContent);

            var typeLoadData = new TypeLoadData(node.GetType());
            Assert.AreEqual(2, typeLoadData.InputParameters.Count());

            Assert.AreEqual(Tuple.Create("input1", "int"), typeLoadData.InputParameters.ElementAt(0));
            Assert.AreEqual(Tuple.Create("input2", "double"), typeLoadData.InputParameters.ElementAt(1));
        }

        [Test]
        public void TestNodeCanLoadOutputPortsFromAttributes()
        {
            var node = new DummyNodeModel();
            Assert.AreEqual(2, node.InPorts.Count);

            Assert.AreEqual("output1", node.OutPorts[0].PortName);
            Assert.AreEqual("output2", node.OutPorts[1].PortName);

            Assert.AreEqual("some description", node.OutPorts[0].ToolTipContent);
            Assert.AreEqual("", node.OutPorts[1].ToolTipContent);

            var typeLoadData = new TypeLoadData(node.GetType());
            Assert.AreEqual(2, typeLoadData.OutputParameters.Count());

            Assert.AreEqual("foo", typeLoadData.OutputParameters.ElementAt(0));
            Assert.AreEqual("bla", typeLoadData.OutputParameters.ElementAt(1));
        }
    }
}

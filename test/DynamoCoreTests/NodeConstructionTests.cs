using Dynamo.Models;

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
    }
}

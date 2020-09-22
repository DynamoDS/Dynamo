using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreNodeModels.Input;
using Dynamo;
using Dynamo.Tests;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesTests
{
    [TestFixture]
    public class SequenceTests : DynamoModelTestBase
    {
        private DoubleInput DoubleInputNode;
        private DoubleInput.IDoubleSequence testNode;

        [SetUp]
        public void TestsSetup()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\number\TestNumberDeserialization.dyn");
            OpenModel(openPath);

            DoubleInputNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("6fc905f8533f433a90fe4b9181463d53"));

            testNode = DoubleInput.ParseValue("1:#TestText", new[] { '\n' }, new List<string>(), DoubleInputNode.Convert).FirstOrDefault();
        }

        [Test]
        public void GetFSchemeValueTest()
        {
            Dictionary<string, double> idLookup = new Dictionary<string, double>();
            idLookup.Add("TestText", 1);

            IEnumerable<double> result = (IEnumerable<double>) testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(1,result.Count());

            idLookup.Clear();
            idLookup.Add("TestText", -1);

            result = (IEnumerable<double>)testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public void GetAstNodeTest()
        {
            var rangExpNode = new RangeExprNode();
            Dictionary<string, AssociativeNode> idLookup = new Dictionary<string, AssociativeNode>();
            idLookup.Add("TestText", rangExpNode);

            var result = (RangeExprNode) testNode.GetAstNode(idLookup);
            var from = (IntNode)result.From;
            var to = (RangeExprNode)result.To;
            var step = (IntNode) result.Step;

            Assert.AreEqual(1, from.Value);
            Assert.IsNotNull(to);
            Assert.IsNull(to.To);
            Assert.AreEqual(1, step.Value);
        }
    }

    [TestFixture]
    public class CountRangeTests : DynamoModelTestBase
    {
        private DoubleInput DoubleInputNode;
        private DoubleInput.IDoubleSequence testNode;

        [SetUp]
        public void TestsSetup()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\number\TestNumberDeserialization.dyn");
            OpenModel(openPath);

            DoubleInputNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("6fc905f8533f433a90fe4b9181463d53"));

            testNode = DoubleInput.ParseValue("1..2:#n", new[] { '\n' }, new List<string>(), DoubleInputNode.Convert).FirstOrDefault();
        }

        [Test]
        public void GetFSchemeValueTest()
        {
            Dictionary<string, double> idLookup = new Dictionary<string, double>();
            idLookup.Add("n", 5);

            IEnumerable<double> result = (IEnumerable<double>)testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(5, result.Count());

            idLookup.Clear();
            idLookup.Add("n", -2);

            result = (IEnumerable<double>)testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void GetAstNodeTest()
        {
            var rangExpNode = new RangeExprNode();
            Dictionary<string, AssociativeNode> idLookup = new Dictionary<string, AssociativeNode>();
            idLookup.Add("n", rangExpNode);

            var result = (RangeExprNode)testNode.GetAstNode(idLookup);
            var from = (IntNode)result.From;
            var to = (IntNode)result.To;

            Assert.AreEqual(1, from.Value);
            Assert.AreEqual(2, to.Value);
        }
    }

    [TestFixture]
    public class ApproxRangeTests : DynamoModelTestBase
    {
        private DoubleInput DoubleInputNode;
        private DoubleInput.IDoubleSequence testNode;

        [SetUp]
        public void TestsSetup()
        {
            string openPath = Path.Combine(TestDirectory,
                @"core\number\TestNumberDeserialization.dyn");
            OpenModel(openPath);

            DoubleInputNode = CurrentDynamoModel.CurrentWorkspace.NodeFromWorkspace<DoubleInput>(
                Guid.Parse("6fc905f8533f433a90fe4b9181463d53"));

            testNode = DoubleInput.ParseValue("1..6:~n", new[] { '\n' }, new List<string>(), DoubleInputNode.Convert).FirstOrDefault();
        }

        [Test]
        public void GetFSchemeValueTest()
        {
            Dictionary<string, double> idLookup = new Dictionary<string, double>();
            idLookup.Add("n", 5);

            IEnumerable<double> result = (IEnumerable<double>)testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(2, result.Count());

            idLookup.Clear();
            idLookup.Add("n", -2);

            result = (IEnumerable<double>)testNode.GetFSchemeValue(idLookup);
            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void GetAstNodeTest()
        {
            var rangExpNode = new RangeExprNode();
            Dictionary<string, AssociativeNode> idLookup = new Dictionary<string, AssociativeNode>();
            idLookup.Add("n", rangExpNode);

            var result = (RangeExprNode)testNode.GetAstNode(idLookup);
            var from = (IntNode)result.From;
            var to = (IntNode)result.To;

            Assert.AreEqual(1, from.Value);
            Assert.AreEqual(6, to.Value);
        }
    }
}

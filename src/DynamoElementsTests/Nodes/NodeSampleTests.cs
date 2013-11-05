using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSCoreNodes;
using Dynamo.Tests;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    class NodeSampleTests : DynamoUnitTest
    {
        [Test]
        public void SliderASTGeneration()
        {
            var sliderNode = new NumberSlider { Value = 10 };
            var buildOutput = sliderNode.BuildAst();

            Assert.IsInstanceOf<DoubleNode>(buildOutput);
            Assert.AreEqual(new DoubleNode("10").value, (buildOutput as DoubleNode).value);
        }
    }
}

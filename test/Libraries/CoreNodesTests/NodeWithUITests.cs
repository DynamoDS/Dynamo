// NOTE: The following namespace has been updated. This test use to fail in unit 
// test, the reason is that LibG.Managed.dll could not be loaded. That was because the
// right "AppDomain.CurrentDomain.AssemblyResolve" callback has not been set to 
// resolve the library path when this test is run.
// 
// Where is "AppDomain.CurrentDomain.AssemblyResolve" set? It is the "Setup" 
// class this project which is marked as "SetUpFixture". The "Setup" class is 
// responsible in setting "AssemblyResolve" callback that will eventually help 
// resolving the path to LibG.Managed.dll. But the set-up fixture wasn't called due 
// to the fact that it was residing in a different namespace (Dynamo.Tests) 
// while "NodeSampleTests" resides in "Dynamo.Nodes". So by relocating the 
// "NodeSampleTests" to "Dynamo.Tests" helps NUnit locating the "Setup" class. 
// 
using System.Collections.Generic;
using System.Linq;
using Dynamo.Nodes;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesTests
{
    [TestFixture]
    class NodeWithUITests
    {
        [Test]
        [Category("Failure")]
        [Category("UnitTests")]
        public void SliderASTGeneration()
        {
            var sliderNode = new DoubleSlider(null) { Value = 10 };
            var buildOutput = sliderNode.BuildOutputAst(new List<AssociativeNode>());

            Assert.AreEqual(
                10,
                ((DoubleNode)((BinaryExpressionNode)buildOutput.First()).RightNode).Value);
        }

        [Test]
        [Category("Failure")]
        public void SliderMaxValue()
        {
            var sliderNode = new DoubleSlider(null) { Value = 500 };
            sliderNode.UpdateValue("Value", "1000");

            Assert.AreEqual(
                 1000,
                 sliderNode.Max);

            sliderNode.UpdateValue("Value", "-1");

            Assert.AreEqual(
                 -1,
                 sliderNode.Min);
        }

        [Test]
        [Category("Failure")]
        public void IntegerSliderMaxValue()
        {
            var integerSliderNode = new IntegerSlider(null) { Value = 500 };
            integerSliderNode.UpdateValue("Value", "1000");

            Assert.AreEqual(
                 1000,
                 integerSliderNode.Max);

            integerSliderNode.UpdateValue("Value", "-1");

            Assert.AreEqual(
                 -1,
                 integerSliderNode.Min);
        }
    }
}

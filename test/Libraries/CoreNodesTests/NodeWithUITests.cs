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
using Dynamo.Models;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using DoubleSlider = DSCoreNodesUI.Input.DoubleSlider;
using IntegerSlider = DSCoreNodesUI.Input.IntegerSlider;

namespace DSCoreNodesTests
{
    [TestFixture]
    class NodeWithUITests
    {
        [Test]
        [Category("UnitTests")]
        public void SliderASTGeneration()
        {
            var sliderNode = new DoubleSlider() { Value = 10 };
            var buildOutput = sliderNode.BuildOutputAst(new List<AssociativeNode>());

            Assert.AreEqual(
                10,
                ((DoubleNode)((BinaryExpressionNode)buildOutput.First()).RightNode).Value);
        }

        [Test]
        public void SliderMaxValue()
        {
            var sliderNode = new DoubleSlider() { Value = 500 };
            var updateValueParams = new UpdateValueParams("Value", "1000");
            sliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                 1000,
                 sliderNode.Max);

            updateValueParams = new UpdateValueParams("Value", "-1");
            sliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                 -1,
                 sliderNode.Min);
        }

        [Test]
        public void IntegerSliderMaxValue()
        {
            var integerSliderNode = new IntegerSlider() { Value = 500 };
            var updateValueParams = new UpdateValueParams("Value", "1000");
            integerSliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                 1000,
                 integerSliderNode.Max);

            updateValueParams = new UpdateValueParams("Value", "-1");
            integerSliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                 -1,
                 integerSliderNode.Min);

            updateValueParams = new UpdateValueParams("Value", "2147483648");
            integerSliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                2147483647,
                integerSliderNode.Max);

            updateValueParams = new UpdateValueParams("Value", "-2147483649");
            integerSliderNode.UpdateValue(updateValueParams);

            Assert.AreEqual(
                -2147483648,
                integerSliderNode.Min);
        }
    }
}

using System.Globalization;
using System.Threading;
using Dynamo.Graph.Nodes;
using NUnit.Framework;


namespace Dynamo.Tests
{

    [Category("InputOutputData")]
    class InputOutputDataTest : DynamoModelTestBase
    {
        [Test]
        [Category("RegressionTests")]
        public void TestNodeOutputDataInDifferentCulture()
        {
            var frCulture = CultureInfo.CreateSpecificCulture("fr-FR");

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = frCulture;
            Thread.CurrentThread.CurrentUICulture = frCulture;

            NodeOutputData outputData1 = new NodeOutputData();
            outputData1.InitialValue = "1.23";
            NodeOutputData outputData2 = new NodeOutputData();
            outputData2.InitialValue = "123";
            Assert.IsFalse(outputData1.Equals(outputData2));

            outputData2.InitialValue = "1.23";
            Assert.IsTrue(outputData1.Equals(outputData2));


            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
        }

        [Test]
        [Category("RegressionTests")]
        public void TestNodeInputDataInDifferentCulture()
        {
            var frCulture = CultureInfo.CreateSpecificCulture("fr-FR");

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            var currentUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = frCulture;
            Thread.CurrentThread.CurrentUICulture = frCulture;

            NodeInputData inputData1 = new NodeInputData();
            inputData1.Value = "1.23";
            NodeInputData inputData2 = new NodeInputData();
            inputData2.Value = "123";
            Assert.IsFalse(inputData1.Equals(inputData2));

            inputData2.Value = "1.23";
            Assert.IsTrue(inputData1.Equals(inputData2));


            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentUICulture;
        }
    }
}
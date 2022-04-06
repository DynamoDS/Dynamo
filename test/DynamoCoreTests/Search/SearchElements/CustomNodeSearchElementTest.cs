using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Search.SearchElements;
using Moq;
using NUnit.Framework;

namespace Dynamo.Tests.Search.SearchElements
{
    [TestFixture]
    class CustomNodeSearchElementTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the CustomNodeSearchElement.ConstructNewNodeModel() method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchConstructNewNodeModel()
        {
            //Arrange
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            const string path = @"C:\temp\graphics.dyn";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElement(moq.Object, dummyInfo1);

            //Act
            var nodeCreated = dummySearch1.CreateNode();

            //Assert
            //We are using a Mock for the customNodeManager parameter passed to CustomNodeSearchElement constructor then it will be null
            Assert.IsNull(nodeCreated);
        }

        /// <summary>
        /// This test method will execute the next method/properties in CustomNodeSearchElement class:
        /// void TryLoadDocumentation()
        /// IEnumerable<Tuple<string, string>> GenerateInputParameters()
        /// IEnumerable<string> GenerateOutputParameters()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchTryLoadDocumentationCustom()
        {
            //Arrange
            string path = Path.Combine(TestDirectory, @"core\CustomNodes\CNDefault.dyf");
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElement(moq.Object, dummyInfo1);

            //Act
            //Execute the GenerateInputParameters() method
            List<Tuple<string, string>> inputParameters = dummySearch1.InputParameters as List<Tuple<string, string>>;

            //Execute the GenerateOutputParameters() method
            List<string> outputParameters = dummySearch1.OutputParameters as List<string>;

            dummySearch1.ProduceNode();

            //Assert
            //It just validates that the parameters is not null and has at least one element in the list
            Assert.IsNotNull(inputParameters);
            Assert.Greater(inputParameters.Count, 0);
            Assert.IsNotNull(outputParameters);
            Assert.Greater(outputParameters.Count, 0);
        }

        /// <summary>
        /// This test method will execute the TryLoadDocumentation() method and reach the exception section
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestSearchTryLoadDocumentationException()
        {
            //Arrange
            const string nodeName = "TheNoodle";
            const string catName = "TheCat";
            const string descr = "TheCat";
            string path = Path.Combine(TestDirectory, @"core\DoesntExists.dyn");//This dyn file doesn't exist

            var guid1 = Guid.NewGuid();
            var dummyInfo1 = new CustomNodeInfo(guid1, nodeName, catName, descr, path);
            var moq = new Mock<ICustomNodeSource>();
            var dummySearch1 = new CustomNodeSearchElement(moq.Object, dummyInfo1);

            //Act
            //It generates an exception because the dyn file doesn't exists, the exception is not re-thrown 
            List<Tuple<string, string>> parameters = dummySearch1.InputParameters as List<Tuple<string, string>>;

            //Assert
            //It just validates that the parameters is not null and has at least one element in the list
            Assert.IsNotNull(parameters);
            Assert.Greater(parameters.Count, 0);
        }
    }
}

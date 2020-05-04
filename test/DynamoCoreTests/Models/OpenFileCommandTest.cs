using System;
using System.IO;
using System.Xml;
using Dynamo.Models;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class OpenFileCommandTest : DynamoModelTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestTryFindFile()
        {
            string wspath = Path.Combine(TestDirectory, @"core\callsite\RebindingSingleDimension.dyn");

            var fileCommand = new DynamoModel.OpenFileCommand(wspath);          

            XmlDocument xmlDocument = new XmlDocument();
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            var command = new OpenFileCommandDerivedTest(wspath);
            command.SerializeCoreTest(elemTest);

            var deserializedCommand = OpenFileCommand.DeserializeCore(elemTest);

            Assert.IsNotNull(deserializedCommand);
        }
    }

    /// <summary>
    /// This test class was created with the purpose of execute the protected method ExecuteCore(DynamoModel dynamoModel)
    /// </summary>
    class OpenFileCommandDerivedTest : OpenFileCommand
    {
        public OpenFileCommandDerivedTest(string filePath) : base(filePath)
        {
        }

        public void ExecuteTest(DynamoModel dynamoModel)
        {
            ExecuteCore(dynamoModel);
        }

        public void SerializeCoreTest(System.Xml.XmlElement element)
        {
            SerializeCore(element);
        }

        protected override void ExecuteCore(DynamoModel dynamoModel)
        {
            try
            {
                //This will raise a NotImplementedException since ExecuteCore should not be implemented for a PausePlaybackCommand 
                base.ExecuteCore(dynamoModel);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected override void SerializeCore(System.Xml.XmlElement element)
        {
            base.SerializeCore(element);
        }
    }
}

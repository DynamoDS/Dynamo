using System;
using Dynamo.Models;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class PausePlaybackCommandTest : DynamoModelTestBase
    {
        //This test method will execute the void ExecuteCore(DynamoModel dynamoModel) method in PausePlaybackCommand class
        [Test]
        [Category("UnitTests")]
        public void TestExecuteCore()
        {
            //Arrange
            var cmd = new PausePlaybackCommandDerivedTest(10);

            //Assert
            //When the ExecuteCore method is executed for a PausePlaybackCommand class it will throw a exception
            Assert.Throws<NotImplementedException>(() => cmd.ExecuteTest(CurrentDynamoModel));
        }
    }

    /// <summary>
    /// This test class was created with the purpose of execute the protected method ExecuteCore(DynamoModel dynamoModel)
    /// </summary>
    class PausePlaybackCommandDerivedTest : PausePlaybackCommand
    {
        public PausePlaybackCommandDerivedTest(int pauseDurationInMs) : base(pauseDurationInMs)
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
            catch (Exception)
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

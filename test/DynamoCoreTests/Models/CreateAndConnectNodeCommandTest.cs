using System;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class CreateAndConnectNodeCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test case will execute the method void ExecuteCore(DynamoModel dynamoModel) from the CreateAndConnectNodeCommand class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateAndConnectNodeCommand_ExecuteCore()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();
            Guid existingNodeGuid = Guid.NewGuid();
            double x = 100;
            double y = 190;

            var cmdOne = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.Input.DoubleSlider", 0, 1, x, y, false, false);
            //Act
            //This will execute the method ExecuteCore(DynamoModel dynamoModel)
            CurrentDynamoModel.ExecuteCommand(cmdOne);

            //Assert
            Assert.IsNotNull(cmdOne);
        }
    }
}

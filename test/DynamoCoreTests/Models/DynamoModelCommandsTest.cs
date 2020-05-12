using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Models;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class will contain test method for executing methods/properties in the DynamoModel class
    /// </summary>
    [TestFixture]
    class DynamoModelCommandsTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the DynamoModel_CreateAndConnectNodeImpl method from the DynamoModel class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void DynamoModel_CreateAndConnectNodeImpl()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();
            Guid existingNodeGuid = new Guid("81c94fd0-35a0-4680-8535-00aff41192d3");
            double x = 100;
            double y = 190;

            string openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);

            //This command is created with CreateAsDownstreamNode flas as true
            var cmdOne = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, true, false);

            //This command is created with CreateAsDownstreamNode flas as false
            var cmdTwo = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, false, false);

            //Act
            //This will execute the method CreateAndConnectNodeImpl for both commands
            CurrentDynamoModel.ExecuteCommand(cmdOne);
            CurrentDynamoModel.ExecuteCommand(cmdTwo);

            //Assert
            Assert.IsNotNull(cmdOne);
            Assert.IsNotNull(cmdTwo);
        }

        [Test]
        [Category("UnitTests")]
        public void DynamoModel_GetNodeFromCommand()
        {
            //Arrange
            Guid newNodeGuid = Guid.NewGuid();
            Guid existingNodeGuid = new Guid("81c94fd0-35a0-4680-8535-00aff41192d3");
            double x = 100;
            double y = 190;

            string openPath = Path.Combine(TestDirectory, @"core\DetailedPreviewMargin_Test.dyn");
            RunModel(openPath);

            //This command is created with CreateAsDownstreamNode flas as true
            var cmdOne = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, true, false);

            //This command is created with CreateAsDownstreamNode flas as false
            var cmdTwo = new DynamoModel.CreateAndConnectNodeCommand(newNodeGuid, existingNodeGuid,
                "CoreNodeModels.CreateList", 0, 0, x, y, false, false);

            //Act
            //This will execute the method CreateAndConnectNodeImpl for both commands
            CurrentDynamoModel.ExecuteCommand(cmdOne);
            CurrentDynamoModel.ExecuteCommand(cmdTwo);

            //Assert
            Assert.IsNotNull(cmdOne);
            Assert.IsNotNull(cmdTwo);
        }
    }
}

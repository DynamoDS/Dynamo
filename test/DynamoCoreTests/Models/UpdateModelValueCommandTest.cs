using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;
using DynCmd = Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class UpdateModelValueCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the  UpdateModelValueCommand class
        /// public UpdateModelValueCommand(string modelGuid, string name, string value)
        /// public UpdateModelValueCommand(Guid modelGuid, string name, string value)
        /// public override string ToString()
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void UpdateModelValueCommand_Constructor()
        {
            //Arrange
            var cbn = CreateCodeBlockNode();
            IEnumerable<Guid> modelGuid = new[] { cbn.GUID };

            //Act
            //Executing the overloaded constructors
            var command1 = new UpdateModelValueCommand(cbn.GUID.ToString(), "nameModel", "valueModel");
            var command2 = new UpdateModelValueCommand(modelGuid, "nameModel", "valueModel");

            //Assert
            //Checking that the commands were created correctly
            Assert.IsNotNull(command1);
            Assert.IsNotNull(command2);
            Assert.Throws<NotImplementedException>(() => command1.ToString());
        }

        private CodeBlockNodeModel CreateCodeBlockNode()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynCmd.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);
            return cbn;
        }
    }
}

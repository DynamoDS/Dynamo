using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class UndoRedoCommandTest
    {
        /// <summary>
        /// This test method will execute the TrackAnalytics() method from the UndoRedoCommand class
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void UndoRedoCommandTest_TrackAnalytics()
        {
            //Arrange
            var command = new UndoRedoCommand(UndoRedoCommand.Operation.Undo);

            //Act
            command.TrackAnalytics();

            //Assert
            //In this section we are just checking that the command instance is valid and has the correct values
            Assert.IsNotNull(command);
            Assert.AreEqual(command.CmdOperation, UndoRedoCommand.Operation.Undo);
      
        }
    }
}

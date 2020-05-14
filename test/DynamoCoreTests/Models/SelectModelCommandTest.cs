using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Utilities;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class SelectModelCommandTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods in the SelectModelCommand class
        /// public SelectModelCommand(string modelGuid, ModifierKeys modifiers)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void SelectModelCommand_Constructor()
        {
            //Arrange
            Guid commandGuid = Guid.NewGuid();

            //Act
            var command = new SelectModelCommand(commandGuid.ToString(), ModifierKeys.Control);

            //Assert
            Assert.AreEqual(command.ModelGuid.ToString(), commandGuid.ToString());
            Assert.AreEqual(command.Modifiers, ModifierKeys.Control);
        }
    }
}

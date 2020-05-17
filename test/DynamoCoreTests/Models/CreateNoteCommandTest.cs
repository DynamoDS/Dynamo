using System;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    [TestFixture]
    class CreateNoteCommandTest
    {
        /// <summary>
        /// This test case will execute a specific contructor for the CreateNoteCommand class
        /// public CreateNoteCommand(string nodeId, string noteText,double x, double y, bool defaultPosition)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void CreateNoteCommand_Constructor()
        {
            //Arrange
            Guid newNoteGuid = Guid.NewGuid();

            //Act
            var noteCommand = new CreateNoteCommand(newNoteGuid.ToString(),"This is a note text", 100, 50, true);

            //Assert
            //This just validates the some properties inside the CreateNoteCommand class
            Assert.AreEqual(noteCommand.NoteText, "This is a note text");
            Assert.AreEqual(noteCommand.X, 100);
            Assert.AreEqual(noteCommand.Y, 50);

        }
    }
}

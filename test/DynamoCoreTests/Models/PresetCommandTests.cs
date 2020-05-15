using System;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Selection;
using NUnit.Framework;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class will contain test cases for executing methods in the AddPresetCommand and ApplyPresetCommand clases
    /// </summary>
    [TestFixture]
    class PresetCommandTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next method from the AddPresetCommand class
        /// public AddPresetCommand(string name, string description, IEnumerable<Guid> currentSelectionIds )
        /// static AddPresetCommand DeserializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void AddPresetCommand_DeserializeCore()
        {
            //Arrange
            var ids = DynamoSelection.Instance.Selection.OfType<NodeModel>().Select(x => x.GUID).ToList();
            var AddPresetCommand = new DynamoModel.AddPresetCommand("PresetName", "Description", ids);
            XmlDocument xmlDocument = new XmlDocument();        
            XmlElement elemTest = xmlDocument.CreateElement("TestCommand");

            //Assert
            //Because we don't have guids it will reach the Exception section in de DeserializeCore method
            Assert.Throws<ArgumentNullException>( () => DynamoModel.AddPresetCommand.DeserializeCore(elemTest));
        }

        /// <summary>
        /// This test method will execute the next methods:
        /// ApplyPresetCommand(Guid workspaceID, Guid stateID)
        /// static ApplyPresetCommand DeserializeCore(XmlElement element)
        /// void ExecuteCore(DynamoModel dynamoModel)
        /// void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void ApplyPresetCommand_ExecuteCore()
        {
            //Arrange
            XmlDocument xmlDocument = new XmlDocument();
            Guid stateGuid = Guid.NewGuid();

            //Act
            var AddPresetCommand = new DynamoModel.ApplyPresetCommand(CurrentDynamoModel.CurrentWorkspace.Guid, stateGuid);
            AddPresetCommand.Execute(CurrentDynamoModel);
            var xmlElementPreset = AddPresetCommand.Serialize(xmlDocument);

            //Assert
            //Just checking that the xmlelement created with the Serialize method was successfull
            Assert.IsNotNull(xmlElementPreset);
            Assert.AreEqual(xmlElementPreset.GetAttribute("WorkspaceID"), CurrentDynamoModel.CurrentWorkspace.Guid.ToString());
        }
    }
}

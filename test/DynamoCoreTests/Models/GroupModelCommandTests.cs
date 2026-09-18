using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Selection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static Dynamo.Models.DynamoModel;

namespace Dynamo.Tests.ModelsTest
{
    /// <summary>
    /// This test class contains methods for testing the  UngroupModelCommand and AddModelToGroupCommand classes
    /// </summary>
    [TestFixture]
    class GroupModelCommandTests : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next constructors from the UngroupModelCommand class
        /// public UngroupModelCommand(string modelGuid)
        /// public UngroupModelCommand(Guid modelGuid)
        /// void SerializeCore(XmlElement element) method
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void UngroupModelCommand_Constructors()
        {
            //Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            XmlDocument xmlDocument = new XmlDocument();

            //Act
            var command1 = new UngroupModelCommand(guid1.ToString());
            var command2 = new UngroupModelCommand(guid2);
            var serializedCommand = command1.Serialize(xmlDocument);

            //Assert
            //Verify that the guid in the commands created are right
            Assert.IsNotNull(command1);
            Assert.AreEqual(command1.ModelGuid.ToString(), guid1.ToString());
            Assert.IsNotNull(command2);
            Assert.AreEqual(command2.ModelGuid.ToString(), guid2.ToString());
            Assert.IsNotNull(serializedCommand);
        }

        /// <summary>
        /// This test method will execute the next constructors from the AddModelToGroupCommand class
        /// public AddModelToGroupCommand(string modelGuid) 
        /// public AddModelToGroupCommand(Guid modelGuid)
        /// public AddModelToGroupCommand(IEnumerable<Guid> modelGuid)
        /// protected override void ExecuteCore(DynamoModel dynamoModel)
        /// protected override void SerializeCore(XmlElement element)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void AddModelToGroupCommand_Constructors()
        {
            //Arrange
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();
            IEnumerable<Guid> groupModelGuid = new[] { guid3 };
            XmlDocument xmlDocument = new XmlDocument();
            var xmlElement = xmlDocument.CreateElement("ModelGroupElement");

            //Act
            var command1 = new AddModelToGroupCommand(guid1.ToString());
            var command2 = new AddModelToGroupCommand(guid2);
            var command3 = new AddModelToGroupCommand(groupModelGuid);

            AddModelToGroupCommand.DeserializeCore(xmlElement);
            command1.Serialize(xmlDocument);

            //Assert
            //Verify that the guid in the commands created are right
            Assert.IsNotNull(command1);
            Assert.IsNotNull(command2);
            Assert.IsNotNull(command3);
            Assert.AreEqual(Guid.Empty, command1.HostGroupGuid);
            Assert.AreEqual(Guid.Empty, command2.HostGroupGuid);
            Assert.AreEqual(Guid.Empty, command3.HostGroupGuid);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidProvidedAndGroupNotSelectedThenNodeIsAdded()
        {
            // Arrange
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();

            DynamoSelection.Instance.ClearSelection();
            Assert.IsFalse(group.IsSelected);

            var command = new AddModelToGroupCommand(nodeToAdd.GUID, group.GUID);

            // Act
            CurrentDynamoModel.ExecuteCommand(command);

            // Assert
            Assert.IsTrue(group.Nodes.Contains(nodeToAdd));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidProvidedAndNodeIdIsMissingThenThrowsAndDoesNotAdd()
        {
            // Arrange
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var countBefore = group.Nodes.Count();
            DynamoSelection.Instance.ClearSelection();

            var command = new AddModelToGroupCommand(Guid.NewGuid(), group.GUID);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(
                () => CurrentDynamoModel.ExecuteCommand(command));
            Assert.AreEqual(countBefore, group.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidProvidedAndNodeAlreadyInGroupThenSucceeds()
        {
            // Arrange — adding a node that is already in the group is a no-op, not a failure
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            DynamoSelection.Instance.ClearSelection();

            var command = new AddModelToGroupCommand(groupedNode.GUID, group.GUID);

            // Act
            CurrentDynamoModel.ExecuteCommand(command);

            // Assert
            Assert.IsTrue(group.Nodes.Contains(groupedNode));
            Assert.AreEqual(1, group.Nodes.Count());
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidOmittedAndGroupSelectedExpandedThenNodeIsAdded()
        {
            // Arrange — this is the canvas path: no host id, group must be selected and expanded
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();

            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(group);
            Assert.IsTrue(group.IsSelected);
            Assert.IsTrue(group.IsExpanded);

            var command = new AddModelToGroupCommand(nodeToAdd.GUID);

            // Act
            CurrentDynamoModel.ExecuteCommand(command);

            // Assert
            Assert.IsTrue(group.Nodes.Contains(nodeToAdd));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidOmittedAndGroupNotSelectedThenThrowsAndDoesNotAdd()
        {
            // Arrange
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();
            var countBefore = group.Nodes.Count();

            DynamoSelection.Instance.ClearSelection();
            var command = new AddModelToGroupCommand(nodeToAdd.GUID);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(
                () => CurrentDynamoModel.ExecuteCommand(command));
            Assert.AreEqual(countBefore, group.Nodes.Count());
            Assert.IsFalse(group.Nodes.Contains(nodeToAdd));
        }
        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidOmittedAndGroupCollapsedThenThrowsAndDoesNotAdd()
        {
            // Arrange
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();
            var countBefore = group.Nodes.Count();

            group.IsExpanded = false;
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(group);

            var command = new AddModelToGroupCommand(nodeToAdd.GUID);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(
                () => CurrentDynamoModel.ExecuteCommand(command));
            Assert.AreEqual(countBefore, group.Nodes.Count());
            Assert.IsFalse(group.Nodes.Contains(nodeToAdd));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenHostGuidProvidedAndGroupCollapsedThenThrowsAndDoesNotAdd()
        {
            // Arrange — even with an explicit id, a collapsed group must fail
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();
            var countBefore = group.Nodes.Count();

            group.IsExpanded = false;
            var command = new AddModelToGroupCommand(nodeToAdd.GUID, group.GUID);

            // Act / Assert
            Assert.Throws<InvalidOperationException>(
                () => CurrentDynamoModel.ExecuteCommand(command));
            Assert.AreEqual(countBefore, group.Nodes.Count());
            Assert.IsFalse(group.Nodes.Contains(nodeToAdd));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenOldCommandXmlHasNoHostAttributeThenHostGuidIsEmpty()
        {
            // Arrange — old recorded commands only stored the node id
            var nodeGuid = Guid.NewGuid();
            var xmlDocument = new XmlDocument();
            var element = xmlDocument.CreateElement(nameof(AddModelToGroupCommand));
            var modelGuidNode = xmlDocument.CreateElement("ModelGuid");
            modelGuidNode.InnerText = nodeGuid.ToString();
            element.AppendChild(modelGuidNode);

            // Act
            var command = AddModelToGroupCommand.DeserializeCore(element);
            // Assert
            Assert.AreEqual(nodeGuid, command.ModelGuid);
            Assert.AreEqual(Guid.Empty, command.HostGroupGuid);
        }

        [Test]
        [Category("UnitTests")]
        public void WhenAddingByHostGuidThenUndoRemovesTheNode()
        {
            // Arrange
            var groupedNode = CreateNode();
            var group = CreateGroupAround(groupedNode);
            var nodeToAdd = CreateNode();

            DynamoSelection.Instance.ClearSelection();
            CurrentDynamoModel.ExecuteCommand(
                new AddModelToGroupCommand(nodeToAdd.GUID, group.GUID));
            Assert.IsTrue(group.Nodes.Contains(nodeToAdd));
            Assert.IsTrue(CurrentDynamoModel.CurrentWorkspace.CanUndo);

            // Act
            CurrentDynamoModel.CurrentWorkspace.Undo();

            // Assert
            Assert.IsFalse(group.Nodes.Contains(nodeToAdd));
            Assert.IsTrue(group.Nodes.Contains(groupedNode));
        }

        [Test]
        [Category("UnitTests")]
        public void WhenCommandWithHostGuidIsSerializedThenDeserializeKeepsHostGuid()
        {
            // Arrange
            var nodeGuid = Guid.NewGuid();
            var hostGuid = Guid.NewGuid();
            var command = new AddModelToGroupCommand(nodeGuid, hostGuid);
            var xmlDocument = new XmlDocument();

            // Act
            var element = command.Serialize(xmlDocument);
            var deserialized = AddModelToGroupCommand.DeserializeCore(element);

            // Assert
            Assert.AreEqual(nodeGuid, deserialized.ModelGuid);
            Assert.AreEqual(hostGuid, deserialized.HostGroupGuid);
        }

        private DummyNode CreateNode()
        {
            var node = new DummyNode();
            CurrentDynamoModel.CurrentWorkspace.AddAndRegisterNode(node, false);
            return node;
        }

        private AnnotationModel CreateGroupAround(DummyNode node)
        {
            DynamoSelection.Instance.ClearSelection();
            DynamoSelection.Instance.Selection.Add(node);
            return CurrentDynamoModel.CurrentWorkspace.AddAnnotation("test group", Guid.NewGuid());
        }
    }
}

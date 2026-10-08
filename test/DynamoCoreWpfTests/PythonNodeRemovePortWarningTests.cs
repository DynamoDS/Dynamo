using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.Wpf.Utilities;
using DynamoCoreWpfTests.Utility;
using Moq;
using NUnit.Framework;
using PythonNodeModels;

namespace DynamoCoreWpfTests
{
    /// <summary>
    /// Covers the "Remove Port?" warning raised by the '-' button on a Python node.
    /// The warning must appear only when the port being removed carries custom properties
    /// that the user would lose, rather than on every removal. See DYN-10569.
    /// </summary>
    /// <remarks>
    /// Not tagged "UnitTests": each case starts a full DynamoView via DynamoTestUIBase and takes
    /// minutes. The comparison logic itself is covered cheaply by the HasCustomInputPortProperties
    /// tests in DynamoPythonTests; this fixture exists to verify the button actually consults it.
    /// </remarks>
    [Category("RegressionTests")]
    public class PythonNodeRemovePortWarningTests : DynamoTestUIBase
    {
        private Mock<MessageBoxService.IMessageBox> dialogMock;

        [TearDown]
        public void ResetMessageBoxOverride()
        {
            // The override is a static field on MessageBoxService, so it would otherwise
            // stay installed for every fixture that runs after this one.
            MessageBoxService.OverrideMessageBoxDuringTests(null);
            dialogMock = null;
        }

        /// <summary>
        /// Installs a recording message box that answers <paramref name="answer"/> to any prompt,
        /// so a warning does not block the test and can be asserted on afterwards.
        /// </summary>
        /// <param name="answer">The result the mocked dialog returns, defaulting to OK.</param>
        private void InstallDialogMock(MessageBoxResult answer = MessageBoxResult.OK)
        {
            dialogMock = new Mock<MessageBoxService.IMessageBox>();
            dialogMock
                .Setup(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()))
                .Returns(answer);

            MessageBoxService.OverrideMessageBoxDuringTests(dialogMock.Object);
        }

        /// <summary>
        /// Adds a Python node to the current workspace and returns its realized NodeView.
        /// </summary>
        private NodeView CreatePythonNodeView(out PythonNode pythonNode)
        {
            pythonNode = new PythonNode();
            Model.ExecuteCommand(new DynamoModel.CreateNodeCommand(pythonNode, 0, 0, true, false));
            DispatcherUtil.DoEventsLoop();

            return NodeViewOf<PythonNode>();
        }

        /// <summary>
        /// Returns the '-' button that VariableInputNodeViewCustomization adds to the node view.
        /// </summary>
        private static DynamoNodeButton RemovePortButton(NodeView nodeView)
        {
            var button = nodeView.inputGrid.ChildrenOfType<DynamoNodeButton>()
                .SingleOrDefault(b => "-".Equals(b.Content));

            Assert.IsNotNull(button, "Expected a single '-' button on the Python node view.");
            return button;
        }

        private static void ClickButton(DynamoNodeButton button)
        {
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            DispatcherUtil.DoEventsLoop();
        }

        private void AssertWarningShown(Times times)
        {
            dialogMock.Verify(m => m.Show(It.IsAny<Window>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>()), times);
        }

        [Test]
        public void WhenRemovingUnmodifiedPythonPortThenNoWarningIsShown()
        {
            // Arrange: a Python node with its single default input port, left untouched.
            InstallDialogMock();
            var nodeView = CreatePythonNodeView(out var pythonNode);
            Assert.AreEqual(1, pythonNode.InPorts.Count);

            // Act: click the '-' button.
            ClickButton(RemovePortButton(nodeView));

            // Assert: nothing was customized, so the user is not prompted and the port just goes.
            AssertWarningShown(Times.Never());
            Assert.AreEqual(0, pythonNode.InPorts.Count);
        }

        [Test]
        public void WhenRemovingRenamedPythonPortThenWarningIsShown()
        {
            // Arrange: a Python node whose last input port has been renamed by the user.
            InstallDialogMock();
            var nodeView = CreatePythonNodeView(out var pythonNode);
            pythonNode.InPorts[0].Name = "myInput";

            // Act: click the '-' button.
            ClickButton(RemovePortButton(nodeView));

            // Assert: the user is warned before the rename is discarded.
            AssertWarningShown(Times.Once());
        }

        [Test]
        public void WhenRemovePortWarningIsCancelledThenPortIsKept()
        {
            // Arrange: a renamed port, so clicking '-' raises the warning. The mocked dialog
            // answers Cancel, standing in for the user declining.
            InstallDialogMock(MessageBoxResult.Cancel);
            var nodeView = CreatePythonNodeView(out var pythonNode);
            pythonNode.InPorts[0].Name = "myInput";

            // Act: click the '-' button and decline the warning.
            ClickButton(RemovePortButton(nodeView));

            // Assert: declining aborts the removal outright - the port and its custom name survive.
            // Without this, nothing verifies that Cancel is honoured rather than ignored.
            AssertWarningShown(Times.Once());
            Assert.AreEqual(1, pythonNode.InPorts.Count);
            Assert.AreEqual("myInput", pythonNode.InPorts[0].Name);
        }

        [Test]
        public void WhenRemovingUnmodifiedLastPortWhileEarlierPortIsRenamedThenNoWarningIsShown()
        {
            // Arrange: two input ports where only the FIRST is renamed. The '-' button removes the
            // LAST port, which is untouched, so no customization is actually at risk.
            InstallDialogMock();
            var nodeView = CreatePythonNodeView(out var pythonNode);
            pythonNode.HandleModelEvent("AddInPort", 0, null);
            DispatcherUtil.DoEventsLoop();
            Assert.AreEqual(2, pythonNode.InPorts.Count);
            pythonNode.InPorts[0].Name = "myInput";

            // Act: click the '-' button.
            ClickButton(RemovePortButton(nodeView));

            // Assert: the gate must inspect the port being removed rather than a fixed index,
            // so an unrelated rename on an earlier port must not raise the warning.
            AssertWarningShown(Times.Never());
            Assert.AreEqual(1, pythonNode.InPorts.Count);
        }

        [Test]
        public void WhenRemovingRenamedLastPortWhileEarlierPortIsDefaultThenWarningIsShown()
        {
            // Arrange: two input ports where only the LAST one - the one that will be removed -
            // is renamed. This is the mirror of the test above.
            InstallDialogMock();
            var nodeView = CreatePythonNodeView(out var pythonNode);
            pythonNode.HandleModelEvent("AddInPort", 0, null);
            DispatcherUtil.DoEventsLoop();
            pythonNode.InPorts[1].Name = "myInput";

            // Act: click the '-' button.
            ClickButton(RemovePortButton(nodeView));

            // Assert: the user is warned before the rename on the last port is discarded.
            AssertWarningShown(Times.Once());
        }

        [Test]
        public void WhenRemovingRetooltippedPythonPortThenWarningIsShown()
        {
            // Arrange: a customized description alone must also trigger the warning, so that
            // the gate cannot be narrowed to only check the port name.
            InstallDialogMock();
            var nodeView = CreatePythonNodeView(out var pythonNode);
            pythonNode.InPorts[0].ToolTip = "my description";

            // Act: click the '-' button.
            ClickButton(RemovePortButton(nodeView));

            // Assert: the user is warned before the description is discarded.
            AssertWarningShown(Times.Once());
        }
    }
}

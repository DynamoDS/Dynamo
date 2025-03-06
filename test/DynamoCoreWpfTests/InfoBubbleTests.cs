using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using CoreNodeModels.Input;
using Dynamo.Graph;
using Dynamo.Graph.Workspaces;

namespace DynamoCoreWpfTests
{
    [TestFixture]
    public class InfoBubbleTests : DynamoViewModelUnitTest
    {
        [Test]
        public void CanTriggerDocumentationLinkEvent()
        {
            // Arrange
            InfoBubbleViewModel infoBubble = new InfoBubbleViewModel(this.ViewModel);
            const string documentationFile = "dynamo.html";
            string content = $"This is the test infoBubble href={documentationFile}";
            InfoBubbleDataPacket infoBubbleDataPacket = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Error,
                new Point(0, 0), new Point(0, 0), content, InfoBubbleViewModel.Direction.Bottom);
            var docEventArgs = new OpenDocumentationLinkEventArgs(infoBubbleDataPacket.Link);

            // register an extra handler for documentation events so we can confirm
            // the Dynamo ViewModel event gets triggered from the InfoBubble viewmodel
            RequestOpenDocumentationLinkHandler handler = (OpenDocumentationLinkEventArgs e) =>
            {
                Assert.IsNotNull(e);
                Assert.AreEqual(documentationFile, e.Link.ToString());
            };
            this.ViewModel.RequestOpenDocumentationLink += handler;

            // Act
            // test we can pass in content with a link and it gets set
            if (infoBubble.UpdateContentCommand.CanExecute(null))
                infoBubble.UpdateContentCommand.Execute(infoBubbleDataPacket);
            // test the InfoBubbleViewModel raises the right DynamoViewModel event
            if (infoBubble.OpenDocumentationLinkCommand.CanExecute(null))
                infoBubble.OpenDocumentationLinkCommand.Execute(docEventArgs.Link);

            // Assert
            Assert.IsNotNull(infoBubble.DocumentationLink);
            Assert.AreEqual(documentationFile, infoBubble.DocumentationLink.ToString());
            this.ViewModel.RequestOpenDocumentationLink -= handler;
        }

        [Test]
        public void DataPacketConstructorCanParseLinksCorrectly()
        {
            // Arrange
            const string i = "href=";
            var goodLink = "ExcelNotInstalled.html";
            var badWhitespaceLink = " ";
            var partialDotLink = ". ";
            var goodRemoteLink = "https://dictionary.dynamobim.org/#/";
            var partialRemoteLink = ".com";
            var badIdentifierIncomplete = "href";
            var badIdentifierTypo = "hre=";

            string content = "This is the base bubble message.";

            // Assert
            AssertParsedLinkIsEqualTo(content + i + goodLink, goodLink);
            AssertParsedLinkIsEqualTo(content + goodLink, null);
            AssertParsedLinkIsEqualTo(content + i + badWhitespaceLink, null);
            AssertParsedLinkIsEqualTo(content + i + partialDotLink, NullIfSystemUriCannotParseValue(partialDotLink));
            AssertParsedLinkIsEqualTo(content + i + goodRemoteLink, goodRemoteLink);
            AssertParsedLinkIsEqualTo(content + goodRemoteLink, null);
            AssertParsedLinkIsEqualTo(content + i + partialRemoteLink, NullIfSystemUriCannotParseValue(partialRemoteLink));
            AssertParsedLinkIsEqualTo(content + badIdentifierIncomplete + goodLink, null);
            AssertParsedLinkIsEqualTo(content + badIdentifierTypo + goodLink, null);
        }
        [Test]
        public void DataPacketConstructorCanParseLinksCorrectly_ContainingMultipleHREFs()
        {
            // Arrange
            const string i = "href=";
            var goodLink = "ExcelNotInstalled.html";

            string content = "This is the base bubble message.";

            // Assert
            AssertParsedLinkIsEqualTo(content + i + goodLink + Environment.NewLine +
                content + i + goodLink + Environment.NewLine +
                content + i + goodLink + Environment.NewLine, goodLink);
            AssertParsedLinkIsEqualTo(content + i + goodLink + Environment.NewLine +
              content + i + goodLink + Environment.NewLine +
              content + i + goodLink, goodLink);
        }

        /// <summary>
        /// Used to check whether a node can have info messages, warnings and errors added to it which are then displayed to the user.
        /// </summary>
        [Test]
        public void CanAddUserFacingMessagesToNode()
        {
            // Arrange
            OpenModel(@"core\Home.dyn");

            var info1 = "Info 1";
            var warning1 = "Warning 1";
            var error1 = "Error 1";

            var dummyNode = new DummyNode();
            DynamoModel model = GetModel();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, true));

            NodeViewModel dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == dummyNode.GUID);

            NodeModel dummyNodeModel = dummyNodeViewModel.NodeModel;

            var topLeft = new Point(dummyNodeModel.X, dummyNodeModel.Y);
            var botRight = new Point(dummyNodeModel.X + dummyNodeModel.Width, dummyNodeModel.Y + dummyNodeModel.Height);

            if (dummyNodeViewModel.ErrorBubble == null)
            {
                dummyNodeViewModel.ErrorBubble = new InfoBubbleViewModel(ViewModel);
            }
            
            InfoBubbleViewModel infoBubbleViewModel = dummyNodeViewModel.ErrorBubble;

            // The collection of messages the node receives
            ObservableCollection<InfoBubbleDataPacket> nodeMessages = infoBubbleViewModel.NodeMessages;
            
            // The collections of messages the node displays to the user
            ObservableCollection<InfoBubbleDataPacket> userFacingInfo = infoBubbleViewModel.NodeInfoToDisplay;
            ObservableCollection<InfoBubbleDataPacket> userFacingWarnings = infoBubbleViewModel.NodeWarningsToDisplay;
            ObservableCollection<InfoBubbleDataPacket> userFacingErrors = infoBubbleViewModel.NodeErrorsToDisplay;

            // Act
            Assert.AreEqual(0, userFacingInfo.Count);
            Assert.AreEqual(0, userFacingWarnings.Count);
            Assert.AreEqual(0, userFacingErrors.Count);

            Assert.IsFalse(infoBubbleViewModel.NodeInfoVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeWarningsVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeErrorsVisible);

            nodeMessages.Add(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Info, topLeft, botRight, info1, InfoBubbleViewModel.Direction.Top));
            nodeMessages.Add(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Warning, topLeft, botRight, warning1, InfoBubbleViewModel.Direction.Top));
            nodeMessages.Add(new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Error, topLeft, botRight, error1, InfoBubbleViewModel.Direction.Top));

            // Assert
            Assert.AreEqual(1, userFacingInfo.Count);
            Assert.AreEqual(1, userFacingWarnings.Count);
            Assert.AreEqual(1, userFacingErrors.Count);

            Assert.IsTrue(infoBubbleViewModel.NodeInfoVisible);
            Assert.IsTrue(infoBubbleViewModel.NodeWarningsVisible);
            Assert.IsTrue(infoBubbleViewModel.NodeErrorsVisible);

            Assert.IsFalse(infoBubbleViewModel.NodeInfoIteratorVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeWarningsIteratorVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeErrorsIteratorVisible);

            Assert.IsFalse(infoBubbleViewModel.NodeInfoShowMoreButtonVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeWarningsShowMoreButtonVisible);
            Assert.IsFalse(infoBubbleViewModel.NodeErrorsShowMoreButtonVisible);

            Assert.AreEqual(info1, userFacingInfo.First().Message);
            Assert.AreEqual(warning1, userFacingWarnings.First().Message);
            Assert.AreEqual(error1, userFacingErrors.First().Message);
        }

        [Test]
        public void TestIntSliderInfoState()
        {
            var slider = new IntegerSlider64Bit();
            Assert.NotNull(slider);

            DynamoModel model = GetModel();
            (model.CurrentWorkspace as HomeWorkspaceModel).RunSettings.RunType = RunType.Manual;
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(slider, 0, 0, true, true));

            NodeViewModel sliderNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == slider.GUID);

            NodeModel sliderNodeModel = sliderNodeViewModel.NodeModel;

            var param = new UpdateValueParams("Value", "9223372036854775808");
            slider.UpdateValue(param);

            Assert.AreEqual(sliderNodeModel.Infos.Count, 1);
            Assert.AreEqual(slider.Value, Int64.MaxValue);

            // After graph run, persistent info still displays
            model.CurrentWorkspace.RequestRun();
            Assert.AreEqual(sliderNodeModel.Infos.Count, 1);
            Assert.AreEqual(slider.Value, Int64.MaxValue);
        }

        /// <summary>
        /// Used to check whether a node's message can be dismissed and are then hidden from the user.
        /// </summary>
        [Test]
        public void CanDismissUserFacingMessages()
        {
            OpenModel(@"core\Home.dyn");

            var info1 = "Dismissed Info 1";

            // Arrange
            var dummyNode = new DummyNode();
            DynamoModel model = GetModel();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, true));

            NodeViewModel dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == dummyNode.GUID);

            NodeModel dummyNodeModel = dummyNodeViewModel.NodeModel;

            var topLeft = new Point(dummyNodeModel.X, dummyNodeModel.Y);
            var botRight = new Point(dummyNodeModel.X + dummyNodeModel.Width, dummyNodeModel.Y + dummyNodeModel.Height);

            if (dummyNodeViewModel.ErrorBubble == null)
            {
                dummyNodeViewModel.ErrorBubble = new InfoBubbleViewModel(ViewModel);
            }

            InfoBubbleViewModel infoBubbleViewModel = dummyNodeViewModel.ErrorBubble;

            // The collection of messages the node receives
            ObservableCollection<InfoBubbleDataPacket> nodeMessages = infoBubbleViewModel.NodeMessages;
            
            // Act
            InfoBubbleDataPacket infoBubbleDataPacket = new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Info, topLeft, botRight, info1, InfoBubbleViewModel.Direction.Top);
            nodeMessages.Add(infoBubbleDataPacket);

            Assert.AreEqual(1, infoBubbleViewModel.NodeInfoToDisplay.Count);
            Assert.AreEqual(0, infoBubbleViewModel.DismissedMessages.Count);
            Assert.IsTrue(infoBubbleViewModel.NodeInfoVisible);

            infoBubbleViewModel.DismissMessageCommand.Execute(infoBubbleDataPacket);

            // Assert
            Assert.AreEqual(0, infoBubbleViewModel.NodeInfoToDisplay.Count);
            Assert.AreEqual(1, infoBubbleViewModel.DismissedMessages.Count);
            Assert.AreEqual(info1, infoBubbleViewModel.DismissedMessages.First().Message);
        }

        /// <summary>
        /// Used to check whether the node body displays an interating count beside each message when it contains multiple messages.
        /// </summary>
        [Test]
        public void IteratorsDisplayWhenMoreThanOneMessage()
        {
            // Arrange
            OpenModel(@"core\Home.dyn");

            var info1 = "Info 1";
            var info2 = "Info 2";

            var dummyNode = new DummyNode();
            DynamoModel model = GetModel();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, true));

            NodeViewModel dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == dummyNode.GUID);

            if (dummyNodeViewModel.ErrorBubble == null)
            {
                dummyNodeViewModel.ErrorBubble = new InfoBubbleViewModel(ViewModel);
            }

            NodeModel dummyNodeModel = dummyNodeViewModel.NodeModel;

            var topLeft = new Point(dummyNodeModel.X, dummyNodeModel.Y);
            var botRight = new Point(dummyNodeModel.X + dummyNodeModel.Width, dummyNodeModel.Y + dummyNodeModel.Height);

            InfoBubbleViewModel infoBubbleViewModel = dummyNodeViewModel.ErrorBubble;

            // The collection of messages the node receives
            ObservableCollection<InfoBubbleDataPacket> nodeMessages = infoBubbleViewModel.NodeMessages;

            // Act
            Assert.IsFalse(infoBubbleViewModel.NodeInfoIteratorVisible);
            
            InfoBubbleDataPacket infoBubbleDataPacket1 = 
                new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Info, topLeft, botRight, info1, InfoBubbleViewModel.Direction.Top);

            nodeMessages.Add(infoBubbleDataPacket1);
            
            Assert.IsFalse(infoBubbleViewModel.NodeInfoIteratorVisible);

            InfoBubbleDataPacket infoBubbleDataPacket2 =
                new InfoBubbleDataPacket(InfoBubbleViewModel.Style.Info, topLeft, botRight, info2, InfoBubbleViewModel.Direction.Top);
            
            nodeMessages.Add(infoBubbleDataPacket2);
            
            Assert.IsTrue(infoBubbleViewModel.NodeInfoIteratorVisible);

            infoBubbleViewModel.DismissMessageCommand.Execute(infoBubbleDataPacket1);

            Assert.IsFalse(infoBubbleViewModel.NodeInfoIteratorVisible);
        }

        /// <summary>
        /// Tests whether the node's warning bar appears when the node has info/warning/error messages to display.
        /// </summary>
        [Test]
        public void NodeWarningBarVisibility()
        {
            OpenModel(@"core\Home.dyn");

            var info = "Information";

            // Arrange
            var dummyNode = new DummyNode();
            DynamoModel model = GetModel();
            model.ExecuteCommand(new DynamoModel.CreateNodeCommand(dummyNode, 0, 0, true, true));

            NodeViewModel dummyNodeViewModel = ViewModel.CurrentSpaceViewModel.Nodes
                .FirstOrDefault(x => x.NodeModel.GUID == dummyNode.GUID);

            NodeModel dummyNodeModel = dummyNodeViewModel.NodeModel;
            
            if (dummyNodeViewModel.ErrorBubble == null)
            {
                dummyNodeViewModel.ErrorBubble = new InfoBubbleViewModel(ViewModel);
            }

            InfoBubbleViewModel infoBubbleViewModel = dummyNodeViewModel.ErrorBubble;

            ObservableCollection<InfoBubbleDataPacket> nodeMessages = infoBubbleViewModel.NodeMessages;

            // Assert
            Assert.IsFalse(dummyNodeViewModel.NodeWarningBarVisible);

            // Act
            InfoBubbleDataPacket infoBubbleDataPacket = new InfoBubbleDataPacket
            (
                InfoBubbleViewModel.Style.Info,
                new Point(dummyNodeModel.X, dummyNodeModel.Y),
                new Point(dummyNodeModel.X + dummyNodeModel.Width, dummyNodeModel.Y + dummyNodeModel.Height),
                info,
                InfoBubbleViewModel.Direction.Top
            );

            nodeMessages.Add(infoBubbleDataPacket);

            Assert.IsTrue(infoBubbleViewModel.NodeInfoVisible);

            infoBubbleViewModel.DismissMessageCommand.Execute(infoBubbleDataPacket);

            Assert.IsFalse(dummyNodeViewModel.NodeWarningBarVisible);
        }

        #region Helpers

        private void AssertParsedLinkIsEqualTo(string content, string expectedLink)
        {
            var packet = new InfoBubbleDataPacket(
                InfoBubbleViewModel.Style.None,
                new Point(0, 0),
                new Point(0, 0),
                content,
                InfoBubbleViewModel.Direction.Top);

            Assert.AreEqual(expectedLink, packet.Link?.ToString());
            if (expectedLink != null)
            {
                Assert.IsTrue(!packet.Text?.Contains("href="));
                Assert.IsTrue(!packet.Text?.Contains(expectedLink));
            }
        }

        private string NullIfSystemUriCannotParseValue(string link)
        {
            Uri uri;
            return Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out uri)
                ? uri.ToString()
                : null;
        }

        #endregion
    }
}

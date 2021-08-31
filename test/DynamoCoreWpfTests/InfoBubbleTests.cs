using Dynamo.Tests;
using Dynamo.ViewModels;
using NUnit.Framework;
using System;
using System.Windows;

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
            var partialDotLink = " . ";
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

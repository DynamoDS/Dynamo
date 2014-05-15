using NUnit.Framework;
using RevitServices.Persistence;

namespace Dynamo.Tests
{
    [TestFixture]
    class DocumentTests : DynamoRevitUnitTestBase
    {
        [Test]
        [TestModel(@"./empty.rfa")]
        public void InitialUIDocumentIsNotNull()
        {
            Assert.IsNotNull(DocumentManager.Instance.CurrentUIDocument);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void OpeningNewDocumentDoesNotSwitchUIDocument()
        {
            var initialDoc = DocumentManager.Instance.CurrentUIDocument;
            DocumentManager.Instance.CurrentUIApplication.OpenAndActivateDocument(_emptyModelPath1);

            // Assert that the active UI document is null
            Assert.IsNull(DocumentManager.Instance.CurrentUIDocument);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void SwitchingViewAwayFromCachedDocumentDisablesRun()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void RunIsDisabledWhenOpeningADocumentinPerspectiveView()
        {
            Assert.Inconclusive("Finish me.");
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void AttachesToOtherWhenTwoDocsAreOpenAndCachedDocumentIsClosed()
        {
            Assert.Inconclusive("Finish me!");
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void WhenActiveDocumentResetIsRequiredVisualizationsAreCleared()
        {
            Assert.Inconclusive("Finish me!");
        }
    }
}

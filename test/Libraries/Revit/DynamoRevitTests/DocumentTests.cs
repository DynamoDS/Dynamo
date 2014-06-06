﻿using System.IO;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices.Persistence;
using RevitTestFramework;

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
            // a reference to the initial document
            var initialDoc = DocumentManager.Instance.CurrentUIApplication.ActiveUIDocument;

            var newDoc = OpenAndActivateNewModel(_emptyModelPath1);

            // Assert that the active UI document is
            // still the initial document
            Assert.AreEqual(DocumentManager.Instance.CurrentUIDocument.Document.PathName, initialDoc.Document.PathName);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void SwitchingViewAwayFromCachedDocumentDisablesRun()
        {
            // empty.rfa will be open at test start
            // swap documents and ensure that 
            var initialDoc = DocumentManager.Instance.CurrentUIDocument;
            var newDoc = OpenAndActivateNewModel(_emptyModelPath1);

            Assert.False(dynSettings.Controller.DynamoViewModel.RunEnabled);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void RunIsDisabledWhenOpeningADocumentinPerspectiveView()
        {
            // Swap to a document that only has one open perspective view
            SwapCurrentModel(Path.Combine(_testPath, "model_with_box.rvt"));

            Assert.False(dynSettings.Controller.DynamoViewModel.RunEnabled);

            // Then you need to swap back because the journal's ID_FLUSH_UNDO
            // is disabled in perspective as well

            SwapCurrentModel(_emptyModelPath);
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void AttachesToNewDocumentWhenAllDocsWereClosed()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }

        [Test]
        [TestModel(@"./empty.rfa")]
        public void WhenActiveDocumentResetIsRequiredVisualizationsAreCleared()
        {
            Assert.Inconclusive("Cannot test. API required for allowing closing all docs.");
        }
    }
}

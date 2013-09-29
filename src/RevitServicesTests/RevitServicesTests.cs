using Autodesk.Revit.DB;
using Dynamo.Utilities;
using NUnit.Framework;
using RevitServices;

namespace RevitServicesTests
{
    [TestFixture]
    public class RevitServicesTests
    {
        Document Document
        {
            get { return dynRevitSettings.Doc.Document; }
        }

        [Test]
        public void MakePoint()
        {
            var transManager = new TransactionManager();
            transManager.StartTransaction(Document);

            var id = Document.FamilyCreate.NewReferencePoint(new XYZ(0, 0, 0)).Id;

            transManager.CommitTransaction();

            ReferencePoint rp;
            Assert.IsTrue(Document.TryGetElement(id, out rp));
            Assert.AreEqual(id, rp.Id);
        }

        [Test]
        public void MakePointThenCancel()
        {
            var transManager = new TransactionManager();
            transManager.StartTransaction(Document);

            var id = Document.FamilyCreate.NewReferencePoint(new XYZ(0, 0, 0)).Id;

            transManager.CancelTransaction();

            ReferencePoint rp;
            Assert.IsFalse(Document.TryGetElement(id, out rp));
        }

        [Test]
        public void TransactionStartedEventFires()
        {
            bool eventWasFired = false;

            var transManager = new TransactionManager();
            transManager.TransactionStarted += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);
            transManager.StartTransaction(Document);

            Assert.IsTrue(eventWasFired);
            transManager.CancelTransaction();
        }

        [Test]
        public void TransactionCommittedEventFires()
        {
            bool eventWasFired = false;

            var transManager = new TransactionManager();
            transManager.TransactionCommitted += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);

            transManager.StartTransaction(Document);
            Assert.IsFalse(eventWasFired);

            transManager.CommitTransaction();
            Assert.IsTrue(eventWasFired);
        }

        [Test]
        public void TransactionCancelledEventFires()
        {
            bool eventWasFired = false;

            var transManager = new TransactionManager();
            transManager.TransactionCancelled += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);

            transManager.StartTransaction(Document);
            Assert.IsFalse(eventWasFired);

            transManager.CancelTransaction();
            Assert.IsTrue(eventWasFired);
        }

        [Test]
        public void TransactionActive()
        {
            var transManager = new TransactionManager();
            Assert.IsFalse(transManager.TransactionActive);

            transManager.StartTransaction(Document);
            Assert.IsTrue(transManager.TransactionActive);

            transManager.CancelTransaction();
            Assert.IsFalse(transManager.TransactionActive);

            transManager.StartTransaction(Document);
            Assert.IsTrue(transManager.TransactionActive);

            transManager.CommitTransaction();
            Assert.IsFalse(transManager.TransactionActive);
        }

        [Test]
        public void TransactionManagerStatus()
        {
            var transManager = new TransactionManager();
            Assert.AreEqual(TransactionStatus.Uninitialized, transManager.TransactionStatus);

            transManager.StartTransaction(Document);
            Assert.AreEqual(TransactionStatus.Started, transManager.TransactionStatus);

            transManager.CommitTransaction();
        }

        [Test]
        public void FailuresRaisedEvent()
        {
            //TODO
            Assert.Inconclusive("TODO: find an example that would cause revit to emit failures");
        }
    }
}

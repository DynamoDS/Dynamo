using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Autodesk.Revit.DB;
using NUnit.Framework;
using System;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace RevitServicesTests
{
    [TestFixture]
    public class RevitServicesTests
    {
        Document Document
        {
            get { return DocumentManager.Instance.CurrentUIDocument.Document; }
        }

        [Test]
        public void MakePoint()
        {
            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            var t = transManager.StartTransaction(Document);

            var id = Document.FamilyCreate.NewReferencePoint(new XYZ(0, 0, 0)).Id;

            t.CommitTransaction();

            ReferencePoint rp;
            Assert.IsTrue(Document.TryGetElement(id, out rp));
            Assert.AreEqual(id, rp.Id);
        }

        [Test]
        public void MakePointThenCancel()
        {
            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            var t = transManager.StartTransaction(Document);

            var id = Document.FamilyCreate.NewReferencePoint(new XYZ(0, 0, 0)).Id;

            t.CancelTransaction();

            ReferencePoint rp;
            Assert.IsFalse(Document.TryGetElement(id, out rp));
        }

        [Test]
        public void TransactionStartedEventFires()
        {
            bool eventWasFired = false;

            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            transManager.TransactionStarted += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);
            var t = transManager.StartTransaction(Document);

            Assert.IsTrue(eventWasFired);
            t.CancelTransaction();
        }

        [Test]
        public void TransactionCommittedEventFires()
        {
            bool eventWasFired = false;

            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            transManager.TransactionCommitted += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);

            var t = transManager.StartTransaction(Document);
            Assert.IsFalse(eventWasFired);

            t.CommitTransaction();
            Assert.IsTrue(eventWasFired);
        }

        [Test]
        public void TransactionCancelledEventFires()
        {
            bool eventWasFired = false;

            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            transManager.TransactionCancelled += delegate { eventWasFired = true; };

            Assert.IsFalse(eventWasFired);

            var t = transManager.StartTransaction(Document);
            Assert.IsFalse(eventWasFired);

            t.CancelTransaction();
            Assert.IsTrue(eventWasFired);
        }

        [Test]
        public void TransactionActive()
        {
            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;
            Assert.IsFalse(transManager.TransactionActive);

            var t = transManager.StartTransaction(Document);
            Assert.IsTrue(transManager.TransactionActive);

            t.CancelTransaction();
            Assert.IsFalse(transManager.TransactionActive);

            t = transManager.StartTransaction(Document);
            Assert.IsTrue(transManager.TransactionActive);

            t.CommitTransaction();
            Assert.IsFalse(transManager.TransactionActive);
        }

        [Test]
        public void TransactionHandleStatus()
        {
            TransactionManager.SetupManager();
            var transManager = TransactionManager.Instance.TransactionWrapper;

            var t = transManager.StartTransaction(Document);
            Assert.AreEqual(TransactionStatus.Started, t.Status);

            t.CommitTransaction();

            Assert.Throws<InvalidOperationException>(
                () => t.CommitTransaction(), 
                "Cannot commit a transaction that isn't active.");

            Assert.Throws<InvalidOperationException>(
                () => t.CancelTransaction(),
                "Cannot cancel a transaction that isn't active.");
        }

        [Test]
        public void FailuresRaisedEvent()
        {
            //TODO
            Assert.Inconclusive("TODO: find an example that would cause revit to emit failures");
        }


        [Test]
        public void TestRoundTripElementSerialisation()
        {

            // Use a BinaryFormatter or SoapFormatter.
            IFormatter formatter = new BinaryFormatter();
            //IFormatter formatter = new SoapFormatter();


            // Create an instance of the type and serialize it.
            var elementId = new SerializableId
            {
                IntID = 42,
                StringID = "{BE507CAC-7F23-43D6-A2B4-13F6AF09046F}"
            };


            //Serialise to a test memory stream
            var m = new MemoryStream();
            formatter.Serialize(m, elementId);
            m.Flush();


            //Reset the stream
            m.Seek(0, SeekOrigin.Begin);

            //Readback
            var readback = (SerializableId)formatter.Deserialize(m);
            
            Assert.IsTrue(readback.IntID == 42);
            Assert.IsTrue(readback.StringID.Equals("{BE507CAC-7F23-43D6-A2B4-13F6AF09046F}"));

        }

    }
}

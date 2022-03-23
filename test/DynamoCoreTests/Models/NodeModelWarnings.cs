using System;
using System.Collections.Generic;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using NUnit.Framework;


namespace Dynamo.Tests.Models
{
    [TestFixture]
    class NodeModelWarningsTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test case will test adding and removing persistent and transient warnings on a node model
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void TestWarningsOnNodeModel()
        {
            //Arrange
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);

            cbn.Warning("TestPermanent0", true);
            cbn.Warning("TestPermanent1", true);
            cbn.Warning("TestPermanent2", true);

            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(3, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));

            cbn.Warning("TestTransient0", false);
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(4, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestTransient0") && x.State == ElementState.Warning));

            cbn.ClearTransientWarning("TestTransientOther");
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(4, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestTransient0") && x.State == ElementState.Warning));

            cbn.ClearTransientWarning("TestTransient0");
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(3, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual(0, cbn.Infos.Count);

            cbn.Warning("TestPermanent0", true);
            cbn.Warning("TestPermanent1", true);
            cbn.Warning("TestPermanent2", true);

            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(3, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));

            cbn.Warning("TestTransient0", false);                                                        
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(4, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestTransient0") && x.State == ElementState.Warning));

            cbn.ClearTransientWarning();                                                                 
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(3, cbn.Infos.Count);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent2") && x.State == ElementState.PersistentWarning));

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual(0, cbn.Infos.Count);
        }

        /// <summary>
        /// This test case will test transition between transient and persistent warnings
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void KeepTransitionBetweenWarningTypes()
        {
            var cbn = new CodeBlockNodeModel(CurrentDynamoModel.LibraryServices);
            var command = new DynamoModel.CreateNodeCommand(cbn, 0, 0, true, false);

            CurrentDynamoModel.ExecuteCommand(command);

            Assert.IsNotNull(cbn);

            cbn.Warning("TestPermanent0", true);

            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.IsTrue(cbn.Infos.Count == 1);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));

            cbn.Warning("TestTransient0", false);

            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.IsTrue(cbn.Infos.Count == 2);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestTransient0") && x.State == ElementState.Warning));

            cbn.Warning("TestPermanent1", true);
  
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.IsTrue(cbn.Infos.Count == 2);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));

            cbn.Warning("TestTransient1", false);
            Assert.IsTrue(cbn.Infos.Count == 3);
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent0") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestPermanent1") && x.State == ElementState.PersistentWarning));
            Assert.IsTrue(cbn.Infos.Any(x => x.Message.Equals("TestTransient1") && x.State == ElementState.Warning));

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual(0, cbn.Infos.Count);
        }
    }
}
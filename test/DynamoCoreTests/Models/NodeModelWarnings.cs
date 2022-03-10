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
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2" }), cbn.ToolTipText);

            cbn.Warning("TestTransient0", false);
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2", "\r\nTestTransient0" }), cbn.ToolTipText);

            cbn.ClearTransientWarning("TestTransientOther");
            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2", "\r\nTestTransient0" }), cbn.ToolTipText);

            cbn.ClearTransientWarning("TestTransient0");
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2" }), cbn.ToolTipText);

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual("", cbn.ToolTipText);

            cbn.Warning("TestPermanent0", true);
            cbn.Warning("TestPermanent1", true);
            cbn.Warning("TestPermanent2", true);

            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2" }), cbn.ToolTipText);
                                                                                                         
            cbn.Warning("TestTransient0", false);                                                        
            Assert.AreEqual(ElementState.Warning, cbn.State);                                            
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2", "\r\nTestTransient0" }), cbn.ToolTipText);
                                                                                                         
            cbn.ClearTransientWarning();                                                                 
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);                                  
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestPermanent2" }), cbn.ToolTipText);

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual("", cbn.ToolTipText);
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
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0" }), cbn.ToolTipText);

            cbn.Warning("TestTransient0", false);

            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestTransient0" }), cbn.ToolTipText);

            cbn.Warning("TestPermanent1", true);
  
            Assert.AreEqual(ElementState.PersistentWarning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1" }), cbn.ToolTipText);

            cbn.Warning("TestTransient1", false);

            Assert.AreEqual(ElementState.Warning, cbn.State);
            Assert.AreEqual(string.Join("", new List<string>() { "TestPermanent0", "\r\nTestPermanent1", "\r\nTestTransient1" }), cbn.ToolTipText);

            cbn.ClearErrorsAndWarnings();
            Assert.AreEqual(ElementState.Active, cbn.State);
            Assert.AreEqual("", cbn.ToolTipText);
        }
    }
}
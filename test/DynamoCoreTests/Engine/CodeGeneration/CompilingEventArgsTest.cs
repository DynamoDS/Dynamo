using Dynamo.Engine.CodeGeneration;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;

namespace Dynamo.Tests.Engine.CodeGeneration
{
    [TestFixture]
    class CompilingEventArgsTest
    {
        [Test]
        [Category("UnitTests")]
        public void TestInternalMigration()
        {
            //Arrange
            var guid = Guid.NewGuid();
            var associativeNodes = new List<AssociativeNode> { new IntNode(1), new IntNode(2) };
            var eventArgsCompiling = new CompilingEventArgs(guid);
            var eventArgsCompiled = new CompiledEventArgs(guid, associativeNodes);

            //Assert
            Assert.AreEqual(eventArgsCompiling.NodeId, guid);
            Assert.AreEqual(eventArgsCompiled.NodeId, guid);
            Assert.AreEqual((eventArgsCompiled.AstNodes as List<AssociativeNode>).Count, associativeNodes.Count);
        }
    }
}

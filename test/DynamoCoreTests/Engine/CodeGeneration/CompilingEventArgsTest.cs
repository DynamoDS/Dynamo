using System;
using System.Collections.Generic;
using Dynamo.Engine.CodeGeneration;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Tests.Engine.CodeGeneration
{
    [TestFixture]
    class CompilingEventArgsTest
    {
        /// <summary>
        /// This will execute the next methods/properties from the CompilingEventArgs class
        /// public CompilingEventArgs(Guid node)
        /// public Guid NodeId
        /// 
        /// This will execute the next methods/properties from the CompiledEventArgs class
        /// internal CompiledEventArgs(Guid node, IEnumerable<AssociativeNode> astNodes)
        /// public Guid NodeId
        /// public IEnumerable<AssociativeNode> AstNodes
        /// </summary>
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
            //Validates that the Guid values are correct
            Assert.AreEqual(eventArgsCompiling.NodeId, guid);
            Assert.AreEqual(eventArgsCompiled.NodeId, guid);
            Assert.AreEqual((eventArgsCompiled.AstNodes as List<AssociativeNode>).Count, associativeNodes.Count);
        }
    }
}

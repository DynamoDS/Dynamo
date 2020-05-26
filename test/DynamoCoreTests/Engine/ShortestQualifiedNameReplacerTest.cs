using Dynamo.Engine;
using Dynamo.Engine.NodeToCode;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using System.Collections.Generic;

namespace Dynamo.Tests.Engine
{
    [TestFixture]
    class ShortestQualifiedNameReplacerTest : DynamoModelTestBase
    {
        /// <summary>
        /// This test method will execute the next methods from the ShortestQualifiedNameReplacer class
        /// AssociativeNode VisitTypedIdentifierNode(TypedIdentifierNode node)
        /// AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        /// </summary>
        [Test]
        [Category("UnitTests")]
        public void VisitTypedIdentifierNodeTest()
        {
            //Arrange
            var engine = CurrentDynamoModel.EngineController;
            ShortestQualifiedNameReplacer replacer = new ShortestQualifiedNameReplacer(engine.LibraryServices.LibraryManagementCore.ClassTable, null);
            TypedIdentifierNode node = null;
            IdentifierListNode node2 = null;

            //Act
            //We need to pass null to both functions in order to reach a specific piece of code wihout coverage
            var response = replacer.VisitTypedIdentifierNode(node);
            var response2 = replacer.VisitIdentifierListNode(node2);

            //Assert
            //Checking that both functions returned null
            Assert.IsNull(response);
            Assert.IsNull(response2);
        }
    }
}

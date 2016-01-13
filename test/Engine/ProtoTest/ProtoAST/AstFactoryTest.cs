using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;

namespace ProtoTest.ProtoAST
{
    class AstFactoryTest
    {
        [Test]
        public void TestMethodHasNullCheck()
        {
            AssociativeNode foo = new IdentifierNode("foo");
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildAssignment(foo, null));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildAssignment(null, foo));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildBinaryExpression(foo, null, ProtoCore.DSASM.Operator.assign));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildBinaryExpression(null, foo, ProtoCore.DSASM.Operator.assign));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildConditionalNode(null, foo, foo));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildConditionalNode(foo, null, foo));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildConditionalNode(foo, foo, null));
            List<AssociativeNode> nullList = null;
            List<AssociativeNode> fooList = new List<AssociativeNode>();
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildExprList(nullList));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildFunctionCall(() => {}, null));
            string nullIdent = null;
            AssociativeNode nullNode = null;
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildFunctionObject(nullNode, 0, new List<int> {}, fooList));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildFunctionObject(foo, 0, null, fooList));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildFunctionObject(nullNode, 0, new List<int> {}, null));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildFunctionObject(nullIdent, 0, new List<int> { } , fooList));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildFunctionObject(string.Empty, 0, new List<int> { } , fooList));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildIdentifier(null));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildIdentifier(String.Empty));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildStringNode(null));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildParamNode(null));
            Assert.Throws<ArgumentException>(() => AstFactory.BuildParamNode(string.Empty));
            Assert.Throws<ArgumentNullException>(() => AstFactory.BuildReturnStatement(null));
            Assert.Throws<ArgumentNullException>(() => AstFactory.AddReplicationGuide(null, null, false));
        }
    }
}

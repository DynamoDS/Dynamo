using NUnit.Framework;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.SyntaxAnalysis;

namespace ProtoTest.UtilsTests
{
    class AstRewriter : AssociativeAstVisitor<AssociativeNode>
    {
        private string variableName;
        private IntNode newValueNode;

        public BinaryExpressionNode ReplaceWithConstant(BinaryExpressionNode node, string variable, int value)
        {
            variableName = variable;
            newValueNode = AstFactory.BuildIntNode(value);
            return node.Accept(this) as BinaryExpressionNode;
        }

        public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
        {
            if (node.Value.Equals(variableName))
                return newValueNode;
            else
                return node;
        }

        public override AssociativeNode VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            if (node.Optr != ProtoCore.DSASM.Operator.assign)
                return node;

            var leftNode = node.LeftNode;
            var rightNode = node.RightNode;

            var newLeftNode = leftNode.Accept(this); 
            var newRightNode = rightNode.Accept(this); 

            if (newLeftNode == leftNode && newRightNode == rightNode)
            {
                return node;
            }
            else
            {
                return new BinaryExpressionNode(newLeftNode, newRightNode, ProtoCore.DSASM.Operator.assign);
            }
        }
    }

    class AstReplacerTest: AstReplacer 
    {
        private string variableName;
        private IntNode newValueNode;

        public BinaryExpressionNode ReplaceWithConstant(BinaryExpressionNode node, string variable, int value)
        {
            variableName = variable;
            newValueNode = AstFactory.BuildIntNode(value);
            return node.Accept(this) as BinaryExpressionNode;
        }

        public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
        {
            if (node.Value.Equals(variableName))
                return newValueNode;
            else
                return node;
        }
    }

    [TestFixture]
    class AstVisitorTest
    {
        [Test]
        public void TestAstVisitorForReplacement()
        {
            AstRewriter rewriter = new AstRewriter();

            var lhs = AstFactory.BuildIdentifier("x");
            var rhs = AstFactory.BuildIdentifier("y");
            var expression1 = AstFactory.BuildAssignment(lhs, rhs);

            var newExpression = rewriter.ReplaceWithConstant(expression1, "y", 21);
            var newLhs = newExpression.LeftNode as IdentifierNode;
            var newRhs = newExpression.RightNode as IntNode;
            Assert.IsTrue(newLhs != null && newLhs.Value == "x" && newRhs != null && newRhs.Value == 21);
        }
        
        [Test]
        public void TestAstReplacer()
        {
            AstReplacerTest replacer = new AstReplacerTest();

            var lhs = AstFactory.BuildIdentifier("x");
            var rhs = AstFactory.BuildIdentifier("y");
            var expression1 = AstFactory.BuildAssignment(lhs, rhs);

            var newExpression = replacer.ReplaceWithConstant(expression1, "y", 21);
            var newLhs = newExpression.LeftNode as IdentifierNode;
            var newRhs = newExpression.RightNode as IntNode;
            Assert.IsTrue(newLhs != null && newLhs.Value == "x" && newRhs != null && newRhs.Value == 21);
        }
    }
}

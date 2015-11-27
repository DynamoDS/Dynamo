using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoCore.AST.ImperativeAST;
using ProtoCore.AST;

namespace ProtoCore.SyntaxAnalysis
{
    public abstract class ImperativeAstVisitor
    {
        public virtual void DefaultVisit(ImperativeNode node)
        {
        }

        public virtual void Visit(Node node)
        {
            ImperativeNode impNode = node as ImperativeNode;
            if (impNode != null)
                impNode.Accept(this);
        }

        public virtual void VisitLanguageBlockNode(LanguageBlockNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitArrayNameNode(ArrayNameNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGroupExpressionNode(GroupExpressionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIdentifierNode(IdentifierNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIdentifierListNode(IdentifierListNode node)
        {
            node.LeftNode.Accept(this);
            node.RightNode.Accept(this);
        }

        public virtual void VisitIntNode(IntNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDoubleNode(DoubleNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBooleanNode(BooleanNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCharNode(CharNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitStringNode(StringNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNullNode(NullNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFunctionCallNode(FunctionCallNode node)
        {
            for (int i = 0; i < node.FormalArguments.Count; ++i)
            {
                node.FormalArguments[i].Accept(this);
            }

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);
        }

        public virtual void VisitVarDeclNode(VarDeclNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitArgumentSignatureNode(ArgumentSignatureNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCodeBlockNode(CodeBlockNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIfStatementNode(IfStmtNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitElseIfNode(ElseIfBlock node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitInlineConditionalNode(InlineConditionalNode node)
        {
            node.ConditionExpression.Accept(this);
            node.TrueExpression.Accept(this);
            node.FalseExpression.Accept(this);
        }

        public virtual void VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            node.LeftNode.Accept(this);
            node.RightNode.Accept(this);
        }

        public virtual void VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitRangeExprNode(RangeExprNode node)
        {
            node.From.Accept(this);
            node.To.Accept(this);

            if (node.Step != null)
                node.Step.Accept(this);

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);
        }

        public virtual void VisitExprListNode(ExprListNode node)
        {
            for (int i = 0; i < node.Exprs.Count; ++i)
            {
                node.Exprs[i].Accept(this);
            }

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);
        }

        public virtual void VisitArrayNode(ArrayNode node)
        {
            if (node.Expr != null)
                node.Expr.Accept(this);

            if (node.Type != null)
                node.Type.Accept(this);
        }

        public virtual void VisitDefaultArgNode(DefaultArgNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitWhileStatementNode(WhileStmtNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitForLoopNode(ForLoopNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBreakNode(BreakNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitContinueNode(ContinueNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIfStmtPositionNode(IfStmtPositionNode node)
        {
            DefaultVisit(node);
        }
    }

    public abstract class ImperativeAstVisitor<TResult>
    {
        public virtual TResult DefaultVisit(ImperativeNode node)
        {
            return default(TResult);
        }

        public virtual TResult Visit(Node node)
        {
            ImperativeNode impNode = node as ImperativeNode;
            if (impNode != null)
                return impNode.Accept(this);

            return default(TResult);
        }

        public virtual TResult VisitLanguageBlockNode(LanguageBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitArrayNameNode(ArrayNameNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitGroupExpressionNode(GroupExpressionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIdentifierNode(IdentifierNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIdentifierListNode(IdentifierListNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIntNode(IntNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDoubleNode(DoubleNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitBooleanNode(BooleanNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCharNode(CharNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitStringNode(StringNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitNullNode(NullNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitFunctionCallNode(FunctionCallNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitVarDeclNode(VarDeclNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitArgumentSignatureNode(ArgumentSignatureNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCodeBlockNode(CodeBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIfStatementNode(IfStmtNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitElseIfNode(ElseIfBlock node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitInlineConditionalNode(InlineConditionalNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitRangeExprNode(RangeExprNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitExprListNode(ExprListNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitArrayNode(ArrayNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDefaultArgNode(DefaultArgNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitWhileStatementNode(WhileStmtNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitForLoopNode(ForLoopNode node)
        {
            return DefaultVisit(node);
        }
        public virtual TResult VisitBreakNode(BreakNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitContinueNode(ContinueNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIfStmtPositionNode(IfStmtPositionNode node)
        {
            return DefaultVisit(node);
        }
    }
}

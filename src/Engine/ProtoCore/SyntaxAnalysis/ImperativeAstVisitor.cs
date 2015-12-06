using System.Linq;
using ProtoCore.AST.ImperativeAST;
using ProtoCore.AST;
using System.Collections.Generic;

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

    public class ImperativeAstReplacer : ImperativeAstVisitor<ImperativeNode>
    {
        public override ImperativeNode DefaultVisit(ImperativeNode node)
        {
            return node;
        }

        public override ImperativeNode VisitLanguageBlockNode(LanguageBlockNode node)
        {
            var cbn = node.CodeBlockNode as CodeBlockNode;
            if (cbn == null)
            {
                return base.VisitLanguageBlockNode(node);
            }

            var nodeList = cbn.Body.Select(astNode => astNode.Accept(this)).ToList();
            cbn.Body = nodeList;
            return node;
        }

        public List<ImperativeNode> VisitNodeList(List<ImperativeNode> nodes)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                var newItem = nodes[i].Accept(this);
                if (nodes[i] != newItem)
                    nodes[i] = newItem;
            }

            return nodes;
        }

        public override ImperativeNode VisitCodeBlockNode(CodeBlockNode node)
        {
            node.Body = VisitNodeList(node.Body); 
            return node;
        }

        public override ImperativeNode VisitGroupExpressionNode(GroupExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);

            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override ImperativeNode VisitIdentifierNode(IdentifierNode node)
        {
            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            return VisitIdentifierNode(node);
        }

        public override ImperativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            var newLeftNode = node.LeftNode.Accept(this);
            if (newLeftNode != node.LeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (newRightNode != node.RightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override ImperativeNode VisitFunctionCallNode(FunctionCallNode node)
        {
            var func = node.Function.Accept(this);
            if (node.Function != func)
                node.Function = func;

            node.FormalArguments = VisitNodeList(node.FormalArguments); 

            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            var nodeList = node.FunctionBody.Body.Select(astNode => astNode.Accept(this)).ToList();
            node.FunctionBody.Body = nodeList;
            return node;
        }

        public override ImperativeNode VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            var newLeftNode = node.LeftNode.Accept(this);
            if (node.LeftNode != newLeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (node.RightNode != newRightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override ImperativeNode VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);
            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override ImperativeNode VisitRangeExprNode(RangeExprNode node)
        {
            var newFromNode = node.From.Accept(this);
            if (node.From != newFromNode)
                node.From = newFromNode;

            var newToNode = node.To.Accept(this);
            if (node.To != newToNode)
                node.To = newToNode;

            if (node.Step != null)
            {
                var newStepNode = node.Step.Accept(this);
                if (node.Step != newStepNode)
                    node.Step = newStepNode;
            }

            return node;
        }

        public override ImperativeNode VisitExprListNode(ExprListNode node)
        {
            node.Exprs = VisitNodeList(node.Exprs);

            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitArrayNode(ArrayNode node)
        {
            var newExpr = node.Expr.Accept(this);
            if (node.Expr != newExpr)
                node.Expr = newExpr;

            if (node.Type != null)
            {
                var newType = node.Type.Accept(this);
                if (node.Type != newType)
                    node.Type = newType;
            }

            return node;
        }

        public override ImperativeNode VisitInlineConditionalNode(InlineConditionalNode node)
        {
            var newCondition = node.ConditionExpression.Accept(this);
            if (node.ConditionExpression != newCondition)
                node.ConditionExpression = newCondition;

            var newTrueExpr = node.TrueExpression.Accept(this);
            if (node.TrueExpression != newTrueExpr)
                node.TrueExpression = newTrueExpr;

            var newFalseExpr = node.FalseExpression.Accept(this);
            if (node.FalseExpression != newFalseExpr)
                node.FalseExpression = newFalseExpr;

            return node;
        }

        public override ImperativeNode VisitIfStatementNode(IfStmtNode node)
        {
            var newIfExpr = node.IfExprNode.Accept(this);
            if (node.IfExprNode != newIfExpr)
                node.IfExprNode = newIfExpr;

            node.IfBody = VisitNodeList(node.IfBody);
            node.ElseIfList = VisitNodeList(node.ElseIfList.Cast<ImperativeNode>().ToList()).Cast<ElseIfBlock>().ToList();
            node.ElseBody = VisitNodeList(node.ElseBody);

            return node;
        }

        public override ImperativeNode VisitElseIfNode(ElseIfBlock node)
        {
            var newExpr = node.Expr.Accept(this);
            if (node.Expr != newExpr)
                node.Expr = newExpr;

            node.Body = VisitNodeList(node.Body);
            return node;
        }

        public override ImperativeNode VisitWhileStatementNode(WhileStmtNode node)
        {
            var newExpr = node.Expr.Accept(this);
            if (node.Expr != newExpr)
                node.Expr = newExpr;

            node.Body = VisitNodeList(node.Body);
            return node;
        }

        public override ImperativeNode VisitForLoopNode(ForLoopNode node)
        {
            var newLoopVar = node.LoopVariable.Accept(this);
            if (node.LoopVariable != newLoopVar)
                node.LoopVariable = newLoopVar;

            var newExpr = node.Expression.Accept(this);
            if (node.Expression != newExpr)
                node.Expression = newExpr;

            node.Body = VisitNodeList(node.Body);
            return node;
        }
    }
}

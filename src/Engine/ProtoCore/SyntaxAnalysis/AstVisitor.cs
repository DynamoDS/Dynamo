using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ImperativeNode = ProtoCore.AST.ImperativeAST.ImperativeNode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.SyntaxAnalysis
{
    public abstract class AstVisitor<TAssociative, TImperative> : Associative.IAstVisitor<TAssociative>, Imperative.IAstVisitor<TImperative>
    {
        public abstract TAssociative VisitAssociativeNode(AssociativeNode node);

        public abstract TImperative VisitImperativeNode(ImperativeNode node);
        
        public virtual TAssociative VisitCommentNode(CommentNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitLanguageBlockNode(LanguageBlockNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitReplicationGuideNode(ReplicationGuideNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitAtLevelNode(AtLevelNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitArrayNameNode(ArrayNameNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitGroupExpressionNode(GroupExpressionNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitIdentifierNode(IdentifierNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitIdentifierListNode(IdentifierListNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitIntNode(IntNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitDoubleNode(DoubleNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitBooleanNode(BooleanNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitCharNode(CharNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitStringNode(StringNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitNullNode(NullNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitFunctionCallNode(FunctionCallNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitFunctionDotCallNode(FunctionDotCallNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitVarDeclNode(VarDeclNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitArgumentSignatureNode(ArgumentSignatureNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitCodeBlockNode(CodeBlockNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitClassDeclNode(ClassDeclNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitConstructorDefinitionNode(ConstructorDefinitionNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitIfStatementNode(IfStatementNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitInlineConditionalNode(InlineConditionalNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitRangeExprNode(RangeExprNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitExprListNode(ExprListNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitArrayNode(ArrayNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitImportNode(ImportNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitDynamicNode(DynamicNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitDynamicBlockNode(DynamicBlockNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitThisPointerNode(ThisPointerNode node)
        {
            return VisitAssociativeNode(node);
        }

        public virtual TAssociative VisitDefaultArgNode(DefaultArgNode node)
        {
            return VisitAssociativeNode(node);
        }
        
        public virtual TImperative VisitLanguageBlockNode(AST.ImperativeAST.LanguageBlockNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitArrayNameNode(AST.ImperativeAST.ArrayNameNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitGroupExpressionNode(AST.ImperativeAST.GroupExpressionNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitIdentifierNode(AST.ImperativeAST.IdentifierNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitTypedIdentifierNode(AST.ImperativeAST.TypedIdentifierNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitIdentifierListNode(AST.ImperativeAST.IdentifierListNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitIntNode(AST.ImperativeAST.IntNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitDoubleNode(AST.ImperativeAST.DoubleNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitBooleanNode(AST.ImperativeAST.BooleanNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitCharNode(AST.ImperativeAST.CharNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitStringNode(AST.ImperativeAST.StringNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitNullNode(AST.ImperativeAST.NullNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitFunctionCallNode(AST.ImperativeAST.FunctionCallNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitCodeBlockNode(AST.ImperativeAST.CodeBlockNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitIfStatementNode(AST.ImperativeAST.IfStmtNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitElseIfNode(AST.ImperativeAST.ElseIfBlock node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitInlineConditionalNode(AST.ImperativeAST.InlineConditionalNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitBinaryExpressionNode(AST.ImperativeAST.BinaryExpressionNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitUnaryExpressionNode(AST.ImperativeAST.UnaryExpressionNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitRangeExprNode(AST.ImperativeAST.RangeExprNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitExprListNode(AST.ImperativeAST.ExprListNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitArrayNode(AST.ImperativeAST.ArrayNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitWhileStatementNode(AST.ImperativeAST.WhileStmtNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitForLoopNode(AST.ImperativeAST.ForLoopNode node)
        {
            return VisitImperativeNode(node);
        }
        public virtual TImperative VisitBreakNode(AST.ImperativeAST.BreakNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitContinueNode(AST.ImperativeAST.ContinueNode node)
        {
            return VisitImperativeNode(node);
        }

        public virtual TImperative VisitIfStmtPositionNode(AST.ImperativeAST.IfStmtPositionNode node)
        {
            return VisitImperativeNode(node);
        }
    }

    public class AstReplacer : AstVisitor<AssociativeNode, ImperativeNode>
    {
        public Node VisitNode(Node node)
        {
            var assocNode = node as AssociativeNode;
            if (null != assocNode)
            {
                return assocNode.Accept(this);
            }

            var impNode = node as ImperativeNode;
            if(null != impNode)
            {
                return impNode.Accept(this);
            }

            return node;
        }
        
        public List<Node> VisitNodeList(List<Node> nodes)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                var newItem = this.VisitNode(nodes[i]);
                if (nodes[i] != newItem)
                    nodes[i] = newItem;
            }

            return nodes;
        }

        public override AssociativeNode VisitAssociativeNode(AssociativeNode node)
        {
            return node;
        }

        public List<AssociativeNode> VisitNodeList(List<AssociativeNode> nodes)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                var newItem = nodes[i].Accept(this);
                if (nodes[i] != newItem)
                    nodes[i] = newItem;
            }

            return nodes;
        }

        public override AssociativeNode VisitGroupExpressionNode(GroupExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);

            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override AssociativeNode VisitIdentifierNode(IdentifierNode node)
        {
            if (node == null)
                return null;

            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as ArrayNode;
            }

            return node;
        }

        public override AssociativeNode VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            return VisitIdentifierNode(node);
        }

        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            if (node == null)
                return null;

            var newLeftNode = node.LeftNode.Accept(this);
            if (newLeftNode != node.LeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (newRightNode != node.RightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override AssociativeNode VisitFunctionCallNode(FunctionCallNode node)
        {
            if (node == null)
                return null;

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

        public override AssociativeNode VisitLanguageBlockNode(LanguageBlockNode node)
        {
            var cbn = node.CodeBlockNode as CodeBlockNode;
            if (cbn == null)
            {
                var icbn = VisitCodeBlockNode(node.CodeBlockNode as AST.ImperativeAST.CodeBlockNode);
                node.CodeBlockNode = icbn;
                return node;
            }

            var nodeList = cbn.Body.Select(astNode => astNode.Accept(this)).ToList();
            cbn.Body = nodeList;
            return node;
        }

        public override AssociativeNode VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            var nodeList = node.FunctionBody.Body.Select(astNode => astNode.Accept(this)).ToList();
            node.FunctionBody.Body = nodeList;
            return node;
        }

        public override AssociativeNode VisitInlineConditionalNode(InlineConditionalNode node)
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

        public override AssociativeNode VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            var newLeftNode = node.LeftNode.Accept(this);
            if (node.LeftNode != newLeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (node.RightNode != newRightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override AssociativeNode VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);
            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override AssociativeNode VisitRangeExprNode(RangeExprNode node)
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

        public override AssociativeNode VisitExprListNode(ExprListNode node)
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

        public override AssociativeNode VisitArrayNode(ArrayNode node)
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

        public override ImperativeNode VisitImperativeNode(ImperativeNode node)
        {
            return node;
        }

        public override ImperativeNode VisitLanguageBlockNode(AST.ImperativeAST.LanguageBlockNode node)
        {
            var icbn = node.CodeBlockNode as AST.ImperativeAST.CodeBlockNode;
            if (icbn == null)
            {
                var cbn = VisitCodeBlockNode(node.CodeBlockNode as CodeBlockNode);
                node.CodeBlockNode = cbn;
                return node;
            }

            var nodeList = icbn.Body.Select(astNode => astNode.Accept(this)).ToList();
            icbn.Body = nodeList;
            return node;
        }

        public override ImperativeNode VisitCodeBlockNode(AST.ImperativeAST.CodeBlockNode node)
        {
            var nodeList = node.Body.Select(n => n.Accept(this)).ToList();
            node.Body = nodeList;
            return node;
        }

        public override ImperativeNode VisitGroupExpressionNode(AST.ImperativeAST.GroupExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);

            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override ImperativeNode VisitIdentifierNode(AST.ImperativeAST.IdentifierNode node)
        {
            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as AST.ImperativeAST.ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitTypedIdentifierNode(AST.ImperativeAST.TypedIdentifierNode node)
        {
            return VisitIdentifierNode(node);
        }

        public override ImperativeNode VisitIdentifierListNode(AST.ImperativeAST.IdentifierListNode node)
        {
            var newLeftNode = node.LeftNode.Accept(this);
            if (newLeftNode != node.LeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (newRightNode != node.RightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override ImperativeNode VisitFunctionCallNode(AST.ImperativeAST.FunctionCallNode node)
        {
            var func = node.Function.Accept(this);
            if (node.Function != func)
                node.Function = func;

            node.FormalArguments = VisitNodeList(node.FormalArguments);

            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as AST.ImperativeAST.ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitBinaryExpressionNode(AST.ImperativeAST.BinaryExpressionNode node)
        {
            var newLeftNode = node.LeftNode.Accept(this);
            if (node.LeftNode != newLeftNode)
                node.LeftNode = newLeftNode;

            var newRightNode = node.RightNode.Accept(this);
            if (node.RightNode != newRightNode)
                node.RightNode = newRightNode;

            return node;
        }

        public override ImperativeNode VisitUnaryExpressionNode(AST.ImperativeAST.UnaryExpressionNode node)
        {
            var newExpression = node.Expression.Accept(this);
            if (node.Expression != newExpression)
                node.Expression = newExpression;

            return node;
        }

        public override ImperativeNode VisitRangeExprNode(AST.ImperativeAST.RangeExprNode node)
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

        public override ImperativeNode VisitExprListNode(AST.ImperativeAST.ExprListNode node)
        {
            node.Exprs = VisitNodeList(node.Exprs);

            if (node.ArrayDimensions != null)
            {
                var newArrayDimensions = node.ArrayDimensions.Accept(this);
                if (node.ArrayDimensions != newArrayDimensions)
                    node.ArrayDimensions = newArrayDimensions as AST.ImperativeAST.ArrayNode;
            }

            return node;
        }

        public override ImperativeNode VisitArrayNode(AST.ImperativeAST.ArrayNode node)
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

        public override ImperativeNode VisitInlineConditionalNode(AST.ImperativeAST.InlineConditionalNode node)
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

        public override ImperativeNode VisitIfStatementNode(AST.ImperativeAST.IfStmtNode node)
        {
            var newIfExpr = node.IfExprNode.Accept(this);
            if (node.IfExprNode != newIfExpr)
                node.IfExprNode = newIfExpr;

            node.IfBody = VisitNodeList(node.IfBody);
            node.ElseIfList = node.ElseIfList.Select(n=>n.Accept(this)).Cast<AST.ImperativeAST.ElseIfBlock>().ToList();
            node.ElseBody = VisitNodeList(node.ElseBody);

            return node;
        }

        public override ImperativeNode VisitElseIfNode(AST.ImperativeAST.ElseIfBlock node)
        {
            var newExpr = node.Expr.Accept(this);
            if (node.Expr != newExpr)
                node.Expr = newExpr;

            node.Body = VisitNodeList(node.Body);
            return node;
        }

        public override ImperativeNode VisitWhileStatementNode(AST.ImperativeAST.WhileStmtNode node)
        {
            var newExpr = node.Expr.Accept(this);
            if (node.Expr != newExpr)
                node.Expr = newExpr;

            node.Body = VisitNodeList(node.Body);
            return node;
        }

        public override ImperativeNode VisitForLoopNode(AST.ImperativeAST.ForLoopNode node)
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

        private List<ImperativeNode> VisitNodeList(List<ImperativeNode> nodes)
        {
            return nodes.Select(n => n.Accept(this)).ToList();
        }
    }

    /// <summary>
    /// AstTraversal visits all nodes of the AST unless the result of a Visit* method is false or you override one of the methods such that
    /// it doesn't visit all of the Node's child nodes.
    /// </summary>
    public abstract class AstTraversal : AstVisitor<bool, bool>
    {
        public override bool VisitAssociativeNode(AssociativeNode node)
        {
            return VisitAllChildren(node);
        }

        public override bool VisitImperativeNode(ImperativeNode node)
        {
            return VisitAllChildren(node);
        }

        public bool VisitAllChildren(Node node)
        {
            if (node == null) return true;

            var children = node.Children();

            foreach (var n in children)
            {
                if (n is AssociativeNode)
                {
                    if (!(n as AssociativeNode).Accept(this))
                    {
                        return false;
                    }
                }
                else if (n is ImperativeNode)
                {
                    if (!(n as ImperativeNode).Accept(this))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

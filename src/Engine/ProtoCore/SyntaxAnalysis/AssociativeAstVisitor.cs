using System.Collections.Generic;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.AST;

namespace ProtoCore.SyntaxAnalysis
{
    public abstract class AssociativeAstVisitor : Associative.IAstVisitor<bool>
    {
        public virtual bool DefaultVisit(AssociativeNode node)
        {
            return false;
        }

        public virtual bool Visit(Node node)
        {
            AssociativeNode assocNode = node as AssociativeNode;
            return (assocNode != null) ? assocNode.Accept(this) : false;
        }

        public virtual bool VisitCommentNode(CommentNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitLanguageBlockNode(LanguageBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitReplicationGuideNode(ReplicationGuideNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitAtLevelNode(AtLevelNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitArrayNameNode(ArrayNameNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitGroupExpressionNode(GroupExpressionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitIdentifierNode(IdentifierNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitIdentifierListNode(IdentifierListNode node)
        {
            node.LeftNode.Accept(this);
            node.RightNode.Accept(this);
            return true;
        }

        public virtual bool VisitIntNode(IntNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitDoubleNode(DoubleNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitBooleanNode(BooleanNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitCharNode(CharNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitStringNode(StringNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitNullNode(NullNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitFunctionCallNode(FunctionCallNode node)
        {
            for (int i = 0; i < node.FormalArguments.Count; ++i)
            {
                node.FormalArguments[i].Accept(this);
            }

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);

            return true;
        }

        public virtual bool VisitFunctionDotCallNode(FunctionDotCallNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitVarDeclNode(VarDeclNode node)
        {
            return DefaultVisit(node);
        }
        
        public virtual bool VisitArgumentSignatureNode(ArgumentSignatureNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitCodeBlockNode(CodeBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitClassDeclNode(ClassDeclNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitConstructorDefinitionNode(ConstructorDefinitionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitIfStatementNode(IfStatementNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitInlineConditionalNode(InlineConditionalNode node)
        {
            node.ConditionExpression.Accept(this);
            node.TrueExpression.Accept(this);
            node.FalseExpression.Accept(this);

            return true;
        }

        public virtual bool VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            node.LeftNode.Accept(this);
            node.RightNode.Accept(this);

            return true;
        }

        public virtual bool VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitRangeExprNode(RangeExprNode node)
        {
            node.From.Accept(this);
            node.To.Accept(this);

            if (node.Step != null)
                node.Step.Accept(this);

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);

            return true;
        }

        public virtual bool VisitExprListNode(ExprListNode node)
        {
            for (int i = 0; i < node.Exprs.Count; ++i)
            {
                node.Exprs[i].Accept(this);
            }

            if (node.ArrayDimensions != null)
                node.ArrayDimensions.Accept(this);

            return true;
        }

        public virtual bool VisitArrayNode(ArrayNode node)
        {
            if (node.Expr != null)
                node.Expr.Accept(this);

            if (node.Type != null)
                node.Type.Accept(this);

            return true;
        }

        public virtual bool VisitImportNode(ImportNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitDynamicNode(DynamicNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitDynamicBlockNode(DynamicBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitThisPointerNode(ThisPointerNode node)
        {
            return DefaultVisit(node);
        }

        public virtual bool VisitDefaultArgNode(DefaultArgNode node)
        {
            return DefaultVisit(node);
        }
    }

    public abstract class AssociativeAstVisitor<TResult> : Associative.IAstVisitor<TResult>
    {
        public virtual TResult DefaultVisit(AssociativeNode node)
        {
            return default(TResult);
        }

        public virtual TResult Visit(Node node)
        {
            AssociativeNode assocNode = node as AssociativeNode;
            if (assocNode != null)
                return assocNode.Accept(this);

            return default(TResult);
        }

        public virtual TResult VisitCommentNode(CommentNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitLanguageBlockNode(LanguageBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitReplicationGuideNode(ReplicationGuideNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitAtLevelNode(AtLevelNode node)
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

        public virtual TResult VisitFunctionDotCallNode(FunctionDotCallNode node)
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

        public virtual TResult VisitClassDeclNode(ClassDeclNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitConstructorDefinitionNode(ConstructorDefinitionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIfStatementNode(IfStatementNode node)
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

        public virtual TResult VisitImportNode(ImportNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDynamicNode(DynamicNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDynamicBlockNode(DynamicBlockNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitThisPointerNode(ThisPointerNode node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDefaultArgNode(DefaultArgNode node)
        {
            return DefaultVisit(node);
        }
    }

    public class AssociativeAstReplacer : AssociativeAstVisitor<AssociativeNode>
    {
        public override AssociativeNode DefaultVisit(AssociativeNode node)
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
                return base.VisitLanguageBlockNode(node);
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
    }
}

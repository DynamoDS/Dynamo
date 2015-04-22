using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.AST;

namespace ProtoCore.SyntaxAnalysis
{
    public class AssociativeAstVisitor
    {
        public virtual void DefaultVisit(AssociativeNode node)
        {
        }

        public virtual void Visit(Node node)
        {
            AssociativeNode assocNode = node as AssociativeNode;
            if (assocNode != null)
                assocNode.Accept(this);
        }

        public virtual void VisitCommentNode(CommentNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitLanguageBlockNode(LanguageBlockNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitReplicationGuideNode(ReplicationGuideNode node)
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
            DefaultVisit(node); 
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
            DefaultVisit(node);
        }

        public virtual void VisitFunctionDotCallNode(FunctionDotCallNode node)
        {
            DefaultVisit(node);
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

        public virtual void VisitClassDeclNode(ClassDeclNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitConstructorDefinitionNode(ConstructorDefinitionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFunctionDefinitionNode(FunctionDefinitionNode node)
        {
            DefaultVisit(node); ;
        }

        public virtual void VisitIfStatementNode(IfStatementNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitInlineConditionalNode(InlineConditionalNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitRangeExprNode(RangeExprNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExprListNode(ExprListNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitArrayNode(ArrayNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitImportNode(ImportNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDynamicNode(DynamicNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDynamicBlockNode(DynamicBlockNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitThisPointerNode(ThisPointerNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDefaultArgNode(DefaultArgNode node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitModifierStackNode(ModifierStackNode node)
        {
            DefaultVisit(node);
        }
    }

    public class AssociativeAstVisitor<TResult>
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
            return DefaultVisit(node); ;
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

        public virtual TResult VisitModifierStackNode(ModifierStackNode node)
        {
            return DefaultVisit(node);
        }
    }
}

using System;
using ProtoCore.AST.AssociativeAST;

namespace ProtoCore.SyntaxAnalysis.Associative
{
    public interface IAstVisitor<TResult>
    {
        TResult VisitCommentNode(CommentNode node);

        TResult VisitLanguageBlockNode(LanguageBlockNode node);

        TResult VisitReplicationGuideNode(ReplicationGuideNode node);

        TResult VisitAtLevelNode(AtLevelNode node);

        TResult VisitArrayNameNode(ArrayNameNode node);

        TResult VisitGroupExpressionNode(GroupExpressionNode node);

        TResult VisitIdentifierNode(IdentifierNode node);

        TResult VisitTypedIdentifierNode(TypedIdentifierNode node);

        TResult VisitIdentifierListNode(IdentifierListNode node);

        TResult VisitIntNode(IntNode node);

        TResult VisitDoubleNode(DoubleNode node);

        TResult VisitBooleanNode(BooleanNode node);

        TResult VisitCharNode(CharNode node);

        TResult VisitStringNode(StringNode node);

        TResult VisitNullNode(NullNode node);

        TResult VisitFunctionCallNode(FunctionCallNode node);

        TResult VisitFunctionDotCallNode(FunctionDotCallNode node);

        TResult VisitVarDeclNode(VarDeclNode node);

        TResult VisitArgumentSignatureNode(ArgumentSignatureNode node);

        TResult VisitCodeBlockNode(CodeBlockNode node);

        TResult VisitClassDeclNode(ClassDeclNode node);

        TResult VisitConstructorDefinitionNode(ConstructorDefinitionNode node);

        TResult VisitFunctionDefinitionNode(FunctionDefinitionNode node);

        [Obsolete("VisitIfStatementNode method is deprecated and not used. To be remove in 3.0")]
        TResult VisitIfStatementNode(IfStatementNode node);

        TResult VisitInlineConditionalNode(InlineConditionalNode node);

        TResult VisitBinaryExpressionNode(BinaryExpressionNode node);

        TResult VisitUnaryExpressionNode(UnaryExpressionNode node);

        TResult VisitRangeExprNode(RangeExprNode node);

        TResult VisitExprListNode(ExprListNode node);

        TResult VisitArrayNode(ArrayNode node);

        TResult VisitImportNode(ImportNode node);

        TResult VisitDynamicNode(DynamicNode node);

        TResult VisitDynamicBlockNode(DynamicBlockNode node);

        TResult VisitThisPointerNode(ThisPointerNode node);

        TResult VisitDefaultArgNode(DefaultArgNode node);
    }
}

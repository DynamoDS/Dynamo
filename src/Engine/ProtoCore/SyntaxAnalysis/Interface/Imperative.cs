using ProtoCore.AST.ImperativeAST;

namespace ProtoCore.SyntaxAnalysis.Imperative
{
    public interface IAstVisitor<TResult>
    {
        TResult VisitLanguageBlockNode(LanguageBlockNode node);

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

        TResult VisitCodeBlockNode(CodeBlockNode node);

        TResult VisitIfStatementNode(IfStmtNode node);

        TResult VisitElseIfNode(ElseIfBlock node);

        TResult VisitInlineConditionalNode(InlineConditionalNode node);

        TResult VisitBinaryExpressionNode(BinaryExpressionNode node);

        TResult VisitUnaryExpressionNode(UnaryExpressionNode node);

        TResult VisitRangeExprNode(RangeExprNode node);

        TResult VisitExprListNode(ExprListNode node);

        TResult VisitArrayNode(ArrayNode node);

        TResult VisitWhileStatementNode(WhileStmtNode node);

        TResult VisitForLoopNode(ForLoopNode node);

        TResult VisitBreakNode(BreakNode node);

        TResult VisitContinueNode(ContinueNode node);

        TResult VisitIfStmtPositionNode(IfStmtPositionNode node);
    }
}

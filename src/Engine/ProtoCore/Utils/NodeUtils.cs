
namespace ProtoCore.Utils
{
    public static class NodeUtils
    {
        public static ProtoCore.AST.Node Clone(ProtoCore.AST.Node rhsNode)
        {
            if (rhsNode is ProtoCore.AST.AssociativeAST.AssociativeNode)
            {
                return Clone(rhsNode as ProtoCore.AST.AssociativeAST.AssociativeNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ImperativeNode)
            {
                return Clone(rhsNode as ProtoCore.AST.ImperativeAST.ImperativeNode);
            }
            else
            {
                Validity.Assert(false);
            }
            return null;
        }

        public static ProtoCore.AST.AssociativeAST.AssociativeNode Clone(ProtoCore.AST.AssociativeAST.AssociativeNode rhsNode)
        {
            if (rhsNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                return new ProtoCore.AST.AssociativeAST.IdentifierNode(rhsNode as ProtoCore.AST.AssociativeAST.IdentifierNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                return new ProtoCore.AST.AssociativeAST.IdentifierListNode(rhsNode as ProtoCore.AST.AssociativeAST.IdentifierListNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                return new ProtoCore.AST.AssociativeAST.BinaryExpressionNode(rhsNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                return new ProtoCore.AST.AssociativeAST.FunctionCallNode(rhsNode as ProtoCore.AST.AssociativeAST.FunctionCallNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.CodeBlockNode)
            {
                return new ProtoCore.AST.AssociativeAST.CodeBlockNode(rhsNode as ProtoCore.AST.AssociativeAST.CodeBlockNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ArrayNode)
            {
                return new ProtoCore.AST.AssociativeAST.ArrayNode(rhsNode as ProtoCore.AST.AssociativeAST.ArrayNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
            {
                return new ProtoCore.AST.AssociativeAST.FunctionDotCallNode(rhsNode as ProtoCore.AST.AssociativeAST.FunctionDotCallNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                return new ProtoCore.AST.AssociativeAST.ExprListNode(rhsNode as ProtoCore.AST.AssociativeAST.ExprListNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.LanguageBlockNode)
            {
                return new ProtoCore.AST.AssociativeAST.LanguageBlockNode(rhsNode as ProtoCore.AST.AssociativeAST.LanguageBlockNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ThisPointerNode)
            {
                return new ProtoCore.AST.AssociativeAST.ThisPointerNode(rhsNode as ProtoCore.AST.AssociativeAST.ThisPointerNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.InlineConditionalNode)
            {
                return new ProtoCore.AST.AssociativeAST.InlineConditionalNode(rhsNode as ProtoCore.AST.AssociativeAST.InlineConditionalNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.RangeExprNode)
            {
                return new ProtoCore.AST.AssociativeAST.RangeExprNode(rhsNode as ProtoCore.AST.AssociativeAST.RangeExprNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ModifierStackNode)
            {
                return new ProtoCore.AST.AssociativeAST.ModifierStackNode(rhsNode as ProtoCore.AST.AssociativeAST.ModifierStackNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.GroupExpressionNode)
            {
                return new ProtoCore.AST.AssociativeAST.GroupExpressionNode(rhsNode as ProtoCore.AST.AssociativeAST.GroupExpressionNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ClassDeclNode)
            {
                return new ProtoCore.AST.AssociativeAST.ClassDeclNode(rhsNode as ProtoCore.AST.AssociativeAST.ClassDeclNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.VarDeclNode)
            {
                return new ProtoCore.AST.AssociativeAST.VarDeclNode(rhsNode as ProtoCore.AST.AssociativeAST.VarDeclNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
            {
                return new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode(rhsNode as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.ReplicationGuideNode)
            {
                return new ProtoCore.AST.AssociativeAST.ReplicationGuideNode(rhsNode as ProtoCore.AST.AssociativeAST.ReplicationGuideNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.IntNode)
            {
                return new ProtoCore.AST.AssociativeAST.IntNode(rhsNode as ProtoCore.AST.AssociativeAST.IntNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.DoubleNode)
            {
                return new ProtoCore.AST.AssociativeAST.DoubleNode(rhsNode as ProtoCore.AST.AssociativeAST.DoubleNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.BooleanNode)
            {
                return new ProtoCore.AST.AssociativeAST.BooleanNode(rhsNode as ProtoCore.AST.AssociativeAST.BooleanNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.CharNode)
            {
                return new ProtoCore.AST.AssociativeAST.CharNode(rhsNode as ProtoCore.AST.AssociativeAST.CharNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.StringNode)
            {
                return new ProtoCore.AST.AssociativeAST.StringNode(rhsNode as ProtoCore.AST.AssociativeAST.StringNode);
            }
            else if (rhsNode is ProtoCore.AST.AssociativeAST.NullNode)
            {
                return new ProtoCore.AST.AssociativeAST.NullNode();
            }


            // Comment Jun: Leaving this as an assert to can catch unhandled nodes
            Validity.Assert(false);
            
            return null;
        }


        public static ProtoCore.AST.ImperativeAST.ImperativeNode Clone(ProtoCore.AST.ImperativeAST.ImperativeNode rhsNode)
        {
            if (rhsNode is ProtoCore.AST.ImperativeAST.IdentifierNode)
            {
                return new ProtoCore.AST.ImperativeAST.IdentifierNode(rhsNode as ProtoCore.AST.ImperativeAST.IdentifierNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
            {
                return new ProtoCore.AST.ImperativeAST.IdentifierListNode(rhsNode as ProtoCore.AST.ImperativeAST.IdentifierListNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.BinaryExpressionNode)
            {
                return new ProtoCore.AST.ImperativeAST.BinaryExpressionNode(rhsNode as ProtoCore.AST.ImperativeAST.BinaryExpressionNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.FunctionCallNode)
            {
                return new ProtoCore.AST.ImperativeAST.FunctionCallNode(rhsNode as ProtoCore.AST.ImperativeAST.FunctionCallNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.CodeBlockNode)
            {
                return new ProtoCore.AST.ImperativeAST.CodeBlockNode(rhsNode as ProtoCore.AST.ImperativeAST.CodeBlockNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ArrayNode)
            {
                return new ProtoCore.AST.ImperativeAST.ArrayNode(rhsNode as ProtoCore.AST.ImperativeAST.ArrayNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.IfStmtNode)
            {
                return new ProtoCore.AST.ImperativeAST.IfStmtNode(rhsNode as ProtoCore.AST.ImperativeAST.IfStmtNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ElseIfBlock)
            {
                return new ProtoCore.AST.ImperativeAST.ElseIfBlock(rhsNode as ProtoCore.AST.ImperativeAST.ElseIfBlock);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.IfStmtPositionNode)
            {
                return new ProtoCore.AST.ImperativeAST.IfStmtPositionNode(rhsNode as ProtoCore.AST.ImperativeAST.IfStmtPositionNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ForLoopNode)
            {
                return new ProtoCore.AST.ImperativeAST.ForLoopNode(rhsNode as ProtoCore.AST.ImperativeAST.ForLoopNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.WhileStmtNode)
            {
                return new ProtoCore.AST.ImperativeAST.WhileStmtNode(rhsNode as ProtoCore.AST.ImperativeAST.WhileStmtNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ExprListNode)
            {
                return new ProtoCore.AST.ImperativeAST.ExprListNode(rhsNode as ProtoCore.AST.ImperativeAST.ExprListNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.LanguageBlockNode)
            {
                return new ProtoCore.AST.ImperativeAST.LanguageBlockNode(rhsNode as ProtoCore.AST.ImperativeAST.LanguageBlockNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.RangeExprNode)
            {
                return new ProtoCore.AST.ImperativeAST.RangeExprNode(rhsNode as ProtoCore.AST.ImperativeAST.RangeExprNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.BreakNode)
            {
                return new ProtoCore.AST.ImperativeAST.BreakNode();
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.ContinueNode)
            {
                return new ProtoCore.AST.ImperativeAST.ContinueNode();
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.IntNode)
            {
                return new ProtoCore.AST.ImperativeAST.IntNode(rhsNode as ProtoCore.AST.ImperativeAST.IntNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.DoubleNode)
            {
                return new ProtoCore.AST.ImperativeAST.DoubleNode(rhsNode as ProtoCore.AST.ImperativeAST.DoubleNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.BooleanNode)
            {
                return new ProtoCore.AST.ImperativeAST.BooleanNode(rhsNode as ProtoCore.AST.ImperativeAST.BooleanNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.CharNode)
            {
                return new ProtoCore.AST.ImperativeAST.CharNode(rhsNode as ProtoCore.AST.ImperativeAST.CharNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.StringNode)
            {
                return new ProtoCore.AST.ImperativeAST.StringNode(rhsNode as ProtoCore.AST.ImperativeAST.StringNode);
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.NullNode)
            {
                return new ProtoCore.AST.ImperativeAST.NullNode();
            }
            else if (rhsNode is ProtoCore.AST.ImperativeAST.UnaryExpressionNode)
            {
                return new ProtoCore.AST.ImperativeAST.UnaryExpressionNode(rhsNode as ProtoCore.AST.ImperativeAST.UnaryExpressionNode);
            }

            Validity.Assert(false);
            return null;
        }

        public static void SetNodeLocation(ProtoCore.AST.Node node, ProtoCore.DesignScriptParser.Token token)
        {
            if (null == node || (null == token))
                return;

            node.line = node.endLine = token.line;
            node.col = token.col;
            node.endCol = token.col + ((null == token.val) ? 0 : token.val.Length);
        }

        public static void SetNodeLocation(ProtoCore.AST.Node node, ProtoCore.AST.Node startNode, ProtoCore.AST.Node endNode)
        {
            if (null == node || (null == startNode) || (null == endNode))
                return;

            node.line = startNode.line;
            node.col = startNode.col;
            node.endLine = endNode.endLine;
            node.endCol = endNode.endCol;
        }

        public static void SetNodeStartLocation(ProtoCore.AST.Node node, ProtoCore.DesignScriptParser.Token token)
        {
            if (null == node || (null == token))
                return;

            node.line = token.line;
            node.col = token.col;
        }

        public static void SetNodeStartLocation(ProtoCore.AST.Node node, int line, int col)
        {
            if (null == node)
                return;

            node.line = line;
            node.col = col;
        }

        public static void SetNodeStartLocation(ProtoCore.AST.Node node, ProtoCore.AST.Node other)
        {
            if (null == node || (null == other) || (node == other))
                return;

            node.line = other.line;
            node.col = other.col;
        }

        public static void SetNodeEndLocation(ProtoCore.AST.Node node, ProtoCore.DesignScriptParser.Token token)
        {
            if (null == node || (null == token))
                return;

            node.endLine = token.line;
            node.endCol = token.col + ((null == token.val) ? 0 : token.val.Length);
        }

        public static void SetNodeEndLocation(ProtoCore.AST.Node node, ProtoCore.AST.Node other)
        {
            if (null == node || (null == other) || (node == other))
                return;

            node.endLine = other.endLine;
            node.endCol = other.endCol;
        }

        public static void CopyNodeLocation(ProtoCore.AST.Node node, ProtoCore.AST.Node other)
        {
            if (null == node || (null == other) || (node == other))
                return;

            node.line = other.line;
            node.col = other.col;
            node.endLine = other.endLine;
            node.endCol = other.endCol;
        }

        public static void UpdateBinaryExpressionLocation(ProtoCore.AST.AssociativeAST.BinaryExpressionNode node)
        {
            if (null == node || (null == node.LeftNode) || (null == node.RightNode))
                return;

            SetNodeLocation(node, node.LeftNode, node.RightNode);
        }

        public static void UpdateBinaryExpressionLocation(ProtoCore.AST.ImperativeAST.BinaryExpressionNode node)
        {
            if (null == node || (null == node.LeftNode) || (null == node.RightNode))
                return;

            SetNodeLocation(node, node.LeftNode, node.RightNode);
        }

        public static bool IsReturnExpressionNode(ProtoCore.AST.ImperativeAST.ImperativeNode node)
        {
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode binaryNode =
                node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode;

            if (null == binaryNode)
                return false;

            return (binaryNode.LeftNode.Name == ProtoCore.DSDefinitions.Keyword.Return);
        }

        public static bool IsReturnExpressionNode(ProtoCore.AST.AssociativeAST.AssociativeNode node)
        {
            ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryNode = 
                node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;

            if (null == binaryNode)
            {
                return false;
            }

            ProtoCore.AST.AssociativeAST.IdentifierNode retNode = binaryNode.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
            if (null == retNode)
            {
                return false;
            }

            return (retNode.Value == ProtoCore.DSDefinitions.Keyword.Return);
        }

        public static bool IsAssignmentNode(ProtoCore.AST.ImperativeAST.ImperativeNode node)
        {
            ProtoCore.AST.ImperativeAST.BinaryExpressionNode binaryNode =
                node as ProtoCore.AST.ImperativeAST.BinaryExpressionNode;

            return (null != binaryNode && (ProtoCore.DSASM.Operator.assign == binaryNode.Optr));
        }
    }
}

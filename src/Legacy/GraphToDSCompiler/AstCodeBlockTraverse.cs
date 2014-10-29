using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;
using ProtoCore.Utils;

namespace GraphToDSCompiler
{
    public class AstCodeBlockTraverse
    {
        //private AST.AssociativeAST.BinaryExpressionNode ben;

        /// <summary>
        /// This is used during ProtoAST generation to connect BinaryExpressionNode's 
        /// generated from Block nodes to its child AST tree - pratapa
        /// </summary>
        protected ProtoCore.AST.AssociativeAST.BinaryExpressionNode ChildTree { get; set; }

        public AstCodeBlockTraverse(List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList)
        //: base(astList)
        { }

        public AstCodeBlockTraverse(ProtoCore.AST.AssociativeAST.BinaryExpressionNode bNode)
        {
            ChildTree = bNode;
        }

        protected void EmitIdentifierNode(ref ProtoCore.AST.AssociativeAST.AssociativeNode identNode)
        {

            ProtoCore.AST.AssociativeAST.IdentifierNode iNode = ChildTree.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
            Validity.Assert(iNode != null);

            if ((identNode as ProtoCore.AST.AssociativeAST.IdentifierNode).Value == iNode.Value)
            {
                //ProtoCore.AST.AssociativeAST.ArrayNode temp = (identNode as ProtoCore.AST.AssociativeAST.IdentifierNode).ArrayDimensions;
                identNode = ChildTree;
                //if ((identNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                //    ((identNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode
                //        as AST.AssociativeAST.IdentifierNode).ArrayDimensions = temp;
            }
        }

        public void DFSTraverse(ref ProtoCore.AST.AssociativeAST.AssociativeNode node)
        {
            if (node is ProtoCore.AST.AssociativeAST.IdentifierNode)
                EmitIdentifierNode(ref node);
            else if (node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                ProtoCore.AST.AssociativeAST.IdentifierListNode identList = node as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                EmitIdentifierListNode(ref identList);
            }
            else if (node is ProtoCore.AST.AssociativeAST.IntNode)
            {
                ProtoCore.AST.AssociativeAST.IntNode intNode = node as ProtoCore.AST.AssociativeAST.IntNode;
                EmitIntNode(ref intNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.DoubleNode)
            {
                ProtoCore.AST.AssociativeAST.DoubleNode doubleNode = node as ProtoCore.AST.AssociativeAST.DoubleNode;
                EmitDoubleNode(ref doubleNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallNode = node as ProtoCore.AST.AssociativeAST.FunctionCallNode;
                EmitFunctionCallNode(ref funcCallNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionDotCallNode)
            {
                ProtoCore.AST.AssociativeAST.FunctionDotCallNode funcDotCall = node as ProtoCore.AST.AssociativeAST.FunctionDotCallNode;
                EmitFunctionDotCallNode(ref funcDotCall);
            }
            else if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryExpr = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                if (binaryExpr.Optr != ProtoCore.DSASM.Operator.assign);

                EmitBinaryNode(ref binaryExpr);
                if (binaryExpr.Optr == ProtoCore.DSASM.Operator.assign)
                {
                }
                if (binaryExpr.Optr != ProtoCore.DSASM.Operator.assign);
            }
            else if (node is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
            {
                ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = node as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode;
                EmitFunctionDefNode(ref funcDefNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ClassDeclNode)
            {
                ProtoCore.AST.AssociativeAST.ClassDeclNode classDeclNode = node as ProtoCore.AST.AssociativeAST.ClassDeclNode;
                EmitClassDeclNode(ref classDeclNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.NullNode)
            {
                ProtoCore.AST.AssociativeAST.NullNode nullNode = node as ProtoCore.AST.AssociativeAST.NullNode;
                EmitNullNode(ref nullNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ArrayIndexerNode)
            {
                ProtoCore.AST.AssociativeAST.ArrayIndexerNode arrIdxNode = node as ProtoCore.AST.AssociativeAST.ArrayIndexerNode;
                EmitArrayIndexerNode(ref arrIdxNode);
            }
            else if (node is ProtoCore.AST.AssociativeAST.ExprListNode)
            {
                ProtoCore.AST.AssociativeAST.ExprListNode exprListNode = node as ProtoCore.AST.AssociativeAST.ExprListNode;
                EmitExprListNode(ref exprListNode);
            }

        }

        //=======================

        /// <summary>
        /// Depth first traversal of an AST node
        /// </summary>
        /// <param name="node"></param>

        /// <summary>
        /// These functions emit the DesignScript code on the destination stream
        /// </summary>
        /// <param name="identNode"></param>
        #region ASTNODE_CODE_EMITTERS

        private void EmitArrayIndexerNode(ref ProtoCore.AST.AssociativeAST.ArrayIndexerNode arrIdxNode)
        {
            if (arrIdxNode.Array is ProtoCore.AST.AssociativeAST.IdentifierNode)
                EmitIdentifierNode(ref arrIdxNode.Array);
            else if (arrIdxNode.Array is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode ben = (arrIdxNode.Array as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode;
                EmitIdentifierNode(ref ben);
                ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = (arrIdxNode.Array as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).RightNode;
                DFSTraverse(ref rightNode);
            }
        }

        private void EmitExprListNode(ref ProtoCore.AST.AssociativeAST.ExprListNode exprListNode)
        {
            for (int i = 0; i < exprListNode.list.Count; i++)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode node = exprListNode.list[i];
                DFSTraverse(ref node);
                exprListNode.list[i] = node;
            }
        }

        protected virtual void EmitImportNode(ProtoCore.AST.AssociativeAST.ImportNode importNode)
        {

        }
        protected virtual void EmitIdentifierListNode(ref ProtoCore.AST.AssociativeAST.IdentifierListNode identList)
        {
            Validity.Assert(null != identList);
            ProtoCore.AST.AssociativeAST.AssociativeNode left = identList.LeftNode;
            DFSTraverse(ref left);
            ProtoCore.AST.AssociativeAST.AssociativeNode right = identList.RightNode;
            DFSTraverse(ref right);
        }
        protected virtual void EmitIntNode(ref ProtoCore.AST.AssociativeAST.IntNode intNode)
        {
            Validity.Assert(null != intNode);
        }

        protected virtual void EmitDoubleNode(ref ProtoCore.AST.AssociativeAST.DoubleNode doubleNode)
        {
            Validity.Assert(null != doubleNode);
        }

        protected virtual void EmitFunctionCallNode(ref ProtoCore.AST.AssociativeAST.FunctionCallNode funcCallNode)
        {
            Validity.Assert(null != funcCallNode);

            Validity.Assert(funcCallNode.Function is ProtoCore.AST.AssociativeAST.IdentifierNode);
            string functionName = (funcCallNode.Function as ProtoCore.AST.AssociativeAST.IdentifierNode).Value;

            Validity.Assert(!string.IsNullOrEmpty(functionName));

            for (int n = 0; n < funcCallNode.FormalArguments.Count; ++n)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode argNode = funcCallNode.FormalArguments[n];
                DFSTraverse(ref argNode);
                funcCallNode.FormalArguments[n] = argNode;
                if (n + 1 < funcCallNode.FormalArguments.Count)
                {
                }
            }
        }

        protected virtual void EmitFunctionDotCallNode(ref ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCall)
        {
            Validity.Assert(null != dotCall);

            ProtoCore.AST.AssociativeAST.AssociativeNode identNode = dotCall.DotCall.FormalArguments[0];
            if (identNode is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode idNode = (identNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode;
                EmitIdentifierNode(ref idNode);
                (identNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode = idNode;
            }
            else
                EmitIdentifierNode(ref identNode);
            dotCall.DotCall.FormalArguments[0] = identNode;
            ProtoCore.AST.AssociativeAST.FunctionCallNode funcDotCall = dotCall.FunctionCall;
            EmitFunctionCallNode(ref funcDotCall);
        }

        protected virtual void EmitBinaryNode(ref ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryExprNode)
        {
            Validity.Assert(null != binaryExprNode);
            ProtoCore.AST.AssociativeAST.AssociativeNode leftNode = binaryExprNode.LeftNode;
            DFSTraverse(ref leftNode);

            ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = binaryExprNode.RightNode;
            DFSTraverse(ref rightNode);
        }

        protected virtual void EmitFunctionDefNode(ref ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode)
        {

            if (funcDefNode.ReturnType.UID != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
            }

            if (funcDefNode.Signature != null)
            {
            }
            else
            {
            }

            if (null != funcDefNode.FunctionBody)
            {
                List<ProtoCore.AST.AssociativeAST.AssociativeNode> funcBody = funcDefNode.FunctionBody.Body;

                //EmitCode("{\n");
                foreach (ProtoCore.AST.AssociativeAST.AssociativeNode bodyNode in funcBody)
                {
                    if (bodyNode is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                    {
                        ProtoCore.AST.AssociativeAST.BinaryExpressionNode binaryEpr = bodyNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                        EmitBinaryNode(ref binaryEpr);
                    }

                    if (bodyNode is ProtoCore.AST.AssociativeAST.ReturnNode)
                    {
                        ProtoCore.AST.AssociativeAST.ReturnNode returnNode = bodyNode as ProtoCore.AST.AssociativeAST.ReturnNode;
                        EmitReturnNode(ref returnNode);
                    }
                }                
            }
        }

        protected virtual void EmitReturnNode(ref ProtoCore.AST.AssociativeAST.ReturnNode returnNode)
        {
            ProtoCore.AST.AssociativeAST.AssociativeNode rightNode = returnNode.ReturnExpr;
            DFSTraverse(ref rightNode);
        }

        protected virtual void EmitVarDeclNode(ref ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode)
        {
        }

        protected virtual void EmitClassDeclNode(ref ProtoCore.AST.AssociativeAST.ClassDeclNode classDeclNode)
        {
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> varList = classDeclNode.varlist;
            foreach (ProtoCore.AST.AssociativeAST.AssociativeNode varMember in varList)
            {
                //how is var member stored?
                if (varMember is ProtoCore.AST.AssociativeAST.VarDeclNode)
                {
                    ProtoCore.AST.AssociativeAST.VarDeclNode varDecl = varMember as ProtoCore.AST.AssociativeAST.VarDeclNode;
                    EmitVarDeclNode(ref varDecl);
                }
            }
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> funcList = classDeclNode.funclist;
            foreach (ProtoCore.AST.AssociativeAST.AssociativeNode funcMember in funcList)
            {
                if (funcMember is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode)
                {
                    ProtoCore.AST.AssociativeAST.FunctionDefinitionNode funcDefNode = funcMember as ProtoCore.AST.AssociativeAST.FunctionDefinitionNode;
                    EmitFunctionDefNode(ref funcDefNode);
                }
            }
        }

        protected virtual void EmitNullNode(ref ProtoCore.AST.AssociativeAST.NullNode nullNode)
        {
            Validity.Assert(null != nullNode);
        }
        #endregion
    }
}

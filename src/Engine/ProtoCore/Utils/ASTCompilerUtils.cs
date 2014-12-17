using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ProtoCore.Utils;
using ProtoCore.AST.AssociativeAST;
using System.Globalization;

namespace ProtoCore.Utils
{
    public class ASTCompilerUtils
    {
        private static List<ProtoCore.AST.AssociativeAST.AssociativeNode> CreateArgs(string formatString, List<string> primitiveArgs, List<AssociativeNode> argNodes)
        {
            List<AssociativeNode> args = new List<AssociativeNode>();

            for (int i = 0; i < formatString.Length; i++)
            {
                // TODO: Add a switch-case to create primitive nodes depending on the format string
                if (formatString[i] == 'd')
                {
                    double value;
                    if (Double.TryParse(primitiveArgs[i], NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                    {
                        args.Add(new DoubleNode(value));
                    }
                }
                else if (formatString[i].Equals('i'))
                {
                    Int64 value;
                    if (Int64.TryParse(primitiveArgs[i], out value))
                    {
                        args.Add(new IntNode(value));
                    }
                }
                else if (formatString[i].Equals('b'))
                {
                    Boolean value;
                    if (Boolean.TryParse(primitiveArgs[i], out value))
                    {
                        args.Add(new BooleanNode(value));
                    }
                }
                else if (formatString[i].Equals('s'))
                {
                    IdentifierNode iNode = new IdentifierNode(primitiveArgs[i]);

                    args.Add(iNode);
                }
            }

            return args;
        }

        private static FunctionDotCallNode CreateEntityNode(long hostInstancePtr, Core core)
        {
            FunctionCallNode fNode = new FunctionCallNode();
            fNode.Function = new IdentifierNode("FromObject");
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> listArgs = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
            listArgs.Add(new ProtoCore.AST.AssociativeAST.IntNode(hostInstancePtr));
            fNode.FormalArguments = listArgs;

            string className = "Geometry";
            IdentifierNode inode = new ProtoCore.AST.AssociativeAST.IdentifierNode(className);
            return ProtoCore.Utils.CoreUtils.GenerateCallDotNode(inode, fNode, core);            
        }

        static int staticVariableIndex = 0;
        private static BinaryExpressionNode CreateAssignmentNode(AssociativeNode rhsNode)
        {

            IdentifierNode lhs = new IdentifierNode(string.Format("tVar_{0}", staticVariableIndex++));
            BinaryExpressionNode bNode = new BinaryExpressionNode(lhs, rhsNode, ProtoCore.DSASM.Operator.assign);
            return bNode;
        }

        private static FunctionDotCallNode CreateFunctionCallNode(string className, string methodName, List<AssociativeNode> args, Core core)
        {
            FunctionCallNode fNode = new FunctionCallNode();
            fNode.Function = new IdentifierNode(methodName);
            fNode.FormalArguments = args;

            IdentifierNode inode = new IdentifierNode(className);
            return CoreUtils.GenerateCallDotNode(inode, fNode, core);
        }

        /// <summary>
        /// API used by external host to build AST for any function call
        /// </summary>
        /// <param name="type"></param>
        /// <param name="hostInstancePtr"></param>
        /// <param name="functionName"></param>
        /// <param name="userDefinedArgs"></param>
        /// <param name="primitiveArgs"></param>
        /// <param name="formatString"></param>
        /// <param name="core"></param>
        /// <param name="symbolName"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static AssociativeNode BuildAST(string type, long hostInstancePtr, string functionName, List<IntPtr> userDefinedArgs, List<string> primitiveArgs, string formatString, ProtoCore.Core core, ref string symbolName, ref string code)
        {
            symbolName = string.Empty;
            List<AssociativeNode> astNodes = new List<AssociativeNode>();
            FunctionDotCallNode dotCall = null; 

            BinaryExpressionNode bNode = null;

            ProtoCore.AST.AssociativeAST.FunctionDotCallNode dotCallNode = null;
            List<AssociativeNode> argNodes = new List<AssociativeNode>();
            if (userDefinedArgs != null)
            {
                foreach (var arg in userDefinedArgs)
                {
                    dotCallNode = CreateEntityNode((long)arg, core);
                    bNode = CreateAssignmentNode(dotCallNode);
                    argNodes.Add(bNode);
                }
                astNodes.AddRange(argNodes);
            }
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> args = CreateArgs(formatString, primitiveArgs, argNodes);

            
            if (hostInstancePtr != 0)
            {
                dotCallNode = CreateEntityNode(hostInstancePtr, core);
                bNode = CreateAssignmentNode(dotCallNode);
                astNodes.Add(bNode);

                dotCall = CreateFunctionCallNode((bNode.LeftNode as IdentifierNode).Value, functionName, args, core);                
            }
            else
            {
                dotCall = CreateFunctionCallNode(type, functionName, args, core);
            }
            bNode = CreateAssignmentNode(dotCall);
            if (bNode.LeftNode is IdentifierNode)
            {
                symbolName = (bNode.LeftNode as IdentifierNode).Value;
            }
            astNodes.Add(bNode);

            CodeBlockNode codeBlockNode = new CodeBlockNode();
            codeBlockNode.Body = astNodes;


            ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(astNodes);
            code = codeGen.GenerateCode();

            return codeBlockNode;
        }

        /// <summary>
        /// API for external hosts to build an identifier array assignment node, e.g.: var = {var1, var2, ...}
        /// </summary>
        /// <param name="arrayInputs"></param>
        /// <param name="symbolName"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static AssociativeNode BuildArrayNode(List<string> arrayInputs, ref string symbolName, ref string code)
        {
            ExprListNode arrayNode = AstFactory.BuildExprList(arrayInputs);
            BinaryExpressionNode bNode = CreateAssignmentNode(arrayNode);

            if (bNode.LeftNode is IdentifierNode)
            {
                symbolName = (bNode.LeftNode as IdentifierNode).Value;
            }

            List<AssociativeNode> astNodes = new List<AssociativeNode>();
            astNodes.Add(bNode);
            ProtoCore.CodeGenDS codeGen = new ProtoCore.CodeGenDS(astNodes);
            code = codeGen.GenerateCode();

            return bNode;
        }

        /// <summary>
        /// A node is a single identifier if its type is Identifer and is not array indexed
        /// 
        /// i.e.    identifer: a, foo, _x
        ///         non identifier: a[0], a.b, f(), f(x)
        ///         
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool IsSingleIdentifier(ProtoCore.AST.Node node)
        {
            Validity.Assert(null != node);
            if (node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                // Check the identifier is array indexed
                if (null == (node as ProtoCore.AST.AssociativeAST.IdentifierNode).ArrayDimensions)
                {
                    // The identifier is not array indexed
                    return true;
                }
            }
            else if (node is ProtoCore.AST.ImperativeAST.IdentifierNode)
            {
                // Check the identifier is array indexed
                if (null == (node as ProtoCore.AST.ImperativeAST.IdentifierNode).ArrayDimensions)
                {
                    // The identifier is not array indexed
                    return true;
                }
            }
            return false;
        }
    }

}

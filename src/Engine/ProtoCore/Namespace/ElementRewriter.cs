using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Namespace
{

    public class ElementRewriter
    {
        private readonly ElementResolver elementResolver;

        private ElementRewriter(ElementResolver elementResolver)
        {
            if(elementResolver == null)
                elementResolver = new ElementResolver();

            this.elementResolver = elementResolver;
        }

        /// <summary>
        /// Lookup namespace resolution map to substitute 
        /// partial classnames with their fully qualified names in ASTs.
        /// If partial class is not found in map, 
        /// update ResolutionMap with fully resolved name from compiler.
        /// </summary>
        /// <param name="classTable"></param>
        /// <param name="codeBlockNode"> parent AST node </param>
        public static void ReplaceClassNamesWithResolvedNames(ClassTable classTable,
            ElementResolver elementResolver, ref CodeBlockNode codeBlockNode)
        {
            var elementRewriter = new ElementRewriter(elementResolver);

            var body = codeBlockNode.Body;
            for (int i = 0; i < body.Count; ++i)
            {
                var astNode = body[i];
                elementRewriter.LookupResolvedNameAndRewriteAst(classTable, ref astNode);
            }
        }

        private void LookupResolvedNameAndRewriteAst(ClassTable classTable, ref AssociativeNode astNode)
        {
            // Get partial class identifier/identifier lists
            var classIdentifiers = GetClassIdentifiers(astNode);

            foreach (var identifier in classIdentifiers)
            {
                var partialName = CoreUtils.GetIdentifierStringUntilFirstParenthesis(identifier);
                var resolvedName = elementResolver.LookupResolvedName(partialName);
                if (!string.IsNullOrEmpty(resolvedName))
                {
                    RewriteAstWithResolvedName(ref astNode, resolvedName);
                }
                else
                {
                    // If namespace resolution map does not contain entry for partial name, 
                    // back up on compiler to resolve the namespace from partial name

                    //var identNode = new IdentifierNode(partialName.Split('.').Last());
                    //var matchingClasses = CoreUtils.GetResolvedClassName(classTable,
                    //    RewriteIdentifierListNode(identNode, partialName));
                    var matchingClasses = CoreUtils.GetResolvedClassName(classTable, identifier);

                    if (matchingClasses.Length == 1)
                    {
                        resolvedName = matchingClasses[0];
                        string assemblyName = CoreUtils.GetAssemblyFromClassName(classTable, partialName);

                        elementResolver.AddToResolutionMap(partialName, resolvedName, assemblyName);
                        RewriteAstWithResolvedName(ref astNode, resolvedName);
                    }
                }
            }
        }

        private static void DfsTraverse(ref AssociativeNode astNode, ref List<IdentifierListNode> classIdentifiers, string resolvedName)
        {
            if (astNode is BinaryExpressionNode)
            {
                var bnode = astNode as BinaryExpressionNode;
                AssociativeNode leftNode = bnode.LeftNode;
                AssociativeNode rightNode = bnode.RightNode;
                DfsTraverse(ref leftNode, ref classIdentifiers, resolvedName);
                DfsTraverse(ref rightNode, ref classIdentifiers, resolvedName);

                bnode.LeftNode = leftNode;
                bnode.RightNode = rightNode;
            }
            else if (astNode is FunctionCallNode)
            {
                var fCall = astNode as FunctionCallNode;
                for (int n = 0; n < fCall.FormalArguments.Count; ++n)
                {
                    AssociativeNode argNode = fCall.FormalArguments[n];
                    DfsTraverse(ref argNode, ref classIdentifiers, resolvedName);
                }
            }
            else if (astNode is ExprListNode)
            {
                var exprList = astNode as ExprListNode;
                for (int n = 0; n < exprList.list.Count; ++n)
                {
                    AssociativeNode exprNode = exprList.list[n];
                    DfsTraverse(ref exprNode, ref classIdentifiers, resolvedName);
                }
            }
            else if (astNode is InlineConditionalNode)
            {
                var inlineNode = astNode as InlineConditionalNode;
                AssociativeNode condition = inlineNode.ConditionExpression;
                AssociativeNode trueBody = inlineNode.TrueExpression;
                AssociativeNode falseBody = inlineNode.FalseExpression;
                DfsTraverse(ref condition, ref classIdentifiers, resolvedName);
                DfsTraverse(ref trueBody, ref classIdentifiers, resolvedName);
                DfsTraverse(ref falseBody, ref classIdentifiers, resolvedName);
            }
            else if (astNode is IdentifierListNode)
            {
                var identListNode = astNode as IdentifierListNode;
                var leftNode = identListNode.LeftNode;
                var rightNode = identListNode.RightNode;

                if (rightNode is FunctionCallNode)
                {
                    if (string.IsNullOrEmpty(resolvedName))
                    {
                        classIdentifiers.Add(leftNode as IdentifierListNode);
                    }
                    else
                    {
                        astNode = RewriteIdentifierListNode(identListNode.RightNode, resolvedName);
                    }
                    DfsTraverse(ref rightNode, ref classIdentifiers, resolvedName);
                }
                if (string.IsNullOrEmpty(resolvedName))
                {
                    classIdentifiers.Add(identListNode);
                }   
                else
                {
                    astNode = RewriteIdentifierListNode(identListNode.RightNode, resolvedName);
                }
            }
            
        }

        private static IEnumerable<IdentifierListNode> GetClassIdentifiers(AssociativeNode astNode)
        {
            var classIdentifiers = new List<IdentifierListNode>();
            DfsTraverse(ref astNode, ref classIdentifiers, resolvedName:string.Empty);
            return classIdentifiers;
        }

        /// <summary>
        /// Replace partial identifier with fully resolved identifier list in original AST
        /// This is the same AST that is also passed to the VM for execution
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="partialName"> partial class name identifier </param>
        /// <param name="resolvedName"> fully qualified class identifier list </param>
        private static void RewriteAstWithResolvedName(ref AssociativeNode astNode, string resolvedName)
        {
            List<IdentifierListNode> classIdentifiers = null;
            DfsTraverse(ref astNode, ref classIdentifiers, resolvedName);
        }


        private static IdentifierListNode RewriteIdentifierListNode(AssociativeNode rightNode, string resolvedName)
        {
            string[] strIdentList = resolvedName.Split('.');
            Validity.Assert(strIdentList.Length >= 2);

            var newIdentList = new IdentifierListNode
            {
                LeftNode = new IdentifierNode(strIdentList[0]),
                RightNode = new IdentifierNode(strIdentList[1]),
                Optr = Operator.dot
            };
            for (int n = 2; n < strIdentList.Length; ++n)
            {
                var subIdentList = new IdentifierListNode
                {
                    LeftNode = newIdentList,
                    RightNode = new IdentifierNode(strIdentList[n]),
                    Optr = Operator.dot
                };
                newIdentList = subIdentList;
            }

            // The last ident list for the functioncall or identifier rhs
            var lastIdentList = new IdentifierListNode
            {
                LeftNode = newIdentList,
                RightNode = rightNode,
                Optr = Operator.dot
            };

            return lastIdentList;
        }
    }
}

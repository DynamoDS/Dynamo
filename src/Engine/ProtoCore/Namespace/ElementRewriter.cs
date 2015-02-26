using System.Diagnostics;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Namespace
{

    public static class ElementRewriter
    {
        /// <summary>
        /// Lookup namespace resolution map to substitute 
        /// partial classnames with their fully qualified names in ASTs.
        /// If partial class is not found in map, 
        /// update ResolutionMap with fully resolved name from compiler.
        /// </summary>
        /// <param name="classTable"></param>
        /// <param name="elementResolver"></param>
        /// <param name="astNodes"> parent AST node </param>
        public static void RewriteElementNames(ClassTable classTable,
            ElementResolver elementResolver, IEnumerable<Node> astNodes)
        {
            foreach (var node in astNodes)
            {
                var astNode = node as AssociativeNode;
                if (astNode == null)
                    continue;

                LookupResolvedNameAndRewriteAst(classTable, elementResolver, ref astNode);
            }
        }

        private static void LookupResolvedNameAndRewriteAst(ClassTable classTable, ElementResolver elementResolver, 
            ref AssociativeNode astNode)
        {
            Debug.Assert(elementResolver != null);

            // Get partial class identifier/identifier lists
            var classIdentifiers = GetClassIdentifiers(astNode);

            var resolvedNames = new Queue<string>();
            foreach (var identifier in classIdentifiers)
            {
                var partialName = CoreUtils.GetIdentifierExceptMethodName(identifier);
                var resolvedName = elementResolver.LookupResolvedName(partialName);
                if (string.IsNullOrEmpty(resolvedName))
                {
                    // If namespace resolution map does not contain entry for partial name, 
                    // back up on compiler to resolve the namespace from partial name
                    var matchingClasses = CoreUtils.GetResolvedClassName(classTable, identifier);

                    if (matchingClasses.Length == 1)
                    {
                        resolvedName = matchingClasses[0];
                        var assemblyName = CoreUtils.GetAssemblyFromClassName(classTable, resolvedName);

                        elementResolver.AddToResolutionMap(partialName, resolvedName, assemblyName);
                    }
                    else
                    {
                        // Namespace conflict, class name could not be resolved.
                        // This will be reported subsequently in the pre-compilation stage
                        return;
                    }
                }
                resolvedNames.Enqueue(resolvedName);
            }
            
            RewriteAstWithResolvedName(ref astNode, resolvedNames);
        }

        private static IEnumerable<IdentifierListNode> GetClassIdentifiers(AssociativeNode astNode)
        {
            var classIdentifiers = new List<IdentifierListNode>();
            var resolvedNames = new Queue<string>();
            DfsTraverse(ref astNode, classIdentifiers, resolvedNames);
            return classIdentifiers;
        }

        /// <summary>
        /// Replace partial identifier with fully resolved identifier list in original AST
        /// This is the same AST that is also passed to the VM for execution
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="resolvedNames"> fully qualified class identifier list </param>
        private static void RewriteAstWithResolvedName(ref AssociativeNode astNode, Queue<string> resolvedNames)
        {
            DfsTraverse(ref astNode, null, resolvedNames);
        }

        #region private utility methods

        private static void DfsTraverse(ref AssociativeNode astNode, ICollection<IdentifierListNode> classIdentifiers, 
            Queue<string> resolvedNames)
        {
            if (astNode is BinaryExpressionNode)
            {
                var bnode = astNode as BinaryExpressionNode;
                AssociativeNode leftNode = bnode.LeftNode;
                AssociativeNode rightNode = bnode.RightNode;
                DfsTraverse(ref leftNode, classIdentifiers, resolvedNames);
                DfsTraverse(ref rightNode, classIdentifiers, resolvedNames);

                bnode.LeftNode = leftNode;
                bnode.RightNode = rightNode;
            }
            else if (astNode is FunctionCallNode)
            {
                var fCall = astNode as FunctionCallNode;
                for (int n = 0; n < fCall.FormalArguments.Count; ++n)
                {
                    AssociativeNode argNode = fCall.FormalArguments[n];
                    DfsTraverse(ref argNode, classIdentifiers, resolvedNames);
                    fCall.FormalArguments[n] = argNode;
                }
            }
            else if (astNode is ExprListNode)
            {
                var exprList = astNode as ExprListNode;
                for (int n = 0; n < exprList.list.Count; ++n)
                {
                    AssociativeNode exprNode = exprList.list[n];
                    DfsTraverse(ref exprNode, classIdentifiers, resolvedNames);
                    exprList.list[n] = exprNode;
                }
            }
            else if (astNode is InlineConditionalNode)
            {
                var inlineNode = astNode as InlineConditionalNode;
                AssociativeNode condition = inlineNode.ConditionExpression;
                AssociativeNode trueBody = inlineNode.TrueExpression;
                AssociativeNode falseBody = inlineNode.FalseExpression;

                DfsTraverse(ref condition, classIdentifiers, resolvedNames);
                DfsTraverse(ref trueBody, classIdentifiers, resolvedNames);
                DfsTraverse(ref falseBody, classIdentifiers, resolvedNames);

                inlineNode.ConditionExpression = condition;
                inlineNode.FalseExpression = falseBody;
                inlineNode.TrueExpression = trueBody;
            }
            else if (astNode is IdentifierListNode)
            {
                var identListNode = astNode as IdentifierListNode;
                var rightNode = identListNode.RightNode;

                if (rightNode is FunctionCallNode)
                {
                    DfsTraverse(ref rightNode, classIdentifiers, resolvedNames);
                }
                if (resolvedNames.Any())
                {
                    astNode = RewriteIdentifierListNode(rightNode, resolvedNames);
                }
                else
                {
                    classIdentifiers.Add(identListNode);
                }
            }

        }

        private static IdentifierListNode RewriteIdentifierListNode(AssociativeNode rightNode, Queue<string> resolvedNames)
        {
            var resolvedName = resolvedNames.Dequeue();

            string[] strIdentList = resolvedName.Split('.');
            Validity.Assert(strIdentList.Length >= 2);

            var newIdentList = new IdentifierListNode
            {
                LeftNode = new IdentifierNode(strIdentList[0]),
                RightNode = new IdentifierNode(strIdentList[1]),
                Optr = Operator.dot
            };
            for (var n = 2; n < strIdentList.Length; ++n)
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

        #endregion
    }
}

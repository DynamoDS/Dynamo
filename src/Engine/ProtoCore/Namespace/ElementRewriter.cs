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

    public class ElementRewriter
    {
        private readonly ClassTable classTable;
        
        internal ElementRewriter(ClassTable classTable)
        {
            this.classTable = classTable;
        }

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
            var elementRewriter = new ElementRewriter(classTable);
            foreach (var node in astNodes)
            {
                var astNode = node as AssociativeNode;
                if (astNode == null)
                    continue;

                elementRewriter.LookupResolvedNameAndRewriteAst(elementResolver, ref astNode);
            }
        }

        internal void LookupResolvedNameAndRewriteAst(ElementResolver elementResolver, 
            ref AssociativeNode astNode)
        {
            Debug.Assert(elementResolver != null);

            // Get partial class identifier/identifier lists
            var classIdentifiers = GetClassIdentifiers(astNode);

            var resolvedNames = new Queue<string>();
            foreach (var identifier in classIdentifiers)
            {
                string partialName = string.Empty;
                var identifierList = identifier as IdentifierListNode;
                if (identifierList != null)
                {
                    partialName = CoreUtils.GetIdentifierExceptMethodName(identifierList);    
                }
                else
                {
                    partialName = identifier.Name;
                }
                
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
                        // Class name could not be resolved - Possible namespace conflict 
                        // This will be reported subsequently in the pre-compilation stage if there's a conflict
                        // Enter an empty resolved name to the list and continue
                        resolvedNames.Enqueue(string.Empty);
                        continue;
                    }
                }
                resolvedNames.Enqueue(resolvedName);
            }
            
            if(resolvedNames.Any())
                RewriteAstWithResolvedName(ref astNode, resolvedNames);
        }

        internal IEnumerable<AssociativeNode> GetClassIdentifiers(AssociativeNode astNode)
        {
            var classIdentifiers = new List<AssociativeNode>();
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
        private void RewriteAstWithResolvedName(ref AssociativeNode astNode, Queue<string> resolvedNames)
        {
            DfsTraverse(ref astNode, null, resolvedNames);
        }

        #region private utility methods

        private void DfsTraverse(ref AssociativeNode astNode, ICollection<AssociativeNode> classIdentifiers, 
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
            else if (astNode is TypedIdentifierNode)
            {
                var typedNode = astNode as TypedIdentifierNode;

                // If type is primitive type
                if (typedNode.datatype.UID != (int)PrimitiveType.kInvalidType &&
                    typedNode.datatype.UID < (int) PrimitiveType.kMaxPrimitives)
                    return;

                var identListNode = CoreUtils.CreateNodeFromString(typedNode.TypeAlias);

                // Rewrite node with resolved name
                if (resolvedNames.Any())
                {
                    if (identListNode is IdentifierNode)
                    {
                        identListNode = RewriteIdentifierListNode(identListNode, resolvedNames);
                    }
                    else
                        DfsTraverse(ref identListNode, classIdentifiers, resolvedNames);

                    var identListString = identListNode.ToString();
                    int indx = identListString.LastIndexOf('.');
                    string name = indx >= 0 ? identListString.Remove(indx) : identListString;

                    var type = new Type
                    {
                        Name = name,
                        UID = classTable.IndexOf(name),
                        rank = typedNode.datatype.rank
                    };

                    typedNode = new TypedIdentifierNode
                    {
                        Name = astNode.Name,
                        Value = astNode.Name,
                        datatype = type
                    };

                    NodeUtils.CopyNodeLocation(typedNode, astNode);
                    astNode = typedNode;
                }
                else if (identListNode is IdentifierNode)
                {
                    classIdentifiers.Add(identListNode);
                }
                else
                {
                    DfsTraverse(ref identListNode, classIdentifiers, resolvedNames);
                }
                
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
                    astNode = RewriteIdentifierListNode(identListNode, resolvedNames);
                }
                else
                {
                    classIdentifiers.Add(identListNode);
                }
            }

        }

        private static AssociativeNode RewriteIdentifierListNode(AssociativeNode identifier, Queue<string> resolvedNames)
        {
            var resolvedName = resolvedNames.Dequeue();

            // if resolved name is null or empty, return the identifier list node as is
            if (string.IsNullOrEmpty(resolvedName))
                return identifier;

            var newIdentList = CoreUtils.CreateNodeFromString(resolvedName);
            Validity.Assert(newIdentList is IdentifierListNode);

            var identListNode = identifier as IdentifierListNode;
            AssociativeNode rightNode = identListNode != null ? identListNode.RightNode : identifier;

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

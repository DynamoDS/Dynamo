using System.Diagnostics;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Namespace
{

    public class ElementRewriter : AstReplacer
    {
        private readonly ClassTable classTable;
        private readonly ElementResolver elementResolver;

        internal ElementRewriter(ClassTable classTable, ElementResolver resolver)
        {
            this.classTable = classTable;
            this.elementResolver = resolver;
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
        public static IEnumerable<Node> RewriteElementNames(ClassTable classTable,
            ElementResolver elementResolver, IEnumerable<Node> astNodes)
        {
            var elementRewriter = new ElementRewriter(classTable, elementResolver);
            return astNodes.OfType<AssociativeNode>().Select(astNode => astNode.Accept(elementRewriter)).Cast<Node>().ToList();
        }

        #region overridden methods

        public override AssociativeNode VisitTypedIdentifierNode(TypedIdentifierNode node)
        {
            // If type is primitive type
            if (node.datatype.UID != (int)PrimitiveType.kInvalidType &&
                node.datatype.UID < (int)PrimitiveType.kMaxPrimitives)
                return node;

            var identListNode = CoreUtils.CreateNodeFromString(node.TypeAlias);

            // Rewrite node with resolved name
            if (identListNode is IdentifierNode)
            {
                identListNode = RewriteIdentifierListNode(identListNode);
            }
            else
                identListNode = identListNode.Accept(this);

            var identListString = identListNode.ToString();
            var type = new Type
            {
                Name = identListString,
                UID = classTable.IndexOf(identListString),
                rank = node.datatype.rank
            };

            var typedNode = new TypedIdentifierNode
            {
                Name = node.Name,
                Value = node.Name,
                datatype = type,
                TypeAlias = node.TypeAlias
            };

            NodeUtils.CopyNodeLocation(typedNode, node);
            return typedNode;
        }

        public override AssociativeNode VisitIdentifierListNode(IdentifierListNode node)
        {
            // First pass attempt to resolve the node before traversing it deeper
            AssociativeNode newIdentifierListNode = null;
            if (IsMatchingResolvedName(node, out newIdentifierListNode))
                return newIdentifierListNode;

            var rightNode = node.RightNode;
            var leftNode = node.LeftNode;

            rightNode = rightNode.Accept(this);
            leftNode = leftNode.Accept(this);

            node = new IdentifierListNode
            {
                LeftNode = leftNode,
                RightNode = rightNode,
                Optr = Operator.dot
            };
            return RewriteIdentifierListNode(node);
        }

        #endregion

        #region private helper methods

        private bool IsMatchingResolvedName(IdentifierListNode identifierList, out AssociativeNode newIdentList)
        {
            newIdentList = null;
            var resolvedName = ResolveClassName(identifierList);
            if (string.IsNullOrEmpty(resolvedName))
                return false;

            newIdentList = CoreUtils.CreateNodeFromString(resolvedName);
            
            var symbol = new Symbol(resolvedName);
            return symbol.Matches(identifierList.ToString());
        }

        private string ResolveClassName(AssociativeNode identifierList)
        {
            var identListNode = identifierList as IdentifierListNode;

            string partialName = identListNode != null ?
                CoreUtils.GetIdentifierExceptMethodName(identListNode) : identifierList.Name;
            
            if(string.IsNullOrEmpty(partialName))
                return String.Empty;

            var resolvedName = elementResolver.LookupResolvedName(partialName);
            if (string.IsNullOrEmpty(resolvedName))
            {
                // If namespace resolution map does not contain entry for partial name, 
                // back up on compiler to resolve the namespace from partial name
                var matchingClasses = CoreUtils.GetResolvedClassName(classTable, identifierList);

                if (matchingClasses.Length == 1)
                {
                    resolvedName = matchingClasses[0];
                    var assemblyName = CoreUtils.GetAssemblyFromClassName(classTable, resolvedName);

                    elementResolver.AddToResolutionMap(partialName, resolvedName, assemblyName);
                }
            }
            return resolvedName;
        }

        private AssociativeNode RewriteIdentifierListNode(AssociativeNode identifierList)
        {
            var identListNode = identifierList as IdentifierListNode;
            var resolvedName = ResolveClassName(identifierList);

            if (string.IsNullOrEmpty(resolvedName))
                return identifierList;

            var newIdentList = CoreUtils.CreateNodeFromString(resolvedName);

            // If the original input node matches with the resolved name, simply return 
            // the identifier list constructed from the resolved name
            var symbol = new Symbol(resolvedName);
            if (symbol.Matches(identifierList.ToString()))
                return newIdentList;

            // Remove partialName from identListNode and replace with newIdentList
            AssociativeNode leftNode = identListNode != null ? identListNode.LeftNode : identifierList;
            AssociativeNode rightNode = identListNode != null ? identListNode.RightNode : identifierList;

            var intermediateNodes = new List<AssociativeNode>();
            while (leftNode is IdentifierListNode && !symbol.Matches(leftNode.ToString()))
            {
                intermediateNodes.Insert(0, ((IdentifierListNode)leftNode).RightNode);
                leftNode = ((IdentifierListNode)leftNode).LeftNode;
            }
            intermediateNodes.Insert(0, newIdentList);

            var lNode = CoreUtils.CreateNodeByCombiningIdentifiers(intermediateNodes);

            // The last ident list for the functioncall or identifier rhs
            var lastIdentList = new IdentifierListNode
            {
                LeftNode = lNode,
                RightNode = rightNode,
                Optr = Operator.dot
            };

            return lastIdentList;
        }

        #endregion
    }
}

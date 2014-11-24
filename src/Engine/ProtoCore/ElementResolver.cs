using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore
{
    // Responsible for resolving a partial class name to its fully resolved name
    public class ElementResolver
    {
        private Dictionary<AssociativeNode, IdentifierListNode> namespaceCache;


        /// <summary>
        /// Maintains a table of partial class IdentifierList vs. its fully qualified IdentifierList 
        /// </summary>
        public Dictionary<AssociativeNode, IdentifierListNode> NamespaceCache
        {
            get { return namespaceCache; }
        }

        #region public constructors and methods
        public ElementResolver() { }

        public ElementResolver(string[] namespaceList)
        {
            namespaceCache = new Dictionary<AssociativeNode,IdentifierListNode>();
            InitializeNamespaceCache(namespaceList);
        }


        public void ResolveClassNamespace(ClassTable classTable, CodeBlockNode codeBlockNode)
        {
            var body = codeBlockNode.Body;
            for (int i = 0; i < body.Count; ++i)
            {
                var astNode = body[i];
                UpdateASTWithFullyQualifiedName(classTable, ref astNode);
            }
        }
        #endregion

        #region private methods
        private void UpdateASTWithFullyQualifiedName(ClassTable classTable, ref AssociativeNode astNode)
        {
            // Get partial class identifier/identifier lists
            IEnumerable<AssociativeNode> classIdentifierNodes = GetClassIdentifiers(astNode);

            foreach (var partialNode in classIdentifierNodes)
            {
                if (namespaceCache != null)
                    namespaceCache = new Dictionary<AssociativeNode, IdentifierListNode>();

                IdentifierListNode fullNode = null;
                if (namespaceCache.TryGetValue(partialNode, out fullNode))
                {
                    ReplacePartialWithFullNode(ref astNode, partialNode, fullNode);
                }
                else
                {
                    // If cache does not contain entry for partial name, 
                    // back up on compiler to resolve the namespace from partial name
                    fullNode = GetResolvedClassName(classTable, partialNode);
                    
                    namespaceCache.Add(partialNode, fullNode);
                    ReplacePartialWithFullNode(ref astNode, partialNode, fullNode);
                }
            }
        }

        /// <summary>
        /// Get fully resolved name as identifier list from class table
        /// </summary>
        /// <param name="classTable"> class table in Core </param>
        /// <param name="partialNode"> partial class name as identifier/identifier list </param>
        /// <returns> fully resolved name as Identifier list </returns>
        private static IdentifierListNode GetResolvedClassName(ClassTable classTable, AssociativeNode partialNode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replace partial identifier with fully resolved identifier list in original AST
        /// This is the same AST that is also passed to the VM for execution
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="partialNode"> partial class name identifier </param>
        /// <param name="fullNode"> fully qualified class identifier list node </param>
        private static void ReplacePartialWithFullNode(ref AssociativeNode astNode, AssociativeNode partialNode, IdentifierListNode fullNode)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find all partial class (Identifier/IdentifierListNode's) by performing a DFS traversal on input AST node
        /// </summary>
        /// <param name="astNode"> input AST node </param>
        /// <returns> list of IdentifierNode/IdentifierListNode of Class identifiers </returns>
        private static IEnumerable<AssociativeNode> GetClassIdentifiers(AST.Node astNode)
        {
            throw new NotImplementedException();
        }


        // Given an input fully resolved class name, convert it into an Identifier/IdentifierListNode
        private AssociativeNode InitializeNamespaceCache(string[] namespaceList)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

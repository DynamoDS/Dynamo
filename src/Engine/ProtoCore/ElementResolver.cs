using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore
{
    /// <summary>
    /// Responsible for resolving a partial class name to its fully resolved name
    /// </summary>
    public class ElementResolver
    {
        private Dictionary<string, string> resolutionMap;


        /// <summary>
        /// Maintains a lookup table of partial class identifiers vs. its fully qualified identifier names
        /// </summary>
        public Dictionary<string, string> ResolutionMap
        {
            get { return resolutionMap; }
        }

        #region public constructors and methods
        public ElementResolver() { }

        public ElementResolver(string[] namespaceLookupMap)
        {
            resolutionMap = new Dictionary<string, string>();
            InitializeNamespaceResolutionMap(namespaceLookupMap);
        }


        public void ReplaceClassNamesWithResolvedNames(ClassTable classTable, ref CodeBlockNode codeBlockNode)
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
            IEnumerable<string> classIdentifiers = GetClassIdentifiers(astNode);

            foreach (var partialName in classIdentifiers)
            {
                if (resolutionMap == null)
                    resolutionMap = new Dictionary<string, string>();

                string resolvedName;
                if (resolutionMap.TryGetValue(partialName, out resolvedName))
                {
                    ReplacePartialWithFullNode(ref astNode, partialName, resolvedName);
                }
                else
                {
                    // If namespace resolution map does not contain entry for partial name, 
                    // back up on compiler to resolve the namespace from partial name
                    resolvedName = GetResolvedClassName(classTable, partialName);
                    
                    resolutionMap.Add(partialName, resolvedName);
                    ReplacePartialWithFullNode(ref astNode, partialName, resolvedName);
                }
            }
        }

        /// <summary>
        /// Get fully resolved name as identifier list from class table
        /// </summary>
        /// <param name="classTable"> class table in Core </param>
        /// <param name="partialName"> partial class name </param>
        /// <returns> fully resolved name as Identifier list </returns>
        public static string GetResolvedClassName(ClassTable classTable, string partialName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replace partial identifier with fully resolved identifier list in original AST
        /// This is the same AST that is also passed to the VM for execution
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="partialName"> partial class name identifier </param>
        /// <param name="resolvedName"> fully qualified class identifier list </param>
        private static void ReplacePartialWithFullNode(ref AssociativeNode astNode, string partialName, string resolvedName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Find all partial class (Identifier/Identifier lists) by performing a DFS traversal on input AST node
        /// </summary>
        /// <param name="astNode"> input AST node </param>
        /// <returns> list of IdentifierNode/IdentifierListNode of Class identifiers </returns>
        private static IEnumerable<string> GetClassIdentifiers(AST.Node astNode)
        {
            throw new NotImplementedException();
        }


        // Given a fully resolved class name, convert it into an Identifier/IdentifierListNode
        private void InitializeNamespaceResolutionMap(string[] namespaceLookupMap)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

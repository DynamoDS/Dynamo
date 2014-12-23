using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
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
            ElementRewriter elementRewriter = new ElementRewriter(elementResolver);

            var body = codeBlockNode.Body;
            for (int i = 0; i < body.Count; ++i)
            {
                var astNode = body[i];
                elementRewriter.LookupResolvedNameAndRewriteAST(classTable, ref astNode);
            }
        }

        private void LookupResolvedNameAndRewriteAST(ClassTable classTable, ref AssociativeNode astNode)
        {
            // Get partial class identifier/identifier lists
            IEnumerable<string> classIdentifiers = ProtoCore.Utils.CoreUtils.GetClassIdentifiers(astNode);

            foreach (var partialName in classIdentifiers)
            {
                string resolvedName = elementResolver.LookupResolvedName(partialName);
                if (!string.IsNullOrEmpty(resolvedName))
                {
                    RewriteASTWithResolvedName(ref astNode, partialName, resolvedName);
                }
                else
                {
                    // If namespace resolution map does not contain entry for partial name, 
                    // back up on compiler to resolve the namespace from partial name
                    resolvedName = ProtoCore.Utils.CoreUtils.GetResolvedClassName(classTable, partialName);
                    string assemblyName = ProtoCore.Utils.CoreUtils.GetAssemblyFromClassName(classTable, partialName);

                    elementResolver.AddToResolutionMap(partialName, resolvedName, assemblyName);
                    RewriteASTWithResolvedName(ref astNode, partialName, resolvedName);
                }
            }
        }

        /// <summary>
        /// Replace partial identifier with fully resolved identifier list in original AST
        /// This is the same AST that is also passed to the VM for execution
        /// </summary>
        /// <param name="astNode"></param>
        /// <param name="partialName"> partial class name identifier </param>
        /// <param name="resolvedName"> fully qualified class identifier list </param>
        private static void RewriteASTWithResolvedName(ref AssociativeNode astNode, string partialName, string resolvedName)
        {
            throw new NotImplementedException();
        }
    }
}

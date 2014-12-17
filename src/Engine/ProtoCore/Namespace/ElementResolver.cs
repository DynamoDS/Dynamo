using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtoCore.Namespace
{
    /// <summary>
    /// Responsible for resolving a partial class name to its fully resolved name
    /// </summary>
    public class ElementResolver
    {
        private Dictionary<string, string> resolutionMap;

        /// <summary>
        /// Maintains a lookup table of partial class identifiers vs. 
        /// fully qualified class identifier names
        /// </summary>
        public Dictionary<string, string> ResolutionMap
        {
            get { return resolutionMap; }
        }

        #region public constructors and methods
        
        public ElementResolver(string[] namespaceLookupMap)
        {
            resolutionMap = new Dictionary<string, string>();
            InitializeNamespaceResolutionMap(namespaceLookupMap);
        }

        public string LookupResolvedName(string partialName)
        {
            string resolvedName = string.Empty;

            resolutionMap.TryGetValue(partialName, out resolvedName);
            
            return resolvedName;
        }

        public void AddToResolutionMap(string partialName, string resolvedName)
        {
            resolutionMap.Add(partialName, resolvedName);
        }

        
        #endregion

        #region private methods

        // Initialize ResolutionMap from lookup table of strings
        private void InitializeNamespaceResolutionMap(string[] namespaceLookupMap)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}

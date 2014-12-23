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
        private Dictionary<string, KeyValuePair<string, string>> resolutionMap;

        /// <summary>
        /// Maintains a lookup table of partial class identifiers vs. 
        /// fully qualified class identifier names and assembly name
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> ResolutionMap
        {
            get { return resolutionMap; }
        }

        #region public constructors and methods

        public ElementResolver()
        {
            resolutionMap = new Dictionary<string, KeyValuePair<string, string>>();
        }

        public ElementResolver(Dictionary<string, KeyValuePair<string, string>> namespaceLookupMap)
        {
            resolutionMap = namespaceLookupMap;
        }

        public string LookupResolvedName(string partialName)
        {
            KeyValuePair<string, string> resolvedName;

            resolutionMap.TryGetValue(partialName, out resolvedName);
            
            return resolvedName.Key;
        }

        public string LookupAssemblyName(string partialName)
        {
            KeyValuePair<string, string> resolvedName;

            resolutionMap.TryGetValue(partialName, out resolvedName);

            return resolvedName.Value;
        }

        public void AddToResolutionMap(string partialName, string resolvedName, string assemblyName)
        {
            var kvp = new KeyValuePair<string, string>(resolvedName, assemblyName);
            resolutionMap.Add(partialName, kvp);
        }

        
        #endregion

    }

}

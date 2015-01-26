
﻿using System.Collections.Generic;
﻿using System.Linq;

namespace ProtoCore.Namespace
{
    /// <summary>
    /// Responsible for resolving a partial class name to its fully resolved name
    /// </summary>
    public class ElementResolver
    {
        private readonly IDictionary<string, KeyValuePair<string, string>> resolutionMap;

        /// <summary>
        /// Maintains a lookup table of partial class identifiers vs. 
        /// fully qualified class identifier names and assembly name
        /// </summary>
        public IDictionary<string, KeyValuePair<string, string>> ResolutionMap
        {
            get { return resolutionMap; }
        }

        #region public constructors and methods

        public ElementResolver()
        {
            resolutionMap = new Dictionary<string, KeyValuePair<string, string>>();
        }

        public ElementResolver(IDictionary<string, KeyValuePair<string, string>> namespaceLookupMap)
        {
            resolutionMap = namespaceLookupMap;
        }

        public void CopyResolutionMap(ElementResolver otherResolver)
        {
            foreach (var e in otherResolver.ResolutionMap.Where(e => !resolutionMap.ContainsKey(e.Key)))
            {
                resolutionMap.Add(e);
            }
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
            if(!resolutionMap.ContainsKey(partialName))
                resolutionMap.Add(partialName, kvp);
        }

        
        #endregion

    }

}

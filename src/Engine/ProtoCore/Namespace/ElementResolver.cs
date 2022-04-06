
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Looks up resolved name in resolution map given the partial name
        /// </summary>
        /// <param name="partialName"></param>
        /// <returns> returns null if partial name is not found in resolution map </returns>
        public string LookupResolvedName(string partialName)
        {
            KeyValuePair<string, string> resolvedName;

            resolutionMap.TryGetValue(partialName, out resolvedName);
            
            return resolvedName.Key;
        }

        public string LookupShortName(string resolvedName)
        {
            var nameList = (from keyValuePair in ResolutionMap 
                            where keyValuePair.Value.Key == resolvedName 
                            select keyValuePair.Key).ToList();

            // return the shortest partial name (key)
            return nameList.Any() ? nameList.OrderBy(x => x.Length).First() : string.Empty;
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

using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.DSASM
{ 
    /// <summary>
    /// Extension to the normal Dictionary. This class can store more than one value for every key. It keeps a HashSet for every Key value.
    /// Calling Add with the same Key and multiple values will store each value under the same Key in the Dictionary. Obtaining the values
    /// for a Key will return the HashSet with the Values of the Key. 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class MultiValueDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        /// <summary>
        /// Adds the specified value under the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");

            HashSet<TValue> container = null;
            if (!TryGetValue(key, out container))
            {
                container = new HashSet<TValue>();
                base.Add(key, container);
            }
            container.Add(value);
        }


        /// <summary>
        /// Determines whether this dictionary contains the specified value for the specified key 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the value is stored for the specified key in this dictionary, false otherwise</returns>
        public bool ContainsValue(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");
            bool toReturn = false;
            HashSet<TValue> values = null;
            if (TryGetValue(key, out values))
            {
                toReturn = values.Contains(value);
            }
            return toReturn;
        }


        /// <summary>
        /// Removes the specified value for the specified key. It will leave the key in the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Remove(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");

            HashSet<TValue> container = null;
            if (TryGetValue(key, out container))
            {
                container.Remove(value);
                if (container.Count <= 0)
                {
                    Remove(key);
                }
            }
        }


        /// <summary>
        /// Merges the specified multivaluedictionary into this instance.
        /// </summary>
        /// <param name="toMergeWith">To merge with.</param>
        public void Merge(MultiValueDictionary<TKey, TValue> toMergeWith)
        {
            if (toMergeWith == null)
            {
                return;
            }

            foreach (KeyValuePair<TKey, HashSet<TValue>> pair in toMergeWith)
            {
                foreach (TValue value in pair.Value)
                {
                    Add(pair.Key, value);
                }
            }
        }


        /// <summary>
        /// Gets the values for the key specified. This method is useful if you want to avoid an exception for key value retrieval and you can't use TryGetValue
        /// (e.g. in lambdas)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="returnEmptySet">if set to true and the key isn't found, an empty hashset is returned, otherwise, if the key isn't found, null is returned</param>
        /// <returns>
        /// This method will return null (or an empty set if returnEmptySet is true) if the key wasn't found, or
        /// the values if key was found.
        /// </returns>
        public HashSet<TValue> GetValues(TKey key, bool returnEmptySet)
        {
            HashSet<TValue> toReturn = null;
            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
            {
                toReturn = new HashSet<TValue>();
            }
            return toReturn;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{name}, fi = {functionIndex}, ci = {classScope}, block = {runtimeTableIndex}")]
    public class SymbolNode
    {
        public string           name;
        public string           forArrayName;
        public int              index;
        public int              heapIndex;
        public int              classScope;
        public int              functionIndex;
        public int              absoluteClassScope;
        public int              absoluteFunctionIndex;
        public ProtoCore.Type   datatype;
        public ProtoCore.Type   staticType;
        
        public bool             isArgument;
        public bool             isTemp;
        public int              size;
        public int              datasize;
        public bool             isArray;
        public List<int>        arraySizeList;
        public MemoryRegion     memregion;
        public int              symbolTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        public int              runtimeTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        public AccessSpecifier  access;
        public bool isStatic;
        public List<AttributeEntry> Attributes { get; set; }
        public int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex;
        public string ExternLib = "";

        public SymbolNode()
        {
            name = string.Empty;
            isArray         = false;
            arraySizeList   = null;
            memregion       = MemoryRegion.kInvalidRegion;
            classScope      = ProtoCore.DSASM.Constants.kInvalidIndex;
            functionIndex   = ProtoCore.DSASM.Constants.kGlobalScope;
            absoluteClassScope = ProtoCore.DSASM.Constants.kGlobalScope;
            absoluteFunctionIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            isStatic        = false;
            isTemp          = false;
        }

        public SymbolNode(
            string name,
            int index, 
            int heapIndex, 
            int functionIndex,
            ProtoCore.Type datatype,
            ProtoCore.Type enforcedType,
            int size,
            int datasize, 
            bool isArgument, 
            int runtimeIndex,
            MemoryRegion memregion = MemoryRegion.kInvalidRegion, 
            bool isArray = false, 
            List<int> arraySizeList = null, 
            int scope = -1,
            AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            bool isStatic = false,
            int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            this.name           = name;
            isTemp         = name.StartsWith("%");
            this.index          = index;
            this.functionIndex = functionIndex;
            this.absoluteFunctionIndex = functionIndex;
            this.datatype       = datatype;
            this.staticType   = enforcedType;
            this.size           = size;
            this.datasize       = datasize;
            this.isArgument     = isArgument;
            this.arraySizeList  = arraySizeList;
            this.memregion      = memregion;
            this.classScope     = scope;
            this.absoluteClassScope = scope;
            runtimeTableIndex = runtimeIndex;
            this.access = access;
            this.isStatic = isStatic;
            this.codeBlockId = codeBlockId;
        }

        public SymbolNode(
            string name,
            string forArrayName,
            int index,
            int heapIndex,
            int functionIndex,
            ProtoCore.Type datatype,
            ProtoCore.Type enforcedType,
            int size,
            int datasize,
            bool isArgument,
            int runtimeIndex,
            MemoryRegion memregion = MemoryRegion.kInvalidRegion,
            bool isArray = false,
            List<int> arraySizeList = null,
            int scope = -1,
            AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            bool isStatic = false,
            int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            this.name = name;
            isTemp = name.StartsWith("%");
            this.index = index;
            this.functionIndex = functionIndex;
            this.absoluteFunctionIndex = functionIndex;
            this.datatype = datatype;
            this.staticType = enforcedType;
            this.size = size;
            this.datasize = datasize;
            this.isArgument = isArgument;
            this.arraySizeList = arraySizeList;
            this.memregion = memregion;
            this.classScope = scope;
            this.absoluteClassScope = scope;
            runtimeTableIndex = runtimeIndex;
            this.access = access;
            this.isStatic = isStatic;
            this.codeBlockId = codeBlockId;
            this.forArrayName = forArrayName;
        }

        public bool IsEqual(SymbolNode rhs)
        {
            return functionIndex == rhs.functionIndex && name == rhs.name;
        }

        public bool IsEqualAtScope(SymbolNode rhs)
        {
            return functionIndex == rhs.functionIndex && name == rhs.name && classScope == rhs.classScope && codeBlockId == rhs.codeBlockId;
        }



        public void SetStaticType(ProtoCore.Type newtype)
        {
            if (staticType.Equals(newtype))
            {
                return;
            }

            staticType = newtype;
            if (staticType.UID != (int)PrimitiveType.kTypeVar || staticType.rank != 0)
            {
                datatype = staticType;
            }
        }
    }

    public class SymbolTable
    {
        private int size;
        private SortedList<int, SymbolNode> symbols = new SortedList<int,SymbolNode>();

        public string ScopeName 
        { 
            get; 
            set; 
        }
        
        public int RuntimeIndex 
        { 
            get; 
            private set; 
        }

        public IDictionary<int,SymbolNode> symbolList
        {
            get
            {
                return symbols;
            }
        }

        public SymbolTable(string scopeName, int runtimeIndex)
        {
            size = 0;
            ScopeName = scopeName;
            RuntimeIndex = runtimeIndex;
        }

        public int GetGlobalSize()
        {
            return size;
        }

        // TODO Jun: return failed status
        public int Append( SymbolNode node )
        {
            if (IndexOf(node) != Constants.kInvalidIndex)
                return Constants.kInvalidIndex;

            int symbolTableIndex = symbolList.Count;
            node.symbolTableIndex = symbolTableIndex;
            symbolList[symbolTableIndex] = node;
            if (Constants.kGlobalScope == node.functionIndex)
            {
                size += node.size;
            }

            return symbolTableIndex;
        }

        public bool Remove(SymbolNode node)
        {
            return symbolList.Remove(node.symbolTableIndex);
        }

        public IEnumerable<SymbolNode> GetNodeForName(string name)
        {
            return symbolList.Values.Where(n => string.Equals(n.name, name));
        }

        public int IndexOf(SymbolNode symbol)
        {
            return symbol.symbolTableIndex;
        }

        public int IndexOf(string name)
        {
            var symbol = symbols.Values.FirstOrDefault(x => string.Equals(x.name, name));

            if (null == symbol)
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            else
                return symbol.symbolTableIndex;
        }

        public int IndexOf(string name, int classScope)
        {
            var symbol = symbols.Values.FirstOrDefault(x =>
                                            string.Equals(x.name, name)
                                        && x.classScope == classScope);

            if (null == symbol)
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            else
                return symbol.symbolTableIndex;
        }

        public int IndexOf(string name, int classScope, int functionindex)
        {
            var symbol = symbols.Values.FirstOrDefault(x => 
                                            string.Equals(x.name, name) 
                                        && x.classScope == classScope 
                                        && x.functionIndex == functionindex);

            if (null == symbol)
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            else
                return symbol.symbolTableIndex;
        }

        public int IndexOfClass(string name, int classScope, int functionindex)
        {
            foreach (var symbol in symbols.Values)
            {
                if (symbol.name != name)
                    continue;

                if (symbol.functionIndex == -1)
                {
                    return symbol.symbolTableIndex;
                }
                else if (classScope == symbol.classScope && functionindex == symbol.functionIndex)
                {
                    return symbol.symbolTableIndex;
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }
    }
}

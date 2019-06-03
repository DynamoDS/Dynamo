using System.Collections.Generic;
using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class ClassUtils
    {
        /// <summary>
        /// Returns the list of classes that this can be upcast to
        /// It includes the class itself
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> GetClassUpcastChain(ClassNode cn, RuntimeCore runtimeCore)
        {
            List<int> ret = new List<int>();

            //@TODO: Replace this with an ID
            ret.Add(runtimeCore.DSExecutable.classTable.ClassNodes.IndexOf(cn));

            ClassNode target = cn;
            while (target.Base != Constants.kInvalidIndex)
            {
                ret.Add(target.Base);
                target = runtimeCore.DSExecutable.classTable.ClassNodes[target.Base];
            }

            if (!ret.Contains((int)(PrimitiveType.Var)))
                ret.Add((int)PrimitiveType.Var);


            return ret;
        }

        /// <summary>
        /// Returns the number of upcasts that need to be performed to turn a class into another class in its upcast chain
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static int GetUpcastCountTo(ClassNode from, ClassNode to, RuntimeCore runtimeCore)
        {
            int toID = runtimeCore.DSExecutable.classTable.ClassNodes.IndexOf(to);

            List<int> upcastChain = GetClassUpcastChain(from, runtimeCore);

            if (!upcastChain.Contains(toID))
                return int.MaxValue;

            return upcastChain.IndexOf(toID);


        }

        // classScope is a global context, it tells we are in which class's scope
        // functionScope is telling us which function we are in. 
        // 
        // 1. Try to find if the target is a member function's local variable
        //        classScope != kInvalidIndex && functionScope != kInvalidIndex;
        // 
        // 2. Try to find if the target is a member variable
        //     2.1 In a member functions classScope != kInvalidIndex && functionScope != kInvalidIndex.
        //         Returns member in derived class, or non-private member in base classes
        // 
        //     2.2 In a global functions classScope == kInvalidIndex && functionScope != kInvalidIndex.
        //         Returns public member in derived class, or public member in base classes
        // 
        //     2.3 Otherwise, classScope == kInvalidIndex && functionScope == kInvalidIndex
        //         Return public member in derived class, or public member in base classes 
        public static int GetSymbolIndex(ClassNode classNode, string name, int classScope, int functionScope, int blockId, List<CodeBlock> codeblockList, out bool hasThisSymbol, out ProtoCore.DSASM.AddressType addressType)
        {
            hasThisSymbol = false;
            addressType = ProtoCore.DSASM.AddressType.Invalid;

            if (classNode.Symbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            IEnumerable<SymbolNode> allSymbols = classNode.Symbols.GetNodeForName(name);
            if (allSymbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            int myself = classNode.TypeSystem.classTable.IndexOf(classNode.Name);
            bool isInMemberFunctionContext = (classScope == myself) && (functionScope != ProtoCore.DSASM.Constants.kInvalidIndex);
            bool isInStaticFunction = isInMemberFunctionContext && 
                classNode.ProcTable.Procedures.Count > functionScope &&
                classNode.ProcTable.Procedures[functionScope].IsStatic;

            // Try for member function variables
            var blocks = GetAncestorBlockIdsOfBlock(blockId, codeblockList);
            blocks.Insert(0, blockId);

            Dictionary<int, SymbolNode> symbolOfBlockScope = new Dictionary<int, SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if ((isInMemberFunctionContext) && (memvar.functionIndex == functionScope))
                {
                    symbolOfBlockScope[memvar.codeBlockId] = memvar;
                }
            }
            if (symbolOfBlockScope.Count > 0)
            {
                foreach (var blockid in blocks)
                {
                    if (symbolOfBlockScope.ContainsKey(blockid))
                    {
                        hasThisSymbol = true;
                        addressType = AddressType.VarIndex;
                        return symbolOfBlockScope[blockid].symbolTableIndex;
                    }
                }
            }

            // Try for member variables. 
            var candidates = new List<SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if (memvar.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                {
                    candidates.Add(memvar);
                }
            }
            // Sort candidates descending based on their class scopes so that
            // we can search member variable in reverse order of hierarchy tree.
            candidates.Sort((lhs, rhs) => rhs.classScope.CompareTo(lhs.classScope));
            hasThisSymbol = candidates.Count > 0;

            foreach (var symbol in candidates)
            {
                bool isAccessible = false;
                if (isInMemberFunctionContext)
                {
                    isAccessible = (symbol.classScope == myself) || (symbol.access != ProtoCore.CompilerDefinitions.AccessModifier.Private);
                    if (isInStaticFunction)
                        isAccessible = isAccessible && symbol.isStatic;
                }
                else
                {
                    isAccessible = symbol.access == ProtoCore.CompilerDefinitions.AccessModifier.Public;
                }

                if (isAccessible)
                {
                    addressType = symbol.isStatic ? AddressType.StaticMemVarIndex : AddressType.MemVarIndex;
                    return symbol.symbolTableIndex;
                }
            }

            return Constants.kInvalidIndex;
        }

        public static List<int> GetAncestorBlockIdsOfBlock(int blockId, List<CodeBlock> codeblockList)
        {
            if (blockId >= codeblockList.Count || blockId < 0)
            {
                return new List<int>();
            }
            CodeBlock thisBlock = codeblockList[blockId];

            var ancestors = new List<int>();
            CodeBlock codeBlock = thisBlock.parent;
            while (codeBlock != null)
            {
                ancestors.Add(codeBlock.codeBlockId);
                codeBlock = codeBlock.parent;
            }
            return ancestors;
        }
    }
}

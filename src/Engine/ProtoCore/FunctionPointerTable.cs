using ProtoCore.Utils;
using System;
using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    public class FunctionPointerTable
    {
        public BiDictionaryOneToOne<int, FunctionPointerNode> functionPointerDictionary { get; set; }
        public FunctionPointerTable()
        {
            functionPointerDictionary = new BiDictionaryOneToOne<int, FunctionPointerNode>();
        }

        /// <summary>
        /// Try to get the original procedure node that the function pointer
        /// points to. 
        /// </summary>
        /// <param name="functionPointer">Function pointer</param>
        /// <param name="core">Core</param>
        /// <param name="procNode">Procedure node</param>
        /// <returns></returns>
        public bool TryGetFunction(StackValue functionPointer, RuntimeCore runtimeCore, out ProcedureNode procNode)
        {
            procNode = null;

            int index = functionPointer.FunctionPointer;
            FunctionPointerNode fptrNode;

            if (functionPointerDictionary.TryGetByFirst(index, out fptrNode))
            {
                var blockId = fptrNode.blockId;
                var classScope = fptrNode.classScope;
                var functionIndex = fptrNode.procId;

                if (classScope != Constants.kGlobalScope)
                {
                    procNode = runtimeCore.DSExecutable.classTable.ClassNodes[classScope].ProcTable.Procedures[functionIndex];
                }
                else
                {
                    bool found = runtimeCore.DSExecutable.CompleteCodeBlockDict.TryGetValue(blockId, out CodeBlock codeBlock);
                    Validity.Assert(found, $"Could find code block with codeBlockId {blockId}");

                    procNode = codeBlock.procedureTable.Procedures[functionIndex];
                }

                return true;
            }

            return false;
        }
    }

    public struct FunctionPointerNode
    {
        public int procId;
        public int blockId;
        public int classScope;

        public FunctionPointerNode(ProcedureNode procNode)
        {
            this.procId = procNode.ID;
            this.classScope = procNode.ClassID;
            this.blockId = procNode.RuntimeIndex;
        }
    }

    /// <summary>
    /// This is a dictionary guaranteed to have only one of each value and key. 
    /// It may be searched either by TFirst or by TSecond, giving a unique answer because it is 1 to 1.
    /// </summary>
    /// <typeparam name="TFirst">The type of the "key"</typeparam>
    /// <typeparam name="TSecond">The type of the "value"</typeparam>
    public class BiDictionaryOneToOne<TFirst, TSecond>
    {
        readonly IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        readonly IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Returns false if either element is already in the dictionary        
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>true if successfully added, false if either element are already in the dictionary</returns>
        public Boolean TryAdd(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) || secondToFirst.ContainsKey(second))
                return false;

            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
            return true;
        }


        /// <summary>
        /// Find the TSecond corresponding to the TFirst first.
        /// Returns false if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <param name="second">the corresponding value</param>
        /// <returns>true if first is in the dictionary, false otherwise</returns>
        public Boolean TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        /// <summary>
        /// Find the TFirst corresponding to the TSecond second.
        /// Returns false if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <param name="first">the corresponding value</param>
        /// <returns>true if second is in the dictionary, false otherwise</returns>
        public Boolean TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }

        /// <summary>
        /// The number of pairs stored in the dictionary
        /// </summary>
        public Int32 Count
        {
            get { return firstToSecond.Count; }
        }
    }
}

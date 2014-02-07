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
    }

    public struct FunctionPointerNode
    {
        public int procId;
        public int blockId;
        public int classScope;

        public FunctionPointerNode(ProcedureNode procNode)
        {
            this.procId = procNode.procId;
            this.classScope = procNode.classScope;
            this.blockId = procNode.runtimeIndex;
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

        public bool ContainsSecond(TSecond second)
        {
            return secondToFirst.ContainsKey(second);
        }

        public bool ContainsFirst(TFirst first)
        {
            return firstToSecond.ContainsKey(first);
        }

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
        /// Remove the record containing first, if there is one.
        /// </summary>
        /// <param name="first"></param>
        /// <returns> If first is not in the dictionary, returns false, otherwise true</returns>
        public Boolean TryRemoveByFirst(TFirst first)
        {
            TSecond second;
            if (!firstToSecond.TryGetValue(first, out second))
                return false;

            firstToSecond.Remove(first);
            secondToFirst.Remove(second);
            return true;
        }

        /// <summary>
        /// Remove the record containing second, if there is one.
        /// </summary>
        /// <param name="second"></param>
        /// <returns> If second is not in the dictionary, returns false, otherwise true</returns>
        public Boolean TryRemoveBySecond(TSecond second)
        {
            TFirst first;
            if (!secondToFirst.TryGetValue(second, out first))
                return false;

            secondToFirst.Remove(second);
            firstToSecond.Remove(first);
            return true;
        }

        /// <summary>
        /// The number of pairs stored in the dictionary
        /// </summary>
        public Int32 Count
        {
            get { return firstToSecond.Count; }
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            firstToSecond.Clear();
            secondToFirst.Clear();
        }

    }
}

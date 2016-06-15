using System.Collections;
using System.Collections.Generic;

namespace Dynamo.Engine
{
    /// <summary>
    ///     A linked list of list (each node in linked list is a list), and node
    ///     can be accessed through a key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    internal class LinkedListOfList<TKey, T> : IEnumerable<List<T>>
    {
        private readonly LinkedList<List<T>> list;
        private readonly Dictionary<TKey, LinkedListNode<List<T>>> map;

        public LinkedListOfList()
        {
            map = new Dictionary<TKey, LinkedListNode<List<T>>>();
            list = new LinkedList<List<T>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        internal void AddItem(TKey key, T item)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                listNode = new LinkedListNode<List<T>>(new List<T>());
                list.AddLast(listNode);
                map[key] = listNode;
            }
            listNode.Value.Add(item);
        }

        public void Removes(TKey key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                map.Remove(key);
                list.Remove(listNode);
            }
        }

        public List<T> GetItems(TKey key)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode) || listNode.Value == null)
                return null;

            var ret = new List<T>(listNode.Value);
            return ret;
        }

        public List<TKey> GetKeys()
        {
            return new List<TKey>(map.Keys);
        }
    }
}
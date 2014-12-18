using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Dynamo.Utilities
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrderedSet<T> : IEnumerable<T>
    {
        private readonly OrderedDictionary dictionary = new OrderedDictionary();

        public void Add(T item)
        {
            if (!dictionary.Contains(item))
                dictionary.Add(item, null);
        }

        public bool Contains(T item)
        {
            return dictionary.Contains(item);
        }

        public void Remove(T item)
        {
            dictionary.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dictionary.Keys.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}
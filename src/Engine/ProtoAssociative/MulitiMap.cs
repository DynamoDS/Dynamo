using System.Collections.Generic;
using System.Linq;

namespace ProtoAssociative
{
    namespace Util
    {
        public class MultiMap<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
        {
            public void Add(TKey key, TValue value)
            {
                if (key == null || value == null)
                    return;

                HashSet<TValue> container = null;
                if (!TryGetValue(key, out container))
                {
                    container = new HashSet<TValue>();
                    base.Add(key, container);
                }
                container.Add(value); 
                
            }

            public void Add(KeyValuePair<TKey, TValue> record)
            {
                Add(record.Key, record.Value);
            }

            public void Add(TKey key, IEnumerable<TValue> list)
            {
                if (key == null || list.Count() == 0)
                    return;

                HashSet<TValue> container = null;
                if (!TryGetValue(key, out container)) 
                { 
                    container = new HashSet<TValue>();
                    base.Add(key, container); 
                }
                foreach(TValue value in list)
                {
                    container.Add(value);
                }
            }

            public bool Contains(TKey key, TValue value)
            {
                if (key == null || value == null)
                    return false;
                bool bContains = false;
                HashSet<TValue> values = null;
                if (TryGetValue(key, out values))
                {
                    bContains = values.Contains(value);
                }
                return bContains;
            }

            public bool Contains(KeyValuePair<TKey, TValue> record)
            {
                return Contains(record.Key, record.Value);
            }

            public void Remove(TKey key, TValue value)
            {
                if (key == null || value == null)
                    return;
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

            public void Remove(KeyValuePair<TKey, TValue> record) 
            {
                Remove(record.Key, record.Value);
            }

            public void Merge(MultiMap<TKey, TValue> map)
            {
                if (map == null) 
                    return; 

                foreach (KeyValuePair<TKey, HashSet<TValue>> pair in map) 
                {
                    Add(pair.Key, pair.Value);
                }
            }

            public HashSet<TValue> GetValues(TKey key)
            {
                HashSet<TValue> values = null;
                if(!base.TryGetValue(key, out values))
                {                        
                    values = new HashSet<TValue>();
                }
                return values;
            }

        }
    }
}


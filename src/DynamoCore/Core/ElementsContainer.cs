using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Utilities;

namespace Dynamo.Core
{
    public abstract class ElementsContainer <T>
    {
        internal Dictionary<Guid, List<List<T>>> storedElementIds =
            new Dictionary<Guid, List<List<T>>>();

        public IEnumerable<Guid> Nodes
        {
            get { return storedElementIds.Keys; }
        }

        public void Clear()
        {
            storedElementIds.Clear();
        }

        public List<List<T>> this[Guid node]
        {
            get
            {
                if (!storedElementIds.ContainsKey(node))
                {
                    storedElementIds[node] = new List<List<T>>()
                    {
                        new List<T>()
                    };
                }
                return storedElementIds[node];
            }
        }

        public void DestroyAll()
        {
            foreach (var e in storedElementIds.Values.SelectMany(x => x.SelectMany(y => y)))
            {               
                DestroyElement(e);   
            }
            storedElementIds.Clear();
        }

        public virtual void DestroyElement(T element)
        {
            //Override method needs to be responsible for attempting
            //to destroy its native objects, as well as handling
            //exceptions.
        }

        public bool HasElements(Guid node)
        {
            return storedElementIds.ContainsKey(node);
        }
    }
}

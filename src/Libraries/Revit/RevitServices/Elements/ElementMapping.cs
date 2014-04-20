using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace RevitServices.Elements
{
    public class ElementMappingCache
    {
        //Singleton model for now
        private static ElementMappingCache instance = new ElementMappingCache();

        public static ElementMappingCache GetInstance()
        {
            return instance;
        }

        private ElementMappingCache()
        {
               cache = new Dictionary<Document, Dictionary<ElementId, string>>();
        }

        private Dictionary<Document, Dictionary<ElementId, string>> cache;

        private void Add(Document doc, ElementId id, String uuid)
        {
            lock (cache)
            {
                if (!cache.ContainsKey(doc))
                    cache.Add(doc, new Dictionary<ElementId, string>());

                Dictionary<ElementId, string> lookup = cache[doc];

                lookup[id] = uuid;
            }
            
        }

        private void Delete(Document doc, ElementId id)
        {
            lock (cache)
            {
                if (!cache.ContainsKey(doc))
                {
                    throw new ArgumentException("Doc wasn't registered");
                }

                if (!cache[doc].ContainsKey(id))
                {
                    throw new ArgumentException("Doc+id wasn't registered");
                }

                cache[doc].Remove(id);
            }
            
        }

        private string Get(Document doc, ElementId id)
        {
            if (!cache.ContainsKey(doc))
                return null;

            Dictionary<ElementId, string> lookup = cache[doc];

            string uuid; 
            if (lookup.TryGetValue(id, out uuid))
            {
                return uuid;
            }
            else
            {
                return null;
            }
        }


        public void WatcherMethodForAdd(Document document, IEnumerable<ElementId> added)
        {
            foreach (ElementId id in added)
            {
                Add(document, id, document.GetElement(id).UniqueId);
            }
        }

        public void WatcherMethodForDelete(Document document, IEnumerable<ElementId> deleted)
        {
            foreach (ElementId id in deleted)
            {
                Delete(document, id);
                ElementIDLifecycleManager<int>.GetInstance().NotifyOfRevitDeletion(id.IntegerValue);
            }
            
        }

    }
}

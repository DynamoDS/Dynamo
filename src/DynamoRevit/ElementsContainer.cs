using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using RevitServices.Persistence;

namespace Dynamo.Revit
{
    public class ElementsContainer
    {
        Dictionary<Guid, List<List<ElementId>>> storedElementIds =
            new Dictionary<Guid, List<List<ElementId>>>();

        internal IEnumerable<Guid> Nodes 
        {
            get { return storedElementIds.Keys; }
        }

        internal void Clear()
        {
            storedElementIds.Clear();
        }

        public List<List<ElementId>> this[Guid node]
        {
            get
            {
                if (!storedElementIds.ContainsKey(node))
                {
                    storedElementIds[node] = new List<List<ElementId>>()
                    {
                        new List<ElementId>()
                    };
                }
                return storedElementIds[node];
            }
        }

        public void DestroyAll()
        {
            foreach (var e in storedElementIds.Values.SelectMany(x => x.SelectMany(y => y)))
            {
                try
                {
                    DocumentManager.GetInstance().CurrentUIDocument.Document.Delete(e);
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    //TODO: Flesh out?
                }
            }
            storedElementIds.Clear();
        }

        public bool HasElements(Guid node)
        {
            return storedElementIds.ContainsKey(node);
        }
    }
}

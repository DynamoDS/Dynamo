using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Utilities;

namespace Dynamo.Revit
{
    public class ElementsContainer
    {
        Dictionary<dynRevitTransactionNode, List<List<ElementId>>> storedElementIds =
            new Dictionary<dynRevitTransactionNode,List<List<ElementId>>>();

        internal IEnumerable<dynRevitTransactionNode> Nodes 
        {
            get { return storedElementIds.Keys; }
        }

        internal void Clear()
        {
            storedElementIds.Clear();
        }

        public List<List<ElementId>> this[dynRevitTransactionNode node]
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
                    dynRevitSettings.Doc.Document.Delete(e);
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    //TODO: Flesh out?
                }
            }
            storedElementIds.Clear();
        }

        public bool HasElements(dynRevitTransactionNode node)
        {
            return storedElementIds.ContainsKey(node);
        }
    }
}

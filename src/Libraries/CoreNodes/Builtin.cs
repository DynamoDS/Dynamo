using System.Collections;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Builtin
    {
        // The indexing syntax a["foo"] or b[1] are syntactic sugar for this particular operation.
        
        public static object Lookup(Dictionary dict, string key)
        {
            return dict.ValueAtKey(key);
        }

        public static object Lookup(IList list, int index)
        {
            return List.GetItemAtIndex(list, index);
        }
    }

}

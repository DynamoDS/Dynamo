using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Builtin
    {
        // The indexing syntax a["foo"] or b[1] are syntactic sugar for these methods.
        
        public static object Lookup(Dictionary dictionary, string key)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }
            return dictionary.ValueAtKey(key);
        }

        public static object Lookup(IList list, int index)
        {
            return List.GetItemAtIndex(list, index);
        }
    }
}

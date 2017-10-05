using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;

namespace DesignScript
{
    namespace Builtin
    {
        [IsVisibleInDynamoLibrary(false)]
        public static class Get
        {
            // The indexing syntax a["foo"] or b[1] are syntactic sugar for these methods.

            public static object ValueAtIndex(Dictionary dictionary, string key)
            {
                return dictionary.ValueAtKey(key);
            }

            public static object ValueAtIndex(IList list, int index)
            {
                return DSCore.List.GetItemAtIndex(list, index);
            }
        }
    }
}

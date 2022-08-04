using Autodesk.DesignScript.Runtime;
using Builtin.Properties;
using System;
using System.Collections;

namespace DesignScript
{
    namespace Builtin
    {
        [IsVisibleInDynamoLibrary(false)]
        public static class Get
        {
            /// <summary>
            /// Selects correct overload of ValueAtIndex at runtime based on collection type.
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static object ValueAtIndexDynamic(object collection, object index)
            {
                //make int cast valid
                if (index is long longi)
                {
                    index = Convert.ToInt32(longi);
                }
                switch (collection)
                {
                    case Dictionary dict:
                        return ValueAtIndex(dict,index as string);
                    case IList list:
                      
                        return ValueAtIndex(list, (int)index);
                    case string strList:
                        return ValueAtIndex(strList, (int)index);
                   
                    default:
                        return null;
                }
            }

            // The indexing syntax a["foo"] or b[1] are syntactic sugar for these methods.

            public static object ValueAtIndex(Dictionary dictionary, string key)
            {
                if (dictionary == null)
                {
                    throw new BuiltinNullReferenceException(Resources.NullReferenceExceptionMessage);
                }

                try
                {
                    return dictionary.ValueAtKey(key);
                }
                catch (System.Collections.Generic.KeyNotFoundException e)
                {
                    throw new KeyNotFoundException(e.Message);
                }
            }

            [AllowArrayPromotion(false)]
            public static object ValueAtIndex(IList list, int index)
            {
                while (index < 0)
                {
                    var count = list.Count;
                    if (count == 0) break;
                    index += count;
                }

                try
                {
                    return list[index];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new IndexOutOfRangeException(Resources.IndexOutOfRangeExceptionMessage);
                }
            }

            public static object ValueAtIndexInForLoop(IList list, int index)
            {
                return ValueAtIndex(list, index);
            }

            public static object ValueAtIndexInForLoop(string stringList, int index)
            {
                return ValueAtIndex(stringList, index);
            }

            public static object ValueAtIndex(string stringList, int index)
            {
                if (stringList == null)
                {
                    throw new BuiltinNullReferenceException(Resources.NullReferenceExceptionMessage);
                }
                while (index < 0)
                {
                    var count = stringList.Length;
                    if (count == 0) break;
                    index += count;
                }

                try
                {
                    return stringList[index];
                }
                catch (System.IndexOutOfRangeException)
                {
                    throw new StringOverIndexingException(Resources.StringOverIndexingExceptionMessage);
                }
            }
        }
    }
}

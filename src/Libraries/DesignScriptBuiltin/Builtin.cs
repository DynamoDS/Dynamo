﻿using System;
using System.Collections;
using Autodesk.DesignScript.Runtime;
using Builtin.Properties;

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
                if (dictionary == null)
                {
                    throw new BuiltinNullReferenceException(DesignScriptBuiltin.NullReferenceExceptionMessage);
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
                    throw new IndexOutOfRangeException(DesignScriptBuiltin.IndexOutOfRangeExceptionMessage);
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
                    throw new BuiltinNullReferenceException(DesignScriptBuiltin.NullReferenceExceptionMessage);
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
                    throw new StringOverIndexingException(DesignScriptBuiltin.StringOverIndexingExceptionMessage);
                }
            }
        }
    }
}

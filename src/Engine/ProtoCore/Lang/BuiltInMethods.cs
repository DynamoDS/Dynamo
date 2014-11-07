using System.Linq;
using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    public class BuiltInMethods
    {
        public enum MethodID
        {
            kInvalidMethodID = -1,
            kAllFalse,
            kAllTrue,
            kAverage,
            kConcat,
            kContains,
            kCount,
            kCountTrue,
            kCountFalse,
            kDifference,
            kDot,
            kEquals,
            kGetElapsedTime,
            kGetType,
            kFlatten,
            kImportData,
            kIndexOf,
            kInsert,
            kIntersection,
            kIsUniformDepth,
            kIsRectangular,
            kIsHomogeneous,
            kLoadCSVWithMode,
            kLoadCSV,
            kMap,
            kMapTo,
            kNormalizeDepth,
            kNormalizeDepthWithRank,
            kPrint,
            kPrintIndexable,
            kRank,
            kRemove,
            kRemoveDuplicates,
            kRemoveNulls,
            kRemoveIfNot,
            kReverse,
            kSleep,
            kSomeFalse,
            kSomeNulls,
            kSomeTrue,
            kSort,
            kSortWithMode,
            kSortIndexByValue,
            kSortIndexByValueWithMode,
            kSortPointer,
            kReorder,
            kRangeExpression,
            kSum,
            kToString,
            kTranspose,
            kUnion,
            kInlineConditional,
            kBreak,
            kGetKeys,
            kGetValues,
            kRemoveKey,
            kContainsKey,
            kEvaluate,
            kTryGetValueFromNestedDictionaries,
            kNodeAstFailed
        }

        private static string[] methodNames = new string[]
        {
            "AllFalse",                 // kAllFalse
            "AllTrue",                  // kAllTrue
            "Average",                  // kAverage
            "Concat",                   // kConcat
            "Contains",                 // kContains
            "Count",                    // kCount
            "CountTrue",                // kCountTrue
            "CountFalse",               // kCountFalse
            "SetDifference",            // kDifference
            "%dot",                     // kDot
            "Equals",                   // kEquals
            "GetElapsedTime",           // kGetElapsedTime
            Constants.kGetTypeMethodName,// kGetType
            "Flatten",                  // kFlatten
            "ImportData",               // kImportData
            "IndexOf",                  // kIndexOf
            "Insert",                   // kInsert
            "SetIntersection",          // Intersection
            "IsUniformDepth",           // kIsUniformDepth
            "IsRectangular",            // kIsRectangular
            "IsHomogeneous",            // kIsHomogeneous
            "ImportFromCSV",            // kLoadCSVWithMode
            "ImportFromCSV",            // kLoadCSV
            "Map",                      // kMap
            "MapTo",                    // kMapTo
            "NormalizeDepth",           // kNormalizeDepth
            "NormalizeDepth",           // kNormalizeDepthWithRank
            "Print",                    // kPrint
            "Print",                    // kPrint
            "Rank",                     // kRank
            "Remove",                   // kRemove
            "RemoveDuplicates",         // kRemoveDuplicates
            "RemoveNulls",              // kRemoveNulls
            "RemoveIfNot",              // kRemoveIfNot
            "Reverse",                  // kReverse
            "Sleep",                    // kSleep
            "SomeFalse",                // kSomeFalse
            "SomeNulls",                // kSomeNulls
            "SomeTrue",                 // kSomeTrue
            "Sort",                     // kSort
            "Sort",                     // kSortWithMode
            "SortIndexByValue",         // kSortIndexByValue
            "SortIndexByValue",         // kSortIndexByValueWithMode
            "Sort",                     // kSortPointer
            "Reorder",                  // kReorder
            Constants.kFunctionRangeExpression, // kGenerateRange
            "Sum",                      // kSum
            "ToString",                 // kToString
            "Transpose",                // kTranspose
            "SetUnion",                 // kUnion
            Constants.kInlineConditionalMethodName,
            "Break",                    // kBreak
            "GetKeys",                  // kGetKeys    
            "GetValues",                // kGetValues    
            "RemoveKey",                // kRemoveKey
            "ContainsKey",              // kContainsKey
            "Evaluate",                 // kEvaluateFunctionPointer
            "__TryGetValueFromNestedDictionaries",// kTryGetValueFromNestedDictionaries
            Constants.kNodeAstFailed,   // kNodeAstFailed
        };

        public static string GetMethodName(MethodID id)
        {
            return methodNames[(int)id];
        }

        public class BuiltInMethod
        {
            public MethodID ID { get; set; }
            public List<KeyValuePair<string, Type>> Parameters { get; set; }
            public Type ReturnType { get; set; }
            public MethodAttributes MethodAttributes { get; set; }
        }


        public List<BuiltInMethod> Methods { get; set; }

        public BuiltInMethods(Core core)
        {
            Validity.Assert(null == Methods);
            Methods = new List<BuiltInMethod>()
            {
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kCount
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kSomeNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kRank
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                    new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kFlatten
                },
                
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kConcat,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kIntersection
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kUnion
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1), 
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kDifference
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kCountTrue
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kCountFalse
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kAllFalse,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kAllTrue,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kIsHomogeneous,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt))
                    },
                    ID = BuiltInMethods.MethodID.kSum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble))
                    },
                    ID = BuiltInMethods.MethodID.kSum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt))
                    },
                    ID = BuiltInMethods.MethodID.kAverage,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble))
                    },
                    ID = BuiltInMethods.MethodID.kAverage,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kSomeTrue,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("milliseconds", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSleep,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kSomeFalse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemove,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveDuplicates,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("type", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveIfNot,
                    //MAGN-3382  MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kReverse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kContains
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kContains
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf,
                    MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert
                },

            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                        new KeyValuePair<string, Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("comparerFunction", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortPointer,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValue,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),  
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValueWithMode,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, Type>("indice", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kReorder
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kIsUniformDepth,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kIsRectangular,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepth,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("rank", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepthWithRank,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        },
                    ID = BuiltInMethods.MethodID.kMap
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("targetRangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, Type>("targetRangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kMapTo
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("Array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kTranspose,
                    MethodAttributes = new MethodAttributes(true)
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeArray),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("start", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, Type>("end", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, Type>("step", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, Type>("op", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0)),
                        new KeyValuePair<string, Type>("nostep", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                        new KeyValuePair<string, Type>("hasAmountOp", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRangeExpression
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kLoadCSV
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, Type>("transpose", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kLoadCSVWithMode
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrint,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrintIndexable,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, Type>> { },
                    ID = BuiltInMethods.MethodID.kGetElapsedTime,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("lhsPtr", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kDot
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("condition", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                        new KeyValuePair<string, Type>("dyn1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("dyn2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kInlineConditional
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kGetType
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kGetType
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    },
                    ID = BuiltInMethods.MethodID.kToString
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string,Type>>(),
                    ID = BuiltInMethods.MethodID.kBreak,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("appname", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, Type>("connectionParameters", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = BuiltInMethods.MethodID.kImportData,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = MethodID.kGetKeys,
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = MethodID.kGetValues,
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kRemoveKey,
                    //MethodAttributes = new MethodAttributes(true), MAGN-3382
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kContainsKey,
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("functionPointer", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, Type>("params", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("unpackParams", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0))
                    },
                    ID = MethodID.kEvaluate,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                    Parameters = new List<KeyValuePair<string,Type>>
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar))
                    },
                    ID = MethodID.kTryGetValueFromNestedDictionaries,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new []
                    {
                        new KeyValuePair<string, Type>("nodeType", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0))
                    }.ToList(),
                    ID = MethodID.kNodeAstFailed,
                    MethodAttributes = new MethodAttributes(true),
                }
            };
        }
    }
}

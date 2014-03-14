using System.Collections.Generic;
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
            kDotDynamic,
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
            kEvaluate
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
            "%dotDynamicResolve",       // kDotDynamic
            "Equals",                   // kEquals
            "GetElapsedTime",           // kGetElapsedTime
            "%get_type",                // kGetType
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
            "%generate_range",          // kGenerateRange
            "Sum",                      // kSum
            "ToString",                 // kToString
            "Transpose",                // kTranspose
            "SetUnion",                 // kUnion
            "%inlineconditional",       // kInlineConditional
            "Break",                    // kBreak
            "GetKeys",                  // kGetKeys    
            "GetValues",                // kGetValues    
            "RemoveKey",                // kRemoveKey
            "ContainsKey",              // kContainsKey
            "Evaluate",                 // kEvaluateFunctionPointer
        };

        public static string GetMethodName(MethodID id)
        {
            return methodNames[(int)id];
        }

        public class BuiltInMethod
        {
            public MethodID ID { get; set; }
            public List<KeyValuePair<string, ProtoCore.Type>> Parameters { get; set; }
            public ProtoCore.Type ReturnType { get; set; }
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
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCount
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeNulls
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRank
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                    new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kFlatten
                },
                
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kConcat
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kIntersection
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kUnion
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1), 
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("array2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kDifference
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCountTrue
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCountFalse
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAllFalse
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAllTrue
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kIsHomogeneous
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSum
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSum
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAverage
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAverage
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeTrue
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("milliseconds", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSleep
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeFalse
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemove
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveDuplicates
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveNulls
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("type", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveIfNot
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kReverse
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kContains
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kContains
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert
                },

            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("comparerFunction", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortPointer
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValue
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValueWithMode
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("indice", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kReorder
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kIsUniformDepth
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kIsRectangular
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepth
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("rank", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepthWithRank
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        },
                    ID = BuiltInMethods.MethodID.kMap
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("targetRangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("targetRangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kMapTo
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("Array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kTranspose
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeArray, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("start", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("end", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("step", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("op", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("nostep", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRangeExpression
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kLoadCSV
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("transpose", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kLoadCSVWithMode
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrint
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrintIndexable
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> { },
                    ID = BuiltInMethods.MethodID.kGetElapsedTime
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("lhsPtr", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kDot
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("lhsPtr", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("functionIndex", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kDotDynamic

                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("condition", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("dyn1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("dyn2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kInlineConditional
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kGetType
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kGetType
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kToString
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string,Type>>(),
                    ID = BuiltInMethods.MethodID.kBreak
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("appname", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("connectionParameters", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kImportData
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = MethodID.kGetKeys,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = MethodID.kGetValues,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kRemoveKey,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kContainsKey,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("functionPointer", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, Type>("params", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = MethodID.kEvaluate,
                }
            };
        }
    }
}

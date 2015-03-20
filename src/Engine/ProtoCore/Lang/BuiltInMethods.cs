using System.Collections.Generic;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System.Linq;
using ProtoCore.Properties;

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
            kToStringFromObject,
            kToStringFromArray,
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
            "__ToStringFromObject",     // kToStringFromObject
            "__ToStringFromArray",      // kToStringFromArray
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
            Constants.kNodeAstFailed    // kNodeAstFailed
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
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCount,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsNumberOfItems}
                   

                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRank,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsTheDeepestDepthOfTheList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                    new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kFlatten,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsTheFlattened1DList}
                },
                
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kConcat,
                    MethodAttributes = new MethodAttributes(true, false, Resources.UseListJoinNode){Description  = Resources.ReturnsConcatenatingList}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kIntersection, 
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ProducesTheSetIntersection}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kUnion,
                     MethodAttributes = new MethodAttributes(){Description  = Resources.ProducesTheSetUnion}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1), 
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1))
                    },
                    ID = BuiltInMethods.MethodID.kDifference,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ObjectsContainsInList1NotInList2}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCountTrue,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ReturnsTheNumberOfTrueValue}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kCountFalse,
                     MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsTheNumberOfFalseValueInList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAllFalse,
                     MethodAttributes = new MethodAttributes(){Description  = Resources.ChecksIfTheListIsAllFalse}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAllTrue,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ChecksIfTheListIsAllTrue}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kIsHomogeneous,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.CheckIfTheElementsInListAreSameType}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAverage,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kAverage,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeTrue,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("milliseconds", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSleep,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kSomeFalse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemove,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveDuplicates,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("type", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0))
                    },
                    ID = BuiltInMethods.MethodID.kRemoveIfNot,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.RemovesTheMembersofTheList}
                    //MAGN-3382  MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kReverse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals,
                    MethodAttributes = new MethodAttributes(){Description = Resources.DeterminesObjectsAreEqual}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kEquals,
                    MethodAttributes = new MethodAttributes(){Description = Resources.DeterminesObjectsAreEqual}
                   
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kContains,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ChecksIfListContainsTheElement}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kContains,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ChecksIfListContainsTheElement}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("member", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf,
                    MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kIndexOf,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsTheIndex}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.InsertsAnElementIntoList}

                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0))
                    },
                    ID = BuiltInMethods.MethodID.kInsert,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.InsertsAnElementIntoList}
                },

            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortWithMode,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("comparerFunction", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortPointer,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValue,
                    MethodAttributes = new MethodAttributes(){Description = Resources.SortsListByValue}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),  
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kSortIndexByValueWithMode,
                     MethodAttributes = new MethodAttributes(){Description = Resources.SortsListByValueInAscending}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("indice", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 1)),
                    },
                    ID = BuiltInMethods.MethodID.kReorder,
                     MethodAttributes = new MethodAttributes(){Description = Resources.ReordersList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kIsUniformDepth,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ChecksListWithUniformDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kIsRectangular,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ChecksIfLengthsAreSameInMultiDimentionalList}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepth,
                     MethodAttributes = new MethodAttributes(){Description = Resources.ReturnsListWithUniformDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("rank", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kNormalizeDepthWithRank,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ReturnsListWithRankDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
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
                    ID = BuiltInMethods.MethodID.kMap,
                    MethodAttributes = new MethodAttributes(){Description = Resources.MapsValueIntoInputRange}
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
                    ID = BuiltInMethods.MethodID.kMapTo,
                     MethodAttributes = new MethodAttributes(){Description = Resources.MapsValueFromOneRangeToAnotherRange}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kTranspose,
                    MethodAttributes = new MethodAttributes(true)
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
                        new KeyValuePair<string, ProtoCore.Type>("nostep", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("hasAmountOp", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0))
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
                    ID = BuiltInMethods.MethodID.kLoadCSV,
                     MethodAttributes = new MethodAttributes(){Description = Resources.ImportFileByGivenFilePath}
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("transpose", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kLoadCSVWithMode,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ImportFileByGivenFilePathWithMode}
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrint,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVoid, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),                    
                    },
                    ID = BuiltInMethods.MethodID.kPrintIndexable,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> { },
                    ID = BuiltInMethods.MethodID.kGetElapsedTime,
                    MethodAttributes = new MethodAttributes(true),
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
                    ID = BuiltInMethods.MethodID.kGetType,
                    MethodAttributes = new MethodAttributes(){Description = Resources.Gettypes}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.kGetType,
                    MethodAttributes = new MethodAttributes(){Description = Resources.Gettypes}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0),

                    Parameters = new [] 
                    {
                        new KeyValuePair<string, Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    }.ToList(),
                    ID = BuiltInMethods.MethodID.kToString,
                    MethodAttributes = new MethodAttributes(true, false, "This node is obsolete, please use \"String from Object\""),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0)),
                    },
                    ID = BuiltInMethods.MethodID.kToStringFromObject,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0),

                    Parameters = new [] 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
                    }.ToList(),
                    ID = BuiltInMethods.MethodID.kToStringFromArray
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
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("appname", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("connectionParameters", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.kImportData,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = MethodID.kGetKeys,
                     MethodAttributes = new MethodAttributes(){Description = Resources.GetKeys}
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank))
                    },
                    ID = MethodID.kGetValues,
                     MethodAttributes = new MethodAttributes(){Description = Resources.GetValues}
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kRemoveKey,
                    MethodAttributes = new MethodAttributes(){Description = Resources.RemoveKeys}
                    //MethodAttributes = new MethodAttributes(true), MAGN-3382
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, 0))
                    },
                    ID = MethodID.kContainsKey,
                    MethodAttributes = new MethodAttributes(){Description = Resources.ContainsKeys}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("functionPointer", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, 0)),
                        new KeyValuePair<string, Type>("params", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("unpackParams", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, 0))
                    },
                    ID = MethodID.kEvaluate,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string,Type>>
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar)),
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
using System.Collections.Generic;
using System.Linq;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    public class BuiltInMethods
    {
        public enum MethodID
        {
            InvalidMethodID = -1,
            AllFalse,
            AllTrue,
            Average,
            Concat,
            Contains,
            Count,
            CountTrue,
            CountFalse,
            Difference,
            Dot,
            Equals,
            GetElapsedTime,
            GetType,
            Flatten,
            ImportData,
            IndexOf,
            Insert,
            Intersection,
            IsUniformDepth,
            IsRectangular,
            IsHomogeneous,
            LoadCSV,
            Map,
            MapTo,
            NormalizeDepth,
            Print,
            Rank,
            Remove,
            RemoveDuplicates,
            RemoveNulls,
            RemoveIfNot,
            Reverse,
            Sleep,
            SomeFalse,
            SomeNulls,
            SomeTrue,
            Sort,
            SortIndexByValue,
            SortPointer,
            Reorder,
            RangeExpression,
            Sum,
            ToString,
            ToStringFromObject,
            ToStringFromArray,
            Transpose,
            Union,
            InlineConditional,
            Break,
            GetKeys,
            GetValues,
            RemoveKey,
            ContainsKey,
            Evaluate,
            NodeAstFailed,
            GC,
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
            "__Equals",                   // kEquals
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
            "ImportFromCSV",            // kLoadCSV
            "Map",                      // kMap
            "MapTo",                    // kMapTo
            "NormalizeDepth",           // kNormalizeDepth
            "Print",                    // kPrint
            "__Rank",                     // kRank
            "Remove",                   // kRemove
            "RemoveDuplicates",         // kRemoveDuplicates
            "RemoveNulls",              // kRemoveNulls
            "__RemoveIfNot",              // kRemoveIfNot
            "Reverse",                  // kReverse
            "Sleep",                    // kSleep
            "SomeFalse",                // kSomeFalse
            "SomeNulls",                // kSomeNulls
            "SomeTrue",                 // kSomeTrue
            "Sort",                     // kSort
            "SortIndexByValue",         // kSortIndexByValue
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
            Constants.kNodeAstFailed,   // kNodeAstFailed
            "__GC",                     // kGC
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
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Count,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ReturnsNumberOfItems}
                   

                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.SomeNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Rank,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.ReturnsTheDeepestDepthOfTheList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                    new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Flatten,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ReturnsTheFlattened1DList}
                },
                
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Concat,
                    MethodAttributes = new MethodAttributes(true, false, Resources.UseListJoinNode){Description  = Resources.ReturnsConcatenatingList}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1))
                    },
                    ID = BuiltInMethods.MethodID.Intersection, 
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ProducesTheSetIntersection}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1))
                    },
                    ID = BuiltInMethods.MethodID.Union,
                     MethodAttributes = new MethodAttributes(true){Description  = Resources.ProducesTheSetUnion}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1), 
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1))
                    },
                    ID = BuiltInMethods.MethodID.Difference,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ObjectsContainsInList1NotInList2}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.CountTrue,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.ReturnsTheNumberOfTrueValue}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.CountFalse,
                     MethodAttributes = new MethodAttributes(true){Description  = Resources.ReturnsTheNumberOfFalseValueInList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.AllFalse,
                     MethodAttributes = new MethodAttributes(true){Description  = Resources.ChecksIfTheListIsAllFalse}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.AllTrue,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ChecksIfTheListIsAllTrue}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.IsHomogeneous,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.CheckIfTheElementsInListAreSameType}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Sum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Sum,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Average,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Average,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.SomeTrue,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Void, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("milliseconds", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0)),
                    },
                    ID = BuiltInMethods.MethodID.Sleep,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.SomeFalse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0))
                    },
                    ID = BuiltInMethods.MethodID.Remove,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.RemoveDuplicates,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.RemoveNulls,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("type", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0))
                    },
                    ID = BuiltInMethods.MethodID.RemoveIfNot,
                    MethodAttributes = new MethodAttributes(){Description  = Resources.RemovesTheMembersofTheList}
                    //MAGN-3382  MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Reverse,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("ObjectA", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("ObjectB", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.Equals,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.DeterminesObjectsAreEqual}
                   
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Contains,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ChecksIfListContainsTheElement}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.IndexOf,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.ReturnsTheIndex}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("element", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("index", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0))
                    },
                    ID = BuiltInMethods.MethodID.Insert,
                    MethodAttributes = new MethodAttributes(true){Description  = Resources.InsertsAnElementIntoList}
                },

            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1)),
                    },
                    ID = BuiltInMethods.MethodID.Sort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.Sort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1)),
                    },
                    ID = BuiltInMethods.MethodID.Sort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.Sort,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("comparerFunction", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.FunctionPointer, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                    },
                    ID = BuiltInMethods.MethodID.SortPointer,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1)),
                    },
                    ID = BuiltInMethods.MethodID.SortIndexByValue,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.SortsListByValueInAscending}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),  
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("ascending", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.SortIndexByValue,
                     MethodAttributes = new MethodAttributes(true){Description = Resources.SortsListByValue}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true), 
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                        new KeyValuePair<string, ProtoCore.Type>("indice", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 1)),
                    },
                    ID = BuiltInMethods.MethodID.Reorder,
                     MethodAttributes = new MethodAttributes(true){Description = Resources.ReordersList}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.IsUniformDepth,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.ChecksListWithUniformDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.IsRectangular,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.ChecksIfLengthsAreSameInMultiDimentionalList}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.NormalizeDepth,
                     MethodAttributes = new MethodAttributes(true){Description = Resources.ReturnsListWithUniformDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("rank", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                    },
                    ID = BuiltInMethods.MethodID.NormalizeDepth,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.ReturnsListWithRankDepth}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                }, 

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        },
                    ID = BuiltInMethods.MethodID.Map,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.MapsValueIntoInputRange}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("rangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("rangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("inputValue", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("targetRangeMin", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("targetRangeMax", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, 0)),
                    },
                    ID = BuiltInMethods.MethodID.MapTo,
                     MethodAttributes = new MethodAttributes(true){Description = Resources.MapsValueFromOneRangeToAnotherRange}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.Transpose,
                    MethodAttributes = new MethodAttributes(true)
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Array, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("start", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("end", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("step", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("op", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("nostep", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("hasAmountOp", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0))
                    },
                    ID = BuiltInMethods.MethodID.RangeExpression
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0)),
                    },
                    ID = BuiltInMethods.MethodID.LoadCSV,
                     MethodAttributes = new MethodAttributes(true){Description = Resources.ImportFileByGivenFilePath}
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Double, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("filePath", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("transpose", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                    },
                    ID = BuiltInMethods.MethodID.LoadCSV,
                    MethodAttributes = new MethodAttributes(true){Description = Resources.ImportFileByGivenFilePathWithMode}
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Void, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("msg", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),                    
                    },
                    ID = BuiltInMethods.MethodID.Print,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod()
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Integer, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> { },
                    ID = BuiltInMethods.MethodID.GetElapsedTime,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("lhsPtr", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.Dot
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("condition", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("dyn1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("dyn2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0))
                    },
                    ID = BuiltInMethods.MethodID.InlineConditional
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                    },
                    ID = BuiltInMethods.MethodID.GetType,
                    MethodAttributes = new MethodAttributes(){Description = Resources.Gettypes}
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0),

                    Parameters = new [] 
                    {
                        new KeyValuePair<string, Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var)),
                    }.ToList(),
                    ID = BuiltInMethods.MethodID.ToString,
                    MethodAttributes = new MethodAttributes(true, false, "This node is obsolete, please use \"String from Object\""),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                    {
                        new KeyValuePair<string, ProtoCore.Type>("object", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0)),
                    },
                    ID = BuiltInMethods.MethodID.ToStringFromObject,
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0),

                    Parameters = new [] 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var)),
                    }.ToList(),
                    ID = BuiltInMethods.MethodID.ToStringFromArray
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Void, 0),
                    Parameters = new List<KeyValuePair<string,Type>>(),
                    ID = BuiltInMethods.MethodID.Break,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                    {
                        new KeyValuePair<string, ProtoCore.Type>("appname", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0)),
                        new KeyValuePair<string, ProtoCore.Type>("connectionParameters", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.ImportData,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = MethodID.GetKeys,
                     MethodAttributes = new MethodAttributes(hiddenInLibrary: true){Description = Resources.GetKeys}
                        
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = MethodID.GetValues,
                     MethodAttributes = new MethodAttributes(hiddenInLibrary: true){Description = Resources.GetValues}
                    //MAGN_3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0))
                    },
                    ID = MethodID.RemoveKey,
                    MethodAttributes = new MethodAttributes(hiddenInLibrary: true){Description = Resources.RemoveKeys}
                    //MethodAttributes = new MethodAttributes(true), MAGN-3382
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0),
                    Parameters = new List<KeyValuePair<string, Type>> 
                    {
                        new KeyValuePair<string, Type>("list", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0))
                    },
                    ID = MethodID.ContainsKey,
                    MethodAttributes = new MethodAttributes(hiddenInLibrary: true){Description = Resources.ContainsKeys}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("functionPointer", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.FunctionPointer, 0)),
                        new KeyValuePair<string, Type>("params", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, Type>("unpackParams", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Bool, 0))
                    },
                    ID = MethodID.Evaluate,
                    MethodAttributes = new MethodAttributes(true),
                },

                new BuiltInMethod
                {
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Void, 0),
                    Parameters = new []
                    {
                        new KeyValuePair<string, Type>("nodeType", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.String, 0))
                    }.ToList(),
                    ID = MethodID.NodeAstFailed,
                    MethodAttributes = new MethodAttributes(true),
                 },

                 new BuiltInMethod
                 {
                     ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Void, 0),
                     Parameters = new List<KeyValuePair<string,Type>>(),
                     ID = MethodID.GC,
                     MethodAttributes  = new MethodAttributes(true),
                 }
            };
        }
    }
}
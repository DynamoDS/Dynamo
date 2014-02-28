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
            Methods = new List<BuiltInMethod>();
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kCount
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kSomeNulls
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kRank
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kFlatten
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kConcat
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.MethodID.kIntersection
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.MethodID.kUnion
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.MethodID.kDifference
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kCountTrue
            });
           
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kCountFalse
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kAllFalse
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kAllTrue
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kIsHomogeneous
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kSum
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kSum
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kAverage
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kAverage
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kSomeTrue
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("milliseconds", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kSleep
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kSomeFalse
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kRemove
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kRemoveDuplicates
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kRemoveNulls
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("type", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kRemoveIfNot
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kReverse
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("ObjectA", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("ObjectB", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kEquals
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("ObjectA", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("ObjectB", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.MethodID.kEquals
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kContains
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kContains
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kIndexOf
            }); 
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kIndexOf
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("element", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false , rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kInsert
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("element", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true , rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.MethodID.kInsert
            });
            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.MethodID.kSort
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kSortWithMode
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 1,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.MethodID.kSort
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 1,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kSortWithMode
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("comparerFunction", new ProtoCore.Type{ UID = (int)PrimitiveType.kTypeFunctionPointer, Name = "function", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.MethodID.kSortPointer
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.MethodID.kSortIndexByValue
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kSortIndexByValueWithMode
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("indice", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.MethodID.kReorder
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.MethodID.kIsUniformDepth
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.MethodID.kIsRectangular
            }); 
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.MethodID.kNormalizeDepth
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("rank", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "var", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kNormalizeDepthWithRank
            }); 
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("rangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("rangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("inputValue", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    },
                ID = BuiltInMethods.MethodID.kMap
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("rangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("rangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("inputValue", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("targetRangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("targetRangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kMapTo
            });
            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("Array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.MethodID.kTranspose
            });
            Methods.Add(new BuiltInMethod
                            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeArray,
                    rank = 0,
                    IsIndexable = true,
                    Name = ProtoCore.DSDefinitions.Keyword.Array,
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("start", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("end", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("step", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("op", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("nostep", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kRangeExpression
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("filePath", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kLoadCSV
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("filePath", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("transpose", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.MethodID.kLoadCSVWithMode
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("msg", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),                    
                },
                ID = BuiltInMethods.MethodID.kPrint
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("msg", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),                    
                },
                ID = BuiltInMethods.MethodID.kPrintIndexable
            });
            Methods.Add(new BuiltInMethod()
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                { },
                ID = BuiltInMethods.MethodID.kGetElapsedTime
            });

            // the %dot function
            {
                Methods.Add(new BuiltInMethod
                {
                    ReturnType = new Type
                    {
                        UID = (int)PrimitiveType.kTypeVar,
                        rank = DSASM.Constants.kArbitraryRank,
                        IsIndexable = true,
                        Name = "var",
                    },
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("lhsPtr", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                        ID = BuiltInMethods.MethodID.kDot
                    });
            }

            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
            {
                new KeyValuePair<string, ProtoCore.Type>("lhsPtr", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                new KeyValuePair<string, ProtoCore.Type>("functionIndex", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
            },
                ID = BuiltInMethods.MethodID.kDotDynamic
            });


            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("condition", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("dyn1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("dyn2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.MethodID.kInlineConditional
            });


            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0}),
                },
                ID = BuiltInMethods.MethodID.kGetType
            });

            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank}),
                },
                ID = BuiltInMethods.MethodID.kGetType
            });

            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeString,
                    rank = 0,
                    IsIndexable = false,
                    Name = "string"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank}),
                },
                ID = BuiltInMethods.MethodID.kToString
            });

            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string,Type>>(),
                ID = BuiltInMethods.MethodID.kBreak
            });

            Methods.Add(new BuiltInMethod
            {
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kUndefinedRank,
                    IsIndexable = false,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("appname", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0}),
                    new KeyValuePair<string, ProtoCore.Type>("connectionParameters", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank})
                },
                ID = BuiltInMethods.MethodID.kImportData
            });

            Methods.Add(new BuiltInMethod
            {
                ID = MethodID.kGetKeys,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kUndefinedRank),
                Parameters = new List<KeyValuePair<string, Type>> 
                {
                    new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank))
                }
            });

            Methods.Add(new BuiltInMethod
            {
                ID = MethodID.kGetValues,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kUndefinedRank),
                Parameters = new List<KeyValuePair<string, Type>> 
                {
                    new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank))
                }
            });

            Methods.Add(new BuiltInMethod
            {
                ID = MethodID.kRemoveKey,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, false, 0),
                Parameters = new List<KeyValuePair<string, Type>> 
                {
                    new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank)),
                    new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, Constants.kUndefinedRank))
                }
            });

            Methods.Add(new BuiltInMethod
            {
                ID = MethodID.kContainsKey,
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, false, 0),
                Parameters = new List<KeyValuePair<string, Type>> 
                {
                    new KeyValuePair<string, Type>("array", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank)),
                    new KeyValuePair<string, Type>("key", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, Constants.kUndefinedRank))
                }
            });

            Methods.Add(new BuiltInMethod
                {
                    ID = MethodID.kEvaluate,
                    ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kUndefinedRank),
                    Parameters = new List<KeyValuePair<string, Type>>
                    {
                        new KeyValuePair<string, Type>("functionPointer", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, false, Constants.kUndefinedRank)),
                        new KeyValuePair<string, Type>("params", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, true, Constants.kArbitraryRank))
                    }
                });
        }
    }
}

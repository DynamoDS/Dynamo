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
            InvalidMethodID = -1,
            Concat,
            Count,
            Dot,
            Equals,
            GetType,
            ImportData,
            Rank,
            RemoveIfNot,
            Sleep,
            RangeExpression,
            Transpose,
            InlineConditional,
            Break,
            Evaluate,
            NodeAstFailed,
            GC,
        }

        private static string[] methodNames = new string[]
        {
            "Concat",                   // kConcat
            "Count",                    // kCount
            "%dot",                     // kDot
            "__Equals",                   // kEquals
            Constants.kGetTypeMethodName,// kGetType
            "ImportData",               // kImportData
            "__Rank",                     // kRank
            "__RemoveIfNot",              // kRemoveIfNot
            "Sleep",                    // kSleep
            Constants.kFunctionRangeExpression, // kGenerateRange
            "Transpose",                // kTranspose
            Constants.kInlineConditionalMethodName,
            "Break",                    // kBreak
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
                        new KeyValuePair<string, ProtoCore.Type>("list1", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank)),
                        new KeyValuePair<string, ProtoCore.Type>("list2", TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, Constants.kArbitraryRank))
                    },
                    ID = BuiltInMethods.MethodID.Concat,
                    MethodAttributes = new MethodAttributes(true, false, Resources.UseListJoinNode){Description  = Resources.ReturnsConcatenatingList}
                    //MAGN-3382 MethodAttributes = new MethodAttributes(true),
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
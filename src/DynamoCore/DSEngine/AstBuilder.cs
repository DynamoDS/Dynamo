using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Nodes;
using Microsoft.FSharp.Collections;
using ProtoCore;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Exceptions;
using ProtoCore.Lang;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoScript.Runners;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// If a Dynamo node doesn't override CompileToAstNodeInternal(), we 
    /// register an instance of NodeEvaluator in DesignScript's context so that 
    /// when the Dynamo node is evaluated, NodeEvaluator.Evaluate() will be 
    /// inovked.  
    ///
    /// This class won't be needed if we completely repalce FScheme evaluation 
    /// engine with DesignScript engine.
    /// </summary>
    public class NodeEvaluator
    {
        private NodeWithOneOutput node;

        public NodeEvaluator(NodeModel dynNode)
        {
            Debug.Assert(dynNode is NodeWithOneOutput);
            node = dynNode as NodeWithOneOutput;
        }

        public object Evaluate(List<object> objs)
        {
            // TODO: we need to do marhsalling and unmarhslling here.
            //
            // The output should be converted to FScheme.Value and will be 
            // marshalled or unmarshalled (depend on it is an input of custom
            // node, which only accepts FScheme.Value typed input, and an input
            // of normal node, which has been converted to DesignScript
            // function) by DesignScript FFI. 
            //
            // Let's assume that this list only contains numbers for testing.
            // And assume that returns a double value. 
            List<FScheme.Value> args = objs.Select(obj => FScheme.Value.NewNumber(Convert.ToDouble(obj))).ToList();
            FSharpList<FScheme.Value> fargs = Utils.SequenceToFSharpList<FScheme.Value>(args);
            FScheme.Value result = node.Evaluate(fargs);

            if (result.IsNumber)
            {
                return (result as FScheme.Value.Number).Item;
            }
            else if (result.IsContainer)
            {
                return (result as FScheme.Value.Container).Item;
            }

            return result;
        }
    }

    /// <summary>
    /// A linked list of list (each node in linked list is a list), and node 
    /// can be accessed through a key. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class LinkedListOfList<Key, T> : IEnumerable<List<T>>
    {
        private Dictionary<Key, LinkedListNode<List<T>>> map;
        private LinkedList<List<T>> list;

        public LinkedListOfList()
        {
            map = new Dictionary<Key, LinkedListNode<List<T>>>();
            list = new LinkedList<List<T>>();
        }

        public void AddItem(Key key, T item)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                listNode = new LinkedListNode<List<T>>(new List<T>());
                list.AddLast(listNode);
                map[key] = listNode;
            }
            listNode.Value.Add(item);
        }

        public bool Contains(Key key)
        {
            return map.ContainsKey(key);
        }

        public void Clears(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                listNode.Value.Clear();
            }
        }

        public void Removes(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (map.TryGetValue(key, out listNode))
            {
                map.Remove(key);
                list.Remove(listNode);
            }
        }

        public List<T> GetItems(Key key)
        {
            LinkedListNode<List<T>> listNode;
            if (!map.TryGetValue(key, out listNode))
            {
                return null;
            }

            return listNode.Value;
        }

        public List<Key> GetKeys()
        {
            return new List<Key>(map.Keys);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator<List<T>> IEnumerable<List<T>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }

    public interface IAstBuilder
    {
        AssociativeNode Build(DSFunction node, List<AssociativeNode> inputs);
        AssociativeNode Build(NodeModel node, List<AssociativeNode> inputs);
        AssociativeNode Build(Identity node, List<AssociativeNode> inputs);
        AssociativeNode Build(Reverse node, List<AssociativeNode> inputs);
        AssociativeNode Build(NewList node, List<AssociativeNode> inputs);
        AssociativeNode Build(Sort node, List<AssociativeNode> inputs);
        AssociativeNode Build(SortWith node, List<AssociativeNode> inputs);
        AssociativeNode Build(NumberRange node, List<AssociativeNode> inputs);
        AssociativeNode Build(List node, List<AssociativeNode> inputs);
        AssociativeNode Build(TakeList node, List<AssociativeNode> inputs);
        AssociativeNode Build(GetFromList node, List<AssociativeNode> inputs);
        AssociativeNode Build(RemoveFromList node, List<AssociativeNode> inputs);
        AssociativeNode Build(Empty node, List<AssociativeNode> inputs);
        AssociativeNode Build(IsEmpty node, List<AssociativeNode> inputs);
        AssociativeNode Build(Length node, List<AssociativeNode> inputs);
        AssociativeNode Build(Append node, List<AssociativeNode> inputs);
        AssociativeNode Build(First node, List<AssociativeNode> inputs);
        AssociativeNode Build(Transpose node, List<AssociativeNode> inputs);
        AssociativeNode Build(FlattenList node, List<AssociativeNode> inputs);
        AssociativeNode Build(LessThan node, List<AssociativeNode> inputs);
        AssociativeNode Build(LessThanEquals node, List<AssociativeNode> inputs);
        AssociativeNode Build(GreaterThan node, List<AssociativeNode> inputs);
        AssociativeNode Build(GreaterThanEquals node, List<AssociativeNode> inputs);
        AssociativeNode Build(Equal node, List<AssociativeNode> inputs);
        AssociativeNode Build(And node, List<AssociativeNode> inputs);
        AssociativeNode Build(Or node, List<AssociativeNode> inputs);
        AssociativeNode Build(Xor node, List<AssociativeNode> inputs);
        AssociativeNode Build(Not node, List<AssociativeNode> inputs);
        AssociativeNode Build(Addition node, List<AssociativeNode> inputs);
        AssociativeNode Build(Subtraction node, List<AssociativeNode> inputs);
        AssociativeNode Build(Multiplication node, List<AssociativeNode> inputs);
        AssociativeNode Build(Division node, List<AssociativeNode> inputs);
        AssociativeNode Build(Modulo node, List<AssociativeNode> inputs);
        AssociativeNode Build(Pow node, List<AssociativeNode> inputs);
        AssociativeNode Build(Round node, List<AssociativeNode> inputs);
        AssociativeNode Build(Floor node, List<AssociativeNode> inputs);
        AssociativeNode Build(Ceiling node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.Random node, List<AssociativeNode> inputs);
        AssociativeNode Build(Pi node, List<AssociativeNode> inputs);
        AssociativeNode Build(PiTimes2 node, List<AssociativeNode> inputs);
        AssociativeNode Build(EConstant node, List<AssociativeNode> inputs);
        AssociativeNode Build(Sin node, List<AssociativeNode> inputs);
        AssociativeNode Build(Cos node, List<AssociativeNode> inputs);
        AssociativeNode Build(Tan node, List<AssociativeNode> inputs);
        AssociativeNode Build(Average node, List<AssociativeNode> inputs);
        AssociativeNode Build(Conditional node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs);
        AssociativeNode Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs);
        AssociativeNode Build(DoubleInput node, List<AssociativeNode> inputs);
        AssociativeNode Build(StringFilename node, List<AssociativeNode> inputs);
        AssociativeNode Build(ConcatStrings node, List<AssociativeNode> inputs);
        AssociativeNode Build(ToString node, List<AssociativeNode> inputs);
        AssociativeNode Build(Num2String node, List<AssociativeNode> inputs);
        AssociativeNode Build(JoinStrings node, List<AssociativeNode> inputs);
        AssociativeNode Build(ApplyList node, List<AssociativeNode> inputs);

        // The following methods not implemented yet
        AssociativeNode Build(SortBy node, List<AssociativeNode> inputs);
        AssociativeNode Build(SplitString node, List<AssociativeNode> inputs);
        AssociativeNode Build(String2Num node, List<AssociativeNode> inputs);
        AssociativeNode Build(StringCase node, List<AssociativeNode> inputs);
        AssociativeNode Build(StringLen node, List<AssociativeNode> inputs);
        AssociativeNode Build(Substring node, List<AssociativeNode> inputs);
        AssociativeNode Build(Sublists node, List<AssociativeNode> inputs);
        AssociativeNode Build(DropList node, List<AssociativeNode> inputs);
        AssociativeNode Build(TakeEveryNth node, List<AssociativeNode> inputs);
        AssociativeNode Build(Filter node, List<AssociativeNode> inputs);
        AssociativeNode Build(FilterOut node, List<AssociativeNode> inputs);
        AssociativeNode Build(FlattenListAmt node, List<AssociativeNode> inputs);
        AssociativeNode Build(ListMax node, List<AssociativeNode> inputs);
        AssociativeNode Build(ListMin node, List<AssociativeNode> inputs);
        AssociativeNode Build(NumberSeq node, List<AssociativeNode> inputs);
        AssociativeNode Build(ShiftList node, List<AssociativeNode> inputs);
        AssociativeNode Build(Combine node, List<AssociativeNode> inputs);
        AssociativeNode Build(RemoveEveryNth node, List<AssociativeNode> inputs);
        AssociativeNode Build(AndMap node, List<AssociativeNode> inputs);
        AssociativeNode Build(Apply1 node, List<AssociativeNode> inputs);
        AssociativeNode Build(ComposeFunctions node, List<AssociativeNode> inputs);
        AssociativeNode Build(DeCons node, List<AssociativeNode> inputs);
        AssociativeNode Build(DiagonalLeftList node, List<AssociativeNode> inputs);
        AssociativeNode Build(DiagonalRightList node, List<AssociativeNode> inputs);
        AssociativeNode Build(Fold node, List<AssociativeNode> inputs);
        AssociativeNode Build(ForEach node, List<AssociativeNode> inputs);
        AssociativeNode Build(Map node, List<AssociativeNode> inputs);
        AssociativeNode Build(OrMap node, List<AssociativeNode> inputs);
        AssociativeNode Build(RandomSeed node, List<AssociativeNode> inputs);
        AssociativeNode Build(LacerBase node, List<AssociativeNode> inputs);
        AssociativeNode Build(NewtonRootFind1DNoDeriv node, List<AssociativeNode> inputs);
        AssociativeNode Build(NewtonRootFind1DWithDeriv node, List<AssociativeNode> inputs);
        AssociativeNode Build(Output node, List<AssociativeNode> inputs);
        AssociativeNode Build(Repeat node, List<AssociativeNode> inputs);
        AssociativeNode Build(Rest node, List<AssociativeNode> inputs);
        AssociativeNode Build(Smooth node, List<AssociativeNode> inputs);
        AssociativeNode Build(Begin node, List<AssociativeNode> inputs);
        AssociativeNode Build(CartProd node, List<AssociativeNode> inputs);
        AssociativeNode Build(Breakpoint node, List<AssociativeNode> inputs);
        AssociativeNode Build(ExecuteInterval node, List<AssociativeNode> inputs);
        AssociativeNode Build(ListToCsv node, List<AssociativeNode> inputs);
        AssociativeNode Build(Pause node, List<AssociativeNode> inputs);
        AssociativeNode Build(Watch node, List<AssociativeNode> inputs);

        //AssociativeNode Build(RenderDescription node, List<AssociativeNode> inputs);
        AssociativeNode Build(VariableInput node, List<AssociativeNode> inputs);
        AssociativeNode Build(Symbol node, List<AssociativeNode> inputs);
        AssociativeNode Build(Formula node, List<AssociativeNode> inputs);
        AssociativeNode Build(Function node, List<AssociativeNode> inputs);

        AssociativeNode Build(Domain node, List<AssociativeNode> inputs);
        /*
        AssociativeNode Build(Color node, List<AssociativeNode> inputs);
        AssociativeNode Build(ColorBrightness node, List<AssociativeNode> inputs);
        AssociativeNode Build(ColorComponents node, List<AssociativeNode> inputs);
        AssociativeNode Build(ColorHue node, List<AssociativeNode> inputs);
        AssociativeNode Build(ColorRange node, List<AssociativeNode> inputs);
        AssociativeNode Build(ColorSaturation node, List<AssociativeNode> inputs);
        */

        AssociativeNode Build(FileReader node, List<AssociativeNode> inputs);
        AssociativeNode Build(FileWriter node, List<AssociativeNode> inputs);
        AssociativeNode Build(ImageFileReader node, List<AssociativeNode> inputs);
        AssociativeNode Build(ImageFileWriter node, List<AssociativeNode> inputs);
        AssociativeNode Build(FileWatcher node, List<AssociativeNode> inputs);
        AssociativeNode Build(FileWatcherChanged node, List<AssociativeNode> inputs);
        AssociativeNode Build(FileWatcherReset node, List<AssociativeNode> inputs);
        AssociativeNode Build(FileWatcherWait node, List<AssociativeNode> inputs);

        AssociativeNode Build(Future node, List<AssociativeNode> inputs);
        AssociativeNode Build(Thunk node, List<AssociativeNode> inputs);
        AssociativeNode Build(Now node, List<AssociativeNode> inputs);
        AssociativeNode Build(UdpListener node, List<AssociativeNode> inputs);
        AssociativeNode Build(WebRequest node, List<AssociativeNode> inputs);

        AssociativeNode Build(DropDrownBase  node, List<AssociativeNode> inputs);
    }

    /// <summary>
    /// AstBuilder implements AST generation logic for each kind of Dynamo node.
    /// 
    /// Internally it keeps a mapping between Dynamo node and the corresponding
    /// DesignScript AST nodes. 
    /// </summary>
    public class AstBuilder : IAstBuilder
    {
        public Dictionary<string, object> evalContext;
        public static AstBuilder Instance = new AstBuilder();
        private LinkedListOfList<Guid, AssociativeNode> astNodes;

        internal class StringConstants
        {
            public const string kEvalFunction = @"Evaluate";
            public const string kEvalFunctionPrefix = @"eval_";
            public const string kParamPrefix = @"p_";
            public const string kFunctionPrefix = @"func_";
            public const string kVarPrefix = @"var_";
        }

        internal class ExprConstants
        {
            public static readonly AssociativeNode PI = AstBuilder.BuildDoubleNode(Math.PI);
            public static readonly AssociativeNode TwoPI = AstBuilder.BuildDoubleNode(Math.PI * 2);
            public static readonly AssociativeNode E = AstBuilder.BuildDoubleNode(Math.E);
            public static readonly AssociativeNode EmptyList = AstBuilder.BuildExprList(new List<AssociativeNode> { });
        }

        internal class NamingUtil
        {
            public static string NewUniqueName(string prefix = null,
                                               string postfix = null)
            {
                return prefix +
                       Guid.NewGuid().ToString().Replace("-", string.Empty) +
                       postfix;
            }
        }

        private AstBuilder()
        {
            evalContext = new Dictionary<string, object>();
            astNodes = new LinkedListOfList<Guid, AssociativeNode>();
        }

        public void AddNode(Guid dynamoNodeId, AssociativeNode astNode)
        {
            astNodes.AddItem(dynamoNodeId, astNode);
        }

        /// <summary>
        /// If AstBuilder has generated AST node for this Dynamo node
        /// </summary>
        /// <param name="dynamoNodeId"></param>
        /// <returns></returns>
        public bool ContainsAstNodes(Guid dynamoNodeId)
        {
            return astNodes.Contains(dynamoNodeId);
        }

        public void RemoveAstNodes(Guid dynamoNodeId)
        {
            astNodes.Removes(dynamoNodeId); 
        }

        public void ClearAstNodes(Guid dynamoNodeId)
        {
            astNodes.Clears(dynamoNodeId);
        }

#if DEBUG
        /// <summary>
        /// Dump DesignScript code from AST nodes, just for testing
        /// </summary>
        /// <returns></returns>
        public string DumpCode()
        {
            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in astNodes)
            {
                allAstNodes.AddRange(item);
            }
            ProtoCore.CodeGenDS codegen = new ProtoCore.CodeGenDS(allAstNodes);
            return codegen.GenerateCode();
        }

        /// <summary>
        /// Execute AST nodes, just for testing. 
        /// </summary>
        public void Execute()
        {
            ProtoScriptTestRunner runner = new ProtoScriptTestRunner();
            ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Options.Verbose = true;
            core.RuntimeStatus.MessageHandler = new ConsoleOutputStream();
            DLLFFIHandler.Register(FFILanguage.CPlusPlus, new ProtoFFI.PInvokeModuleHelper());
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            CLRModuleType.ClearTypes();

            List<AssociativeNode> allAstNodes = new List<AssociativeNode>();
            foreach (var item in astNodes)
            {
                allAstNodes.AddRange(item);
            }

            DynamoLogger logger = DynamoLogger.Instance;
            try
            {
                ExecutionMirror mirror = runner.Execute(allAstNodes, core);
                List<Guid> keys = astNodes.GetKeys();
                foreach (var guid in keys)
                {
                    string varname = StringConstants.kVarPrefix + guid.ToString().Replace("-", string.Empty);
                    Obj o = mirror.GetValue(varname);
                    string value = mirror.GetStringValue(o.DsasmValue, core.Heap, 0, true);
                    logger.Log(varname + "=" + value);
                }
            }
            catch (CompileErrorsOccured e)
            {
                logger.Log(e.Message);
            }
        }
#endif

        #region IAstBuilder interface
        public AssociativeNode Build(DSFunction node, List<AssociativeNode> inputs)
        {
            string function = node.FunctionName;
            return BuildFunctionCall(function, inputs);
        }

        public AssociativeNode Build(NodeModel node, List<AssociativeNode> inputs)
        {
            return BuildNullNode();
        }

        public AssociativeNode Build(Identity node, List<AssociativeNode> inputs)
        {
            return inputs.Count > 0 ? inputs[0] : BuildNullNode();
        }

        public AssociativeNode Build(Reverse node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Reverse", inputs);
        }

        public AssociativeNode Build(NewList node, List<AssociativeNode> inputs)
        {
            return BuildExprList(inputs);
        }

        public AssociativeNode Build(SortWith node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Sort", inputs);
        }

        public AssociativeNode Build(SortBy node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Sort node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Sort", inputs);
        }

        public AssociativeNode Build(NumberRange node, List<AssociativeNode> inputs)
        {
            RangeExprNode range = new RangeExprNode();
            range.FromNode = inputs[0];
            range.ToNode = inputs[1];
            range.StepNode = inputs[2];
            range.stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize;
            return range;
        }

        public AssociativeNode Build(List node, List<AssociativeNode> inputs)
        {
            List<AssociativeNode> arguments = new List<AssociativeNode>
            {
                inputs[1],
                inputs[0],
                BuildIntNode(0)
            };
            return BuildFunctionCall("Insert", arguments);
        }

        public AssociativeNode Build(TakeList node, List<AssociativeNode> inputs)
        {
            // return list[x..(y - 1)]
            var listExpr = inputs[1] as ArrayNameNode;
            if (listExpr == null)
            {
                return BuildNullNode();
            }

            var const1 = BuildIntNode(1);
            var toExpr = BuildBinaryExpression(inputs[0], const1, Operator.sub);
            RangeExprNode range = new RangeExprNode();
            range.FromNode = BuildIntNode(0);
            range.ToNode = toExpr;
            range.StepNode = const1;
            range.stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize;

            listExpr.ArrayDimensions.Expr = range;
            return listExpr;
        }

        public AssociativeNode Build(GetFromList node, List<AssociativeNode> inputs)
        {
            var listExpr = inputs[1] as ArrayNameNode;
            Debug.Assert(listExpr != null);
            if (listExpr == null)
            {
                return BuildNullNode();
            }

            listExpr.ArrayDimensions.Expr = inputs[0];
            return listExpr;
        }

        public AssociativeNode Build(RemoveFromList node, List<AssociativeNode> inputs)
        {
            inputs.Reverse();
            return BuildFunctionCall("Remove", inputs);
        }

        public AssociativeNode Build(Empty ndoe, List<AssociativeNode> inputs)
        {
            throw new NotSupportedException();
        }

        public AssociativeNode Build(IsEmpty node, List<AssociativeNode> inputs)
        {
            var lhs = BuildFunctionCall("Count", inputs);
            var rhs = BuildIntNode(0);
            return BuildBinaryExpression(lhs, rhs, Operator.eq);
        }

        public AssociativeNode Build(Length node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Count", inputs);
        }

        public AssociativeNode Build(Append node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Concat", inputs);
        }

        public AssociativeNode Build(First node, List<AssociativeNode> inputs)
        {
            var listExpr = inputs[0] as ArrayNameNode;
            Debug.Assert(listExpr != null);
            if (listExpr == null)
            {
                return BuildNullNode();
            }
            listExpr.ArrayDimensions.Expr = BuildIntNode(0);
            return listExpr;
        }

        public AssociativeNode Build(Transpose node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Transpose", inputs);
        }

        public AssociativeNode Build(FlattenList node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Flatten", inputs);
        }

        public AssociativeNode Build(LessThan node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.lt), inputs);
        }

        public AssociativeNode Build(LessThanEquals node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.le), inputs);
        }

        public AssociativeNode Build(GreaterThan node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.gt), inputs);
        }

        public AssociativeNode Build(GreaterThanEquals node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.ge), inputs);
        }

        public AssociativeNode Build(Equal node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.eq), inputs);
        }

        public AssociativeNode Build(And node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.and), inputs);
        }

        public AssociativeNode Build(Or node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.or), inputs);
        }

        public AssociativeNode Build(Xor node, List<AssociativeNode> inputs)
        {
            // p xor q = (p or q) and not (p and q)
            var expr1 = BuildFunctionCall(GetOpFunctionName(Operator.or), inputs);
            var expr2 = BuildFunctionCall(GetOpFunctionName(Operator.and), inputs);
            var nexpr2 = BuildFunctionCall(GetOpFunctionName(UnaryOperator.Not), 
                                    new List<AssociativeNode> { expr2 });
            return BuildFunctionCall(GetOpFunctionName(Operator.and),
                                    new List<AssociativeNode> { expr1, nexpr2 });
        }

        public AssociativeNode Build(Not node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(UnaryOperator.Not), inputs);
        }

        public AssociativeNode Build(Addition node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.add), inputs);
        }

        public AssociativeNode Build(Subtraction node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.sub), inputs);
        }

        public AssociativeNode Build(Multiplication node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.mul), inputs);
        }

        public AssociativeNode Build(Division node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.div), inputs);
        }

        public AssociativeNode Build(Modulo node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.mod), inputs);
        }

        public AssociativeNode Build(Pow node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Pow", inputs);
        }

        public AssociativeNode Build(Round node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Round", inputs);
        }

        public AssociativeNode Build(Floor node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Floor", inputs);
        }

        public AssociativeNode Build(Ceiling node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Ceiling", inputs);
        }

        public AssociativeNode Build(Dynamo.Nodes.Random node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Rand", inputs);
        }

        public AssociativeNode Build(Pi node, List<AssociativeNode> inputs)
        {
            // Constant expression directly return from node.
            throw new NotSupportedException();
        }

        public AssociativeNode Build(PiTimes2 node, List<AssociativeNode> inputs)
        {
            // Constant expression directly returned from node
            throw new NotSupportedException();
        }

        public AssociativeNode Build(EConstant node, List<AssociativeNode> inputs)
        {
            // Constant expression directly returned from node
            throw new NotSupportedException();
        }

        public AssociativeNode Build(Sin node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Sin", inputs);
        }

        public AssociativeNode Build(Cos node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Tan", inputs);
        }

        public AssociativeNode Build(Tan node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Math.Tan", inputs);
        }

        public AssociativeNode Build(Average node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("Average", inputs);
        }

        public AssociativeNode Build(Conditional node, List<AssociativeNode> inputs)
        {
            return BuildConditionalNode(inputs[0], inputs[1], inputs[2]);
        }

        public AssociativeNode Build(Dynamo.Nodes.Double node, List<AssociativeNode> inputs)
        {
            return BuildDoubleNode(node.Value);
        }

        public AssociativeNode Build(Dynamo.Nodes.Bool node, List<AssociativeNode> inputs)
        {
            return BuildBooleanNode(node.Value);
        }

        public AssociativeNode Build(Dynamo.Nodes.String node, List<AssociativeNode> inputs)
        {
            return BuildStringNode(node.Value);
        }

        public AssociativeNode Build(DoubleInput node, List<AssociativeNode> inputs)
        {
            // Implemented in DoubleInput.BuildAstNode()
            throw new NotSupportedException();
        }

        public AssociativeNode Build(StringFilename node, List<AssociativeNode> inputs)
        {
            if (string.IsNullOrEmpty(node.Value))
            {
                return BuildNullNode();
            }
            else
            {
                return BuildStringNode(node.Value);
            }
        }

        public AssociativeNode Build(ConcatStrings node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall(GetOpFunctionName(Operator.add), inputs);
        }

        public AssociativeNode Build(ToString node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("ToString", inputs);
        }

        public AssociativeNode Build(JoinStrings node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("JoinStrings", inputs);
        }

        public AssociativeNode Build(SplitString node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(String2Num node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(StringCase node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(StringLen node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Substring node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Sublists node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Num2String node, List<AssociativeNode> inputs)
        {
            return BuildFunctionCall("ToString", inputs);
        }

        public AssociativeNode Build(TakeEveryNth node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Filter node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FilterOut node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FlattenListAmt node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ListMax node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ListMin node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(NumberSeq node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ShiftList node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Combine node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Formula node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Function node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(RemoveEveryNth node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(AndMap node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Apply1 node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ApplyList node, List<AssociativeNode> inputs)
        {
            string function = inputs[0].Name;
            inputs.RemoveAt(0);
            return BuildFunctionCall(function, inputs);
        }

        public AssociativeNode Build(ComposeFunctions node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(DeCons node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(DiagonalLeftList node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(DiagonalRightList node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Fold node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ForEach node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Map node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(OrMap node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(RandomSeed node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Symbol node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(VariableInput node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(LacerBase node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(NewtonRootFind1DNoDeriv node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(NewtonRootFind1DWithDeriv node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Output node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        //public AssociativeNode Build(RenderDescription node, List<AssociativeNode> inputs)
        //{
        //    throw new NotImplementedException();
        //}

        public AssociativeNode Build(Repeat node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Rest node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Smooth node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Begin node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(CartProd node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Breakpoint node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ExecuteInterval node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ListToCsv node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Pause node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Watch node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Domain node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        /*
        public AssociativeNode Build(Color node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ColorBrightness node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ColorComponents node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ColorHue node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ColorRange node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ColorSaturation node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }
        */

        public AssociativeNode Build(FileReader node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FileWriter node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ImageFileReader node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(ImageFileWriter node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FileWatcher node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FileWatcherChanged node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FileWatcherReset node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(FileWatcherWait node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Future node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Thunk node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(Now node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(UdpListener node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(WebRequest node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(DropDrownBase node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        public AssociativeNode Build(DropList node, List<AssociativeNode> inputs)
        {
            throw new NotImplementedException();
        }

        #endregion

        public static string GetOpFunctionName(Operator op)
        {
            return ProtoCore.DSASM.Constants.kInternalNamePrefix + op.ToString();
        }

        public static string GetOpFunctionName(UnaryOperator op)
        {
            return ProtoCore.DSASM.Constants.kInternalNamePrefix + op.ToString();
        }

        public static NullNode BuildNullNode()
        {
            return new NullNode();
        }

        public static IntNode BuildIntNode(int value)
        {
            return new IntNode(value.ToString());
        }

        public static DoubleNode BuildDoubleNode(double value)
        {
            return new DoubleNode(value.ToString());
        }

        public static StringNode BuildStringNode(string str)
        {
            return new StringNode { value = str };
        }

        public static BooleanNode BuildBooleanNode(bool value)
        {
            string strValue = value ? "true" : "false";
            return new BooleanNode { value = strValue };
        }

        public static AssociativeNode BuildUnaryExpression(AssociativeNode expression,
                                                           UnaryOperator op)
        {
            return BuildFunctionCall(GetOpFunctionName(op), new List<AssociativeNode> { expression });
        }

        public static AssociativeNode BuildBinaryExpression(AssociativeNode lhs,
                                                            AssociativeNode rhs,
                                                            Operator op)
        {
            return BuildFunctionCall(GetOpFunctionName(op), new List<AssociativeNode> { lhs, rhs });
        }

        public static InlineConditionalNode BuildConditionalNode(AssociativeNode condition,
                                                                 AssociativeNode trueExpr,
                                                                 AssociativeNode falseExpr)
        {
            InlineConditionalNode cond = new InlineConditionalNode();
            cond.ConditionExpression = condition;
            cond.TrueExpression = trueExpr;
            cond.FalseExpression = falseExpr;
            return cond;
        }

        public static AssociativeNode BuildFunctionCall(string function,
                                                        List<AssociativeNode> arguments)
        {
            string[] dotcalls = function.Split('.');
            string functionName = dotcalls[dotcalls.Length - 1];

            FunctionCallNode funcCall = new FunctionCallNode();
            funcCall.Function = BuildIdentifier(functionName);
            funcCall.FormalArguments = arguments;

            if (dotcalls.Length == 1)
            {
                return funcCall;
            }
            else
            {
                IdentifierNode lhs = BuildIdentifier(dotcalls[0]);
                return CoreUtils.GenerateCallDotNode(lhs, funcCall);
            }
        }

        public static IdentifierNode BuildIdentifier(string name)
        {
            return new IdentifierNode(name);
        }

        public static ExprListNode BuildExprList(List<AssociativeNode> nodes)
        {
            ExprListNode exprList = new ExprListNode();
            exprList.list = nodes;
            return exprList;
        }

        public static ExprListNode BuildExprList(List<string> exprs)
        {
            List<AssociativeNode> nodes = new List<AssociativeNode>();
            foreach (var item in exprs)
            {
                nodes.Add(BuildIdentifier(item));
            }
            return BuildExprList(nodes);
        }

        public static BinaryExpressionNode BuildAssignment(AssociativeNode lhs,
                                                           AssociativeNode rhs)
        {
            BinaryExpressionNode assignment = new BinaryExpressionNode();
            assignment.LeftNode = lhs;
            assignment.RightNode = rhs;
            assignment.Optr = ProtoCore.DSASM.Operator.assign;

            return assignment;
        }

        public static VarDeclNode BuildParamNode(string paramName)
        {
            VarDeclNode param = new VarDeclNode();
            param.NameNode = BuildIdentifier(paramName);

            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            type.Name = "var";
            param.ArgumentType = type;

            return param;
        }

        /// <summary>
        /// Create a function defintion for a partially applied function call. 
        /// E.g.
        /// 
        ///     foo(?, x, y, ?, z);
        ///     
        ///  ->
        ///
        ///     def foo_guid(p_1, p_2) = foo(p_1, x, y, p_2, z);
        ///     
        /// </summary>
        /// <param name="func"></param>
        /// </param>
        /// <returns></returns>
        public static FunctionDefinitionNode BuildPartialFunction(FunctionCallNode func)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            for (int i = 0; i < func.FormalArguments.Count; ++i)
            {
                if (func.FormalArguments[i] == null)
                {
                    VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                    partialArgs.Add(param);

                    func.FormalArguments[i] = param.NameNode;
                    paramPostfix++;
                }
            }

            // It is not a partial function call. 
            if (paramPostfix == 0)
            {
                return null;
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                var lhs = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                var rhs = BuildFunctionCall(func.Function.Name, func.FormalArguments);
                var returnStmt = BuildAssignment(lhs, rhs);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /// <summary>
        /// Binary expression in DesignScript internally will be converted to
        /// a function call, so need to create a function defintion together
        /// with a function call if any operand is missing in binary expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static FunctionDefinitionNode BuildPartilFunctionForBinaryOp(BinaryExpressionNode expr)
        {
            List<VarDeclNode> partialArgs = new List<VarDeclNode>();
            int paramPostfix = 0;

            if (expr.LeftNode == null)
            {
                VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                partialArgs.Add(param);
                expr.LeftNode = param.NameNode;
                paramPostfix++;
            }

            if (expr.RightNode == null)
            {
                VarDeclNode param = BuildParamNode(StringConstants.kParamPrefix + paramPostfix);
                partialArgs.Add(param);
                expr.RightNode = param.NameNode;
                paramPostfix++;
            }

            // It is not a partial function call. 
            if (paramPostfix == 0)
            {
                return null;
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                var lhs = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                var returnStmt = BuildAssignment(lhs, expr);
                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode partialFunc = new FunctionDefinitionNode();
            partialFunc.IsExternLib = false;
            partialFunc.IsDNI = false;
            partialFunc.ExternLibName = null;
            partialFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            partialFunc.Singnature = new ArgumentSignatureNode();
            partialFunc.Singnature.Arguments = partialArgs;
            partialFunc.FunctionBody = funcBody;

            return partialFunc;
        }

        /// <summary>
        /// Create a evaluation function defition node:
        /// 
        ///     def func_guid(p_1, p_2, p_3)
        ///     {
        ///         args = {p_1, p_2, p_3};
        ///         return = eval_guid.Evaluate(args);
        ///     }
        ///
        /// And create a function call to func_guid().
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="inputAstNodes"></param>
        /// <returns></returns>
        public FunctionCallNode BuildEvaluator(NodeModel node,
                                                      List<AssociativeNode> inputAstNodes)
        {
            // Here we'll create a function defintion which redirect 
            // DesignScript function call back to the node's own implementation 
            // of Evaluate().
            string evaluator = NamingUtil.NewUniqueName(StringConstants.kEvalFunctionPrefix);
            this.evalContext.Add(evaluator, new NodeEvaluator(node));

            List<VarDeclNode> arguments = new List<VarDeclNode>();
            for (int i = 0; i < inputAstNodes.Count; ++i)
            {
                VarDeclNode argument = BuildParamNode(StringConstants.kParamPrefix + i);
                arguments.Add(argument);
            }

            CodeBlockNode funcBody = new CodeBlockNode();
            {
                // args = {p_1, p_2, p_3};
                IdentifierNode argsVar = BuildIdentifier("args");
                ExprListNode argList = new ExprListNode();
                argList.list = arguments.Select(a => a.NameNode).ToList();
                BinaryExpressionNode assignment = BuildAssignment(argsVar, argList);
                funcBody.Body.Add(assignment);

                // return = e_guid.Evaluate(args);
                List<AssociativeNode> evaluateArgs = new List<AssociativeNode>();
                evaluateArgs.Add(argsVar);

                IdentifierNode evalInstance = BuildIdentifier(evaluator);
                FunctionCallNode eval = BuildFunctionCall(StringConstants.kEvalFunction, evaluateArgs) as FunctionCallNode;
                FunctionDotCallNode evalCall = CoreUtils.GenerateCallDotNode(evalInstance, eval, null);

                IdentifierNode ret = BuildIdentifier(ProtoCore.DSDefinitions.Kw.kw_return);
                BinaryExpressionNode returnStmt = BuildAssignment(ret, evalCall);

                funcBody.Body.Add(returnStmt);
            }

            FunctionDefinitionNode evalFunc = new FunctionDefinitionNode();
            evalFunc.IsExternLib = false;
            evalFunc.IsDNI = false;
            evalFunc.ExternLibName = null;
            evalFunc.Name = NamingUtil.NewUniqueName(StringConstants.kFunctionPrefix);
            evalFunc.Singnature = new ArgumentSignatureNode();
            evalFunc.Singnature.Arguments = arguments;
            evalFunc.FunctionBody = funcBody;
            this.AddNode(node.GUID, evalFunc);

            // Now make a call to this wrapper function
            return BuildFunctionCall(evalFunc.Name, inputAstNodes) as FunctionCallNode;
        }

        public void BuildEvaluation(NodeModel node, AssociativeNode rhs, bool isPartial = false)
        {
            // If it is a partially applied function, need to create a function 
            // definition and function pointer.
            if (isPartial)
            {
                if (rhs is FunctionCallNode)
                {
                    FunctionCallNode funcCall = rhs as FunctionCallNode;
                    // create a function definition for it
                    var newFunc = BuildPartialFunction(funcCall);
                    AddNode(node.GUID, newFunc);

                    // create a function pointer for this node
                    rhs = BuildIdentifier(newFunc.Name);
                }
                else if (rhs is BinaryExpressionNode)
                {
                    BinaryExpressionNode expr = rhs as BinaryExpressionNode;
                    if (expr.Optr != Operator.assign)
                    {
                        var newFunc = BuildPartilFunctionForBinaryOp(expr);
                        AddNode(node.GUID, newFunc);
                        rhs = BuildIdentifier(newFunc.Name);
                    }
                }
            }

            var assignment = BuildAssignment(node.AstIdentifier, rhs);
            AddNode(node.GUID, assignment);
        }
    }
}

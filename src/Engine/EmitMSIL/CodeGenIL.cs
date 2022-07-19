using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using System.IO;
using DSASM = ProtoCore.DSASM;
using ProtoCore.Properties;
using ProtoCore.Utils;

namespace EmitMSIL
{
    public class CodeGenIL:IDisposable
    {
        private ILGenerator ilGen;
        internal string className;
        internal string methodName;
        private bool isOperator; 
        private IDictionary<string, IList> input;
        private IDictionary<string, IList> output;
        private int localVarIndex = -1;
        private Dictionary<string, Tuple<int, Type>> variables = new Dictionary<string, Tuple<int, Type>>();
        private StreamWriter writer;
        private Dictionary<int, IEnumerable<MethodBase>> methodCache = new Dictionary<int, IEnumerable<MethodBase>>();
        private CompilePass compilePass;

        private enum CompilePass
        {
            // Compile pass to perform method lookup and populate method cache only
            MethodLookup,
            // Compile pass that performs the actual MSIL opCode emission
            emitIL,
            Done
        }

        public static int KeyGen(string className, string methodName, int numParameters)
        {
            unchecked
            {
                int hash = 0;
                hash = (hash * 397) ^ className.GetHashCode();
                hash = (hash * 397) ^ methodName.GetHashCode();
                return hash + numParameters;
            }
        }

        public CodeGenIL(IDictionary<string, IList> input, string filePath)
        {
            this.input = input;
            writer = new StreamWriter(filePath);
        }

        public IDictionary<string, IList> Emit(List<AssociativeNode> astList)
        {
            var compileResult = CompileAstToDynamicType(astList, AssemblyBuilderAccess.RunAndSave);

            // Invoke emitted method (ExecuteIL.Execute)
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute");
            var output = new Dictionary<string, IList>();
            var obj = mi.Invoke(null, new object[] { null, methodCache, output });

            compileResult.asmbuilder.Save("DynamicAssembly.dll");

            return output;
        }

        internal Dictionary<string,IList> EmitAndExecute(List<AssociativeNode> astList)
        {
            var compileResult = CompileAstToDynamicType(astList,AssemblyBuilderAccess.RunAndCollect);

            // Invoke emitted method (ExecuteIL.Execute)
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute");
            var output = new Dictionary<string, IList>();
            mi.Invoke(null, new object[] { null, methodCache, output });
            return output;
        }

        private (AssemblyBuilder asmbuilder,TypeBuilder tbuilder) CompileAstToDynamicType(List<AssociativeNode> astList, AssemblyBuilderAccess access)
        {
            compilePass = CompilePass.MethodLookup;
            foreach (var ast in astList)
            {
                DfsTraverse(ast);
            }

            // 1. Create assembly builder (dynamic assembly)
            var asm = BuilderHelper.CreateAssemblyBuilder("DynamicAssembly", false, access);
            // 2. Create module builder
            var mod = BuilderHelper.CreateDLLModuleBuilder(asm, "DynamicModule");
            // 3. Create type builder (name it "ExecuteIL")
            var type = BuilderHelper.CreateType(mod, "ExecuteIL");
            // 4. Create method ("Execute"), get ILGenerator 
            var execMethod = BuilderHelper.CreateMethod(type, "Execute",
                System.Reflection.MethodAttributes.Static, typeof(void), new[] { typeof(IDictionary<string, IList>),
                typeof(IDictionary<int, IEnumerable<MethodBase>>), typeof(IDictionary<string, IList>)});
            ilGen = execMethod.GetILGenerator();

            compilePass = CompilePass.emitIL;
            // 5. Traverse AST and use ILGen to emit code for Execute method
            foreach (var ast in astList)
            {
                DfsTraverse(ast);
            }
            EmitOpCode(OpCodes.Ret);

            writer.Close();
            return (asm,type);
        }

        private Type EmitCoercionCode(Type argType, ParameterInfo param)
        {
            if(argType == typeof(double) && param.ParameterType == typeof(long))
            {
                EmitOpCode(OpCodes.Conv_I8);
                return typeof(long);
            }
            // TODO: Add more coercion cases here.

            return argType;
        }

        private IEnumerable<MethodBase> FunctionLookup(IList args)
        {
            IEnumerable<MethodBase> mi = null;
            var key = KeyGen(className, methodName, args.Count);
            if (methodCache.TryGetValue(key, out IEnumerable<MethodBase> mBase))
            {
                mi = mBase;
            }
            else
            {
                if (!isOperator)
                {
                    var modules = ProtoFFI.DLLFFIHandler.Modules.Values.OfType<ProtoFFI.CLRDLLModule>();
                    var assemblies = modules.Select(m => m.Assembly ?? (m.Module?.Assembly)).Where(m => m != null);
                    foreach (var asm in assemblies)
                    {
                        var type = asm.GetType(className);
                        if (type == null) continue;

                        // There should be a way to get the exact method after matching parameter types for a node
                        // using its function descriptor. AST isn't sufficient for parameter type info.
                        // Fist check for static methods
                        mi = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
                            m => m.Name == methodName && m.GetParameters().Length == args.Count).ToList();

                        // Check for instance methods
                        if (mi == null || !mi.Any())
                        {
                            mi = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(
                                m => m.Name == methodName && m.GetParameters().Length + 1 == args.Count).ToList();
                        }

                        // Check for property getters
                        if (mi == null || !mi.Any())
                        {
                            var prop = type
                                .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(
                                    p => p.Name == methodName).FirstOrDefault();

                            if (prop != null)
                            {
                                mi = prop.GetAccessors().ToList();
                            }
                        }

                        // TODO: Add check for constructorinfo objects

                        if (mi != null)
                        {
                            methodCache.Add(key, mi);
                            break;
                        }

                        //if (method != null)
                        //{
                        //    argTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
                        //    return method.ReturnType;
                        //}
                    }
                }
                else
                {
                    mi = Operators.GetOperatorMethod(methodName);
                    if (mi != null)
                    {
                        methodCache.Add(key, mi);
                    }
                }
            }
            if (mi == null || !mi.Any())
            {
                throw new MissingMethodException("No matching method found in loaded assemblies.");
            }
            return mi;
        }

        private static HashSet<Type> GetTypeStatisticsForArray(ExprListNode array)
        {
            var arrayTypes = new HashSet<Type>();

            foreach (var exp in array.Exprs)
            {
                if (exp is ExprListNode eln)
                {
                    var subArray = GetTypeStatisticsForArray(eln);
                    var t = GetOverallTypeForArray(subArray);

                    if (t != typeof(object))
                    {
                        t = t.MakeArrayType();
                    }
                    arrayTypes.Add(t);
                }
                else
                {
                    Type t;
                    switch (exp.Kind)
                    {
                        case AstKind.Integer:
                            t = typeof(long);
                            break;
                        case AstKind.Double:
                            t = typeof(double);
                            break;
                        case AstKind.Boolean:
                            t = typeof(bool);
                            break;
                        case AstKind.Char:
                            t = typeof(char);
                            break;
                        case AstKind.String:
                            t = typeof(string);
                            break;
                        case AstKind.Identifier:
                            t = typeof(object);
                            break;
                        default:
                            t = typeof(object);
                            break;
                    }
                    arrayTypes.Add(t);
                }
            }
            return arrayTypes;
        }

        private static Type GetOverallTypeForArray(HashSet<Type> arrayTypes)
        {
            if (arrayTypes.Count == 1)
            {
                // There is only a single type in the array, return it.
                return arrayTypes.FirstOrDefault();
            }
            // TODO: Do we need to address cases, where there are more than 2 types?
            if(arrayTypes.Count == 2)
            {
                bool isLong = false;
                bool isDouble = false;
                bool isChar = false;
                bool isString = false;
                foreach (var type in arrayTypes)
                {
                    isLong = isLong || type == typeof(long);
                    isDouble = isDouble || type == typeof(double);
                    isChar = isChar || type == typeof(char);
                    isString = isString || type == typeof(string);
                }
                if (isLong && isDouble)
                {
                    return typeof(double);
                }
                if (isChar && isString)
                {
                    return typeof(string);
                }
            }
            return typeof(object);
        }

        public Type DfsTraverse(Node n)
        {
            Type t = null;
            if (!(n is AssociativeNode node) || node.skipMe)
                return t;

            switch (node.Kind)
            {
                case AstKind.Identifier:
                case AstKind.TypedIdentifier:
                    t = EmitIdentifierNode(node);
                    break;
                case AstKind.Integer:
                    t = EmitIntNode(node);
                    break;
                case AstKind.Double:
                    t = EmitDoubleNode(node);
                    break;
                case AstKind.Boolean:
                    t = EmitBooleanNode(node);
                    break;
                case AstKind.Char:
                    t = EmitCharNode(node);
                    break;
                case AstKind.String:
                    t = EmitStringNode(node);
                    break;
                case AstKind.DefaultArgument:
                    EmitDefaultArgNode();
                    break;
                case AstKind.Null:
                    t = EmitNullNode(node);
                    break;
                case AstKind.RangeExpression:
                    t = EmitRangeExprNode(node);
                    break;
                case AstKind.LanguageBlock:
                    EmitLanguageBlockNode(node);
                    break;
                case AstKind.ClassDeclaration:
                    EmitClassDeclNode(node);
                    break;
                case AstKind.Constructor:
                    EmitConstructorDefinitionNode(node);
                    break;
                case AstKind.FunctionDefintion:
                    EmitFunctionDefinitionNode(node);
                    break;
                case AstKind.FunctionCall:
                    t = EmitFunctionCallNode(node);
                    break;
                case AstKind.FunctionDotCall:
                    EmitFunctionCallNode(node);
                    break;
                case AstKind.ExpressionList:
                    t = EmitExprListNode(node);
                    break;
                case AstKind.IdentifierList:
                    t = EmitIdentifierListNode(node);
                    break;
                case AstKind.InlineConditional:
                    EmitInlineConditionalNode(node);
                    break;
                case AstKind.UnaryExpression:
                    EmitUnaryExpressionNode(node);
                    break;
                case AstKind.BinaryExpression:
                    EmitBinaryExpressionNode(node);
                    break;
                case AstKind.Import:
                    EmitImportNode(node);
                    break;
                case AstKind.DynamicBlock:
                    {
                        int block = (node as DynamicBlockNode).block;
                        EmitDynamicBlockNode(block);
                        break;
                    }
                case AstKind.ThisPointer:
                    EmitThisPointerNode();
                    break;
                case AstKind.Dynamic:
                    EmitDynamicNode();
                    break;
                case AstKind.GroupExpression:
                    EmitGroupExpressionNode(node);
                    break;
            }
            return t;
        }

        private LocalBuilder DeclareLocal(Type t)
        {
            writer.WriteLine($"{nameof(ilGen.DeclareLocal)} {t}");
            return ilGen.DeclareLocal(t);
        }

        private void EmitOpCode(OpCode opCode, LocalBuilder local)
        {
            ilGen.Emit(opCode, local);
            writer.WriteLine($"{opCode} {local}");
        }
        
        private void EmitOpCode(OpCode opCode)
        {
            ilGen.Emit(opCode);
            writer.WriteLine(opCode);
        }

        private void EmitOpCode(OpCode opCode, Type t)
        {
            ilGen.Emit(opCode, t);
            writer.WriteLine($"{opCode} {t}");
        }

        private void EmitOpCode(OpCode opCode, int index)
        {
            ilGen.Emit(opCode, index);
            writer.WriteLine($"{opCode} {index}");
        }

        private void EmitOpCode(OpCode opCode, string str)
        {
            ilGen.Emit(opCode, str);
            writer.WriteLine($"{opCode} {str}");
        }

        private void EmitOpCode(OpCode opCode, MethodBase mBase)
        {
            var mInfo = mBase as MethodInfo;
            if (mInfo != null)
            {
                ilGen.Emit(opCode, mInfo);
            }
            else
            {
                var cInfo = mBase as ConstructorInfo;
                if(cInfo != null) ilGen.Emit(opCode, cInfo);
            }
            writer.WriteLine($"{opCode} {mBase}");
        }

        private void EmitOpCode(OpCode opCode, double val)
        {
            ilGen.Emit(opCode, val);
            writer.WriteLine($"{opCode} {val}");
        }

        private void EmitOpCode(OpCode opCode, long val)
        {
            ilGen.Emit(opCode, val);
            writer.WriteLine($"{opCode} {val}");
        }

        private void EmitGroupExpressionNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitDynamicNode()
        {
            throw new NotImplementedException();
        }

        private void EmitThisPointerNode()
        {
            throw new NotImplementedException();
        }

        private void EmitDynamicBlockNode(int block)
        {
            throw new NotImplementedException();
        }
        
        private void EmitImportNode(AssociativeNode node)
        {
            //doing absolutely nothing is actually
            //enough to import binaries.
            //TODO do other important things here!
            //see: ProtoAssociative.CodeGen.EmitImportNode
        }

        private void EmitBinaryExpressionNode(AssociativeNode node)
        {
            var bNode = node as BinaryExpressionNode;
            if (bNode == null) throw new ArgumentException("AST node must be a Binary Expression");

            if (bNode.Optr == ProtoCore.DSASM.Operator.assign)
            {
                var t = DfsTraverse(bNode.RightNode);

                if (compilePass == CompilePass.MethodLookup) return;

                var lNode = bNode.LeftNode as IdentifierNode;
                if(lNode == null)
                {
                    throw new Exception("Left node is expected to be an identifier.");
                }
                if(variables.ContainsKey(lNode.Value))
                {
                    // variable being assigned already exists in dictionary.
                    throw new Exception("Variable redefinition is not allowed.");
                }
                variables.Add(lNode.Value, new Tuple<int, Type>(++localVarIndex, t));
                DeclareLocal(t);

                EmitOpCode(OpCodes.Stloc, localVarIndex);
                // Add variable to output dictionary: output.Add("varName", variable);
                EmitOpCode(OpCodes.Ldarg_2);
                EmitOpCode(OpCodes.Ldstr, lNode.Value);
                // if t is a single value, wrap it in an array of the single value.
                if (!typeof(IEnumerable).IsAssignableFrom(t))
                {
                    var localVarIndxes = new List<int>() { localVarIndex };
                    EmitArray(t, localVarIndxes, (int varIdx, int _) =>
                    {
                        EmitOpCode(OpCodes.Ldloc, varIdx);
                    });
                }
                else
                {
                    EmitOpCode(OpCodes.Ldloc, localVarIndex);
                }
                var mInfo = typeof(IDictionary<string, IList>).GetMethod(nameof(IDictionary<string, IList>.Add));
                EmitOpCode(OpCodes.Callvirt, mInfo);
            }
            else if(bNode.Optr == ProtoCore.DSASM.Operator.add)
            {
                DfsTraverse(bNode.LeftNode);
                DfsTraverse(bNode.RightNode);

                //TODO: Replace this!
                // It needs to be replaced with emission of a builtin Function call to an Add function
                // so we can call it via ReplicationLogic for replication scenarios.
                EmitOpCode(OpCodes.Add);
            }
            // TODO: add Emit calls for other binary operators

        }

        private void EmitUnaryExpressionNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitInlineConditionalNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private Type EmitIdentifierListNode(AssociativeNode node)
        {
            var iln = node as IdentifierListNode;
            if (iln == null) throw new ArgumentException("AST node must be an Identifier List.");
            className = CoreUtils.GetIdentifierExceptMethodName(iln);

            return DfsTraverse(iln.RightNode);
        }

        private Type EmitExprListNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (!(node is ExprListNode eln))
            {
                throw new ArgumentException("AST node must be an Expression List.");
            }
            var arrayTypes = GetTypeStatisticsForArray(eln);
            var ot = GetOverallTypeForArray(arrayTypes);

            EmitArray(ot, eln.Exprs, (AssociativeNode el, int idx) => 
            {
                Type t = DfsTraverse(el);
                if (t == null) return;

                if (ot == typeof(object) && t.IsValueType)
                {
                    EmitOpCode(OpCodes.Box, t);
                }
                if (ot == typeof(double))
                {
                    if (t == typeof(int) || t == typeof(long))
                    {
                        EmitOpCode(OpCodes.Conv_R8);
                    }
                }
            });
            return ot.MakeArrayType();
        }

        /// <summary>
        /// Creates an array of type "arrType" on the evaluation stack
        /// Each Item of the array must be emitted by the caller through the itemEmitter callback.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arrType">The type of the array</param>
        /// <param name="items">The list that will be iterated</param>
        /// <param name="itemEmitter">A callback on each of the item's items</param>
        private void EmitArray<T>(Type arrType, IEnumerable<T> items, Action<T, int> itemEmitter)
        {
            EmitOpCode(OpCodes.Ldc_I4, items == null ? 0 : items.Count());
            EmitOpCode(OpCodes.Newarr, arrType);

            if (items == null) return;

            OpCode stElemOpCode = OpCodes.Stelem_Ref;
            if (arrType == typeof(int))
                stElemOpCode = OpCodes.Stelem_I4;
            else if (arrType == typeof(long))
                stElemOpCode = OpCodes.Stelem_I8;
            else if (arrType == typeof(double))
                stElemOpCode = OpCodes.Stelem_R8;
            else if (arrType == typeof(bool))
                stElemOpCode = OpCodes.Stelem_I1;
            else if (arrType == typeof(char))
                stElemOpCode = OpCodes.Stelem_I2;

            int itemIndex = -1;
            foreach (T item in items)
            {
                itemIndex++;

                EmitOpCode(OpCodes.Dup);
                EmitOpCode(OpCodes.Ldc_I4, itemIndex);

                itemEmitter(item, itemIndex);

                EmitOpCode(stElemOpCode);
            }
        }

        private Type EmitFunctionCallNode(AssociativeNode node)
        {
            var fcn = node as FunctionCallNode;
            if (fcn == null) throw new ArgumentException("AST node must be a Function Call Node.");

            methodName = fcn.Function.Name;
            var args = fcn.FormalArguments;
            var numArgs = args.Count;
            isOperator = Operators.IsOperator(methodName);

            if (isOperator)
            {
                className = nameof(Operators);
            }

            if (compilePass == CompilePass.MethodLookup)
            {
                FunctionLookup(args);
                return null;
            }

            // Emit methodCache
            EmitOpCode(OpCodes.Ldarg_1);

            // Emit className for input to call to CodeGenIL.KeyGen
            EmitOpCode(OpCodes.Ldstr, className);

            // Emit methodName for input to call to CodeGenIL.KeyGen
            EmitOpCode(OpCodes.Ldstr, methodName);
            EmitOpCode(OpCodes.Ldc_I4, numArgs);

            var keygen = typeof(CodeGenIL).GetMethod(nameof(CodeGenIL.KeyGen));
            EmitOpCode(OpCodes.Call, keygen);

            var local = DeclareLocal(typeof(IEnumerable<MethodBase>));

            EmitOpCode(OpCodes.Ldloca, local);

            // Emit methodCache.TryGetValue(KeyGen(...), out IEnumerable<MethodBase> mInfos)
            var dictLookup = typeof(IDictionary<int, IEnumerable<MethodBase>>).GetMethod(
                nameof(IDictionary<int, IEnumerable<MethodBase>>.TryGetValue));
            EmitOpCode(OpCodes.Callvirt, dictLookup);

            EmitOpCode(OpCodes.Pop);
            EmitOpCode(OpCodes.Ldloc, local.LocalIndex);

            // Retrieve previously cached functions
            // TODO: Decide whether to process overloaded methods at compile time or leave it for runtime.
            // For now, we assume no overloads.
            var mBase = FunctionLookup(args).FirstOrDefault();
            var parameters = mBase.GetParameters();

            // Emit args for input to call to ReplicationLogic
            EmitArray(typeof(object), args, (AssociativeNode n, int index) => {
                Type t = DfsTraverse(n);
                t = EmitCoercionCode(t, parameters[index]);

                if (t == null) return;

                if (t.IsValueType)
                {
                    EmitOpCode(OpCodes.Box, t);
                }
            });

            // Emit guides
            EmitArray(typeof(string[]), args, (AssociativeNode n, int idx) =>
            {
                if (n is ArrayNameNode argIdent)
                {
                    var argGuides = argIdent.ReplicationGuides;
                    EmitArray(typeof(string), argGuides, (AssociativeNode gn, int gidx) =>
                    {
                        EmitOpCode(OpCodes.Ldstr, (gn as ReplicationGuideNode).RepGuide.Name);
                    });
                }
                else
                {
                    // Emit an empty string array.
                    EmitArray<string>(arrType: typeof(string), items: null, itemEmitter: null);
                }
            });

            // Emit call to ReplicationLogic
            keygen = typeof(Replication).GetMethod(nameof(Replication.ReplicationLogic));
            EmitOpCode(OpCodes.Call, keygen);

            return typeof(IList);
        }

        private void EmitFunctionDefinitionNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitConstructorDefinitionNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitClassDeclNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitLanguageBlockNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }



        private double GetStepValueAsDouble(AssociativeNode stepNode)
        {
            if(stepNode == null)
            {
                return 1;
            }
            if(stepNode is IntNode stpInt)
            {
                return stpInt.Value;
            }
            if(stepNode is DoubleNode stpDB)
            {
                return stpDB.Value;
            }
            return double.NaN;
        }

        private bool CheckIdentType<T>(IdentifierNode ident)
        {
            if (variables.TryGetValue(ident.Value, out Tuple<int, Type> output))
            {
                if (typeof(T) == output.Item2)
                {
                    return true;
                }
            }
            return false;
        }

            private Type EmitRangeExprNode(AssociativeNode node)
        {
            //we don't do anything if this is the methodlookup phase
            //as we need want to access the variable types which are not computed
            //until the emitIL phase.
            if (compilePass == CompilePass.MethodLookup)
            {
                return null;
            }
            const string unselectedToken = "UNSELECTEDTOKEN";
            const string intRangeMethodName = nameof(Builtin.RangeHelpers.GenerateRangeILInt);
            const string doubleRangeMethodName = nameof(Builtin.RangeHelpers.GenerateRangeILDouble);

            var range = node as RangeExprNode;
            var fromNode = range.From;
            var toNode = range.To;
            var stepNode = range.Step;
            var stepOp = range.StepOperator;
            var hasAmountOperator = range.HasRangeAmountOperator;

            //TODO we may want to do this check again at runtime.
            if(stepNode is DoubleNode && stepOp == DSASM.RangeStepOperator.Number)
            {
                throw new ArgumentException(Resources.kInvalidAmountInRangeExpression);
            }


            var methodName = unselectedToken;

            var isIntStep = stepNode is IntNode stpInt ||
                (hasAmountOperator && stepOp == DSASM.RangeStepOperator.StepSize && stepNode is DoubleNode stpDB && Math.Truncate(stpDB.Value) == stpDB.Value) ||
                stepNode == null; 
            

            if (fromNode is IntNode fint && toNode is IntNode tint && isIntStep)
            {

                var stpval = GetStepValueAsDouble(stepNode);
                //the requested range was not divided evenly by the approximate step, so we create a double range.
                if (stepOp == DSASM.RangeStepOperator.ApproximateSize &&  Math.Abs(fint.Value - tint.Value)%stpval != 0 ||
                 //the requested number of items does not fit evenly into the range, so we create a double range.
                   stepOp == DSASM.RangeStepOperator.Number && (Math.Abs(fint.Value - tint.Value) % (stpval-1) != 0)
                   )
                {
                    methodName = doubleRangeMethodName;
                }
                else
                {
                    methodName = intRangeMethodName;
                }
            }

            else if(fromNode is DoubleNode || toNode is DoubleNode || stepNode is DoubleNode)
            {
                methodName = doubleRangeMethodName;
            }

            //we still have not selected a method, lets check if our inputs are idents and have known types.
            if(methodName == unselectedToken)
            {
                //if we are generating a simple range and we have all doubles or ints we know what methods to call
                //in other cases we can't determine which overload to call without the values of these idents.
                if (stepOp == DSASM.RangeStepOperator.StepSize)
                {
                    if (new[] { fromNode, toNode, stepNode }.All(x => x is IdentifierNode ident && CheckIdentType<long>(ident))){
                        methodName = intRangeMethodName;
                    }
                    else if (new[] { fromNode, toNode, stepNode }.All(x => x is IdentifierNode ident && CheckIdentType<double>(ident)))
                    {
                        methodName = doubleRangeMethodName;
                    }
                }
            }
            if (methodName == unselectedToken)
            {
                //if we still have not been able to determine the type of range to generate, temporarily generate a double range.
                //TODO when we add alphabetic ranges we'll want to check for string/char types in the above logic but,
                //if we still get to this line we'll need to call a dynamic version of generate range that boxes objects.
                methodName = doubleRangeMethodName;
            }
               
            //call the generate method we've selected.

            IntNode op = null;
            switch (stepOp)
            {
                case DSASM.RangeStepOperator.StepSize:
                    op = new IntNode(0);
                    break;
                case DSASM.RangeStepOperator.Number:
                    op = new IntNode(1);
                    break;
                case DSASM.RangeStepOperator.ApproximateSize:
                    op = new IntNode(2);
                    break;
                default:
                    op = new IntNode(-1);
                    break;
            }
            var arguments = new List<AssociativeNode>
            {
                fromNode,
                toNode,
                stepNode ?? new NullNode(),
                op,
                AstFactory.BuildBooleanNode(stepNode != null),
                AstFactory.BuildBooleanNode(hasAmountOperator),
            };

            
            var rangeExprFunc = AstFactory.BuildFunctionCall(methodName, arguments);
            var idlist = new IdentifierListNode()
            {
                LeftNode = new IdentifierNode(typeof(Builtin.RangeHelpers).FullName),
                RightNode = rangeExprFunc
            };
            //we want to cache the call to generate range, so traverse down in any case.
            var t = DfsTraverse(idlist);
          
            return t;
        }


        private Type EmitNullNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;
            if (node is NullNode)
            {
                EmitOpCode(OpCodes.Ldnull);
                return null;
            }
            throw new ArgumentException("null node is expected.");
        }

        private void EmitDefaultArgNode()
        {
            throw new NotImplementedException();
        }

        private Type EmitStringNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (node is StringNode strNode)
            {
                EmitOpCode(OpCodes.Ldstr, strNode.Value);
                return typeof(string);
            }
            throw new ArgumentException("string node is expected.");
        }

        private Type EmitCharNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (node is CharNode charNode)
            {
                EmitOpCode(OpCodes.Ldc_I4_S, charNode.Value[0]);
                return typeof(char);
            }
            throw new ArgumentException("char node is expected.");
        }

        private Type EmitBooleanNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (node is BooleanNode boolNode)
            {
                if (boolNode.Value)
                    EmitOpCode(OpCodes.Ldc_I4_1);
                else
                    EmitOpCode(OpCodes.Ldc_I4_0);
                return typeof(bool);
            }
            throw new ArgumentException("bool node is expected.");
        }

        private Type EmitDoubleNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (node is DoubleNode dblNode)
            {
                EmitOpCode(OpCodes.Ldc_R8, dblNode.Value);
                return typeof(double);
            }
            throw new ArgumentException("double node is expected.");
        }

        private Type EmitIntNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            if (node is IntNode intNode)
            {
                EmitOpCode(OpCodes.Ldc_I8, intNode.Value);
                return typeof(long);
            }
            throw new ArgumentException("Int node is expected.");
        }

        private Type EmitIdentifierNode(AssociativeNode node)
        {
            if (compilePass == CompilePass.MethodLookup) return null;

            // only handle identifiers on rhs of assignment expression for now.
            if (node is IdentifierNode idNode)
            {
                // local variables on rhs of expression should have already been defined.
                if(!variables.TryGetValue(idNode.Value, out Tuple<int, Type> tup))
                {
                    throw new Exception("Variable is undefined!");
                }
                EmitOpCode(OpCodes.Ldloc, tup.Item1);
                return tup.Item2;
            }
            throw new ArgumentException("Identifier node expected.");
        }

        public void Dispose()
        {
            writer?.Dispose();
        }
    }

}

using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Properties;
using ProtoCore.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DSASM = ProtoCore.DSASM;

namespace EmitMSIL
{
    public class CodeGenIL : IDisposable
    {
        private ILGenerator ilGen;
        internal string className;
        internal string methodName;
        private IDictionary<string, IList> input;
        //private Dictionary<int, bool> isRepCall = new Dictionary<int, bool>();

        private Dictionary<string, Tuple<int, Type>> variables = new Dictionary<string, Tuple<int, Type>>();
        /// <summary>
        /// AST node to type info map, filled in the GatherTypeInfo compiler phase.
        /// </summary>
        private Dictionary<int, Type> astTypeInfoMap = new Dictionary<int, Type>();
        private StreamWriter writer;
        private Dictionary<int, IEnumerable<MethodBase>> methodCache = new Dictionary<int, IEnumerable<MethodBase>>();
        private CompilePass compilePass;
        internal (TimeSpan compileTime, TimeSpan executionTime) CompileAndExecutionTime;

        private enum CompilePass
        {
            // Compile pass to perform method lookup and populate method cache only
            MethodLookup,
            // Compile pass that performs the actual MSIL opCode emission
            emitIL,
            GatherTypeInfo,
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

        public IDictionary<string, object> Emit(List<AssociativeNode> astList)
        {
            var timer = new Stopwatch();
            timer.Start();
            var compileResult = CompileAstToDynamicType(astList, AssemblyBuilderAccess.RunAndSave);
            timer.Stop();
            CompileAndExecutionTime.compileTime = timer.Elapsed;
            // Invoke emitted method (ExecuteIL.Execute)
            timer.Restart();
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute");
            var output = new Dictionary<string, object>();

            // null can be replaced by an 'input' dictionary if available.
            var obj = mi.Invoke(null, new object[] { null, methodCache, output });
            timer.Stop();
            CompileAndExecutionTime.executionTime = timer.Elapsed;

            compileResult.asmbuilder.Save("DynamicAssembly.dll");

            return output;
        }

        internal Dictionary<string, object> EmitAndExecute(List<AssociativeNode> astList)
        {
            var compileResult = CompileAstToDynamicType(astList, AssemblyBuilderAccess.RunAndCollect);

            // Invoke emitted method (ExecuteIL.Execute)
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute");
            var output = new Dictionary<string, object>();
            mi.Invoke(null, new object[] { null, methodCache, output });
            return output;
        }

        private (AssemblyBuilder asmbuilder, TypeBuilder tbuilder) CompileAstToDynamicType(List<AssociativeNode> astList, AssemblyBuilderAccess access)
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
                typeof(IDictionary<int, IEnumerable<MethodBase>>), typeof(IDictionary<string, object>)});
            ilGen = execMethod.GetILGenerator();

            compilePass = CompilePass.GatherTypeInfo;
            // 5. Traverse AST and gather what type info we can.
            foreach (var ast in astList)
            {
                DfsTraverse(ast);
            }

            compilePass = CompilePass.emitIL;
            // 6. Traverse AST and use ILGen to emit code for Execute method
            foreach (var ast in astList)
            {
                DfsTraverse(ast);
            }
            EmitOpCode(OpCodes.Ret);

            writer.Close();
            return (asm, type);
        }

        // Given a double value on the stack, emit call to Math.Round(arg, 0, MidpointRounding.AwayFromZero);
        // to convert to int or long.
        private void EmitMathRound()
        {
            EmitOpCode(OpCodes.Ldc_I4_0);
            EmitOpCode(OpCodes.Ldc_I4_1);
            var roundMethod = typeof(Math).GetMethod(nameof(Math.Round), new[] { typeof(double), typeof(int), typeof(MidpointRounding) });
            EmitOpCode(OpCodes.Call, roundMethod);
        }

        private Type EmitCoercionCode(AssociativeNode arg, Type argType, ParameterInfo param)
        {
            if (argType == null) return argType;

            if(param.ParameterType == typeof(object) && argType.IsValueType)
            {
                EmitOpCode(OpCodes.Box, argType);
                return typeof(object);
            }

            if (param.ParameterType.IsAssignableFrom(argType)) return argType;

            if(argType == typeof(double) && param.ParameterType == typeof(long))
            {
                // Call Math.Round(arg, 0, MidpointRounding.AwayFromZero);
                EmitMathRound();

                EmitOpCode(OpCodes.Conv_I8);
                return typeof(long);
            }
            if (argType == typeof(double) && param.ParameterType == typeof(int))
            {
                // Call Math.Round(arg, 0, MidpointRounding.AwayFromZero);
                EmitMathRound();

                EmitOpCode(OpCodes.Conv_I4);
                return typeof(int);
            }
            if (argType == typeof(long) && param.ParameterType == typeof(int))
            {
                EmitOpCode(OpCodes.Conv_I4);
                return typeof(int);
            }
            if (argType == typeof(long) && param.ParameterType == typeof(double))
            {
                EmitOpCode(OpCodes.Conv_R8);
                return typeof(double);
            }

            if (argType == typeof(double[]) && typeof(IEnumerable<int>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<double, int>(arg, param.ParameterType);
            }
            if (argType == typeof(int[]) && typeof(IEnumerable<double>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<int, double>(arg, param.ParameterType);
            }
            if (argType == typeof(double[]) && typeof(IEnumerable<long>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<double, long>(arg, param.ParameterType);
            }
            if (argType == typeof(long[]) && typeof(IEnumerable<double>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<long, double>(arg, param.ParameterType);
            }
            if (argType == typeof(long[]) && typeof(IEnumerable<int>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<long, int>(arg, param.ParameterType);
            }
            if (argType == typeof(int[]) && typeof(IEnumerable<long>).IsAssignableFrom(param.ParameterType))
            {
                return EmitArrayCoercion<int, long>(arg, param.ParameterType);
            }

            if (typeof(IEnumerable<int>).IsAssignableFrom(argType) && typeof(IEnumerable<double>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<int, double>(arg);
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(argType) && typeof(IEnumerable<long>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<int, long>(arg);
            }
            if (typeof(IEnumerable<long>).IsAssignableFrom(argType) && typeof(IEnumerable<double>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<long, double>(arg);
            }
            if (typeof(IEnumerable<long>).IsAssignableFrom(argType) && typeof(IEnumerable<int>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<long, int>(arg);
            }
            if (typeof(IEnumerable<double>).IsAssignableFrom(argType) && typeof(IEnumerable<int>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<double, int>(arg);
            }
            if (typeof(IEnumerable<double>).IsAssignableFrom(argType) && typeof(IEnumerable<long>).IsAssignableFrom(param.ParameterType))
            {
                return EmitIEnumerableCoercion<double, long>(arg);
            }
            // TODO: Add more coercion cases here.

            return argType;
        }

        private Type EmitIEnumerableCoercion<Source, Target>(AssociativeNode arg)
        {
            if (compilePass == CompilePass.GatherTypeInfo)
            {
                return typeof(Target[]);
            }

            // Load array to be coerced.
            LocalBuilder localBuilder;
            int sourceArrayIndex = -1;
            if (arg is IdentifierNode ident)
            {
                sourceArrayIndex = variables[ident.Value].Item1;
            }
            else
            {
                localBuilder = DeclareLocal(typeof(IEnumerable<Source>), "IEnumerable to coerce");
                sourceArrayIndex = localBuilder.LocalIndex;

                EmitOpCode(OpCodes.Stloc, sourceArrayIndex);
                EmitOpCode(OpCodes.Ldloc, sourceArrayIndex);
            }

            var mInfo = typeof(Enumerable).GetMethods().Single(
                m => m.Name == nameof(Enumerable.Count) && m.IsStatic && m.GetParameters().Length == 1);
            var genericMInfo = mInfo.MakeGenericMethod(typeof(Source));

            // len = source.Count();
            EmitOpCode(OpCodes.Call, genericMInfo);
            localBuilder = DeclareLocal(typeof(int), "length of IEnumerable to coerce");
            var sourceArrayLengthIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, sourceArrayLengthIndex);

            // Load length of source array, var newarr = new Target[len];
            EmitOpCode(OpCodes.Ldloc, sourceArrayLengthIndex);
            EmitOpCode(OpCodes.Newarr, typeof(Target));

            // Declare new array to store coerced values
            var t = typeof(Target[]);
            localBuilder = DeclareLocal(t, "coerced target array");
            var newArrIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, newArrIndex);

            // Emit for loop to loop over array and convert.

            // i = 0;
            EmitOpCode(OpCodes.Ldc_I4_0);
            localBuilder = DeclareLocal(typeof(int), "for loop counter");
            var counterIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, counterIndex);

            // Load array to be coerced.
            EmitOpCode(OpCodes.Ldloc, sourceArrayIndex);
            mInfo = typeof(IEnumerable<Source>).GetMethod(nameof(IEnumerable<Source>.GetEnumerator));
            EmitOpCode(OpCodes.Callvirt, mInfo);

            localBuilder = DeclareLocal(typeof(IEnumerator<Source>), "enumerator");
            var enumeratorIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, enumeratorIndex);

            var loopBodyLabel = DefineLabel();
            var loopCondLabel = DefineLabel();

            EmitOpCode(OpCodes.Br_S, loopCondLabel.Value);

            MarkLabel(loopBodyLabel.Value, "label:body");
            EmitOpCode(OpCodes.Ldloc, enumeratorIndex);

            var prop = typeof(IEnumerator<Source>).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                                    p => p.Name == nameof(IEnumerator<Source>.Current)).FirstOrDefault();
            mInfo = prop.GetAccessors().FirstOrDefault();
            EmitOpCode(OpCodes.Callvirt, mInfo);

            localBuilder = DeclareLocal(typeof(Source), "element in source array");
            var sourceArrayElementIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, sourceArrayElementIndex);

            // target[i] = source_element;
            EmitOpCode(OpCodes.Ldloc, newArrIndex);
            EmitOpCode(OpCodes.Ldloc, counterIndex);
            EmitOpCode(OpCodes.Ldloc, sourceArrayElementIndex);
            if (typeof(Target) == typeof(double))
            {
                EmitOpCode(OpCodes.Conv_R8);
                EmitOpCode(OpCodes.Stelem_R8);
            }
            else if (typeof(Target) == typeof(int))
            {
                EmitOpCode(OpCodes.Conv_I4);
                EmitOpCode(OpCodes.Stelem_I4);
            }
            else if (typeof(Target) == typeof(long))
            {
                EmitOpCode(OpCodes.Conv_I8);
                EmitOpCode(OpCodes.Stelem_I8);
            }
            // i++;
            EmitOpCode(OpCodes.Ldloc, counterIndex);
            EmitOpCode(OpCodes.Ldc_I4_1);
            EmitOpCode(OpCodes.Add);
            EmitOpCode(OpCodes.Stloc, counterIndex);

            MarkLabel(loopCondLabel.Value, "label:loop condition");
            EmitOpCode(OpCodes.Ldloc, enumeratorIndex);

            mInfo = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));
            EmitOpCode(OpCodes.Callvirt, mInfo);

            var leaveLabel = DefineLabel();
            EmitOpCode(OpCodes.Brtrue_S, loopBodyLabel.Value);
            EmitOpCode(OpCodes.Leave_S, leaveLabel.Value);

            EmitOpCode(OpCodes.Ldloc, enumeratorIndex);

            var finallyLabel = DefineLabel();
            EmitOpCode(OpCodes.Brfalse_S, finallyLabel.Value);

            EmitOpCode(OpCodes.Ldloc, enumeratorIndex);

            mInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
            EmitOpCode(OpCodes.Callvirt, mInfo);

            MarkLabel(finallyLabel.Value, "label: finally label");
            EmitOpCode(OpCodes.Endfinally);

            MarkLabel(leaveLabel.Value, "label: exit label");
            EmitOpCode(OpCodes.Ldloc, newArrIndex);

            localBuilder = DeclareLocal(t, "target array");
            var targetArrayIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, targetArrayIndex);

            var endLabel = DefineLabel();
            EmitOpCode(OpCodes.Br_S, endLabel.Value);

            MarkLabel(endLabel.Value, "label: end label");
            EmitOpCode(OpCodes.Ldloc, targetArrayIndex);

            return t;
        }

        // Coerce int/long/double arrays to IEnumerable<T> or IList<T>
        private Type EmitArrayCoercion<Source, Target>(AssociativeNode arg, Type ienumerableParamType)
        {
            if (compilePass == CompilePass.GatherTypeInfo)
            {
                var returnType = typeof(Target[]);
                if (typeof(List<Target>).IsAssignableFrom(ienumerableParamType))
                {
                    returnType = typeof(List<Target>);
                }
                return returnType;
            }
            LocalBuilder localBuilder;
            // Load array to be coerced.
            int currentVarIndex = -1;
            if (arg is IdentifierNode ident)
            {
                currentVarIndex = variables[ident.Value].Item1;
            }
            else
            {
                localBuilder = DeclareLocal(typeof(Source[]), "array to coerce");
                currentVarIndex = localBuilder.LocalIndex;

                EmitOpCode(OpCodes.Stloc, currentVarIndex);
                EmitOpCode(OpCodes.Ldloc, currentVarIndex);
            }
            // Find length for array to be coerced (already on top of eval stack), len
            EmitOpCode(OpCodes.Ldlen);
            EmitOpCode(OpCodes.Conv_I4);

            // var newarr = new Target[len];
            EmitOpCode(OpCodes.Newarr, typeof(Target));

            // Declare new array to store coerced values
            var t = typeof(Target[]);
            localBuilder = DeclareLocal(t, "coerced array");
            var newArrIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, newArrIndex);

            // Emit for loop to loop over array and convert.

            // i = 0;
            //var counterIndex = newArrIndex + 1;
            localBuilder = DeclareLocal(typeof(int), "for loop counter");
            var counterIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Ldc_I4_0);
            EmitOpCode(OpCodes.Stloc, counterIndex);

            var loopBodyLabel = DefineLabel();
            var loopCondLabel = DefineLabel();

            EmitOpCode(OpCodes.Br_S, loopCondLabel.Value);

            // newarr[i] = (Target)arr[i];
            MarkLabel(loopBodyLabel.Value, "label:body");

            EmitOpCode(OpCodes.Ldloc, newArrIndex);
            EmitOpCode(OpCodes.Ldloc, counterIndex);

            
            EmitOpCode(OpCodes.Ldloc, currentVarIndex);
            EmitOpCode(OpCodes.Ldloc, counterIndex);

            if (typeof(Source) == typeof(double))
            {
                EmitOpCode(OpCodes.Ldelem_R8);
            }
            else if (typeof(Source) == typeof(long))
            {
                EmitOpCode(OpCodes.Ldelem_I8);
            }
            else if (typeof(Source) == typeof(int))
            {
                EmitOpCode(OpCodes.Ldelem_I4);
            }
            if (typeof(Target) == typeof(int))
            {
                if (typeof(Source) == typeof(double))
                {
                    EmitMathRound();
                }
                EmitOpCode(OpCodes.Conv_I4);
                EmitOpCode(OpCodes.Stelem_I4);
            }
            else if (typeof(Target) == typeof(long))
            {
                if (typeof(Source) == typeof(double))
                {
                    EmitMathRound();
                }
                EmitOpCode(OpCodes.Conv_I8);
                EmitOpCode(OpCodes.Stelem_I8);
            }
            if (typeof(Target) == typeof(double))
            {
                EmitOpCode(OpCodes.Conv_R8);
                EmitOpCode(OpCodes.Stelem_R8);
            }
            // i++;
            EmitOpCode(OpCodes.Ldloc, counterIndex);
            EmitOpCode(OpCodes.Ldc_I4_1);

            EmitOpCode(OpCodes.Add);
            
            EmitOpCode(OpCodes.Stloc, counterIndex);

            // i < arr.Length;
            MarkLabel(loopCondLabel.Value,"label:cond");

            EmitOpCode(OpCodes.Ldloc, counterIndex);

            // Load input array
            EmitOpCode(OpCodes.Ldloc, currentVarIndex);
            EmitOpCode(OpCodes.Ldlen);
            EmitOpCode(OpCodes.Conv_I4);

            EmitOpCode(OpCodes.Clt);

            EmitOpCode(OpCodes.Brtrue_S, loopBodyLabel.Value);

            EmitOpCode(OpCodes.Ldloc, newArrIndex);

            if(typeof(List<Target>).IsAssignableFrom(ienumerableParamType))
            {
                var requiredType = typeof(Target);
                var toListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));
                var mInfo = toListMethod.MakeGenericMethod(requiredType);

                EmitOpCode(OpCodes.Call, mInfo);
                t = typeof(List<Target>);
            }
            return t;
        }

        private IEnumerable<MethodBase> FunctionLookup(IList args)
        {
            IEnumerable<MethodBase> mbs = null;
            var key = KeyGen(className, methodName, args.Count);
            if (methodCache.TryGetValue(key, out IEnumerable<MethodBase> mBase))
            {
                mbs = mBase;
            }
            else
            {
                if (!CoreUtils.IsInternalMethod(methodName))
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
                        mbs = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
                            m => m.Name == methodName && m.GetParameters().Length == args.Count).ToList();

                        // Check for instance methods
                        if (mbs == null || !mbs.Any())
                        {
                            mbs = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(
                                m => m.Name == methodName && m.GetParameters().Length + 1 == args.Count).ToList();
                        }

                        // Check for property getters
                        if (mbs == null || !mbs.Any())
                        {
                            var prop = type
                                .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(
                                    p => p.Name == methodName).FirstOrDefault();

                            if (prop != null)
                            {
                                mbs = prop.GetAccessors().ToList();
                            }
                        }

                        // Check for constructorinfo objects
                        if(mbs == null || !mbs.Any())
                        {
                            mbs = type.GetConstructors().Where(
                                m => m.DeclaringType.Name == methodName && m.GetParameters().Length == args.Count).ToList();
                        }

                        if (mbs != null && mbs.Any())
                        {
                            methodCache.Add(key, mbs);
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
                    var method = BuiltIn.GetInternalMethod(methodName);
                    if (method != null)
                    {
                        mbs = new List<MethodBase>() { method };
                        methodCache.Add(key, mbs);
                    }
                }
            }
            if (mbs == null || !mbs.Any())
            {
                throw new MissingMethodException("No matching method found in loaded assemblies.");
            }
            return mbs;
        }

        private HashSet<Type> GetTypeStatisticsForArray(ExprListNode array, ref int rank)
        {
            rank += 1;
            var arrayTypes = new HashSet<Type>();

            foreach (var exp in array.Exprs)
            {
                if (exp is ExprListNode eln)
                {
                    var subArray = GetTypeStatisticsForArray(eln, ref rank);
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
                            if(variables.TryGetValue((exp as IdentifierNode).Value, out Tuple<int, Type> tuple))
                            {
                                t = tuple.Item2;
                            }
                            else t = typeof(object);
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
            if (arrayTypes.Count == 2)
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
                    t = EmitBinaryExpressionNode(node);
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
            if(compilePass == CompilePass.GatherTypeInfo)
            {
                //if the map already contains the node id AND it has changed type, then throw.
                if (astTypeInfoMap.ContainsKey(node.ID))
                {
                    if(t != astTypeInfoMap[node.ID])
                    {
                        throw new Exception($"ast {node.ID}:{node.Kind} already exists in map, and has changed type {astTypeInfoMap[node.ID]}-> {t}");
                    }
                }
                else
                {
                    astTypeInfoMap.Add(node.ID, t);
                }
            }
            return t;
        }

        private LocalBuilder DeclareLocal(Type t, string identifier)
        {         
            if (compilePass == CompilePass.GatherTypeInfo) return null;
            writer.WriteLine($"{nameof(ilGen.DeclareLocal)} {t} {identifier}");
            return ilGen.DeclareLocal(t);
        }

        private void EmitOpCode(OpCode opCode, Label label)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, label);
            writer.WriteLine($"{opCode} {label}");
        }

        private void EmitOpCode(OpCode opCode, LocalBuilder local)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, local);
            writer.WriteLine($"{opCode} {local}");
        }

        private void EmitOpCode(OpCode opCode)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode);
            writer.WriteLine(opCode);
        }

        private void EmitOpCode(OpCode opCode, Type t)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, t);
            writer.WriteLine($"{opCode} {t}");
        }

        private void EmitOpCode(OpCode opCode, int index)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, index);
            writer.WriteLine($"{opCode} {index}");
        }

        private void EmitOpCode(OpCode opCode, string str)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, str);
            writer.WriteLine($"{opCode} {str}");
        }

        private void EmitOpCode(OpCode opCode, MethodBase mBase)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            var mInfo = mBase as MethodInfo;
            if (mInfo != null)
            {
                ilGen.Emit(opCode, mInfo);
            }
            else
            {
                ilGen.Emit(opCode, mBase as ConstructorInfo);
            }
            writer.WriteLine($"{opCode} {mBase}");
        }

        private void EmitOpCode(OpCode opCode, double val)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, val);
            writer.WriteLine($"{opCode} {val}");
        }

        private void EmitOpCode(OpCode opCode, long val)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            ilGen.Emit(opCode, val);
            writer.WriteLine($"{opCode} {val}");
        }
        private void EmitILComment(string comment)
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            writer.WriteLine($"//{comment}");
        }

        private Label? DefineLabel()
        {
            if (compilePass == CompilePass.GatherTypeInfo) return null;
            return ilGen.DefineLabel();
        }

        private void MarkLabel(Label label,string labelcomment = "")
        {
            if (compilePass == CompilePass.GatherTypeInfo) return;
            writer.WriteLine($"//{labelcomment}");
            ilGen.MarkLabel(label);
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
        /// <summary>
        /// Emits binary expression IL, returns type of RHS if assignment.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>T of right hand side if assignment.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        private Type EmitBinaryExpressionNode(AssociativeNode node)
        {
            var bNode = node as BinaryExpressionNode;
            if (bNode == null) throw new ArgumentException("AST node must be a Binary Expression");

            if (bNode.Optr == DSASM.Operator.assign)
            {
                var t = DfsTraverse(bNode.RightNode);

                if (compilePass == CompilePass.MethodLookup) return null;

                var lNode = bNode.LeftNode as IdentifierNode;
                if (lNode == null)
                {
                    throw new Exception("Left node is expected to be an identifier.");
                }
                if (compilePass == CompilePass.GatherTypeInfo)
                {
                    if (variables.ContainsKey(lNode.Value))
                    {
                        // variable being assigned already exists in dictionary.
                        throw new Exception("Variable redefinition is not allowed.");
                    }
                    variables.Add(lNode.Value, new Tuple<int, Type>(-1, t));
                }
                var localBuilder = DeclareLocal(t, lNode.Value);
                int currentLocalVarIndex = -1;
                if (localBuilder != null)
                {
                    currentLocalVarIndex = localBuilder.LocalIndex;
                    variables[lNode.Value] = new Tuple<int, Type>(currentLocalVarIndex, variables[lNode.Value].Item2);
                }
                EmitOpCode(OpCodes.Stloc, currentLocalVarIndex);
                // Add variable to output dictionary: output.Add("varName", variable);
                EmitOpCode(OpCodes.Ldarg_2);
                EmitOpCode(OpCodes.Ldstr, lNode.Value);

                EmitOpCode(OpCodes.Ldloc, currentLocalVarIndex);
                if (t.IsValueType)
                {
                    EmitOpCode(OpCodes.Box, t);
                }
                var mInfo = typeof(IDictionary<string, object>).GetMethod(nameof(IDictionary<string, object>.Add));
                EmitOpCode(OpCodes.Callvirt, mInfo);
                return t;
            }
            return null;
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
            int rank = 0;
            var arrayTypes = GetTypeStatisticsForArray(eln, ref rank);
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
        /// <param name="itemEmitter">A callback on each of the items</param>
        private void EmitArray<T>(Type arrType, IEnumerable<T> items, Action<T, int> itemEmitter)
        {
            EmitOpCode(OpCodes.Ldc_I4, items == null ? 0 : items.Count());
            EmitOpCode(OpCodes.Newarr, arrType);

            if (items == null) return;

            OpCode stElemOpCode = OpCodes.Stelem_Ref;
            if (arrType == typeof(int))
            {
                stElemOpCode = OpCodes.Stelem_I4;
            }
            else if (arrType == typeof(long))
            {
                stElemOpCode = OpCodes.Stelem_I8;
            }
            else if (arrType == typeof(double))
            {
                stElemOpCode = OpCodes.Stelem_R8;
            }
            else if (arrType == typeof(bool))
            {
                stElemOpCode = OpCodes.Stelem_I1;
            }
            else if (arrType == typeof(char))
            {
                stElemOpCode = OpCodes.Stelem_I2;
            }

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

        //tries to emit opcodes for indexing an array or dictioanry
        private (bool success, Type type) TryEmitIndexing(FunctionCallNode fcn)
        {
          

            //to emit the correct msil we need to know the type of collection we are indexing.
            var array = fcn.FormalArguments.FirstOrDefault();

            //lets check the types in the astTypeMap - if enough info is known
            //we can proceed to emit indexing opcodes.
            if(compilePass == CompilePass.emitIL)
            {
                Type arrayT;
                if (astTypeInfoMap.TryGetValue(array.ID, out arrayT))
                {
                    //can't handle these with compile time indexing.
                    //TODO remove IList from this if stmt when we figure out function call return wrapping behavior.
                    //this is still a problem for BuiltIn Dictionaries that are wrapped in an IList.
                    if (arrayT == null || arrayT == typeof(IList) || arrayT == typeof(object))
                    {
                        return (false, null);
                    }
                }
                else
                {
                    return (false, null);
                }
            }
          
            //emit load array to stack.
            var t = DfsTraverse(array);

            if (t == null)
            {
                return (false, null);
            }
            else if (typeof(IDictionary).IsAssignableFrom(t))
            {
                if (t.IsGenericType)
                {
                    EmitIndexingForDictionary(fcn.FormalArguments[0], fcn.FormalArguments[1], t);
                    return (true, t.GenericTypeArguments[0]);
                }
                else
                {
                    EmitIndexingForDictionary(fcn.FormalArguments[0], fcn.FormalArguments[1], t);
                    return (true, typeof(object));
                }
            }
            //builtin DS dict is a wrapper
            else if (typeof(DesignScript.Builtin.Dictionary).IsAssignableFrom(t))
            {
                //TODO
                //emit function call for ValueAtKey or fallback to replication.
            }

            else if (t.IsArray)
            {
                EmitIndexingForArray(fcn.FormalArguments[0], fcn.FormalArguments[1], t.GetElementType());
                return (true, t.GetElementType());
            }
            // TODO we may want to bail for IList currently and let 
            // replication handle it as Ilist is usually a replicated output, and is nested.
            //Today either we end up emitting IList index opcodes incorrectly for all func return vals,
            //or we'll call the wrong overload until overload matching is fixed.
            else if (t == typeof(IList))
            {
                if (t.IsGenericType)
                {
                    EmitIndexingForIList(fcn.FormalArguments[0], fcn.FormalArguments[1], t, t.GenericTypeArguments[0]);
                    return (true, t.GenericTypeArguments[0]);
                }
                else
                {
                    EmitIndexingForIList(fcn.FormalArguments[0], fcn.FormalArguments[1], t, typeof(object));
                    return (true, typeof(object));
                }
            }
            EmitILComment("NOT ENOUGH TYPE INFO TO EMIT INDEXING");
            return (false, null);
        }


        private void EmitIndexingForIList(AssociativeNode array, AssociativeNode index, Type collectionType, Type listElementType)
        {
            var indexT = DfsTraverse(index);
            var prop = typeof(IList).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                                  p => p.Name == "Item").FirstOrDefault();
            var mi = prop.GetAccessors().FirstOrDefault();
            if (collectionType.IsGenericType)
            {
                mi = collectionType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetAccessors().FirstOrDefault();
            }
            EmitOpCode(OpCodes.Callvirt, mi);
            EmitILComment("INDEX ILIST OPERATION END");
        }

        private void EmitIndexingForDictionary(AssociativeNode array, AssociativeNode index, Type collectionType)
        {
            var indexT = DfsTraverse(index);
            var mi = typeof(IDictionary).GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public);
            if (collectionType.IsGenericType)
            {
                mi = collectionType.GetMethod("get_Item");
            }
            EmitOpCode(OpCodes.Callvirt, mi);
            EmitILComment("INDEX IDICTIONARY OPERATION END");
        }

        private void EmitIndexingForArray(AssociativeNode array, AssociativeNode index, Type arrayElementType)
        {
            //emit load index to stack.
            var indexT = DfsTraverse(index);
            //TODO if indexT is a collection then we need to generate multiple ldelem calls -
            //or we could also give up and let replication handle this by falling back to ValueAtIndex()

            //emit the call to do the lookup.
            if (arrayElementType.IsValueType)
            {
                EmitOpCode(OpCodes.Ldelem, arrayElementType);
            }
            else
            {
                EmitOpCode(OpCodes.Ldelem_Ref);
            }
            EmitILComment("INDEX ARRAY OPERATION END");
        }


        private Type EmitFunctionCallNode(AssociativeNode node)
        {
            var fcn = node as FunctionCallNode;
            if (fcn == null) throw new ArgumentException("AST node must be a Function Call Node.");

            methodName = fcn.Function.Name;
            var args = fcn.FormalArguments;
            var numArgs = args.Count;
            if (CoreUtils.IsInternalMethod(methodName))
            {
                className = nameof(BuiltIn);
            }

            //if the method name is builtin.valueAtIndex then don't emit a function call yet.
            //instead try to emit direct indexing... to do so, we'll need to wait until ilemit phase
            //so variable dictionary has valid data.
            if (className == Node.BuiltinGetValueAtIndexTypeName && methodName == Node.BuiltinValueAtIndexMethodName)
            {
                if (compilePass == CompilePass.MethodLookup)
                {
                    return null;
                }
                //try to emit indexing
                {
                    //if we succeed, no need to emit a function call for indexing.
                    //if we fail to emit direct indexing, we should emit a function
                    //call for one of the ValueAtIndex() overloads or ValueAtIndexDynamic().

                    var indexResult = TryEmitIndexing(fcn);
                    if (indexResult.success)
                    {
                        return indexResult.type;
                    }
                    //if we fail to emit indexing at compile time, 
                    //emit a function call to ValueAtIndex.
                    else
                    {
                        methodName = nameof(DesignScript.Builtin.Get.ValueAtIndex);
                        EmitILComment("NOT ENOUGH TYPE INFO TO EMIT INDEXING, EMIT ValueAtIndex FUNCTION CALL");
                    }
                }
            }

            if (compilePass == CompilePass.MethodLookup)
            {
                FunctionLookup(args);
                return null;
            }
            // Retrieve previously cached functions
            // TODO: Decide whether to process overloaded methods at compile time or leave it for runtime.
            // For now, we assume no overloads.
            var mBase = FunctionLookup(args).FirstOrDefault();
            var parameters = mBase.GetParameters();

            // number of args = number of parameters if static.
            // num args = num params + 1 if instance as first arg is this pointer.
            var isStaticOrCtor = mBase.IsStatic || mBase.IsConstructor;

            var doesReplicate = WillCallReplicate(parameters, isStaticOrCtor, args);

            // TODO: Figure out a way to avoid calling WillCallReplicate twice -
            // once in the GatherTypeInfo phase and again in the emitIL phase.
            // We should be able to cache the result in the GatherTypeInfo compile pass
            // and reuse it in the emitIL pass.
            //if (compilePass == CompilePass.GatherTypeInfo)
            //{
            //    if(isRepCall.ContainsKey(node.ID))
            //    {
            //        throw new Exception($"ast {node.ID}:{node.Kind} already exists in replicated call map.");
            //    }
            //    isRepCall.Add(node.ID, WillCallReplicate(parameters, isStatic, args));
            //}

            //bool doesReplicate = false;
            //if(!isRepCall.TryGetValue(node.ID, out doesReplicate))
            //{
            //    throw new Exception($"ast { node.ID }:{ node.Kind} does not exist in replicated call map.");
            //}
            if (doesReplicate)
            {
                // Emit methodCache passed as arg to global Execute method.
                EmitOpCode(OpCodes.Ldarg_1);

                // Emit className for input to call to CodeGenIL.KeyGen
                EmitOpCode(OpCodes.Ldstr, className);

                // Emit methodName for input to call to CodeGenIL.KeyGen
                EmitOpCode(OpCodes.Ldstr, methodName);
                EmitOpCode(OpCodes.Ldc_I4, numArgs);

                var keygen = typeof(CodeGenIL).GetMethod(nameof(CodeGenIL.KeyGen));
                EmitOpCode(OpCodes.Call, keygen);

                var local = DeclareLocal(typeof(IEnumerable<MethodBase>), "cached MethodBase objects");
                //local could be null if we are not emitting currently.
                if(local != null)
                {
                    EmitOpCode(OpCodes.Ldloca, local);
                }

                // Emit methodCache.TryGetValue(KeyGen(...), out IEnumerable<MethodBase> mInfos)
                var dictLookup = typeof(IDictionary<int, IEnumerable<MethodBase>>).GetMethod(
                    nameof(IDictionary<int, IEnumerable<MethodBase>>.TryGetValue));
                EmitOpCode(OpCodes.Callvirt, dictLookup);

                EmitOpCode(OpCodes.Pop);
                if (local != null)
                {
                    EmitOpCode(OpCodes.Ldloc, local.LocalIndex);
                }

                // Emit args for input to call to ReplicationLogic
                EmitArray(typeof(object), args, (AssociativeNode n, int index) =>
                {
                    Type t = DfsTraverse(n);
                    if (isStaticOrCtor)
                    {
                        t = EmitCoercionCode(n, t, parameters[index]);
                    }
                    else if (index > 0)
                    {
                        t = EmitCoercionCode(n, t, parameters[index - 1]);
                    }

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
                            var repGuideNode = gn as ReplicationGuideNode;
                            EmitOpCode(OpCodes.Ldstr, repGuideNode.ToString());
                        });
                    }
                    else
                    {
                    // Emit an empty string array.
                    EmitArray<string>(arrType: typeof(string), items: null, itemEmitter: null);
                    }
                });

                // Emit call to ReplicationLogic
                var repLogic = typeof(Replication).GetMethod(nameof(Replication.ReplicationLogic));
                EmitOpCode(OpCodes.Call, repLogic);

                return typeof(object);
            }
            else
            {
                // non-replicating call
                int index = 0;
                foreach(var arg in args)
                {
                    Type t = DfsTraverse(arg);
                    if (isStaticOrCtor)
                    {
                        t = EmitCoercionCode(arg, t, parameters[index]);
                    }
                    else if (index > 0)
                    {
                        t = EmitCoercionCode(arg, t, parameters[index - 1]);
                    }
                    index++;
                }

                if (mBase is MethodInfo mi)
                {
                    EmitOpCode(OpCodes.Call, mBase);
                    return mi.ReturnType;
                }
                else
                {
                    var ci = mBase as ConstructorInfo;
                    EmitOpCode(OpCodes.Newobj, ci);
                    return ci.DeclaringType;
                }
            }
        }

        private bool DoesParamArgRankMatch(ParameterInfo[] parameters, List<AssociativeNode> args, int argIndex)
        {
            foreach (var p in parameters)
            {
                if (CoreUtils.IsPrimitiveASTNode(args[argIndex]) || args[argIndex] is CharNode)
                {
                    // arg is a single value (primitive type), but param is not (array promotion case).
                    if (ArrayUtils.IsEnumerable(p.ParameterType) && p.ParameterType != typeof(string))
                    {
                        return false;
                    }

                    switch (args[argIndex].Kind)
                    {
                        case AstKind.String:
                            var strNode = args[argIndex] as StringNode;
                            if (p.ParameterType == typeof(char))
                            {
                                if (strNode.Value.Length != 1) return false;
                            }
                            break;
                        case AstKind.Char:
                            if (p.ParameterType == typeof(string)) return false;
                            break;
                    }
                }
                else if (args[argIndex] is ExprListNode exp)
                {
                    if (!ArrayUtils.IsEnumerable(p.ParameterType) || p.ParameterType == typeof(string))
                    {
                        // arg is an enumerable type, param is not.
                        return false;
                    }
                    //var argRank = 0;
                    //var arrayTypes = GetTypeStatisticsForArray(exp, ref argRank);
                    var argRank = GetReductionDepth(exp as AssociativeNode, (x) => x is ExprListNode ? (x as ExprListNode).Exprs : null);
                    if (argRank == -1)
                    {
                        // non-rectangular (jagged) array best handled by replication
                        return false;
                    }
                    if (argRank != GetRank(p.ParameterType)) return false;
                }
                else if (args[argIndex] is IdentifierNode idn)
                {
                    if (idn.ReplicationGuides.Count > 0) return false;

                    var t = variables[idn.Value].Item2;
                    if (t == typeof(object)) return false;

                    if (!p.ParameterType.Equals(t))
                    {
                        // TODO: check if rank of t matches with that of p.
                        var argRank = GetRank(t);
                        var paramRank = GetRank(p.ParameterType);
                        if (argRank != paramRank) return false;

                        // If both have rank 0, it could also be because their type info is ambiguous.
                        if (argRank == 0 && paramRank == 0) return false;
                    }
                }
                else return false;
                argIndex++;
            }
            return true;
        }

        private static int GetRank(Type type)
        {
            return type.IsArray ? GetArrayRank(type) : GetEnumerableRank(type);
        }

        private static int GetArrayRank(Type type)
        {
            if (!type.IsArray) return 0;

            return 1 + GetArrayRank(type.GetElementType());
        }

        private static int GetEnumerableRank(Type type)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 0) return 0;

            return 1 + GetEnumerableRank(genericArgs.FirstOrDefault());
        }

        private static int GetReductionDepth<T>(T val, Func<T, IEnumerable> asArr)
        {
            IEnumerable arr = asArr(val);
            if (arr == null)
            {
                return 0;
            }
            int firstRank = 0;
            int i = 0;
            //De-ref the val
            foreach (T subVal in arr)
            {
                int rank = GetReductionDepth(subVal, asArr);
                if (i == 0) firstRank = rank;

                if (rank != firstRank) return -1;
                i++;
            }
            return 1 + firstRank;
        }

        private bool WillCallReplicate(ParameterInfo[] parameters, bool isStaticOrCtor, List<AssociativeNode> args)
        {
            // number of args = number of parameters if static.
            // num args = num params + 1 if instance as first arg is this pointer.
            if (isStaticOrCtor)
            {
                return !DoesParamArgRankMatch(parameters, args, argIndex: 0);
            }
            else
            {
                // args[0] is assigned to the instance object in case of an instance method.
                if(args[0] is IdentifierNode idn)
                {
                    var t = variables[idn.Value].Item2;

                    if(t.IsArray || ArrayUtils.IsEnumerable(t))
                    {
                        return true;
                    }
                }
                return !DoesParamArgRankMatch(parameters, args, argIndex: 1);
            }
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
            if (stepNode == null)
            {
                return 1;
            }
            if (stepNode is IntNode stpInt)
            {
                return stpInt.Value;
            }
            if (stepNode is DoubleNode stpDB)
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
            if (stepNode is DoubleNode && stepOp == DSASM.RangeStepOperator.Number)
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
                if (stepOp == DSASM.RangeStepOperator.ApproximateSize && Math.Abs(fint.Value - tint.Value) % stpval != 0 ||
                   //the requested number of items does not fit evenly into the range, so we create a double range.
                   stepOp == DSASM.RangeStepOperator.Number && (Math.Abs(fint.Value - tint.Value) % (stpval - 1) != 0)
                   )
                {
                    methodName = doubleRangeMethodName;
                }
                else
                {
                    methodName = intRangeMethodName;
                }
            }

            else if (fromNode is DoubleNode || toNode is DoubleNode || stepNode is DoubleNode)
            {
                methodName = doubleRangeMethodName;
            }

            //we still have not selected a method, lets check if our inputs are idents and have known types.
            if (methodName == unselectedToken)
            {
                //if we are generating a simple range and we have all doubles or ints we know what methods to call
                //in other cases we can't determine which overload to call without the values of these idents.
                if (stepOp == DSASM.RangeStepOperator.StepSize)
                {
                    if (new[] { fromNode, toNode, stepNode }.All(x => x is IdentifierNode ident && CheckIdentType<long>(ident)))
                    {
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

            bool hasStep = stepNode != null;
            // The value of the dummy DoubleNode does not matter since the hasStep boolean will be false.
            AssociativeNode dummyStepNode = AstFactory.BuildDoubleNode(1);
            var arguments = new List<AssociativeNode>
            {
                fromNode,
                toNode,
                // TODO_MSIL: Figure out a better solution for this scenario.
                // Use DoubleNode(1) because standard replication cannot handle null to value type coerce
                // The old VM handles builtin functions (like range expr) in a special way...that does not try coercion
                hasStep ? stepNode : dummyStepNode,//NullNode()
                op,
                AstFactory.BuildBooleanNode(hasStep),
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
                if (!variables.TryGetValue(idNode.Value, out Tuple<int, Type> tup))
                {
                    throw new Exception($"Variable {idNode.Value} is undefined!");
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

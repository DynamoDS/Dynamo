using ProtoCore.AST;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Lang;
using ProtoCore.Properties;
using ProtoCore.Utils;
using ProtoFFI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DSASM = ProtoCore.DSASM;
using Autodesk.DesignScript.Runtime;

namespace EmitMSIL
{
    public class CodeGenIL : IDisposable
    {
        private ILGenerator ilGen;
        internal string className;
        internal string methodName;
        private IDictionary<string, IList> input;
        //private Dictionary<int, bool> isRepCall = new Dictionary<int, bool>();

        internal ProtoCore.MSILRuntimeCore runtimeCore;
        private Dictionary<string, Tuple<int, Type>> variables = new Dictionary<string, Tuple<int, Type>>();
        /// <summary>
        /// AST node to type info map, filled in the GatherTypeInfo compiler phase.
        /// </summary>
        private Dictionary<int, Type> astTypeInfoMap = new Dictionary<int, Type>();
        private StreamWriter writer;
        private Dictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>> methodCache = new Dictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>();
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

        internal CodeGenIL(IDictionary<string, IList> input, string filePath, ProtoCore.MSILRuntimeCore runtimeCore)
        {
            this.input = input;
            writer = new StreamWriter(filePath);
            this.runtimeCore = runtimeCore;
        }

        internal IDictionary<string, object> Emit(List<AssociativeNode> astList)
        {
            var compileResult = CompileAstToDynamicType(astList, AssemblyBuilderAccess.RunAndSave);
            // Invoke emitted method (ExecuteIL.Execute)
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Static);
            var output = new BuiltIn.MSILOutputMap<string, object>(runtimeCore);
            compileResult.asmbuilder.Save("DynamicAssembly.dll");

            // null can be replaced by an 'input' dictionary if available.
            var obj = mi.Invoke(null, new object[] { null, methodCache, output, runtimeCore });


            return output;
        }

        internal IDictionary<string, object> EmitAndExecute(List<AssociativeNode> astList)
        {
            var timer = new Stopwatch();
            timer.Start();
            var compileResult = CompileAstToDynamicType(astList, AssemblyBuilderAccess.RunAndCollect);
            timer.Stop();
            CompileAndExecutionTime.compileTime = timer.Elapsed;

            // Invoke emitted method (ExecuteIL.Execute)
            timer.Restart();
            var t = compileResult.tbuilder.CreateType();
            var mi = t.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Static);
            var output = new BuiltIn.MSILOutputMap<string, object>(runtimeCore);
            mi.Invoke(null, new object[] { null, methodCache, output, runtimeCore });
            timer.Stop();
            CompileAndExecutionTime.executionTime = timer.Elapsed;

            return output;
        }

        private (AssemblyBuilder asmbuilder, TypeBuilder tbuilder) CompileAstToDynamicType(List<AssociativeNode> astList, AssemblyBuilderAccess access)
        {
            compilePass = CompilePass.MethodLookup;
            // 0. Gather all loaded function endpoints and cache them.
            foreach (var ast in astList)
            {
                DfsTraverse(ast);
            }
            // 1. Create assembly builder (dynamic assembly)
            var asm = BuilderHelper.CreateAssemblyBuilder("DynamicAssembly", false, access);
            // 2. Create module builder
            var mod = BuilderHelper.CreateDLLModuleBuilder(asm, "DynamicAssembly");
            // 3. Create type builder (name it "ExecuteIL")
            var type = BuilderHelper.CreateType(mod, "ExecuteIL");
            // 4. Create method ("Execute"), get ILGenerator 
            var execMethod = BuilderHelper.CreateMethod(type, "Execute",
                System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.Private, typeof(void), new[] { typeof(IDictionary<string, IList>),
                typeof(IDictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>), typeof(IDictionary<string, object>), typeof(ProtoCore.MSILRuntimeCore)});
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

        private Type EmitCoercionCode(AssociativeNode arg, Type argType, Type param)
        {
            EmitILComment("coerce impl");
            if (argType == null) return argType;

            if(param == typeof(object) && argType.IsValueType)
            {
                EmitOpCode(OpCodes.Box, argType);
                return typeof(object);
            }

            if (param.IsAssignableFrom(argType)) return argType;

            if(argType == typeof(double) && param == typeof(long))
            {
                // Call Math.Round(arg, 0, MidpointRounding.AwayFromZero);
                EmitMathRound();

                EmitOpCode(OpCodes.Conv_I8);
                return typeof(long);
            }
            if (argType == typeof(double) && param == typeof(int))
            {
                // Call Math.Round(arg, 0, MidpointRounding.AwayFromZero);
                EmitMathRound();

                EmitOpCode(OpCodes.Conv_I4);
                return typeof(int);
            }
            if (argType == typeof(long) && param == typeof(int))
            {
                EmitOpCode(OpCodes.Conv_I4);
                return typeof(int);
            }
            if (argType == typeof(long) && param == typeof(double))
            {
                EmitOpCode(OpCodes.Conv_R8);
                return typeof(double);
            }

            if (argType == typeof(double[]) && typeof(IEnumerable<int>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<double, int>(arg, param);
            }
            if (argType == typeof(int[]) && typeof(IEnumerable<double>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<int, double>(arg, param);
            }
            if (argType == typeof(double[]) && typeof(IEnumerable<long>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<double, long>(arg, param);
            }
            if (argType == typeof(long[]) && typeof(IEnumerable<double>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<long, double>(arg, param);
            }
            if (argType == typeof(long[]) && typeof(IEnumerable<int>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<long, int>(arg, param);
            }
            if (argType == typeof(int[]) && typeof(IEnumerable<long>).IsAssignableFrom(param))
            {
                return EmitArrayCoercion<int, long>(arg, param);
            }

            if (typeof(IEnumerable<int>).IsAssignableFrom(argType) && typeof(IEnumerable<double>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<int, double>(arg);
            }
            if (typeof(IEnumerable<int>).IsAssignableFrom(argType) && typeof(IEnumerable<long>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<int, long>(arg);
            }
            if (typeof(IEnumerable<long>).IsAssignableFrom(argType) && typeof(IEnumerable<double>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<long, double>(arg);
            }
            if (typeof(IEnumerable<long>).IsAssignableFrom(argType) && typeof(IEnumerable<int>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<long, int>(arg);
            }
            if (typeof(IEnumerable<double>).IsAssignableFrom(argType) && typeof(IEnumerable<int>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<double, int>(arg);
            }
            if (typeof(IEnumerable<double>).IsAssignableFrom(argType) && typeof(IEnumerable<long>).IsAssignableFrom(param))
            {
                return EmitIEnumerableCoercion<double, long>(arg);
            }
            // TODO: Add more coercion cases here.

            return argType;
        }

        private Type EmitIEnumerableCoercion<Source, Target>(AssociativeNode arg)
        {
            EmitILComment("coerce IEnumerable");
            if (compilePass == CompilePass.GatherTypeInfo)
            {
                return typeof(Target[]);
            }

            /* This is emitting the following foreach loop for the conversion:
             
                var len = a.Count();    // where a is an IEnumerable<Source>
                var res = new Target[len];
                int c = 0;
                foreach (var i in a)
                {
                    res[c] = i;
                    c++;
                }
            */

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

            EmitOpCode(OpCodes.Brtrue_S, loopBodyLabel.Value);

            EmitOpCode(OpCodes.Ldloc, enumeratorIndex);

            mInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
            EmitOpCode(OpCodes.Callvirt, mInfo);

           
            EmitOpCode(OpCodes.Ldloc, newArrIndex);

            localBuilder = DeclareLocal(t, "target array");
            var targetArrayIndex = localBuilder.LocalIndex;
            EmitOpCode(OpCodes.Stloc, targetArrayIndex);
            EmitOpCode(OpCodes.Ldloc, targetArrayIndex);

            return t;
        }

        // Coerce int/long/double arrays to IEnumerable<T> or IList<T>
        private Type EmitArrayCoercion<Source, Target>(AssociativeNode arg, Type ienumerableParamType)
        {
            EmitILComment("array coercion");
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

        private IEnumerable<ProtoCore.CLRFunctionEndPoint> FunctionLookup(IList args)
        {
            var mbs = new List<ProtoCore.CLRFunctionEndPoint>();
            var key = KeyGen(className, methodName, args.Count);
            if (methodCache.TryGetValue(key, out IEnumerable<ProtoCore.CLRFunctionEndPoint> mBase))
            {
                return mBase;
            }
            
            if (!CoreUtils.IsInternalMethod(methodName))
            {
                int cid = runtimeCore.ClassTable.GetClassId(className);
                Validity.Assert(cid != DSASM.Constants.kInvalidIndex);
            }

            // TODO_MSIL: Figure out polymorfism when calling functions
            // That should be done at runtime (when we know the exact runtime type of the caller type)
            var fg = runtimeCore.GetFuncGroup(methodName, className);

            // This checks if there is a static property like Point.X(arg) 
            // and if so renames it to Point.get_X(arg) so that it can be 
            // found as a static getter in the class declaration.
            if (fg == null && args.Count == 1)
            {
                // Try to find a getter
                methodName = DSASM.Constants.kGetterPrefix + methodName;
                fg = runtimeCore.GetFuncGroup(methodName, className);
            }

            Validity.Assert(fg != null, "Did not find a function group");

            foreach (var funcEnd in fg.FunctionEndPoints)
            {
                var procNode = funcEnd.procedureNode;
                Validity.Assert(procNode != null, "Expected to have a valid procedureNode");

                bool IsExternalInstanceMethod = !procNode.IsStatic &&
                    !procNode.IsConstructor && !CoreUtils.IsInternalMethod(methodName);

                int parameterNumber = funcEnd.FormalParams.Length;
                int argsNumber = args.Count;
                int defaultParamNumber = funcEnd.procedureNode.ArgumentInfos.Count(x => x.IsDefault);
                if (!(argsNumber <= parameterNumber &&
                    parameterNumber - argsNumber <= defaultParamNumber))
                {
                    continue;
                }

                var methodParams = funcEnd.FormalParams.ToList();
                if (procNode.IsAutoGeneratedThisProc)
                {
                    // These are special pseudo-static functions created by the VM, to wrap non-static CLR methods.
                    // Ex. for ClassA_instance.DoSomething(int, int); the autoGeneratedThisProc function would be
                    // ClassA.DoSomething(ClassA_instance, int, int) {ClassA_instance.DoSomething(int, int);};

                    // We need to remove the "this" parameter so that we can match the CLR paramters
                    methodParams.RemoveAt(0);
                }

                FFIMemberInfo fFIMemberInfo = null;
                System.Type declaringType = null;
                ParameterInfo[] parameterInfos = null;

                if (CoreUtils.IsInternalMethod(methodName))//equivalent to if (funcEnd is JILFunctionEndPoint)
                {
                    var mInfo = BuiltIn.GetInternalMethod(methodName);
                    parameterInfos = mInfo.GetParameters();
                    declaringType = mInfo.DeclaringType;
                    fFIMemberInfo = new FFIMethodInfo(mInfo);
                }
                else if (funcEnd is FFIFunctionEndPoint ffiFep)
                {
                    FFIHandler handler = FFIFunctionEndPoint.FFIHandlers[ffiFep.activation.ModuleType];
                    Validity.Assert(handler != null, "Expected a valid FFIHandler");

                    FFIFunctionPointer functionPointer = handler.GetFunctionPointer(ffiFep.activation.ModuleName, className,
                            ffiFep.activation.FunctionName, methodParams, ffiFep.activation.ReturnType);
                    
                    if (functionPointer == null)
                    {
                        continue;
                    }

                    var clrFFI = functionPointer as CLRFFIFunctionPointer;
                    Validity.Assert(clrFFI != null, "Only CLRFFIFunctionPointer is supported for now");

                    parameterInfos = clrFFI.ReflectionInfo.GetParameters().Select(x => x.Info).ToArray();
                    declaringType = clrFFI.ReflectionInfo.DeclaringType;
                    fFIMemberInfo = clrFFI.ReflectionInfo;
                }
                else
                {
                    throw new NotImplementedException("Unkown FunctionEndpoint type. Not implemented yet");
                }

                Validity.Assert(parameterInfos.Length == methodParams.Count, "Expected argument counts to match");
                var fepParams = new List<ProtoCore.CLRFunctionEndPoint.ParamInfo>();

                if (IsExternalInstanceMethod || procNode.IsAutoGeneratedThisProc)
                {
                    // First argument is the this pointer
                    // So we need to add an extra parameter to match the arguments
                    var dsType = runtimeCore.GetProtoCoreType(declaringType);
                    fepParams.Add(new ProtoCore.CLRFunctionEndPoint.ParamInfo() { CLRType = declaringType, ProtoInfo = dsType });
                }

                // Add the CLR parameter infos and matching protoCore parameter types
                int i = 0;
                foreach (var paramInfo in parameterInfos)
                {
                    var protoType = methodParams[i];
                    fepParams.Add(new ProtoCore.CLRFunctionEndPoint.ParamInfo() { CLRType = paramInfo.ParameterType, ProtoInfo = protoType });
                    i++;
                }

                ProtoCore.CLRFunctionEndPoint fep = new ProtoCore.CLRFunctionEndPoint(fFIMemberInfo, fepParams, procNode);
                mbs.Add(fep);
            }

            if (mbs.Any())
            {
                methodCache.Add(key, mbs);
            }

            if (!mbs.Any())
            {
                throw new MissingMethodException("No matching method found in loaded assemblies.");
            }

            return mbs;
        }

        private HashSet<Type> GetTypeStatisticsForArray(ExprListNode array)
        {
            var arrayTypes = new HashSet<Type>();

            foreach (var exp in array.Exprs)
            {
                arrayTypes.Add(DfsTraverse(exp));
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
            if (compilePass == CompilePass.GatherTypeInfo)
            {
                var arrayTypes = GetTypeStatisticsForArray(eln);
                return GetOverallTypeForArray(arrayTypes).MakeArrayType();
            }
            if (compilePass == CompilePass.emitIL)
            {
                Type ot;
                if(!astTypeInfoMap.TryGetValue(node.ID, out ot))
                {
                    throw new Exception("unkown array type");
                }
                var otElementType = ot.GetElementType();
                EmitArray(otElementType, eln.Exprs, (AssociativeNode el, int idx) =>
                {

                    Type t;
                //if this element is a CLRStackValue, we need to unmarshal it.

                    if (astTypeInfoMap.TryGetValue(el.ID, out t) && t == typeof(DSASM.CLRStackValue))
                    {
                        EmitOpCode(OpCodes.Ldarg_2);
                        DfsTraverse(el);
                    //TODO cache this
                        var unmarshalMethod = typeof(BuiltIn.MSILOutputMap<string, object>).GetMethod("Unmarshal",
                        BindingFlags.Instance | BindingFlags.Public);
                        EmitOpCode(OpCodes.Callvirt, unmarshalMethod);
                        t = unmarshalMethod.ReturnType;
                    }
                    else
                    {
                        t = DfsTraverse(el);
                    }


                    if (t == null) return;



                    if (otElementType == typeof(object) && t.IsValueType)
                    {
                        EmitOpCode(OpCodes.Box, t);
                    }
                    if (otElementType == typeof(double))
                    {
                        if (t == typeof(int) || t == typeof(long))
                        {
                            EmitOpCode(OpCodes.Conv_R8);
                        }
                    }
                });
                return ot;
            }
            return null;
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
            if (array is IdentifierNode idNode)
            {
                // local variables on rhs of expression should have already been defined.
                if (!variables.TryGetValue(idNode.Value, out Tuple<int, Type> tup))
                {
                    throw new Exception($"Variable {idNode.Value} is undefined!");
                }
                //builtin DS dict is a wrapper
                if (typeof(DesignScript.Builtin.Dictionary).IsAssignableFrom(tup.Item2))
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
            var clrFep = FunctionLookup(args).FirstOrDefault();
            var parameters = clrFep.FormalParams.Select(x => x.CLRType).ToList();

            var isStaticOrCtor = clrFep.IsStatic || clrFep.IsConstructor;

            var doesReplicate = WillCallReplicate(clrFep, parameters, isStaticOrCtor, args);

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
                EmitILComment("emit replicating call");
                // Emit methodCache passed as arg to global Execute method.
                EmitOpCode(OpCodes.Ldarg_1);

                // Emit className for input to call to CodeGenIL.KeyGen
                EmitOpCode(OpCodes.Ldstr, className);

                // Emit methodName for input to call to CodeGenIL.KeyGen
                EmitOpCode(OpCodes.Ldstr, methodName);
                EmitOpCode(OpCodes.Ldc_I4, numArgs);

                var keygen = typeof(CodeGenIL).GetMethod(nameof(CodeGenIL.KeyGen));
                EmitOpCode(OpCodes.Call, keygen);

                var local = DeclareLocal(typeof(IEnumerable<ProtoCore.CLRFunctionEndPoint>), "cached MethodBase objects");
                //local could be null if we are not emitting currently.
                if(local != null)
                {
                    EmitOpCode(OpCodes.Ldloca, local);
                }

                // Emit methodCache.TryGetValue(KeyGen(...), out IEnumerable<CLRFunctionEndPoint> feps)
                var dictLookup = typeof(IDictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>).GetMethod(
                    nameof(IDictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>.TryGetValue));
                EmitOpCode(OpCodes.Callvirt, dictLookup);

                EmitOpCode(OpCodes.Pop);
                if (local != null)
                {
                    EmitOpCode(OpCodes.Ldloc, local.LocalIndex);
                }

                // Emit args for input to call to ReplicationLogic
                EmitILComment("emit args array start");
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
                EmitILComment("emit guides array start");
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

                // Emit call to load the runtimeCore argument
                EmitOpCode(OpCodes.Ldarg_3);

                // Emit call to ReplicationLogic
                var repLogic = typeof(Replication).GetMethod(nameof(Replication.ReplicationLogic), BindingFlags.Public | BindingFlags.Static);
                EmitOpCode(OpCodes.Call, repLogic);

                return typeof(DSASM.CLRStackValue);
            }
            else
            {
                EmitILComment("emit non replicating call");
                // non-replicating call

                Type argT;
                var unmarshalFunctionArgs = false;
                //if the args to this function are from replicated calls unmarshal them.
                foreach (var arg in args)
                {

                    if (astTypeInfoMap.TryGetValue(arg.ID, out argT) && argT == typeof(DSASM.CLRStackValue))
                    {
                        // one of the args is from replicated call - unmarshal.
                        unmarshalFunctionArgs = true;
                        break;
                    }
                }
                if (unmarshalFunctionArgs)
                {
                    EmitILComment("found replication wrapper arg, unmarshaling");
                    EmitUnmarshalFunctionArgs(args, parameters, isStaticOrCtor);
                }
                else
                {
                    EmitILComment("direct call, with no unmarshaling");
                    //emit args to the stack.
                    int index = 0;
                    foreach (var arg in args)
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
                }

                if (clrFep.MemberInfo is MethodInfo mi)
                {
                    EmitOpCode(OpCodes.Call, mi);
                    return mi.ReturnType;
                }
                else
                {
                    Validity.Assert(clrFep.IsConstructor);

                    var ci = clrFep.MemberInfo as ConstructorInfo;
                    EmitOpCode(OpCodes.Newobj, ci);
                    return ci.DeclaringType;
                }
            }
        }

        private void EmitUnmarshalFunctionArgs(List<AssociativeNode> args, List<Type> parameters, bool isStaticOrCtor)
        {

            // Emit methodCache passed as arg to global Execute method.
            EmitOpCode(OpCodes.Ldarg_1);
            // Emit className for input to call to CodeGenIL.KeyGen
            EmitOpCode(OpCodes.Ldstr, className);

            // Emit methodName for input to call to CodeGenIL.KeyGen
            EmitOpCode(OpCodes.Ldstr, methodName);
            EmitOpCode(OpCodes.Ldc_I4, args.Count);
            var keygen = typeof(CodeGenIL).GetMethod(nameof(CodeGenIL.KeyGen));
            EmitOpCode(OpCodes.Call, keygen);

            var local = DeclareLocal(typeof(IEnumerable<ProtoCore.CLRFunctionEndPoint>), "cached MethodBase objects");
            //local could be null if we are not emitting currently.
            if (local != null)
            {
                EmitOpCode(OpCodes.Ldloca, local);
            }

            // Emit methodCache.TryGetValue(KeyGen(...), out IEnumerable<CLRFunctionEndPoint> feps)
            var dictLookup = typeof(IDictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>).GetMethod(
                nameof(IDictionary<int, IEnumerable<ProtoCore.CLRFunctionEndPoint>>.TryGetValue));
            EmitOpCode(OpCodes.Callvirt, dictLookup);

            EmitOpCode(OpCodes.Pop);
            if (local != null)
            {
                EmitOpCode(OpCodes.Ldloc, local.LocalIndex);
            }

            EmitILComment("load array of args");
            //emit an array of args to pass to unmarshal
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

            //emit unmarshal for args
            // Emit call to load the runtimeCore argument
            EmitOpCode(OpCodes.Ldarg_3);
            //TODO_MSIL cache this and other method lookups at start to speedup compile.
            //now call unmarshall to convert any clrstackvalues to plain c# objs. 
            var marshalMethod = typeof(Replication).GetMethod(nameof(Replication.UnMarshalFunctionArguments2),
                BindingFlags.Static | BindingFlags.Public);
            EmitOpCode(OpCodes.Call, marshalMethod);
            //TODO_MSIL can we just use unmarshal here instead of unmarshalfunctionargs?
            var unMarshaledArgsArray = DeclareLocal(marshalMethod.ReturnType, "unMarshaledArgsArray");
            if (unMarshaledArgsArray != null)
            {
                EmitOpCode(OpCodes.Stloc, unMarshaledArgsArray.LocalIndex);
            }

            //foreach item in args -
            //increment index
            //emit code to index into array
            //ldelem ref.
            // we know array.length because it must be the same length as args.len

            if (unMarshaledArgsArray != null)
            {
                for (int i = 0; i < args.Count; i++)
                {
                    //load array
                    EmitOpCode(OpCodes.Ldloc, unMarshaledArgsArray.LocalIndex);
                    EmitOpCode(OpCodes.Ldc_I4, i);
                    EmitOpCode(OpCodes.Ldelem_Ref);
                    if (parameters[i].IsValueType)
                    {
                        EmitOpCode(OpCodes.Unbox_Any, parameters[i]);
                    }
                }
            }
        }

        private bool DoesParamArgRankMatch(ProtoCore.CLRFunctionEndPoint fep, List<Type> parameterTypes, List<AssociativeNode> args)
        {
            var argIndex = 0;
          
            foreach (var p in parameterTypes)
            {
                var currentArg = args[argIndex];
                if (compilePass == CompilePass.GatherTypeInfo)
                {
                    DfsTraverse(currentArg);
                }
                if (CoreUtils.IsPrimitiveASTNode(args[argIndex]) || args[argIndex] is CharNode)
                {
                    // arg is a single value (primitive type), but param is not (array promotion case).
                    if (ArrayUtils.IsEnumerable(p) && p != typeof(string))
                    {
                        return false;
                    }

                    switch (args[argIndex].Kind)
                    {
                        case AstKind.String:
                            var strNode = args[argIndex] as StringNode;
                            if (p == typeof(char))
                            {
                                if (strNode.Value.Length != 1) return false;
                            }
                            break;
                        case AstKind.Char:
                            if (p == typeof(string)) return false;
                            break;
                    }
                }
                else if (currentArg is ExprListNode exp)
                {
                    if (exp.ReplicationGuides.Count > 0) return false;
                    Type t;
                    if (!astTypeInfoMap.TryGetValue(exp.ID, out t))
                    {
                        throw new Exception("unkown ast type");
                    }
                    if (!DoesParamArgRankMatchInner(fep, p, t, argIndex)) return false;
                }
                else if (currentArg is IdentifierNode idn)
                {
                    if (idn.ReplicationGuides.Count > 0) return false;

                    var t = variables[idn.Value].Item2;
                    if (t == typeof(object)) return false;

                    if(!DoesParamArgRankMatchInner(fep,p, t,argIndex)) return false;
                }
                else if (currentArg is RangeExprNode rgnNode)
                {
                    Type t;
                    if (!astTypeInfoMap.TryGetValue(rgnNode.ID, out t))
                    {
                        throw new Exception("unkown ast type");
                    }
                    if (!DoesParamArgRankMatchInner(fep,p, t,argIndex)) return false;
                }
                else return false;
                argIndex++;
            }
            return true;
        }

        private static bool DoesParamArgRankMatchInner(ProtoCore.CLRFunctionEndPoint fep,Type p, Type t, int paramIndex)
        {
            if (!p.Equals(t))
            {
                //if both types are value types, let's
                //let coercion figure this out.
                if (p.IsValueType && t.IsValueType)
                {
                    return true;
                }

                var argRank = GetRank(fep,t,paramIndex);
                var paramRank = GetRank(fep,p, paramIndex);

                //if the param is an arbitrary rank array and
                //arg is some array type, replication should not occur.
                if (paramRank == -1 && argRank > 0)
                {
                    return true;
                }
                if (argRank != paramRank) return false;

                // If both have rank 0, it could also be because their type info is ambiguous.
                if (argRank == 0 && paramRank == 0) return false;
            }
            return true;
        }

        private static int GetRank(ProtoCore.CLRFunctionEndPoint fep,Type type,int paramIndex)
        {
            //TODO IList returns wrong rank.
            if(type == typeof(IList)){
                return -1;
            }
            if(type == typeof(object) && fep.ParamAttributes[paramIndex].Any(attr => attr is ArbitraryDimensionArrayImportAttribute))
            {
                return -1;
            }
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

        private static int GetArgumentRank(AssociativeNode val)
        {
            var arr = val is ExprListNode ? (val as ExprListNode).Exprs : null;
            if (arr == null)
            {
                return 0;
            }
            int firstRank = 0;
            int i = 0;
            //De-ref the val
            foreach (var subVal in arr)
            {
                int rank = GetArgumentRank(subVal);
                if (i == 0) firstRank = rank;

                if (rank != firstRank) return -1;
                i++;
            }
            return 1 + firstRank;
        }

        private bool WillCallReplicate(ProtoCore.CLRFunctionEndPoint fep,List<Type> paramTypes, bool isStaticOrCtor, List<AssociativeNode> args)
        {
            if (isStaticOrCtor)
            {
                return !DoesParamArgRankMatch(fep,paramTypes, args);
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
                return !DoesParamArgRankMatch(fep,paramTypes, args);
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

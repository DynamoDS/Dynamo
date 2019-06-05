using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Lang;
using ProtoCore.Runtime;
using ProtoCore.Utils;
using ProtoFFI;

namespace ProtoCore.DSASM.Mirror
{
    public class SymbolNotFoundException: Exception
    {
        public SymbolNotFoundException(string symbolName)
            : base("Cannot find symbol: " + symbolName)
        {
            this.SymbolName = symbolName;
        }

        public string SymbolName { get; private set; } 
    }

    //Status: Draft, experiment
    
    /// <summary>
    /// Provides reflective capabilities over the execution of a DSASM Executable
    /// </summary>
    public class ExecutionMirror
    {
        private readonly ProtoCore.RuntimeCore runtimeCore;
        public Executive MirrorTarget { get; private set; }
        private OutputFormatParameters formatParams;
        private Dictionary<string, List<string>> propertyFilter;

        /// <summary>
        /// Create a mirror for a given executive
        /// </summary>
        /// <param name="exec"></param>
        public ExecutionMirror(ProtoCore.DSASM.Executive exec, ProtoCore.RuntimeCore coreObj)
        {
            Validity.Assert(exec != null, "Can't mirror a null executive");

            runtimeCore = coreObj;
            MirrorTarget = exec;
        }

        public string GetStringValue(StackValue val, Heap heap, int langblock, bool forPrint = false)
        {
            return GetStringValue(val, heap, langblock, -1, -1, forPrint);
        }

        public string GetStringValue(StackValue val, Heap heap, int langblock, int maxArraySize, int maxOutputDepth, bool forPrint = false)
        {
            if (formatParams == null)
                formatParams = new OutputFormatParameters(maxArraySize, maxOutputDepth);

            if (val.IsInteger)
            {
                return val.IntegerValue.ToString();
            }

            if (val.IsDouble)
            {
                return val.DoubleValue.ToString("F6");
            }

            if (val.IsNull)
            {
                return "null";
            }

            if (val.IsPointer)
            {
                return GetClassTrace(val, heap, langblock, forPrint);
            }

            if (val.IsArray)
            {
                HashSet<int> pointers = new HashSet<int> { val.ArrayPointer };
                string arrTrace = GetArrayTrace(val, heap, langblock, pointers, forPrint);
                if (forPrint)
                    return "[" + arrTrace + "]";

                return "[ " + arrTrace + " ]";
            }

            if (val.IsFunctionPointer)
            {
                ProcedureNode procNode;
                if (runtimeCore.DSExecutable.FuncPointerTable.TryGetFunction(val, runtimeCore, out procNode))
                {
                    string className = String.Empty;
                    if (procNode.ClassID != Constants.kGlobalScope)
                    {
                        className = runtimeCore.DSExecutable.classTable.GetTypeName(procNode.ClassID).Split('.').Last() + ".";
                    }

                    return "function: " + className + procNode.Name;
                }
                return "function: " + val.FunctionPointer.ToString();
            }

            if (val.IsBoolean)
            {
                return val.BooleanValue ? "true" : "false";
            }

            if (val.IsString)
            {
                if (forPrint)
                    return heap.ToHeapObject<DSString>(val).Value;

                return "\"" + heap.ToHeapObject<DSString>(val).Value + "\"";
            }

            if (val.IsChar)
            {
                Char character = Convert.ToChar(val.CharValue);
                if (forPrint)
                    return character.ToString();

                return "'" + character + "'";
            }

            return "null"; // "Value not yet supported for tracing";
        }

        public string GetClassTrace(StackValue val, Heap heap, int langblock, bool forPrint)
        {
            if (!formatParams.ContinueOutputTrace())
                return "...";

            RuntimeMemory rmem = MirrorTarget.rmem;
            Executable exe = MirrorTarget.exe;
            ClassTable classTable = MirrorTarget.RuntimeCore.DSExecutable.classTable;

            int classtype = val.metaData.type;
            if (classtype < 0 || (classtype >= classTable.ClassNodes.Count))
            {
                formatParams.RestoreOutputTraceDepth();
                return string.Empty;
            }

            ClassNode classnode = classTable.ClassNodes[classtype];
            if (classnode.IsImportedClass)
            {
                var helper = DLLFFIHandler.GetModuleHelper(FFILanguage.CSharp);
                var marshaller = helper.GetMarshaler(runtimeCore);
                var strRep = marshaller.GetStringValue(val);
                formatParams.RestoreOutputTraceDepth();
                return strRep;
            }
            else
            {
                var obj = heap.ToHeapObject<DSObject>(val);

                StringBuilder classtrace = new StringBuilder();
                if (classnode.Symbols != null && classnode.Symbols.symbolList.Count > 0)
                {
                    if (!classnode.Name.Equals("Function"))
                    {
                        bool firstPropertyDisplayed = false;
                        for (int n = 0; n < obj.Count; ++n)
                        {
                            SymbolNode symbol = classnode.Symbols.symbolList[n];
                            string propName = symbol.name;

                            if (firstPropertyDisplayed)
                                classtrace.Append(", ");

                            string propValue = "";
                            if (symbol.isStatic)
                            {
                                var staticSymbol = exe.runtimeSymbols[langblock].symbolList[symbol.symbolTableIndex];
                                StackValue staticProp = rmem.GetSymbolValue(staticSymbol);
                                propValue = GetStringValue(staticProp, heap, langblock, forPrint);
                            }
                            else
                            {
                                propValue = GetStringValue(obj.GetValueFromIndex(symbol.index, runtimeCore), heap, langblock, forPrint);
                            }
                            classtrace.Append(string.Format("{0} = {1}", propName, propValue));
                            firstPropertyDisplayed = true;
                        }
                    }
                }
                else
                {
                    var stringValues = obj.Values.Select(x => GetStringValue(x, heap, langblock, forPrint))
                                                      .ToList();

                    for (int n = 0; n < stringValues.Count(); ++n)
                    {
                        if (0 != n)
                            classtrace.Append(", ");

                        classtrace.Append(stringValues[n]);
                    }
                }

                formatParams.RestoreOutputTraceDepth();
                if (classtype >= (int)ProtoCore.PrimitiveType.MaxPrimitive)
                    if (forPrint)
                        return (string.Format("{0}{{{1}}}", classnode.Name, classtrace.ToString()));
                    else
                    {
                        string tempstr =  (string.Format("{0}({1})", classnode.Name, classtrace.ToString()));
                        return tempstr;
                    }

                return classtrace.ToString();
            }
        }

        private string GetPointerTrace(StackValue ptr, Heap heap, int langblock, HashSet<int> pointers, bool forPrint)
        {
            if (pointers.Contains(ptr.ArrayPointer))
            {
                return "[ ... ]";
            }

            pointers.Add(ptr.ArrayPointer);

            if (forPrint)
            {
                return "[" + GetArrayTrace(ptr, heap, langblock, pointers, forPrint) + "]";
            }

            return "[ " + GetArrayTrace(ptr, heap, langblock, pointers, forPrint) + " ]";
        }

        private string GetArrayTrace(StackValue svArray, Heap heap, int langblock, HashSet<int> pointers, bool forPrint)
        {
            if (!formatParams.ContinueOutputTrace())
                return "...";

            StringBuilder arrayElements = new StringBuilder();
            var array = heap.ToHeapObject<DSArray>(svArray);

            int halfArraySize = -1;
            if (formatParams.MaxArraySize > 0) // If the caller did specify a max value...
            {
                // And our array is larger than that max value...
                if (array.Count > formatParams.MaxArraySize)
                    halfArraySize = (int)Math.Floor(formatParams.MaxArraySize * 0.5);
            }

            int totalElementCount = array.Count; 
            if (svArray.IsArray)
            {
                totalElementCount = heap.ToHeapObject<DSArray>(svArray).Values.Count();
            }

            for (int n = 0; n < array.Count; ++n)
            {
                // As we try to output the next element in the array, there 
                // should be a comma if there were previously output element.
                if (arrayElements.Length > 0)
                    if(forPrint)
                        arrayElements.Append(",");
                    else
                        arrayElements.Append(", ");

                StackValue sv = array.GetValueFromIndex(n, runtimeCore);
                if (sv.IsArray)
                {
                    arrayElements.Append(GetPointerTrace(sv, heap, langblock, pointers, forPrint));
                }
                else
                {
                    arrayElements.Append(GetStringValue(array.GetValueFromIndex(n, runtimeCore), heap, langblock, forPrint));
                }

                // If we need to truncate this array (halfArraySize > 0), and we have 
                // already reached the first half of it, then offset the loop counter 
                // to the next half of the array.
                if (halfArraySize > 0 && (n == halfArraySize - 1))
                {
                    arrayElements.Append(", ...");
                    n = totalElementCount - halfArraySize - 1;
                }
            }

            formatParams.RestoreOutputTraceDepth();
            return arrayElements.ToString();
        }

        private int GetSymbolIndex(string name, out int ci, ref int block, out SymbolNode symbol)
        {
            RuntimeMemory rmem = MirrorTarget.rmem;
            ProtoCore.DSASM.Executable exe = runtimeCore.DSExecutable;

            int functionIndex = Constants.kGlobalScope;
            ci = Constants.kInvalidIndex;
            int functionBlock = Constants.kGlobalScope;

            if (runtimeCore.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun))
            {
                ci = runtimeCore.watchClassScope = rmem.CurrentStackFrame.ClassScope;
                functionIndex = rmem.CurrentStackFrame.FunctionScope;
                functionBlock = rmem.CurrentStackFrame.FunctionBlock;
            }

            // TODO Jun: 'block' is incremented only if there was no other block provided by the programmer
            // This is only to address NUnit issues when retrieving a global variable
            // Some predefined functions are hard coded in the AST so isSingleAssocBlock will never be true
            //if (exe.isSingleAssocBlock)
            //{
            //    ++block;
            //}

            int index = -1;
            if (ci != Constants.kInvalidIndex)
            {
                ClassNode classnode = runtimeCore.DSExecutable.classTable.ClassNodes[ci];

                if (functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex && functionBlock != runtimeCore.RunningBlock)
                {
                    index = exe.runtimeSymbols[block].IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                }

                if (index == Constants.kInvalidIndex)
                {
                    index = classnode.Symbols.IndexOfClass(name, ci, functionIndex);
                }

                if (index != Constants.kInvalidIndex)
                {
                    symbol = classnode.Symbols.symbolList[index];
                    return index;
                }
            }
            else
            {
                CodeBlock searchBlock = runtimeCore.DSExecutable.CompleteCodeBlocks[block];

                // To detal with the case that a language block defined in a function
                //
                // def foo()
                // {   
                //     [Imperative]
                //     {
                //          a;
                //     }
                // }
                if (functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    if (searchBlock.IsMyAncestorBlock(functionBlock))
                    {
                        while (searchBlock.codeBlockId != functionBlock)
                        {
                            index = exe.runtimeSymbols[searchBlock.codeBlockId].IndexOf(name, ci, ProtoCore.DSASM.Constants.kInvalidIndex);
                            if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                searchBlock = searchBlock.parent;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        index = exe.runtimeSymbols[searchBlock.codeBlockId].IndexOf(name, ci, functionIndex);
                    }

                    if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        index = exe.runtimeSymbols[searchBlock.codeBlockId].IndexOf(name, ci, ProtoCore.DSASM.Constants.kInvalidIndex);
                    }
                }
                else
                {
                    index = exe.runtimeSymbols[searchBlock.codeBlockId].IndexOf(name, ci, ProtoCore.DSASM.Constants.kInvalidIndex);
                }

                if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    searchBlock = searchBlock.parent;
                    while (searchBlock != null)
                    {
                        block = searchBlock.codeBlockId;
                        index = exe.runtimeSymbols[searchBlock.codeBlockId].IndexOf(name, ci, ProtoCore.DSASM.Constants.kInvalidIndex);

                        if (index != ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            break;
                        }
                        else
                        {
                            searchBlock = searchBlock.parent;
                        }
                    }
                }

                if (index != ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    block = searchBlock.codeBlockId;
                    symbol = exe.runtimeSymbols[searchBlock.codeBlockId].symbolList[index];
                    return index;
                }
            }
            throw new NameNotFoundException { Name = name };

            //throw new NotImplementedException("{F5ACC95F-AEC9-486D-BC82-FF2CB26E7E6A}"); //@TODO(Luke): Replace this with a symbol lookup exception
        }

        public string GetType(string name)
        {
            RuntimeMemory rmem = MirrorTarget.rmem;

            int classcope;
            int block = MirrorTarget.RuntimeCore.RunningBlock;
            SymbolNode symbol;
            int index = GetSymbolIndex(name, out classcope, ref block, out symbol);

            StackValue val;
            if (symbol.functionIndex == -1 && classcope != Constants.kInvalidIndex)
                val = rmem.GetMemberData(index, classcope, runtimeCore.DSExecutable);
            else
                val = rmem.GetSymbolValue(symbol);

            if (val.IsInteger)
            {
                return "int";
            }
            else if (val.IsDouble)
            {
                return "double";
            }
            else if (val.IsNull)
            {
                return "null";
            }
            else if (val.IsPointer)
            {
                int classtype = val.metaData.type;
                ClassNode classnode = runtimeCore.DSExecutable.classTable.ClassNodes[classtype];
                return classnode.Name;
            }
            else if (val.IsArray)
            {
                return "array";
            }
            else if (val.IsBoolean)
            {
                return "bool";
            }
            else if (val.IsString)
            {
                return "string";
            }
            else
            { 
                    return "null"; // "Value not yet supported for tracing";
            }
        }

        // overload version of GetType which takes an Obj instead of string for IDE debuger use
        // usually the IDE has already get an instance of Obj before they call GetType
        // there is no need to look up that symbol again
        public string GetType(Obj obj)
        {
            if (obj.DsasmValue.IsPointer)
            {
                return runtimeCore.DSExecutable.classTable.ClassNodes[obj.DsasmValue.metaData.type].Name;
            }
            else
            {
                switch (obj.DsasmValue.optype)
                {
                    case AddressType.ArrayPointer:
                        return "array";
                    case AddressType.Int:
                        return "int";
                    case AddressType.Double:
                        return "double";
                    case AddressType.Null:
                        return "null";
                    case AddressType.Boolean:
                        return "bool";

                    case AddressType.String:
                        return "string";
                    case AddressType.Char:
                        return "char";
                    case AddressType.FunctionPointer:
                        return "function pointer";
                    default:
                        return null;
                }
            }
        }

        public Obj GetWatchValue()
        {
            RuntimeCore runtimeCore = MirrorTarget.RuntimeCore;
            int count = runtimeCore.watchStack.Count;
            int n = runtimeCore.WatchSymbolList.FindIndex(x => { return string.Equals(x.name, Constants.kWatchResultVar); });

            if (n < 0 || n >= count)
            {
                runtimeCore.WatchSymbolList.Clear();
                return new Obj { Payload = null };
            }

            Obj retVal = null;
            try
            {
                StackValue sv = runtimeCore.watchStack[n];
                if (!sv.IsInvalid)
                {
                    retVal = Unpack(runtimeCore.watchStack[n], MirrorTarget.rmem.Heap, runtimeCore);
                }
                else
                {
                    retVal = new Obj { Payload = null };
                }
            }
            catch
            {
                retVal = new Obj { Payload = null };
            }
            finally
            {
                runtimeCore.WatchSymbolList.Clear();
            }

            return retVal;
        }
        
        //
        //  1.	Get the graphnode given the varname
        //  2.	Get the sv of the symbol
        //  3.	set the sv to the new value
        //  4.	Get all graphnpodes dependent on the symbol and mark them dirty
        //  5.	Re-execute the script

        //  proc AssociativeEngine.SetValue(string varname, int block, StackValue, sv)
        //      symbol = dsEXE.GetSymbol(varname, block)
        //      globalStackIndex = symbol.stackindex
        //      runtime.stack[globalStackIndex] = sv
        //      AssociativeEngine.Propagate(symbol)
        //      runtime.Execute()
        //  end
        //

        private bool SetValue(string varName, int? value, out int nodesMarkedDirty)
        {
            int blockId = 0;

            // 1. Get the graphnode given the varname
            AssociativeGraph.GraphNode graphNode = MirrorTarget.GetFirstGraphNode(varName, out blockId);

            if (graphNode == null)
            {
                nodesMarkedDirty = 0;
                return false;
            }

            SymbolNode symbol = graphNode.updateNodeRefList[0].nodeList[0].symbol;

            // 2. Get the sv of the symbol
            int globalStackIndex = symbol.index;

            // 3. set the sv to the new value
            StackValue sv;
            if (null == value)
            {
                sv = StackValue.Null;
            }
            else
            {
                sv = StackValue.BuildInt((long)value);
            }
            MirrorTarget.rmem.Stack[globalStackIndex] = sv;

            // 4. Get all graphnpodes dependent on the symbol and mark them dirty
            const int outerBlock = 0;
            ProtoCore.DSASM.Executable exe = MirrorTarget.exe;
            List<AssociativeGraph.GraphNode> reachableGraphNodes = AssociativeEngine.Utils.UpdateDependencyGraph(
                graphNode, MirrorTarget, graphNode.exprUID, false, runtimeCore.Options.ExecuteSSA, outerBlock, false);

            // Mark reachable nodes as dirty
            Validity.Assert(reachableGraphNodes != null);
            nodesMarkedDirty = reachableGraphNodes.Count;
            foreach (AssociativeGraph.GraphNode gnode in reachableGraphNodes)
            {
                gnode.isDirty = true;
            }

            // 5. Re-execute the script - re-execution occurs after this call 

            return true;
        }

        /// <summary>
        /// Reset an existing value and re-execute the vm
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="value"></param>
        public void SetValueAndExecute(string varName, int? value)
        {
            Executable exe = runtimeCore.DSExecutable;

            runtimeCore.Options.IsDeltaExecution = true;
            int nodesMarkedDirty = 0;
            bool wasSet = SetValue(varName, value, out nodesMarkedDirty);

            if (wasSet && nodesMarkedDirty > 0)
            {
                try
                {
                    foreach (ProtoCore.DSASM.CodeBlock codeblock in exe.CodeBlocks)
                    {
                        ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(runtimeCore.RuntimeMemory.GlobOffset);
                        int locals = 0;

                        // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                        // Register TX is used for this.
                        stackFrame.TX = StackValue.BuildCallingConversion((int)CallingConvention.BounceType.Implicit);

                        runtimeCore.CurrentExecutive.CurrentDSASMExec.Bounce(
                            codeblock.codeBlockId, 
                            codeblock.instrStream.entrypoint, 
                            stackFrame,
                            locals);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public Obj GetDebugValue(string name)
        {
            int classcope;
            int block = runtimeCore.GetCurrentBlockId();

            RuntimeMemory rmem = MirrorTarget.rmem;
            SymbolNode symbol;
            int index = GetSymbolIndex(name, out classcope, ref block, out symbol);
            StackValue sv;
            if (symbol.functionIndex == -1 && classcope != Constants.kInvalidIndex)
                sv = rmem.GetMemberData(index, classcope, runtimeCore.DSExecutable);
            else
                sv = rmem.GetSymbolValue(symbol);

            if (sv.IsInvalid)
                throw new UninitializedVariableException { Name = name };

            return Unpack(sv);
        }

        // traverse an class type object to get its property
        public Dictionary<string, Obj> GetProperties(Obj obj, bool excludeStatic = false)
        {
            RuntimeMemory rmem = MirrorTarget.rmem;
            if (obj == null || !obj.DsasmValue.IsPointer)
                return null;

            Dictionary<string, Obj> ret = new Dictionary<string, Obj>();
            int classIndex = obj.DsasmValue.metaData.type;
            IDictionary<int,SymbolNode> symbolList = runtimeCore.DSExecutable.classTable.ClassNodes[classIndex].Symbols.symbolList;
            StackValue[] svs = rmem.Heap.ToHeapObject<DSObject>(obj.DsasmValue).Values.ToArray();
            int index = 0;
            for (int ix = 0; ix < svs.Length; ++ix)
            {
                if (excludeStatic && symbolList[ix].isStatic)
                    continue;
                string name = symbolList[ix].name;
                StackValue val = svs[index];

                // check if the members are primitive type
                if (val.IsPointer)
                {
                    var pointer = rmem.Heap.ToHeapObject<DSObject>(val);
                    var firstItem = pointer.Count == 1 ? pointer.GetValueFromIndex(0, runtimeCore) : StackValue.Null;
                    if (pointer.Count == 1 &&
                        !firstItem.IsPointer && 
                        !firstItem.IsArray)
                    {
                        val = firstItem;
                    }
                }

                ret[name] = Unpack(val);
                index++;
            }

            return ret;
        }

        // traverse an array Obj return its member
        public List<Obj> GetArrayElements(Obj obj)
        {
            if ( obj == null || !obj.DsasmValue.IsArray)
                return null;

            return MirrorTarget.rmem.Heap.ToHeapObject<DSArray>(obj.DsasmValue).Values.Select(x => Unpack(x)).ToList();
        }

        /// <summary>
        /// Searching variable name starting from specified block.
        /// Exception:
        ///     SymbolNotFoundException if variable not found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startBlock"></param>
        /// <returns></returns>
        public StackValue GetValue(string name, int startBlock = 0)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.exe;

            for (int block = startBlock; block < exe.runtimeSymbols.Length; block++)
            {
                int index = exe.runtimeSymbols[block].IndexOf(name, Constants.kInvalidIndex, Constants.kGlobalScope);
                if (Constants.kInvalidIndex != index)
                {
                    SymbolNode symNode = exe.runtimeSymbols[block].symbolList[index];
                    if (symNode.absoluteFunctionIndex == Constants.kGlobalScope)
                    {
                        return MirrorTarget.rmem.GetAtRelative(symNode.index);
                    }
                }
            }

            throw new SymbolNotFoundException(name);
        }

        public StackValue GetRawFirstValue(string name, int startBlock = 0, int classcope = Constants.kGlobalScope)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.exe;

            for (int block = startBlock; block < exe.runtimeSymbols.Length; block++)
            {
                int index = exe.runtimeSymbols[block].IndexOf(name, classcope, Constants.kGlobalScope);
                if (Constants.kInvalidIndex != index)
                {
                    //Q(Luke): This seems to imply that the LHS is an array index?
                    var symbol = exe.runtimeSymbols[block].symbolList[index];
                    return MirrorTarget.rmem.GetSymbolValue(symbol);
                }
            }
            throw new NotImplementedException("{F5ACC95F-AEC9-486D-BC82-FF2CB26E7E6A}"); //@TODO(Luke): Replace this with a symbol lookup exception
        }

        public Obj GetFirstValue(string name, int startBlock = 0, int classcope = Constants.kGlobalScope)
        {
            Obj retVal = Unpack(GetRawFirstValue(name, startBlock, classcope), MirrorTarget.rmem.Heap, runtimeCore);
            return retVal;
        }


        //@TODO(Luke): Add in the methods here that correspond to each of the internal datastructures in use by the executive
        //@TODO(Jun): if this method stays static, then the Heap needs to be referenced from a parameter
        /// <summary>
        /// Do the recursive unpacking of the data structure into mirror objects
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Obj Unpack(StackValue val, Heap heap, RuntimeCore runtimeCore, int type = (int)PrimitiveType.Pointer) 
        {
            Executable exe = runtimeCore.DSExecutable;
            switch (val.optype)
            {
                case AddressType.ArrayPointer:
                    {
                        DsasmArray ret = new DsasmArray();

                        //Pull the item out of the heap


                        var array = heap.ToHeapObject<DSArray>(val);

                        StackValue[] nodes = array.Values.ToArray();
                        ret.members = new Obj[array.Count];
                        for (int i = 0; i < ret.members.Length; i++)
                        {
                            ret.members[i] = Unpack(nodes[i], heap, runtimeCore, type);
                        }

                        // TODO Jun: ret.members[0] is hardcoded  and means we are assuming a homogenous collection
                        // How to handle mixed-type arrays?
                        Obj retO = new Obj(val) 
                        { 
                            Payload = ret, 
                        };

                        return retO;
                    }
                case AddressType.String:
                    {
                        string str = heap.ToHeapObject<DSString>(val).Value;
                        Obj o = new Obj(val)
                        {
                            Payload = str,
                        };
                        return o;
                    }
                case AddressType.Int:
                    {
                        Obj o = new Obj(val) 
                        { 
                            Payload = val.IntegerValue, 
                        };
                        return o;
                    }
                case AddressType.Boolean:
                    {
                        Obj o = new Obj(val)
                        {
                            Payload = val.BooleanValue,
                        };
                        return o;
                    }

                case AddressType.Null:
                    {
                        Obj o = new Obj(val) 
                        { 
                            Payload = null, 
                        };
                        return o;
                    }
                case AddressType.Char:
                    {
                        Obj o = new Obj(val) 
                        {
                            Payload = val.CharValue, 
                        };
                        return o;
                    }
                case AddressType.Double:
                    {
                        Obj o = new Obj(val) 
                        { 
                            Payload = val.DoubleValue,
                        };
                        return o;
                    }
                case AddressType.Pointer:
                    {
                        Obj o = new Obj(val) 
                        { 
                            Payload = val.Pointer,
                        };
                        return o;
                    }
                case AddressType.FunctionPointer:
                    {
                        Obj o = new Obj(val) 
                        { 
                            Payload = val.FunctionPointer, 
                        };
                        return o;
                    }
                case AddressType.Invalid:
                    {
                        return new Obj(val) {Payload = null};
                    }
                default:
                    {
                        throw new NotImplementedException(string.Format("unknown datatype {0}", val.optype.ToString()));
                    }
            }

        }

        // this method is used for the IDE to query object values 
        // Copy from the the existing Unpack with some modifications
        //  1: It is a non-static method so there is no need to pass the core and heap
        //  2: It does not traverse the array, array traverse is done in method GetArrayElement
        //  3: The payload for boolean and null is changed to Boolean and null type in .NET, such that the watch windows can directly call ToString() 
        //     to print the value, otherwize for boolean it will print either 0 or 1, for null it will print 0
        public Obj Unpack(StackValue val)
        {
            Obj obj = null;


            switch (val.optype)
            {
                case AddressType.Pointer:
                    obj = new Obj(val) 
                    { 
                        Payload = val.Pointer, 
                    };
                    break;
                case AddressType.ArrayPointer:
                    obj = new Obj(val) 
                    { 
                        Payload = val.ArrayPointer, 
                    };
                    break;
       
                case AddressType.Int:
                    obj = new Obj(val) 
                    { 
                        Payload = val.IntegerValue, 
                    };
                    break;
                case AddressType.Boolean:
                    obj = new Obj(val)
                    {
                        Payload = val.BooleanValue,
                    };
                    break;
                case AddressType.Double:
                    obj = new Obj(val) 
                    { 
                        Payload = val.DoubleValue, 
                    };
                    break;
                case AddressType.Null:
                    obj = new Obj(val) 
                    { 
                        Payload = null, 
                    };
                    break;
                case AddressType.FunctionPointer:
                    obj = new Obj(val) 
                    { 
                        Payload = val.FunctionPointer, 
                    };
                    break;
                case AddressType.String:
                    obj = new Obj(val) 
                    { 
                        Payload = val.StringPointer, 
                    };
                    break;
                case AddressType.Char:
                    obj = new Obj(val) 
                    { 
                        Payload = val.CharValue, 
                    };
                    break;
            }

            return obj;
        }
    }

    class OutputFormatParameters
    {
        private int maximumDepth = -1;
        private int maximumArray = -1;

        internal OutputFormatParameters()
        {
            this.CurrentOutputDepth = -1;
        }

        internal OutputFormatParameters(int maximumArray, int maximumDepth)
        {
            this.maximumArray = maximumArray;
            this.maximumDepth = maximumDepth;
            this.CurrentOutputDepth = this.maximumDepth;
        }

        internal bool ContinueOutputTrace()
        {
            // No output depth specified.
            if (-1 == maximumDepth)
                return true;

            // Previously reached zero, don't keep decreasing because that 
            // will essentially reach -1 and depth control will be disabled.
            if (0 == CurrentOutputDepth)
                return false;

            // Discontinue if we reaches zero.
            CurrentOutputDepth--;
            return (0 != CurrentOutputDepth);
        }

        internal void RestoreOutputTraceDepth()
        {
            if (-1 == maximumDepth)
                return;

            CurrentOutputDepth++;
        }

        internal int MaxArraySize { get { return maximumArray; } }
        internal int CurrentOutputDepth { get; private set; }
    }

    public class NameNotFoundException : Exception
    {
        public string Name { get; set; }
    }
    public class UninitializedVariableException : Exception
    {
        public string Name { get; set; }
    }

    //@TODO(Luke): turn this into a proper shadow array representation
    
    public class DsasmArray
    {
        public Obj[] members;

    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ProtoCore.Exceptions;
using ProtoCore.Lang;
using ProtoFFI;
using ProtoCore.Utils;
using ProtoCore.Runtime;

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
        private readonly ProtoCore.Core core;
        public Executive MirrorTarget { get; private set; }
        private OutputFormatParameters formatParams = null;
        private Dictionary<string, List<string>> propertyFilter;

        /// <summary>
        /// Create a mirror for a given executive
        /// </summary>
        /// <param name="exec"></param>
        public ExecutionMirror(ProtoCore.DSASM.Executive exec, ProtoCore.Core coreObj)
        {
            Validity.Assert(exec != null, "Can't mirror a null executive");

            core = coreObj;
            MirrorTarget = exec;

            LoadPropertyFilters();
        }

        private void LoadPropertyFilters()
        {
            if (core.Options.RootCustomPropertyFilterPathName == null)
            {
                return;
            }

            System.IO.FileInfo file = new System.IO.FileInfo(core.Options.RootCustomPropertyFilterPathName);
            if (!file.Exists)
            {
                return;
            }

            using (var stream = file.OpenText())
            {
                propertyFilter = new Dictionary<string, List<string>>();
                var line = stream.ReadLine();
                while (line != null)
                {
                    //  after removing leading and trailing spaces if there is something then
                    //  only try to tokenize
                    //
                    line = line.Trim();
                    if (line.Length != 0)
                    {
                        if (line.StartsWith(";"))
                        {
                            //  now over to next line
                            line = stream.ReadLine();

                            //  neglect a comment;
                            continue;
                        }
                        //  Point X,Y,Z
                        //
                        //  this will give you two strings:
                        //  0-> Point
                        //  1-> X,Y,Z
                        var splitStrings1 = line.Split(' ', ',');

                        //  first string in this array is class-name
                        //
                        var className = splitStrings1[0];

                        //  second string is optional, so check it it exists
                        //
                        List<string> classProps = null;
                        if (splitStrings1.Length > 1)
                        {
                            classProps = new List<string>();
                            for (int i = 1; i < splitStrings1.Length; ++i)
                            {
                                while (String.IsNullOrWhiteSpace(splitStrings1[i]))
                                {
                                    i++;
                                }
                                classProps.Add(splitStrings1[i]);
                            }
                        }
                        propertyFilter.Add(className, classProps);
                    }

                    //  now over to next line
                    line = stream.ReadLine();
                }
            }
        }

        public string PrintClass(StackValue val, Heap heap, int langblock, bool forPrint)
        {
            return PrintClass(val, heap, langblock, -1, -1, forPrint);
        }

        public string PrintClass(StackValue val, Heap heap, int langblock, int maxArraySize, int maxOutputDepth, bool forPrint)
        {
            if (null == formatParams)
                formatParams = new OutputFormatParameters(maxArraySize, maxOutputDepth);

            return GetClassTrace(val, heap, langblock, forPrint);
        }

        private string GetFormattedValue(string varname, string value)
        {
            return string.Format("{0} = {1}", varname, value);
        }

        public string GetStringValue(StackValue val, Heap heap, int langblock, bool forPrint = false)
        {
            return GetStringValue(val, heap, langblock, -1, -1, forPrint);
        }

        public string GetStringValue(StackValue val, Heap heap, int langblock, int maxArraySize, int maxOutputDepth, bool forPrint = false)
        {
            if (formatParams == null)
                formatParams = new OutputFormatParameters(maxArraySize, maxOutputDepth);

            switch (val.optype)
            {
                case AddressType.Int:
                    return val.opdata.ToString();
                case AddressType.Double:
                    return val.opdata_d.ToString(core.Options.FormatToPrintFloatingPoints);
                case AddressType.Null:
                    return "null";
                case AddressType.Pointer:
                    return GetClassTrace(val, heap, langblock, forPrint);
                case AddressType.ArrayPointer:
                    HashSet<int> pointers = new HashSet<int>{(int)val.opdata};
                    string arrTrace = GetArrayTrace((int)val.opdata, heap, langblock, pointers, forPrint);
                    if (forPrint)
                        return "{" + arrTrace + "}";
                    else
                        return "{ " + arrTrace + " }";
                case AddressType.FunctionPointer:
                    return "fptr: " + val.opdata.ToString();
                case AddressType.Boolean:
                    return (val.opdata == 0) ? "false" : "true";
                case AddressType.String:
                    if (forPrint)
                        return GetStringTrace((int)val.opdata, heap);
                    else
                        return "\"" + GetStringTrace((int)val.opdata, heap) + "\"";                    
                case AddressType.Char:
                    Char character = ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(val.opdata);
                    if (forPrint)
                        return character.ToString();
                    else
                        return "'" + character + "'";
                default:
                    return "null"; // "Value not yet supported for tracing";
            }
        }

        private string GetStringTrace(int strptr, Heap heap)
        {
            string str = "";
            for (int n = 0; n < heap.Heaplist[strptr].VisibleSize; ++n)
            {
                if (heap.Heaplist[strptr].Stack[n].optype != AddressType.Char)
                {
                    break;
                }
                str += ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(heap.Heaplist[strptr].Stack[n].opdata);
            }
            return str;
        }

        public string GetClassTrace(StackValue val, Heap heap, int langblock, bool forPrint)
        {
            if (!formatParams.ContinueOutputTrace())
                return "...";

            ClassTable classTable = MirrorTarget.rmem.Executable.classTable;

            int classtype = (int)val.metaData.type;
            if (classtype < 0 || (classtype >= classTable.ClassNodes.Count))
            {
                formatParams.RestoreOutputTraceDepth();
                return string.Empty;
            }

            ClassNode classnode = classTable.ClassNodes[classtype];
            if (classnode.IsImportedClass)
            {
                var helper = DLLFFIHandler.GetModuleHelper(FFILanguage.CSharp);
                var marshaller = helper.GetMarshaller(core);
                var strRep = marshaller.GetStringValue(val);
                formatParams.RestoreOutputTraceDepth();
                return strRep;
            }
            else
            {
                int ptr = (int)val.opdata;
                HeapElement hs = heap.Heaplist[ptr];

                List<string> visibleProperties = null;
                if (null != propertyFilter)
                {
                    if (!propertyFilter.TryGetValue(classnode.name, out visibleProperties))
                        visibleProperties = null;
                }

                StringBuilder classtrace = new StringBuilder();
                if (classnode.symbols != null && classnode.symbols.symbolList.Count > 0)
                {
                    bool firstPropertyDisplayed = false;
                    for (int n = 0; n < hs.VisibleSize; ++n)
                    {
                        SymbolNode symbol = classnode.symbols.symbolList[n];
                        string propName = symbol.name;

                        if ((null != visibleProperties) && visibleProperties.Contains(propName) == false)
                            continue; // This property is not to be displayed.

                        if (false != firstPropertyDisplayed)
                            classtrace.Append(", ");

                        string propValue = "";
                        if (symbol.isStatic)
                        {
                            StackValue staticProp = this.core.Rmem.GetStackData(langblock, symbol.symbolTableIndex, Constants.kGlobalScope);
                            propValue = GetStringValue(staticProp, heap, langblock, forPrint);
                        }
                        else
                        {
                            propValue = GetStringValue(hs.Stack[symbol.index], heap, langblock, forPrint);
                        }
                        classtrace.Append(string.Format("{0} = {1}", propName, propValue));
                        firstPropertyDisplayed = true;
                    }
                }
                else
                {
                    for (int n = 0; n < hs.VisibleSize; ++n)
                    {
                        if (0 != n)
                            classtrace.Append(", ");

                        classtrace.Append(GetStringValue(hs.Stack[n], heap, langblock, forPrint));
                    }
                }

                formatParams.RestoreOutputTraceDepth();
                if (classtype >= (int)ProtoCore.PrimitiveType.kMaxPrimitives)
                    if (forPrint)
                        return (string.Format("{0}{{{1}}}", classnode.name, classtrace.ToString()));
                    else
                    {
                        string tempstr =  (string.Format("{0}({1})", classnode.name, classtrace.ToString()));
                        return tempstr;
                    }

                return classtrace.ToString();
            }
        }

        private string GetPointerTrace(int ptr, Heap heap, int langblock, HashSet<int> pointers, bool forPrint)
        {
            if (pointers.Contains(ptr))
            {
                return "{ ... }";
            }
            else
            {
                pointers.Add(ptr);

                if (forPrint)
                {
                    return "{" + GetArrayTrace(ptr, heap, langblock, pointers, forPrint) + "}";
                }
                else
                {
                    return "{ " + GetArrayTrace(ptr, heap, langblock, pointers, forPrint) + " }";
                }
            }
        }

        private string GetArrayTrace(int pointer, Heap heap, int langblock, HashSet<int> pointers, bool forPrint)
        {
            if (!formatParams.ContinueOutputTrace())
                return "...";

            StringBuilder arrayElements = new StringBuilder();
            HeapElement hs = heap.Heaplist[pointer];

            int halfArraySize = -1;
            if (formatParams.MaxArraySize > 0) // If the caller did specify a max value...
            {
                // And our array is larger than that max value...
                if (hs.VisibleSize > formatParams.MaxArraySize)
                    halfArraySize = (int)Math.Floor(formatParams.MaxArraySize * 0.5);
            }

            int totalElementCount = hs.VisibleSize + (hs.Dict == null ? 0 : hs.Dict.Count());

            for (int n = 0; n < hs.VisibleSize; ++n)
            {
                // As we try to output the next element in the array, there 
                // should be a comma if there were previously output element.
                if (arrayElements.Length > 0)
                    if(forPrint)
                        arrayElements.Append(",");
                    else
                        arrayElements.Append(", ");

                StackValue sv = hs.Stack[n];
                if (sv.optype == AddressType.ArrayPointer)
                {
                    arrayElements.Append(GetPointerTrace((int)sv.opdata, heap, langblock, pointers, forPrint));
                }
                else
                {
                    arrayElements.Append(GetStringValue(hs.Stack[n], heap, langblock, forPrint));
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

            if (hs.Dict != null)
            {
                int startIndex = (halfArraySize > 0) ? hs.Dict.Count - halfArraySize : 0;
                int index = -1;

                foreach (var keyValuePair in hs.Dict)
                {
                    index++;
                    if (index < startIndex)
                    {
                        continue;
                    }

                    if (arrayElements.Length > 0)
                    {
                        if (forPrint)
                        {
                            arrayElements.Append(",");
                        }
                        else
                        {
                            arrayElements.Append(", ");
                        }
                    }

                    StackValue key = keyValuePair.Key;
                    StackValue value = keyValuePair.Value;

                    if (key.optype == AddressType.ArrayPointer)
                    {
                        arrayElements.Append(GetPointerTrace((int)key.opdata, heap, langblock, pointers, forPrint));
                    }
                    else
                    {
                        arrayElements.Append(GetStringValue(key, heap, langblock, forPrint));
                    }

                    arrayElements.Append("=");

                    if (value.optype == AddressType.ArrayPointer)
                    {
                        arrayElements.Append(GetPointerTrace((int)value.opdata, heap, langblock, pointers, forPrint));
                    }
                    else
                    {
                        arrayElements.Append(GetStringValue(value, heap, langblock, forPrint));
                    }
                }
            }

            formatParams.RestoreOutputTraceDepth();
            return arrayElements.ToString();
        }

        /*
        private string GetArrayTrace(int pointer, Heap heap, int langblock)
        {
            StringBuilder arrayelements = new StringBuilder();
            HeapElement hs = heap.heaplist[pointer];
            for (int n = 0; n < hs.visibleSize; ++n)
            {
                arrayelements.Append(GetStringValue(hs.stack[n], heap, langblock));
                if (n < hs.visibleSize - 1)
                {
                    arrayelements.Append(", ");
                }
            }
            return arrayelements.ToString();
        }
        */

        private string GetGlobalVarTrace(List<string> variableTraces)
        {
            // Prints out the final Value of every symbol in the program
            // Traverse order:
            //  Exelist, Globals symbols

            StringBuilder globaltrace = null;
            if (null == variableTraces)
                globaltrace = new StringBuilder();

            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            // Only display symbols defined in the default top-most langauge block;
            // Otherwise garbage information may be displayed.
            if (exe.runtimeSymbols.Length > 0)
            {
                int blockId = 0;

                // when this code block is of type construct, such as if, else, while, all the symbols inside are local
                //if (exe.instrStreamList[blockId] == null) 
                //    continue;

                SymbolTable symbolTable = exe.runtimeSymbols[blockId];
                for (int i = 0; i < symbolTable.symbolList.Count; ++i)
                {
                    formatParams.ResetOutputDepth();
                    SymbolNode symbolNode = symbolTable.symbolList[i];

                    bool isLocal = Constants.kGlobalScope != symbolNode.functionIndex;
                    bool isStatic = (symbolNode.classScope != Constants.kInvalidIndex && symbolNode.isStatic);
                    if (symbolNode.isArgument || isLocal || isStatic || symbolNode.isTemp)
                    {
                        // These have gone out of scope, their values no longer exist
                        continue;
                    }

                    RuntimeMemory rmem = MirrorTarget.rmem;
                    StackValue sv = rmem.GetStackData(blockId, i, Constants.kGlobalScope);
                    string formattedString = GetFormattedValue(symbolNode.name, GetStringValue(sv, rmem.Heap, blockId));

                    if (null != globaltrace)
                    {
                        int maxLength = 1020;
                        while (formattedString.Length > maxLength)
                        {
                            globaltrace.AppendLine(formattedString.Substring(0, maxLength));
                            formattedString = formattedString.Remove(0, maxLength);
                        }

                        globaltrace.AppendLine(formattedString);
                    }

                    if (null != variableTraces)
                        variableTraces.Add(formattedString);
                }

                formatParams.ResetOutputDepth();
            }

            return ((null == globaltrace) ? string.Empty : globaltrace.ToString());
        }

        public string GetCoreDump()
        {
            formatParams = new OutputFormatParameters();
            return GetGlobalVarTrace(null);
        }

        public void GetCoreDump(out List<string> variableTraces, int maxArraySize, int maxOutputDepth)
        {
            variableTraces = new List<string>();
            formatParams = new OutputFormatParameters(maxArraySize, maxOutputDepth);
            GetGlobalVarTrace(variableTraces);
        }

        private int GetSymbolIndex(string name, out int ci, ref int block, out SymbolNode symbol)
        {
            ProtoCore.DSASM.Executable exe = core.Rmem.Executable;

            int functionIndex = Constants.kGlobalScope;
            ci = Constants.kInvalidIndex;
            int functionBlock = Constants.kGlobalScope;
            
            if (core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.FepRun))
            {
                ci = core.watchClassScope = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexClass)).opdata;
                functionIndex = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunction)).opdata;
                functionBlock = (int)core.Rmem.GetAtRelative(core.Rmem.GetStackIndex(ProtoCore.DSASM.StackFrame.kFrameIndexFunctionBlock)).opdata;
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
                ClassNode classnode = core.Rmem.Executable.classTable.ClassNodes[ci];

                if (functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex && functionBlock != core.RunningBlock)
                {
                    index = exe.runtimeSymbols[block].IndexOf(name, Constants.kGlobalScope, Constants.kGlobalScope);
                }

                if (index == Constants.kInvalidIndex)
                {
                    index = classnode.symbols.IndexOfClass(name, ci, functionIndex);
                }

                if (index != Constants.kInvalidIndex)
                {
                    if (classnode.symbols.symbolList[index].arraySizeList != null)
                    {
                        throw new NotImplementedException("{C5877FF2-968D-444C-897F-FE83650D5201}");
                    }
                    symbol = classnode.symbols.symbolList[index];
                    return index;
                }
            }
            else
            {
                CodeBlock searchBlock = core.CompleteCodeBlockList[block];

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
                    if (exe.runtimeSymbols[searchBlock.codeBlockId].symbolList[index].arraySizeList != null)
                    {
                        throw new NotImplementedException("{C5877FF2-968D-444C-897F-FE83650D5201}");
                    }
                    block = searchBlock.codeBlockId;
                    symbol = exe.runtimeSymbols[searchBlock.codeBlockId].symbolList[index];
                    return index;
                }

                //if (block == 0)
                //{
                //    for (block = 0; block < exe.runtimeSymbols.Length; ++block)
                //    {
                //        index = exe.runtimeSymbols[block].IndexOf(name, ci, functionIndex);

                //        if (index != -1)
                //            break;
                //    }
                //}
                //else
                //{
                //    while (block >= 0)
                //    {
                //        index = exe.runtimeSymbols[block].IndexOf(name, ci, functionIndex);
                //        if (index != -1)
                //            break;
                //        else
                //            block--;

                //    }
                //}
            }
            throw new NameNotFoundException { Name = name };

            //throw new NotImplementedException("{F5ACC95F-AEC9-486D-BC82-FF2CB26E7E6A}"); //@TODO(Luke): Replace this with a symbol lookup exception
        }

        public string GetType(string name)
        {
            RuntimeMemory rmem = core.Rmem;

            int classcope;
            int block = core.RunningBlock;
            SymbolNode symbol;
            int index = GetSymbolIndex(name, out classcope, ref block, out symbol);

            StackValue val;
            if (symbol.functionIndex == -1 && classcope != Constants.kInvalidIndex)
                val = rmem.GetMemberData(index, classcope);                
            else
                val = rmem.GetStackData(block, index, classcope);                

            switch (val.optype)
            {
                case AddressType.Int:
                    return "int";
                case AddressType.Double:
                    return "double";
                case AddressType.Null:
                    return "null";
                case AddressType.Pointer:
                    {
                        int classtype = (int)val.metaData.type;
                        ClassNode classnode = rmem.Executable.classTable.ClassNodes[classtype];
                        return classnode.name;
                    }
                case AddressType.ArrayPointer:
                    return "array";
                case AddressType.Boolean:
                    return "bool";
                case AddressType.String:
                    return "string";
                default:
                    return "null"; // "Value not yet supported for tracing";
            }
        }

        // overload version of GetType which takes an Obj instead of string for IDE debuger use
        // usually the IDE has already get an instance of Obj before they call GetType
        // there is no need to look up that symbol again
        public string GetType(Obj obj)
        {
            if (obj.DsasmValue.optype == AddressType.Pointer)
            {
                return core.ClassTable.ClassNodes[(int)obj.DsasmValue.metaData.type].name;
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
            int count = MirrorTarget.Core.watchStack.Count;
            int n = MirrorTarget.Core.watchSymbolList.FindIndex(x => { return string.Equals(x.name, Constants.kWatchResultVar); });

            if (n < 0 || n >= count)
            {
                core.watchSymbolList.Clear();
                return new Obj { Payload = null };
            }

            Obj retVal = null;
            try
            {
                StackValue sv = MirrorTarget.Core.watchStack[n];
                if (sv.optype != AddressType.Invalid)
                {
                    retVal = Unpack(MirrorTarget.Core.watchStack[n], MirrorTarget.rmem.Heap, core);
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
                core.watchSymbolList.Clear();
            }

            return retVal;
        }
        
        public Obj GetValue(string name, int block = 0, int classcope = Constants.kGlobalScope)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            int index = Constants.kInvalidIndex;
            if (block == 0)
            {
                for (block = 0; block < exe.runtimeSymbols.Length; ++block)
                {
                    index = exe.runtimeSymbols[block].IndexOf(name, classcope, Constants.kInvalidIndex);
                    if (index != Constants.kInvalidIndex)
                        break;
                }
            }
            else
            {
                index = exe.runtimeSymbols[block].IndexOf(name, classcope, Constants.kInvalidIndex);
            }

            if (Constants.kInvalidIndex == index)
            {
                throw new SymbolNotFoundException(name);
            }
            else
            {
                if (exe.runtimeSymbols[block].symbolList[index].arraySizeList != null)
                {
                    throw new NotImplementedException("{C5877FF2-968D-444C-897F-FE83650D5201}");
                }

                RuntimeMemory rmem = MirrorTarget.rmem;
                Obj retVal = Unpack(MirrorTarget.rmem.GetStackData(block, index, classcope), MirrorTarget.rmem.Heap, core);

                return retVal;

            }
        }

        public void UpdateValue(int line, int index, int value)
        {
        }

        public void UpdateValue(int line, int index, double value)
        {
        }

        [Obsolete]
        private bool __Set_Value(string varName, int? value)
        {
            int blockId = 0;
            AssociativeGraph.GraphNode graphNode = MirrorTarget.GetFirstGraphNode(varName, out blockId);

            // There was no variable to set
            if (null == graphNode)
            {
                return false;
            }

            int index = graphNode.updateNodeRefList[0].nodeList[0].symbol.index;
            graphNode.isDirty = true;
            int startpc = graphNode.updateBlock.startpc;
            MirrorTarget.Modify_istream_entrypoint_FromSetValue(blockId, startpc);

            StackValue sv;
            if (null == value)
            {
                sv = StackValue.Null;
            }
            else
            {
                sv = StackValue.BuildInt((long)value);
            }
            MirrorTarget.Modify_istream_instrList_FromSetValue(blockId, startpc, sv);
            return true;
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
            MirrorTarget.Core.Rmem.Stack[globalStackIndex] = sv;

            // 4. Get all graphnpodes dependent on the symbol and mark them dirty
            nodesMarkedDirty = MirrorTarget.UpdateDependencyGraph(graphNode.exprUID, ProtoCore.DSASM.Constants.kInvalidIndex, false, graphNode);


            // 5. Re-execute the script - re-execution occurs after this call 

            return true;
        }

        public void NullifyVariable(string varName)
        {
            if (!string.IsNullOrEmpty(varName))
            {
                int nodesMarkedDirty = 0;
                SetValue(varName, null, out nodesMarkedDirty);
            }
        }

        public void NullifyVariableAndExecute(string varName)
        {
            if (!string.IsNullOrEmpty(varName))
            {
                SetValueAndExecute(varName, null);
            }
        }

        /// <summary>
        /// Reset an existing value and re-execute the vm
        /// </summary>
        /// <param name="varName"></param>
        /// <param name="value"></param>
        public void SetValueAndExecute(string varName, int? value)
        {
            core.Options.IsDeltaExecution = true;
            int nodesMarkedDirty = 0;
            bool wasSet = SetValue(varName, value, out nodesMarkedDirty);

            if (wasSet && nodesMarkedDirty > 0)
            {
                try
                {
                    foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                    {
                        ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();

                        ProtoCore.DSASM.StackFrame stackFrame = new ProtoCore.DSASM.StackFrame(core.GlobOffset);
                        int locals = 0;

                        // Comment Jun: Tell the new bounce stackframe that this is an implicit bounce
                        // Register TX is used for this.
                        StackValue svCallConvention = StackValue.BuildCallingConversion((int)ProtoCore.DSASM.CallingConvention.BounceType.kImplicit);
                        stackFrame.SetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kRegisterTX, svCallConvention);

                        core.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, context, stackFrame, locals, new ProtoCore.DebugServices.ConsoleEventSink());
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
            int block = core.GetCurrentBlockId();
            
            RuntimeMemory rmem = core.Rmem;
            SymbolNode symbol;
            int index = GetSymbolIndex(name, out classcope, ref block, out symbol);
            StackValue sv;
            if (symbol.functionIndex == -1 && classcope != Constants.kInvalidIndex)
                sv = rmem.GetMemberData(index, classcope);
            else
                sv = rmem.GetStackData(block, index, classcope);

            if (sv.optype == AddressType.Invalid)
                throw new UninitializedVariableException { Name = name };

            return Unpack(sv);
        }

        // traverse an class type object to get its property
        public Dictionary<string, Obj> GetProperties(Obj obj, bool excludeStatic = false)
        {
            if (obj == null || obj.DsasmValue.optype != AddressType.Pointer)
                return null;

            Dictionary<string, Obj> ret = new Dictionary<string, Obj>();
            int classIndex = (int)obj.DsasmValue.metaData.type;
            IDictionary<int,SymbolNode> symbolList = core.ClassTable.ClassNodes[classIndex].symbols.symbolList;
            StackValue[] svs = core.Heap.Heaplist[(int)obj.DsasmValue.opdata].Stack;
            int index = 0;
            for (int ix = 0; ix < svs.Length; ++ix)
            {
                if (excludeStatic && symbolList[ix].isStatic)
                    continue;
                string name = symbolList[ix].name;
                StackValue val = svs[index];

                // check if the members are primitive type
                if (val.optype == AddressType.Pointer &&
                    core.Heap.Heaplist[(int)val.opdata].Stack.Length == 1 &&
                    core.Heap.Heaplist[(int)val.opdata].Stack[0].optype != AddressType.Pointer &&
                    core.Heap.Heaplist[(int)val.opdata].Stack[0].optype != AddressType.ArrayPointer)
                    val = core.Heap.Heaplist[(int)val.opdata].Stack[0];

                ret[name] = Unpack(val);
                index++;
            }

            return ret;
        }

        public List<string> GetPropertyNames(Obj obj)
        {
            if (obj == null || obj.DsasmValue.optype != AddressType.Pointer)
                return null;

            List<string> ret = new List<string>();
            int classIndex = (int)obj.DsasmValue.metaData.type;

            StackValue[] svs = core.Heap.Heaplist[(int)obj.DsasmValue.opdata].Stack;
            for (int ix = 0; ix < svs.Length; ++ix)
            {
                string propertyName = core.ClassTable.ClassNodes[classIndex].symbols.symbolList[ix].name;
                ret.Add(propertyName);
            }

            return ret;
        }

        // traverse an array Obj return its member
        public List<Obj> GetArrayElements(Obj obj)
        {
            if ( obj == null || obj.DsasmValue.optype != AddressType.ArrayPointer)
                return null;

            return core.Heap.Heaplist[(int)obj.DsasmValue.opdata].Stack.Select(x => Unpack(x)).ToList();
        }

        public StackValue GetGlobalValue(string name, int startBlock = 0)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            for (int block = startBlock; block < exe.runtimeSymbols.Length; block++)
            {
                int index = exe.runtimeSymbols[block].IndexOf(name, Constants.kInvalidIndex, Constants.kGlobalScope);
                if (Constants.kInvalidIndex != index)
                {
                    //Q(Luke): This seems to imply that the LHS is an array index?
                    if (exe.runtimeSymbols[block].symbolList[index].arraySizeList != null)
                    {
                        throw new NotImplementedException("{C5877FF2-968D-444C-897F-FE83650D5202}");
                    }

                    RuntimeMemory rmem = MirrorTarget.rmem;

                    SymbolNode symNode = exe.runtimeSymbols[block].symbolList[index];
                    if (symNode.absoluteFunctionIndex == Constants.kGlobalScope)
                    {
                        return MirrorTarget.rmem.GetAtRelative(symNode.index);
                    }
                }
            }
            return StackValue.Null;
        }

        public StackValue GetRawFirstValue(string name, int startBlock = 0, int classcope = Constants.kGlobalScope)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            for (int block = startBlock; block < exe.runtimeSymbols.Length; block++)
            {
                int index = exe.runtimeSymbols[block].IndexOf(name, classcope, Constants.kGlobalScope);
                if (Constants.kInvalidIndex != index)
                {
                    //Q(Luke): This seems to imply that the LHS is an array index?
                    if (exe.runtimeSymbols[block].symbolList[index].arraySizeList != null)
                    {
                        throw new NotImplementedException("{C5877FF2-968D-444C-897F-FE83650D5201}");
                    }

                    RuntimeMemory rmem = MirrorTarget.rmem;

                    return MirrorTarget.rmem.GetStackData(block, index, classcope);
                }
            }
            throw new NotImplementedException("{F5ACC95F-AEC9-486D-BC82-FF2CB26E7E6A}"); //@TODO(Luke): Replace this with a symbol lookup exception
        }

        public string GetFirstNameFromValue(StackValue v)
        {
            if (v.optype != AddressType.Pointer)
                throw new ArgumentException("SV to highlight must be a pointer");

            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            List<SymbolNode> symNodes = new List<SymbolNode>();

            foreach (SymbolTable symTable in exe.runtimeSymbols)
            {
                foreach (SymbolNode symNode in symTable.symbolList.Values)
                {
                    symNodes.Add(symNode);
                }

            }


            int index = MirrorTarget.rmem.Stack.FindIndex(0, value => value.opdata == v.opdata);

            List<SymbolNode> matchingNodes = symNodes.FindAll(value => value.index == index);

            if (matchingNodes.Count > 0)
                return matchingNodes[0].name;
            else
            {
                return null;
            }
        }


        public Obj GetFirstValue(string name, int startBlock = 0, int classcope = Constants.kGlobalScope)
        {
            ProtoCore.DSASM.Executable exe = MirrorTarget.rmem.Executable;

            Obj retVal = Unpack(GetRawFirstValue(name, startBlock, classcope), MirrorTarget.rmem.Heap, core);
            return retVal;
        }


        //@TODO(Luke): Add in the methods here that correspond to each of the internal datastructures in use by the executive
        //@TODO(Jun): if this method stays static, then the Heap needs to be referenced from a parameter
        /// <summary>
        /// Do the recursive unpacking of the data structure into mirror objects
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Obj Unpack(StackValue val, Heap heap, Core core, int type = (int)PrimitiveType.kTypePointer) 
        {
            switch (val.optype)
            {
                case AddressType.ArrayPointer:
                case AddressType.String:
                    {
                        //It was a pointer that we pulled, so the value lives on the heap
                        Int64 ptr = val.opdata;

                        DsasmArray ret = new DsasmArray();

                        //Pull the item out of the heap


                        HeapElement hs = heap.Heaplist[(int)ptr];

                        StackValue[] nodes = hs.Stack;
                        ret.members = new Obj[hs.VisibleSize];

                        for (int i = 0; i < ret.members.Length; i++)
                        {
                            ret.members[i] = Unpack(nodes[i], heap, core, type);
                        }

                        // TODO Jun: ret.members[0] is hardcoded  and means we are assuming a homogenous collection
                        // How to handle mixed-type arrays?
                        Obj retO = new Obj(val) { Payload = ret, Type = core.TypeSystem.BuildTypeObject((ret.members.Length > 0) ? core.TypeSystem.GetType(ret.members[0].Type.Name) : (int)ProtoCore.PrimitiveType.kTypeVoid, true) };

                        return retO;
                    }
                case AddressType.Int:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, false) };
                        return o;
                    }
                case AddressType.Boolean:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = (data != 0), Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, false) };
                        return o;
                    }

                case AddressType.Null:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = null, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeNull, false) };
                        return o;
                    }
                case AddressType.Char:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeChar, false) };
                        return o;
                    }
                case AddressType.Double:
                    {
                        double data = val.opdata_d;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, false) };
                        return o;
                    }
                case AddressType.Pointer:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = core.TypeSystem.BuildTypeObject(type, false) };
                        return o;
                    }
                case AddressType.FunctionPointer:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, false) };
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

        public static Obj Unpack(StackValue val, Core core)
        {
            switch (val.optype)
            {
                case AddressType.ArrayPointer:
                    {
                        //It was a pointer that we pulled, so the value lives on the heap
                        Int64 ptr = val.opdata;

                        DsasmArray ret = new DsasmArray();
                        HeapElement hs = core.Heap.Heaplist[(int)ptr];

                        StackValue[] nodes = hs.Stack;
                        ret.members = new Obj[nodes.Length];

                        for (int i = 0; i < ret.members.Length; i++)
                        {
                            ret.members[i] = Unpack(nodes[i], core);
                        }

                        Obj retO = new Obj(val) { Payload = ret, Type = core.TypeSystem.BuildTypeObject((ret.members.Length > 0) ? core.TypeSystem.GetType(ret.members[0].Type.Name) : (int)ProtoCore.PrimitiveType.kTypeVar, true) };

                        return retO;
                    }
                case AddressType.Int:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, false) };
                        return o;
                    }
                case AddressType.Boolean:
                    {
                        Obj o = new Obj(val) { Payload = val.opdata == 0 ? false : true, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, false) };
                        return o;
                    }
                case AddressType.Null:
                    {
                        Obj o = new Obj(val) { Payload = null, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeNull, false) };
                        return o;
                    }
                case AddressType.Double:
                    {
                        double data = val.opdata_d;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, false) };
                        return o;
                    }
                case AddressType.Char:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeChar, false) };
                        return o;
                    }
                case AddressType.Pointer:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = core.TypeSystem.BuildTypeObject((int)val.metaData.type, false) };
                        return o;
                    }
                case AddressType.DefaultArg:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false) };
                        return o;
                    }
                case AddressType.FunctionPointer:
                    {
                        Int64 data = val.opdata;
                        Obj o = new Obj(val) { Payload = data, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, false) };
                        return o;
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
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypePointer, false) };
                    break;
                case AddressType.ArrayPointer:
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeArray, true) };
                    break;
                case AddressType.Int:
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeInt, false) };
                    break;
                case AddressType.Boolean:
                    obj = new Obj(val) { Payload = val.opdata == 0 ? false : true, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeBool, false) };
                    break;
                case AddressType.Double:
                    obj = new Obj(val) { Payload = val.opdata_d, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeDouble, false) };
                    break;
                case AddressType.Null:
                    obj = new Obj(val) { Payload = null, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeNull, false) };
                    break;
                case AddressType.FunctionPointer:
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeFunctionPointer, false) };
                    break;
                case AddressType.String:
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeString, true, Constants.kPrimitiveSize) };
                    break;
                case AddressType.Char:
                    obj = new Obj(val) { Payload = val.opdata, Type = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeChar, false) };
                    break;
            }

            return obj;
        }

        public static StackValue Repack(Obj obj, ProtoCore.DSASM.Heap heap)
        {
            if (obj.Type.IsIndexable)
            {
                //Unpack each of the elements
                DsasmArray arr = (DsasmArray)obj.Payload;

                StackValue[] sv = new StackValue[arr.members.Length];

                //recurse over the array
                for (int i = 0; i < sv.Length; i++)
                    sv[i] = Repack(arr.members[i], heap);

                int size = sv.Length;

                lock (heap.cslock)
                {
                    int ptr = heap.Allocate(size);
                    ++heap.Heaplist[ptr].Refcount;
                    for (int n = size - 1; n >= 0; --n)
                    {
                        heap.Heaplist[ptr].Stack[n] = sv[n];
                    }

                    StackValue overallSv = StackValue.BuildArrayPointer(ptr);

                    return overallSv;
                }
            }

            // For non-arrays, there is nothing to repack so just return the original stackvalue
            return obj.DsasmValue;
        }

        public bool CompareArrays(DsasmArray dsArray, List<Object> expected, System.Type type)
        {
            if (dsArray.members.Length != expected.Count)
                return false;

            for (int i = 0; i < dsArray.members.Length; ++i)
            {
                List<Object> subExpected = expected[i] as List<Object>;
                DsasmArray subArray = dsArray.members[i].Payload as DsasmArray;

                if ((subExpected != null) && (subArray != null)) {

                    if (!CompareArrays(subArray, subExpected, type))
                        return false;
                }
                else if ((subExpected == null) && (subArray == null))
                {
                    if (type == typeof(Int64))
                    {
                        if (Convert.ToInt64(dsArray.members[i].Payload) != Convert.ToInt64(expected[i]))
                            return false;
                    }
                    else if (type == typeof(Double))
                    {
                        // can't use Double.Episilion, according to msdn, it is smaller than most
                        // errors.
                        if (Math.Abs(Convert.ToDouble(dsArray.members[i].Payload) - Convert.ToDouble(expected[i])) > 0.000001)
                            return false;
                    }
                    else if (type == typeof(Boolean))
                    {
                        if (Convert.ToBoolean(dsArray.members[i].Payload) != Convert.ToBoolean(expected[i]))
                            return false;
                    }
                    else if (type == typeof(Char))
                    {
                        object payload = dsArray.members[i].Payload;
                        return ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(Convert.ToInt64(payload)) == Convert.ToChar(expected[i]);
                    }
                    else
                    {
                        throw new NotImplementedException("Test comparison not implemented: {EBAFAE6C-BCBF-42B8-B99C-49CFF989F0F0}");
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool CompareArrays(string mirrorObj, List<Object> expected, System.Type type, int blockIndex = 0)
        {
            DsasmArray computedArray = GetValue(mirrorObj, blockIndex).Payload as DsasmArray;
            return CompareArrays(computedArray, expected, type);
        }

        public bool EqualDotNetObject(Obj dsObj, object dotNetObj)
        {
            // check for null first
            if (dotNetObj == null)
            {
                if (dsObj.DsasmValue.optype == AddressType.Null)
                    return true;
                else
                    return false;
            }

            System.Type t = dotNetObj.GetType();
            switch (dsObj.DsasmValue.optype)
            {
                case AddressType.ArrayPointer:
                    if (t.IsArray)
                    {
                        object[] dotNetValue = (object[])dotNetObj;
                        Obj[] dsValue = GetArrayElements(dsObj).ToArray();

                        if (dotNetValue.Length == dsValue.Length)
                        {
                            for (int ix = 0; ix < dsValue.Length; ++ix)
                            {
                                if (!EqualDotNetObject(dsValue[ix], dotNetValue[ix]))
                                    return false;
                            }
                            return true;
                        }
                    }
                    return false;
                case AddressType.Int:
                    if (dotNetObj is int)
                        return (Int64)dsObj.Payload == (int)dotNetObj;
                    else
                        return false;
                case AddressType.Double:
                    if (dotNetObj is double)
                        return (Double)dsObj.Payload == (Double)dotNetObj;
                    else
                        return false;
                case AddressType.Boolean:
                    if (dotNetObj is bool)
                        return (Boolean)dsObj.Payload == (Boolean)dotNetObj;
                    else
                        return false;
                case AddressType.Pointer:
                    if (t == typeof(Dictionary<string, Object>))
                    {
                        Dictionary<string, Obj> dsProperties = GetProperties(dsObj);
                        foreach (KeyValuePair<string, object> dotNetProperty in dotNetObj as Dictionary<string, object>)
                        {
                            if (!(dsProperties.ContainsKey(dotNetProperty.Key) && EqualDotNetObject(dsProperties[dotNetProperty.Key], dotNetProperty.Value)))
                                return false;
                        }
                        return true;
                    }
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        //
        public void Verify(string dsVariable, object expectedValue, int startBlock = 0)
        {
            try
            {
                Obj dsObj = this.GetFirstValue(dsVariable, startBlock);
                var indices = new List<int>();
                //VerifyInternal(expectedValue, dsObj, dsVariable, indices);
            }
            catch (NotImplementedException)
            {
                throw new NotImplementedException();
            }
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

        internal void ResetOutputDepth()
        {
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
        internal int MaxOutputDepth { get { return maximumDepth; } }
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

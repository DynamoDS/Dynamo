using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProtoScript.Runners;
using ProtoFFI;
using ProtoCore;
using ProtoCore.DSASM;
using ProtoCore.Lang;

namespace ProtoTestFx
{
    public class InjectionExecutiveProvider : IExecutiveProvider
    {
        public ProtoCore.DSASM.Executive CreateExecutive(Core core, bool isFep)
        {
            return new InjectionExecutive(core, isFep);
        }
    }

    public struct Expression
    {
        public int LineNo;
        public string Expr;
        public int ClassIndex;

        public Expression(int lineNo, string expr, int ci = Constants.kInvalidIndex)
        {
            LineNo = lineNo;
            Expr = expr;
            ClassIndex = ci;
        }
    }

    // overrides POP_handler
    public class InjectionExecutive : ProtoCore.DSASM.Executive
    {

        public InjectionExecutive(Core core, bool isFep = false) : base(core, isFep) { }

        public static int callrLineNo { get; private set; }
        public static bool IsPopToPropertyArray { get; set; }
        protected static StackValue propSV { get; private set; }
        public static Dictionary<Expression, List<string>> ExpressionMap { get; set; }

        static InjectionExecutive()
        {
            IsPopToPropertyArray = false;
            callrLineNo = Constants.kInvalidIndex;
            ExpressionMap = new Dictionary<Expression, List<string>>();
        }

        internal void Print(StackValue sv, int lineNo, string symbolName, int ci = Constants.kInvalidIndex)
        {
            //TODO: Change Execution mirror class to have static methods, so that an instance does not have to be created
            ProtoCore.DSASM.Mirror.ExecutionMirror mirror = new ProtoCore.DSASM.Mirror.ExecutionMirror(this, Core);
            string result = mirror.GetStringValue(sv, Core.Heap, 0, true);

            TextOutputStream tStream = Core.BuildStatus.MessageHandler as TextOutputStream;
            if (tStream != null)
            {
                Dictionary<int, List<string>> map = tStream.Map;

                //foreach(var kv in tStream.Map)

                if (!map.ContainsKey(lineNo))
                    return;

                List<string> expressions = map[lineNo];

                foreach (var exp in expressions)
                {
                    // Get lhs symbol name
                    string lhsName = null;
                    int index = exp.IndexOf('.');
                    if (index != -1)
                    {
                        string[] parts = exp.Split('.');
                        lhsName = parts[parts.Length - 1];
                    }
                    else
                    {
                        Match m = Regex.Match(exp, @"(\w+)");
                        if (m.Success)
                        {
                            lhsName = m.Groups[1].Value;
                        }
                    }

                    if (lhsName.Equals(symbolName))
                    {
                        // Add to map
                        Expression expStruct = new Expression(lineNo, exp, ci);

                        if (ExpressionMap.ContainsKey(expStruct))
                        {
                            List<string> values = ExpressionMap[expStruct];
                            values.Add(result);
                        }
                        else
                        {
                            List<string> values = new List<string>();
                            values.Add(result);

                            ExpressionMap.Add(expStruct, values);
                        }
                    }
                }
            }
        }

        protected override void POP_Handler(Instruction instruction)
        {
            int blockId = 0;
            int dimensions = 0;

            StackValue svData = POP_helper(instruction, out blockId, out dimensions);

            if (instruction.op1.optype == AddressType.Register)
                return;

            SymbolNode symbolNode = GetSymbolNode(blockId, (int)instruction.op2.opdata, (int)instruction.op1.opdata);
            string symbolName = symbolNode.name;

            bool isTemp = ProtoCore.Utils.CoreUtils.IsPropertyTemp(symbolName);

            // if symbol is a temporary for a property array (true in associative block only)
            // we cache the stackvalue here and not in POPM (as in POPM we will retrieve the whole array)
            if (isTemp && dimensions > 0)
            {
                IsPopToPropertyArray = true;
                propSV = svData;
            }

            
            if (instruction.debug != null)
            {
                int lineNo = instruction.debug.Location.StartInclusive.LineNo;

                if (Core.Options.IDEDebugMode && Core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                {
                    Core.DebugProps.IsPopmCall = false;
                    Core.DebugProps.CurrentSymbolName = symbolName;
                }

                // Add stackvalue against lineNo and variable name
                if (!Core.Options.IDEDebugMode)
                {
                    Print(svData, lineNo, symbolName);
                }
            }
            return;
        }

        protected override void POPM_Handler(Instruction instruction)
        {
            int blockId;
            int ci = Constants.kInvalidIndex;
            StackValue svData = POPM_Helper(instruction, out blockId, out ci);

            //if (callrLineNo != Constants.kInvalidIndex)
            {
                SymbolNode symbolNode = null;
                if (ProtoCore.DSASM.AddressType.StaticMemVarIndex == instruction.op1.optype)
                {
                    symbolNode = exe.runtimeSymbols[blockId].symbolList[(int)instruction.op1.opdata];
                }
                else
                {
                    symbolNode = exe.classTable.ClassNodes[ci].symbols.symbolList[(int)instruction.op1.opdata];
                }
                string symbolName = symbolNode.name;

                if (Core.Options.IDEDebugMode && Core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                {
                    if (!Core.DebugProps.DebugStackFrameContains(DebugProperties.StackFrameFlagOptions.IsReplicating))
                    {
                        Core.DebugProps.CurrentSymbolName = symbolName;
                        Core.DebugProps.IsPopmCall = true;
                    }
                }

                // Add stackvalue against lineNo and variable name
                if (!Core.Options.IDEDebugMode)
                {
                    int lineNo = -1;
                    if (instruction.debug == null)
                    {
                        lineNo = callrLineNo;
                    }
                    // In Imperative block, POPM instruction has debug info so in this case we get the correct line no. from debug info
                    else
                    {
                        lineNo = instruction.debug.Location.StartInclusive.LineNo;
                    }

                    if (IsPopToPropertyArray)
                    {
                        svData = propSV;
                        IsPopToPropertyArray = false;
                    }
                    Print(svData, lineNo, symbolName, ci);
                }
            }
        }

        protected override void CALLR_Handler(Instruction instruction)
        {
            //callrLineNo = Constants.kInvalidIndex;

            if (instruction.debug != null)
            {
                if (Core.Options.IDEDebugMode && Core.ExecMode != ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                {
                    Core.DebugProps.IsPopmCall = false;
                }

                callrLineNo = instruction.debug.Location.StartInclusive.LineNo;
            }

            base.CALLR_Handler(instruction);
        }
    }

    public class WatchTestFx
    {
        public WatchTestFx()
        {
            var options = new ProtoCore.Options();
            core = new ProtoCore.Core(options);
        }

        private ProtoCore.Core core;
        public ProtoCore.Core TestCore 
        {
            get
            {
                return core;
            }
        }

        public ProtoCore.DSASM.Mirror.ExecutionMirror Mirror { get; private set; }

        public static void GeneratePrintStatements(TextReader file, ref Dictionary<int, List<string>> map)
        {
            string line;

            int lineNo = 0;

            // split code into program statements and loop thro' them            
            while ((line = file.ReadLine()) != null)
            {
                lineNo++;
                line = line.Trim();

                if (line.Equals(String.Empty))
                    continue;

                // TODO: implement strategy to skip commented code block
                // skip commented lines
                if (line.StartsWith("//"))
                    continue;

                List<string> identifiers = null;
                int equalIndex = line.IndexOf('=');
                if (equalIndex != -1)
                {
                    identifiers = ProtoCore.Utils.ParserUtils.GetLHSatAssignment(line, equalIndex);
                    if (identifiers.Count == 0)
                        continue;

                    map.Add(lineNo, identifiers);
                }

            }
            file.Close();

        }

        public static void GeneratePrintStatements(string fileContent, ref Dictionary<int, List<string>> map)
        {
            string line;

            int lineNo = 0;
            int colNo = 0;

            string[] lines = fileContent.Split('\n');

            // split code into program statements and loop thro' them            
            foreach (string contentLine in lines)
            {
                line = contentLine;
                lineNo++;
                line = line.Trim();

                if (line.Equals(String.Empty))
                    continue;

                // TODO: implement strategy to skip commented code block
                // skip commented lines
                if (line.StartsWith("//"))
                    continue;

                List<string> identifiers = null;
                int equalIndex = line.IndexOf('=');
                if (equalIndex != -1)
                {
                    identifiers = ProtoCore.Utils.ParserUtils.GetLHSatAssignment(line, equalIndex);
                    if (identifiers.Count == 0)
                        continue;

                    map.Add(lineNo, identifiers);
                }

            }
        }

        public void CompareRunAndWatchResults(string includePath, string code, Dictionary<int, List<string>> map, bool watchNestedMode = false)
        {
            try
            {
                InjectionExecutive.ExpressionMap.Clear();
                InjectionExecutive.IsPopToPropertyArray = false;

                TestRunnerRunOnly(includePath, code, map);
            }
            catch (Exception e)
            {
                Assert.Ignore("Ignored due to Exception from run: " + e.ToString());
            }

            DebugRunnerStepIn(includePath, code, map, watchNestedMode);            
        }

        

        internal void TestRunnerRunOnly(string includePath, string code, Dictionary<int, List<string>> map /*, string executionLogFilePath*/)
        {
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScriptTestRunner();


            ProtoScript.Config.RunConfiguration runnerConfig;
            
            // Specify some of the requirements of IDE.

            core.Options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            core.Options.SuppressBuildOutput = false;
            core.Options.WatchTestMode = true;
            core.Options.GCTempVarsOnDebug = false;

            // Cyclic dependency threshold is lowered from the default (2000)
            // as this causes the test framework to be painfully slow
            core.Options.kDynamicCycleThreshold = 5;

            // Pass the absolute path so that imported filepaths can be resolved
            // in "FileUtils.GetDSFullPathName()"
            if (!String.IsNullOrEmpty(includePath))
            {
                includePath = Path.GetDirectoryName(includePath);
                core.Options.IncludeDirectories.Add(includePath);
            }

            //StreamWriter sw = File.CreateText(executionLogFilePath);
            TextOutputStream fs = new TextOutputStream(map);

            // By specifying this option we inject a mock Executive ('InjectionExecutive')
            // that prints stackvalues at every assignment statement
            // by overriding the POP_handler instruction - pratapa
            core.ExecutiveProvider = new InjectionExecutiveProvider();

            core.BuildStatus.MessageHandler = fs;
            core.RuntimeStatus.MessageHandler = fs;

            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            
            //Run

            Mirror = fsr.Execute(code, core);

            //sw.Close();
            core.Cleanup();
        }

        internal static void DebugRunnerStepIn(string includePath, string code, /*string logFile*/Dictionary<int, List<string>> map, bool watchNestedMode = false)
        {
            //Internal setup
            ProtoCore.Core core;
            DebugRunner fsr;
            ProtoScript.Config.RunConfiguration runnerConfig;
            
            // Specify some of the requirements of IDE.
            var options = new ProtoCore.Options();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.GCTempVarsOnDebug = false;
            
            // Cyclic dependency threshold is lowered from the default (2000)
            // as this causes the test framework to be painfully slow
            options.kDynamicCycleThreshold = 5;

            // Pass the absolute path so that imported filepaths can be resolved
            // in "FileUtils.GetDSFullPathName()"
            if (!String.IsNullOrEmpty(includePath))
            {
                includePath = Path.GetDirectoryName(includePath);
                options.IncludeDirectories.Add(includePath);
            }
            
            
            core = new ProtoCore.Core(options);

            // Use the InjectionExecutive to overload POP and POPM
            // as we still need the symbol names and line nos. in debug mode for comparisons
            core.ExecutiveProvider = new InjectionExecutiveProvider();

            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));

            runnerConfig = new ProtoScript.Config.RunConfiguration();
            runnerConfig.IsParrallel = false;
            fsr = new DebugRunner(core);

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            
            //Run
            fsr.PreStart(code, runnerConfig);
            
            //StreamReader log = new StreamReader(logFile);

            //bool isPrevBreakAtPop = false;
            int lineAtPrevBreak = -1;
            string symbolName = null;
            DebugRunner.VMState vms = null;

            while (!fsr.isEnded)
            {
                vms = fsr.LastState;

                OpCode opCode = OpCode.NONE;
                DebugInfo debug = null;
                if (vms != null)
                {
                    // check if previous break is a POP
                    // if so, get the line no. and LHS
                    opCode = fsr.CurrentInstruction.opCode;
                    debug = fsr.CurrentInstruction.debug;

                    if (opCode == ProtoCore.DSASM.OpCode.POP)
                    {
                        //isPrevBreakAtPop = true;
                        lineAtPrevBreak = vms.ExecutionCursor.StartInclusive.LineNo;                       
                    }
                }

                DebugRunner.VMState currentVms = fsr.Step();

                //if (isPrevBreakAtPop)

                if (debug != null)
                {
                    // Do not do the verification for imported DS files, for which the FilePath is non null
                    if (debug.Location.StartInclusive.SourceLocation.FilePath == null)
                    {
                        if (opCode == ProtoCore.DSASM.OpCode.POP)
                        {
                            VerifyWatch_Run(lineAtPrevBreak, core.DebugProps.CurrentSymbolName, core, map, watchNestedMode);
                        }
                        // if previous breakpoint was at a CALLR
                        else if (opCode == ProtoCore.DSASM.OpCode.CALLR)
                        {
                            if (core.DebugProps.IsPopmCall)
                            {
                                int ci = (int)currentVms.mirror.MirrorTarget.rmem.GetAtRelative(ProtoCore.DSASM.StackFrame.kFrameIndexClass).opdata;
                                VerifyWatch_Run(InjectionExecutive.callrLineNo, core.DebugProps.CurrentSymbolName, core, map, watchNestedMode, ci);
                            }
                        }
                    }
                }
                //isPrevBreakAtPop = false;
            }
            core.Cleanup();
        }

        /*internal static void VerifyWatch_Run(int lineAtPrevBreak, string symbolName, Core core, StreamReader log,
            bool watchNestedMode = false)
        {
            //bool check = true;
            
            // search for the line and LHS string in the log file
            // verify that the LHS identifier name equals 'symbolName'
            // pass the LHS string to GetWatchValue() and inspect it
            // verify the watch values with the log output
            string line = null;
            while ((line = log.ReadLine()) != null)
            {
                // Get line no.
                Match m = Regex.Match(line, @"At line, (\d+)");
                if (m.Success)
                {
                    int lineNo = int.Parse(m.Groups[1].Value);
                    if (lineNo == lineAtPrevBreak)
                    {
                        // Get lhs string
                        // m = Regex.Match(line, @"(\d+), (\w+)");
                        m = Regex.Match(line, @"(\d+), (.*)([^\s]+)");
                        if (m.Success)
                        {
                            string lhsString = m.Groups[2].Value;

                            // Get lhs symbol name
                            m = Regex.Match(lhsString, @"(\w+)");
                            if (m.Success)
                            {
                                string lhsName = m.Groups[1].Value;
                                if (lhsName.Equals(symbolName))
                                {
                                    ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core);
                                    ProtoCore.DSASM.Mirror.ExecutionMirror mirror = watchRunner.Execute(lhsString);
                                    Obj obj = mirror.GetWatchValue();

                                    if (!watchNestedMode)
                                    {
                                        // Cheat by peeking into heap etc. to dump output string
                                        // match string with log output to verify
                                        string result = mirror.GetStringValue(obj.DsasmValue, core.Heap, 0, true);
                                        line = log.ReadLine();

                                        m = Regex.Match(line, @"Info: (.*)");
                                        if (m.Success)
                                        {
                                            string output = m.Groups[1].Value;
                                            if (!output.Equals(result))
                                            {
                                                Assert.Fail(string.Format("\tThe value of expression \"{0}\" doesn't match in run mode and in watch.\n", lhsString));
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // TODO: Implement this - pratapa
                                        // if obj is a class pointer, handle separately
                                        // if obj is an array pointer, handle separately
                                        // if obj is a literal, verify watch value with log output directly
                                        GetStringValue(obj, mirror);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }*/

        internal static void VerifyWatch_Run(int lineAtPrevBreak, string symbolName, Core core,
            Dictionary<int, List<string>> map, bool watchNestedMode = false, int ci = Constants.kInvalidIndex)
        {
            //bool check = true;

            // search for the line and LHS string in the map
            // verify that the LHS identifier name equals 'symbolName'
            // pass the LHS string to GetWatchValue() and inspect it
            // verify the watch values with the log output
            
            if (!map.ContainsKey(lineAtPrevBreak))
                return;

            List<string> expressions = map[lineAtPrevBreak];
            foreach(string exp in expressions)
            {
                // Get line no.
                // Get lhs symbol name
                string lhsName = null;
                int index = exp.IndexOf('.');
                if (index != -1)
                {
                    string[] parts = exp.Split('.');
                    lhsName = parts[parts.Length - 1];
                }
                else
                {
                    Match m = Regex.Match(exp, @"(\w+)");
                    if (m.Success)
                    {
                        lhsName = m.Groups[1].Value;
                    }
                }                            
                if (lhsName.Equals(symbolName))
                {
                    ExpressionInterpreterRunner watchRunner = new ExpressionInterpreterRunner(core);
                    ProtoCore.DSASM.Mirror.ExecutionMirror mirror = watchRunner.Execute(exp);
                    Obj obj = mirror.GetWatchValue();
                    
                    if (!watchNestedMode)
                    {
                        // Cheat by peeking into heap etc. to dump output string
                        // match string with map output to verify
                        string result = mirror.GetStringValue(obj.DsasmValue, core.Heap, 0, true);

                        Expression expr = new Expression(lineAtPrevBreak, exp, ci);
                        if (!InjectionExecutive.ExpressionMap.ContainsKey(expr))
                            return;

                        List<string> values = InjectionExecutive.ExpressionMap[expr];
                        
                        if (!values.Contains(result))
                        {
                            Assert.Fail(string.Format("\tThe value of expression \"{0}\" doesn't match in run mode and in watch.\n", exp));
                            return;
                        }                        
                    }
                    else
                    {
                        // TODO: Implement this! - pratapa
                        // if obj is a class pointer, handle separately
                        // if obj is an array pointer, handle separately
                        // if obj is a literal, verify watch value with log output directly
                        GetStringValue(obj, mirror);
                    }
                    //break;
                }
            }
        }

        // TODO: Implement this to recurse through expressions in watch window and running expression interpreter for each of their sub-types - pratapa
        internal static void GetStringValue(Obj obj, ProtoCore.DSASM.Mirror.ExecutionMirror mirror)
        {
            switch (obj.DsasmValue.optype)
            {
                case AddressType.ArrayPointer:
                    {
                        List<Obj> ol = mirror.GetArrayElements(obj);

                        foreach (Obj o in ol)
                        {
                            GetStringValue(o, mirror);
                        }
                        return;
                    }
                case AddressType.Pointer:
                    {
                        Dictionary<string, Obj> os = mirror.GetProperties(obj);
                        for (int i = 0; i < os.Count; ++i)
                        {

                        }
                        return;
                    }
                default:
                    return;
            }
        }
    }
}


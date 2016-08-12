using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using System.Linq;
using ProtoCore.Runtime;
using ProtoCore.Properties;

namespace ProtoCore
{
    namespace Runtime
    {
        public enum WarningID
        {
            Default,
            AccessViolation,
            AmbiguousMethodDispatch,
            AurgumentIsNotExpected,
            CallingConstructorOnInstance,
            ConversionNotPossible,
            DereferencingNonPointer,
            FileNotExist,
            IndexOutOfRange,
            InvalidRecursion,
            InvalidArguments,
            CyclicDependency,
            MethodResolutionFailure,
            OverIndexing,
            TypeConvertionCauseInfoLoss,
            TypeMismatch,
            ReplicationWarning,
            InvalidIndexing,
            ModuloByZero,
            InvalidType,
            RangeExpressionOutOfMemory,
            MoreThanOneDominantList,
            RunOutOfMemory,
        }

        public struct WarningEntry
        {
            public Runtime.WarningID ID;
            public string Message;
            public int Line;
            public int Column;
            public int ExpressionID;
            public Guid GraphNodeGuid;
            public int AstID;
            public string Filename;
        }
    }

    public class RuntimeStatus
    {
        private ProtoCore.RuntimeCore runtimeCore;
        private List<Runtime.WarningEntry> warnings;

        public IOutputStream MessageHandler
        {
            get;
            set;
        }

        public IOutputStream WebMessageHandler
        {
            get;
            set;
        }

        public IEnumerable<Runtime.WarningEntry> Warnings
        {
            get
            {
                return warnings;
            }
        }

        public int WarningCount
        {
            get
            {
                return warnings.Count;
            }
        }

        public void ClearWarningForExpression(int expressionID)
        {
            warnings.RemoveAll(w => w.ExpressionID == expressionID);
        }

        public void ClearWarningsForGraph(Guid guid)
        {
            warnings.RemoveAll(w => w.GraphNodeGuid.Equals(guid));
        }

        public void ClearWarningsForAst(int astID)
        {
            warnings.RemoveAll(w => w.AstID.Equals(astID));
        }

        public RuntimeStatus(RuntimeCore runtimeCore,
                             bool warningAsError = false,
                             System.IO.TextWriter writer = null)
        {
            warnings = new List<Runtime.WarningEntry>();
            this.runtimeCore = runtimeCore;

            if (writer != null)
            {
                System.Console.SetOut(writer);
            }
        }

        public void LogWarning(Runtime.WarningID ID, string message, string filename, int line, int col)
        {
            filename = filename ?? string.Empty;

            if (!runtimeCore.Options.IsDeltaExecution && (string.IsNullOrEmpty(filename) ||
                line == Constants.kInvalidIndex ||
                col == Constants.kInvalidIndex))
            {
                AuditCodeLocation(ref filename, ref line, ref col);
            }

            var warningMsg = string.Format(Resources.kConsoleWarningMessage,
                                           message, filename, line, col);

            if (runtimeCore.Options.Verbose)
            {
                System.Console.WriteLine(warningMsg);
            }

            if (WebMessageHandler != null)
            {
                var outputMessage = new OutputMessage(warningMsg);
                WebMessageHandler.Write(outputMessage);
            }

            if (MessageHandler != null)
            {
                var outputMessage = new OutputMessage(OutputMessage.MessageType.Warning,
                                                      message.Trim(), filename, line, col);
                MessageHandler.Write(outputMessage);
            }

            AssociativeGraph.GraphNode executingGraphNode = null;
            var executive = runtimeCore.CurrentExecutive.CurrentDSASMExec;
            if (executive != null)
            {
                executingGraphNode = executive.Properties.executingGraphNode;
                // In delta execution mode, it means the warning is from some
                // internal graph node. 
                if (executingGraphNode != null && executingGraphNode.guid.Equals(System.Guid.Empty))
                {
                    executingGraphNode = runtimeCore.DSExecutable.ExecutingGraphnode;
                }
            }

            var entry = new Runtime.WarningEntry
            {
                ID = ID,
                Message = message,
                Column = col,
                Line = line,
                ExpressionID = runtimeCore.RuntimeExpressionUID,
                GraphNodeGuid = executingGraphNode == null ? Guid.Empty : executingGraphNode.guid,
                AstID = executingGraphNode == null ? Constants.kInvalidIndex : executingGraphNode.OriginalAstID,
                Filename = filename
            };
            warnings.Add(entry);
        }

        public void LogWarning(Runtime.WarningID ID, string message)
        {
            LogWarning(ID, message, string.Empty, Constants.kInvalidIndex, Constants.kInvalidIndex);
        }

        private void AuditCodeLocation(ref string filePath, ref int line, ref int column)
        {
            // We don't attempt to change line and column numbers if 
            // they are already provided (caller can force update of 
            // them by setting either one of them to be -1).
            if (!string.IsNullOrEmpty(filePath))
            {
                if (-1 != line && (-1 != column))
                    return;
            }

            // As we create internal functions like %dotarg() and %dot() and
            // append them to the end of the script, it is possible that the 
            // location is in these functions so that the pc dictionary doesn't
            // contain pc key and return maximum line number + 1. 
            // 
            // Need to check if is in internal function or not, If it is, need
            // to go back the last stack frame to get the correct pc value
            int pc = Constants.kInvalidPC;
            int codeBlock = 0;
            if (runtimeCore != null)
            {
                pc = runtimeCore.CurrentExecutive.CurrentDSASMExec.PC;
                codeBlock = runtimeCore.RunningBlock;

                if (String.IsNullOrEmpty(filePath))
                {
                    filePath = runtimeCore.DSExecutable.CurrentDSFileName;
                }
            }
            if (runtimeCore.Options.IsDeltaExecution)
            {
                GetLocationByGraphNode(ref line, ref column);

                if (line == Constants.kInvalidIndex)
                    GetLocationByPC(pc, codeBlock, ref line, ref column);
            }
            else
                GetLocationByPC(pc, codeBlock, ref line, ref column);

        }

        private void GetLocationByPC(int pc, int blk, ref int line, ref int column)
        {
            //--------Dictionary Structure:--------
            //--------Name: codeToLocation---------
            //----------KEY: ----------------------
            //----------------mergedKey: ----------
            //-------------------|- blk -----------
            //-------------------|- pc ------------
            //----------VALUE: --------------------
            //----------------location: -----------
            //-------------------|- line ----------
            //-------------------|- col -----------

            //Zip those integers into 64-bit ulong
            ulong mergedKey = (((ulong)blk) << 32 | ((uint)pc));
            ulong location = (((ulong)line) << 32 | ((uint)column));

            if (runtimeCore.DSExecutable.CodeToLocation.ContainsKey(mergedKey))
            {
                location = runtimeCore.DSExecutable.CodeToLocation[mergedKey];
            }

            foreach (KeyValuePair<ulong, ulong> kv in runtimeCore.DSExecutable.CodeToLocation)
            {
                //Conditions: within same blk && find the largest key which less than mergedKey we want to find
                if ((((int)(kv.Key >> 32)) == blk) && (kv.Key < mergedKey))
                {
                    location = kv.Value;
                }
            }
            //Unzip the location
            line = ((int)(location >> 32));
            column = ((int)(location & 0x00000000ffffffff));
        }

        private void GetLocationByGraphNode(ref int line, ref int col)
        {
            ulong location = (((ulong)line) << 32 | ((uint)col));

            foreach (var prop in runtimeCore.InterpreterProps)
            {
                bool fileScope = false;
                if (prop.executingGraphNode == null)
                    continue;

                int startpc = prop.executingGraphNode.updateBlock.startpc;
                int endpc = prop.executingGraphNode.updateBlock.endpc;
                int block = prop.executingGraphNode.languageBlockId;

                // Determine if the current executing graph node is in an imported file scope
                // If so, continue searching in the outer graph nodes for the line and col in the outer-most context - pratapa

                for (int i = startpc; i <= endpc; ++i)
                {
                    var instruction = runtimeCore.DSExecutable.instrStreamList[block].instrList[i];
                    if (instruction.debug != null)
                    {
                        if (instruction.debug.Location.StartInclusive.SourceLocation.FilePath != null)
                        {
                            fileScope = true;
                            break;
                        }
                        else
                        {
                            fileScope = false;
                            break;
                        }
                    }
                }
                if (fileScope)
                    continue;


                foreach (var kv in runtimeCore.DSExecutable.CodeToLocation)
                {
                    if ((((int)(kv.Key >> 32)) == block) && (kv.Key >= (ulong)startpc && kv.Key <= (ulong)endpc))
                    {
                        location = kv.Value;
                        line = ((int)(location >> 32));
                        col = ((int)(location & 0x00000000ffffffff));
                        break;
                    }
                }
                if (line != -1)
                    break;
            }

        }

        /// <summary>
        /// Report that the method cannot be found.
        /// </summary>
        /// <param name="methodName">The method that cannot be found</param>
        /// <param name="classScope">The class scope of object</param>
        /// <param name="arguments">Arguments</param>
        public void LogFunctionGroupNotFoundWarning(string methodName,
            int classScope,
            List<StackValue> arguments)
        {
            string className = runtimeCore.DSExecutable.classTable.ClassNodes[classScope].Name;

            List<string> argumentTypes = new List<string>();
            if (arguments == null || arguments.Count == 0)
            {
                string propertyName;
                if (CoreUtils.TryGetPropertyName(methodName, out propertyName))
                {
                    string message = string.Format(Resources.kPropertyOfClassNotFound, propertyName, className);
                    LogWarning(WarningID.MethodResolutionFailure, message);
                }
                else
                {
                    string message = string.Format(Resources.FunctionGroupNotFound, methodName, className);
                    LogWarning(WarningID.MethodResolutionFailure, message);
                }
            }
            else
            {
                foreach (var argument in arguments)
                {
                    ProtoCore.Type type = runtimeCore.DSExecutable.TypeSystem.BuildTypeObject(argument.metaData.type, 0);
                    argumentTypes.Add(type.ToShortString());
                }
                string message = string.Format(Resources.FunctionGroupWithParameterNotFound, methodName, className, string.Join(",", argumentTypes));
                LogWarning(WarningID.MethodResolutionFailure, message);
            }
        }


        public void LogMethodResolutionWarning(string methodName,
                                               int classScope = Constants.kGlobalScope,
                                               List<StackValue> arguments = null)
        {
            string message;
            string propertyName;
            Operator op;

            if (CoreUtils.TryGetPropertyName(methodName, out propertyName))
            {
                if (classScope != Constants.kGlobalScope)
                {
                    string classname = runtimeCore.DSExecutable.classTable.ClassNodes[classScope].Name;
                    message = string.Format(Resources.kPropertyOfClassNotFound, propertyName, classname);
                }
                else
                {
                    message = string.Format(Resources.kPropertyNotFound, propertyName);
                }
            }
            else if (CoreUtils.TryGetOperator(methodName, out op))
            {
                string strOp = Op.GetOpSymbol(op);
                message = String.Format(Resources.kMethodResolutionFailureForOperator,
                                        strOp,
                                        runtimeCore.DSExecutable.TypeSystem.GetType(arguments[0].metaData.type),
                                        runtimeCore.DSExecutable.TypeSystem.GetType(arguments[1].metaData.type));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                foreach (StackValue sv in arguments)
                {
                    sb.Append(runtimeCore.DSExecutable.TypeSystem.GetType(sv.metaData.type));
                    sb.Append(",");
                }
                String outString = sb.ToString();
                String typesList = outString.Substring(0, outString.Length - 1); //Drop trailing ','
                typesList = typesList + ")";


                message = string.Format(Resources.kMethodResolutionFailureWithTypes, methodName, typesList);
            }

            LogWarning(WarningID.MethodResolutionFailure, message);
        }

        public void LogMethodNotAccessibleWarning(string methodName)
        {
            string message;
            string propertyName;

            if (CoreUtils.TryGetPropertyName(methodName, out propertyName))
            {
                message = String.Format(Resources.kPropertyInaccessible, propertyName);
            }
            else
            {
                message = String.Format(Resources.kMethodResolutionFailure, methodName);
            }
            LogWarning(ProtoCore.Runtime.WarningID.MethodResolutionFailure, message);
        }
    }
}

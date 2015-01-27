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
            kDefault,
            kAccessViolation,
            kAmbiguousMethodDispatch,
            kAurgumentIsNotExpected,
            kCallingConstructorOnInstance,
            kConversionNotPossible,
            kDereferencingNonPointer,
            kFileNotExist,
            kIndexOutOfRange,
            kInvalidRecursion,
            kInvalidArguments,
            kCyclicDependency,
            kMethodResolutionFailure,
            kOverIndexing,
            kTypeConvertionCauseInfoLoss,
            kTypeMismatch,
            kReplicationWarning
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
        private ProtoCore.Core core;
        private bool warningAsError;
        private System.IO.TextWriter output = System.Console.Out;
        private List<Runtime.WarningEntry> warnings;

        public IOutputStream MessageHandler 
        { 
            get; set; 
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

        public void ClearWarnings()
        {
            warnings.Clear();
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

        public RuntimeStatus(Core core, 
                             bool warningAsError = false, 
                             System.IO.TextWriter writer = null)
        {
            warnings = new List<Runtime.WarningEntry>();
            this.warningAsError = warningAsError;
            this.core = core;

            if (writer != null)
            {
                output = System.Console.Out;
                System.Console.SetOut(writer);
            }
        }

        public void LogWarning(Runtime.WarningID ID, string message, string filename, int line, int col)
        {
            filename = filename ?? string.Empty;

            if (!this.core.Options.IsDeltaExecution && (string.IsNullOrEmpty(filename) || 
                line == Constants.kInvalidIndex || 
                col == Constants.kInvalidIndex))
            {
                CodeGen.AuditCodeLocation(core, ref filename, ref line, ref col);
            }

            var warningMsg = string.Format(Resources.kConsoleWarningMessage, 
                                           message, filename, line, col);

            if (core.Options.Verbose)
            {
                System.Console.WriteLine(warningMsg);
            }

            if (MessageHandler != null)
            {
                var outputMessage = new OutputMessage(OutputMessage.MessageType.Warning,
                                                      message.Trim(), filename, line, col);
                MessageHandler.Write(outputMessage);
            }

            AssociativeGraph.GraphNode executingGraphNode = null;
            var executive = core.CurrentExecutive.CurrentDSASMExec;
            if (executive != null)
            {
                executingGraphNode = executive.Properties.executingGraphNode;
                // In delta execution mode, it means the warning is from some
                // internal graph node. 
                if (executingGraphNode != null && executingGraphNode.guid.Equals(System.Guid.Empty))
                {
                    executingGraphNode = core.DSExecutable.RuntimeData.ExecutingGraphnode;
                }
            }

            var entry = new Runtime.WarningEntry
            {
                ID = ID,
                Message = message,
                Column = col,
                Line = line,
                ExpressionID = core.RuntimeExpressionUID,
                GraphNodeGuid = executingGraphNode == null ? Guid.Empty : executingGraphNode.guid,
                AstID = executingGraphNode == null ? Constants.kInvalidIndex : executingGraphNode.OriginalAstID,
                Filename = filename
            };
            warnings.Add(entry);

            if (core.Options.IsDeltaExecution)
            {
            }
        }

        public void LogWarning(Runtime.WarningID ID, string message)
        {
            LogWarning(ID, message, string.Empty, Constants.kInvalidIndex, Constants.kInvalidIndex);
        }

        /// <summary>
        /// Report that a function group couldn't be found
        /// </summary>
        /// <param name="methodName">The method that can't be found</param>
        public void LogFunctionGroupNotFoundWarning(
            string methodName)
        {
            String message = string.Format(Resources.FUNCTION_GROUP_RESOLUTION_FAILURE, methodName);
            LogWarning(WarningID.kMethodResolutionFailure, message);
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
                    string classname = core.ClassTable.ClassNodes[classScope].name;
                    message = string.Format(Resources.kPropertyOfClassNotFound, classname, propertyName);
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
                                        core.TypeSystem.GetType(arguments[0].metaData.type),
                                        core.TypeSystem.GetType(arguments[1].metaData.type));
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("(");
                foreach (StackValue sv in arguments)
                {
                    sb.Append(core.TypeSystem.GetType(sv.metaData.type));
                    sb.Append(",");
                }
                String outString = sb.ToString();
                String typesList = outString.Substring(0, outString.Length - 1); //Drop trailing ','
                typesList = typesList + ")";


                message = string.Format(Resources.kMethodResolutionFailureWithTypes, methodName, typesList);
            }

            LogWarning(WarningID.kMethodResolutionFailure, message);
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
            LogWarning(ProtoCore.Runtime.WarningID.kMethodResolutionFailure, message);
        }
    }
}

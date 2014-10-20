using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using ProtoCore.DSASM;
using ProtoCore.Utils;
using System.Linq;
using ProtoCore.RuntimeData;

namespace ProtoCore
{
    namespace RuntimeData
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

        public struct WarningMessage
        {
            public const string kArrayOverIndexed = "Variable is over indexed.";
            public const string kIndexOutOfRange = "Index is out of range.";
            public const string kSymbolOverIndexed = "'{0}' is over indexed.";
            public const string kStringOverIndexed = "String is over indexed.";
            public const string kStringIndexOutOfRange = "The index to string is out of range";
            public const string kAssignNonCharacterToString = "Only character can be assigned to a position in a string.";
            public const string KCallingConstructorOnInstance = "Cannot call constructor '{0}()' on instance.";
            public const string kInvokeMethodOnInvalidObject = "Method '{0}()' is invoked on invalid object.";
            public const string kMethodStackOverflow = "Stack overflow caused by calling method '{0}()' recursively.";
            public const string kCyclicDependency = "Cyclic dependency detected at '{0}' and '{1}'.";
            public const string kFFIFailedToObtainThisObject = "Failed to obtain this object for '{0}.{1}'.";
            public const string kFFIFailedToObtainObject = "Failed to obtain object '{0}' for '{1}.{2}'.";
            public const string kFFIInvalidCast = "'{0}' is being cast to '{1}', but the allowed range is [{2}..{3}].";
            public const string kDeferencingNonPointer = "Dereferencing a non-pointer.";
            public const string kFailToConverToPointer = "Converting other things to pointer is not allowed.";
            public const string kFailToConverToNull = "Converting other things to null is not allowed.";
            public const string kFailToConverToFunction = "Converting other things to function pointer is not allowed.";
            public const string kConvertDoubleToInt = "Converting double to int will cause possible information loss.";
            public const string kArrayRankReduction = "Type conversion would cause array rank reduction. This is not permitted outside of replication. {511ED65F-FB66-4709-BDDA-DCD5E053B87F}";
            public const string kConvertArrayToNonArray = "Converting an array to {0} would cause array rank reduction and is not permitted.";
            public const string kConvertNonConvertibleTypes = "Asked to convert non-convertible types.";
            public const string kFunctionNotFound = "No candidate function could be found.";
            public const string kAmbigousMethodDispatch = "Candidate function could not be located on final replicated dispatch GUARD {FDD1F347-A9A6-43FB-A08E-A5E793EC3920}.";
            public const string kInvalidArguments = "Argument is invalid.";
            public const string kInvalidArgumentsInRangeExpression = "The value that used in range expression should be either integer or double.";
            public const string kInvalidAmountInRangeExpression = "The amount in range expression should be an positive integer.";
            public const string kNoStepSizeInAmountRangeExpression = "No step size is specified in amount range expression.";
            public const string kFileNotFound = "'{0}' doesn't exist.";
            public const string kPropertyNotFound = "Object does not have a property '{0}'.";
            public const string kPropertyOfClassNotFound = "Class '{0}' does not have a property '{1}'.";
            public const string kPropertyInaccessible = "Property '{0}' is inaccessible.";
            public const string kMethodResolutionFailure = "Method resolution failure on: {0}() - 0CD069F4-6C8A-42B6-86B1-B5C17072751B.";
            public const string kMethodResolutionFailureWithTypes = "One or more of the input types are not matching, please check that the right variable types are being passed to the inputs. Couldn't find a version of {0} that takes arguments of type {1}.";
            public const string kMethodResolutionFailureForOperator = "Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'.";
            public const string kConsoleWarningMessage = "> Runtime warning: {0}\n - \"{1}\" <line: {2}, col: {3}>";

            public const string FUNCTION_GROUP_RESOLUTION_FAILURE =
                "No function called {0} could be found. Please check the name of the function.";

        }

        public struct WarningEntry
        {
            public RuntimeData.WarningID ID;
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
        private List<RuntimeData.WarningEntry> warnings;

        public IOutputStream MessageHandler 
        { 
            get; set; 
        }

        public IOutputStream WebMessageHandler 
        { 
            get; set; 
        }

        public IEnumerable<RuntimeData.WarningEntry> Warnings 
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
            warnings = new List<RuntimeData.WarningEntry>();
            this.warningAsError = warningAsError;
            this.core = core;

            if (core.Options.WebRunner)
            {
                this.WebMessageHandler = new WebOutputStream(core);
            }

            if (writer != null)
            {
                output = System.Console.Out;
                System.Console.SetOut(writer);
            }
        }

        public void LogWarning(RuntimeData.WarningID ID, string message, string filename, int line, int col)
        {
            filename = filename ?? string.Empty;

            if (!this.core.Options.IsDeltaExecution && (string.IsNullOrEmpty(filename) || 
                line == Constants.kInvalidIndex || 
                col == Constants.kInvalidIndex))
            {
                CodeGen.AuditCodeLocation(core, ref filename, ref line, ref col);
            }

            var warningMsg = string.Format(WarningMessage.kConsoleWarningMessage, 
                                           message, filename, line, col);

            if (core.Options.Verbose)
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
            var executive = core.CurrentExecutive.CurrentDSASMExec;
            if (executive != null)
            {
                executingGraphNode = executive.Properties.executingGraphNode;
                // In delta execution mode, it means the warning is from some
                // internal graph node. 
                if (executingGraphNode != null && executingGraphNode.guid.Equals(System.Guid.Empty))
                {
                    executingGraphNode = core.ExecutingGraphnode;
                }
            }

            var entry = new RuntimeData.WarningEntry
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

        public void LogWarning(RuntimeData.WarningID ID, string message)
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
            String message = string.Format(WarningMessage.FUNCTION_GROUP_RESOLUTION_FAILURE, methodName);
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
                    message = string.Format(WarningMessage.kPropertyOfClassNotFound, classname, propertyName);
                }
                else
                {
                    message = string.Format(WarningMessage.kPropertyNotFound, propertyName);
                }
            }
            else if (CoreUtils.TryGetOperator(methodName, out op))
            {
                string strOp = Op.GetOpSymbol(op);
                message = String.Format(WarningMessage.kMethodResolutionFailureForOperator,
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


                message = string.Format(WarningMessage.kMethodResolutionFailureWithTypes, methodName, typesList);
            }

            LogWarning(WarningID.kMethodResolutionFailure, message);
        }

        public void LogMethodNotAccessibleWarning(string methodName)
        {
            string message;
            string propertyName;

            if (CoreUtils.TryGetPropertyName(methodName, out propertyName))
            {
                message = String.Format(RuntimeData.WarningMessage.kPropertyInaccessible, propertyName);
            }
            else
            {
                message = String.Format(RuntimeData.WarningMessage.kMethodResolutionFailure, methodName);
            }
            LogWarning(ProtoCore.RuntimeData.WarningID.kMethodResolutionFailure, message);
        }
    }
}

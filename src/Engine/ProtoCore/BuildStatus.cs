using System;
using System.Collections.Generic;
using System.IO;

namespace ProtoCore
{
    public class BuildHaltException : Exception
    {
        public string ErrorMessage 
        { 
            get; private set; 
        }

        public BuildHaltException()
        {
            ErrorMessage = "Stopping Build\n";
        }

        public BuildHaltException(string message)
        {
            ErrorMessage = message + '\n';
        }
    }

    namespace BuildData
    {
        public enum WarningID
        {
            kDefault,
            kAccessViolation,
            kCallingConstructorInConstructor,
            kCallingConstructorOnInstance,
            kCallingNonStaticMethodOnClass,
            kFunctionAbnormalExit,
            kFunctionAlreadyDefined,
            kFunctionNotFound,
            kIdUnboundIdentifier,
            kInvalidArguments,
            kInvalidStaticCyclicDependency,
            kInvalidRangeExpression,
            kInvalidThis,
            kMismatchReturnType,
            kMissingReturnStatement,
            kParsing,
            kTypeUndefined,
            kPropertyNotFound,
            kFileNotFound,
            kAlreadyImported,
            kMultipleSymbolFound,
            kWarnMax
        }

        public struct WarningMessage
        {
            public const string kAssingToThis = "'this' is readonly and cannot be assigned to.";
            public const string kCallingNonStaticProperty = "'{0}.{1}' is not a static property.";
            public const string kCallingNonStaticMethod = "'{0}.{1}()' is not a static method.";
            public const string kMethodHasInvalidArguments = "'{0}()' has some invalid arguments.";
            public const string kInvalidStaticCyclicDependency = "Cyclic dependency detected at '{0}' and '{1}'.";
            public const string KCallingConstructorOnInstance = "Cannot call constructor '{0}()' on instance.";
            public const string kPropertyIsInaccessible = "Property '{0}' is inaccessible.";
            public const string kMethodIsInaccessible = "Method '{0}()' is inaccessible.";
            public const string kCallingConstructorInConstructor = "Cannot call constructor '{0}()' in itself.";
            public const string kPropertyNotFound = "Property '{0}' not found";
            public const string kMethodNotFound = "Method '{0}()' not found";
            public const string kUnboundIdentifierMsg = "Variable '{0}' hasn't been defined yet.";
            public const string kFunctionNotReturnAtAllCodePaths = "Method '{0}()' doesn't return at all code paths.";
            public const string kRangeExpressionWithStepSizeZero = "The step size of range expression should not be 0.";
            public const string kRangeExpressionWithInvalidStepSize = "The step size of range expression is invalid.";
            public const string kRangeExpressionWithNonIntegerStepNumber = "The step number of range expression should be integer.";
            public const string kRangeExpressionWithNegativeStepNumber = "The step number of range expression should be greater than 0.";
            public const string kTypeUndefined = "Type '{0}' is not defined.";
            public const string kMethodAlreadyDefined = "Method '{0}()' is already defined.";
            public const string kReturnTypeUndefined = "Return type '{0}' of method '{1}()' is not defined.";
            public const string kExceptionTypeUndefined = "Exception type '{0}' is not defined.";
            public const string kArgumentTypeUndefined = "Type '{0}' of argument '{1}' is not defined.";
            public const string kInvalidBreakForFunction = "Statement break causes function to abnormally return null.";
            public const string kInvalidContinueForFunction = "Statement continue cause function to abnormally return null.";
            public const string kUsingThisInStaticFunction = "'this' cannot be used in static method.";
            public const string kInvalidThis = "'this' can only be used in member methods.";
            public const string kUsingNonStaticMemberInStaticContext = "'{0}' is not a static property, so cannot be assigned to static properties or used in static methods.";
            public const string kFileNotFound = "File : '{0}' not found";
            public const string kAlreadyImported = "File : '{0}' is already imported";
            public const string kMultipleSymbolFound = "Multiple definitions for '{0}' are found as {1}";
        }

        public struct ErrorEntry
        {
            public string FileName;
            public string Message;
            public int Line;
            public int Column;
        }

        public struct WarningEntry
        {
            public WarningID ID;
            public string Message;
            public int Line;
            public int Column;
            public string FileName;
        }
    }

    public class OutputMessage
    {
        public enum MessageType { Info, Warning, Error }
        // A constructor for message only for print-out purpose
        public OutputMessage(string message)
        {
            Type = MessageType.Info;
            Message = message;
            FilePath = string.Empty;
            Line = -1;
            Column = -1;
        }
        // A constructor for generic message.
        public OutputMessage(MessageType type, string message)
        {
            Type = type;
            Message = message;
            FilePath = string.Empty;
            Line = -1;
            Column = -1;
        }

        // A constructor for source location related messages.
        public OutputMessage(MessageType type, string message,
            string filePath, int line, int column)
        {
            Type = type;
            Message = message;
            FilePath = filePath;
            Line = line;
            Column = column;
        }

        public MessageType Type { get; private set; }
        public string FilePath { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Message { get; private set; }
        public bool Continue { get; set; }
    }

    public interface IOutputStream
    {
        void Write(OutputMessage message);
        List<OutputMessage> GetMessages();
    }

    public class FileOutputStream : IOutputStream
    {
        StreamWriter FileStream { get; set; }

        public FileOutputStream(StreamWriter sw)
        {
            FileStream = sw;
        }

        public void Write(ProtoCore.OutputMessage message)
        {
            if (null == message)
                return;

            if (string.IsNullOrEmpty(message.FilePath))
            {
                // Type: Message
                string formatWithoutFile = "{0}: {1}";
                FileStream.WriteLine(string.Format(formatWithoutFile,
                    message.Type.ToString(), message.Message));
            }
            else
            {
                // Type: Message (File - Line, Column)
                string formatWithFile = "{0}: {1} ({2} - line: {3}, col: {4})";
                FileStream.WriteLine(string.Format(formatWithFile,
                    message.Type.ToString(), message.Message,
                    message.FilePath, message.Line, message.Column));
            }

            if (message.Type == ProtoCore.OutputMessage.MessageType.Warning)
                message.Continue = true;
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            return null;
        }
    }

    public class TextOutputStream : IOutputStream
    {
        public StringWriter TextStream { get; private set; }
        public Dictionary<int, List<string>> Map { get; private set; }

        public TextOutputStream(StringWriter sw, Dictionary<int, List<string>> map)
        {
            TextStream = sw;
            Map = map;
        }

        public TextOutputStream(Dictionary<int, List<string>> map)
        {
            TextStream = new StringWriter();
            Map = map;
        }

        public void Write(ProtoCore.OutputMessage message)
        {
            if (null == message)
                return;

            if (string.IsNullOrEmpty(message.FilePath))
            {
                // Type: Message
                string formatWithoutFile = "{0}: {1}";
                TextStream.WriteLine(string.Format(formatWithoutFile,
                    message.Type.ToString(), message.Message));
            }
            else
            {
                // Type: Message (File - Line, Column)
                string formatWithFile = "{0}: {1} ({2} - line: {3}, col: {4})";
                TextStream.WriteLine(string.Format(formatWithFile,
                    message.Type.ToString(), message.Message,
                    message.FilePath, message.Line, message.Column));
            }

            if (message.Type == ProtoCore.OutputMessage.MessageType.Warning)
                message.Continue = true;
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            return null;
        }
    }

    public class ConsoleOutputStream : IOutputStream
    {
        public ConsoleOutputStream()
        {
        }

        public void Write(ProtoCore.OutputMessage message)
        {
            if (null == message)
                return;

            if (string.IsNullOrEmpty(message.FilePath))
            {
                // Type: Message
                string formatWithoutFile = "{0}: {1}";
                System.Console.WriteLine(string.Format(formatWithoutFile,
                    message.Type.ToString(), message.Message));
            }
            else
            {
                // Type: Message (File - Line, Column)
                string formatWithFile = "{0}: {1} ({2} - line: {3}, col: {4})";
                System.Console.WriteLine(string.Format(formatWithFile,
                    message.Type.ToString(), message.Message,
                    message.FilePath, message.Line, message.Column));
            }

            if (message.Type == ProtoCore.OutputMessage.MessageType.Warning)
                message.Continue = true;
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            return null;
        }
    }

    public class WebOutputStream : IOutputStream
    {
        public Core core;
        public string filename;
        public WebOutputStream(Core core)
        {
            this.core = core;
            this.filename = core.CurrentDSFileName;
        }
        public string GetCurrentFileName()
        {
            return this.filename;
        }

        public void Write(ProtoCore.OutputMessage message)
        {
            if (null == message)
                return;

            if (string.IsNullOrEmpty(message.FilePath))
            {
                // Type: Message
                string formatWithoutFile = "{0}: {1}";

                //System.IO.StreamWriter logFile = new System.IO.StreamWriter("c:\\test.txt");
                if (null != core.ExecutionLog)
                {
                    core.ExecutionLog.WriteLine(string.Format(formatWithoutFile,
                        message.Type.ToString(), message.Message));
                }
            }
            else
            {
                // Type: Message (File - Line, Column)
                if (null != core.ExecutionLog)
                {
                    string formatWithFile = "{0}: {1} ({2} - line: {3}, col: {4})";
                    core.ExecutionLog.WriteLine(string.Format(formatWithFile,
                        message.Type.ToString(), message.Message,
                        message.FilePath, message.Line, message.Column));
                }
            }

            if (message.Type == ProtoCore.OutputMessage.MessageType.Warning)
                message.Continue = true;
        }

        public void Close()
        {
            if (null != core.ExecutionLog)
                core.ExecutionLog.Close();
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            return null;
        }
    }

    public class BuildStatus
    {
        private ProtoCore.Core core;
        private System.IO.TextWriter consoleOut = System.Console.Out;
        private readonly bool LogWarnings = true;
        private readonly bool logErrors = true;
        private readonly bool displayBuildResult = true;
        private readonly bool warningAsError;
        private readonly bool errorAsWarning = false;

        public IOutputStream MessageHandler 
        { 
            get; set; 
        }

        public WebOutputStream WebMsgHandler
        {
            get;
            set;
        }

        private List<BuildData.WarningEntry> warnings;
        public IEnumerable<BuildData.WarningEntry> Warnings
        {
            get
            {
                return warnings;
            }
        }

        public int WarningCount
        {
            get { return warnings.Count; }
        }

        private readonly List<BuildData.ErrorEntry> errors;
        public IEnumerable<BuildData.ErrorEntry> Errors
        {
            get
            {
                return errors;
            }
        }
        
        public int ErrorCount
        {
            get { return errors.Count; }
        }

        public bool BuildSucceeded
        {
            get
            {
                return warningAsError 
                    ? (ErrorCount == 0 && WarningCount == 0)
                    : (ErrorCount == 0);
            }
        }

        //  logs all errors and warnings by default
        //
        public BuildStatus(Core core,bool warningAsError, System.IO.TextWriter writer = null, bool errorAsWarning = false)
        {
            this.core = core;
            warnings = new List<BuildData.WarningEntry>();
            errors = new List<BuildData.ErrorEntry>();
            this.warningAsError = warningAsError;
            this.errorAsWarning = errorAsWarning;

            if (writer != null)
            {
                consoleOut = System.Console.Out;
                System.Console.SetOut(writer);
            }

            // Create a default console output stream, and this can 
            // be overwritten in IDE by assigning it a different value.
            this.MessageHandler = new ConsoleOutputStream();
            if (core.Options.WebRunner)
            {
                this.WebMsgHandler = new WebOutputStream(core);
            }
        }

        public BuildStatus(Core core,bool LogWarnings, bool logErrors, bool displayBuildResult, System.IO.TextWriter writer = null)
        {
            this.core = core;
            this.LogWarnings = LogWarnings;
            this.logErrors = logErrors;
            this.displayBuildResult = displayBuildResult;

            warnings = new List<BuildData.WarningEntry>();
            errors = new List<BuildData.ErrorEntry>();

            if (writer != null)
            {
                consoleOut = System.Console.Out;
                System.Console.SetOut(writer);
            }

            // Create a default console output stream, and this can 
            // be overwritten in IDE by assigning it a different value.
            this.MessageHandler = new ConsoleOutputStream();
        }

        public void SetStream(System.IO.TextWriter writer)
        {
            //  flush the stream first
            System.Console.Out.Flush();

            if (writer != null)
            {
                consoleOut = System.Console.Out;
                System.Console.SetOut(writer);
            }
            else
            {
                System.Console.SetOut(consoleOut);
            }
        }

        public void ClearWarnings()
        {
            warnings.Clear();
        }
        
        public void ClearErrors()
        {
            errors.Clear();
        }

        public void LogSyntaxError(string msg, string fileName = null, int line = -1, int col = -1)
        {
            if (logErrors)
            {
                var message = string.Format("{0}({1},{2}) Error:{3}", fileName, line, col, msg);
                System.Console.WriteLine(message);
            }

            var errorEntry = new BuildData.ErrorEntry
            {
                FileName = fileName,
                Message = msg,
                Line = line,
                Column = col            
            };

            if(core.Options.IsDeltaExecution)
            {
            }

            errors.Add(errorEntry);

            OutputMessage outputmessage = new OutputMessage(OutputMessage.MessageType.Error, msg.Trim(), fileName, line, col);
            if (MessageHandler != null)
            {
                MessageHandler.Write(outputmessage);
                if (WebMsgHandler != null)
                {
                    OutputMessage webOutputMsg = new OutputMessage(OutputMessage.MessageType.Error, msg.Trim(), "", line, col);
                    WebMsgHandler.Write(webOutputMsg);
                }
                if (!outputmessage.Continue)
                    throw new BuildHaltException(msg);
            }
        }

        public void LogSemanticError(string msg, string fileName = null, int line = -1, int col = -1, AssociativeGraph.GraphNode graphNode = null)
        {
            if (logErrors)
            {
                System.Console.WriteLine("{0}({1},{2}) Error:{3}", fileName, line, col, msg);
            }

            if (core.Options.IsDeltaExecution)
            {
            }

            BuildData.ErrorEntry errorEntry = new BuildData.ErrorEntry
            {
                FileName = fileName,
                Message = msg,
                Line = line,
                Column = col
            };
            errors.Add(errorEntry);

            OutputMessage outputmessage = new OutputMessage(OutputMessage.MessageType.Error, msg.Trim(), fileName, line, col);
            if (MessageHandler != null)
            {
                MessageHandler.Write(outputmessage);
                if (WebMsgHandler != null)
                {
                    OutputMessage webOutputMsg = new OutputMessage(OutputMessage.MessageType.Error, msg.Trim(), "", line, col);
                    WebMsgHandler.Write(webOutputMsg);
                }
                if (!outputmessage.Continue)
                    throw new BuildHaltException(msg);
            }
            throw new BuildHaltException(msg);
        }

        public void LogWarning(BuildData.WarningID warningID, string message, string fileName = null, int line = -1, int col = -1)
        { 
            if (LogWarnings)
            {
                System.Console.WriteLine("{0}({1},{2}) Warning:{3}", fileName, line, col, message);
            }

            var entry = new BuildData.WarningEntry 
            { 
                ID = warningID, 
                Message = message, 
                Line = line, 
                Column = col, 
                FileName = fileName 
            };
            warnings.Add(entry);

            if (core.Options.IsDeltaExecution)
            {
            }

            OutputMessage outputmessage = new OutputMessage(OutputMessage.MessageType.Warning, message.Trim(), fileName, line, col);
            if (MessageHandler != null)
            {
                MessageHandler.Write(outputmessage);
                if (WebMsgHandler != null)
                {
                    OutputMessage webOutputMsg = new OutputMessage(OutputMessage.MessageType.Warning, message.Trim(), "", line, col);
                    WebMsgHandler.Write(webOutputMsg);
                }
                if (!outputmessage.Continue)
                    throw new BuildHaltException(message);
            }
        }

        public void ReportBuildResult()
        {
            string buildResult = string.Format("========== Build: {0} error(s), {1} warning(s) ==========\n", errors.Count, warnings.Count);

            if (displayBuildResult)
            {
                System.Console.WriteLine(buildResult);
            }

            if (MessageHandler != null)
            {
                var outputMsg = new OutputMessage(buildResult);
                MessageHandler.Write(outputMsg);
                if (WebMsgHandler != null)
                {
                    WebMsgHandler.Write(outputMsg);
                }
            }
        }
	}
}


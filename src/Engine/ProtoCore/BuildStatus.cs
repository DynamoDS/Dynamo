using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.DSASM;

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
            kMultipleSymbolFoundFromName,
            kWarnMax
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
            public Guid GraphNodeGuid;
            public int AstID;
            public string FileName;
            public SymbolNode UnboundVariableSymbolNode;
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

            displayBuildResult = logErrors = LogWarnings = core.Options.Verbose;
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

        /// <summary>
        /// Remove unbound variable warnings that match all symbols in the symbolList
        /// </summary>
        /// <param name="symbolList"></param>
        public void RemoveUnboundVariableWarnings(HashSet<SymbolNode> symbolList)
        {
            foreach (SymbolNode symbol in symbolList)
            {
                // Remove all warnings that match the symbol
                warnings.RemoveAll(w => w.ID == BuildData.WarningID.kIdUnboundIdentifier && w.UnboundVariableSymbolNode != null && w.UnboundVariableSymbolNode.Equals(symbol));
            }
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

        public void ClearWarningsForAst(int astID)
        {
            warnings.RemoveAll(w => w.AstID.Equals(astID));
        }

        public void ClearWarningsForGraph(Guid guid)
        {
            warnings.RemoveAll(w => w.GraphNodeGuid.Equals(guid));
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

        /// <summary>
        /// Logs the warning where the usage of a symbol (symbolName) cannot be 
        /// resolved because it collides with multiple symbols(collidingSymbolNames) 
        /// </summary>
        /// <param name="symbolUsage"></param>
        /// <param name="duplicateSymbolNames"></param>
        public void LogSymbolConflictWarning(string symbolName, string[] collidingSymbolNames)
        {
            string message = string.Format(StringConstants.kMultipleSymbolFoundFromName, symbolName, "");
            message += String.Join(", ", collidingSymbolNames);
            LogWarning(BuildData.WarningID.kMultipleSymbolFoundFromName, message);
        }

        /// <summary>
        /// Logs the unbound variable warning and sets the unbound symbol
        /// </summary>
        /// <param name="unboundSymbol"></param>
        /// <param name="message"></param>
        /// <param name="fileName"></param>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="graphNode"></param>
        public void LogUnboundVariableWarning(
                                SymbolNode unboundSymbol,  
                                string message, 
                                string fileName = null, 
                                int line = -1, 
                                int col = -1, 
                                AssociativeGraph.GraphNode graphNode = null)
        {
            LogWarning(BuildData.WarningID.kIdUnboundIdentifier, message, core.CurrentDSFileName, line, col, graphNode, unboundSymbol);
        }

        public void LogWarning(BuildData.WarningID warningID, 
                               string message, 
                               string fileName = null, 
                               int line = -1, 
                               int col = -1, 
                               AssociativeGraph.GraphNode graphNode = null,
                               SymbolNode associatedSymbol = null)
        { 
            var entry = new BuildData.WarningEntry 
            { 
                ID = warningID, 
                Message = message, 
                Line = line, 
                Column = col, 
                GraphNodeGuid = graphNode == null ? default(Guid) : graphNode.guid,
                AstID = graphNode == null? DSASM.Constants.kInvalidIndex : graphNode.OriginalAstID,
                FileName = fileName,
                UnboundVariableSymbolNode = associatedSymbol
            };
            warnings.Add(entry);

            if (core.Options.IsDeltaExecution)
            {
            }

            if (LogWarnings)
            {
                System.Console.WriteLine("{0}({1},{2}) Warning:{3}", fileName, line, col, message);

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
        }

        public void ReportBuildResult()
        {
            string buildResult = string.Format("========== Build: {0} error(s), {1} warning(s) ==========\n", errors.Count, warnings.Count);

            if (displayBuildResult)
            {
                System.Console.WriteLine(buildResult);

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
}


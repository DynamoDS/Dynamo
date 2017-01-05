using System;
using System.Collections.Generic;
using System.IO;
using ProtoCore.DSASM;
using ProtoCore.Properties;

namespace ProtoCore
{
    public class BuildHaltException : Exception
    {
        public string ErrorMessage 
        { 
            get; private set; 
        }

        public BuildHaltException(string message)
        {
            ErrorMessage = message + '\n';
        }
    }

    namespace BuildData
    {
        public enum ErrorType
        {
            SyntaxError,
            MaxErrorID
        }

        public enum WarningID
        {
            AccessViolation,
            CallingConstructorInConstructor,
            CallingConstructorOnInstance,
            CallingNonStaticMethodOnClass,
            FunctionAbnormalExit,
            FunctionAlreadyDefined,
            FunctionNotFound,
            IdUnboundIdentifier,
            InvalidStaticCyclicDependency,
            InvalidRangeExpression,
            InvalidThis,
            MissingReturnStatement,
            Parsing,
            TypeUndefined,
            PropertyNotFound,
            FileNotFound,
            MultipleSymbolFound,
            MultipleSymbolFoundFromName,
        }

        public struct ErrorEntry
        {
            public ErrorType ID;
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
    }

    public class TextOutputStream : IOutputStream
    {
        public StringWriter TextStream { get; private set; }
        public Dictionary<int, List<string>> Map { get; private set; }

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
    }

    public class BuildStatus
    {
        private ProtoCore.Core core;
        private System.IO.TextWriter consoleOut = System.Console.Out;
        private readonly bool LogWarnings = true;
        private readonly bool logErrors = true;
        private readonly bool displayBuildResult = true;

        public IOutputStream MessageHandler { get; set; }

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
                return ErrorCount == 0;
            }
        }

        //  logs all errors and warnings by default
        //
        public BuildStatus(Core core, System.IO.TextWriter writer = null, bool errorAsWarning = false)
        {
            this.core = core;
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
            displayBuildResult = logErrors = LogWarnings = core.Options.Verbose;
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
                warnings.RemoveAll(w => w.ID == BuildData.WarningID.IdUnboundIdentifier && w.UnboundVariableSymbolNode != null && w.UnboundVariableSymbolNode.Equals(symbol));
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
            var localizedMessage = LocalizeErrorMessage(msg);

            if (logErrors)
            {
                var message = string.Format("{0}({1},{2}) Error:{3}", fileName, line, col, localizedMessage);
                System.Console.WriteLine(message);
            }

            var errorEntry = new BuildData.ErrorEntry
            {
                ID = BuildData.ErrorType.SyntaxError,
                FileName = fileName,
                Message = localizedMessage,
                Line = line,
                Column = col
            };

            if (core.Options.IsDeltaExecution)
            {
            }

            errors.Add(errorEntry);


            OutputMessage outputmessage = new OutputMessage(OutputMessage.MessageType.Error, localizedMessage.Trim(), fileName, line, col);


            if (MessageHandler != null)
            {
                MessageHandler.Write(outputmessage);
                if (!outputmessage.Continue)
                    throw new BuildHaltException(localizedMessage);
            }
        }

        private string LocalizeErrorMessage(string errorMessage)
        {
            switch (errorMessage)
            {
                case "EOF expected":
                    return Properties.Resources.EOF_expected; 
                case "ident expected":
                    return Properties.Resources.ident_expected;
                case "number expected":
                    return Properties.Resources.number_expected;
                case "float expected":
                    return Properties.Resources.float_expected;
                case "textstring expected":
                    return Properties.Resources.textstring_expected;
                case "char expected":
                    return Properties.Resources.char_expected;
                case "period expected":
                    return Properties.Resources.period_expected;
                case "postfixed_replicationguide expected":
                    return Properties.Resources.postfixed_replicationguide_expected;
                case  "openbracket expected":
                    return Properties.Resources.openbracket_expected;
                case "closebracket expected":
                    return Properties.Resources.closebracket_expected;
                case  "openparen expected":
                    return Properties.Resources.openparen_expected;
                case"closeparen expected":
                    return Properties.Resources.closeparen_expected;
                case "not expected":
                    return Properties.Resources.not_expected;
                case  "neg expected":
                    return Properties.Resources.neg_expected;
                case "pipe expected":
                    return Properties.Resources.pipe_expected;
                case  "lessthan expected":
                    return Properties.Resources.lessthan_expected;
                case  "greaterthan expected":
                    return Properties.Resources.greaterthan_expected;
                case  "lessequal expected":
                    return Properties.Resources.lessequal_expected;
                case  "greaterequal expected": 
                    return Properties.Resources.greaterequal_expected;
                case  "equal expected":
                    return Properties.Resources.equal_expected;
                case  "notequal expected":
                    return Properties.Resources.notequal_expected;
                case "endline expected":
                    return Properties.Resources.endline_expected;
                case "rangeop expected":
                    return Properties.Resources.rangeop_expected;
                case "kw_native expected":
                    return Properties.Resources.kw_native_expected;
                case  "kw_class expected":
                    return Properties.Resources.kw_class_expected;
                case  "kw_constructor expected":
                    return Properties.Resources.kw_constructor_expected;
                case "kw_def expected":
                    return Properties.Resources.kw_def_expected;
                case "kw_external expected":
                    return Properties.Resources.kw_external_expected;
                case  "kw_extend expected":
                    return Properties.Resources.kw_extend_expected;
                case  "kw_heap expected":
                    return Properties.Resources.kw_heap_expected;
                case  "kw_if expected":
                    return Properties.Resources.kw_if_expected;
                case  "kw_elseif expected":
                    return Properties.Resources.kw_elseif_expected;
                case "kw_else expected":
                    return Properties.Resources.kw_else_expected;
                case   "kw_while expected":
                    return Properties.Resources.kw_while_expected;
                case   "kw_for expected":
                    return Properties.Resources.kw_for_expected;
                case  "kw_import expected":
                    return Properties.Resources.kw_import_expected;
                case  "kw_prefix expected":
                    return Properties.Resources.kw_prefix_expected;
                case   "kw_from expected":
                    return Properties.Resources.kw_from_expected;
                case  "kw_break expected":
                    return Properties.Resources.kw_break_expected;
                case   "kw_continue expected":
                    return Properties.Resources.kw_continue_expected;
                case    "kw_static expected":
                    return Properties.Resources.kw_static_expected;
                case   "kw_local expected":
                    return Properties.Resources.kw_local_expected;
                case   "literal_true expected":
                    return Properties.Resources.literal_true_expected;
                case  "literal_false expected":
                    return Properties.Resources.literal_false_expected;
                case  "literal_null expected":
                    return Properties.Resources.literal_null_expected;
                case  "replicationguide_postfix expected":
                    return Properties.Resources.replicationguide_postfix_expected;
                case   "\"throw\" expected":
                    return Properties.Resources.throw_expected;
                case   "\"{\" expected":
                    return Properties.Resources.openbrace_expected;
                case   "\"}\" expected":
                    return Properties.Resources.closebrace_expected;
                case   "\",\" expected":
                    return Properties.Resources.comma_expected;
                case   "\"=\" expected":
                    return Properties.Resources.equalmark_expected;
                case "\":\" expected":
                    return Properties.Resources.doublecolumn_expected;
                case   "\"public\" expected":
                    return Properties.Resources.public_expected;
                case   "\"private\" expected":
                    return Properties.Resources.private_expected;
                case  "\"protected\" expected":
                    return Properties.Resources.protected_expected;
                case  "\"=>\" expected":
                    return Properties.Resources.equalright_expected;
                case   "\"?\" expected":
                    return Properties.Resources.question_expected;
                case   "\"try\" expected":
                    return Properties.Resources.try_expected;
                case   "\"catch\" expected":
                    return Properties.Resources.catch_expected;
                case  "\"+\" expected":
                    return Properties.Resources.add_expected;
                case   "\"*\" expected":
                    return Properties.Resources.asterisk_expected;
                case   "\"/\" expected":
                    return Properties.Resources.divider_expected;
                case   "\"%\" expected":
                    return Properties.Resources.reminder_expected;
                case  "\"&\" expected":
                    return Properties.Resources.and_expected;
                case   "\"^\" expected":
                    return Properties.Resources.power_expected;
                case  "\"&&\" expected":
                    return Properties.Resources.andand_expected;
                case  "\"||\" expected":
                    return Properties.Resources.oror_expected;
                case   "\"~\" expected":
                    return Properties.Resources.curvedash_expected;
                case  "\"++\" expected":
                    return Properties.Resources.addadd_expected;
                case  "\"--\" expected":
                    return Properties.Resources.dashdash_expected;
                case   "\"#\" expected":
                    return Properties.Resources.hax_expected;
                case  "\"in\" expected":
                    return Properties.Resources.in_expected;
                case  "??? expected":
                    return Properties.Resources.triquestionmark_expected;
                case "';' is expected":
                    return Properties.Resources.SemiColonExpected;
                case  "invalid Hydrogen":
                    return Properties.Resources.invalid_Hydrogen;
                case   "this symbol not expected in Import_Statement":
                    return Properties.Resources.this_symbol_not_expected_in_Import_Statement;
                case  "invalid Import_Statement":
                    return Properties.Resources.invalid_Import_Statement;
                case   "this symbol not expected in Associative_Statement":
                    return Properties.Resources.this_symbol_not_expected_in_Associative_Statement;
                case  "invalid Associative_Statement":
                    return Properties.Resources.invalid_Associative_Statement;
                case   "invalid Associative_functiondecl":
                    return Properties.Resources.invalid_Associative_functiondecl;
                case   "invalid Associative_classdecl":
                    return Properties.Resources.invalid_Associative_classdecl;             
                case   "this symbol not expected in Associative_NonAssignmentStatement":
                    return Properties.Resources.this_symbo_no_expected_in_Associative_NonAssignmentStatement;
                case   "this symbol not expected in Associative_FunctionCallStatement":
                    return Properties.Resources.this_symbol_not_expected_in_Associative_FunctionCallStatement;
                case  "this symbol not expected in Associative_FunctionalStatement":
                    return Properties.Resources.this_symbol_not_expected_in_Associative_FunctionalStatement;           
                case   "invalid Associative_FunctionalStatement":
                    return Properties.Resources.invalid_Associative_FunctionalStatement;           
                case   "invalid Associative_LanguageBlock":
                    return Properties.Resources.invalid_Associative_FunctionalStatement;               
                case   "invalid Associative_AccessSpecifier":
                    return Properties.Resources.invalid_Associative_AccessSpecifier;
                case   "invalid Associative_BinaryOps":
                    return Properties.Resources.invalid_Associative_BinaryOps;
                case   "invalid Associative_AddOp":
                    return Properties.Resources.invalid_Associative_AddOp;
                case   "invalid Associative_MulOp":
                    return Properties.Resources.invalid_Associative_MulOp;
                case   "invalid Associative_ComparisonOp":
                    return Properties.Resources.invalid_Associative_ComparisonOp;
                case   "invalid Associative_LogicalOp":
                    return Properties.Resources.invalid_Associative_LogicalOp;
                case   "invalid Associative_DecoratedIdentifier":
                    return Properties.Resources.invalid_Associative_DecoratedIdentifier;             
                case   "invalid Associative_UnaryExpression":
                    return Properties.Resources.invalid_Associative_UnaryExpression;
                case   "invalid Associative_unaryop":
                    return Properties.Resources.invalid_Associative_unaryop;
                case   "invalid Associative_Factor":
                    return Properties.Resources.invalid_Associative_Factor;
                case   "invalid Associative_negop":
                    return Properties.Resources.invalid_Associative_negop;
                case   "invalid Associative_BitOp":
                    return Properties.Resources.invalid_Associative_BitOp;
                case  "invalid Associative_PostFixOp":
                    return Properties.Resources.invalid_Associative_PostFixOp;
                case   "invalid Associative_Number":
                    return Properties.Resources.invalid_Associative_Number;
                case "invalid Associative_Level":
                    return Properties.Resources.invalid_Associative_Level;
                case "invalid Associative_NameReference":
                    return Properties.Resources.invalid_Associative_NameReference;                        
                case  "invalid Imperative_stmt":
                    return Properties.Resources.invalid_Imperative_stmt;
                case "invalid Imperative_functiondecl":
                    return Properties.Resources.invalid_Imperative_functiondecl;
                case  "invalid Imperative_languageblock":
                    return Properties.Resources.invalid_Imperative_languageblock;               
                case   "invalid Imperative_ifstmt":
                    return Properties.Resources.invalid_Imperative_ifstmt;             
                case   "invalid Imperative_forloop":
                    return Properties.Resources.invalid_Imperative_forloop;
                case   "invalid Imperative_assignstmt":
                    return Properties.Resources.invalid_Imperative_assignstmt;            
                case   "invalid Imperative_decoratedIdentifier":
                    return Properties.Resources.invalid_Imperative_decoratedIdentifier;
                case   "invalid Imperative_NameReference":
                    return Properties.Resources.invalid_Imperative_NameReference;
                case  "invalid Imperative_unaryexpr":
                    return Properties.Resources.invalid_Imperative_unaryexpr;
                case  "invalid Imperative_unaryop":
                    return Properties.Resources.invalid_Imperative_unaryexpr;
                case  "invalid Imperative_factor":
                    return Properties.Resources.invalid_Imperative_factor;
                case   "invalid Imperative_logicalop":
                    return Properties.Resources.invalid_Imperative_logicalop;
                case   "invalid Imperative_relop":
                    return Properties.Resources.invalid_Imperative_relop;
                case   "invalid Imperative_addop":
                    return Properties.Resources.invalid_Imperative_addop;
                case  "invalid Imperative_mulop":
                    return Properties.Resources.invalid_Imperative_mulop;
                case   "invalid Imperative_bitop":
                    return Properties.Resources.invalid_Imperative_bitop;
                case  "invalid Imperative_num":
                    return Properties.Resources.invalid_Imperative_num;
                case   "invalid Imperative_PostFixOp":
                    return Properties.Resources.invalid_Imperative_PostFixOp;
                default :
                    return errorMessage;
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
            string message = string.Format(Resources.kMultipleSymbolFoundFromName, symbolName, "");
            message += String.Join(", ", collidingSymbolNames);
            LogWarning(BuildData.WarningID.MultipleSymbolFoundFromName, message);
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
            LogWarning(BuildData.WarningID.IdUnboundIdentifier, message, core.CurrentDSFileName, line, col, graphNode, unboundSymbol);
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
                }
            }
        }
	}
}


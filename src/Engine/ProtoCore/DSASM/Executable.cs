﻿using System;
using System.Collections.Generic;

namespace ProtoCore.DSASM
{
    /// <summary>
    /// InstructionStream holds the executable dsasm code and relevant information
    /// </summary>
    /// 
    public class InstructionStream
    {
        public ProtoCore.Language language { get; set; }
        public int entrypoint { get; set; }
        public List<Instruction> instrList { get; set; }
        public ProtoCore.AssociativeGraph.DependencyGraph dependencyGraph { get; set; }
        public List<ProtoCore.AssociativeGraph.UpdateNodeRef> xUpdateList { get; set; }

        public InstructionStream(ProtoCore.Language langId, ProtoCore.Core core)
        {
            language = langId;
            entrypoint = Constants.kInvalidIndex;
            instrList = new List<Instruction>();
            dependencyGraph = new ProtoCore.AssociativeGraph.DependencyGraph(core);
            xUpdateList = new List<AssociativeGraph.UpdateNodeRef>();
        }
    }

    /// <summary>
    /// Executable holds the body of code that will be executed along with associated
    /// meta-information
    /// </summary>
    /// 
    public class Executable
    {
        // Constants that represent indices into Executable data
        public enum OffsetConstants
        {
            kInstrStreamGlobalScope = 0 // Offset into the instruction stream where global scope instructions are stored
        }

        /// <summary>
        /// RuntimeData is set in the executable to isolate data passed to the runtime VM
        /// The RuntimeData will eventually be integrated completely into executable,
        /// this means moving RuntimeData properties to Executable and deprecating the RuntimeData object
        /// </summary>
        public RuntimeData RuntimeData { get; set; }

        public bool isSingleAssocBlock { get; set; }
        public ProtoCore.DSASM.ClassTable classTable { get; set; }
        public ProtoCore.DSASM.ProcedureTable[] procedureTable { get; set; }
        public ProtoCore.DSASM.SymbolTable[] runtimeSymbols { get; set; }

        public InstructionStream[] instrStreamList { get; set; } 
        public InstructionStream iStreamCanvas { get; set; }

        public DebugServices.EventSink EventSink = new DebugServices.ConsoleEventSink();

        public Executable()
        {
            Reset();
        }

        public void Reset()
        {
            isSingleAssocBlock = true;
            runtimeSymbols = null;
            procedureTable = null;
            classTable = null;
            instrStreamList = null;
            iStreamCanvas = null;
            RuntimeData = null;
        }
    }

    public enum CodeBlockType
    {
        kLanguage,
        kConstruct,
        kFunction,
        kTypesMax
    }

    public class CodeBlock
    {
        public Guid guid { get; set; }
        public CodeBlockType blockType { get; set; }

        public CodeBlock parent { get; set; }
        public List<CodeBlock> children { get; set; }

        public ProtoCore.Language language { get; private set; }
        public int globsize { get; set; }
        public int codeBlockId { get; private set; }

        public ProtoCore.DSASM.SymbolTable symbolTable { get; set; }
        public ProtoCore.DSASM.ProcedureTable procedureTable { get; private set; }
        public List<AttributeEntry> Attributes { get; set; }
        public InstructionStream instrStream { get; set; }

        public DebugServices.EventSink EventSink = new DebugServices.ConsoleEventSink();

        public bool isBreakable { get; set; }

        public CodeBlock(Guid guid, CodeBlockType type, ProtoCore.Language langId, int cbID, SymbolTable symbols, ProcedureTable procTable, bool isBreakableBlock = false, ProtoCore.Core core = null)
        {
            this.guid = guid;
            blockType = type;

            parent = null;
            children = new List<CodeBlock>();

            language = langId;
            instrStream = new InstructionStream(langId, core);

            symbolTable = symbols;
            procedureTable = procTable;

            isBreakable = isBreakableBlock;
            core.CompleteCodeBlockList.Add(this);
            this.codeBlockId = core.CompleteCodeBlockList.Count - 1;

            symbols.RuntimeIndex = this.codeBlockId;

            if (core.ProcNode != null)
            {
                core.ProcNode.ChildCodeBlocks.Add(codeBlockId);
            }
        }

        public bool IsMyAncestorBlock(int blockId)
        {
            CodeBlock ancestor = this.parent;
            while (ancestor != null)
            {
                if (ancestor.codeBlockId == blockId)
                {
                    return true;
                }
                else
                {
                    ancestor = ancestor.parent;
                }
            }
            return false;
        }
    }
}

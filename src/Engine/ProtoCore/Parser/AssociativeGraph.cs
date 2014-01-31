
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using ProtoCore;
using ProtoCore.AST;

namespace ProtoCore.AST.AssociativeAST
{
    public abstract class AssociativeNode : Node
	{
	}

    public class LanguageBlockNode : AssociativeNode
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
        }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }

        public Node CodeBlockNode { get; set; }
    }
	
	/// <summary>
	/// This node will be used by the optimiser
	/// </summary>
    public class MergeNode : AssociativeNode
	{
		public List<AssociativeNode> MergedNodes  {
			get;
			private set;
		}
		
		public MergeNode ()
		{
			MergedNodes = new List<AssociativeNode>();
		}
	}

    public class IdentifierNode : AssociativeNode
	{
        public IdentifierNode()
        {
            type = (int)ProtoCore.PrimitiveType.kInvalidType;
        }
        
        public int type {
            get;
            set;
        }

        public ProtoCore.PrimitiveType datatype {
            get;
            set;
        }

		public string Value {
			get;
			set;
		}

        public ArrayNode ArrayDimensions 
        { 
            get; 
            set; 
        }

        public List<AssociativeNode> ReplicationGuides
        {
            get;
            set;
        }
	}

    public class IdentifierListNode : AssociativeNode
    {
        public AssociativeNode LeftNode
        {
            get;
            set;
        }

        public ProtoCore.DSASM.Operator Optr
        {
            get;
            set;
        }

        public AssociativeNode RightNode
        {
            get;
            set;
        }
    }

    public class IntNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class DoubleNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class BooleanNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class CharNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class StringNode : AssociativeNode
    {
        public string value { get; set; }
    }

    public class NullNode : AssociativeNode
    {
    }

    public class ReturnNode : AssociativeNode
    {
        public AssociativeNode ReturnExpr
        {
            get;
            set;
        }
    }

	public class FunctionCallNode : AssociativeNode
	{
        public AssociativeNode Function { get; set; }
        public List<AssociativeNode> FormalArguments { get; set; }
		
		public FunctionCallNode ()
		{
			FormalArguments = new List<AssociativeNode>();
		}
	}    

    public class QualifiedNode : AssociativeNode
    {
        public AssociativeNode Value { get; set; }

        public List<AssociativeNode> ReplicationGuides { get; set; }
    }

    public class VarDeclNode : AssociativeNode
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;            
        }

        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public AssociativeNode NameNode { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
    }

    public class ArgumentSignatureNode : AssociativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<VarDeclNode>();
        }

        public List<VarDeclNode> Arguments { get; set; }

        public void AddArgument(VarDeclNode arg)
        {
            Arguments.Add(arg);
        }
    }

    public class CodeBlockNode : AssociativeNode
    {
        public ProtoCore.DSASM.SymbolTable symbols { get; set; }
        public ProtoCore.DSASM.ProcedureTable procTable { get; set; }

        public CodeBlockNode()
        {
            Body = new List<AssociativeNode>();
            symbols = new ProtoCore.DSASM.SymbolTable("AST generated", ProtoCore.DSASM.Constants.kInvalidIndex);
            procTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        public List<AssociativeNode> Body { get; set; }
    }

    public class ClassDeclNode : AssociativeNode
    {
        public ClassDeclNode()
        {
            varlist = new List<AssociativeNode>();
            funclist = new List<AssociativeNode>();
        }
        public string name { get; set; }
        public List<string> superClass { get; set; }
        public List<AssociativeNode> varlist { get; set; }
        public List<AssociativeNode> funclist { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }
    }

    public class ConstructorDefinitionNode : AssociativeNode
    {
        public int localVars { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public AssociativeNode Pattern { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public FunctionCallNode baseConstr { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }
    }

    public class FunctionDefinitionNode : AssociativeNode
    {
        public CodeBlockNode FunctionBody { get; set; }

        public ProtoCore.Type ReturnType { get; set; }
        public ArgumentSignatureNode Singnature { get; set; }
        public AssociativeNode Pattern { get; set; }
        public bool IsExternLib { get; set; }
        public bool IsDNI { get; set; }
        public string ExternLibName { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
    }

    public class IfStatementNode : AssociativeNode
    {
        public AssociativeNode ifExprNode { get; set; }
        public List<AssociativeNode> IfBody { get; set; }
        public List<AssociativeNode> ElseBody { get; set; }
    }

    public class InlineConditionalNode : AssociativeNode
    {
        public AssociativeNode ConditionExpression { get; set; }
        public AssociativeNode TrueExpression { get; set; }
        public AssociativeNode FalseExpression { get; set; }
    }

	public class BinaryExpressionNode : AssociativeNode
	{
        public AssociativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public AssociativeNode RightNode { get; set; }
	}

    public class UnaryExpressionNode : AssociativeNode
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public AssociativeNode Expression { get; set; }
    }

	
	public class ModifierStackNode : AssociativeNode
	{
		public ModifierStackNode ()
		{
			ElementNodes = new List<AssociativeNode>();
			AtNames = new Dictionary<string, AssociativeNode>();
		}

        public void AddElementNode(AssociativeNode n, string name)
        {
            ElementNodes.Add(n);
            if (name != string.Empty)
            {
                AtNames.Add(name, n);
                BinaryExpressionNode o = n as BinaryExpressionNode;
                IdentifierNode t = o.LeftNode as IdentifierNode;
                BinaryExpressionNode e = new BinaryExpressionNode();
                e.LeftNode = new IdentifierNode() { Value = name, Name = name, type = t.type, datatype = t.datatype };
                e.RightNode = t;
                e.Optr = ProtoCore.DSASM.Operator.assign;
                ElementNodes.Add(e);
            }
        }
		
		public List<AssociativeNode> ElementNodes  { get; private set; }		
		public AssociativeNode ReturnNode { get; set; }		
		public Dictionary<string, AssociativeNode> AtNames { get; private set; }
	}

    public class RangeExprNode : AssociativeNode
    {
        public RangeExprNode()
        {
            IntNode defaultStep = new IntNode();
            defaultStep.value = "1";
            StepNode = defaultStep;            
        }

        public AssociativeNode FromNode { get; set; }
        public AssociativeNode ToNode { get; set; }
        public AssociativeNode StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }
       
    }

    public class ExprListNode : AssociativeNode
    {
        public ExprListNode()
        {
            list = new List<AssociativeNode>();
        }

        public List<AssociativeNode> list { get; set; }
    }

    public class ForLoopNode : AssociativeNode
    {
        public AssociativeNode id { get; set; }
        public AssociativeNode expression { get; set; }
        public List<AssociativeNode> body { get; set; }
    }

    public class ArrayNode : AssociativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }
        public AssociativeNode Expr { get; set; }
        public AssociativeNode Type { get; set; }
    }

    public class ImportNode: AssociativeNode
    {
        public ImportNode()
        {
            HasBeenImported = false;
            Identifiers = new HashSet<string>();
        }

        public CodeBlockNode CodeNode {get; set;}
        public bool HasBeenImported { get; set; }
        public HashSet<string> Identifiers { get; set; }

        private string moduleName;
        public string ModuleName
        {
            get
            {
                return moduleName;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    moduleName = ProtoCore.Utils.FileUtils.GetFullPathName(value.Replace("\"", String.Empty));
            }
        }
    }

    public class PostFixNode : AssociativeNode
    {
        public AssociativeNode Identifier { get; set; }
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
    }

    public class BreakNode : AssociativeNode
    {
    }

    public class ContinueNode : AssociativeNode
    {
    }
}


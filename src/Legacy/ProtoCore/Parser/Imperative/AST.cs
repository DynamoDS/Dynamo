

using System;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoCore;


namespace ProtoImperative.AST
{
	public class GenAST
	{
        List<Node> sourceAST;

        public GenAST()
		{
            sourceAST = new List<Node>();
		}


        public CodeBlockNode GenerateAST(string pathFilename, ProtoCore.Core core)
        {
            Imperative.Scanner s = new Imperative.Scanner(pathFilename);
            Imperative.Parser p = new Imperative.Parser(s, core);
            p.Parse();
            return p.codeblock as CodeBlockNode;
        }
	}
	

	public abstract class Node: ProtoCore.NodeBase
	{
        private static int sID = 0;
        
        public int ID
        {
            get;
            private set;
        }
        public Node()
        {
            ID = ++sID;
        }

		public string Name {
			get;
			set;
		}
	}

    public class LanguageBlockNode : Node
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
        }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }

        public NodeBase codeBlockNode { get; set; }
    }


    public class ClassDeclNode : Node
    {
        public ClassDeclNode()
        {
            varlist = new List<Node>();
            funclist = new List<Node>();
        }
        public string name { get; set; }
        public List<Node> varlist { get; set; }
        public List<Node> funclist { get; set; }
    }

    public class IdentifierNode : Node
	{
        public IdentifierNode() 
        {
            type = (int)ProtoCore.PrimitiveType.kInvalidType;
            ArrayDimensions = null;
        }

        public int type { get; set; }
        public ProtoCore.PrimitiveType datatype { get; set; }
        public string Value { get; set; }
        public ArrayNode ArrayDimensions { get; set; }
	}

    public class IntNode: Node
    {
        public string value { get; set; }
    }

    public class DoubleNode : Node
    {
        public string value { get; set; }
    }

    public class BooleanNode : Node
    {
        public string value { get; set; }
    }

    public class CharNode : Node
    {
        public string value { get; set; }
    }

    public class StringNode : Node
    {
        public string value { get; set; }
    }

    public class NullNode : Node
    {
    }

    public class ArrayNode : Node
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }
        public Node Expr { get; set; }
        public Node Type { get; set; }
    }

	public class FunctionCallNode : Node
	{
		public Node Function  
        {
			get;
			set;
		}
		
		public List<Node> FormalArguments  
        {
			get;
			set;
		}
		
		public FunctionCallNode ()
		{
			FormalArguments = new List<Node>();
		}
	}

    public class VarDeclNode : Node
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;            
        }

        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public Node NameNode { get; set; }        
    }

    public class ReturnNode : Node
    {
        public Node ReturnExpr { get; set; }
    }

    public class ArgumentSignatureNode : Node
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

    public class ExprListNode : Node
    {
        public ExprListNode()
        {
            list = new List<Node>();
        }

        public List<Node> list { get; set; }
    }

    public class CodeBlockNode : Node
    {
        public CodeBlockNode()
        {
            Body = new List<Node>();
        }

        public List<Node> Body { get; set; }
    }

    public class ConstructorDefinitionNode : Node
    {
        public int localVars { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
    }

    public class FunctionDefinitionNode : Node
    {
        public int localVars { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
    }

    public class InlineConditionalNode : Node
    {
        public Node ConditionExpression { get; set; }
        public Node TrueExpression { get; set; }
        public Node FalseExpression { get; set; }
    }

	public class BinaryExpressionNode : Node
	{
        public Node LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public Node RightNode { get; set; }
	}


    public class ElseIfBlock : Node
    {
        public ElseIfBlock()
        {
            Body = new List<Node>();
        }
        public Node Expr { get; set; }
        public List<Node> Body {get;set;}
    }

    public class IfStmtNode : Node
    {
        public IfStmtNode()
        {
            ElseIfList = new List<ElseIfBlock>();
            IfBody = new List<Node>();
            ElseBody = new List<Node>();
        }

        public Node IfExprNode { get; set; }
        public List<Node> IfBody { get; set; }
        public List<ElseIfBlock> ElseIfList { get; set; }
        public List<Node> ElseBody { get; set; }
    }

    public class WhileStmtNode : Node
    {
        public WhileStmtNode()
        {
            Body = new List<Node>();
        }

        public Node Expr { get; set; }
        public List<Node> Body { get; set; }
    }

    public class UnaryExpressionNode : Node
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public Node Expression { get; set; }
    }

    public class RangeExprNode : Node
    {
        public RangeExprNode()
        {
            IntNode defaultStep = new IntNode();
            defaultStep.value = "1";
            StepNode = defaultStep;
            stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize;
        }

        public Node FromNode {get; set; }
        public Node ToNode { get; set; }
        public Node StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }        
    }

    public class ForLoopNode : Node
    {
        public ForLoopNode()
        {
            body = new List<Node>();
        }
        public Node id { get; set; }
        public Node expression { get; set; }
        public List<Node> body { get; set; }
    }

    public class IdentifierListNode : Node
    {
        public Node LeftNode{ get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public Node RightNode { get; set; }
    }
   
}


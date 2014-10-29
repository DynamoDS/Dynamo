
#define TEST_DIRECT 

using System;
using System.Diagnostics;
using System.Collections.Generic;
using ProtoCore;
 

namespace ProtoAssociative.DependencyPass
{
	//@TODO: Replace this whole structure with a DS-Hydrogen implementation

	public class AST
	{
		public AST ()
		{
		}

        public DependencyPass.DependencyTracker GetDemoTracker2(ProtoCore.Core core)
        {
            Associative.Scanner s = new Associative.Scanner(@"..\..\Scripts\expr.ds");
            Associative.Parser p = new Associative.Parser(s, core);
            p.Parse();

            CodeBlockNode code = p.codeblock as CodeBlockNode;

            DependencyTracker tempTracker = new DependencyTracker();
            Dictionary<string, List<Node>> names = new Dictionary<string, List<Node>>();
            code.ConsolidateNames(ref(names));
            tempTracker.GenerateDependencyGraph(code.Body);
            return tempTracker;
        }

        public DependencyPass.DependencyTracker GetDemoTracker3(out ProtoCore.DSASM.SymbolTable symbols, string pathFilename, ProtoCore.Core core)
        {
            Associative.Scanner s = new Associative.Scanner(pathFilename);
            Associative.Parser p = new Associative.Parser(s, core); 
            p.Parse();
            CodeBlockNode code = p.codeblock as CodeBlockNode;
            symbols = code.symbols;
            
            DependencyTracker tempTracker = new DependencyTracker();

#if TEST_DIRECT
            foreach (Node node in code.Body)
            {
                tempTracker.AllNodes.Add(node);
            }
#else
            Dictionary<string, List<Node>> names = new Dictionary<string, List<Node>>();
            code.ConsolidateNames(ref(names));
            tempTracker.GenerateDependencyGraph(code.Body);
#endif
            return tempTracker;
        }
        /*
        public DependencyPass.DependencyTracker generateAST(ProtoCore.CodeBlock codeblock)
        {
            DependencyTracker tempTracker = new DependencyTracker();

            foreach (Object obj in codeblock.Body)
            {
                Debug.Assert(obj is ProtoAssociative.DependencyPass.Node);
                Node node = obj as ProtoAssociative.DependencyPass.Node;
                tempTracker.AllNodes.Add(node);
            }
            return tempTracker;
        }
         * */

		public DependencyPass.DependencyTracker GetDemoTracker()
		{
            IdentifierNode a = new IdentifierNode();
			a.Value = "1..1000..+1";
			
			FunctionCallNode b = new FunctionCallNode();
            b.Function = new IdentifierNode() { Value = "SQRT" };
			b.FormalArguments.Add(a);
			
			BinaryExpressionNode c = new BinaryExpressionNode();
			c.LeftNode = a;
            c.Optr = ProtoCore.DSASM.Operator.mul;
            IdentifierNode _2Node = new IdentifierNode() { Value = "2" };
			c.RightNode = _2Node;
			
			BinaryExpressionNode d = new BinaryExpressionNode();
			d.LeftNode = c;
			d.RightNode = c;
			d.Optr = ProtoCore.DSASM.Operator.mul;
			
			FunctionCallNode e = new FunctionCallNode();
            e.Function = new IdentifierNode() { Value = "LineFromPoint" };
			e.FormalArguments.Add(a);
			e.FormalArguments.Add(b);
			e.FormalArguments.Add(d);

            Node f = new FunctionCallNode() { Function = new IdentifierNode() { Value = "Trim" } };
            Node g = new FunctionCallNode() { Function = new IdentifierNode() { Value = "Rotate" } };

			DependencyPass.DependencyTracker tracker = new DependencyPass.DependencyTracker();
			tracker.AllNodes.Add(a);
			tracker.AllNodes.Add(b);
			tracker.AllNodes.Add(c);
			tracker.AllNodes.Add(_2Node);
			tracker.AllNodes.Add(d);
			tracker.AllNodes.Add(e);
			tracker.AllNodes.Add(f);
			tracker.AllNodes.Add(g);
			
			tracker.DirectContingents.Add(a, new List<Node>() { });
			tracker.DirectContingents.Add(_2Node, new List<Node>() { });
			
			tracker.DirectContingents.Add(b, new List<Node>() { a });
			tracker.DirectContingents.Add(c, new List<Node>() { a, _2Node });
			tracker.DirectContingents.Add(d, new List<Node>() { c });
			tracker.DirectContingents.Add(e, new List<Node>() { a, b, d });
			tracker.DirectContingents.Add(f, new List<Node>() { e });
			tracker.DirectContingents.Add(g, new List<Node>() { f });
	
			tracker.DirectDependents.Add(a, new List<Node>() {b, c, e});
			tracker.DirectDependents.Add(b, new List<Node>() {e});
			tracker.DirectDependents.Add(c, new List<Node>() {d});
			tracker.DirectDependents.Add(d, new List<Node>() {e});
			tracker.DirectDependents.Add(e, new List<Node>() {f});
			tracker.DirectDependents.Add(f, new List<Node>() {g});
			tracker.DirectDependents.Add(g, new List<Node>() {});
			
			tracker.DirectDependents.Add(_2Node, new List<Node>() {c});
			
			return tracker;
		}


        // a = 25
        // b = 4 + 20 / 5
        // c = a - 20 * 5
        public DependencyPass.DependencyTracker GetDemoTrackerJun()
        {
            DependencyPass.DependencyTracker tracker = new DependencyPass.DependencyTracker();

            
            string varIdent = null;

            //========================================================
            // a = 25
            BinaryExpressionNode i1 = new BinaryExpressionNode();

            varIdent = "a";
            IdentifierNode fAssignLeft = new IdentifierNode() { Value = varIdent, type = /*SDD*/(int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i1.LeftNode = fAssignLeft;
            i1.Optr = ProtoCore.DSASM.Operator.assign;
            IdentifierNode fAssignRight = new IdentifierNode() { Value = "25", type = (int)ProtoCore.PrimitiveType.kTypeInt };
            i1.RightNode = fAssignRight;

            //========================================================


            // b = 4 + 20 / 5

            // 20 / 5
            BinaryExpressionNode sDiv = new BinaryExpressionNode();
            IdentifierNode sDivLeft = new IdentifierNode() { Value = "20", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sDiv.LeftNode = sDivLeft;
            sDiv.Optr = ProtoCore.DSASM.Operator.div;
            IdentifierNode sDivRight = new IdentifierNode() { Value = "5", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sDiv.RightNode = sDivRight;


            // 4 + ( 20 / 5 )
            BinaryExpressionNode sAdd = new BinaryExpressionNode();
            IdentifierNode sAddLeft = new IdentifierNode() { Value = "4", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sAdd.LeftNode = sAddLeft;
            sAdd.Optr = ProtoCore.DSASM.Operator.add;
            BinaryExpressionNode sAddRight = new BinaryExpressionNode();
            sAddRight = sDiv;
            sAdd.RightNode = sAddRight;


            // b = 4 + 20 / 5
            BinaryExpressionNode i2 = new BinaryExpressionNode();

            varIdent = "b";
            IdentifierNode sAssignLeft = new IdentifierNode() { Value = varIdent, /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i2.LeftNode = sAssignLeft;
            i2.Optr = ProtoCore.DSASM.Operator.assign;
            BinaryExpressionNode sAssignRight = new BinaryExpressionNode();
            sAssignRight = sAdd;
            i2.RightNode = sAssignRight;



            // c = a - 20 * 5

            // 20 * 5
            BinaryExpressionNode sMul = new BinaryExpressionNode();
            IdentifierNode sMulLeft = new IdentifierNode() { Value = "20", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sMul.LeftNode = sMulLeft;
            sMul.Optr = ProtoCore.DSASM.Operator.mul;
            IdentifierNode sMulRight = new IdentifierNode() { Value = "5", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sMul.RightNode = sMulRight;


            // a - ( 20 * 5 )
            BinaryExpressionNode sSub = new BinaryExpressionNode();
            IdentifierNode sSubLeft = new IdentifierNode() { Value = "a", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
            sSub.LeftNode = sSubLeft;
            sSub.Optr = ProtoCore.DSASM.Operator.sub;
            BinaryExpressionNode sSubRight = new BinaryExpressionNode();
            sSubRight = sMul;
            sSub.RightNode = sSubRight;


            // c = a - 20 * 5
            BinaryExpressionNode i3 = new BinaryExpressionNode();

            varIdent = "c";
            IdentifierNode si3Left = new IdentifierNode() { Value = varIdent, /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i3.LeftNode = si3Left;
            i3.Optr = ProtoCore.DSASM.Operator.assign;
            BinaryExpressionNode si3Right = new BinaryExpressionNode();
            si3Right = sSub;
            i3.RightNode = si3Right;


            tracker.AllNodes.Add(i1);
            tracker.AllNodes.Add(i2);
            tracker.AllNodes.Add(i3);

            return tracker;
        }
	}
	
	public abstract class Node: ProtoCore.NodeBase
	{
        private static int sID = 0;
        //allow the assignment node to be part of dependency struture?
        //this lead to entiely different set of results in optimization
        protected static bool AssignNodeDependencyEnabled = true;
        
        //even if the '=' is not a link between LHS and RHS, can we keep it in dependency graph?
        //protected static bool AssignNodeDependencyEnabledLame = true;

        public int ID
        {
            get;
            private set;
        }
        public Node()
        {
            ID = ++sID;
        }
				/// <summary>
		/// Optional name for the node
		/// </summary>
		public string Name  {
			get;
			set;
		}

        public virtual void GenerateDependencyGraph(DependencyTracker tracker)
        {
            tracker.AddNode(this);//get rid of this later

            IEnumerable<Node> contingents = getContingents();
            
            foreach (Node node in contingents)
            {
                tracker.AddNode(node);
                if (node == null)
                    continue;
                tracker.AddDirectContingent(this, node);
                tracker.AddDirectDependent(node, this);
                node.GenerateDependencyGraph(tracker);
            }
        }

        public virtual IEnumerable<Node> getContingents()
        {
            return new List<Node>();
        }

        public virtual void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
        }

        protected static void Consolidate(ref Dictionary<string, List<Node>> names, ref IdentifierNode node)
        {
            if (null != node.Name)
            {
                if (names.ContainsKey(node.Name))
                {
                    List<Node> candidates = names[node.Name];
                    node = candidates[candidates.Count - 1] as IdentifierNode;
                }
                else
                {
                    //symbol not defined.
                    //should we ignore this until somebody else defines a symbol? 
                    //or add the symbol?
                    //throw new KeyNotFoundException();
                    List<Node> candidates = new List<Node>();
                    candidates.Add(node);
                    names.Add(node.Name, candidates);
                }
            }
        }
	}

    public class LanguageBlockNode : Node
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
        }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }

        public ProtoCore.NodeBase codeBlockNode { get; set; }
    }
	
	/// <summary>
	/// This node will be used by the optimiser
	/// </summary>
	public class MergeNode : Node
	{
		public List<Node> MergedNodes  {
			get;
			private set;
		}
		
		public MergeNode ()
		{
			MergedNodes = new List<Node>();
		}

        public override IEnumerable<Node> getContingents()
        {
            return MergedNodes;
        }
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            foreach (Node node in MergedNodes)
                node.ConsolidateNames(ref(names));
        }

	}
	
	public class IdentifierNode : Node
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

        public List<Node> ReplicationGuides
        {
            get;
            set;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            throw new NotImplementedException(); //we should not be here at all. the parent node should take care.
            //disabling execption as functioncalls will still need to add the nodes to 
        }
	}

    public class IdentifierListNode : Node
    {
        public Node LeftNode
        {
            get;
            set;
        }

        public ProtoCore.DSASM.Operator Optr
        {
            get;
            set;
        }

        public Node RightNode
        {
            get;
            set;
        }
    }

    public class IntNode : Node
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

    public class ReturnNode : Node
    {
        public Node ReturnExpr
        {
            get;
            set;
        }
    }

	public class FunctionCallNode : Node
	{
        public Node Function { get; set; }
        public List<Node> FormalArguments { get; set; }
		
		public FunctionCallNode ()
		{
			FormalArguments = new List<Node>();
		}

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(FormalArguments) ;
            contingents.Add(Function);
            return contingents;
        }
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            List<Node> newFormalArguments = new List<Node>();
            //replace the names in arguments by current names in calling context
            foreach(Node argument in FormalArguments)
            {
                Node newArgument = argument;
                IdentifierNode terminalNode = newArgument as IdentifierNode;
                if (terminalNode != null)
                {
                    Consolidate(ref(names), ref(terminalNode));
                    newArgument = terminalNode;
                }
                else
                {
                    argument.ConsolidateNames(ref(names));
                }
                newFormalArguments.Add(newArgument);
            }
            FormalArguments = newFormalArguments;
        }
	}    

    public class QualifiedNode : Node
    {
        public Node Value { get; set; }

        public List<Node> ReplicationGuides { get; set; }

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(ReplicationGuides);
            contingents.Add(Value);
            return contingents;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            Value.ConsolidateNames(ref(names));
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
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            if (names.ContainsKey(NameNode.Name))
                throw new Exception(); 
            List<Node> records = new List<Node>();
            records.Add(NameNode);
            names.Add(NameNode.Name, records);

            Dictionary<string, List<Node>> localnames = new Dictionary<string, List<Node>>();
            localnames.Add(NameNode.Name, records);            
        }

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

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(Arguments);
            return contingents;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            foreach (Node node in Arguments)
                node.ConsolidateNames(ref(names));
        }
    }

    public class CodeBlockNode : Node
    {
        public ProtoCore.DSASM.SymbolTable symbols { get; set; }
        public ProtoCore.DSASM.ProcedureTable procTable { get; set; }

        public CodeBlockNode()
        {
            Body = new List<Node>();
            symbols = new ProtoCore.DSASM.SymbolTable("AST generated", ProtoCore.DSASM.Constants.kInvalidIndex);
            procTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        public List<Node> Body { get; set; }

        public override IEnumerable<Node> getContingents()
        {
            return new List<Node>(Body);
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            //todo make a decision whether to pass out the local names.
            foreach (Node node in Body)
            {
                node.ConsolidateNames(ref(names));
            }
        }
    }

    public class ClassDeclNode : Node
    {
        public ClassDeclNode()
        {
            varlist = new List<Node>();
            funclist = new List<Node>();
        }
        public string name { get; set; }
        public List<string> superClass { get; set; }
        public List<Node> varlist { get; set; }
        public List<Node> funclist { get; set; }
    }

    public class ConstructorDefinitionNode : Node
    {
        public int localVars { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public Node Pattern { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public FunctionCallNode baseConstr { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
    }

    public class FunctionDefinitionNode : Node
    {
        public CodeBlockNode FunctionBody { get; set; }

        public ProtoCore.Type ReturnType { get; set; }
        public ArgumentSignatureNode Singnature { get; set; }
        public Node Pattern { get; set; }
        public bool IsExternLib { get; set; }
        public bool IsDNI { get; set; }
        public string ExternLibName { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>();
            contingents.Add(FunctionBody);
            contingents.Add(Singnature);
            contingents.Add(Pattern);
            return contingents;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            Dictionary<string, List<Node>> localNames = new Dictionary<string, List<Node>>();
            Singnature.ConsolidateNames(ref(localNames));
            Pattern.ConsolidateNames(ref(localNames));
            FunctionBody.ConsolidateNames(ref(localNames));
            if (names.ContainsKey(Name))
            {
                throw new Exception();
            }
            List<Node> namelist = new List<Node>();
            namelist.Add(this);
            names.Add(Name, namelist);
        }
    }

    public class IfStatementNode : Node
    {
        public Node ifExprNode { get; set; }
        public List<Node> IfBody { get; set; }
        public List<Node> ElseBody { get; set; }
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

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>();
            if (Optr != ProtoCore.DSASM.Operator.assign)
            {
                contingents.Add(LeftNode);
            }
            //if we have enabled the '=' node to be a part of depencency, then we return RHS, no matter what
            if (AssignNodeDependencyEnabled || Optr != ProtoCore.DSASM.Operator.assign)
            {
                contingents.Add(RightNode);
            }
            return contingents;
        }


        public override void GenerateDependencyGraph(DependencyTracker tracker)
        {
            base.GenerateDependencyGraph(tracker);
            if (Optr == ProtoCore.DSASM.Operator.assign)
            {
                //so do we set dependency between LeftNode and '=' or LeftNode and RightNode : may be later is better
                if (AssignNodeDependencyEnabled)
                {
                    //if we have enabled the '=' node to be a part of depencency, then we already handled RHS as a contingent
                    //so skip it
                    tracker.AddNode(LeftNode);
                    tracker.AddDirectContingent(LeftNode, this);
                    tracker.AddDirectDependent(this, LeftNode);
                }
                else
                {
                    //if(AssignNodeDependencyEnabledLame)
                    //{
                    //    tracker.AddDirectContingent(this, RightNode);  //? still keep in dependency?
                    //    tracker.AddDirectContingent(LeftNode, RightNode); 
                    //}
                    tracker.AddNode(RightNode);
                    tracker.AddNode(LeftNode);
                    tracker.AddDirectContingent(LeftNode, RightNode);
                    tracker.AddDirectDependent(RightNode, LeftNode);
                    RightNode.GenerateDependencyGraph(tracker);
                }
            }
        }


        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            IdentifierNode rightTerminalNode = RightNode as IdentifierNode;
            if (rightTerminalNode != null)
            {
                if (Optr != ProtoCore.DSASM.Operator.dot)
                {
                    //replace RHS
                    Consolidate(ref(names), ref(rightTerminalNode));
                    RightNode = rightTerminalNode;
                }
            }
            else
            {
                RightNode.ConsolidateNames(ref(names));
            }

            //left has to be done 2nd, because in case of modifiers, we dont want to 
            //replace the node on RHS by a node on LHS. So a modifier stack name is not unique.
            IdentifierNode leftTerminalNode = LeftNode as IdentifierNode;
            if (leftTerminalNode != null)
            {
                if (Optr != ProtoCore.DSASM.Operator.assign)
                {
                    //replace LHS
                    Consolidate(ref(names), ref(leftTerminalNode));
                    LeftNode = leftTerminalNode;
                }
                else
                {
                    if (leftTerminalNode.Name != null)
                    {
                        if (names.ContainsKey(leftTerminalNode.Name))
                        {
                            List<Node> candidates = names[leftTerminalNode.Name];
                            candidates.Add(leftTerminalNode);
                        }
                        else
                        {
                            //append LHS
                            List<Node> candidates = new List<Node>();
                            candidates.Add(leftTerminalNode);
                            names.Add(leftTerminalNode.Name, candidates);
                        }
                    }
                    
                }
            }
            else
            {
                LeftNode.ConsolidateNames(ref(names));
            }
        }

	}

    public class UnaryExpressionNode : Node
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public Node Expression { get; set; }

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(1);
            contingents.Add(Expression);
            return contingents;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            Expression.ConsolidateNames(ref(names));
        }
    }

	
	public class ModifierStackNode : Node
	{
		public ModifierStackNode ()
		{
			ElementNodes = new List<Node>();
			AtNames = new Dictionary<string, Node>();
		}

        public void AddElementNode(Node n, string name)
        {
            ElementNodes.Add(n);
            if ("" != name)
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
		
		public List<Node> ElementNodes  { get; private set; }		
		public Node ReturnNode { get; set; }		
		public Dictionary<string, Node> AtNames { get; private set; }
			
        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(ElementNodes);
            contingents.Add(ReturnNode);
            return contingents;
        }
	}

    public class RangeExprNode : Node
    {
        public RangeExprNode()
        {
            IntNode defaultStep = new IntNode();
            defaultStep.value = "1";
            StepNode = defaultStep;            
        }

        public Node FromNode { get; set; }
        public Node ToNode { get; set; }
        public Node StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }
       
    }

    public class ExprListNode : Node
    {
        public ExprListNode()
        {
            list = new List<Node>();
        }

        public List<Node> list { get; set; }
    }

    public class ForLoopNode : Node
    {
        public Node id { get; set; }
        public Node expression { get; set; }
        public List<Node> body { get; set; }
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
}


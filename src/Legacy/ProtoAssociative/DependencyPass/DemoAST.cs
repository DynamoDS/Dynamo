
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

        public DependencyPass.DependencyTracker GetDemoTracker2()
        {
            Associative.Scanner s = new Associative.Scanner(@"..\..\Scripts\expr.ds");
            Associative.Parser p = new Associative.Parser(s);
            p.Parse();

            CodeBlockNode code = p.codeblock;

            DependencyTracker tempTracker = new DependencyTracker();
            Dictionary<string, List<Node>> names = new Dictionary<string, List<Node>>();
            code.ConsolidateNames(ref(names));
            tempTracker.GenerateDependencyGraph(code.Body);
            return tempTracker;
        }

        public DependencyPass.DependencyTracker GetDemoTracker3(out ProtoCore.DSASM.SymbolTable symbols, string pathFilename)
        {
            Associative.Scanner s = new Associative.Scanner(pathFilename);
            Associative.Parser p = new Associative.Parser(s); 
            p.Parse();
            CodeBlockNode code = p.codeblock;
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
			TerminalNode a = new TerminalNode();
			a.Value = "1..1000..+1";
			
			FunctionCallNode b = new FunctionCallNode();
            b.Function = new TerminalNode() { Value = "SQRT" };
			b.FormalArguments.Add(a);
			
			BinaryExpressionNode c = new BinaryExpressionNode();
			c.LeftNode = a;
			c.Operator = Operator.Mult;
			TerminalNode _2Node = new TerminalNode() { Value = "2" };
			c.RightNode = _2Node;
			
			BinaryExpressionNode d = new BinaryExpressionNode();
			d.LeftNode = c;
			d.RightNode = c;
			d.Operator = Operator.Mult;
			
			FunctionCallNode e = new FunctionCallNode();
            e.Function = new TerminalNode() { Value = "LineFromPoint" };
			e.FormalArguments.Add(a);
			e.FormalArguments.Add(b);
			e.FormalArguments.Add(d);

            Node f = new FunctionCallNode() { Function = new TerminalNode() { Value = "Trim" } };
			Node g = new FunctionCallNode() { Function = new TerminalNode() { Value = "Rotate" } };

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
            const int functionIndex = (int)ProtoCore.DSASM.Constants.kGlobalScope;

            //========================================================
            // a = 25
            BinaryExpressionNode i1 = new BinaryExpressionNode();

            varIdent = "a";
            TerminalNode fAssignLeft = new TerminalNode() { Value = varIdent, type = /*SDD*/(int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i1.LeftNode = fAssignLeft;
            i1.Operator = Operator.Assign;
            TerminalNode fAssignRight = new TerminalNode() { Value = "25", type = (int)ProtoCore.PrimitiveType.kTypeInt };
            i1.RightNode = fAssignRight;

            //========================================================


            // b = 4 + 20 / 5

            // 20 / 5
            BinaryExpressionNode sDiv = new BinaryExpressionNode();
            TerminalNode sDivLeft = new TerminalNode() { Value = "20", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sDiv.LeftNode = sDivLeft;
            sDiv.Operator = Operator.Divide;
            TerminalNode sDivRight = new TerminalNode() { Value = "5", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sDiv.RightNode = sDivRight;


            // 4 + ( 20 / 5 )
            BinaryExpressionNode sAdd = new BinaryExpressionNode();
            TerminalNode sAddLeft = new TerminalNode() { Value = "4", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sAdd.LeftNode = sAddLeft;
            sAdd.Operator = Operator.Add;
            BinaryExpressionNode sAddRight = new BinaryExpressionNode();
            sAddRight = sDiv;
            sAdd.RightNode = sAddRight;


            // b = 4 + 20 / 5
            BinaryExpressionNode i2 = new BinaryExpressionNode();

            varIdent = "b";
            TerminalNode sAssignLeft = new TerminalNode() { Value = varIdent, /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i2.LeftNode = sAssignLeft;
            i2.Operator = Operator.Assign;
            BinaryExpressionNode sAssignRight = new BinaryExpressionNode();
            sAssignRight = sAdd;
            i2.RightNode = sAssignRight;



            // c = a - 20 * 5

            // 20 * 5
            BinaryExpressionNode sMul = new BinaryExpressionNode();
            TerminalNode sMulLeft = new TerminalNode() { Value = "20", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sMul.LeftNode = sMulLeft;
            sMul.Operator = Operator.Mult;
            TerminalNode sMulRight = new TerminalNode() { Value = "5", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeInt };
            sMul.RightNode = sMulRight;


            // a - ( 20 * 5 )
            BinaryExpressionNode sSub = new BinaryExpressionNode();
            TerminalNode sSubLeft = new TerminalNode() { Value = "a", /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
            sSub.LeftNode = sSubLeft;
            sSub.Operator = Operator.Subtract;
            BinaryExpressionNode sSubRight = new BinaryExpressionNode();
            sSubRight = sMul;
            sSub.RightNode = sSubRight;


            // c = a - 20 * 5
            BinaryExpressionNode i3 = new BinaryExpressionNode();

            varIdent = "c";
            TerminalNode si3Left = new TerminalNode() { Value = varIdent, /*SDD*/type = (int)ProtoCore.PrimitiveType.kTypeVar };
                // SDD - automatic allocation
                //tracker.Allocate(varIdent, (int)FusionCore.DSASM.Constants.kGlobalScope, (int)FusionCore.DSASM.Constants.kPrimitiveSize);

            i3.LeftNode = si3Left;
            i3.Operator = Operator.Assign;
            BinaryExpressionNode si3Right = new BinaryExpressionNode();
            si3Right = sSub;
            i3.RightNode = si3Right;


            tracker.AllNodes.Add(i1);
            tracker.AllNodes.Add(i2);
            tracker.AllNodes.Add(i3);

            return tracker;
        }
	}
	
	public abstract class Node
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

        protected static void Consolidate(ref Dictionary<string, List<Node>> names, ref TerminalNode node)
        {
            if (null != node.Name)
            {
                if (names.ContainsKey(node.Name))
                {
                    List<Node> candidates = names[node.Name];
                    node = candidates[candidates.Count - 1] as TerminalNode;
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
            codeblock = new ProtoCore.CodeBlock();
        }
        public ProtoCore.CodeBlock codeblock { get; set; }
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
	
	public class TerminalNode : Node
	{
        public TerminalNode() {
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

		public string Value  {
			get;
			set;
		}
        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            throw new NotImplementedException(); //we should not be here at all. the parent node should take care.
            //disabling execption as functioncalls will still need to add the nodes to 
        }
		
	}
	public class FunctionCallNode : Node
	{
		public Node Function  {
			get;
			set;
		}
		
		public List<Node> FormalArguments  {
			get;
			set;
		}
		
		
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
                TerminalNode terminalNode = newArgument as TerminalNode;
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

    public class Pattern : Node
    {
        public Node Expression
        {
            get;
            set;
        }

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

    };

    public class QualifiedNode : Node
    {
        public Node Value
        {
            get;
            set;
        }

        public List<Node> ReplicationGuides
        {
            get;
            set;
        }

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

    public class ArgumentNode : Node
    {
        public ProtoCore.Type ArgumentType
        {
            get;
            set;
        }

        public Pattern Pattern
        {
            get;
            set;
        }

        public TerminalNode NameNode
        {
            get;
            set;
        }

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(1);
            contingents.Add(Pattern);
            return contingents;
        }

        public override void ConsolidateNames(ref Dictionary<string, List<Node>> names)
        {
            if (names.ContainsKey(NameNode.Name))
                throw new Exception(); 
            List<Node> records = new List<Node>();
            records.Add(NameNode);
            names.Add(NameNode.Name, records);

            Dictionary<string, List<Node>> localnames = new Dictionary<string, List<Node>>();
            localnames.Add(NameNode.Name, records);
            Pattern.ConsolidateNames(ref(localnames));
        }

    }

    public class ArgumentSignatureNode : Node
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<ArgumentNode>();
        }

        public List<ArgumentNode> Arguments
        {
            get;
            set;
        }

        public void AddArgument(ArgumentNode arg)
        {
            Arguments.Add(arg);
        }

        public List<KeyValuePair<ProtoCore.Type, Pattern>> ArgumentStructure
        {
            get
            {
                List<KeyValuePair<ProtoCore.Type, Pattern>> argStructure = new List<KeyValuePair<ProtoCore.Type, Pattern>>(); 
                foreach(ArgumentNode i in Arguments)
                {
                    argStructure.Add(new KeyValuePair<ProtoCore.Type, Pattern>(i.ArgumentType, i.Pattern));
                }
                return argStructure;
            }
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
        public ProtoCore.DSASM.FunctionTable functions { get; set; }

        public CodeBlockNode()
        {
            Body = new List<Node>();
            symbols = new ProtoCore.DSASM.SymbolTable();
            functions = new ProtoCore.DSASM.FunctionTable();
        }

        public List<Node> Body
        {
            get;
            set;
        }

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

    public class ConstructorDefinitionNode : Node
    {
        public int localVars
        {
            get;
            set;
        }

        public ArgumentSignatureNode Signature
        {
            get;
            set;
        }

        public CodeBlockNode FunctionBody
        {
            get;
            set;
        }
    }

    public class FunctionDefinitionNode : Node
    {
        public CodeBlockNode FunctionBody
        {
            get;
            set;
        }
       
        public ProtoCore.Type ReturnType
        {
            get;
            set;
        }
        public ArgumentSignatureNode Singnature
        {
            get;
            set;
        }
        public Pattern Pattern
        {
            get;
            set;
        }

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

	public class BinaryExpressionNode : Node
	{
		public Node LeftNode {
			get;
			set;
		}
		
		public Operator Operator {
			get;
			set;
		}
		
		public Node RightNode {
			get;
			set;
		}

        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>();
            if (Operator != Operator.Assign)
            {
                contingents.Add(LeftNode);
            }
            //if we have enabled the '=' node to be a part of depencency, then we return RHS, no matter what
            if (AssignNodeDependencyEnabled || Operator != Operator.Assign)
            {
                contingents.Add(RightNode);
            }
            return contingents;
        }


        public override void GenerateDependencyGraph(DependencyTracker tracker)
        {
            base.GenerateDependencyGraph(tracker);
            if (Operator == Operator.Assign)
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
            TerminalNode rightTerminalNode = RightNode as TerminalNode;
            if (rightTerminalNode != null)
            {
                if (Operator != Operator.Dot)
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
            TerminalNode leftTerminalNode = LeftNode as TerminalNode;
            if (leftTerminalNode != null)
            {
                if (Operator != Operator.Assign)
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
        public UnaryOperator Operator
        {
            get;
            set;
        }

        public Node Expression
        {
            get;
            set;
        }

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

    public enum UnaryOperator
    {
        None,
        Not,
        Negate
    }

	public enum Operator
	{
        Assign,
		LessThan,
        GreaterThan,
        LessThanEqual,
        GreaterThanEqual,
		Equals,
        NotEquals,
		Add,
		Subtract,
		Mult,
		Divide,
		Modulo, 
        And,
        Or,
        Dot
	}
	
	public class ModifierStackNode : Node
	{
		public ModifierStackNode ()
		{
			ElementNodes = new List<Node>();
			AtNames = new Dictionary<string, Node>();
		}
		
		public List<Node> ElementNodes  {
			get;
			private set;
		}
		
		public Node ReturnNode
		{
			get;
			set;
		}
		
		public Dictionary<string, Node> AtNames
		{
			get;
			private set;
		}
			
        public override IEnumerable<Node> getContingents()
        {
            List<Node> contingents = new List<Node>(ElementNodes);
            contingents.Add(ReturnNode);
            return contingents;
        }
	}
}


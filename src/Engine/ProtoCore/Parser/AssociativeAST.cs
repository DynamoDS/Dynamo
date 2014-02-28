using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DesignScriptParser;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore.AST.AssociativeAST
{
    public abstract class AssociativeNode : Node
    {
        public bool IsModifier;

        public AssociativeNode()
        {
        }

        public AssociativeNode(AssociativeNode rhs) : base(rhs)
        {
            IsModifier = rhs.IsModifier;
        }

    }

    public class CommentNode : AssociativeNode
    {
        public enum CommentType { Inline, Block }
        public CommentType Type { get; private set; }
        public string Value { get; private set; }
        public CommentNode(int col, int line, string value, CommentType type)
        {
            this.col = col;
            this.line = line;
            Value = value;
            Type = type;
        }
    }

    public class LanguageBlockNode : AssociativeNode
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
            Attributes = new List<AssociativeNode>();
        }

        public LanguageBlockNode(LanguageBlockNode rhs) : base(rhs)
        {
            CodeBlockNode = NodeUtils.Clone(rhs.CodeBlockNode);

            codeblock = new ProtoCore.LanguageCodeBlock(rhs.codeblock);

            Attributes = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Attributes)
            {
                AssociativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
        }

        public Node CodeBlockNode { get; set; }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }
        public List<AssociativeNode> Attributes { get; set; }

        //only comparing attributes and codeblock at the moment
        public override bool Equals(object other)
        {
            var otherNode = other as LanguageBlockNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(Attributes, otherNode.Attributes);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            string strLang = ProtoCore.Utils.CoreUtils.GetLanguageString(codeblock.language);

            buf.Append("[");
            buf.Append(strLang);
            buf.Append("]");

            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");

            buf.Append(CodeBlockNode.ToString());

            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            return buf.ToString();
        }
    }

    /// <summary>
    /// This node will be used by the optimiser
    /// </summary>
    public class MergeNode : AssociativeNode
    {
        public List<AssociativeNode> MergedNodes
        {
            get;
            private set;
        }

        public MergeNode()
        {
            MergedNodes = new List<AssociativeNode>();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as MergeNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(MergedNodes, otherNode.MergedNodes);
        }
    }

    /// <summary>
    /// This class is only used in GraphCompiler
    /// </summary>
    public class ArrayIndexerNode : AssociativeNode 
    {
        public ArrayNode ArrayDimensions;
        public AssociativeNode Array;

        public override bool Equals(object other)
        {
            var otherNode = other as ArrayIndexerNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<ArrayNode>.Default.Equals(ArrayDimensions, otherNode.ArrayDimensions) &&
                   EqualityComparer<AssociativeNode>.Default.Equals(Array, otherNode.Array);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append(Array.ToString());
            buf.Append("[");
            buf.Append(ArrayDimensions.Expr.ToString());
            buf.Append("]");

            if (ArrayDimensions.Type != null)
                buf.Append(ArrayDimensions.Type.ToString());

            return buf.ToString();
        }
    }

    public class ReplicationGuideNode : AssociativeNode
    {
        public bool IsLongest { get; set; }
        public AssociativeNode RepGuide { get; set; }

        public ReplicationGuideNode() 
        {
            IsLongest = false;
            RepGuide = null;
        }

        public ReplicationGuideNode(ReplicationGuideNode rhs)
            : base(rhs)
        {
            if (null != rhs.RepGuide)
            {
                IsLongest = rhs.IsLongest;
                RepGuide = NodeUtils.Clone(rhs.RepGuide);
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ReplicationGuideNode;
            if (null == otherNode)
                return false;

            return  IsLongest == otherNode.IsLongest
                    && EqualityComparer<AssociativeNode>.Default.Equals(RepGuide, otherNode.RepGuide);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (RepGuide != null)
            {
                buf.Append(RepGuide.ToString());
                if (IsLongest)
                {
                    buf.Append("L");
                }
            }
            return buf.ToString();
        }
    }


    public class ArrayNameNode : AssociativeNode
    {
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

        public ArrayNameNode()
        {
            ArrayDimensions = null;
            ReplicationGuides = new List<AssociativeNode>();
        }


        public ArrayNameNode(ArrayNameNode rhs) : base(rhs)
        {
            ArrayDimensions = null;
            if (null != rhs.ArrayDimensions)
            {
                ArrayDimensions = new ArrayNode(rhs.ArrayDimensions);
            }

            ReplicationGuides = null;
            if (null != rhs.ReplicationGuides)
            {
                ReplicationGuides = new List<AssociativeNode>();
                foreach (AssociativeNode argNode in rhs.ReplicationGuides)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(argNode);
                    ReplicationGuides.Add(tempNode);
                }
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ArrayNameNode;
            if (null == otherNode)
                return false;
            
            return EqualityComparer<ArrayNode>.Default.Equals(ArrayDimensions, otherNode.ArrayDimensions) &&
                   Enumerable.SequenceEqual(ReplicationGuides, otherNode.ReplicationGuides);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (ArrayDimensions != null)
                buf.Append(ArrayDimensions.ToString());

            ReplicationGuides.ForEach(x => buf.Append("<" + x.ToString() + ">"));

            return buf.ToString();
        }
    }

    public class GroupExpressionNode : ArrayNameNode
    {
        public AssociativeNode Expression
        {
            get;
            set;
        }

        public GroupExpressionNode()
        {
        }

        public GroupExpressionNode(GroupExpressionNode rhs)
            : base(rhs)
        {
        }

        public override bool Equals(object other)
        {
            var otherNode = other as GroupExpressionNode;

            if (otherNode != null)
                return EqualityComparer<AssociativeNode>.Default.Equals(Expression, otherNode.Expression);
            else if (Expression != null)
                return Expression.Equals(otherNode.Expression);
            else
                return false;
        }

        public override string ToString()
        {
            if (null == Expression)
                return DSDefinitions.Keyword.Null;
            else 
                return "(" + Expression.ToString() + ")" + base.ToString();
        }
    }

    public class IdentifierNode : ArrayNameNode
    {
        public IdentifierNode(string identName = null)
        {
            datatype = new ProtoCore.Type
            {
                UID = (int)PrimitiveType.kInvalidType,
                rank = 0,
                IsIndexable = false,
                Name = null
            };
            Value = Name = identName;
        }

        public IdentifierNode(IdentifierNode rhs) : base(rhs)
        {
            datatype = new ProtoCore.Type
            {
                UID = rhs.datatype.UID,
                rank = rhs.datatype.rank,
                IsIndexable = rhs.datatype.IsIndexable,
                Name = rhs.datatype.Name
            };

            Value = rhs.Value;
        }

        public ProtoCore.Type datatype
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public override bool Equals(object other)
        {
            IdentifierNode otherNode = other as IdentifierNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<string>.Default.Equals(Value, otherNode.Value) && 
                   datatype.Equals(otherNode.datatype) && 
                   base.Equals(otherNode);
        }

        public override string ToString()
        {
            return Value.Replace("%", string.Empty) + base.ToString();
        }
    }

    public class TypedIdentifierNode : IdentifierNode
    {
        public override string ToString()
        {
            return base.ToString() + " : " + datatype.ToString();
        }
    }

    public class IdentifierListNode : AssociativeNode
    {
        public bool isLastSSAIdentListFactor { get; set; }

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

        public IdentifierListNode()
        {
            isLastSSAIdentListFactor = false;
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = NodeUtils.Clone(rhs.RightNode);
            isLastSSAIdentListFactor = rhs.isLastSSAIdentListFactor;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IdentifierListNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<AssociativeNode>.Default.Equals(LeftNode, otherNode.LeftNode) && 
                   EqualityComparer<AssociativeNode>.Default.Equals(RightNode, otherNode.RightNode) && 
                   Optr.Equals(otherNode.Optr);
        }

        public override string ToString()
        {
            return LeftNode.ToString() + "." + RightNode.ToString();
        }
    }

    public class IntNode : AssociativeNode
    {
        public string value { get; set; }

        public IntNode(string val = null)
        {
            value = val;
        }
        public IntNode(IntNode rhs) : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IntNode;
            if (null == otherNode || string.IsNullOrEmpty(value))
                return false;

            long thisValue;
            if (Int64.TryParse(value, out thisValue))
            {
                long otherValue;
                if (Int64.TryParse(otherNode.value, out otherValue))
                {
                    return thisValue == otherValue;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return value;
        }
    }

    public class DoubleNode : AssociativeNode
    {
        public string value { get; set; }
        public DoubleNode(string val = null)
        {
            value = val;
        }
        public DoubleNode(DoubleNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as DoubleNode;
            if (null == otherNode || string.IsNullOrEmpty(value))
                return false;

            double thisValue;
            if (double.TryParse(value, out thisValue))
            {
                double otherValue;
                if (double.TryParse(otherNode.value, out otherValue))
                {
                    return thisValue == otherValue;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return value;
        }
    }

    public class BooleanNode : AssociativeNode
    {
        public string value { get; set; }
        public BooleanNode()
        {
            value = string.Empty;
        }
        public BooleanNode(BooleanNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as BooleanNode;
            if (null == otherNode || string.IsNullOrEmpty(value))
                return false;

            return EqualityComparer<string>.Default.Equals(value, otherNode.value);
        }

        public override string ToString()
        {
            return value;
        }
    }

    public class CharNode : AssociativeNode
    {
        public string value { get; set; }
        public CharNode()
        {
            value = string.Empty;
        }
        public CharNode(CharNode rhs)
        {
            value = rhs.value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as CharNode;
            if (null == otherNode || string.IsNullOrEmpty(value))
                return false;

            return EqualityComparer<string>.Default.Equals(value, otherNode.value);
        }

        public override string ToString()
        {
            return "'" + value + "'";
        }
    }

    public class StringNode : AssociativeNode
    {
        public string value { get; set; }
        public StringNode()
        {
            value = string.Empty;
        }
        public StringNode(StringNode rhs)
            : base(rhs)
        {
            value = rhs.value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as StringNode;
            if (null == otherNode || null == value)
                return false;

            return value.Equals(otherNode.value);
        }

        public override string ToString()
        {
            return "\"" + value + "\"";
        }
    }

    public class NullNode : AssociativeNode
    {
        public override bool Equals(object other)
        {
            return other is NullNode;
        }

        public override string ToString()
        {
            return ProtoCore.DSDefinitions.Keyword.Null;
        }
    }

    public class ReturnNode : AssociativeNode
    {
        public AssociativeNode ReturnExpr
        {
            get;
            set;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ReturnNode;
            if (null == otherNode)
                return false;

            if (null == ReturnExpr)
                return false;

            return ReturnExpr.Equals(otherNode.ReturnExpr);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append(DSDefinitions.Keyword.Return);
            buf.Append(" = ");
            buf.Append(null == ReturnExpr ? DSDefinitions.Keyword.Null : ReturnExpr.ToString());
            buf.Append(Constants.termline);

            return buf.ToString();
        }
    }

    public class FunctionCallNode : ArrayNameNode
    {
        public int DynamicTableIndex { get; set; }
        public AssociativeNode Function { get; set; }
        public List<AssociativeNode> FormalArguments { get; set; }

        public FunctionCallNode()
        {
            FormalArguments = new List<AssociativeNode>();
            DynamicTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public FunctionCallNode(FunctionCallNode rhs)
            : base(rhs)
        {
            Function = NodeUtils.Clone(rhs.Function);
            FormalArguments = new List<AssociativeNode>();
            foreach (AssociativeNode argNode in rhs.FormalArguments)
            {
                AssociativeNode tempNode = NodeUtils.Clone(argNode);
                FormalArguments.Add(tempNode);
            }

            DynamicTableIndex = rhs.DynamicTableIndex;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as FunctionCallNode;
            if (null == otherNode)
                return false;

            return DynamicTableIndex == otherNode.DynamicTableIndex &&
                   EqualityComparer<AssociativeNode>.Default.Equals(Function, otherNode.Function) &&
                   Enumerable.SequenceEqual(FormalArguments, otherNode.FormalArguments) &&
                   base.Equals(otherNode);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            string functionName = (Function as IdentifierNode).Value;
            string postfix = base.ToString();

            if (CoreUtils.IsInternalMethod(functionName))
            {
                if (!string.IsNullOrEmpty(postfix))
                    buf.Append("(");

                string nameWithoutPrefix = functionName.Substring(DSASM.Constants.kInternalNamePrefix.Length);
                Operator op;
                UnaryOperator uop;

                if (Enum.TryParse<Operator>(nameWithoutPrefix, out op))
                {
                    buf.Append(FormalArguments[0].ToString());
                    buf.Append(" " + Op.GetOpSymbol(op) + " ");
                    buf.Append(FormalArguments[1].ToString());
                }
                else if (Enum.TryParse<UnaryOperator>(nameWithoutPrefix, out uop))
                {
                    buf.Append(Op.GetUnaryOpSymbol(uop));
                    buf.Append(FormalArguments[0].ToString());
                }
                else
                {
                    return ProtoCore.DSDefinitions.Keyword.Null;
                }

                if (!string.IsNullOrEmpty(postfix))
                    buf.Append(")");
            }
            else
            {
                buf.Append(functionName);
                buf.Append("(");

                if (FormalArguments != null)
                {
                    for (int n = 0; n < FormalArguments.Count; ++n)
                    {
                        buf.Append(FormalArguments[n]);
                        if (n < FormalArguments.Count - 1)
                        {
                            buf.Append(", ");
                        }
                    }
                }
                buf.Append(")");
            }

            buf.Append(postfix);

            return buf.ToString();
        }
    }

    public class FunctionDotCallNode : AssociativeNode
    {
        public IdentifierNode staticLHSIdent { get; set; }
        public FunctionCallNode DotCall { get; set; }
        public FunctionCallNode FunctionCall { get; set; }
        public FunctionCallNode NameMangledCall { get; set; }
        public bool isLastSSAIdentListFactor { get; set; }
        public string lhsName { get; set; }

        public FunctionDotCallNode(FunctionCallNode callNode)
        {
            DotCall = new FunctionCallNode();
            FunctionCall = callNode;
            lhsName = string.Empty;
            isLastSSAIdentListFactor = false;
            staticLHSIdent = null;
        }

        public FunctionDotCallNode(string lhsName, FunctionCallNode callNode)
        {
            this.lhsName = lhsName;
            FunctionCall = callNode;
            isLastSSAIdentListFactor = false;
            staticLHSIdent = null;
        }

        public FunctionDotCallNode(FunctionDotCallNode rhs): base(rhs)
        {
            DotCall = new FunctionCallNode(rhs.DotCall);
            FunctionCall = new FunctionCallNode(rhs.FunctionCall);
            lhsName = rhs.lhsName;
            isLastSSAIdentListFactor = rhs.isLastSSAIdentListFactor;
            staticLHSIdent = rhs.staticLHSIdent;
        }

        public IdentifierListNode GetIdentList()
        {
            IdentifierListNode inode = new IdentifierListNode();
            inode.LeftNode = DotCall.FormalArguments[0];
            inode.Optr = DSASM.Operator.dot;
            inode.RightNode = FunctionCall.Function;
            return inode;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as FunctionDotCallNode;
            if (null == otherNode)
                return false;

            return lhsName.Equals(otherNode.lhsName) &&
                   DotCall.Equals(otherNode.DotCall) &&
                   FunctionCall.Equals(otherNode.FunctionCall);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(DotCall.FormalArguments[0].ToString());
            buf.Append(".");
            buf.Append(FunctionCall.ToString());
            return buf.ToString();
        }
    }

    public class VarDeclNode : AssociativeNode
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;
            Attributes = new List<AssociativeNode>();
        }

        public VarDeclNode(VarDeclNode rhs)
            : base(rhs)
        {
            Attributes = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Attributes)
            {
                AssociativeNode newNode = NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
            memregion = rhs.memregion;
            ArgumentType = new ProtoCore.Type
            {
                UID = rhs.ArgumentType.UID,
                rank = rhs.ArgumentType.rank,
                IsIndexable = rhs.ArgumentType.IsIndexable,
                Name = rhs.ArgumentType.Name
            };
            NameNode = NodeUtils.Clone(rhs.NameNode);
            access = rhs.access;
            IsStatic = rhs.IsStatic;
        }

        public List<AssociativeNode> Attributes { get; set; }
        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public AssociativeNode NameNode { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
        public bool IsStatic { get; set; }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (IsStatic)
                buf.Append(DSDefinitions.Keyword.Static + " ");

            if (NameNode is TypedIdentifierNode)
            {
                buf.AppendLine(NameNode.ToString());
            }
            else if (NameNode is IdentifierNode)
            {
                buf.Append(NameNode.ToString());
                string argType = ArgumentType.ToString();
                if (!string.IsNullOrEmpty(argType))
                    buf.Append(" : " + argType);
            }
            else
                buf.Append(NameNode.ToString());

            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as VarDeclNode;
            if (null == otherNode)
                return false;

            return memregion == otherNode.memregion  &&
                   ArgumentType.Equals(otherNode.ArgumentType) &&
                   EqualityComparer<AssociativeNode>.Default.Equals(NameNode, otherNode.NameNode) && 
                   IsStatic == otherNode.IsStatic && 
                   Enumerable.SequenceEqual(Attributes, otherNode.Attributes);
        }
    }

    public class ArgumentSignatureNode : AssociativeNode
    {
        public ArgumentSignatureNode()
        {
            Arguments = new List<VarDeclNode>();
        }

        public ArgumentSignatureNode(ArgumentSignatureNode rhs)
            : base(rhs)
        {
            Arguments = new List<VarDeclNode>();

            foreach (VarDeclNode aNode in rhs.Arguments)
            {
                VarDeclNode newNode = new VarDeclNode(aNode);
                Arguments.Add(newNode);
            }

            IsVarArg = rhs.IsVarArg;
        }

        public bool IsVarArg { get; set; }

        public List<VarDeclNode> Arguments { get; set; }

        public void AddArgument(VarDeclNode arg)
        {
            Arguments.Add(arg);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < Arguments.Count; ++i)
            {
                buf.Append(Arguments[i].ToString());
                if (i < Arguments.Count - 1)
                    buf.Append(", ");
            }
            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ArgumentSignatureNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(Arguments, otherNode.Arguments);
        }
    }

    public class CodeBlockNode : AssociativeNode
    {
        public ProtoCore.DSASM.SymbolTable symbols { get; set; }
        public ProtoCore.DSASM.ProcedureTable procTable { get; set; }
        public List<AssociativeNode> Body { get; set; }

        public CodeBlockNode()
        {
            Body = new List<AssociativeNode>();
            symbols = new ProtoCore.DSASM.SymbolTable("AST generated", ProtoCore.DSASM.Constants.kInvalidIndex);
            procTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kInvalidIndex);
        }

        public CodeBlockNode(CodeBlockNode rhs) : base(rhs)
        {
            Body = new List<AssociativeNode>();
            foreach (AssociativeNode aNode in rhs.Body)
            {
                AssociativeNode newNode = NodeUtils.Clone(aNode);
                Body.Add(newNode);
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as CodeBlockNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(Body, otherNode.Body);
        }
    }

    public class ClassDeclNode : AssociativeNode
    {
        public ClassDeclNode()
        {
            varlist = new List<AssociativeNode>();
            funclist = new List<AssociativeNode>();
            superClass = new List<string>();
            IsImportedClass = false;
        }

        public ClassDeclNode(ClassDeclNode rhs)
        {
            IsImportedClass = rhs.IsImportedClass;
            className = rhs.className;

            Attributes = new List<AssociativeNode>();
            if (null != rhs.Attributes)
            {
                foreach (AssociativeNode attr in rhs.Attributes)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(attr);
                    Attributes.Add(tempNode);
                }
            }

            superClass = new List<string>();
            if (null != rhs.superClass)
            {
                superClass.AddRange(rhs.superClass);
            }

            varlist = new List<AssociativeNode>();
            if (null != rhs.varlist)
            {
                foreach (AssociativeNode varnode in rhs.varlist)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(varnode);
                    varlist.Add(tempNode);
                }
            }

            funclist = new List<AssociativeNode>();
            if (null != rhs.funclist)
            {
                foreach (AssociativeNode funcNode in rhs.funclist)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(funcNode);
                    funclist.Add(tempNode);
                }
            }

            IsExternLib = rhs.IsExternLib ;
            ExternLibName = rhs.ExternLibName;

        }

        public bool IsImportedClass { get; set; }
        public string className { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public List<string> superClass { get; set; }
        public List<AssociativeNode> varlist { get; set; }
        public List<AssociativeNode> funclist { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(DSDefinitions.Keyword.Class + " " + className);
            if (null != superClass)
            {
                if (superClass.Count > 0)
                    buf.Append(" " + DSDefinitions.Keyword.Extend + " ");

                for (int i = 0; i < superClass.Count; ++i)
                {
                    buf.Append(superClass[i]);
                    if (i < superClass.Count - 1)
                        buf.Append(", ");
                }
            }
            buf.AppendLine();

            buf.AppendLine("{");

            foreach (var item in varlist)
            {
                VarDeclNode member = item as VarDeclNode;
                if (member != null)
                {
                    if (member.NameNode is BinaryExpressionNode)
                        buf.Append(member.ToString());
                    else
                        buf.Append(member.ToString() + Constants.termline);
                }
            }

            foreach (var item in funclist)
            {
                if (!item.Name.StartsWith("%"))
                    buf.AppendLine(item.ToString());
            }

            buf.AppendLine("}");

            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ClassDeclNode;
            if (null == otherNode)
                return false;

            //not comparing isImportedClass, isExternLib, ExternLibName
            return (className != null && className.Equals(otherNode.className)) &&
                   Enumerable.SequenceEqual(Attributes, otherNode.Attributes) &&
                   Enumerable.SequenceEqual(superClass, otherNode.superClass) &&
                   Enumerable.SequenceEqual(varlist, otherNode.varlist) &&
                   Enumerable.SequenceEqual(funclist, otherNode.funclist);
        }
    }

    public class ConstructorDefinitionNode : AssociativeNode
    {
        public int localVars { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public AssociativeNode Pattern { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public FunctionCallNode baseConstr { get; set; }
        public ProtoCore.DSASM.AccessSpecifier access { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }

        public ConstructorDefinitionNode()
        {
        }

        public ConstructorDefinitionNode(ConstructorDefinitionNode rhs)
            : base(rhs)
        {
            this.localVars = rhs.localVars;

            Attributes = new List<AssociativeNode>();
            if (null != rhs.Attributes)
            {
                foreach (AssociativeNode elemNode in rhs.Attributes)
                {
                    AssociativeNode tempNode = NodeUtils.Clone(elemNode);
                    Attributes.Add(tempNode);
                }
            }

            if (null != rhs.Signature)
            {
                this.Signature = NodeUtils.Clone(rhs.Signature) as ArgumentSignatureNode;
            }
            if (null != rhs.Pattern)
            {
                Pattern = NodeUtils.Clone(rhs.Pattern);
            }
            this.ReturnType = rhs.ReturnType;
            if (null != rhs.FunctionBody)
            {
                this.FunctionBody = NodeUtils.Clone(rhs.FunctionBody) as CodeBlockNode;
            }
            if (null != rhs.baseConstr)
            {
                this.baseConstr = NodeUtils.Clone(rhs.baseConstr) as FunctionCallNode;
            }
            this.access = rhs.access;
            this.IsExternLib = rhs.IsExternLib;
            this.ExternLibName = rhs.ExternLibName;
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append(DSDefinitions.Keyword.Constructor + " ");
            buf.Append(Name);

            buf.Append("(");
            if (Signature != null)
                buf.Append(Signature.ToString());
            buf.Append(")");

            if (baseConstr != null)
                buf.Append(" : " + baseConstr.ToString());

            if (FunctionBody != null)
            {
                buf.AppendLine("\n{");
                FunctionBody.Body.ForEach(stmt => buf.Append(stmt.ToString()));
                buf.AppendLine("}");
            }

            if (baseConstr == null && FunctionBody == null)
                buf.Append(Constants.termline);

            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ConstructorDefinitionNode;
            if (null == otherNode)
                return false;

            return localVars == otherNode.localVars &&
                   EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) &&
                   ReturnType.Equals(otherNode.ReturnType) &&
                   EqualityComparer<CodeBlockNode>.Default.Equals(FunctionBody, otherNode.FunctionBody) &&
                   Enumerable.SequenceEqual(Attributes, otherNode.Attributes); 
        }
    }

    public class FunctionDefinitionNode : AssociativeNode
    {
        public CodeBlockNode FunctionBody { get; set; }

        public ProtoCore.Type                                   ReturnType      { get; set; }
        public List<AssociativeNode>                            Attributes      { get; set; }
        public ArgumentSignatureNode                            Signature      { get; set; }
        public AssociativeNode                                  Pattern         { get; set; }
        public bool                                             IsExternLib     { get; set; }
        public bool                                             IsBuiltIn       { get; set; }
        public ProtoCore.Lang.BuiltInMethods.MethodID    BuiltInMethodId { get; set; }
        public bool                                             IsDNI           { get; set; }
        public string                                           ExternLibName   { get; set; }
        public ProtoCore.DSASM.AccessSpecifier                  access          { get; set; }
        public bool                                             IsStatic        { get; set; }
        public bool                                             IsAutoGenerated { get; set; }
        public bool IsAssocOperator { get; set; }
        public bool IsAutoGeneratedThisProc { get; set; }
        public ProtoFFI.FFIMethodAttributes MethodAttributes { get; set; } 

        public FunctionDefinitionNode()
        {
            BuiltInMethodId = ProtoCore.Lang.BuiltInMethods.MethodID.kInvalidMethodID;
            IsAutoGenerated = false;
            IsAutoGeneratedThisProc = false;

            Type t = new Type();
            t.Initialize();
            ReturnType = t;

            IsBuiltIn = false;
            Signature = new ArgumentSignatureNode();

            Attributes = new List<AssociativeNode>();
        }

        public FunctionDefinitionNode(FunctionDefinitionNode rhs)
        {
            this.Name = rhs.Name;
            if (null != rhs.FunctionBody)
            {
                this.FunctionBody = new CodeBlockNode(rhs.FunctionBody);
            }
            else
            {
                this.FunctionBody = new CodeBlockNode();
            }

            this.ReturnType = rhs.ReturnType;

            this.Attributes = rhs.Attributes;
            this.Signature = new ArgumentSignatureNode(rhs.Signature);
            this.Pattern = rhs.Pattern;
            this.IsExternLib = rhs.IsExternLib;
            this.BuiltInMethodId = rhs.BuiltInMethodId;
            this.IsDNI = rhs.IsDNI;
            this.ExternLibName = rhs.ExternLibName;
            this.access = rhs.access;
            this.IsStatic = rhs.IsStatic;
            this.IsAutoGenerated = rhs.IsAutoGenerated;
            this.IsAssocOperator = rhs.IsAssocOperator;
            this.IsAutoGeneratedThisProc = IsAutoGeneratedThisProc;
            this.IsBuiltIn = rhs.IsBuiltIn;
        }

        //only compare return type, attributes and signature
        public override bool Equals(object other)
        {
            var otherNode = other as FunctionDefinitionNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) &&
                   ReturnType.Equals(otherNode.ReturnType) &&
                   Enumerable.SequenceEqual(Attributes, otherNode.Attributes);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (IsStatic)
                buf.Append(DSDefinitions.Keyword.Static + " ");

            buf.Append(DSDefinitions.Keyword.Def + " ");
            buf.Append(Name);

            if (ReturnType.UID != Constants.kInvalidIndex)
                buf.Append(": " + ReturnType.ToString());

            buf.Append("(");
            if (Signature != null)
                buf.Append(Signature.ToString());
            buf.Append(")");

            if (FunctionBody != null)
            {
                buf.AppendLine("\n{");
                FunctionBody.Body.ForEach(stmt => buf.Append(stmt.ToString()));
                buf.AppendLine("}");
            }
            else
                buf.Append(Constants.termline);

            return buf.ToString();
        }
    }

    public class IfStatementNode : AssociativeNode
    {
        public AssociativeNode ifExprNode { get; set; }
        public List<AssociativeNode> IfBody { get; set; }
        public List<AssociativeNode> ElseBody { get; set; }

        public IfStatementNode()
        {
            IfBody = new List<AssociativeNode>();
            ElseBody = new List<AssociativeNode>();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IfStatementNode;
            if (null == otherNode)
                return false;

            return ifExprNode.Equals(otherNode.ifExprNode) &&
                   Enumerable.SequenceEqual(IfBody, otherNode.IfBody) &&
                   Enumerable.SequenceEqual(ElseBody, otherNode.ElseBody);
        }
    }

    public class InlineConditionalNode : AssociativeNode
    {
        public AssociativeNode ConditionExpression { get; set; }
        public AssociativeNode TrueExpression { get; set; }
        public AssociativeNode FalseExpression { get; set; }
        public bool IsAutoGenerated { get; set; }

        public InlineConditionalNode()
        {
            IsAutoGenerated = false;
        }

        public InlineConditionalNode(InlineConditionalNode rhs) : base(rhs)
        {
            IsAutoGenerated = false;
            ConditionExpression = NodeUtils.Clone(rhs.ConditionExpression);
            TrueExpression = NodeUtils.Clone(rhs.TrueExpression);
            FalseExpression = NodeUtils.Clone(rhs.FalseExpression);
        }

        public override bool Equals(object other)
        {
            if (null == ConditionExpression || null == TrueExpression || null == FalseExpression)
                return false;

            var otherNode = other as InlineConditionalNode;
            if (null == otherNode)
                return false;

            return ConditionExpression.Equals(otherNode.ConditionExpression) &&
                   TrueExpression.Equals(otherNode.TrueExpression) &&
                   FalseExpression.Equals(otherNode.FalseExpression);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append("(");
            buf.Append(ConditionExpression == null ? DSDefinitions.Keyword.Null : ConditionExpression.ToString());
            buf.Append(" ? ");
            buf.Append(TrueExpression == null ? DSDefinitions.Keyword.Null : TrueExpression.ToString());
            buf.Append(" : ");
            buf.Append(FalseExpression == null ? DSDefinitions.Keyword.Null : FalseExpression.ToString());
            buf.Append(")");

            return buf.ToString();
        }
    }

    public class BinaryExpressionNode : AssociativeNode
    {
        public int exprUID { get; set; }
        public int modBlkUID { get; set; }
        public bool isSSAAssignment { get; set; }
        public bool isSSAPointerAssignment { get; set; }
        public bool isSSAFirstAssignment { get; set; }
        public bool isMultipleAssign { get; set; }
        public AssociativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public AssociativeNode RightNode { get; set; }

        // These properties are used only for the GraphUI ProtoAST
        public uint Guid { get; set; }
        //private uint splitFromUID = 0;
        //public uint SplitFromUID { get { return splitFromUID; } set { splitFromUID = value; } }

        public BinaryExpressionNode(AssociativeNode left = null, AssociativeNode right = null, ProtoCore.DSASM.Operator optr = DSASM.Operator.none)
        {
            isSSAAssignment = false;
            isSSAPointerAssignment = false;
            isSSAFirstAssignment = false;
            isMultipleAssign = false;
            exprUID = ProtoCore.DSASM.Constants.kInvalidIndex;
            modBlkUID = ProtoCore.DSASM.Constants.kInvalidIndex;
            LeftNode = left;
            Optr = optr;
            RightNode = right;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            isSSAAssignment = rhs.isSSAAssignment;
            isSSAPointerAssignment = rhs.isSSAPointerAssignment;
            isSSAFirstAssignment = rhs.isSSAFirstAssignment;
            isMultipleAssign = rhs.isMultipleAssign;
            exprUID = rhs.exprUID;
            modBlkUID = rhs.modBlkUID;

            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = null;
            if (null != rhs.RightNode)
            {
                RightNode = NodeUtils.Clone(rhs.RightNode);
            }
        }

        public override bool Equals(object other)
        {
            if (null == LeftNode || null == RightNode)
                return false;
            
            var otherNode = other as BinaryExpressionNode;
            if (null == otherNode)
                return false;

            return LeftNode.Equals(otherNode.LeftNode) &&
                   Optr.Equals(otherNode.Optr) &&
                   RightNode.Equals(otherNode.RightNode);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            bool needBracket = LeftNode is BinaryExpressionNode || LeftNode is InlineConditionalNode || LeftNode is RangeExprNode;
            if (needBracket)
                buf.Append("(");
            buf.Append(LeftNode.ToString());
            if (needBracket)
                buf.Append(")");

            buf.Append(" " + CoreUtils.GetOperatorString(Optr) + " ");

            needBracket = RightNode is BinaryExpressionNode || RightNode is InlineConditionalNode || RightNode is RangeExprNode;
            if (needBracket)
                buf.Append("(");
            buf.Append(RightNode.ToString());
            if (needBracket)
                buf.Append(")");

            if (DSASM.Operator.assign == Optr)
                buf.Append(DSASM.Constants.termline);

            return buf.ToString();
        }
    }

    public class UnaryExpressionNode : AssociativeNode
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public AssociativeNode Expression { get; set; }

        public override bool Equals(object other)
        {
            if (null == Expression)
                return false;

            var otherNode = other as UnaryExpressionNode;
            if (null == otherNode)
                return false;

            return Operator.Equals(otherNode.Operator) &&
                   Expression.Equals(otherNode.Expression);
        }
    }


    public class ModifierStackNode : AssociativeNode
    {
        public ModifierStackNode()
        {
            ElementNodes = new List<AssociativeNode>();
        }

        public ModifierStackNode(ModifierStackNode rhs)
            : base(rhs)
        {
            ElementNodes = new List<AssociativeNode>();
            foreach (AssociativeNode elemNode in rhs.ElementNodes)
            {
                AssociativeNode tempNode = NodeUtils.Clone(elemNode);
                ElementNodes.Add(tempNode);
            }

            ReturnNode = null;
            if (null != rhs.ReturnNode)
            {
                ReturnNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ReturnNode);
            }
        }

        public IdentifierNode CreateIdentifierNode(Token token, AssociativeNode leftNode)
        {
            if (null == token || (string.IsNullOrEmpty(token.val)))
                return null;

            IdentifierNode leftIdentifier = leftNode as IdentifierNode;
            if (null == leftIdentifier)
                return null;

            IdentifierNode identNode = new IdentifierNode
            {
                Value = token.val,
                Name = token.val,
                datatype = leftIdentifier.datatype
            };

            ProtoCore.Utils.NodeUtils.SetNodeLocation(identNode, token);
            return identNode;
        }

        public IdentifierNode CreateIdentifierNode(AssociativeNode leftNode, ProtoCore.Core core)
        {
            IdentifierNode leftIdentifier = leftNode as IdentifierNode;
            if (null == leftIdentifier)
                return null;

            string modifierName = leftIdentifier.Name;
            string stackName = core.GetModifierBlockTemp(modifierName);
            IdentifierNode identNode = new IdentifierNode
            {
                Value = stackName,
                Name = stackName,
                datatype = leftIdentifier.datatype
            };

            return identNode;
        }

        public BinaryExpressionNode AddElementNode(AssociativeNode n, IdentifierNode identNode)
        {
            BinaryExpressionNode o = n as BinaryExpressionNode;
            BinaryExpressionNode elementNode = new BinaryExpressionNode();

            elementNode.LeftNode = identNode;
            elementNode.RightNode = o.RightNode;
            elementNode.Optr = ProtoCore.DSASM.Operator.assign;

            if (ProtoCore.DSASM.Constants.kInvalidIndex == identNode.line)
            {
                // If the identifier was created as a temp, then we don't have the 
                // corresponding source code location. In that case we'll just use 
                // the entire "RightNode" as the location indicator.
                NodeUtils.CopyNodeLocation(elementNode, elementNode.RightNode);
            }
            else
            {
                // If we do have the name explicitly specified, then we have just 
                // the right location we're after. Only catch here is, for the case 
                // of "foo() => a2", the "RightNode" would have been "foo()" and 
                // the "LeftNode" would have been "a2". So in order to set the 
                // right start and end column, we need these two swapped.
                NodeUtils.SetNodeLocation(elementNode, elementNode.RightNode, elementNode.LeftNode);
            }

            ElementNodes.Add(elementNode);
            return elementNode;

            // TODO: For TP1 we are temporarily updating the modifier block variable 
            // only for its final state instead of updating it for each of its states
            // (which is what we eventually wish to do). This is in order to make MB's behave
            // properly when assigned to class properties - pratapa
            /*BinaryExpressionNode e2 = new BinaryExpressionNode();
            e2.LeftNode = o.LeftNode as IdentifierNode;
            e2.RightNode = e1.LeftNode;
            e2.Optr = ProtoCore.DSASM.Operator.assign;
            ElementNodes.Add(e2);*/
        }

        public List<AssociativeNode> ElementNodes { get; private set; }
        public AssociativeNode ReturnNode { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ModifierStackNode;
            if (null == otherNode)
                return false;

            return ReturnNode.Equals(otherNode.ReturnNode) &&
                   Enumerable.SequenceEqual(ElementNodes, otherNode.ElementNodes);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append("{");
            if (ElementNodes != null) 
                ElementNodes.ForEach(e => buf.AppendLine(e.ToString() + ";"));
            buf.Append("}");

            return buf.ToString();
        }
    }

    public class RangeExprNode : ArrayNameNode
    {
        public AssociativeNode FromNode { get; set; }
        public AssociativeNode ToNode { get; set; }
        public AssociativeNode StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            FromNode = ProtoCore.Utils.NodeUtils.Clone(rhs.FromNode);
            ToNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ToNode);

            // A step can be optional
            if (null != rhs.StepNode)
            {
                StepNode = ProtoCore.Utils.NodeUtils.Clone(rhs.StepNode);
            }
            stepoperator = rhs.stepoperator;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as RangeExprNode;
            if (null == otherNode)
                return false;

            return FromNode.Equals(otherNode.FromNode) &&
                   ToNode.Equals(otherNode.ToNode) &&
                   stepoperator.Equals(otherNode.stepoperator) &&
                   ((StepNode == otherNode.StepNode) || (StepNode != null && StepNode.Equals(otherNode.StepNode)));
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            string postfix = base.ToString();
            if (!string.IsNullOrEmpty(postfix))
                buf.Append("(");

            buf.Append(FromNode.ToString());
            buf.Append("..");
            buf.Append(ToNode.ToString());

            if (StepNode != null)
            {
                buf.Append("..");
                if (DSASM.RangeStepOperator.approxsize == stepoperator)
                {
                    buf.Append("~");
                }
                else if (DSASM.RangeStepOperator.num == stepoperator)
                {
                    buf.Append("#");
                }
                buf.Append(StepNode.ToString());
            }

            if (!string.IsNullOrEmpty(postfix))
                buf.Append(")");

            buf.Append(postfix);

            return buf.ToString();
        }
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            list = new List<AssociativeNode>();
        }

        public ExprListNode(ExprListNode rhs) : base(rhs)
        {
            list = new List<AssociativeNode>();
            foreach (AssociativeNode argNode in rhs.list)
            {
                AssociativeNode tempNode = NodeUtils.Clone(argNode);
                list.Add(tempNode);
            }
        }

        public List<AssociativeNode> list { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ExprListNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(list, otherNode.list);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append("{");
            if (list != null)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    buf.Append(list[i].ToString());
                    if (i < list.Count - 1)
                        buf.Append(", ");
                }
            }
            buf.Append("}");
            buf.Append(base.ToString());

            return buf.ToString();
        }
    }

    public class ForLoopNode : AssociativeNode
    {
        public AssociativeNode loopVar { get; set; }
        public AssociativeNode expression { get; set; }
        public List<AssociativeNode> body { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ForLoopNode;
            if (null == otherNode)
                return false;

            return loopVar.Equals(otherNode.loopVar) &&
                   expression.Equals(otherNode.expression) &&
                   Enumerable.SequenceEqual(body, otherNode.body);
        }
    }

    public class ArrayNode : AssociativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }

        public ArrayNode(AssociativeNode expr, AssociativeNode type)
        {
            Expr = expr;
            Type = type;
        }

        public ArrayNode(ArrayNode rhs) : base(rhs)
        {
            Expr = null;
            Type = null;
            if (null != rhs)
            {
                if (null != rhs.Expr)
                {
                    Expr = NodeUtils.Clone(rhs.Expr);
                }

                if (null != rhs.Type)
                {
                    Type = NodeUtils.Clone(rhs.Type);
                }
            }
        }

        public AssociativeNode Expr { get; set; }
        public AssociativeNode Type { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ArrayNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<AssociativeNode>.Default.Equals(Expr, otherNode.Expr) &&
                   EqualityComparer<AssociativeNode>.Default.Equals(Type, otherNode.Type);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (null != Expr)
            {
                buf.Append("[");
                buf.Append(Expr.ToString());
                buf.Append("]");
            }

            if (null != Type)
                buf.Append(Type.ToString());

            return buf.ToString();
        }
    }

    public class ImportNode : AssociativeNode
    {
        public ImportNode()
        {
            HasBeenImported = false;
            Identifiers = new HashSet<string>();
        }

        public ImportNode(ImportNode rhs)
        {
            CodeNode = new CodeBlockNode(rhs.CodeNode);
            HasBeenImported = rhs.HasBeenImported;
            Identifiers = new HashSet<string>(rhs.Identifiers);
            ModuleName = rhs.ModuleName;
            modulePathFileName = rhs.ModulePathFileName;
        }


        public CodeBlockNode CodeNode { get; set; }
        public bool HasBeenImported { get; set; }
        public HashSet<string> Identifiers { get; set; }
        public string ModuleName { get; set; }

        private string modulePathFileName;
        public string ModulePathFileName
        {
            get
            {
                return modulePathFileName;
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                    modulePathFileName = ProtoCore.Utils.FileUtils.GetFullPathName(value.Replace("\"", String.Empty));
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ImportNode;
            if (null == otherNode)
                return false;

            return CodeNode.Equals(otherNode.CodeNode) && 
                   Identifiers.Equals(otherNode.Identifiers) &&
                   ModuleName.Equals(otherNode.ModuleName) &&
                   modulePathFileName.Equals(otherNode.modulePathFileName);
        }

        public override string ToString()
        {
            return ProtoCore.DSDefinitions.Keyword.Import + "(\"" + ModuleName + "\")" + ProtoCore.DSASM.Constants.termline;
        }
    }

    public class PostFixNode : AssociativeNode
    {
        public AssociativeNode Identifier { get; set; }
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as PostFixNode;
            if (null == otherNode)
                return false;

            return Operator.Equals(otherNode.Operator) &&
                   Identifier.Equals(otherNode.Identifier);
        }
    }

    public class BreakNode : AssociativeNode
    {
        public override string ToString()
        {
            return DSDefinitions.Keyword.Break;
        }

        public override bool Equals(object other)
        {
            return other is BreakNode;
        }
    }

    public class ContinueNode : AssociativeNode
    {
        public override string ToString()
        {
            return DSDefinitions.Keyword.Continue;
        }

        public override bool Equals(object other)
        {
            return other is ContinueNode;
        }
    }

    public class DefaultArgNode : AssociativeNode
    {// not supposed to be used in parser 
    }

    public class DynamicNode : AssociativeNode
    {
        public DynamicNode()
        {
        }

        public DynamicNode(DynamicNode rhs) : base(rhs)
        {
        }
    }

    public class DynamicBlockNode : AssociativeNode
    {
        public int block { get; set; }
        public DynamicBlockNode(int blockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            block = blockId;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as DynamicBlockNode;
            if (null == otherNode)
                return false;

            return block == otherNode.block;
        }
    }

    public class DotFunctionBodyNode : AssociativeNode
    {
        public AssociativeNode leftNode { get; set; }
        public AssociativeNode rightNode { get; set; }
        public AssociativeNode rightNodeDimExprList { get; set; }
        public AssociativeNode rightNodeDim { get; set; }
        public AssociativeNode rightNodeArgList { get; set; }
        public AssociativeNode rightNodeArgNum { get; set; }
        public DotFunctionBodyNode(AssociativeNode lhs, AssociativeNode rhs, AssociativeNode dimExprList, AssociativeNode dim, AssociativeNode rhsArgList = null, AssociativeNode rhsArgNum = null)
        {
            leftNode = lhs;
            rightNode = rhs;
            rightNodeDimExprList = dimExprList;
            rightNodeDim = dim;
            rightNodeArgList = rhsArgList;
            rightNodeArgNum = rhsArgNum;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as DotFunctionBodyNode;
            if (null == otherNode)
                return false;

            return leftNode.Equals(otherNode.leftNode) &&
                   rightNode.Equals(otherNode.rightNode) &&
                   rightNodeDimExprList.Equals(otherNode.rightNodeDimExprList) &&
                   rightNodeDim.Equals(otherNode.rightNodeDim) &&
                   rightNodeArgList.Equals(otherNode.rightNodeArgList) &&
                   rightNodeArgNum.Equals(otherNode.rightNodeArgNum); 
        }

        public override string ToString()
        {
            return DSDefinitions.Keyword.Null;
        }
    }

    public class ThisPointerNode : AssociativeNode
    {
        public ThisPointerNode()
        {
        }

        public ThisPointerNode(ThisPointerNode rhs) : base (rhs)
        {
        }

        public override string ToString()
        {
            return DSDefinitions.Keyword.This;
        }

        public override bool Equals(object other)
        {
            return other is ThisPointerNode;
        }
    }

    public class ThrowNode : AssociativeNode
    {
        public AssociativeNode expression { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ThrowNode;
            if (null == otherNode)
                return false;
               
            return expression.Equals(otherNode.expression);
        }
    }

    public class TryBlockNode : AssociativeNode
    {
        public List<AssociativeNode> body { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as TryBlockNode;
            if (null == otherNode)
                return false;

            return Enumerable.SequenceEqual(body, otherNode.body);
        }
    }

    public class CatchFilterNode : AssociativeNode
    {
        public IdentifierNode var { get; set; }
        public ProtoCore.Type type { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as CatchFilterNode;
            if (null == otherNode)
                return false;

            return var.Equals(otherNode.var) &&
                   type.Equals(otherNode.type);
        }
    }

    public class CatchBlockNode : AssociativeNode
    {
        public CatchFilterNode catchFilter { get; set; }
        public List<AssociativeNode> body { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as CatchBlockNode;
            if (null == otherNode)
                return false;

            return catchFilter.Equals(otherNode.catchFilter) && Enumerable.SequenceEqual(body, otherNode.body);
        }
    }

    public class ExceptionHandlingNode : AssociativeNode
    {
        public TryBlockNode tryBlock { get; set; }
        public List<CatchBlockNode> catchBlocks { get; set; }

        public ExceptionHandlingNode()
        {
            catchBlocks = new List<CatchBlockNode>();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ExceptionHandlingNode;
            if (null == otherNode)
                return false;

            return tryBlock.Equals(otherNode.tryBlock) && Enumerable.SequenceEqual(catchBlocks, otherNode.catchBlocks);
        }
    }

    public class AstFactory
    {
        public static NullNode BuildNullNode()
        {
            return new NullNode();
        }

        public static IntNode BuildIntNode(int value)
        {
            return new IntNode(value.ToString());
        }

        public static DoubleNode BuildDoubleNode(double value)
        {
            return new DoubleNode(value.ToString());
        }

        public static StringNode BuildStringNode(string str)
        {
            return new StringNode { value = str };
        }

        public static BooleanNode BuildBooleanNode(bool value)
        {
            string strValue = value ? Literal.True : Literal.False;
            return new BooleanNode { value = strValue };
        }

        public static InlineConditionalNode BuildConditionalNode(AssociativeNode condition,
                                                                 AssociativeNode trueExpr,
                                                                 AssociativeNode falseExpr)
        {
            InlineConditionalNode cond = new InlineConditionalNode();
            cond.ConditionExpression = condition;
            cond.TrueExpression = trueExpr;
            cond.FalseExpression = falseExpr;
            return cond;
        }

        public static AssociativeNode BuildFunctionCall(string className,
                                                        string functionName,
                                                        List<AssociativeNode> arguments, 
                                                        Core core = null)
        {
            return new IdentifierListNode
            {
                LeftNode = new IdentifierNode(className),
                RightNode = AstFactory.BuildFunctionCall(functionName, arguments)
            };
        }

        public static AssociativeNode BuildFunctionCall(string functionName,
                                                        List<AssociativeNode> arguments,
                                                        Core core = null)
        {
            FunctionCallNode funcCall = new FunctionCallNode();
            funcCall.Function = BuildIdentifier(functionName);
            funcCall.FormalArguments = arguments;
            return funcCall;
        }

        public static IdentifierNode BuildIdentifier(string name)
        {
            return new IdentifierNode(name);
        }

        public static ExprListNode BuildExprList(List<AssociativeNode> nodes)
        {
            ExprListNode exprList = new ExprListNode();
            exprList.list = nodes;
            return exprList;
        }

        public static ExprListNode BuildExprList(List<string> exprs)
        {
            List<AssociativeNode> nodes = new List<AssociativeNode>();
            foreach (var item in exprs)
            {
                nodes.Add(BuildIdentifier(item));
            }
            return BuildExprList(nodes);
        }

        public static BinaryExpressionNode BuildBinaryExpression(AssociativeNode lhs,
                                                                 AssociativeNode rhs,
                                                                 Operator op)
        {
            return new BinaryExpressionNode(lhs, rhs, op);
        }

        public static BinaryExpressionNode BuildAssignment(AssociativeNode lhs,
                                                           AssociativeNode rhs)
        {
            return new BinaryExpressionNode(lhs, rhs, Operator.assign);
        }

        public static VarDeclNode BuildParamNode(string paramName)
        {
            VarDeclNode param = new VarDeclNode();
            param.NameNode = BuildIdentifier(paramName);
            param.ArgumentType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar, false, 0);
            return param;
        }

        public static BinaryExpressionNode BuildReturnStatement(AssociativeNode rhs)
        {
            var retNode = AstFactory.BuildIdentifier(DSDefinitions.Keyword.Return);
            return AstFactory.BuildAssignment(retNode, rhs);
        }
    }
}

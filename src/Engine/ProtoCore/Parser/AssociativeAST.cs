using ProtoCore.AST.ImperativeAST;
using ProtoCore.DSASM;
using ProtoCore.DSDefinitions;
using ProtoCore.Lang;
using ProtoCore.SyntaxAnalysis.Associative;
using ProtoCore.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProtoCore.AST.AssociativeAST
{
    public enum AstKind
    {
        Array,
        ArrayName,
        ArgumentSignature,
        AtLevel,
        BinaryExpression,
        Boolean,
        Char,
        ClassDeclaration,
        CodeBlock,
        Comment,
        Constructor,
        DefaultArgument,
        Double,
        Dynamic,
        DynamicBlock,
        ExpressionList,
        FunctionCall,
        FunctionDefintion,
        FunctionDotCall,
        GroupExpression,
        IdentifierList,
        Identifier,
        If,
        Import,
        InlineConditional,
        Integer,
        LanguageBlock,
        Null,
        RangeExpression,
        ReplicationGuide,
        String,
        ThisPointer,
        TypedIdentifier,
        UnaryExpression,
        VariableDeclaration
    }

    public abstract class AssociativeNode : Node
    {
        public bool IsModifier;

        public bool IsLiteral = false;

        // The immediate scope of this AST is within a function 
        public bool IsProcedureOwned = false;

        protected AssociativeNode() { }

        protected AssociativeNode(AssociativeNode rhs) : base(rhs)
        {
            IsModifier = rhs.IsModifier;
            IsProcedureOwned = rhs.IsProcedureOwned;
        }

        public abstract AstKind Kind { get; }
        public abstract TResult Accept<TResult>(IAstVisitor<TResult> visitor);
    }

    public class CommentNode : AssociativeNode
    {
        public enum CommentType
        {
            Inline,
            Block
        }

        public CommentType Type
        {
            get; private set;
        }
        
        public string Value
        {
            get; private set;
        }

        public CommentNode(int col, int line, string value, CommentType type)
        {
            this.col = col;
            this.line = line;
            Type = type;

            Value = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                if (Type == CommentType.Inline)
                {
                    int start = value.IndexOf("//");
                    if (start >= 0)
                    {
                        Value = value.Substring(start + 2);
                    }
                }
                else
                {
                    int start = value.IndexOf("/*");
                    int end = value.IndexOf("*/");
                    if (start >= 0 && end >= 0)
                    {
                        Value = value.Substring(start + 2, end - start - 2);
                    }
                }
            }
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Comment;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitCommentNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class LanguageBlockNode : AssociativeNode
    {
        public List<AssociativeNode> Attributes { get; set; }
        public Node CodeBlockNode { get; set; }
        public LanguageCodeBlock codeblock { get; set; }

        public LanguageBlockNode()
        {
            codeblock = new LanguageCodeBlock();
            Attributes = new List<AssociativeNode>();
        }

        public LanguageBlockNode(LanguageBlockNode rhs) : base(rhs)
        {
            CodeBlockNode = NodeUtils.Clone(rhs.CodeBlockNode);
            codeblock = new LanguageCodeBlock(rhs.codeblock);
            Attributes = rhs.Attributes.Select(NodeUtils.Clone).ToList();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as LanguageBlockNode;
            if (otherNode == null)
            {
                return false;
            }

            // Compare language block properties
            bool eqLangBlockProperties = codeblock.Equals(otherNode.codeblock);

            // Compare language block contents
            bool eqLangblockContents = CodeBlockNode is AssociativeNode && otherNode.CodeBlockNode is AssociativeNode
                                    && (CodeBlockNode as AssociativeNode).Equals(otherNode.CodeBlockNode as AssociativeNode)
                                    ||
                                    CodeBlockNode is ImperativeNode && otherNode.CodeBlockNode is ImperativeNode
                                    && (CodeBlockNode as ImperativeNode).Equals(otherNode.CodeBlockNode as ImperativeNode)
                                    ;

            bool eqAttribute = null != otherNode && Attributes.SequenceEqual(otherNode.Attributes);

            return eqLangBlockProperties && eqLangblockContents && eqAttribute;
        }

        public override int GetHashCode()
        {
            var AttributesHashCode =
                (Attributes == null ? base.GetHashCode() : Attributes.GetHashCode());

            return AttributesHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            string strLang = CoreUtils.GetLanguageString(codeblock.Language);

            buf.Append("[");
            buf.Append(strLang);
            buf.Append("]");

            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");

            buf.Append(CodeBlockNode);

            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.LanguageBlock;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitLanguageBlockNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Attributes.Concat(this.CodeBlockNode);
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

        public override int GetHashCode()
        {
            var IsLongestHashCode = IsLongest? 1 : 0;
            var RepGuideHashCode =
                (RepGuide == null ? base.GetHashCode() : RepGuide.GetHashCode());

            return IsLongestHashCode ^ RepGuideHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            if (RepGuide != null)
            {
                buf.Append(RepGuide);
                if (IsLongest)
                {
                    buf.Append("L");
                }
            }
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.ReplicationGuide;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitReplicationGuideNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.RepGuide.AsEnumerable();
        }
    }

    public class AtLevelNode : AssociativeNode
    {
        public Int64 Level
        {
            get; set;
        }

        public bool IsDominant
        {
            get; set;
        }

        public AtLevelNode()
        {
            Level = 0;
            IsDominant = false;
        }

        public AtLevelNode(AtLevelNode rhs)
            : base(rhs)
        {
            if (rhs != null)
            {
                Level = rhs.Level;
                IsDominant = rhs.IsDominant;
            }
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.AtLevel;
            }
        }
        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitAtLevelNode(this);
        }
        
        public override bool Equals(object other)
        {
            var otherNode = other as AtLevelNode;
            if (null == otherNode)
                return false;

            return Level == otherNode.Level && IsDominant == otherNode.IsDominant;
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode() ^ IsDominant.GetHashCode();
        }

        public override string ToString()
        {
            var buf = new StringBuilder();
            if (IsDominant)
                buf.Append("@@L");
            else
                buf.Append("@L");
            buf.Append(Math.Abs(Level));
            return buf.ToString();
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
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

        public AtLevelNode AtLevel
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
                ReplicationGuides = rhs.ReplicationGuides.Select(NodeUtils.Clone).ToList();
            }

            AtLevel = null;
            if (rhs.AtLevel != null)
            {
                AtLevel = NodeUtils.Clone(rhs.AtLevel) as AtLevelNode;
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ArrayNameNode;
            if (null == otherNode)
                return false;

            bool arrayDimEqual = (null == ArrayDimensions && null == otherNode.ArrayDimensions);
            if (null != ArrayDimensions && null != otherNode.ArrayDimensions)
            {
                arrayDimEqual = EqualityComparer<ArrayNode>.Default.Equals(ArrayDimensions, otherNode.ArrayDimensions);
            }

            bool atLevelEquals = (AtLevel == null && otherNode.AtLevel == null);
            if (AtLevel != null && otherNode.AtLevel != null)
            {
                atLevelEquals = AtLevel.Equals(otherNode.AtLevel);
            }

            bool repGuidesEqual = (null == ReplicationGuides && null == otherNode.ReplicationGuides);
            if (null != ReplicationGuides && null != otherNode.ReplicationGuides)
            {
                repGuidesEqual = ReplicationGuides.SequenceEqual(otherNode.ReplicationGuides);
            }

            return arrayDimEqual && atLevelEquals && repGuidesEqual;
        }

        public override int GetHashCode()
        {
            var ArrayDimensionsHashCode =
                (ArrayDimensions == null ? base.GetHashCode() : ArrayDimensions.GetHashCode());
            var ReplicationGuidesHashCode =
                (ReplicationGuides == null ? base.GetHashCode() : ReplicationGuides.GetHashCode());

            return ArrayDimensionsHashCode ^ ReplicationGuidesHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            if (ArrayDimensions != null)
                buf.Append(ArrayDimensions.ToString());

            if (AtLevel != null)
                buf.Append(AtLevel.ToString());

            ReplicationGuides.ForEach(x => buf.Append("<" + x.ToString() + ">"));

            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.ArrayName;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitArrayNameNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.ArrayDimensions.AsEnumerable()
                .Concat(this.ReplicationGuides)
                .Concat(this.AtLevel);
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

            return otherNode != null
                && EqualityComparer<AssociativeNode>.Default.Equals(Expression, otherNode.Expression);
        }

        public override int GetHashCode()
        {
            var ExpressionHashCode =
                (Expression == null ? base.GetHashCode() : Expression.GetHashCode());

            return ExpressionHashCode;
        }

        public override string ToString()
        {
            if (Expression == null)
                return Keyword.Null;
            return "(" + Expression + ")" + base.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.GroupExpression;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitGroupExpressionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Expression.AsEnumerable();
        }
    }

    public class IdentifierNode : ArrayNameNode
    {
        public IdentifierNode(string identName = null)
        {
            datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.InvalidType, 0);
            Value = Name = identName;
        }

        public IdentifierNode(IdentifierNode rhs) : base(rhs)
        {
            datatype = new Type
            {
                UID = rhs.datatype.UID,
                rank = rhs.datatype.rank,
                Name = rhs.datatype.Name
            };

            Value = rhs.Value;
            IsLocal = false;
        }

        public Type datatype
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public bool IsLocal
        {
            get;
            set;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IdentifierNode;
            if (null == otherNode)
                return false;

            return IsLocal == otherNode.IsLocal &&
                   EqualityComparer<string>.Default.Equals(Value, otherNode.Value) && 
                   datatype.Equals(otherNode.datatype) && 
                   base.Equals(otherNode);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value.Replace("%", string.Empty) + base.ToString();
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.Identifier;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitIdentifierNode(this);
        }
    }

    public class TypedIdentifierNode : IdentifierNode
    {
        public string TypeAlias { get; set; }

        public TypedIdentifierNode()
        {
        }

        public TypedIdentifierNode(IdentifierNode rhs)
            : base(rhs) {}

        public override string ToString()
        {
            return base.ToString() + " : " + datatype;
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.TypedIdentifier;
            }
        }
        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitTypedIdentifierNode(this);
        }
    }

    public class IdentifierListNode : ArrayNameNode
    {
        public bool IsLastSSAIdentListFactor { get; set; }

        public AssociativeNode LeftNode
        {
            get;
            set;
        }

        public Operator Optr
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
            IsLastSSAIdentListFactor = false;
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = NodeUtils.Clone(rhs.RightNode);
            IsLastSSAIdentListFactor = rhs.IsLastSSAIdentListFactor;
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

        public override int GetHashCode()
        {
            var LeftNodeHashCode =
                (LeftNode == null ? base.GetHashCode() : LeftNode.GetHashCode());
            var RightNodeHashCode =
                (RightNode == null ? base.GetHashCode() : RightNode.GetHashCode());
            var OptrHashCode = Optr.GetHashCode();

            return LeftNodeHashCode ^ RightNodeHashCode ^ OptrHashCode;
        }

        public override string ToString()
        {
            return LeftNode + "." + RightNode;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.IdentifierList;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitIdentifierListNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.LeftNode.AsEnumerable()
                .Concat(this.RightNode)
                .Concat(base.Children());
        }
    }

    public class IntNode : AssociativeNode
    {
        public Int64 Value { get; set; }

        public IntNode(Int64 value)
        {
            this.IsLiteral = true;
            Value = value;
        }

        public IntNode(IntNode rhs) : base(rhs)
        {
            this.IsLiteral = true;
            Value = rhs.Value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IntNode;
            return null != otherNode && Value.Equals(otherNode.Value);
        }

        public override int GetHashCode()
        {
            var valueHashCode = Convert.ToInt32(Value);
            return valueHashCode;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Integer;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitIntNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class DoubleNode : AssociativeNode
    {
        public double Value { get; set; }

        public DoubleNode(double value)
        {
            this.IsLiteral = true;
            Value = value;
        }

        public DoubleNode(DoubleNode rhs)
            : base(rhs)
        {
            this.IsLiteral = true;
            Value = rhs.Value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as DoubleNode;
            if (null == otherNode)
                return false;

            return Value.Equals(otherNode.Value);
        }

        public override int GetHashCode()
        {
            var valueHashCode = Convert.ToInt32(Value);
            return valueHashCode;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Double;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitDoubleNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class BooleanNode : AssociativeNode
    {
        public bool Value { get; set; }

        public BooleanNode(bool value)
        {
            this.IsLiteral = true;
            Value = value;
        }

        public BooleanNode(BooleanNode rhs)
            : base(rhs)
        {
            this.IsLiteral = true;
            Value = rhs.Value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as BooleanNode;
            if (null == otherNode)
                return false;

            return Value == otherNode.Value;
        }

        public override int GetHashCode()
        {
            var valueHashCode =
                (Value ? 10357 : 10463);

            return valueHashCode;
        }

        public override string ToString()
        {
            // Do not use "ToString" here since it converts things to Camel case
            return (Value ? "true" : "false");

        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.Boolean;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitBooleanNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class CharNode : AssociativeNode
    {
        public string Value { get; set; }
        public CharNode()
        {
            this.IsLiteral = true;
            Value = string.Empty;
        }
        public CharNode(CharNode rhs)
        {
            this.IsLiteral = true;
            Value = rhs.Value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as CharNode;
            if (null == otherNode || string.IsNullOrEmpty(Value))
                return false;

            return EqualityComparer<string>.Default.Equals(Value, otherNode.Value);
        }

        public override int GetHashCode()
        {
            var valueHashCode =
                (Value == null ? base.GetHashCode() : Value.GetHashCode());

            return valueHashCode;
        }

        public override string ToString()
        {
            return "'" + Value + "'";
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Char;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitCharNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class StringNode : AssociativeNode
    {
        public string Value { get; set; }
        public StringNode()
        {
            this.IsLiteral = true;
            Value = string.Empty;
        }
        public StringNode(StringNode rhs)
            : base(rhs)
        {
            this.IsLiteral = true;
            Value = rhs.Value;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as StringNode;
            if (null == otherNode || null == Value)
                return false;

            return Value.Equals(otherNode.Value);
        }

        public override int GetHashCode()
        {
            var valueHashCode =
                (Value == null ? base.GetHashCode() : Value.GetHashCode());

            return valueHashCode;
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.String;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitStringNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class NullNode : AssociativeNode
    {
        public NullNode()
        {
            this.IsLiteral = true;
        }

        public override bool Equals(object other)
        {
            return other is NullNode;
        }

        public override int GetHashCode()
        {
            return 10099;
        }

        public override string ToString()
        {
            return Keyword.Null;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Null;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitNullNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
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
            DynamicTableIndex = Constants.kInvalidIndex;
        }

        public FunctionCallNode(FunctionCallNode rhs)
            : base(rhs)
        {
            Function = NodeUtils.Clone(rhs.Function);
            FormalArguments = rhs.FormalArguments.Select(NodeUtils.Clone).ToList();
            DynamicTableIndex = rhs.DynamicTableIndex;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as FunctionCallNode;
            if (null == otherNode)
                return false;

            return DynamicTableIndex == otherNode.DynamicTableIndex &&
                   EqualityComparer<AssociativeNode>.Default.Equals(Function, otherNode.Function) &&
                   FormalArguments.SequenceEqual(otherNode.FormalArguments) &&
                   base.Equals(otherNode);
        }

        public override int GetHashCode()
        {
            var dynamicTableIndexHashCode = Convert.ToInt32(DynamicTableIndex);
            var functionHashCode =
                (Function == null ? base.GetHashCode() : Function.GetHashCode());
            var formalArgumentsHashCode =
                (FormalArguments == null ? base.GetHashCode() : FormalArguments.GetHashCode());

            return dynamicTableIndexHashCode ^ functionHashCode ^ formalArgumentsHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();
            string functionName = (Function as IdentifierNode).Value;
            if (string.IsNullOrEmpty(functionName))
                functionName = Function.Name;
            string postfix = base.ToString();

            if (CoreUtils.IsInternalMethod(functionName))
            {
                if (!string.IsNullOrEmpty(postfix))
                    buf.Append("(");

                string nameWithoutPrefix = functionName.Substring(Constants.kInternalNamePrefix.Length);
                Operator op;
                UnaryOperator uop;

                if (Enum.TryParse(nameWithoutPrefix, out op))
                {
                    var arg1 = FormalArguments[0];
                    bool needsParens = !arg1.IsLiteral && !(arg1 is IdentifierNode);
                    if (needsParens)
                        buf.Append("(");

                    buf.Append(arg1);

                    if (needsParens)
                        buf.Append(")");

                    buf.Append(" " + Op.GetOpSymbol(op) + " ");

                    var arg2 = FormalArguments[1];
                    needsParens = !arg2.IsLiteral && !(arg2 is IdentifierNode); ;
                    if (needsParens)
                        buf.Append("(");

                    buf.Append(arg2);

                    if (needsParens)
                        buf.Append(")");
                }
                else if (Enum.TryParse(nameWithoutPrefix, out uop))
                {
                    buf.Append(Op.GetUnaryOpSymbol(uop));
                    buf.Append(FormalArguments[0]);
                }
                else
                {
                    return Keyword.Null;
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

        public override AstKind Kind
        {
            get
            {
                return AstKind.FunctionCall;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitFunctionCallNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Function.AsEnumerable()
                .Concat(this.FormalArguments);
        }
    }

    public class FunctionDotCallNode : AssociativeNode
    {
        public IList<AssociativeNode> Arguments { get; private set; }
        public FunctionCallNode FunctionCall { get; set; }
        public bool isLastSSAIdentListFactor { get; set; }

        public FunctionDotCallNode(FunctionCallNode callNode, List<AssociativeNode> arguments)
        {
            Arguments = arguments;
            FunctionCall = callNode;
            isLastSSAIdentListFactor = false;
        }

        public FunctionDotCallNode(FunctionDotCallNode rhs): base(rhs)
        {
            Arguments = new List<AssociativeNode>(rhs.Arguments);
            FunctionCall = new FunctionCallNode(rhs.FunctionCall);
            isLastSSAIdentListFactor = rhs.isLastSSAIdentListFactor;
        }

        public IdentifierListNode GetIdentList()
        {
            var inode = new IdentifierListNode
            {
                LeftNode = Arguments[0],
                Optr = Operator.dot,
                RightNode = FunctionCall.Function
            };
            return inode;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as FunctionDotCallNode;
            if (null == otherNode)
                return false;

            return Arguments.SequenceEqual(otherNode.Arguments) &&
                   FunctionCall.Equals(otherNode.FunctionCall);
        }

        public override int GetHashCode()
        {
            var functionCallHashCode =
                (FunctionCall == null ? base.GetHashCode() : FunctionCall.GetHashCode());

            return functionCallHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append(Arguments[0]);
            buf.Append(".");
            buf.Append(FunctionCall);
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.FunctionDotCall;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitFunctionDotCallNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.FunctionCall.AsEnumerable()
                .Concat(this.Arguments);
        }
    }

    public class VarDeclNode : AssociativeNode
    {
        public VarDeclNode()
        {
        }

        public VarDeclNode(VarDeclNode rhs)
            : base(rhs)
        {
            ArgumentType = new Type
            {
                UID = rhs.ArgumentType.UID,
                rank = rhs.ArgumentType.rank,
                Name = rhs.ArgumentType.Name
            };
            NameNode = NodeUtils.Clone(rhs.NameNode);
            Access = rhs.Access;
            IsStatic = rhs.IsStatic;
            ExternalAttributes = rhs.ExternalAttributes;
        }

        public Type ArgumentType { get; set; }
        public AssociativeNode NameNode { get; set; }
        public CompilerDefinitions.AccessModifier Access { get; set; }
        public bool IsStatic { get; set; }
        public ExternalAttributes ExternalAttributes { get; set; }

        public override string ToString()
        {
            var buf = new StringBuilder();

            if (IsStatic)
                buf.Append(Keyword.Static + " ");

            if (NameNode is TypedIdentifierNode)
            {
                buf.AppendLine(NameNode.ToString());
            }
            else if (NameNode is IdentifierNode)
            {
                buf.Append(NameNode);
                string argType = ArgumentType.ToString();
                if (!string.IsNullOrEmpty(argType) && !argType.Equals("null"))
                    buf.Append(" : " + argType);
            }
            else
                buf.Append(NameNode);

            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as VarDeclNode;
            if (null == otherNode)
                return false;

            return ArgumentType.Equals(otherNode.ArgumentType) &&
                   EqualityComparer<AssociativeNode>.Default.Equals(NameNode, otherNode.NameNode) &&
                   IsStatic == otherNode.IsStatic;
        }

        public override int GetHashCode()
        {
            var argumentTypeHashCode = ArgumentType.GetHashCode();
            var nameNodeHashCode =
                (NameNode == null ? base.GetHashCode() : NameNode.GetHashCode());
            var isStaticHashCode = IsStatic? 1 : 0;

            return argumentTypeHashCode ^ nameNodeHashCode ^ isStaticHashCode;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.VariableDeclaration;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitVarDeclNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.NameNode.AsEnumerable();
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
            Arguments = rhs.Arguments.Select(aNode => new VarDeclNode(aNode)).ToList();
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
            var buf = new StringBuilder();
            for (int i = 0; i < Arguments.Count; ++i)
            {
                buf.Append(Arguments[i]);
                if (i < Arguments.Count - 1)
                    buf.Append(", ");
            }
            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ArgumentSignatureNode;
            return null != otherNode && Arguments.SequenceEqual(otherNode.Arguments);
        }

        public override int GetHashCode()
        {
            var argumentsHashCode =
                (Arguments == null ? base.GetHashCode() : Arguments.GetHashCode());

            return argumentsHashCode;
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.ArgumentSignature;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitArgumentSignatureNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Arguments;
        }
    }

    public class CodeBlockNode : AssociativeNode
    {
        public SymbolTable Symbols { get; set; }
        public ProcedureTable ProcTable { get; set; }
        public List<AssociativeNode> Body { get; set; }

        public CodeBlockNode()
        {
            Body = new List<AssociativeNode>();
            Symbols = new SymbolTable("AST generated", Constants.kInvalidIndex);
            ProcTable = new ProcedureTable(Constants.kInvalidIndex);
        }

        public CodeBlockNode(CodeBlockNode rhs) : base(rhs)
        {
            Body = rhs.Body.Select(NodeUtils.Clone).ToList();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as CodeBlockNode;
            return null != otherNode && Body.SequenceEqual(otherNode.Body);
        }

        public override int GetHashCode()
        {
            var bodyHashCode =
                (Body == null ? base.GetHashCode() : Body.GetHashCode());

            return bodyHashCode;
        }

        public override string ToString()
        {
            if (Body == null)
            {
                return string.Empty;
            }

            var buf = new StringBuilder();
            for (int i = 0; i < Body.Count; ++i)
            {
                buf.Append(Body[i].ToString());
            }
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.CodeBlock;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitCodeBlockNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Body;
        }
    }

    public class ClassDeclNode : AssociativeNode
    {
        public ClassDeclNode()
        {
            Variables = new List<AssociativeNode>();
            Procedures = new List<AssociativeNode>();
            Interfaces = new List<string>();
            IsImportedClass = false;
        }

        public ClassDeclNode(ClassDeclNode rhs)
        {
            IsImportedClass = rhs.IsImportedClass;
            ClassName = rhs.ClassName;

            Attributes = new List<AssociativeNode>();
            if (null != rhs.Attributes)
                Attributes.AddRange(rhs.Attributes.Select(NodeUtils.Clone));

            BaseClass = rhs.BaseClass;

            Interfaces = new List<string>();
            if (null != rhs.Interfaces)
                Interfaces.AddRange(rhs.Interfaces);

            Variables = new List<AssociativeNode>();
            if (null != rhs.Variables)
                Variables.AddRange(rhs.Variables.Select(NodeUtils.Clone));

            Procedures = new List<AssociativeNode>();
            if (null != rhs.Procedures)
                Procedures.AddRange(rhs.Procedures.Select(NodeUtils.Clone));

            IsExternLib = rhs.IsExternLib ;
            ExternLibName = rhs.ExternLibName;
        }

        public bool IsInterface { get; set; }
        public bool IsStatic { get; set; }
        public bool IsImportedClass { get; set; }
        public string ClassName { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public string BaseClass { get; set; }
        public List<string> Interfaces { get; set; }
        public List<AssociativeNode> Variables { get; set; }
        public List<AssociativeNode> Procedures { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }
        public ClassAttributes ClassAttributes { get; set; }

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append(Keyword.Class + " " + ClassName);
            if (!string.IsNullOrEmpty(BaseClass))
            {
                buf.Append(" " + Keyword.Extend + " ");
                buf.Append(BaseClass);
            }
            buf.AppendLine();

            buf.AppendLine("{");

            foreach (var member in Variables.OfType<VarDeclNode>()) 
            {
                if (member.NameNode is BinaryExpressionNode)
                    buf.Append(member);
                else
                    buf.Append(member + Constants.termline);
            }

            foreach (var item in Procedures.Where(item => !item.Name.StartsWith("%"))) 
            {
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
            return (ClassName != null && ClassName.Equals(otherNode.ClassName)) &&
                   Attributes.SequenceEqual(otherNode.Attributes) &&
                   BaseClass.SequenceEqual(otherNode.BaseClass) &&
                   Variables.SequenceEqual(otherNode.Variables) &&
                   Procedures.SequenceEqual(otherNode.Procedures);
        }

        public override int GetHashCode()
        {
            var classNameHashCode =
                (ClassName == null ? base.GetHashCode() : ClassName.GetHashCode());
            var superClassHashCode =
                (BaseClass == null ? base.GetHashCode() : BaseClass.GetHashCode());
            var varlistHashCode =
                (Variables == null ? base.GetHashCode() : Variables.GetHashCode());
            var attributesHashCode =
                (Attributes == null ? base.GetHashCode() : Attributes.GetHashCode());
            var funclistHashCode =
                (Procedures == null ? base.GetHashCode() : Procedures.GetHashCode());

            return classNameHashCode ^ superClassHashCode ^ 
                varlistHashCode ^ attributesHashCode ^ funclistHashCode;
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.ClassDeclaration;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitClassDeclNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return ( this.Attributes ?? Enumerable.Empty<Node>() )
                .Concat(this.Procedures)
                .Concat(this.Variables);
        }
    }

    public class ClassAttributes 
    {
        public string PreferredShortName { get; protected set; }
        public bool HiddenInLibrary { get; protected set; }
        public string ObsoleteMessage { get; protected set; }
        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }
        public ClassAttributes(string msg = "", string preferredShortName = "")
        {
            ObsoleteMessage = msg;
            HiddenInLibrary = IsObsolete;
            PreferredShortName = preferredShortName;
        }
    }

    public class MethodAttributes
    {
        public bool HiddenInLibrary { get; protected set; }
        public bool CanUpdatePeriodically { get; protected set; }
        public IEnumerable<string> ReturnKeys
        {
            get
            {
                return returnKeys;
            }
        }
        protected List<string> returnKeys;
        public string ObsoleteMessage { get; protected set; }
        public bool IsObsolete { get { return !string.IsNullOrEmpty(ObsoleteMessage); } }
        public bool IsLacingDisabled { get; protected set; }
        public bool AllowArrayPromotion { get; protected set; } = true;

        /// <summary>
        /// Returns/Sets description for the method.
        /// </summary>
        public string Description { get; set; }

        public MethodAttributes(bool hiddenInLibrary = false, bool canUpdatePeriodically = false, string msg = "")
        {
            HiddenInLibrary = hiddenInLibrary;
            CanUpdatePeriodically = canUpdatePeriodically;
            ObsoleteMessage = msg;
        }
    }

    public class ExternalAttributes
    {
        private Dictionary<String, object> attributes;

        public ExternalAttributes()
        {
            attributes = new Dictionary<string, object>();
        }

        public bool TryGetAttribute(string attribute, out object value)
        {
            return attributes.TryGetValue(attribute, out value);
        }

        public void AddAttribute(string attribute, object value)
        {
            attributes[attribute] = value;
        }
    }

    public class ConstructorDefinitionNode : AssociativeNode
    {
        public int LocalVariableCount { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public Type ReturnType { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public FunctionCallNode BaseConstructor { get; set; }
        public CompilerDefinitions.AccessModifier Access { get; set; }
        public bool IsExternLib { get; set; }
        public string ExternLibName { get; set; }
        public MethodAttributes MethodAttributes { get; set; } 

        public ConstructorDefinitionNode()
        {
        }

        public ConstructorDefinitionNode(ConstructorDefinitionNode rhs)
            : base(rhs)
        {
            LocalVariableCount = rhs.LocalVariableCount;

            if (null != rhs.Signature)
                Signature = NodeUtils.Clone(rhs.Signature) as ArgumentSignatureNode;

            ReturnType = rhs.ReturnType;
            if (null != rhs.FunctionBody)
                FunctionBody = NodeUtils.Clone(rhs.FunctionBody) as CodeBlockNode;

            if (null != rhs.BaseConstructor)
                BaseConstructor = NodeUtils.Clone(rhs.BaseConstructor) as FunctionCallNode;

            Access = rhs.Access;
            IsExternLib = rhs.IsExternLib;
            ExternLibName = rhs.ExternLibName;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append(Keyword.Constructor + " ");
            buf.Append(Name);

            buf.Append("(");
            if (Signature != null)
                buf.Append(Signature);
            buf.Append(")");

            if (BaseConstructor != null)
                buf.Append(" : " + BaseConstructor);

            if (FunctionBody != null)
            {
                buf.AppendLine("\n{");
                FunctionBody.Body.ForEach(stmt => buf.Append(stmt.ToString()));
                buf.AppendLine("}");
            }

            if (BaseConstructor == null && FunctionBody == null)
                buf.Append(Constants.termline);

            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            var otherNode = other as ConstructorDefinitionNode;
            if (null == otherNode)
                return false;

            return LocalVariableCount == otherNode.LocalVariableCount &&
                   EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) &&
                   ReturnType.Equals(otherNode.ReturnType) &&
                   EqualityComparer<CodeBlockNode>.Default.Equals(FunctionBody, otherNode.FunctionBody); 
        }

        public override int GetHashCode()
        {
            var localVarsHashCode = Convert.ToInt32(LocalVariableCount);
            var signatureHashCode =
                (Signature == null ? base.GetHashCode() : Signature.GetHashCode());
            var returnTypeHashCode = ReturnType.GetHashCode();
            var functionBodyHashCode =
                (FunctionBody == null ? base.GetHashCode() : FunctionBody.GetHashCode());

            return localVarsHashCode ^ signatureHashCode ^ returnTypeHashCode ^ functionBodyHashCode;
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.Constructor;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitConstructorDefinitionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.FunctionBody.AsEnumerable();
        }
    }

    public class FunctionDefinitionNode : AssociativeNode
    {
        public CodeBlockNode FunctionBody { get; set; }
        public Type ReturnType { get; set; }
        public List<AssociativeNode> Attributes { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public bool IsExternLib { get; set; }
        public bool IsBuiltIn { get; set; }
        public BuiltInMethods.MethodID BuiltInMethodId { get; set; }
        public string ExternLibName { get; set; }
        public CompilerDefinitions.AccessModifier Access { get; set; }
        public bool IsStatic { get; set; }
        public bool IsAutoGenerated { get; set; }
        public bool IsAssocOperator { get; set; }
        public bool IsAutoGeneratedThisProc { get; set; }
        public MethodAttributes MethodAttributes { get; set; } 

        public FunctionDefinitionNode()
        {
            BuiltInMethodId = BuiltInMethods.MethodID.InvalidMethodID;
            IsAutoGenerated = false;
            IsAutoGeneratedThisProc = false;

            var t = new Type();
            t.Initialize();
            ReturnType = t;

            IsBuiltIn = false;
            Signature = new ArgumentSignatureNode();
        }

        public FunctionDefinitionNode(FunctionDefinitionNode rhs)
        {
            Name = rhs.Name;
            FunctionBody = null != rhs.FunctionBody
                ? new CodeBlockNode(rhs.FunctionBody)
                : new CodeBlockNode();

            ReturnType = rhs.ReturnType;

            Signature = new ArgumentSignatureNode(rhs.Signature);
            IsExternLib = rhs.IsExternLib;
            BuiltInMethodId = rhs.BuiltInMethodId;
            ExternLibName = rhs.ExternLibName;
            Access = rhs.Access;
            IsStatic = rhs.IsStatic;
            IsAutoGenerated = rhs.IsAutoGenerated;
            IsAssocOperator = rhs.IsAssocOperator;
            IsAutoGeneratedThisProc = IsAutoGeneratedThisProc;
            IsBuiltIn = rhs.IsBuiltIn;
        }

        //only compare return type, attributes and signature
        public override bool Equals(object other)
        {
            var otherNode = other as FunctionDefinitionNode;
            if (null == otherNode)
            {
                return false;
            }

            bool equalSignature = EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) 
                && ReturnType.Equals(otherNode.ReturnType) 
                && Name.Equals(otherNode.Name);

            bool equalBody = FunctionBody.Equals(otherNode.FunctionBody);

            return equalSignature && equalBody;
        }

        public override int GetHashCode()
        {
            var signatureHashCode =
                (Signature == null ? base.GetHashCode() : Signature.GetHashCode());
            var returnTypeHashCode = ReturnType.GetHashCode();

            return  signatureHashCode ^ returnTypeHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            if (IsStatic)
                buf.Append(Keyword.Static + " ");

            buf.Append(Keyword.Def + " ");
            buf.Append(Name);

            if (ReturnType.UID != Constants.kInvalidIndex)
                buf.Append(": " + ReturnType);

            buf.Append("(");
            if (Signature != null)
                buf.Append(Signature);
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
        public override AstKind Kind
        {
            get
            {
                return AstKind.FunctionDefintion;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitFunctionDefinitionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Attributes.Concat(this.FunctionBody);
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
                   IfBody.SequenceEqual(otherNode.IfBody) &&
                   ElseBody.SequenceEqual(otherNode.ElseBody);
        }

        public override int GetHashCode()
        {
            var ifExprNodeHashCode =
                (ifExprNode == null ? base.GetHashCode() : ifExprNode.GetHashCode());
            var ifBodyHashCode =
                (IfBody == null ? base.GetHashCode() : IfBody.GetHashCode());
            var elseBodyHashCode =
                (ElseBody == null ? base.GetHashCode() : ElseBody.GetHashCode());

            return ifExprNodeHashCode ^ ifBodyHashCode ^ elseBodyHashCode;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.If;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitIfStatementNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.ifExprNode.AsEnumerable()
                .Concat(this.IfBody)
                .Concat(this.ElseBody);
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

        public override int GetHashCode()
        {
            var conditionExpressionHashCode =
                (ConditionExpression == null ? base.GetHashCode() : ConditionExpression.GetHashCode());
            var trueExpressionHashCode =
                (TrueExpression == null ? base.GetHashCode() : TrueExpression.GetHashCode());
            var falseExpressionHashCode =
                (FalseExpression == null ? base.GetHashCode() : FalseExpression.GetHashCode());

            return conditionExpressionHashCode ^ trueExpressionHashCode ^ falseExpressionHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("(");
            buf.Append(ConditionExpression == null ? Keyword.Null : ConditionExpression.ToString());
            buf.Append(" ? ");
            buf.Append(TrueExpression == null ? Keyword.Null : TrueExpression.ToString());
            buf.Append(" : ");
            buf.Append(FalseExpression == null ? Keyword.Null : FalseExpression.ToString());
            buf.Append(")");

            return buf.ToString();
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.InlineConditional;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitInlineConditionalNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.ConditionExpression.AsEnumerable()
                .Concat(this.TrueExpression)
                .Concat(this.FalseExpression);
        }
    }

    public class BinaryExpressionNode : AssociativeNode
    {
        public int SSAExpressionUID { get; set; }
        public int ExpressionUID { get; set; }
        public int SSASubExpressionID { get; set; }
        public Guid guid { get; set; }
        public int OriginalAstID { get; set; }    // The original AST that this Binarynode was derived from
        public bool isSSAAssignment { get; set; }
        public bool isSSAFirstAssignment { get; set; }
        public bool isMultipleAssign { get; set; }
        public AssociativeNode LeftNode { get; set; }
        public Operator Optr { get; set; }
        public AssociativeNode RightNode { get; set; }
        public bool isSSAPointerAssignment { get; set; }
        public bool IsFirstIdentListNode { get; set; }

        public BinaryExpressionNode(AssociativeNode left = null, AssociativeNode right = null, Operator optr = Operator.none)
        {
            isSSAAssignment = false;
            isSSAPointerAssignment = false;
            isSSAFirstAssignment = false;
            isMultipleAssign = false;
            ExpressionUID = Constants.kInvalidIndex;
            OriginalAstID = ID;
            guid = System.Guid.Empty;
            LeftNode = left;
            Optr = optr;
            RightNode = right;
            IsFirstIdentListNode = false;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            isSSAAssignment = rhs.isSSAAssignment;
            isSSAPointerAssignment = rhs.isSSAPointerAssignment;
            isSSAFirstAssignment = rhs.isSSAFirstAssignment;
            isMultipleAssign = rhs.isMultipleAssign;
            ExpressionUID = rhs.ExpressionUID;
            guid = rhs.guid;
            OriginalAstID = rhs.OriginalAstID;

            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = null;
            if (null != rhs.RightNode)
            {
                RightNode = NodeUtils.Clone(rhs.RightNode);
            }
            IsFirstIdentListNode = rhs.IsFirstIdentListNode;
        }

        /// <summary>
         /// Create a Binary assignment node from a given lhs identifier and given right node
         /// with line and col properties of rhs node
         /// </summary>
         /// <param name="lhs"></param>
         /// <param name="rhs"></param>
         public BinaryExpressionNode(IdentifierNode lhs, AssociativeNode rhs)
             : base(rhs)
         {
             isSSAAssignment = false;
             isSSAPointerAssignment = false;
             isSSAFirstAssignment = false;
             isMultipleAssign = false;
             ExpressionUID = Constants.kInvalidIndex;
             OriginalAstID = ID;
             guid = System.Guid.Empty;
            
             Optr = Operator.assign;
             LeftNode = lhs;
             RightNode = NodeUtils.Clone(rhs);
             IsFirstIdentListNode = false;
             
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

        public override int GetHashCode()
        {
            var leftNodeHashCode =
                (LeftNode == null ? base.GetHashCode() : LeftNode.GetHashCode());
            var optrHashCode = Optr.GetHashCode();
            var rightNodeHashCode =
                (RightNode == null ? base.GetHashCode() : RightNode.GetHashCode());

            return leftNodeHashCode ^ optrHashCode ^ rightNodeHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            bool needBracket = LeftNode is BinaryExpressionNode || LeftNode is InlineConditionalNode || LeftNode is RangeExprNode;
            if (needBracket)
                buf.Append("(");
            buf.Append(LeftNode);
            if (needBracket)
                buf.Append(")");

            buf.Append(" " + CoreUtils.GetOperatorString(Optr) + " ");

            needBracket = RightNode is BinaryExpressionNode || RightNode is InlineConditionalNode || RightNode is RangeExprNode;
            if (needBracket)
                buf.Append("(");
            buf.Append(RightNode);
            if (needBracket)
                buf.Append(")");

            if (Operator.assign == Optr)
                buf.Append(Constants.termline);

            return buf.ToString();
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.BinaryExpression;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitBinaryExpressionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.LeftNode.AsEnumerable()
                .Concat(this.RightNode);
        }
    }

    public class UnaryExpressionNode : AssociativeNode
    {
        public UnaryOperator Operator { get; set; }
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

        public override int GetHashCode()
        {
            var operatorHashCode = Operator.GetHashCode();
            var expressionHashCode =
                (Expression == null ? base.GetHashCode() : Expression.GetHashCode());

            return operatorHashCode ^ expressionHashCode;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.UnaryExpression;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitUnaryExpressionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Expression.AsEnumerable();
        }
    }

    public class RangeExprNode : ArrayNameNode
    {
        public AssociativeNode From { get; set; }
        public AssociativeNode To { get; set; }
        public AssociativeNode Step { get; set; }
        public RangeStepOperator StepOperator { get; set; }
        public bool HasRangeAmountOperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            From = NodeUtils.Clone(rhs.From);
            To = NodeUtils.Clone(rhs.To);

            // A step can be optional
            if (null != rhs.Step)
            {
                Step = NodeUtils.Clone(rhs.Step);
            }
            StepOperator = rhs.StepOperator;
            HasRangeAmountOperator = rhs.HasRangeAmountOperator;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as RangeExprNode;
            if (null == otherNode)
                return false;

            return From.Equals(otherNode.From) &&
                   To.Equals(otherNode.To) &&
                   StepOperator.Equals(otherNode.StepOperator) &&
                   ((Step == otherNode.Step) || (Step != null && Step.Equals(otherNode.Step))) &&
                   HasRangeAmountOperator == otherNode.HasRangeAmountOperator
                   && base.Equals(other);
        }

        public override int GetHashCode()
        {
            var fromNodeHashCode =
                (From == null ? base.GetHashCode() : From.GetHashCode());
            var toNodeHashCode =
                (To == null ? base.GetHashCode() : To.GetHashCode());
            //var stepoperatorHashCode = stepoperator.GetHashCode());
            var stepNodeHashCode =
                (Step == null ? base.GetHashCode() : Step.GetHashCode());
            var hasRangeAmountOperatorHashCode = HasRangeAmountOperator ? 1 : 0;

            return fromNodeHashCode ^ toNodeHashCode ^ 
                stepNodeHashCode ^ hasRangeAmountOperatorHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            string postfix = base.ToString();
            if (!string.IsNullOrEmpty(postfix))
                buf.Append("(");

            buf.Append(From);
            buf.Append("..");
            if (HasRangeAmountOperator)
                buf.Append("#");
            buf.Append(To);

            if (Step != null)
            {
                buf.Append("..");
                switch (StepOperator)
                {
                    case RangeStepOperator.ApproximateSize:
                        buf.Append("~");
                        break;
                    case RangeStepOperator.Number:
                        buf.Append("#");
                        break;
                }
                buf.Append(Step);
            }

            if (!string.IsNullOrEmpty(postfix))
                buf.Append(")");

            buf.Append(postfix);

            return buf.ToString();
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.RangeExpression;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitRangeExprNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.From.AsEnumerable()
                .Concat(this.To)
                .Concat(this.Step)
                .Concat(base.Children());
        }
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            Exprs = new List<AssociativeNode>();
        }

        public ExprListNode(ExprListNode rhs) : base(rhs)
        {
            Exprs = rhs.Exprs.Select(NodeUtils.Clone).ToList();
        }

        public List<AssociativeNode> Exprs { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ExprListNode;
            return null != otherNode && Exprs.SequenceEqual(otherNode.Exprs);
        }

        public override int GetHashCode()
        {
            var listHashCode =
                (Exprs == null ? base.GetHashCode() : Exprs.GetHashCode());

            return listHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.Append("[");
            if (Exprs != null)
            {
                for (int i = 0; i < Exprs.Count; ++i)
                {
                    buf.Append(Exprs[i]);
                    if (i < Exprs.Count - 1)
                        buf.Append(", ");
                }
            }
            buf.Append("]");
            buf.Append(base.ToString());

            return buf.ToString();
        }
        public override AstKind Kind
        {
            get
            {
                return AstKind.ExpressionList;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitExprListNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Exprs
                .Concat(base.Children());
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

        public override int GetHashCode()
        {
            var exprHashCode =
                (Expr == null ? base.GetHashCode() : Expr.GetHashCode());
            var typeHashCode =
                (Type == null ? base.GetHashCode() : Type.GetHashCode());

            return exprHashCode ^ typeHashCode;
        }

        public override string ToString()
        {
            var buf = new StringBuilder();

            if (null != Expr)
                buf.Append(string.Format("[{0}]", Expr.ToString()));

            if (null != Type)
                buf.Append(Type);

            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Array;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitArrayNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Expr.AsEnumerable();
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
                    modulePathFileName = FileUtils.GetFullPathName(value.Replace("\"", String.Empty));
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

        public override int GetHashCode()
        {
            var codeNodeHashCode =
                (CodeNode == null ? base.GetHashCode() : CodeNode.GetHashCode());
            var identifiersHashCode =
                (Identifiers == null ? base.GetHashCode() : Identifiers.GetHashCode());
            var moduleNameHashCode =
                (ModuleName == null ? base.GetHashCode() : ModuleName.GetHashCode());
            var modulePathFileNameHashCode =
                (modulePathFileName == null ? base.GetHashCode() : modulePathFileName.GetHashCode());

            return codeNodeHashCode ^ identifiersHashCode ^
                moduleNameHashCode ^ modulePathFileNameHashCode;
        }

        public override string ToString()
        {
            return Keyword.Import + " (\"" + CompilerUtils.ToLiteral(ModuleName) + "\") " + Constants.termline;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Import;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitImportNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.CodeNode.AsEnumerable();
        }
    }

    public class DefaultArgNode : AssociativeNode
    {// not supposed to be used in parser 
        public override AstKind Kind
        {
            get
            {
                return AstKind.DefaultArgument;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitDefaultArgNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class DynamicNode : AssociativeNode
    {
        public DynamicNode()
        {
        }

        public DynamicNode(DynamicNode rhs) : base(rhs)
        {
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Dynamic;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitDynamicNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class DynamicBlockNode : AssociativeNode
    {
        public int block { get; set; }
        public DynamicBlockNode(int blockId = Constants.kInvalidIndex)
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

        public override int GetHashCode()
        {
            var blockHashCode = Convert.ToInt32(block);

            return blockHashCode;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.DynamicBlock;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitDynamicBlockNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
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
            return Keyword.This;
        }

        public override bool Equals(object other)
        {
            return other is ThisPointerNode;
        }

        public override int GetHashCode()
        {
            return 10037;
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.ThisPointer;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitThisPointerNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
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
            return new IntNode(value);
        }

        public static DoubleNode BuildDoubleNode(double value)
        {
            return new DoubleNode(value);
        }

        public static StringNode BuildStringNode(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");

            return new StringNode { Value = str };
        }

        public static BooleanNode BuildBooleanNode(bool value)
        {
            return new BooleanNode(value);
        }

        /// <summary>
        /// Builds a integer, double, string, boolean or null node depending
        /// on input value type.
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>AssociativeNode</returns>
        public static AssociativeNode BuildPrimitiveNodeFromObject(object value)
        {
            if (null == value)
                return BuildNullNode();

            string type = value.GetType().Name;
            switch (type)
            {
                case "Int16":
                case "Int32":
                case "Int64":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return BuildIntNode(Convert.ToInt32(value));
                case "Single":
                case "Double":
                    return BuildDoubleNode(Convert.ToDouble(value));
                case "String":
                    return BuildStringNode(value.ToString());
                case "Boolean":
                    return BuildBooleanNode(Convert.ToBoolean(value));
                default:
                    Validity.Assert(false, "Invalide Input type to make AST node");
                    break;
            }
            return BuildNullNode();
        }

        public static AssociativeNode BuildIndexExpression(AssociativeNode value, AssociativeNode index)
        {
            var node = BuildFunctionCall(Node.BuiltinGetValueAtIndexTypeName, Node.BuiltinValueAtIndexMethodName, 
                new List<AssociativeNode>() { value, index });
            NodeUtils.SetNodeLocation(node, value, index);
            return node;
        }

        public static InlineConditionalNode BuildConditionalNode(
            AssociativeNode condition, AssociativeNode trueExpr, AssociativeNode falseExpr)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            if (trueExpr == null)
                throw new ArgumentNullException("trueExpr");

            if (falseExpr == null)
                throw new ArgumentNullException("falseExpr");

            var cond = new InlineConditionalNode
            {
                ConditionExpression = condition,
                TrueExpression = trueExpr,
                FalseExpression = falseExpr
            };
            return cond;
        }

        //Due to the lack of var arg support for generic types, we need to do
        //the manual type expansions

        #region BuildFunctionCall type safe overloads

        public static AssociativeNode BuildFunctionCall(
            Action a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1>(
            Action<T1> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2>(
            Action<T1, T2> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3>(
            Action<T1, T2, T3> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4>(
                    Action<T1, T2, T3, T4> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5>(
                    Action<T1, T2, T3, T4, T5> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6>(
                    Action<T1, T2, T3, T4, T5, T6> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7>(
                    Action<T1, T2, T3, T4, T5, T6, T7> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8>(
                   Action<T1, T2, T3, T4, T5, T6, T7, T8> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                   Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                   Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                   Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                   Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }



        public static AssociativeNode BuildFunctionCall<TRetType>(
            Func<TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, TRetType>(
            Func<T1, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, TRetType>(
            Func<T1, T2, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, TRetType>(
                    Func<T1, T2, T3, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, TRetType>(
                    Func<T1, T2, T3, T4, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, TRetType>(
                    Func<T1, T2, T3, T4, T5, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, TRetType>(
                    Func<T1, T2, T3, T4, T5, T6, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, TRetType>(
                   Func<T1, T2, T3, T4, T5, T6, T7, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, TRetType>(
                   Func<T1, T2, T3, T4, T5, T6, T7, T8, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRetType>(
                   Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRetType>(
                   Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }
        public static AssociativeNode BuildFunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRetType>(
                   Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRetType> a, List<AssociativeNode> arguments, Core core = null)
        {
            return BuildFunctionCall(
                a.Method.DeclaringType.FullName,
                a.Method.Name,
                arguments, core);
        }

        #endregion

        public static AssociativeNode BuildFunctionCall(
            string className, string functionName, List<AssociativeNode> arguments, Core core = null)
        {
            if (string.IsNullOrEmpty(className))
                throw new ArgumentException("className");

            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentException("functionName");

            return new IdentifierListNode
            {
                LeftNode = new IdentifierNode(className),
                Optr = Operator.dot,
                RightNode = BuildFunctionCall(functionName, arguments)
            };
        }

        public static AssociativeNode BuildFunctionCall(
            string functionName, List<AssociativeNode> arguments, Core core = null)
        {
            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentException("functionName");

            if (arguments == null)
                throw new ArgumentNullException("arguments");

            var funcCall = new FunctionCallNode
            {
                Function = BuildIdentifier(functionName),
                FormalArguments = arguments
            };
            return funcCall;
        }

        public static IdentifierNode BuildIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            return new IdentifierNode(name);
        }

        public static IdentifierNode BuildIdentifier(string name, AssociativeNode arrayIndex)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            if (arrayIndex == null)
                throw new ArgumentNullException("arrayIndex");

            return new IdentifierNode(name)
            {
                ArrayDimensions = new ArrayNode { Expr = arrayIndex }
            };
        }

        public static ExprListNode BuildExprList(List<AssociativeNode> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            return new ExprListNode { Exprs = nodes };
        }

        public static ExprListNode BuildExprList(List<string> exprs)
        {
            if (exprs == null)
                throw new ArgumentNullException("exprs");

            var nodes = exprs.Select(BuildIdentifier).Cast<AssociativeNode>().ToList();
            return BuildExprList(nodes);
        }

        public static IdentifierListNode BuildIdentList(AssociativeNode leftNode, AssociativeNode rightNode)
        {
            var identList = new IdentifierListNode();
            identList.LeftNode = leftNode;
            identList.RightNode = rightNode;
            identList.Optr = Operator.dot;
            return identList;
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
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("paramName");

            return BuildParamNode(
                paramName,
                TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.Var, 0));
        }

        public static VarDeclNode BuildParamNode(string paramName, Type type)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException("paramName");

            return new VarDeclNode
            {
                NameNode = BuildIdentifier(paramName),
                ArgumentType = type
            };
        }

        public static BinaryExpressionNode BuildReturnStatement(AssociativeNode rhs)
        {
            if (rhs == null)
                throw new ArgumentNullException("rhs");

            var retNode = BuildIdentifier(Keyword.Return);
            return BuildAssignment(retNode, rhs);
        }

        public static AssociativeNode BuildFunctionObject(
            string functionName,
            int numParams,
            IEnumerable<int> connectedIndices,
            List<AssociativeNode> inputs)
        {
            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentException("functionname");

            if (connectedIndices == null)
                throw new ArgumentNullException("connectedIndices");

            if (inputs == null)
                throw new ArgumentNullException("inputs");

            return BuildFunctionObject(BuildIdentifier(functionName), numParams, connectedIndices, inputs);
        }

        public static AssociativeNode BuildFunctionObject(
            AssociativeNode functionNode,
            int numParams,
            IEnumerable<int> connectedIndices,
            List<AssociativeNode> inputs)
        {
            if (functionNode == null)
                throw new ArgumentNullException("functionNode");

            if (connectedIndices == null)
                throw new ArgumentNullException("connectedIndices");

            if (inputs == null)
                throw new ArgumentNullException("input");

            var paramNumNode = new IntNode(numParams);
            var positionNode = BuildExprList(connectedIndices.Select(BuildIntNode).Cast<AssociativeNode>().ToList());
            var arguments = BuildExprList(inputs);
            var inputParams = new List<AssociativeNode>
            {
                functionNode,
                paramNumNode,
                positionNode,
                arguments,
                AstFactory.BuildBooleanNode(true)
            };

            return BuildFunctionCall("__CreateFunctionObject", inputParams);
        }

        /// <summary>
        /// Create a copy of the node with replication guide added. 
        /// </summary>
        /// <param name="node">Associative AST node.</param>
        /// <param name="guides">Replication guide.</param>
        /// <param name="isLongest">If use the Longest replication strategy.</param>
        /// <returns></returns>
        public static AssociativeNode AddReplicationGuide(AssociativeNode node,
                                                          List<int> guides,
                                                          bool isLongest)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (guides == null)
                throw new ArgumentNullException("guides");

            ArrayNameNode repNode = null;

            if (node is ArrayNameNode)
            {
                repNode = NodeUtils.Clone(node) as ArrayNameNode;
            }
            else
            {
                return node;
            }

            repNode.ReplicationGuides = guides.Select(g => new ReplicationGuideNode
                {
                    RepGuide = AstFactory.BuildIdentifier(g.ToString()),
                    IsLongest = isLongest
                } as AssociativeNode).ToList();

            return repNode;
        }

        /// <summary>
        /// Create a copy of node with at-level. E.g., xs@-2
        /// </summary>
        /// <param name="node"></param>
        /// <param name="level"></param>
        /// <param name="shouldKeepListStructure"></param>
        /// <returns></returns>
        public static AssociativeNode AddAtLevel(AssociativeNode node, int level, bool shouldKeepListStructure)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ArrayNameNode repNode = null;
            if (node is ArrayNameNode)
            {
                repNode = NodeUtils.Clone(node) as ArrayNameNode;
            }
            else
            {
                return node;
            }

            repNode.AtLevel = new AtLevelNode();
            repNode.AtLevel.IsDominant = shouldKeepListStructure;
            repNode.AtLevel.Level = level;

            return repNode;
        }
    }

    public static class AstExtensions
    {
        private static void CopyProps(Node @from, Node to)
        {
            to.Name = from.Name;
            to.col = from.col;
            to.endCol = from.endCol;
            to.line = from.line;
            to.endLine = from.endLine;
            to.skipMe = from.skipMe;
        }

        public static ImperativeAST.ArrayNameNode ToImperativeNode(this ArrayNameNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.ArrayNameNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.ArrayNode ToImperativeNode(this ArrayNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.ArrayNode
            {
                Type = aNode.Type.ToImperativeAST(),
                Expr = aNode.Expr.ToImperativeAST()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.BinaryExpressionNode ToImperativeNode(this BinaryExpressionNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.BinaryExpressionNode
            {
                LeftNode = aNode.LeftNode.ToImperativeAST(),
                RightNode = aNode.RightNode.ToImperativeAST(),
                Optr = aNode.Optr
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.BooleanNode ToImperativeNode(this BooleanNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.BooleanNode(aNode.Value);
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.BreakNode ToImperativeNode(this BreakNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.BreakNode();
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.CharNode ToImperativeNode(this CharNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.CharNode
            {
                Value = aNode.Value
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.CodeBlockNode ToImperativeNode(this CodeBlockNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.CodeBlockNode
            {
                Body = aNode.Body.Select(ToImperativeAST).ToList()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.ContinueNode ToImperativeNode(this ContinueNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.ContinueNode();
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.DoubleNode ToImperativeNode(this DoubleNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.DoubleNode(aNode.Value);
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.ExprListNode ToImperativeNode(this ExprListNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.ExprListNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode(),
                Exprs = aNode.Exprs.Select(ToImperativeAST).ToList()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.FunctionCallNode ToImperativeNode(this FunctionCallNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.FunctionCallNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode(),
                FormalArguments = aNode.FormalArguments.Select(ToImperativeAST).ToList(),
                Function = aNode.Function.ToImperativeAST()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.GroupExpressionNode ToImperativeNode(this GroupExpressionNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.GroupExpressionNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.IdentifierListNode ToImperativeNode(this IdentifierListNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.IdentifierListNode
            {
                LeftNode = aNode.LeftNode.ToImperativeAST(),
                Optr = aNode.Optr,
                RightNode = aNode.RightNode.ToImperativeAST()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.IdentifierNode ToImperativeNode(this IdentifierNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.IdentifierNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode(),
                Value = aNode.Value,
                DataType = aNode.datatype
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.InlineConditionalNode ToImperativeNode(this InlineConditionalNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.InlineConditionalNode
            {
                ConditionExpression = aNode.ConditionExpression.ToImperativeAST(),
                TrueExpression = aNode.TrueExpression.ToImperativeAST(),
                FalseExpression = aNode.FalseExpression.ToImperativeAST()
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.LanguageBlockNode ToImperativeNode(this LanguageBlockNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.LanguageBlockNode
            {
                Attributes = aNode.Attributes.Select(ToImperativeAST).ToList(),
                CodeBlockNode = aNode.CodeBlockNode,
                codeblock = aNode.codeblock
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.IntNode ToImperativeNode(this IntNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.IntNode(aNode.Value);
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.NullNode ToImperativeNode(this NullNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.NullNode();
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.RangeExprNode ToImperativeNode(this RangeExprNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.RangeExprNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode(),
                From = aNode.From.ToImperativeAST(),
                Step = aNode.Step.ToImperativeAST(),
                To = aNode.To.ToImperativeAST(),
                StepOperator = aNode.StepOperator
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.StringNode ToImperativeNode(this StringNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.StringNode
            {
                Value = aNode.Value
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.TypedIdentifierNode ToImperativeNode(this TypedIdentifierNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.TypedIdentifierNode
            {
                ArrayDimensions = aNode.ArrayDimensions.ToImperativeNode(),
                Value = aNode.Value,
                DataType = aNode.datatype
            };
            CopyProps(aNode, result);
            return result;
        }

        public static ImperativeAST.UnaryExpressionNode ToImperativeNode(this UnaryExpressionNode aNode)
        {
            if (aNode == null) return null;

            var result = new ImperativeAST.UnaryExpressionNode
            {
                Expression = aNode.Expression.ToImperativeAST(),
                Operator = aNode.Operator
            };
            CopyProps(aNode, result);
            return result;
        }

        private static ImperativeNode ToImperativeNode(this AssociativeNode aNode)
        {
            throw new ArgumentException("No Imperative version of " + aNode.GetType().FullName);
        }

        public static ImperativeNode ToImperativeAST(this AssociativeNode aNode)
        {
            return aNode == null ? null : ToImperativeNode(aNode as dynamic);
        }
    }
}

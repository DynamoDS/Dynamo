using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using ProtoCore.SyntaxAnalysis.Imperative;
using ProtoCore.Utils;

namespace ProtoCore.AST
{
    public static class NodeEnumerableExtensions
    {
        public static IEnumerable<Node> AsEnumerable(this Node item)
        {
            return item != null ? new List<Node>() { item } : new List<Node>();
        }

        public static IEnumerable<Node> Concat(this IEnumerable<Node> list, Node item)
        {
            return (list ?? new List<Node>()).Concat(item.AsEnumerable());
        }
    }
}

namespace ProtoCore.AST.ImperativeAST
{
    public enum AstKind
    {
        ArrayName,
        Array,
        BinaryExpression,
        Boolean,
        Break,
        Char,
        CodeBlock,
        Continue,
        Double,
        ElseIf,
        ExpressionList,
        ForLoop,
        FunctionCall,
        GroupExpression,
        Identifier,
        IdentifierList,
        If,
        IfPosition,
        InlineConditional,
        Integer,
        LanguageBlock,
        Null,
        RangeExpression,
        String,
        TypedIdentifier,
        UnaryExpression,
        While
    }

    public abstract class ImperativeNode : Node
    {
        public ImperativeNode()
        {
        }

        public ImperativeNode(ImperativeNode rhs) : base(rhs)
        {
        }

        public abstract AstKind Kind { get; }
        
        public abstract TResult Accept<TResult>(IAstVisitor<TResult> visitor);
    }

    public class LanguageBlockNode : ImperativeNode
    {
        public LanguageBlockNode()
        {
            codeblock = new ProtoCore.LanguageCodeBlock();
            Attributes = new List<ImperativeNode>();
        }

        public LanguageBlockNode(LanguageBlockNode rhs) : base(rhs)
        {
            CodeBlockNode = NodeUtils.Clone(rhs.CodeBlockNode);
            codeblock = new ProtoCore.LanguageCodeBlock(rhs.codeblock);
            Attributes = new List<ImperativeNode>();
            foreach (ImperativeNode aNode in rhs.Attributes)
            {
                ImperativeNode newNode = NodeUtils.Clone(aNode);
                Attributes.Add(newNode);
            }
            CodeBlockNode = NodeUtils.Clone(rhs.CodeBlockNode);
        }

        public List<ImperativeNode> Attributes { get; set; }
        public ProtoCore.LanguageCodeBlock codeblock { get; set; }
        public Node CodeBlockNode { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as LanguageBlockNode;

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

        public override AstKind Kind
        {
            get
            {
                return AstKind.LanguageBlock;
            }
        }

        public override int GetHashCode()
        {
            return Attributes == null ? base.GetHashCode() : Attributes.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            string strLang = CoreUtils.GetLanguageString(codeblock.Language);

            buf.Append("[");
            buf.Append(strLang);
            buf.Append("]");

            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");

            if (CodeBlockNode != null)
            {
                buf.Append(CodeBlockNode.ToString());
            }

            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            return buf.ToString();
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

    public class ArrayNameNode : ImperativeNode
    {
        public ArrayNode ArrayDimensions { get; set; }

        public ArrayNameNode()
        {
            ArrayDimensions = null;
        }

        public ArrayNameNode(ArrayNameNode rhs) : base(rhs)
        {
            ArrayDimensions = null;
            if (null != rhs.ArrayDimensions)
            {
                ArrayDimensions = new ArrayNode(rhs.ArrayDimensions);
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

            return arrayDimEqual;
        }
        public override int GetHashCode()
        {
            return ArrayDimensions == null ? base.GetHashCode() : ArrayDimensions.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (ArrayDimensions != null)
            {
                buf.Append(ArrayDimensions.ToString());
            }

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
            return this.ArrayDimensions.AsEnumerable();
        }
    }

    public class GroupExpressionNode : ArrayNameNode
    {
        public ImperativeNode Expression { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as GroupExpressionNode;

            return otherNode != null
                && EqualityComparer<ImperativeNode>.Default.Equals(Expression, otherNode.Expression);
        }
        public override int GetHashCode()
        {
            var ExpressionHashCode =
                (Expression == null ? base.GetHashCode() : Expression.GetHashCode());

            return ExpressionHashCode;
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
    }

    public class IdentifierNode : ArrayNameNode 
    {
        public IdentifierNode()
        {
            ArrayDimensions = null;
            DataType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.InvalidType, 0);
        }

        public IdentifierNode(string identName = null)
        {
            ArrayDimensions = null;
            DataType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.InvalidType, 0);
            Value = Name = identName;
        }

        public IdentifierNode(IdentifierNode rhs) : base(rhs)
        {
            DataType = new ProtoCore.Type
            {
                UID = rhs.DataType.UID,
                rank = rhs.DataType.rank,
                Name = rhs.DataType.Name
            };

            Value = rhs.Value;
            IsLocal = false;
        }

        public Type DataType { get; set; }
        public string Value { get; set; }
        public string ArrayName { get; set; }
        public bool IsLocal { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as IdentifierNode;
            if (null == otherNode)
                return false;

            return  IsLocal == otherNode.IsLocal &&
                    EqualityComparer<string>.Default.Equals(Value, otherNode.Value) &&
                    DataType.Equals(otherNode.DataType) &&
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

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class TypedIdentifierNode: IdentifierNode
    {
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

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class IntNode : ImperativeNode
    {
        public Int64 Value { get; set; }

        public IntNode(Int64 value)
        {
            Value = value;
        }

        public IntNode(IntNode rhs)
            : base(rhs)
        {
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

    public class DoubleNode : ImperativeNode
    {
        public double Value { get; set; }

        public DoubleNode(double value)
        {
            Value = value;
        }

        public DoubleNode(DoubleNode rhs) : base(rhs)
        {
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

    public class BooleanNode : ImperativeNode
    {
        public bool Value { get; set; }

        public BooleanNode(bool value)
        {
            Value = value;
        }

        public BooleanNode(BooleanNode rhs) : base(rhs)
        {
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
            return Value.ToString();
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

    public class CharNode : ImperativeNode
    {
        public string Value { get; set; }
        public CharNode()
        {
            Value = string.Empty;
        }
        public CharNode(CharNode rhs)
        {
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

    public class StringNode : ImperativeNode
    {
        public string Value { get; set; }
        public StringNode()
        {
            Value = string.Empty;
        }
        public StringNode(StringNode rhs)
            : base(rhs)
        {
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

    public class NullNode : ImperativeNode
    {
        public override string ToString()
        {
            return ProtoCore.DSDefinitions.Keyword.Null;
        }

        public override bool Equals(object other)
        {
            return other is NullNode;
        }

        public override int GetHashCode()
        {
            return 10099;
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

    public class ArrayNode : ImperativeNode
    {
        public ArrayNode()
        {
            Expr = null;
            Type = null;
        }

        public ArrayNode(ArrayNode rhs)
            : base(rhs)
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

        public ImperativeNode Expr { get; set; }
        public ImperativeNode Type { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ArrayNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<ImperativeNode>.Default.Equals(Expr, otherNode.Expr) &&
                   EqualityComparer<ImperativeNode>.Default.Equals(Type, otherNode.Type);
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
            return new List<Node>
            {
                this.Expr, this.Type
            };
        }
    }

    public class FunctionCallNode : ArrayNameNode 
    {
        public ImperativeNode Function
        {
            get;
            set;
        }

        public List<ImperativeNode> FormalArguments
        {
            get;
            set;
        }

        public FunctionCallNode()
        {
            FormalArguments = new List<ImperativeNode>();
        }

        public FunctionCallNode(FunctionCallNode rhs) : base(rhs)
        {
            Function = NodeUtils.Clone(rhs.Function);
            FormalArguments = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.FormalArguments)
            {
                ImperativeNode tempNode = NodeUtils.Clone(argNode);
                FormalArguments.Add(tempNode);
            }
        }

        public override bool Equals(object other)
        {
            var otherNode = other as FunctionCallNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<ImperativeNode>.Default.Equals(Function, otherNode.Function) &&
                   FormalArguments.SequenceEqual(otherNode.FormalArguments) &&
                   base.Equals(otherNode);
        }

        public override int GetHashCode()
        {
            var functionHashCode =
                (Function == null ? base.GetHashCode() : Function.GetHashCode());
            var formalArgumentsHashCode =
                (FormalArguments == null ? base.GetHashCode() : FormalArguments.GetHashCode());

            return functionHashCode ^ formalArgumentsHashCode;
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
            return new List<Node>() { this.Function }.Concat(this.FormalArguments);
        }
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            Exprs = new List<ImperativeNode>();
        }

        public ExprListNode(ExprListNode rhs)
            : base(rhs)
        {
            Exprs = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.Exprs)
            {
                ImperativeNode tempNode = NodeUtils.Clone(argNode);
                Exprs.Add(tempNode);
            }
        }

        public List<ImperativeNode> Exprs { get; set; }

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
            StringBuilder buf = new StringBuilder();

            buf.Append("[");
            if (Exprs != null)
            {
                for (int i = 0; i < Exprs.Count; ++i)
                {
                    buf.Append(Exprs[i].ToString());
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
            return this.Exprs;
        }
    }

    public class CodeBlockNode : ImperativeNode
    {
        public CodeBlockNode()
        {
            Body = new List<ImperativeNode>();
        }

        public CodeBlockNode(CodeBlockNode rhs) : base(rhs)
        {
            Body = new List<ImperativeNode>();
            foreach (ImperativeNode aNode in rhs.Body)
            {
                ImperativeNode newNode = NodeUtils.Clone(aNode);
                Body.Add(newNode);
            }
        }

        public List<ImperativeNode> Body { get; set; }

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
            StringBuilder buf = new StringBuilder();
            foreach (ImperativeNode node in Body)
            {
                buf.Append(node.ToString());
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

    public class InlineConditionalNode : ImperativeNode
    {
        public ImperativeNode ConditionExpression { get; set; }
        public ImperativeNode TrueExpression { get; set; }
        public ImperativeNode FalseExpression { get; set; }

        public InlineConditionalNode()
        {
        }

        public InlineConditionalNode(InlineConditionalNode rhs) : base(rhs)
        {
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
            return new List<Node>() {this.ConditionExpression, this.TrueExpression, this.FalseExpression};
        }
    }

    public class BinaryExpressionNode : ImperativeNode
    {
        public Guid guid { get; set; }
        public ImperativeNode LeftNode { get; set; }
        public Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public BinaryExpressionNode()
        {
        }

        public BinaryExpressionNode(ImperativeNode left = null, ImperativeNode right = null, Operator optr = DSASM.Operator.none)
        {
            LeftNode = left;
            Optr = optr;
            RightNode = right;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = rhs.LeftNode == null ? null : NodeUtils.Clone(rhs.LeftNode);
            RightNode = rhs.RightNode == null ? null : NodeUtils.Clone(rhs.RightNode);
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
            return new List<Node>() { this.LeftNode, this.RightNode };
        }
    }


    public class ElseIfBlock : ImperativeNode
    {
        public ElseIfBlock()
        {
            Body = new List<ImperativeNode>();
            ElseIfBodyPosition = new IfStmtPositionNode();
        }


        public ElseIfBlock(ElseIfBlock rhs) : base(rhs)
        {
            Expr = NodeUtils.Clone(rhs.Expr);
            ElseIfBodyPosition = NodeUtils.Clone(rhs.ElseIfBodyPosition);

            Body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = NodeUtils.Clone(iNode);
                Body.Add(newNode);
            }
        }

        public ImperativeNode Expr { get; set; }
        public List<ImperativeNode> Body { get; set; }
        public ImperativeNode ElseIfBodyPosition { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ElseIfBlock;
            if (null == otherNode)
            {
                return false;
            }

            return Expr.Equals(otherNode.Expr)
                    && otherNode != null && Body.SequenceEqual(otherNode.Body)
                    && ElseIfBodyPosition.Equals(otherNode.ElseIfBodyPosition);
        }

        public override int GetHashCode()
        {
            return Expr.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // elseif statement
            buf.Append(ProtoCore.DSDefinitions.Keyword.Elseif);
            buf.Append("(");
            buf.Append(Expr);
            buf.Append(")");

            // elseif body
            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");
            foreach (ImperativeNode node in Body)
            {
                buf.Append(node.ToString());
            }
            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.ElseIf;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitElseIfNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Expr.AsEnumerable()
                .Concat(this.Body)
                .Concat(this.ElseIfBodyPosition);
        }
    }

    public class IfStmtPositionNode: ImperativeNode
    {
        public IfStmtPositionNode()
        {
        }

        public IfStmtPositionNode(IfStmtPositionNode rhs):base(rhs)
        {
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.IfPosition;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitIfStmtPositionNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class IfStmtNode : ImperativeNode
    {
        public IfStmtNode()
        {
            ElseIfList = new List<ElseIfBlock>();
            IfBody = new List<ImperativeNode>();
            IfBodyPosition = new IfStmtPositionNode();
            ElseBody = new List<ImperativeNode>();
            ElseBodyPosition = new IfStmtPositionNode();
        }


        public IfStmtNode(IfStmtNode rhs) : base(rhs)
        {
            //
            IfExprNode = NodeUtils.Clone(rhs.IfExprNode);


            //
            IfBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.IfBody)
            {
                ImperativeNode body = NodeUtils.Clone(stmt);
                IfBody.Add(body);
            }

            //
            IfBodyPosition = NodeUtils.Clone(rhs.IfBodyPosition);

            //
            ElseIfList = new List<ElseIfBlock>();
            foreach (ElseIfBlock elseBlock in rhs.ElseIfList)
            {
                ImperativeNode elseNode = NodeUtils.Clone(elseBlock);
                ElseIfList.Add(elseNode as ElseIfBlock);
            }

            //
            ElseBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.ElseBody)
            {
                ImperativeNode tmpNode = NodeUtils.Clone(stmt);
                ElseBody.Add(tmpNode);
            }

            //
            ElseBodyPosition = NodeUtils.Clone(rhs.ElseBodyPosition);
        }

        public ImperativeNode IfExprNode { get; set; }
        public List<ImperativeNode> IfBody { get; set; }
        public ImperativeNode IfBodyPosition { get; set; }
        public List<ElseIfBlock> ElseIfList { get; set; }
        public List<ImperativeNode> ElseBody { get; set; }
        public ImperativeNode ElseBodyPosition { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as IfStmtNode;
            if (null == otherNode)
            {
                return false;
            }

            return IfExprNode.Equals(otherNode.IfExprNode)
                    && otherNode != null && IfBody.SequenceEqual(otherNode.IfBody)
                    && ElseIfList != null && ElseIfList.SequenceEqual(otherNode.ElseIfList)
                    && ElseBody != null && ElseBody.SequenceEqual(otherNode.ElseBody);
        }

        public override int GetHashCode()
        {
            return IfExprNode.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // If statement
            buf.Append(ProtoCore.DSDefinitions.Keyword.If);
            buf.Append("(");
            buf.Append(IfExprNode);  
            buf.Append(")");
            
            // If body
            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");
            foreach (ImperativeNode node in IfBody)
            {
                buf.Append(node.ToString());
            }
            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            // Else if
            foreach (ImperativeNode node in ElseIfList)
            {
                buf.Append(node.ToString());
            }

            if (ElseBody.Count > 0)
            {
                // else statement
                buf.Append(ProtoCore.DSDefinitions.Keyword.Else);

                // else body
                buf.Append("\n");
                buf.Append("{");
                buf.Append("\n");
                foreach (ImperativeNode node in ElseBody)
                {
                    buf.Append(node.ToString());
                }
                buf.Append("\n");
                buf.Append("}");
                buf.Append("\n");
            }

            return buf.ToString();
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
            return this.IfExprNode.AsEnumerable()
                .Concat(this.IfBody)
                .Concat(this.IfBodyPosition)
                .Concat(this.ElseIfList)
                .Concat(this.ElseBody)
                .Concat(this.ElseBodyPosition);
        }
    }

    public class WhileStmtNode : ImperativeNode
    {
        public WhileStmtNode()
        {
            Body = new List<ImperativeNode>();
        }

        public WhileStmtNode(WhileStmtNode rhs) : base(rhs)
        {
            Expr = NodeUtils.Clone(rhs.Expr);
            Body = new List<ImperativeNode>(); 
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = NodeUtils.Clone(iNode);
                Body.Add(newNode);
            }
        }

        public ImperativeNode Expr { get; set; }
        public List<ImperativeNode> Body { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as WhileStmtNode;
            if (null == otherNode)
            {
                return false;
            }

            return Expr.Equals(otherNode.Expr) && otherNode != null && Body.SequenceEqual(otherNode.Body);
        }

        public override int GetHashCode()
        {
            return Expr.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // If statement
            buf.Append(ProtoCore.DSDefinitions.Keyword.While);
            buf.Append("(");
            buf.Append(Expr);
            buf.Append(")");

            // If body
            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");
            foreach (ImperativeNode node in Body)
            {
                buf.Append(node.ToString());
            }
            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.While;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitWhileStatementNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return this.Expr.AsEnumerable()
                .Concat(this.Body);
        }
    }

    public class UnaryExpressionNode : ImperativeNode
    {
        public UnaryOperator Operator { get; set; }
        public ImperativeNode Expression { get; set; }

        public UnaryExpressionNode()
        {

        }

        public UnaryExpressionNode(UnaryExpressionNode rhs) : base(rhs)
        {
            Operator = rhs.Operator;
            Expression = NodeUtils.Clone(rhs.Expression);
        }

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
        public ImperativeNode From { get; set; }
        public ImperativeNode To { get; set; }
        public ImperativeNode Step { get; set; }
        public RangeStepOperator StepOperator { get; set; }
        public bool HasRangeAmountOperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            From = NodeUtils.Clone(rhs.From);
            To = NodeUtils.Clone(rhs.To);
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
                   HasRangeAmountOperator == otherNode.HasRangeAmountOperator;
        }

        public override int GetHashCode()
        {
            var fromNodeHashCode =
                (From == null ? base.GetHashCode() : From.GetHashCode());
            var toNodeHashCode =
                (To == null ? base.GetHashCode() : To.GetHashCode());
            var stepNodeHashCode =
                (Step == null ? base.GetHashCode() : Step.GetHashCode());
            var hasRangeAmountOperatorHashCode = HasRangeAmountOperator ? 1 : 0;

            return fromNodeHashCode ^ toNodeHashCode ^
                stepNodeHashCode ^ hasRangeAmountOperatorHashCode;
        }

        // Check if this can be unified associative range expr 
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            string postfix = base.ToString();
            if (!string.IsNullOrEmpty(postfix))
                buf.Append("(");

            buf.Append(From.ToString());
            buf.Append("..");
            if (HasRangeAmountOperator)
                buf.Append("#");
            buf.Append(To.ToString());

            if (Step != null)
            {
                buf.Append("..");
                if (DSASM.RangeStepOperator.ApproximateSize == StepOperator)
                {
                    buf.Append("~");
                }
                else if (DSASM.RangeStepOperator.Number == StepOperator)
                {
                    buf.Append("#");
                }
                buf.Append(Step.ToString());
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
            return new List<Node> {this.From, this.To, this.Step };
        }
    }

    public class ForLoopNode : ImperativeNode
    {
        public ForLoopNode()
        {
            Body = new List<ImperativeNode>();
        }


        public ForLoopNode(ForLoopNode rhs) : base(rhs)
        {
            Body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = NodeUtils.Clone(iNode);
                Body.Add(newNode);
            }
            LoopVariable = NodeUtils.Clone(rhs.LoopVariable);
            Expression = NodeUtils.Clone(rhs.Expression);

            KwForLine = rhs.KwForLine;
            KwForCol = rhs.KwForCol;
            KwInLine = rhs.KwInLine;
            KwInCol = rhs.KwInCol;
        }

        public int KwForLine { get; set; }
        public int KwForCol { get; set; }
        public int KwInLine { get; set; }
        public int KwInCol { get; set; }
        public ImperativeNode LoopVariable { get; set; }
        public ImperativeNode Expression { get; set; }
        public List<ImperativeNode> Body { get; set; }


        public override bool Equals(object other)
        {
            var otherNode = other as ForLoopNode;
            if (null == otherNode)
            {
                return false;
            }

            return LoopVariable.Equals(otherNode.LoopVariable)
                    && Expression.Equals(otherNode.Expression)
                    && otherNode != null && Body.SequenceEqual(otherNode.Body);
        }

        public override int GetHashCode()
        {
            return LoopVariable.GetHashCode() ^ Expression.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // If statement
            buf.Append(ProtoCore.DSDefinitions.Keyword.For);
            buf.Append("(");
            buf.Append(LoopVariable);
            buf.Append(" ");
            buf.Append(ProtoCore.DSDefinitions.Keyword.In);
            buf.Append(" ");
            buf.Append(Expression);
            buf.Append(")");

            // If body
            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");
            foreach (ImperativeNode node in Body)
            {
                buf.Append(node.ToString());
            }
            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");


            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.ForLoop;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitForLoopNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return new List<Node> {this.LoopVariable, this.Expression}.Concat(this.Body);
        }
    }

    public class IdentifierListNode : ArrayNameNode
    {
        public ImperativeNode LeftNode { get; set; }
        public Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public IdentifierListNode()
        {
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = NodeUtils.Clone(rhs.LeftNode);
            RightNode = NodeUtils.Clone(rhs.RightNode);
        }

        public override bool Equals(object other)
        {
            var otherNode = other as IdentifierListNode;
            if (null == otherNode)
                return false;

            return EqualityComparer<ImperativeNode>.Default.Equals(LeftNode, otherNode.LeftNode) &&
                   EqualityComparer<ImperativeNode>.Default.Equals(RightNode, otherNode.RightNode) &&
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
            return LeftNode.ToString() + "." + RightNode.ToString();
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
            return new List<Node> {this.LeftNode, this.RightNode};
        }
    }

    public class BreakNode: ImperativeNode
    {
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(ProtoCore.DSDefinitions.Keyword.Break);
            buf.Append(";");
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Break;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitBreakNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public class ContinueNode: ImperativeNode
    {
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(ProtoCore.DSDefinitions.Keyword.Continue);
            buf.Append(";");
            return buf.ToString();
        }

        public override AstKind Kind
        {
            get
            {
                return AstKind.Continue;
            }
        }

        public override TResult Accept<TResult>(IAstVisitor<TResult> visitor)
        {
            return visitor.VisitContinueNode(this);
        }

        public override IEnumerable<Node> Children()
        {
            return Enumerable.Empty<Node>();
        }
    }

    public static class AstFactory
    {
        public static ImperativeNode BuildIndexExpression(ImperativeNode value, ImperativeNode index)
        {
            var node = BuildFunctionCall(Node.BuiltinGetValueAtIndexTypeName, Node.BuiltinValueAtIndexMethodName, 
                new List<ImperativeNode>() { value, index });
            NodeUtils.SetNodeLocation(node, value, index);
            return node;
        }

        public static ImperativeNode BuildForLoopIndexExpression(ImperativeNode value, ImperativeNode index)
        {
            var node = BuildFunctionCall(Node.BuiltinGetValueAtIndexTypeName, Node.BuiltinValueAtIndexInForLoopMethodName,
                new List<ImperativeNode>() { value, index });
            NodeUtils.SetNodeLocation(node, value, index);
            return node;
        }

        public static ImperativeNode BuildFunctionCall(string className, string functionName, List<ImperativeNode> args)
        {
            return new IdentifierListNode
            {
                LeftNode = new IdentifierNode(className),
                Optr = Operator.dot,
                RightNode = new FunctionCallNode
                {
                    Function = new IdentifierNode(functionName),
                    FormalArguments = args
                }
            };
        }
    }
}

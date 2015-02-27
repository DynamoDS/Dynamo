
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.Utils;
using ProtoCore.DSASM;
using ProtoCore.AST.AssociativeAST;

namespace ProtoCore.AST.ImperativeAST
{
    public abstract class ImperativeNode : Node
    {
        public ImperativeNode()
        {
        }

        public ImperativeNode(ImperativeNode rhs) : base(rhs)
        {
        }
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
            CodeBlockNode = ProtoCore.Utils.NodeUtils.Clone(rhs.CodeBlockNode);
            codeblock = new ProtoCore.LanguageCodeBlock(rhs.codeblock);
            Attributes = new List<ImperativeNode>();
            foreach (ImperativeNode aNode in rhs.Attributes)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
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

            if (CodeBlockNode != null)
            {
                buf.Append(CodeBlockNode.ToString());
            }

            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");

            return buf.ToString();
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

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            if (ArrayDimensions != null)
            {
                buf.Append(ArrayDimensions.ToString());
            }

            return buf.ToString();
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
    }

    public class IdentifierNode : ArrayNameNode 
    {
        public IdentifierNode()
        {
            ArrayDimensions = null;
            datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
        }

        public IdentifierNode(string identName = null)
        {
            ArrayDimensions = null;
            datatype = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kInvalidType, 0);
            Value = Name = identName;
        }


        public IdentifierNode(IdentifierNode rhs) : base(rhs)
        {
            datatype = new ProtoCore.Type
            {
                UID = rhs.datatype.UID,
                rank = rhs.datatype.rank,
                Name = rhs.datatype.Name
            };

            Value = rhs.Value;
            IsLocal = false;
        }

        public ProtoCore.Type datatype { get; set; }
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
                    datatype.Equals(otherNode.datatype) &&
                    base.Equals(otherNode);
        }

        public override string ToString()
        {
            return Value.Replace("%", string.Empty) + base.ToString();
        }
    }

    public class TypedIdentifierNode: IdentifierNode
    {
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

        public override string ToString()
        {
            return Value.ToString();
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

        public override string ToString()
        {
            return Value.ToString();
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

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class CharNode : ImperativeNode
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

    public class StringNode : ImperativeNode
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
                    Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
                }

                if (null != rhs.Type)
                {
                    Type = ProtoCore.Utils.NodeUtils.Clone(rhs.Type);
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
            Function = ProtoCore.Utils.NodeUtils.Clone(rhs.Function);
            FormalArguments = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.FormalArguments)
            {
                ImperativeNode tempNode = ProtoCore.Utils.NodeUtils.Clone(argNode);
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

    public class VarDeclNode : ImperativeNode
    {
        public VarDeclNode()
        {
            memregion = ProtoCore.DSASM.MemoryRegion.kInvalidRegion;
        }

        public ProtoCore.DSASM.MemoryRegion memregion { get; set; }
        public ProtoCore.Type ArgumentType { get; set; }
        public ImperativeNode NameNode { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as VarDeclNode;
            if (null == otherNode)
                return false;

            return memregion == otherNode.memregion &&
                   ArgumentType.Equals(otherNode.ArgumentType) &&
                   EqualityComparer<ImperativeNode>.Default.Equals(NameNode, otherNode.NameNode);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

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
    }

    public class ReturnNode : ImperativeNode
    {
        public ImperativeNode ReturnExpr { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ReturnNode;
            if (null == otherNode)
                return false;

            return null != ReturnExpr && ReturnExpr.Equals(otherNode.ReturnExpr);
        }
    }

    public class ArgumentSignatureNode : ImperativeNode
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

        public override bool Equals(object other)
        {
            var otherNode = other as ArgumentSignatureNode;
            return null != otherNode && Arguments.SequenceEqual(otherNode.Arguments);
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
    }

    public class ExprListNode : ArrayNameNode
    {
        public ExprListNode()
        {
            list = new List<ImperativeNode>();
        }


        public ExprListNode(ExprListNode rhs)
            : base(rhs)
        {
            list = new List<ImperativeNode>();
            foreach (ImperativeNode argNode in rhs.list)
            {
                ImperativeNode tempNode = ProtoCore.Utils.NodeUtils.Clone(argNode);
                list.Add(tempNode);
            }
        }

        public List<ImperativeNode> list { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ExprListNode;
            return null != otherNode && list.SequenceEqual(otherNode.list);
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
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(aNode);
                Body.Add(newNode);
            }
        }

        public List<ImperativeNode> Body { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as CodeBlockNode;
            return null != otherNode && Body.SequenceEqual(otherNode.Body);
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
    }

    public class ConstructorDefinitionNode : ImperativeNode
    {
        public int localVars { get; set; }
        public ArgumentSignatureNode Signature { get; set; }
        public CodeBlockNode FunctionBody { get; set; }

        public override bool Equals(object other)
        {
            var otherNode = other as ConstructorDefinitionNode;
            if (null == otherNode)
                return false;

            return localVars == otherNode.localVars &&
                   EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) &&
                   EqualityComparer<CodeBlockNode>.Default.Equals(FunctionBody, otherNode.FunctionBody);
        }
    }

    public class FunctionDefinitionNode : ImperativeNode
    {
        public int localVars { get; set; }
        public List<ImperativeNode> Attributes { get; set; }
        public CodeBlockNode FunctionBody { get; set; }
        public ProtoCore.Type ReturnType { get; set; }
        public ArgumentSignatureNode Signature { get; set; }

        //only compare return type, attributes and signature
        public override bool Equals(object other)
        {
            var otherNode = other as FunctionDefinitionNode;
            if (null == otherNode)
            {
                return false;
            }

            bool equalSignature = EqualityComparer<ArgumentSignatureNode>.Default.Equals(Signature, otherNode.Signature) &&
                   ReturnType.Equals(otherNode.ReturnType) &&
                   Attributes.SequenceEqual(otherNode.Attributes);

            bool equalBody = FunctionBody.Equals(otherNode.FunctionBody);

            return equalSignature && equalBody;
        }
    }

    public class InlineConditionalNode : ImperativeNode
    {
        public ImperativeNode ConditionExpression { get; set; }
        public ImperativeNode TrueExpression { get; set; }
        public ImperativeNode FalseExpression { get; set; }

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

    public class BinaryExpressionNode : ImperativeNode
    {
        public Guid guid { get; set; }
        public ImperativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public BinaryExpressionNode()
        {
        }

        public BinaryExpressionNode(ImperativeNode left = null, ImperativeNode right = null, ProtoCore.DSASM.Operator optr = DSASM.Operator.none)
        {
            LeftNode = left;
            Optr = optr;
            RightNode = right;
        }

        public BinaryExpressionNode(BinaryExpressionNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = rhs.LeftNode == null ? null : ProtoCore.Utils.NodeUtils.Clone(rhs.LeftNode);
            RightNode = rhs.RightNode == null ? null : ProtoCore.Utils.NodeUtils.Clone(rhs.RightNode);
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


    public class ElseIfBlock : ImperativeNode
    {
        public ElseIfBlock()
        {
            Body = new List<ImperativeNode>();
            ElseIfBodyPosition = new IfStmtPositionNode();
        }


        public ElseIfBlock(ElseIfBlock rhs) : base(rhs)
        {
            Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
            ElseIfBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.ElseIfBodyPosition);

            Body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
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
    }

    public class IfStmtPositionNode: ImperativeNode
    {
        public IfStmtPositionNode()
        {
        }

        public IfStmtPositionNode(IfStmtPositionNode rhs):base(rhs)
        {
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
            IfExprNode = ProtoCore.Utils.NodeUtils.Clone(rhs.IfExprNode);


            //
            IfBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.IfBody)
            {
                ImperativeNode body = ProtoCore.Utils.NodeUtils.Clone(stmt);
                IfBody.Add(body);
            }

            //
            IfBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.IfBodyPosition);

            //
            ElseIfList = new List<ElseIfBlock>();
            foreach (ElseIfBlock elseBlock in rhs.ElseIfList)
            {
                ImperativeNode elseNode = ProtoCore.Utils.NodeUtils.Clone(elseBlock);
                ElseIfList.Add(elseNode as ElseIfBlock);
            }

            //
            ElseBody = new List<ImperativeNode>();
            foreach (ImperativeNode stmt in rhs.ElseBody)
            {
                ImperativeNode tmpNode = ProtoCore.Utils.NodeUtils.Clone(stmt);
                ElseBody.Add(tmpNode);
            }

            //
            ElseBodyPosition = ProtoCore.Utils.NodeUtils.Clone(rhs.ElseBodyPosition);
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
    }

    public class WhileStmtNode : ImperativeNode
    {
        public WhileStmtNode()
        {
            Body = new List<ImperativeNode>();
        }

        public WhileStmtNode(WhileStmtNode rhs) : base(rhs)
        {
            Expr = ProtoCore.Utils.NodeUtils.Clone(rhs.Expr);
            Body = new List<ImperativeNode>(); 
            foreach (ImperativeNode iNode in rhs.Body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
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
    }

    public class UnaryExpressionNode : ImperativeNode
    {
        public ProtoCore.DSASM.UnaryOperator Operator { get; set; }
        public ImperativeNode Expression { get; set; }

        public UnaryExpressionNode()
        {

        }

        public UnaryExpressionNode(UnaryExpressionNode rhs) : base(rhs)
        {
            Operator = rhs.Operator;
            Expression = ProtoCore.Utils.NodeUtils.Clone(rhs.Expression);
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
    }

    public class RangeExprNode : ArrayNameNode
    {
        public ImperativeNode FromNode { get; set; }
        public ImperativeNode ToNode { get; set; }
        public ImperativeNode StepNode { get; set; }
        public ProtoCore.DSASM.RangeStepOperator stepoperator { get; set; }
        public bool HasRangeAmountOperator { get; set; }

        public RangeExprNode()
        {
        }

        public RangeExprNode(RangeExprNode rhs) : base(rhs)
        {
            FromNode = ProtoCore.Utils.NodeUtils.Clone(rhs.FromNode);
            ToNode = ProtoCore.Utils.NodeUtils.Clone(rhs.ToNode);
            if (null != rhs.StepNode)
            {
                StepNode = ProtoCore.Utils.NodeUtils.Clone(rhs.StepNode);
            }
            stepoperator = rhs.stepoperator;
            HasRangeAmountOperator = rhs.HasRangeAmountOperator;
        }

        public override bool Equals(object other)
        {
            var otherNode = other as RangeExprNode;
            if (null == otherNode)
                return false;

            return FromNode.Equals(otherNode.FromNode) &&
                   ToNode.Equals(otherNode.ToNode) &&
                   stepoperator.Equals(otherNode.stepoperator) &&
                   ((StepNode == otherNode.StepNode) || (StepNode != null && StepNode.Equals(otherNode.StepNode))) &&
                   HasRangeAmountOperator == otherNode.HasRangeAmountOperator;
        }

        // Check if this can be unified associative range expr 
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            string postfix = base.ToString();
            if (!string.IsNullOrEmpty(postfix))
                buf.Append("(");

            buf.Append(FromNode.ToString());
            buf.Append("..");
            if (HasRangeAmountOperator)
                buf.Append("#");
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

    public class ForLoopNode : ImperativeNode
    {
        public ForLoopNode()
        {
            body = new List<ImperativeNode>();
        }


        public ForLoopNode(ForLoopNode rhs) : base(rhs)
        {
            body = new List<ImperativeNode>();
            foreach (ImperativeNode iNode in rhs.body)
            {
                ImperativeNode newNode = ProtoCore.Utils.NodeUtils.Clone(iNode);
                body.Add(newNode);
            }
            loopVar = ProtoCore.Utils.NodeUtils.Clone(rhs.loopVar);
            expression = ProtoCore.Utils.NodeUtils.Clone(rhs.expression);

            KwForLine = rhs.KwForLine;
            KwForCol = rhs.KwForCol;
            KwInLine = rhs.KwInLine;
            KwInCol = rhs.KwInCol;
        }

        public int KwForLine { get; set; }
        public int KwForCol { get; set; }
        public int KwInLine { get; set; }
        public int KwInCol { get; set; }
        public ImperativeNode loopVar { get; set; }
        public ImperativeNode expression { get; set; }
        public List<ImperativeNode> body { get; set; }


        public override bool Equals(object other)
        {
            var otherNode = other as ForLoopNode;
            if (null == otherNode)
            {
                return false;
            }

            return loopVar.Equals(otherNode.loopVar)
                    && expression.Equals(otherNode.expression)
                    && otherNode != null && body.SequenceEqual(otherNode.body);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();

            // If statement
            buf.Append(ProtoCore.DSDefinitions.Keyword.For);
            buf.Append("(");
            buf.Append(loopVar);
            buf.Append(" ");
            buf.Append(ProtoCore.DSDefinitions.Keyword.In);
            buf.Append(" ");
            buf.Append(expression);
            buf.Append(")");

            // If body
            buf.Append("\n");
            buf.Append("{");
            buf.Append("\n");
            foreach (ImperativeNode node in body)
            {
                buf.Append(node.ToString());
            }
            buf.Append("\n");
            buf.Append("}");
            buf.Append("\n");


            return buf.ToString();
        }
    }

    public class IdentifierListNode : ImperativeNode
    {
        public ImperativeNode LeftNode { get; set; }
        public ProtoCore.DSASM.Operator Optr { get; set; }
        public ImperativeNode RightNode { get; set; }

        public IdentifierListNode()
        {
        }

        public IdentifierListNode(IdentifierListNode rhs) : base(rhs)
        {
            Optr = rhs.Optr;
            LeftNode = ProtoCore.Utils.NodeUtils.Clone(rhs.LeftNode);
            RightNode = ProtoCore.Utils.NodeUtils.Clone(rhs.RightNode);
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

        public override string ToString()
        {
            return LeftNode.ToString() + "." + RightNode.ToString();
        }
    }

    public class PostFixNode : ImperativeNode
    {
        public ImperativeNode Identifier { get; set; }
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

    public class BreakNode: ImperativeNode
    {
        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(ProtoCore.DSDefinitions.Keyword.Break);
            buf.Append(";");
            return buf.ToString();
        }

        public override bool Equals(object other)
        {
            return other is BreakNode;
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

        public override bool Equals(object other)
        {
            return other is ContinueNode;
        }
    }

    public class DefaultArgNode : ImperativeNode
    {// not supposed to be used in parser 
    }

    public class ThrowNode : ImperativeNode
    {
        public ImperativeNode expression { get; set; }
    }

    public class TryBlockNode : ImperativeNode
    {
        public List<ImperativeNode> body { get; set; }
    }

    public class CatchFilterNode : ImperativeNode
    {
        public IdentifierNode var { get; set; }
        public ProtoCore.Type type { get; set; }
    }

    public class CatchBlockNode : ImperativeNode
    {
        public CatchFilterNode catchFilter { get; set; }
        public List<ImperativeNode> body { get; set; }
    }

    public class ExceptionHandlingNode : ImperativeNode
    {
        public TryBlockNode tryBlock { get; set; }
        public List<CatchBlockNode> catchBlocks { get; set; }

        public ExceptionHandlingNode()
        {
            catchBlocks = new List<CatchBlockNode>();
        }
    }
}

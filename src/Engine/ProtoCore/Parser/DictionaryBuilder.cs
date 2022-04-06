using System.Collections.Generic;
using ProtoCore.DSASM;

namespace ProtoCore.AST
{
    /// <summary>
    /// Dictionary syntax like { "foo" : 12 } isn't part of the AST. Instead, this helper class exists to 
    /// build a function call to build a Dictionary via zero touch.
    /// </summary>
    public abstract class DictionaryExpressionBuilderBase<T>
    {
        internal readonly List<T> keys;
        internal readonly List<T> values;

        protected static readonly string DictionaryTypeName = typeof(DesignScript.Builtin.Dictionary).FullName;
        protected static readonly string DictionaryByKeysValuesName = nameof(DesignScript.Builtin.Dictionary.ByKeysValues);

        protected int col;
        protected int line;
        protected int endCol;
        protected int endLine;

        public DictionaryExpressionBuilderBase()
        {
            keys = new List<T>();
            values = new List<T>();
        }

        public void AddValue(T value)
        {
            this.values.Add(value);
        }

        public void AddKey(T value)
        {
            this.keys.Add(value);
        }

        public void SetNodeStartLocation(ProtoCore.DesignScriptParser.Token token)
        {
            line = token.line;
            col = token.col;
        }

        public void SetNodeEndLocation(ProtoCore.DesignScriptParser.Token token)
        {
            endLine = token.line;
            endCol = token.col;
        }

        public abstract T ToFunctionCall();
    }

    namespace AssociativeAST
    {
        public class DictionaryExpressionBuilder : DictionaryExpressionBuilderBase<AssociativeNode>
        {
            public DictionaryExpressionBuilder() : base() { }
            
            public override AssociativeAST.AssociativeNode ToFunctionCall()
            {
                var keys = new ExprListNode
                {
                    Exprs = this.keys
                };

                var values = new ExprListNode
                {
                    Exprs = this.values
                };

                var f = AstFactory.BuildFunctionCall(DictionaryTypeName, DictionaryByKeysValuesName,
                    new List<AssociativeNode>() { keys, values });
                f.col = this.col;
                f.line = this.line;
                f.endCol = this.endCol;
                f.endLine = this.endLine;

                return f;
            }
        }
    }

    namespace ImperativeAST
    {
        public class DictionaryExpressionBuilder : DictionaryExpressionBuilderBase<ImperativeNode>
        {
            public DictionaryExpressionBuilder() : base() { }
            
            public override ImperativeNode ToFunctionCall()
            {
                var keys = new ExprListNode
                {
                    Exprs = this.keys
                };

                var values = new ExprListNode
                {
                    Exprs = this.values
                };

                var f = new IdentifierListNode
                {
                    LeftNode = new IdentifierNode(DictionaryTypeName),
                    Optr = Operator.dot,
                    RightNode = new FunctionCallNode
                    {
                        Function = new IdentifierNode(DictionaryByKeysValuesName),
                        FormalArguments = new List<ImperativeNode> {  keys, values }
                    }
                };

                f.col = this.col;
                f.line = this.line;
                f.endCol = this.endCol;
                f.endLine = this.endLine;

                return f;
            }
        }
    }
}

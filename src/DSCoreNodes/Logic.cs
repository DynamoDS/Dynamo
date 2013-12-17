using System.Collections.Generic;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace DSCoreNodes
{
    /// <summary>
    /// Methods for handling boolean logic.
    /// </summary>
    public class Logic
    {
        //TODO: Make Variable Input?
        /// <summary>
        /// Abstract base class for short-circuiting binary logic operators.
        /// </summary>
        public abstract class BinaryLogic : NodeModel
        {
            private readonly Operator _op;

            protected BinaryLogic(string symbol, Operator op)
            {
                _op = op;

                InPortData.Add(new PortData("a", "operand"));
                InPortData.Add(new PortData("b", "operand"));
                OutPortData.Add(new PortData("a" + symbol + "b", "result"));
                RegisterAllPorts();
            }

            public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        new BinaryExpressionNode(inputAstNodes[0], inputAstNodes[1], _op))
                };
            }
        }

        /// <summary>
        /// Short-circuiting Logical AND
        /// </summary>
        public class And : BinaryLogic
        {
            public And() : base("∧", Operator.and) { }
        }

        /// <summary>
        /// Short-circuiting Logical OR
        /// </summary>
        public class Or : BinaryLogic
        {
            public Or() : base("∨", Operator.or) { }
        }
    }
}

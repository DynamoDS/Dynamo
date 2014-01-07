using System.Collections.Generic;
using System.Linq;
using DSCoreNodesUI;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace DSCoreNodes.Logic
{
    //TODO: Make Variable Input?
    /// <summary>
    /// Abstract base class for short-circuiting binary logic operators.
    /// </summary>
    public abstract class BinaryLogic : VariableInputNode
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
                    BuildNestedOpCall(Enumerable.Reverse(inputAstNodes)))
            };
        }

        private AssociativeNode BuildNestedOpCall(IEnumerable<AssociativeNode> inputAstNodes)
        {
            return inputAstNodes.Skip(1)
                                .Aggregate(
                                    inputAstNodes.First(),
                                    (current, node) =>
                                        AstFactory.BuildBinaryExpression(node, current, _op));
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override string InputRootName
        {
            get { return "bool"; }
        }

        protected override string TooltipRootName
        {
            get { return "Boolean #"; }
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

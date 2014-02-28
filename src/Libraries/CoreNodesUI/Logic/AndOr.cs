using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace DSCore.Logic
{
    /// <summary>
    /// Abstract base class for short-circuiting binary logic operators.
    /// </summary>
    [Browsable(false)]
    public abstract class BinaryLogic : VariableInputNode
    {
        private readonly Operator _op;

        protected BinaryLogic(string symbol, Operator op)
        {
            _op = op;

            InPortData.Add(new PortData("bool0", "operand"));
            InPortData.Add(new PortData("bool1", "operand"));
            OutPortData.Add(new PortData("", "result"));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var inputs = inputAstNodes as IEnumerable<AssociativeNode>;
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    inputs.Reverse()
                          .Aggregate(
                              (current, node) =>
                                  AstFactory.BuildBinaryExpression(node, current, _op)))
            };
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override string GetInputName(int index)
        {
            return "bool" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "Boolean #" + index;
        }
    }

    /// <summary>
    /// Short-circuiting Logical AND
    /// </summary>
    [NodeName("And")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean AND: Returns true only if both of the inputs are true. If either is false, returns false.")]
    [IsDesignScriptCompatible]
    public class And : BinaryLogic
    {
        public And() : base("∧", Operator.and) { }
    }

    /// <summary>
    /// Short-circuiting Logical OR
    /// </summary>
    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean OR: Returns true if either of the inputs are true. If neither are true, returns false.")]
    [IsDesignScriptCompatible]
    public class Or : BinaryLogic
    {
        public Or() : base("∨", Operator.or) { }
    }
}

using System.Collections.Generic;
using System.Linq;

using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;
using Autodesk.DesignScript.Runtime;
using DSCoreNodesUI.Properties;

namespace DSCore.Logic
{
    /// <summary>
    /// Abstract base class for short-circuiting binary logic operators.
    /// </summary>
    [SupressImportIntoVM]
    public abstract class BinaryLogic : VariableInputNode
    {
        private readonly Operator _op;

        protected BinaryLogic(Operator op)
        {
            _op = op;

            InPortData.Add(new PortData("bool0", Resources.PortDataOperandToolTip));
            InPortData.Add(new PortData("bool1", Resources.PortDataOperandToolTip));
            OutPortData.Add(new PortData("", Resources.PortDataResultToolTip));
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
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeDescription("AndDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class And : BinaryLogic
    {
        public And() : base(Operator.and) { }
    }

    /// <summary>
    /// Short-circuiting Logical OR
    /// </summary>
    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.LOGIC)]
    [NodeDescription("OrDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class Or : BinaryLogic
    {
        public Or() : base(Operator.or) { }
    }
}

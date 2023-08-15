using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace CoreNodeModels.Logic
{
    /// <summary>
    /// Abstract base class for short-circuiting binary logic operators.
    /// </summary>
    [SupressImportIntoVM]
    public abstract class BinaryLogic : VariableInputNode
    {
        private readonly Operator _op;

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        protected BinaryLogic(Operator op, IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Auto;
            _op = op;
        }

        protected BinaryLogic(Operator op)
        {
            ArgumentLacing = LacingStrategy.Auto;
            _op = op;

            InPorts.Add(new PortModel(PortType.Input, this, new PortData("bool0", Resources.PortDataOperandToolTip + 0)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("bool1", Resources.PortDataOperandToolTip + 1)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("bool", Resources.PortDataResultToolTip)));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            UseLevelAndReplicationGuide(inputAstNodes);
            var inputs = inputAstNodes as IEnumerable<AssociativeNode>;
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    inputs.Reverse()
                          .Aggregate(
                              (current, node) =>
                                  AstFactory.BuildFunctionCall(Op.GetOpFunction(_op), new List<AssociativeNode>{ node, current})))
            };
        }

        protected override void RemoveInput()
        {
            if (InPorts.Count > 2)
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
    [NodeCategory("Core.Math")]
    [NodeDescription("AndDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortTypes("bool")]
    [AlsoKnownAs("DSCore.Logic.And", "DSCoreNodesUI.Logic.And")]
    public class And : BinaryLogic
    {
        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        private And(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base( Operator.and, inPorts, outPorts) { }

        public And() : base(Operator.and) { }
    }

    /// <summary>
    /// Short-circuiting Logical OR
    /// </summary>
    [NodeName("Or")]
    [NodeCategory("Core.Math")]
    [NodeDescription("OrDescription", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortTypes("bool")]
    [AlsoKnownAs("DSCore.Logic.Or", "DSCoreNodesUI.Logic.Or")]
    public class Or : BinaryLogic
    {
        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        private Or(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base( Operator.or, inPorts, outPorts) { }

        public Or() : base(Operator.or) { }
    }
}

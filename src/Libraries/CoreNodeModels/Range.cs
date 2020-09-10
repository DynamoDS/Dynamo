using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Dynamo.Properties;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.DSASM;

namespace CoreNodeModels
{
    [NodeName("Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("RangeDescription", typeof(Properties.Resources))]
    [NodeSearchTags("RangeSearchTags", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Range")]
    public class Range : NodeModel
    {
        private readonly IntNode startPortDefaultValue = new IntNode(0);
        private readonly IntNode endPortDefaultValue = new IntNode(9);
        private readonly IntNode stepPortDefaultValue = new IntNode(1);

        /// <summary>
        /// Json Constructor for Range Node
        /// </summary>
        /// <param name="inPorts"></param>
        /// <param name="outPorts"></param>
        [JsonConstructor]
        private Range(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            if (inPorts.Count() == 3)
            {
                inPorts.ElementAt(0).DefaultValue = startPortDefaultValue;
                inPorts.ElementAt(1).DefaultValue = endPortDefaultValue;
                inPorts.ElementAt(2).DefaultValue = stepPortDefaultValue;
            }
            else
            {
                // If information from json does not look correct, clear the default ports and add ones with default value
                InPorts.Clear();
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("end", Resources.RangePortDataEndToolTip, endPortDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue)));
            }
            if(outPorts.Count() == 0) OutPorts.Add(new PortModel(PortType.Output, this, new PortData("seq", Resources.RangePortDataSeqToolTip)));
            ArgumentLacing = LacingStrategy.Auto;
            SetNodeStateBasedOnConnectionAndDefaults();
        }

        /// <summary>
        /// Default constructor for Range Node
        /// </summary>
        public Range()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("end", Resources.RangePortDataEndToolTip, endPortDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("seq", Resources.RangePortDataSeqToolTip)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Auto;
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                var connectedPorts = Enumerable.Range(0, this.InPorts.Count)
                    .Where(index=>this.InPorts[index].IsConnected)
                    .ToList();

                // 3d, 4th, 5th are always connected.
                connectedPorts.AddRange(new List<int> { 3, 4, 5 });
                return new[]
                {
                     AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                             AstFactory.BuildFunctionObject(
                                Constants.kFunctionRangeExpression,
                                6,
                                connectedPorts,
                                new List<AssociativeNode>
                                {
                                    inputAstNodes[0],
                                    inputAstNodes[1],
                                    inputAstNodes[2],
                                    new IntNode(0),
                                    new BooleanNode(true),
                                    new BooleanNode(false)
                                }))
                };
            }
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new RangeExprNode
                    {
                        From = inputAstNodes[0],
                        To = inputAstNodes[1],
                        Step = inputAstNodes[2],
                        StepOperator = ProtoCore.DSASM.RangeStepOperator.StepSize
                    })
            };
        }
    }

    [NodeName("Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("SequenceDescription", typeof(Properties.Resources))]
    [NodeSearchTags("SequenceSearchTags", typeof(Properties.Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCoreNodesUI.Sequence")]
    public class Sequence : NodeModel
    {
        private readonly IntNode startPortDefaultValue = new IntNode(0);
        private readonly IntNode amountPortDefaultValue = new IntNode(10);
        private readonly IntNode stepPortDefaultValue = new IntNode(1);

        [JsonConstructor]
        private Sequence(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            if (inPorts.Count() == 3)
            {
                inPorts.ElementAt(0).DefaultValue = startPortDefaultValue;
                inPorts.ElementAt(1).DefaultValue = amountPortDefaultValue;
                inPorts.ElementAt(2).DefaultValue = stepPortDefaultValue;
            }
            else
            {
                // If information from json does not look correct, clear the default ports and add ones with default value
                InPorts.Clear();
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("amount", Resources.RangePortDataAmountToolTip, amountPortDefaultValue)));
                InPorts.Add(new PortModel(PortType.Input, this, new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue)));
            }
            if (outPorts.Count() == 0) OutPorts.Add(new PortModel(PortType.Output, this, new PortData("seq", Resources.RangePortDataSeqToolTip)));
            ArgumentLacing = LacingStrategy.Auto;
            SetNodeStateBasedOnConnectionAndDefaults();
        }

        public Sequence()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("start", Resources.RangePortDataStartToolTip, startPortDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("amount", Resources.RangePortDataAmountToolTip, amountPortDefaultValue)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("step", Resources.RangePortDataStepToolTip, stepPortDefaultValue)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("seq", Resources.RangePortDataSeqToolTip)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Auto;
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                var connectedPorts = Enumerable.Range(0, this.InPorts.Count)
                    .Where(index=>this.InPorts[index].IsConnected)
                    .ToList();

                // 3d, 4th, 5th are always connected.
                connectedPorts.AddRange(new List<int> { 3, 4, 5 });
                return new[]
                {
                     AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                             AstFactory.BuildFunctionObject(
                                Constants.kFunctionRangeExpression,
                                6,
                                connectedPorts,
                                new List<AssociativeNode>
                                {
                                    inputAstNodes[0],
                                    inputAstNodes[1],
                                    inputAstNodes[2],
                                    new IntNode(0),
                                    new BooleanNode(true),
                                    new BooleanNode(true)
                                }))
                };
            }
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new RangeExprNode 
                    {
                        From = inputAstNodes[0],
                        To = inputAstNodes[1],
                        Step = inputAstNodes[2],
                        HasRangeAmountOperator = true,
                        StepOperator = ProtoCore.DSASM.RangeStepOperator.StepSize                     
                    })
            };
        }
    }
}

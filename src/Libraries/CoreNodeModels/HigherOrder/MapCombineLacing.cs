using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
using Dynamo.Utilities;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace CoreNodeModels.HigherOrder
{
    [NodeName("List.Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListMapDescription", typeof(Resources))]
    [NodeSearchTags("ListMapSearchTags",typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Map", "DSCoreNodesUI.HigherOrder.Map")]
    public class Map : NodeModel
    {
        [JsonConstructor]
        private Map(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public Map()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list", Resources.MapPortDataListToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("f(x)", Resources.MapPortDataFxToolTip)));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("mapped", Resources.MapPortDataResultToolTip)));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    IsPartiallyApplied
                        ? AstFactory.BuildFunctionObject(
                            "__Map",
                            2,
                            new[] { 0, 1 }.Where(index=>InPorts[index].IsConnected).Select(x => 1 - x),
                            Enumerable.Reverse(inputAstNodes).ToList())
                        : AstFactory.BuildFunctionCall("__Map", Enumerable.Reverse(inputAstNodes).ToList()))
            };
        }
    }

    public abstract class CombinatorNode : VariableInputNode
    {
        private readonly int minPorts;

        protected CombinatorNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        protected CombinatorNode() : this(3)
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("comb", Resources.CombinatorPortDataCombToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list1", Resources.PortDataListToolTip + " #1")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list2", Resources.PortDataListToolTip + " #2")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("combined", Resources.CombinatorPortDataResultToolTip)));

            RegisterAllPorts();
        }

        protected CombinatorNode(int minPorts)
        {
            this.minPorts = minPorts;
        }

        protected override string GetInputName(int index)
        {
            return "list" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return Resources.PortDataListToolTip + " #" + index;
        }

        protected override void RemoveInput()
        {
            if (InPorts.Count > minPorts)
                base.RemoveInput();
        }
    }

    [NodeName("List.Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListCombineDescription", typeof(Resources))]
    [NodeSearchTags("ListCombineSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Combine", "DSCoreNodesUI.HigherOrder.Combine")]
    public class Combine : CombinatorNode
    {
        [JsonConstructor]
        private Combine(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public Combine() : base()
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__Combine",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                        }))
            };
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    [NodeName("List.ForEach")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListForEachDescription", typeof(Resources))]
    [NodeSearchTags("ListForEachSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.ForEach", "DSCoreNodesUI.HigherOrder.ForEach")]
    public class ForEach : CombinatorNode
    {
        [JsonConstructor]
        private ForEach(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }
  
        public ForEach() : base(2)
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__ForEach",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                        }))
            };
        }
    }

    //MAGN-3382 [IsVisibleInDynamoLibrary(false)]
    [NodeName("List.LaceShortest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListLaceShortestDescription", typeof(Resources))]
    [NodeSearchTags("ListLaceShortestSearchTags", typeof(Resources))]
    [OutPortTypes("List")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.LaceShortest", "DSCoreNodesUI.HigherOrder.LaceShortest")]
    public class LaceShortest : CombinatorNode
    {
        [JsonConstructor]
        private LaceShortest(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public LaceShortest() : base()
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__LaceShortest",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                        }))
            };
        }
    }

    //MAGN-3382 [IsVisibleInDynamoLibrary(false)]
    [NodeName("List.LaceLongest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListLaceLongestDescription", typeof(Resources))]
    [NodeSearchTags("ListLaceLongestSearchTags", typeof(Resources))]
    [OutPortTypes("List")]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.LaceLongest", "DSCoreNodesUI.HigherOrder.LaceLongest")]
    public class LaceLongest : CombinatorNode
    {
        [JsonConstructor]
        private LaceLongest(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public LaceLongest() : base()
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__LaceLongest",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                        }))
            };
        }
    }

    ///<search>cross</search>
    //MAGN-3382 [IsVisibleInDynamoLibrary(false)]
    [NodeName("List.CartesianProduct")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListCartesianProductDescription", typeof(Resources))]
    [NodeSearchTags("ListCartesianProductSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [OutPortTypes("List")]
    [AlsoKnownAs("DSCore.CartesianProduct", "DSCoreNodesUI.HigherOrder.CartesianProduct")]
    public class CartesianProduct : CombinatorNode
    {
        [JsonConstructor]
        private CartesianProduct(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public CartesianProduct() : base()
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__CartesianProduct",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            AstFactory.BuildExprList(inputAstNodes.Skip(1).ToList())
                        }))
            };
        }
    }

    [NodeName("List.Reduce")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListReduceDescription", typeof(Resources))]
    [NodeSearchTags("ListReduceSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Reduce", "DSCoreNodesUI.HigherOrder.Reduce")]
    public class Reduce : VariableInputNode
    {
        private readonly PortData reductorPort;

        [JsonConstructor]
        private Reduce(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) :base(inPorts, outPorts) { }

        public Reduce()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("reductor", Resources.ReducePortDataReductorToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("seed", Resources.ReducePortDataSeedToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list1", Resources.PortDataListToolTip + " #1")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("reduced", Resources.ReducePortDataResultToolTip)));

            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPorts.Count > 3)
            {
                base.RemoveInput();
            }
        }

        protected override void AddInput()
        {
            base.AddInput();
        }

        private void UpdateReductorPort()
        {
            if (InPorts.Count > 6) 
                reductorPort.Name = "f(x1, x2, ... xN, a)";
            else
            {
                if (InPorts.Count == 3) 
                    reductorPort.Name = "f(x, a)";
                else
                {
                    reductorPort.Name = "f("
                        + string.Join(
                            ", ",
                            Enumerable.Range(0, InPorts.Count - 2).Select(x => "x" + (x + 1)))
                        + ", a)";
                }
            }
            RegisterAllPorts();
        }

        protected override string GetInputName(int index)
        {
            return "list" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return Resources.PortDataListToolTip + " #" + index;
        }

        protected override int GetInputIndex()
        {
            return InPorts.Count - 1;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__Reduce",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            inputAstNodes[1],
                            AstFactory.BuildExprList(inputAstNodes.Skip(2).ToList())
                        }))
            };
        }
    }

    [NodeName("List.Scan")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListScanDescription", typeof(Resources))]
    [NodeSearchTags("ListScanSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.ScanList", "DSCoreNodesUI.HigherOrder.ScanList")]
    public class ScanList : VariableInputNode
    {
        private readonly PortData reductorPort;

        [JsonConstructor]
        private ScanList(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public ScanList()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("reductor", Resources.ScanPortDataReductorToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("seed", Resources.ScanPortDataSeedToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list1", Resources.PortDataListToolTip + " #1")));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("scanned", Resources.ScanPortDataResultToolTip)));

            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPorts.Count > 3)
            {
                base.RemoveInput();
            }
        }

        protected override void AddInput()
        {
            base.AddInput();
        }

        private void UpdateReductorPort()
        {
            if (InPorts.Count > 6)
                reductorPort.Name = "f(x1, x2, ... xN, a)";
            else
            {
                if (InPorts.Count == 3)
                    reductorPort.Name = "f(x, a)";
                else
                {
                    reductorPort.Name = "f("
                        + string.Join(
                            ", ",
                            Enumerable.Range(0, InPorts.Count - 2).Select(x => "x" + (x + 1)))
                        + ", a)";
                }
            }
            RegisterAllPorts();
        }

        protected override string GetInputName(int index)
        {
            return "list" + index;
        }

        protected override string GetInputTooltip(int index)
        {
            return "List" + index;
        }

        protected override int GetInputIndex()
        {
            return InPorts.Count - 1;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "__Scan",
                        new List<AssociativeNode>
                        {
                            inputAstNodes[0],
                            inputAstNodes[1],
                            AstFactory.BuildExprList(inputAstNodes.Skip(2).ToList())
                        }))
            };
        }
    }

    [NodeName("List.Filter")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListFilterDescription", typeof(Resources))]
    [NodeSearchTags("ListFilterSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Filter", "DSCoreNodesUI.HigherOrder.Filter")]
    public class Filter : NodeModel
    {
        [JsonConstructor]
        private Filter(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public Filter()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("list", Resources.FilterPortDataListToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("condition", Resources.FilterPortDataConditionToolTip)));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("in", Resources.FilterPortDataResultInToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("out", Resources.FilterPortDataResultOutToolTip)));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var packedId = "__temp" + AstIdentifierGuid;
            return new[]
            {
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(packedId),
                    AstFactory.BuildFunctionCall("__Filter", inputAstNodes)),
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    new IdentifierNode(packedId)
                    {
                        ArrayDimensions = new ArrayNode { Expr = AstFactory.BuildIntNode(0) }
                    }),
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(1),
                    new IdentifierNode(packedId)
                    {
                        ArrayDimensions = new ArrayNode { Expr = AstFactory.BuildIntNode(1) }
                    })
            };
        }
    }

    [NodeName("ReplaceByCondition")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ReplaceByConditionDescription", typeof(Resources))]
    [NodeSearchTags("ReplaceByConditionSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Replace", "DSCoreNodesUI.HigherOrder.Replace")]
    public class Replace : NodeModel
    {
        [JsonConstructor]
        private Replace(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public Replace()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("item", Resources.ReplacePortDataItemToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("replaceWith", Resources.ReplacePortDataReplaceWithToolTip)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("condition", Resources.ReplacePortDataConditionToolTip)));

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("var", Resources.ReplacePortDataResultToolTip)));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall("__Replace", inputAstNodes))
            };
        }
    }
}

﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using CoreNodeModels.Properties;
using Dynamo.Graph.Nodes;
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
        public Map()
        {
            InPortData.Add(new PortData("list", Resources.MapPortDataListToolTip));
            InPortData.Add(new PortData("f(x)", Resources.MapPortDataFxToolTip));

            OutPortData.Add(new PortData("mapped", Resources.MapPortDataResultToolTip));

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
                            new[] { 0, 1 }.Where(HasConnectedInput).Select(x => 1 - x),
                            Enumerable.Reverse(inputAstNodes).ToList())
                        : AstFactory.BuildFunctionCall("__Map", Enumerable.Reverse(inputAstNodes).ToList()))
            };
        }
    }

    public abstract class CombinatorNode : VariableInputNode
    {
        private readonly int minPorts;

        protected CombinatorNode() : this(3)
        {
            InPortData.Add(new PortData("comb", Resources.CombinatorPortDataCombToolTip));
            InPortData.Add(new PortData("list1", Resources.PortDataListToolTip + " #1"));
            InPortData.Add(new PortData("list2", Resources.PortDataListToolTip + " #2"));

            OutPortData.Add(new PortData("combined", Resources.CombinatorPortDataResultToolTip));

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
            if (InPortData.Count > minPorts)
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
        public ForEach() : base(2) { }

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
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.LaceShortest", "DSCoreNodesUI.HigherOrder.LaceShortest")]
    public class LaceShortest : CombinatorNode
    {
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
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.LaceLongest", "DSCoreNodesUI.HigherOrder.LaceLongest")]
    public class LaceLongest : CombinatorNode
    {
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
    [AlsoKnownAs("DSCore.CartesianProduct", "DSCoreNodesUI.HigherOrder.CartesianProduct")]
    public class CartesianProduct : CombinatorNode
    {
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

    /*
    [NodeName("True For Any")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if any elements in a sequence satisfy the given predicate.")]
    [IsDesignScriptCompatible]
    public class TrueForAny : NodeModel
    {
        public TrueForAny()
        {
            InPortData.Add(new PortData("list", "The list to test."));
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements"));

            OutPortData.Add(new PortData("any?", "Whether or not any elements satisfy the given predicate."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "TrueForAny",
                        (inputAstNodes as IEnumerable<AssociativeNode>).Reverse().ToList()))
            };
        }
    }

    [NodeName("True For All")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if all elements in a sequence satisfy the given predicate.")]
    [IsDesignScriptCompatible]
    public class TrueForAll : NodeModel
    {
        public TrueForAll()
        {
            InPortData.Add(new PortData("list", "The list to test."));
            InPortData.Add(new PortData("p(x)", "The predicate used to test items"));

            OutPortData.Add(new PortData("all?", "Whether or not all items satisfy the given predicate."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "TrueForAll",
                        (inputAstNodes as IEnumerable<AssociativeNode>).Reverse().ToList()))
            };
        }
    }
    */

    [NodeName("List.Reduce")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("ListReduceDescription", typeof(Resources))]
    [NodeSearchTags("ListReduceSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("DSCore.Reduce", "DSCoreNodesUI.HigherOrder.Reduce")]
    public class Reduce : VariableInputNode
    {
        private readonly PortData reductorPort;

        public Reduce()
        {
            InPortData.Add(new PortData("reductor", Resources.ReducePortDataReductorToolTip));
            InPortData.Add(new PortData("seed", Resources.ReducePortDataSeedToolTip));
            InPortData.Add(new PortData("list1", Resources.PortDataListToolTip + " #1"));

            OutPortData.Add(new PortData("reduced", Resources.ReducePortDataResultToolTip));

            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 3)
            {
                base.RemoveInput();
                //UpdateReductorPort();
            }
        }

        protected override void AddInput()
        {
            base.AddInput();
            //UpdateReductorPort();
        }

        private void UpdateReductorPort()
        {
            if (InPortData.Count > 6) 
                reductorPort.NickName = "f(x1, x2, ... xN, a)";
            else
            {
                if (InPortData.Count == 3) 
                    reductorPort.NickName = "f(x, a)";
                else
                {
                    reductorPort.NickName = "f("
                        + string.Join(
                            ", ",
                            Enumerable.Range(0, InPortData.Count - 2).Select(x => "x" + (x + 1)))
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
            return InPortData.Count - 1;
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

        public ScanList()
        {
            InPortData.Add(new PortData("reductor", Resources.ScanPortDataReductorToolTip));
            InPortData.Add(new PortData("seed", Resources.ScanPortDataSeedToolTip));
            InPortData.Add(new PortData("list1", Resources.PortDataListToolTip + " #1"));

            OutPortData.Add(new PortData("scanned", Resources.ScanPortDataResultToolTip));

            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 3)
            {
                base.RemoveInput();
                //UpdateReductorPort();
            }
        }

        protected override void AddInput()
        {
            base.AddInput();
            //UpdateReductorPort();
        }

        private void UpdateReductorPort()
        {
            if (InPortData.Count > 6)
                reductorPort.NickName = "f(x1, x2, ... xN, a)";
            else
            {
                if (InPortData.Count == 3)
                    reductorPort.NickName = "f(x, a)";
                else
                {
                    reductorPort.NickName = "f("
                        + string.Join(
                            ", ",
                            Enumerable.Range(0, InPortData.Count - 2).Select(x => "x" + (x + 1)))
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
            return InPortData.Count - 1;
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
        public Filter()
        {
            InPortData.Add(new PortData("list", Resources.FilterPortDataListToolTip));
            InPortData.Add(new PortData("condition", Resources.FilterPortDataConditionToolTip));

            OutPortData.Add(new PortData("in", Resources.FilterPortDataResultInToolTip));
            OutPortData.Add(new PortData("out", Resources.FilterPortDataResultOutToolTip));

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
        public Replace()
        {
            InPortData.Add(new PortData("item", Resources.ReplacePortDataItemToolTip));
            InPortData.Add(new PortData("replaceWith", Resources.ReplacePortDataReplaceWithToolTip));
            InPortData.Add(new PortData("condition", Resources.ReplacePortDataConditionToolTip));

            OutPortData.Add(new PortData("var", Resources.ReplacePortDataResultToolTip));

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

using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DSCore
{
    [NodeName("List.Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription(
        "Applies a function over all elements of a list, generating a new list from the results.")]
    [IsDesignScriptCompatible]
    public class Map : NodeModel
    {
        public Map()
        {
            InPortData.Add(new PortData("list", "The list to map over."));
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements"));

            OutPortData.Add(new PortData("mapped", "Mapped list"));

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
            InPortData.Add(new PortData("comb", "Combinator"));
            InPortData.Add(new PortData("list1", "List #1"));
            InPortData.Add(new PortData("list2", "List #2"));

            OutPortData.Add(new PortData("combined", "Combined lists"));

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
            return "List" + index;
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > minPorts)
                base.RemoveInput();
        }
    }

    [NodeName("List.Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_ACTION)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    [IsDesignScriptCompatible]
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
    [NodeDescription("Performs a computation on each element of a list. Does not accumulate results.")]
    [IsDesignScriptCompatible]
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
    [NodeDescription("Applies a combinator to each pair resulting from a shortest lacing of the input lists. All lists are truncated to the length of the shortest input.")]
    [IsDesignScriptCompatible]
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
    [NodeDescription("Applies a combinator to each pair resulting from a longest lacing of the input lists. All lists have their last element repeated to match the length of the longest input.")]
    [IsDesignScriptCompatible]
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
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [IsDesignScriptCompatible]
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
    [NodeDescription("Reduces a list into a new value by combining each element with an accumulated result.")]
    [IsDesignScriptCompatible]
    public class Reduce : VariableInputNode
    {
        private readonly PortData reductorPort;

        public Reduce()
        {
            reductorPort = new PortData(
                "reductor",
                "Reductor Function: accepts one item from each list being reduced, and the current accumulated value, result is the new accumulated value.");

            InPortData.Add(reductorPort);
            InPortData.Add(new PortData("seed", "Starting accumulated value, to be passed into the first call to the Reductor function."));
            InPortData.Add(new PortData("list1", "List #1"));

            OutPortData.Add(new PortData("reduced", "Reduced lists"));

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
    [NodeDescription("Reduces a list into a new value by combining each element with an accumulated result, produces a list of successive reduced values.")]
    [IsDesignScriptCompatible]
    public class ScanList : VariableInputNode
    {
        private readonly PortData reductorPort;

        public ScanList()
        {
            reductorPort = new PortData(
                "reductor",
                "Reductor Function: accepts one item from each list being reduced, and the current accumulated value, result is the new accumulated value.");

            InPortData.Add(reductorPort);
            InPortData.Add(new PortData("seed", "Starting accumulated value, to be passed into the first call to the Reductor function."));
            InPortData.Add(new PortData("list1", "List #1"));

            OutPortData.Add(new PortData("scanned", "Scanned lists"));

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
    [NodeDescription("Filters a sequence by a given condition such that for an arbitrary element \"x,\" condition(x) = True or False.")]
    [IsDesignScriptCompatible]
    public class Filter : NodeModel
    {
        public Filter()
        {
            InPortData.Add(new PortData("list", "List to filter"));
            InPortData.Add(new PortData("condition", "Predicate used to determine if an element is filtered in or out."));

            OutPortData.Add(
                new PortData("in", "List containing all elements \"x\" where condition(x) = True"));
            OutPortData.Add(
                new PortData("out", "List containing all elements \"x\" where condition(x) = False"));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var packedId = "__temp" + GUID.ToString().Replace("-", "");
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
    [NodeDescription("Replaces an object with a given substitute if the original object satisfies a given condition.")]
    [IsDesignScriptCompatible]
    public class Replace : NodeModel
    {
        public Replace()
        {
            InPortData.Add(new PortData("item", "Item to potentially be replaced"));
            InPortData.Add(new PortData("replaceWith", "Object to replace with"));
            InPortData.Add(new PortData("condition", "Predicate used to determine if it should be replaced."));

            OutPortData.Add(new PortData("var", "If condition(item) = True, then \"replaceWith\" is returned. Otherwise \"item\" is returned unaltered."));

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

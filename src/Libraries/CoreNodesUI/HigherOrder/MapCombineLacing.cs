using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;

namespace DSCore
{
    [NodeName("Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a function over all elements of a list, generating a new list from the results.")]
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
            throw new NotImplementedException();
        }
    }

    public abstract class CombinatorNode : VariableInputNode
    {
        protected CombinatorNode()
        {
            InPortData.Add(new PortData("comb", "Combinator"));
            InPortData.Add(new PortData("list1", "List #1"));
            InPortData.Add(new PortData("list2", "List #2"));

            OutPortData.Add(new PortData("combined", "Combined lists"));

            RegisterAllPorts();
        }

        protected override string InputRootName
        {
            get { return "list"; }
        }

        protected override string TooltipRootName
        {
            get { return "List"; }
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 3)
                base.RemoveInput();
        }
    }

    [NodeName("Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    public class Combine : CombinatorNode
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("For Each")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Performs a computation on each element of a list. Does not accumulate results.")]
    public class ForEach : CombinatorNode
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Lace Shortest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair resulting from a shortest lacing of the input lists. All lists are truncated to the length of the shortest input.")]
    public class LaceShortest : CombinatorNode
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Lace Longest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair resulting from a longest lacing of the input lists. All lists have their last element repeated to match the length of the longest input.")]
    public class LaceLongest : NodeModel
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Cartesian Product")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    ///<search>cross</search>
    public class CartesianProduct : NodeModel
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("True For Any")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if any elements in a sequence satisfy the given predicate.")]
    public class TrueForAny : NodeModel
    {
        public TrueForAny()
        {
            InPortData.Add(new PortData("list", "The list to test."));
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements"));

            OutPortData.Add(new PortData("any?", "Whether or not any elements satisfy the given predicate."));

            RegisterAllPorts();
        }
    }

    [NodeName("True For All")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_QUERY)]
    [NodeDescription("Tests to see if all elements in a sequence satisfy the given predicate.")]
    public class TrueForAll : NodeModel
    {
        public TrueForAll()
        {
            InPortData.Add(new PortData("list", "The list to test."));
            InPortData.Add(new PortData("p(x)", "The predicate used to test items"));

            OutPortData.Add(new PortData("all?", "Whether or not all items satisfy the given predicate."));

            RegisterAllPorts();
        }
    }

    [NodeName("Reduce")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Reduces a list into a new value by combining each element with an accumulated result.")]
    public class Reduce : VariableInputNode
    {
        public Reduce()
        {
            InPortData.Add(new PortData("list1", "List #1"));
            InPortData.Add(new PortData("a", "Starting accumulated value, to be passed into the first call to the Reductor function."));
            InPortData.Add(new PortData("f(x, a)", "Reductor Function: first argument is an arbitrary item in the list being reduced, second is the current accumulated value, result is the new accumulated value."));
            
            OutPortData.Add(new PortData("", "Result"));

            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 3)
                base.RemoveInput();
        }

        protected override string InputRootName
        {
            get { return "list"; }
        }

        protected override string TooltipRootName
        {
            get { return "List"; }
        }

        protected override int GetInputIndex()
        {
            return InPortData.Count - 1;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Filter")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_EVALUATE)]
    [NodeDescription("Filters a sequence by a given predicate \"p\" such that for an arbitrary element \"x\" p(x) = True or False.")]
    public class Filter : NodeModel
    {
        public Filter()
        {
            InPortData.Add(new PortData("list", "List to filter"));
            InPortData.Add(new PortData("p(x)", "Predicate"));

            OutPortData.Add(new PortData("in", "List containing all elements \"x\" where p(x) = True"));
            OutPortData.Add(new PortData("out", "List containing all elements \"x\" where p(x) = False"));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    public class MinByKey : NodeModel
    {
        
    }

    public class MaxByKey : NodeModel
    {
        
    }

    public class SortByKey : NodeModel
    {
        
    }

    public class SortByCompare : NodeModel
    {
        
    }
}

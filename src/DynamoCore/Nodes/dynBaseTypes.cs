﻿//Copyright 2013 Ian Keough

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using RestSharp.Contrib;
using Value = Dynamo.FScheme.Value;
using System.Globalization;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static partial class BuiltinNodeCategories
    {
        public const string CORE = "Core";
        public const string CORE_PRIMITIVES = "Core.Primitives";
        public const string CORE_STRINGS = "Core.Strings";
        public const string CORE_LISTS = "Core.Lists";
        public const string CORE_VIEW = "Core.View";
        public const string CORE_ANNOTATE = "Core.Annotate";
        public const string CORE_SELECTION = "Revit.Selection";
        public const string CORE_EVALUATE = "Core.Evaluate";
        public const string CORE_TIME = "Core.Time";
        public const string CORE_FUNCTIONS = "Core.Functions";
        public const string CORE_GEOMETRY = "Core.Geometry";
        public const string CORE_EXPERIMENTAL_GEOMETRY = "Core.Experimental.Geometry";

        public const string LOGIC = "Logic";
        public const string LOGIC_MATH = "Logic.Math";
        public const string LOGIC_COMPARISON = "Logic.Comparison";
        public const string LOGIC_CONDITIONAL = "Logic.Conditional";
        public const string LOGIC_LOOP = "Logic.Loop";

        public const string CREATEGEOMETRY = "Create Geometry";
        public const string CREATEGEOMETRY_POINT = "Create Geometry.Point";
        public const string CREATEGEOMETRY_CURVE = "Create Geometry.Curve";
        public const string CREATEGEOMETRY_SOLID = "Create Geometry.Solid";
        public const string CREATEGEOMETRY_SURFACE = "Create Geometry.Surface";

        public const string MODIFYGEOMETRY= "Modify Geometry";
        public const string MODIFYGEOMETRY_INTERSECT = "Modify Geometry.Intersect";
        public const string MODIFYGEOMETRY_TRANSFORM = "Modify Geometry.Transform";
        public const string MODIFYGEOMETRY_TESSELATE = "Modify Geometry.Tesselate";

        public const string REVIT = "Revit";
        public const string REVIT_DOCUMENT = "Revit.Document";
        public const string REVIT_DATUMS = "Revit.Datums";
        public const string REVIT_FAMILYCREATION = "Revit.Family Creation";
        public const string REVIT_VIEW = "Revit.View";
        public const string REVIT_PARAMETERS = "Revit.Parameters";
        public const string REVIT_BAKE = "Revit.Bake";
        public const string REVIT_API = "Revit.API";

        public const string IO = "Input/Output";
        public const string IO_FILE = "Input/Output.File";
        public const string IO_NETWORK = "Input/Output.Network";
        public const string IO_HARDWARE = "Input/Output.Hardware";

        public const string ANALYZE = "Analyze";
        public const string ANALYZE_MEASURE = "Analyze.Measure";
        public const string ANALYZE_DISPLAY = "Analyze.Display";
        public const string ANALYZE_SURFACE = "Analyze.Surface";
        public const string ANALYZE_STRUCTURE = "Analyze.Structure";
        public const string ANALYZE_CLIMATE = "Analyze.Climate";
        public const string ANALYZE_ACOUSTIC = "Analyze.Acoustic";
        public const string ANALYZE_SOLAR = "Analyze.Solar";

        public const string SCRIPTING = "Scripting";
        public const string SCRIPTING_CUSTOMNODES = "Scripting.Custom Nodes";
        public const string SCRIPTING_PYTHON = "Scripting.Python";
        public const string SCRIPTING_DESIGNSCRIPT = "Scripting.DesignScript";

    }

    static class Utilities
    {
        public static string Ellipsis(string value, int desiredLength)
        {
            if (desiredLength > value.Length)
            {
                return value;
            }
            else
            {
                return value.Remove(desiredLength - 1) + "...";
            }
        }
    }

    #region FScheme Builtin Interop

    public abstract class dynBuiltinFunction : dynNodeWithOneOutput
    {
        public Func<FSharpList<Value>, Value> Func { get; protected internal set; }

        internal dynBuiltinFunction(FSharpFunc<FSharpList<Value>, Value> builtIn) 
            : this(builtIn.Invoke)
        { }

        internal dynBuiltinFunction(Func<FSharpList<Value>, Value> builtIn)
        {
            Func = builtIn;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
           return Func(args);
        }
    }

    #endregion

    public abstract partial class dynVariableInput : dynNodeWithOneOutput
    {
        protected dynVariableInput()
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        private int _lastEvaledAmt;
        public override bool RequiresRecalc
        {
            get
            {
                return _lastEvaledAmt != InPortData.Count || base.RequiresRecalc;
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        protected internal virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx, typeof(object)));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode elNode)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            _lastEvaledAmt = InPortData.Count;
        }
    }

    [NodeName("Identity")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES )]
    [NodeDescription("Identity function")]
    public class dynIdentity : dynNodeWithOneOutput
    {
        public dynIdentity()
        {
            InPortData.Add(new PortData("x", "in", typeof(object)));
            OutPortData.Add(new PortData("x", "out", typeof(object)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return args[0];
        }
    }

    #region Functions

    [NodeName("Compose Functions")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    [NodeDescription("Composes two single parameter functions into one function.")]
    public class dynComposeFunctions : dynNodeWithOneOutput
    { 
        public dynComposeFunctions()
        {
            InPortData.Add(new PortData("f", "A Function", typeof(Value.Function)));
            InPortData.Add(new PortData("g", "A Function", typeof(Value.Function)));
            OutPortData.Add(new PortData("g ∘ f", "Composed function: g(f(x))", typeof(Value.Function)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var g = ((Value.Function)args[1]).Item;

            return Value.NewFunction(Utils.ConvertToFSchemeFunc(x => g.Invoke(Utils.MakeFSharpList(f.Invoke(x)))));
        }
    }

    #endregion

    #region Lists

    [NodeName("Reverse")]
    [NodeDescription("Reverses a list")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    public class dynReverse : dynBuiltinFunction
    {
        public dynReverse()
            : base(FScheme.Rev)
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("rev", "Reversed list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("List")]
    [NodeDescription("Makes a new list out of the given inputs")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    public class dynNewList : dynVariableInput
    {
        public dynNewList()
        {
            InPortData.Add(new PortData("index0", "Item Index #0", typeof(object)));
            OutPortData.Add(new PortData("list", "A list", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputRootName()
        {
            return "index";
        }

        protected override string GetTooltipRootName()
        {
            return "Item Index #";
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
                return base.Compile(portNames);
            else
                return new FunctionNode("list", portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol("list"))
                .Item.Invoke(args);
        }
    }

    [NodeName("Sort-With")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns a sorted list, using the given comparitor.")]
    public class dynSortWith : dynBuiltinFunction
    {
        public dynSortWith()
            : base(FScheme.SortWith)
        {
            InPortData.Add(new PortData("c(x, y)", "Comparitor", typeof(object)));
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Sort-By")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns a sorted list, using the given key mapper. The key mapper must return either all numbers or all strings.")]
    public class dynSortBy : dynBuiltinFunction
    {
        public dynSortBy()
            : base(FScheme.SortBy)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(object), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Sort")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns a sorted list of numbers or strings.")]
    public class dynSort : dynBuiltinFunction
    {
        public dynSort()
            : base(FScheme.Sort)
        {
            InPortData.Add(new PortData("list", "List of numbers or strings to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Minimum")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns the minimum value of a list, using the given key mapper. The key mapper must return either all numbers or all strings.")]
    public class dynListMin : dynBuiltinFunction
    {
        public dynListMin() 
            : base(FScheme.Min)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(Value.Function), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to get the minimum value of.", typeof(Value.List)));
            OutPortData.Add(new PortData("min", "Minimum value.", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Maximum")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns the maximum value of a list, using the given key mapper. The key mapper must return either all numbers or all strings.")]
    public class dynListMax : dynBuiltinFunction
    {
        public dynListMax()
            : base(FScheme.Max)
        {
            InPortData.Add(new PortData("f(x)", "Key Mapper", typeof(Value.Function), Value.NewFunction(Utils.ConvertToFSchemeFunc(FScheme.Identity))));
            InPortData.Add(new PortData("list", "List to get the maximum value of.", typeof(Value.List)));
            OutPortData.Add(new PortData("max", "Maximum value.", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Reduce")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Reduces a sequence.")]
    [NodeSearchTags("foldl")]
    public class dynFold : dynBuiltinFunction
    {
        public dynFold()
            : base(FScheme.FoldL)
        {
            InPortData.Add(new PortData("f(x, a)", "Reductor Funtion", typeof(object)));
            InPortData.Add(new PortData("a", "Seed", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence", typeof(Value.List)));
            OutPortData.Add(new PortData("out", "Result", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Filter")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Filters a sequence by a given predicate \"p\" such that for an arbitrary element \"x\" p(x) = True.")]
    public class dynFilter : dynBuiltinFunction
    {
        public dynFilter()
            : base(FScheme.Filter)
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(Value.List)));
            OutPortData.Add(new PortData("filtered", "Sequence containing all elements \"x\" where p(x) = True", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Filter Out")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Filters a sequence by a given predicate \"p\" such that for an arbitrary element \"x\" p(x) = False.")]
    public class dynFilterOut : dynNodeWithOneOutput
    {
        public dynFilterOut()
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(Value.Function)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(Value.List)));
            OutPortData.Add(new PortData("filtered", "Sequence containing all elements \"x\" where p(x) = False", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var p = ((Value.Function)args[0]).Item;
            var seq = ((Value.List)args[1]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(seq.Where(x => !FScheme.ValueToBool(p.Invoke(Utils.MakeFSharpList(x))))));
        }
    }

    [NodeName("Number Range")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Creates a sequence of numbers in the specified range.")]
    [AlsoKnownAs("Dynamo.Nodes.dynBuildSeq")]
    public class dynNumberRange : dynBuiltinFunction
    {
        public dynNumberRange()
            : base(FScheme.BuildSeq)
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("end", "Number to end the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(Value.Number)));
            OutPortData.Add(new PortData("seq", "New sequence", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Number Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Creates a sequence of numbers.")]
    public class dynNumberSeq : dynNodeWithOneOutput
    {
        public dynNumberSeq()
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(Value.Number)));
            InPortData.Add(new PortData("amount", "Amount of numbers in the sequence", typeof(Value.Number)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(Value.Number)));
            OutPortData.Add(new PortData("seq", "New sequence", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var start = ((Value.Number)args[0]).Item;
            var amount = (int)((Value.Number)args[1]).Item;
            var step = ((Value.Number)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(MakeSequence(start, amount, step)));
        }

        private IEnumerable<Value> MakeSequence(double start, int amount, double step)
        {
            for (int i = 0; i < amount; i++)
            {
                yield return Value.NewNumber(start);
                start += step;
            }
        }
    }

    [NodeName("Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    [NodeSearchTags("zip")]
    public class dynCombine : dynVariableInput
    {
        public dynCombine()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "List #1", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "List #2", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputRootName()
        {
            return "list";
        }

        protected override string GetTooltipRootName()
        {
            return "List #";
        }

        protected internal override void RemoveInput()
        {
            //don't allow us to remove
            //the second list
            if (InPortData.Count == 3)
                return;

            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 3)
                base.RemoveInput();
        }

        protected internal override void AddInput()
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        protected override void LoadNode(XmlNode elNode)
        {
            var inputAttr = elNode.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(GetInputRootName() + GetInputNameIndex(), "", typeof(object)));
                }

                RegisterAllPorts();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Map(args);
        }
    }

    public class dynLacerBase : dynVariableInput
    {
        private readonly Func<FSharpList<Value>, Value> _func;

        public dynLacerBase(Func<FSharpList<Value>, Value> func)
        {
            _func = func;

            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "List #1", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "List #2", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "list";
        }

        protected override string GetTooltipRootName()
        {
            return "List #";
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected internal override void AddInput()
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        protected override void LoadNode(XmlNode elNode)
        {
            var inputAttr = elNode.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(GetInputRootName() + GetInputNameIndex(), "", typeof(object)));
                }

                RegisterAllPorts();
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return _func(args);
        }
    }

    [NodeName("Cartesian Product")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [NodeSearchTags("cross")]
    public class dynCartProd : dynLacerBase
    {
        public dynCartProd() : base(FScheme.CartProd) { }
    }

    [NodeName("Lace Shortest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each pair resulting from a shortest lacing of the input lists. All lists are truncated to the length of the shortest input.")]
    public class dynLaceShortest : dynLacerBase
    {
        public dynLaceShortest() : base(FScheme.LaceShortest) { }
    }

    [NodeName("Lace Longest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each pair resulting from a longest lacing of the input lists. All lists have their last element repeated to match the length of the longest input.")]
    public class dynLaceLongest : dynLacerBase
    {
        public dynLaceLongest() : base(FScheme.LaceLongest) { }
    }

    [NodeName("Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Maps a sequence")]
    public class dynMap : dynBuiltinFunction
    {
        public dynMap()
            : base(FScheme.Map)
        {
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to map over.", typeof(Value.List)));
            OutPortData.Add(new PortData("mapped", "Mapped sequence", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("For Each")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Performs a computation on each element of a list. Does not accumulate results.")]
    public class dynForEach : dynBuiltinFunction
    {
        public dynForEach()
            : base(FScheme.ForEach)
        {
            InPortData.Add(new PortData("f(x)", "The computation to perform on each element", typeof(object)));
            InPortData.Add(new PortData("seq", "The list to of elements.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "", typeof(Value.Dummy)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get { return true; }
            set { }
        }
    }

    [NodeName("True For All")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Tests to see if all elements in a sequence satisfy the given predicate.")]
    public class dynAndMap : dynBuiltinFunction
    {
        public dynAndMap()
            : base(FScheme.AndMap)
        {
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to test.", typeof(Value.List)));
            OutPortData.Add(new PortData("all?", "Whether or not all elements satisfy the given predicate.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("True For Any")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Tests to see if any elements in a sequence satisfy the given predicate.")]
    public class dynOrMap : dynBuiltinFunction
    {
        public dynOrMap()
            : base(FScheme.OrMap)
        {
            InPortData.Add(new PortData("p(x)", "The predicate used to test elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to test.", typeof(Value.List)));
            OutPortData.Add(new PortData("any?", "Whether or not any elements satisfy the given predicate.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Split List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Deconstructs a list pair.")]
    public class dynDeCons : dynNodeModel
    {
        public dynDeCons()
        {
            InPortData.Add(new PortData("list", "A non-empty list", typeof(Value.List)));
            OutPortData.Add(new PortData("first", "Head of the list", typeof(object)));
            OutPortData.Add(new PortData("rest", "Tail of the list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var list = (Value.List)args[0];

            outPuts[OutPortData[0]] = list.Item.Head;
            outPuts[OutPortData[1]] = Value.NewList(list.Item.Tail);
        }
    }

    [NodeName("Add to List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Adds an element to the beginning of a list.")]
    public class dynList : dynBuiltinFunction
    {
        public dynList()
            : base(FScheme.Cons)
        {
            InPortData.Add(new PortData("item", "The Head of the new list", typeof(object)));
            InPortData.Add(new PortData("list", "The Tail of the new list", typeof(object)));
            OutPortData.Add(new PortData("list", "New list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Take From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Takes elements from a list")]
    public class dynTakeList : dynBuiltinFunction
    {
        public dynTakeList()
            : base(FScheme.Take)
        {
            InPortData.Add(new PortData("amt", "Amount of elements to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("elements", "List of extraced elements", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Drop From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Drops elements from a list")]
    public class dynDropList : dynBuiltinFunction
    {
        public dynDropList()
            : base(FScheme.Drop)
        {
            InPortData.Add(new PortData("amt", "Amount of elements to drop", typeof(object)));
            InPortData.Add(new PortData("list", "The list to drop elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("elements", "List of remaining elements", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Shift List Indeces")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Shifts the indeces of a list by a given amount.")]
    public class dynShiftList : dynNodeWithOneOutput
    {
        public dynShiftList()
        {
            InPortData.Add(
                new PortData("amt", "Amount to shift the list indeces by.", typeof(Value.Number)));
            InPortData.Add(new PortData("list", "List to shift indeces of.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "Shifted list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var amt = (int)((Value.Number)args[0]).Item;
            var list = ((Value.List)args[1]).Item;

            if (amt == 0)
                return Value.NewList(list);

            if (amt < 0)
            {
                return Value.NewList(
                    Utils.SequenceToFSharpList(
                        list.Skip(-amt).Concat(list.Take(-amt))));
            }

            var len = list.Length;
            return Value.NewList(
                Utils.SequenceToFSharpList(
                    list.Skip(len - amt).Concat(list.Take(len - amt))));
        }
    }


    [NodeName("Get From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets an element from a list at a specified index.")]
    public class dynGetFromList : dynBuiltinFunction
    {
        public dynGetFromList()
            : base(FScheme.Get)
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract the element from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted element", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Slice List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Get a sublist from a given list.")]
    public class dynSliceList : dynNodeWithOneOutput
    {
        public dynSliceList()
        {
            InPortData.Add(new PortData("start", "Inclusive start index", typeof(object)));
            InPortData.Add(new PortData("count", "Number of elements to obtain from list", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract the elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted elements", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var start = (int)((Value.Number)args[0]).Item;
            var count = (int)((Value.Number)args[1]).Item;
            var lst = ((Value.List)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Skip(start).Take(count)));
        }
    }

    [NodeName("Remove From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Removes an element from a list at a specified index.")]
    public class dynRemoveFromList : dynNodeWithOneOutput
    {
        public dynRemoveFromList()
        {
            InPortData.Add(new PortData("index", "Index of the element to remove", typeof(object)));
            InPortData.Add(new PortData("list", "The list to remove the element from", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "List with element removed", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var idx = (int)((Value.Number)args[0]).Item;
            var lst = ((Value.List)args[1]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Where((_, i) => i != idx)));
        }
    }

    [NodeName("Drop Every Nth")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Removes every nth element from a list.")]
    public class dynRemoveEveryNth : dynNodeWithOneOutput
    {
        public dynRemoveEveryNth()
        {
            InPortData.Add(new PortData("n", "All indeces that are a multiple of this number will be removed.", typeof(object)));
            InPortData.Add(new PortData("list", "The list to remove elements from.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "List with elements removed.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var n = (int)((Value.Number)args[0]).Item;
            var lst = ((Value.List)args[1]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Where((_, i) => (i + 1) % n != 0)));
        }
    }

    [NodeName("Take Every Nth")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Extracts every nth element from a list.")]
    public class dynTakeEveryNth : dynNodeWithOneOutput
    {
        public dynTakeEveryNth()
        {
            InPortData.Add(new PortData("n", "All indeces that are a multiple of this number will be extracted.", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "Extracted elements.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var n = (int)((Value.Number)args[0]).Item;
            var lst = ((Value.List)args[1]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(lst.Where((_, i) => (i + 1) % n == 0)));
        }
    }

    [NodeName("Empty List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("An empty list")]
    [IsInteractive(false)]
    public class dynEmpty : dynNodeModel
    {
        public dynEmpty()
        {
            OutPortData.Add(new PortData("empty", "An empty list", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new SymbolNode("empty");
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Is Empty List?")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Checks to see if the given list is empty.")]
    public class dynIsEmpty : dynBuiltinFunction
    {
        public dynIsEmpty()
            : base(FScheme.IsEmpty)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("empty?", "Is the given list empty?", typeof(bool)));

            RegisterAllPorts();
        }
    }

    [NodeName("List Length")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets the length of a list")]
    [NodeSearchTags("count")]
    public class dynLength : dynBuiltinFunction
    {
        public dynLength()
            : base(FScheme.Len)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("length", "Length of the list", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Concatenate Lists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Concatenates two lists.")]
    public class dynAppend : dynBuiltinFunction
    {
        public dynAppend()
            : base(FScheme.Append)
        {
            InPortData.Add(new PortData("listA", "First list", typeof(Value.List)));
            InPortData.Add(new PortData("listB", "Second list", typeof(Value.List)));
            OutPortData.Add(new PortData("A+B", "A appended onto B", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("First of List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets the Head of a list")]
    public class dynFirst : dynBuiltinFunction
    {
        public dynFirst()
            : base(FScheme.Car)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("first", "First element in the list", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Rest of List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets the Tail of a list (list with the first element removed).")]
    public class dynRest : dynBuiltinFunction
    {
        public dynRest()
            : base(FScheme.Cdr)
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("rest", "Tail of the list.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Partition List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Create a lists of lists with each sub-list containing n elements.")]
    public class dynSlice : dynNodeWithOneOutput
    {
        public dynSlice()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The length of each new sub list.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists of length n.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("A list is required to slice.");
            if(args.Length != 2)
                throw new Exception("A number is required to specify the sublist length.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in ther 
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();
            int count = 0;

            foreach (Value v in lst)
            {
                count++;

                currList.Add(v);

                if (count == n)
                {
                    finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
                    currList = new List<Value>();
                    count = 0;
                }
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
            }

            return Value.NewList(Utils.MakeFSharpList(finalList.ToArray()));

        }
    }

    [NodeName("Diagonal Right List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Create a diagonal lists of lists from top left to lower right.")]
    public class dynDiagonalRightList : dynNodeWithOneOutput
    {
        public dynDiagonalRightList()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The width of the new sub lists.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists representing diagonals.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to create diagonals.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in the
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();

            //int count = 0;

            var startIndices = new List<int>();
            
            //get indices along 'side' of array
            for (int i = n; i < lst.Count(); i += n)
            {
                startIndices.Add(i);
            }

            startIndices.Reverse();

            //get indices along 'top' of array
            for (int i = 0; i < n; i++)
            {
                startIndices.Add(i);
            }

            foreach(int start in startIndices)
            {
                int index = start;

                while (index < lst.Count())
                {
                    var currentRow = (int)Math.Ceiling((index + 1)/(double)n);
                    currList.Add(lst.ElementAt(index));
                    index += n + 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int) Math.Ceiling((index + 1)/(double)n);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
                currList = new List<Value>();
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
            }

            return Value.NewList(Utils.MakeFSharpList(finalList.ToArray()));

        }
    }

    [NodeName("Diagonal Left List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Create a diagonal lists of lists from top right to lower left.")]
    public class dynDiagonalLeftList : dynNodeWithOneOutput
    {
        public dynDiagonalLeftList()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The width of the new sublists.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "A list of lists representing diagonals.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to create diagonals.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            var n = (int)Math.Round(((Value.Number)args[1]).Item);

            //if we have less elements in the
            //incoming list than the slice size,
            //just return the list
            if (lst.Count() < n)
            {
                return Value.NewList(lst);
            }

            var finalList = new List<Value>();
            var currList = new List<Value>();

            //int count = 0;

            var startIndices = new List<int>();

            //get indices along 'top' of array
            for (int i = 0; i < n; i++)
            {
                startIndices.Add(i);
            }

            //get indices along 'side' of array
            for (int i = n-1 + n; i < lst.Count(); i += n)
            {
                startIndices.Add(i);
            }

            foreach (int start in startIndices)
            {
                int index = start;

                while (index < lst.Count())
                {
                    var currentRow = (int)Math.Ceiling((index + 1) / (double)n);
                    currList.Add(lst.ElementAt(index));
                    index += n - 1;

                    //ensure we are skipping a row to get the next index
                    var nextRow = (int)Math.Ceiling((index + 1) / (double)n);
                    if (nextRow > currentRow + 1 || nextRow == currentRow)
                        break;
                }
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
                currList = new List<Value>();
            }

            if (currList.Any())
            {
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
            }

            return Value.NewList(Utils.MakeFSharpList<Value>(finalList.ToArray()));

        }
    }

    [NodeName("Transpose Lists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Swaps rows and columns in a list of lists.")]
    public class dynTranspose : dynBuiltinFunction
    {
        public dynTranspose() 
            : base(FScheme.Transpose)
        {
            InPortData.Add(new PortData("lists", "The list of lists to transpose.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "Transposed list of lists.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Build Sublists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Build sublists from a list using a list-building syntax.")]
    public partial class dynSublists : dynBasicInteractive<string>
    {
        public dynSublists()
        {
            InPortData.Add(new PortData("list", "The list from which to create sublists.", typeof(Value.List)));
            InPortData.Add(new PortData("offset", "The offset to apply to the sub-list. Ex. \"0..3\" with an offset of 1 will yield {0,1,2,3}{1,2,3,4}{2,3,4,5}...", typeof(Value.List)));

            OutPortData.RemoveAt(0); //remove the existing blank output
            OutPortData.Add(new PortData("list", "The sublists.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
            Value = "";
        }

        protected override void LoadNode(XmlNode elNode)
        {
            base.LoadNode(elNode);
            processTextForNewInputs();
        }

        private void processTextForNewInputs()
        {
            if (InPortData.Count > 2)
                InPortData.RemoveRange(2, InPortData.Count - 2);

            var parameters = new HashSet<string>();

            try
            {
                processText(
                    Value,
                    int.MaxValue,
                    delegate(string identifier)
                    {
                        parameters.Add(identifier);
                        return 0;
                    });
            }
            catch (Exception e)
            {
                Error(e.Message);
            }

            foreach (string parameter in parameters)
            {
                InPortData.Add(new PortData(parameter, "variable", typeof(Value.Number)));
            }

            RegisterInputs();
        }

        internal static readonly Regex IdentifierPattern = new Regex(@"(?<id>[a-zA-Z_][^ ]*)|\[(?<id>\w(?:[^}\\]|(?:\\}))*)\]");
        internal static readonly string[] RangeSeparatorTokens = { "..", ":", };

        private static List<Tuple<int, int, int>> processText(string text, int maxVal, Func<string, int> idFoundCallback)
        {
            text = text.Replace(" ", "");

            string[] chunks = text.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (!chunks.Any())
                throw new Exception("Sub-list expression could not be parsed.");

            var ranges = new List<Tuple<int, int, int>>();

            foreach (string chunk in chunks)
            {
                string[] valueRange = chunk.Split(RangeSeparatorTokens, StringSplitOptions.RemoveEmptyEntries);

                int start = 0;
                int step = 1;

                if (!int.TryParse(valueRange[0], out start))
                {
                    var match = IdentifierPattern.Match(valueRange[0]);
                    if (match.Success)
                    {
                        start = idFoundCallback(match.Groups["id"].Value);
                    }
                    else
                    {
                        throw new Exception("Range start could not be parsed.");
                    }
                }

                int end = start;

                if (valueRange.Length > 1)
                {
                    if (!int.TryParse(valueRange[1], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[1]);
                        if (match.Success)
                        {
                            end = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range " + (valueRange.Length > 2 ? "step" : "end") + "could not be parsed.");
                        }
                    }
                }

                if (valueRange.Length > 2)
                {
                    step = end;
                    if (!int.TryParse(valueRange[2], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[2]);
                        if (match.Success)
                        {
                            end = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range end could not be parsed.");
                        }
                    }
                }

                if (start < 0 || end < 0 || step <= 0)
                    throw new Exception("Range values must be greater than zero.");

                //if any values are greater than the length of the list - fail
                if (start >= maxVal || end >= maxVal)
                    throw new Exception("The start or end of a range is greater than the number of available elements in the list.");

                ranges.Add(Tuple.Create(start, end, step));
            }

            return ranges;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to create sub-lists.");

            FSharpList<Value> list = ((Value.List)args[0]).Item;
            int offset = Convert.ToInt32(((Value.Number)args[1]).Item);

            if (offset <= 0)
                throw new Exception(InPortData[1].NickName + " argument must be greater than zero.");

            //sublist creation semantics are as follows:
            //EX. 1..2,5..8
            //This expression says give me elements 1-2 then jump 3 and give me elements 5-8
            //For a list 1,2,3,4,5,6,7,8,9,10, this will give us
            //1,2,5,8,2,3,6,9

            var paramLookup = args.Skip(2)
                                  .Select(
                                      (x, i) => new { Name = InPortData[i+2].NickName, Argument = x })
                                  .ToDictionary(x => x.Name, x => Convert.ToInt32(((Value.Number)x.Argument).Item));

            var ranges = processText(Value, list.Length, x => paramLookup[x]);

            //move through the list, creating sublists
            var finalList = new List<Value>();

            for (int j = 0; j < list.Count(); j+=offset)
            {
                var currList = new List<Value>();
                foreach (Tuple<int, int, int> range in ranges)
                {
                    if (range.Item1 + j > list.Count() - 1 ||
                        range.Item2 + j > list.Count() - 1)
                    {
                        continue;
                    }

                    for (int i = range.Item1 + j; i <= range.Item2 + j; i += range.Item3)
                    {
                        currList.Add(list.ElementAt(i));
                    }
                }

                if (currList.Any())
                    finalList.Add(FScheme.Value.NewList(Utils.SequenceToFSharpList(currList)));
            }

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(finalList));

        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }
    }

    [NodeName("Repeat")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Construct a list of a given item repeated a given number of times.")]
    public class dynRepeat : dynNodeWithOneOutput
    {
        public dynRepeat()
        {
            InPortData.Add(new PortData("thing", "The thing to repeat. This can be a single object or a list.", typeof(Value)));
            InPortData.Add(new PortData("length", "The number of times to repeat.", typeof(Value.Number)));
            OutPortData.Add(new PortData("list", "The list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            int n = Convert.ToInt16(((Value.Number) args[1]).Item);

            if(n<0)
                throw new Exception("Can't make a repeated list of a negative amount.");

            return Value.NewList(Utils.SequenceToFSharpList(Enumerable.Repeat(args[0], n).ToList()));
        }
    }


    [NodeName("Flatten Completely")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Flatten nested lists into one list.")]
    public class dynFlattenList : dynNodeWithOneOutput
    {
        public dynFlattenList()
        {
            InPortData.Add(new PortData("list", "The list of lists to flatten.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "The flattened list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        internal static IEnumerable<Value> Flatten(IEnumerable<Value> list, ref int amt)
        {
            while (amt != 0)
            {
                bool keepFlattening = false;

                list = list.SelectMany<Value, Value>(
                    x =>
                    {
                        if (x is Value.List)
                        {
                            keepFlattening = true;
                            return (x as Value.List).Item;
                        }
                        return new[] { x };
                    }).ToList();

                if (keepFlattening)
                    amt--;
                else
                    break;
            }
            return list;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to flatten.");

            IEnumerable<Value> list = ((Value.List)args[0]).Item;

            int amt = -1;
            return Value.NewList(Utils.SequenceToFSharpList(Flatten(list, ref amt)));
        }
    }

    [NodeName("Flatten")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Flatten nested lists into one list.")]
    public class dynFlattenListAmt : dynNodeWithOneOutput
    {
        public dynFlattenListAmt()
        {
            InPortData.Add(new PortData("list", "The list of lists to flatten.", typeof(Value.List)));
            InPortData.Add(new PortData("amt", "Amount of nesting to remove.", typeof(Value.Number)));

            OutPortData.Add(new PortData("list", "The flattened list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to flatten.");

            IEnumerable<Value> list = ((Value.List)args[0]).Item;

            var oldAmt = Convert.ToInt32(((Value.Number)args[1]).Item);

            if (oldAmt < 0)
                throw new Exception("Cannot flatten a list by a negative amount.");

            var amt = oldAmt;
            var result = dynFlattenList.Flatten(list, ref amt);

            if (amt > 0)
                throw new Exception("List not nested enough to flatten by given amount. Nesting Amt = " + (oldAmt - amt) + ", Given Amt = " + oldAmt);

            return Value.NewList(Utils.SequenceToFSharpList(result));
        }
    }

    #endregion

    #region Boolean

    public abstract class dynComparison : dynBuiltinFunction
    {
        protected dynComparison(FSharpFunc<FSharpList<Value>, Value> op, string name)
            : base(op)
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x" + name + "y", "comp", typeof(Value.Number)));

            RegisterAllPorts();
        }

    }

    [NodeName("Less Than")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("less", "than", "<")]
    public class dynLessThan : dynComparison
    {
        public dynLessThan() : base(FScheme.LT, "<") { }
    }

    [NodeName("Less Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("<=")]
    public class dynLessThanEquals : dynComparison
    {
        public dynLessThanEquals() : base(FScheme.LTE, "≤") { }
    }

    [NodeName("Greater Than")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">")]
    public class dynGreaterThan : dynComparison
    {
        public dynGreaterThan() : base(FScheme.GT, ">") { }
    }

    [NodeName("Greater Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">=", "Greater Than Or Equal")]
    public class dynGreaterThanEquals : dynComparison
    {
        public dynGreaterThanEquals() : base(FScheme.GTE, "≥") { }
    }

    [NodeName("Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    public class dynEqual : dynComparison
    {
        public dynEqual() : base(FScheme.EQ, "=") { }
    }

    [NodeName("And")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean AND: Returns true only if both of the inputs are true. If either is false, returns false.")]
    public class dynAnd : dynNodeModel
    {
        public dynAnd()
        {
            InPortData.Add(new PortData("a", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("b", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("a∧b", "result", typeof(Value.Number)));
            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result)) 
                return result[outPort];

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", Inputs[0].Item2.Build(preBuilt, Inputs[0].Item1));
                ifNode.ConnectInput("true", Inputs[1].Item2.Build(preBuilt, Inputs[1].Item1));
                ifNode.ConnectInput("false", new NumberNode(0));
                result = new Dictionary<int, INode>();
                result[outPort] = ifNode;
            }
            else
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", new SymbolNode(InPortData[0].NickName));
                ifNode.ConnectInput("true", new SymbolNode(InPortData[1].NickName));
                ifNode.ConnectInput("false", new NumberNode(0));

                var node = new AnonymousFunctionNode(
                    InPortData.Select(x => x.NickName),
                    ifNode);

                //For each index in InPortData
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (var data in Enumerable.Range(0, InPortData.Count).Where(HasInput))
                {
                    //Compile input and connect it
                    node.ConnectInput(
                        InPortData[data].NickName,
                        Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1));
                }

                RequiresRecalc = false;
                OnEvaluate(); //TODO: insert call into actual ast using a begin

                result = new Dictionary<int, INode>();
                result[outPort] = node;
            }
            preBuilt[this] = result;
            return result[outPort];
        }
    }

    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean OR: Returns true if either of the inputs are true. If neither are true, returns false.")]
    public class dynOr : dynNodeModel
    {
        public dynOr()
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a∨b", "result", typeof(bool)));
            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (preBuilt.TryGetValue(this, out result)) 
                return result[outPort];

            if (Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", Inputs[0].Item2.Build(preBuilt, Inputs[0].Item1));
                ifNode.ConnectInput("true", new NumberNode(1));
                ifNode.ConnectInput("false", Inputs[1].Item2.Build(preBuilt, Inputs[1].Item1));

                result = new Dictionary<int, INode>();
                result[outPort] = ifNode;
            }
            else
            {
                var ifNode = new ConditionalNode();
                ifNode.ConnectInput("test", new SymbolNode(InPortData[0].NickName));
                ifNode.ConnectInput("true", new NumberNode(1));
                ifNode.ConnectInput("false", new SymbolNode(InPortData[1].NickName));

                var node = new AnonymousFunctionNode(
                    InPortData.Select(x => x.NickName),
                    ifNode);

                //For each index in InPortData
                //for (int i = 0; i < InPortData.Count; i++)
                foreach (var data in Enumerable.Range(0, InPortData.Count).Where(HasInput))
                {
                    //Compile input and connect it
                    node.ConnectInput(
                        InPortData[data].NickName,
                        Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1));
                }

                RequiresRecalc = false;
                OnEvaluate(); //TODO: insert call into actual ast using a begin

                result = new Dictionary<int, INode>();
                result[outPort] = node;
            }
            preBuilt[this] = result;
            return result[outPort];
        }
    }

    [NodeName("Xor")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean XOR: Returns true if one input is true and the other is false. If both inputs are the same, returns false.")]
    public class dynXor : dynBuiltinFunction
    {
        public dynXor()
            : base(FScheme.Xor)
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a⊻b", "result", typeof(bool)));
            RegisterAllPorts();
        }
    }

    [NodeName("Not")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean NOT: Inverts a boolean value. (True -> False, False -> True)")]
    [NodeSearchTags("invert")]
    public class dynNot : dynBuiltinFunction
    {
        public dynNot()
            : base(FScheme.Not)
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            OutPortData.Add(new PortData("!a", "result", typeof(bool)));
            RegisterAllPorts();
        }

    }

    #endregion

    #region Math

    public abstract class dynMathBase : dynNodeWithOneOutput
    {
        protected dynMathBase()
        {
            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Add")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Adds two numbers.")]
    [NodeSearchTags("plus", "sum", "+")]
    public class dynAddition : dynMathBase
    {
        public dynAddition()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x+y", "sum", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x + y);
        }

    }

    [NodeName("Subtract")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Subtracts two numbers.")]
    [NodeSearchTags("minus", "difference", "-")]
    public class dynSubtraction : dynMathBase
    {
        public dynSubtraction()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x-y", "difference", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x - y);
        }
    }

    [NodeName("Multiply")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Multiplies two numbers.")]
    [NodeSearchTags("times", "product", "*")]
    public class dynMultiplication : dynMathBase
    {
        public dynMultiplication()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x∙y", "product", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x * y);
        }

    }

    [NodeName("Divide")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Divides two numbers.")]
    [NodeSearchTags("division", "quotient", "/")]
    public class dynDivision : dynMathBase
    {
        public dynDivision()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x÷y", "result", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x / y);
        }

    }

    [NodeName("Modulo")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Remainder of division of two numbers.")]
    [NodeSearchTags("%", "remainder")]
    public class dynModulo : dynMathBase
    {
        public dynModulo()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x%y", "result", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(x % y);
        }
    }

    [NodeName("Power")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Raises a number to the power of another.")]
    [NodeSearchTags("pow", "exponentiation", "^")]
    public class dynPow : dynMathBase
    {
        public dynPow()
        {
            InPortData.Add(new PortData("x", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("x^y", "result", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var x = ((Value.Number)args[0]).Item;
            var y = ((Value.Number)args[1]).Item;

            return Value.NewNumber(Math.Pow(x,y));
        }
    }

    [NodeName("Round")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Rounds a number to the nearest integer value.")]
    public class dynRound : dynMathBase
    {
        public dynRound()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Rounded number", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Round(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Floor")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Rounds a number to the nearest smaller integer.")]
    [NodeSearchTags("round")]
    public class dynFloor : dynMathBase
    {
        public dynFloor()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Number rounded down", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Floor(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Ceiling")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Rounds a number to the nearest larger integer value.")]
    [NodeSearchTags("round")]
    public class dynCeiling : dynMathBase
    {
        public dynCeiling()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("int", "Number rounded up", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(
               Math.Ceiling(((Value.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Random With Seed")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    public class dynRandomSeed : dynNodeWithOneOutput
    {
        public dynRandomSeed()
        {
            InPortData.Add(new PortData("num", "A number to function as a seed", typeof(Value.Number)));
            OutPortData.Add(new PortData("rand", "Random number between 0.0 and 1.0.", typeof(Value.Number)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        private static Random random = new Random();
        public override Value Evaluate(FSharpList<Value> args)
        {
            random = new Random((int) ( (Value.Number) args[0] ).Item );
            return Value.NewNumber(random.NextDouble());
        }
    }

    [NodeName("Random")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    public class dynRandom : dynNodeWithOneOutput
    {
        public dynRandom()
        {
            OutPortData.Add(new PortData("rand", "Random number between 0.0 and 1.0.", typeof(Value.Number)));
            RegisterAllPorts();
        }

        private static Random random = new Random();

        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(random.NextDouble());
        }
    }

    [NodeName("e")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("e (base of natural logarithm) constant")]
    [NodeSearchTags("statistics", "natural", "logarithm")]
    [IsInteractive(false)]
    public class dynEConstant : dynNodeModel
    {
        public dynEConstant()
        {
            OutPortData.Add(new PortData("2.71828...", "e", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(Math.E);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Pi")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Pi constant")]
    [NodeSearchTags("trigonometry", "circle", "π")]
    [IsInteractive(false)]
    public class dynPi : dynNodeModel
    {
        public dynPi()
        {
            OutPortData.Add(new PortData("3.14159...", "pi", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(3.14159265358979);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("2*Pi")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Pi constant")]
    [NodeSearchTags("trigonometry", "circle", "π")]
    [IsInteractive(false)]
    public class dyn2Pi : dynNodeModel
    {
        public dyn2Pi()
        {
            OutPortData.Add(new PortData("3.14159...*2", "2*pi", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new NumberNode(3.14159265358979 * 2);
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Sine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Computes the sine of the given angle.")]
    public class dynSin : dynMathBase
    {
        public dynSin()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("sin(θ)", "Sine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Sin(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Sin(theta));
            }
        }
    }

    [NodeName("Cosine")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Computes the cosine of the given angle.")]
    public class dynCos : dynMathBase
    {
        public dynCos()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("cos(θ)", "Cosine value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Cos(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Cos(theta));
            }
        }
    }

    [NodeName("Tangent")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Computes the tangent of the given angle.")]
    public class dynTan : dynMathBase
    {
        public dynTan()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(Value.Number)));
            OutPortData.Add(new PortData("tan(θ)", "Tangent value of the given angle", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Value.NewList(
                   FSchemeInterop.Utils.SequenceToFSharpList(
                      ((Value.List)input).Item.Select(
                         x =>
                            Value.NewNumber(Math.Tan(((Value.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Value.Number)input).Item;
                return Value.NewNumber(Math.Tan(theta));
            }
        }
    }

    [NodeName("Average")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Averages a list of numbers.")]
    [NodeSearchTags("avg")]
    public class dynAverage : dynMathBase
    {
        public dynAverage()
        {
            InPortData.Add(new PortData("numbers", "The list of numbers to average.", typeof(Value.List)));
            OutPortData.Add(new PortData("avg", "average", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list of numbers is required to average.");

            var vals = ((Value.List)args[0]).Item.Select(
               x => ((Value.Number)x).Item
            );

            var average = vals.Average();

            return Value.NewNumber(average);
        }
    }

    /// <summary>
    /// keeps a simple moving average to smooth out noisy values over time. 
    /// https://en.wikipedia.org/wiki/Moving_average
    /// 
    /// </summary>
    [NodeName("Smooth")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Smooths a list of numbers using a running average.")]
    [NodeSearchTags("running average", "moving average", "sma")]
    public class dynSmooth : dynMathBase
    {
        Queue<Value.Number> values = new Queue<Value.Number>();
        int maxNumValues = 10;

        public dynSmooth()
        {
            InPortData.Add(new PortData("val", "The current value.", typeof(Value.Container)));
            OutPortData.Add(new PortData("avg", "uses a simple moving average to smooth out values that fluctuate over time", typeof(Value.Number)));
            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsNumber)
                throw new Exception("A number is required to smooth.");

            if (values.Count() < maxNumValues)
                values.Enqueue((Value.Number)args[0]); // add current values to queue until it fills up
            else
                values.Dequeue();//throw out the first value once we are up to the full queue amount

            var average = values.Average(num => num.Item);

            return Value.NewNumber(average);
        }
    }

    #endregion

    #region Control Flow

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Perform All")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Evaluates all inputs in order, and returns result of the last input.")]
    [NodeSearchTags("begin")]
    public class dynBegin : dynVariableInput
    {
        public dynBegin()
        {
            InPortData.Add(new PortData("expr1", "Expression #1", typeof(object)));
            InPortData.Add(new PortData("expr2", "Expression #2", typeof(object)));
            OutPortData.Add(new PortData("last", "Result of final expression", typeof(object)));

            RegisterAllPorts();
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override string GetInputRootName()
        {
            return "expr";
        }

        protected override string GetTooltipRootName()
        {
            return "Expression #";
        }

        protected override int GetInputNameIndex()
        {
            return InPortData.Count + 1;
        }

        private INode nestedBegins(Stack<Tuple<int, dynNodeModel>> inputs, Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt)
        {
            var popped = inputs.Pop();
            var firstVal = popped.Item2.Build(preBuilt, popped.Item1);

            if (inputs.Any())
            {
                var newBegin = new BeginNode(new List<string>() { "expr1", "expr2" });
                newBegin.ConnectInput("expr1", nestedBegins(inputs, preBuilt));
                newBegin.ConnectInput("expr2", firstVal);
                return newBegin;
            }
            else
                return firstVal;
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("All inputs must be connected.");
                throw new Exception("Begin Node requires all inputs to be connected.");
            }
            
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>(); 
                result[outPort] = 
                    nestedBegins(
                        new Stack<Tuple<int, dynNodeModel>>(
                            Enumerable.Range(0, InPortData.Count).Select(x => Inputs[x])),
                    preBuilt);
                preBuilt[this] = result;
            }
            return result[outPort];
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Apply Function to List")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to a list of arguments.")]
    public class dynApplyList : dynNodeWithOneOutput
    {
        public dynApplyList()
        {
            InPortData.Add(new PortData("func", "Function", typeof(Value.Function)));
            InPortData.Add(new PortData("args", "List of arguments to apply function to.", typeof(Value.List)));

            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var fArgs = ((Value.List)args[1]).Item;

            return f.Invoke(fArgs);
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Apply Function")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to arguments.")]
    public class dynApply1 : dynVariableInput
    {
        public dynApply1()
        {
            InPortData.Add(new PortData("func", "Function", typeof(object)));
            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "arg";
        }

        protected override string GetTooltipRootName()
        {
            return "Argument #";
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var f = ((Value.Function)args[0]).Item;
            var fArgs = args.Tail;

            return f.Invoke(fArgs);
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(1))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Input")
                {
                    var attr = subNode.Attributes["name"].Value;

                    if (!attr.Equals("func"))
                        InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Conditional statement")]
    public class dynConditional : dynNodeModel
    {
        public dynConditional()
        {
            InPortData.Add(new PortData("test", "Test block", typeof(bool)));
            InPortData.Add(new PortData("true", "True block", typeof(object), Value.NewDummy("Empty true")));
            InPortData.Add(new PortData("false", "False block", typeof(object), Value.NewDummy("Empty false")));
            OutPortData.Add(new PortData("result", "Result", typeof(object)));

            RegisterAllPorts();

            foreach (var port in InPorts.Skip(1))
            {
                port.DefaultValueEnabled = false;
            }
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!HasInput(0))
            {
                Error("Test input must be connected.");
                throw new Exception("If Node requires test input to be connected.");
            }
            return base.Build(preBuilt, outPort);
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return new ConditionalNode(portNames);
        }
    }
    
    [NodeName("Debug Breakpoint")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Halts execution until user clicks button.")]
    public partial class dynBreakpoint : dynNodeWithOneOutput
    {
        public dynBreakpoint()
        {
            InPortData.Add(new PortData("", "Object to inspect", typeof(object)));
            OutPortData.Add(new PortData("", "Object inspected", typeof(object)));
            RegisterAllPorts();
        }

        private bool Enabled { get; set; }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = args[0];

            DynamoLogger.Instance.Log(FScheme.print(result));

            if (Controller.DynamoViewModel.RunInDebug)
            {
                Enabled = true;
                Select();
                Controller.DynamoViewModel.ShowElement(this);

                while (Enabled)
                {
                    Thread.Sleep(1);
                }
            }

            return result;
        }
    }

    #endregion

    #region Interactive Primitive Types

    #region Base Classes

    [IsInteractive(true)]
    public abstract partial class dynBasicInteractive<T> : dynNodeWithOneOutput
    {
        private T _value;
        public virtual T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == null || !_value.Equals(value))
                {
                    _value = value;
                    //DynamoLogger.Instance.Log("Value changed to: " + _value);
                    RequiresRecalc = value != null;
                    RaisePropertyChanged("Value");
                }
            }
        }

        protected abstract T DeserializeValue(string val);

        protected dynBasicInteractive()
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name, type));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", Value.ToString());
            dynEl.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(T).FullName))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }
    }

    public abstract class dynDouble : dynBasicInteractive<double>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value);
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }
    }

    public abstract class dynBool : dynBasicInteractive<bool>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewNumber(Value ? 1 : 0);
        }
    }

    public abstract partial class dynString : dynBasicInteractive<string>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewString(Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }
    }

    #endregion

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Creates a number.")]
    public partial class dynDoubleInput : dynNodeWithOneOutput
    {
        public dynDoubleInput()
        {
            OutPortData.Add(new PortData("", "", typeof(Value.Number)));

            RegisterAllPorts();
        }

        private List<IDoubleSequence> _parsed;
        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != null && _value.Equals(value)) 
                    return;

                _value = value;

                var idList = new List<string>();

                try
                {
                    _parsed = ParseValue(idList);

                    InPortData.Clear();

                    foreach (var id in idList)
                    {
                        InPortData.Add(new PortData(id, "variable", typeof (Value.Number)));
                    }

                    RegisterInputs();

                    ArgumentLacing = InPortData.Any() ? LacingStrategy.Longest : LacingStrategy.Disabled;
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }

                RequiresRecalc = value != null;
                RaisePropertyChanged("Value");
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value);
            dynEl.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes.Cast<XmlNode>().Where(subNode => subNode.Name.Equals(typeof(double).FullName)))
            {
                Value = subNode.Attributes[0].Value;
            }
        }

        private List<IDoubleSequence> ParseValue(List<string> identifiers)
        {
            var idSet = new HashSet<string>(identifiers);
            return Value.Replace(" ", "").Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Select(
                delegate(string x)
                {
                    var rangeIdentifiers = x.Split(
                        dynSublists.RangeSeparatorTokens,
                        StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                    if (rangeIdentifiers.Length > 3)
                        throw new Exception("Bad range syntax: not of format \"start..end[..(increment|#count)]\"");

                    if (rangeIdentifiers.Length == 0)
                        throw new Exception("No identifiers found.");

                    IDoubleInputToken startToken = ParseToken(rangeIdentifiers[0], idSet, identifiers);

                    if (rangeIdentifiers.Length > 1)
                    {
                        if (rangeIdentifiers[1].StartsWith("#"))
                        {
                            var countToken = rangeIdentifiers[1].Substring(1);
                            IDoubleInputToken endToken = ParseToken(countToken, idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#") || rangeIdentifiers[2].StartsWith("~"))
                                    throw new Exception("Cannot use range or approx. identifier on increment field when one has already been used to specify a count.");
                                return new Sequence(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken);
                            }

                            return new Sequence(startToken, new DoubleToken(1), endToken) as IDoubleSequence;
                        }
                        else
                        {
                            IDoubleInputToken endToken = ParseToken(rangeIdentifiers[1], idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#"))
                                {
                                    var count = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken countToken = ParseToken(count, idSet, identifiers);

                                    return new CountRange(startToken, countToken, endToken);
                                }

                                if (rangeIdentifiers[2].StartsWith("~"))
                                {
                                    var approx = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken approxToken = ParseToken(approx, idSet, identifiers);

                                    return new ApproxRange(startToken, approxToken, endToken);
                                }

                                return new Range(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken);
                            }
                            return new Range(startToken, new DoubleToken(1), endToken) as IDoubleSequence;
                        }

                    }

                    return new OneNumber(startToken) as IDoubleSequence;
                }).ToList();
        }

        private static IDoubleInputToken ParseToken(string id, HashSet<string> identifiers, List<string> list)
        {
            double dbl;
            if (double.TryParse(id, NumberStyles.Any, CultureInfo.CurrentCulture, out dbl))
                return new DoubleToken(dbl);

            var match = dynSublists.IdentifierPattern.Match(id);
            if (match.Success)
            {
                var tokenId = match.Groups["id"].Value;
                if (!identifiers.Contains(tokenId))
                {
                    identifiers.Add(tokenId);
                    list.Add(tokenId);
                }
                return new IdentifierToken(tokenId);
            }

            throw new Exception("Bad identifier syntax: \"" + id + "\"");
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var paramDict = InPortData.Select(x => x.NickName)
                .Zip(args, Tuple.Create)
                .ToDictionary(x => x.Item1, x => ((Value.Number)x.Item2).Item);

            return _parsed.Count == 1
                ? _parsed[0].GetValue(paramDict)
                : FScheme.Value.NewList(Utils.SequenceToFSharpList(_parsed.Select(x => x.GetValue(paramDict))));
        }

        interface IDoubleSequence
        {
            Value GetValue(Dictionary<string, double> idLookup);
        }

        private class OneNumber : IDoubleSequence
        {
            private readonly IDoubleInputToken _token;

            private readonly Value _result;

            public OneNumber(IDoubleInputToken t)
            {
                _token = t;

                if (_token is DoubleToken)
                    _result = GetValue(new Dictionary<string, double>());
            }

            public Value GetValue(Dictionary<string, double> idLookup)
            {
                return _result ?? (FScheme.Value.NewNumber(_token.GetValue(idLookup)));
            }
        }

        private class Sequence : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _count;

            private readonly Value _result;

            public Sequence(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken count)
            {
                _start = start;
                _step = step;
                _count = count;

                if (_start is DoubleToken && _step is DoubleToken && _count is DoubleToken)
                {
                    _result = GetValue(new Dictionary<string, double>());
                }
            }

            public Value GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _step.GetValue(idLookup);

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _start.GetValue(idLookup);
                    var count = (int)_count.GetValue(idLookup);

                    if (count < 0)
                    {
                        count *= -1;
                        start += step*(count-1);
                        step *= -1;
                    }

                    return FScheme.Value.NewList(Utils.SequenceToFSharpList(CreateSequence(start, step, count)));
                }
                return _result;
            }

            private static IEnumerable<Value> CreateSequence(double start, double step, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return FScheme.Value.NewNumber(start);
                    start += step;
                }
            }
        }

        private class Range : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _end;

            private readonly Value _result;

            public Range(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end)
            {
                _start = start;
                _step = step;
                _end = end;

                if (_start is DoubleToken && _step is DoubleToken && _end is DoubleToken)
                {
                    _result = GetValue(new Dictionary<string, double>());
                }
            }

            public Value GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _step.GetValue(idLookup);

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _start.GetValue(idLookup);
                    var end = _end.GetValue(idLookup);

                    return Process(start, step, end);
                }
                return _result;
            }

            protected virtual Value Process(double start, double step, double end)
            {
                if (step < 0)
                {
                    step *= -1;
                    var tmp = end;
                    end = start;
                    start = tmp;
                }

                var countingUp = start < end;

                var range = countingUp ? FScheme.Range(start, step, end) : FScheme.Range(end, step, start).Reverse();

                return FScheme.Value.NewList(Utils.SequenceToFSharpList(range.Select(FScheme.Value.NewNumber)));
            }
        }

        private class CountRange : Range
        {
            public CountRange(IDoubleInputToken startToken, IDoubleInputToken countToken, IDoubleInputToken endToken)
                : base(startToken, countToken, endToken)
            { }

            protected override Value Process(double start, double count, double end)
            {
                var c = (int)count;

                var neg = c < 0;

                c = Math.Abs(c) - 1;

                if (neg)
                    c *= -1;

                return base.Process(start, Math.Abs(start - end) / c, end);
            }
        }

        private class ApproxRange : Range
        {
            public ApproxRange(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end) 
                : base(start, step, end)
            { }

            protected override Value Process(double start, double approx, double end)
            {
                var neg = approx < 0;

                var a = Math.Abs(approx);

                var dist = end - start;
                var stepnum = 1;
                if (dist != 0)
                {
                    var ceil = (int)Math.Ceiling(dist/a);
                    var floor = (int)Math.Floor(dist/a);

                    if (ceil != 0 && floor != 0)
                    {
                        var ceilApprox = Math.Abs(dist/ceil - a);
                        var floorApprox = Math.Abs(dist/floor - a);
                        stepnum = ceilApprox < floorApprox ? ceil : floor;
                    }
                }

                if (neg)
                    stepnum *= -1;

                return base.Process(start, Math.Abs(dist) / stepnum, end);
            }
        }

        interface IDoubleInputToken
        {
            double GetValue(Dictionary<string, double> idLookup);
        }

        private struct IdentifierToken : IDoubleInputToken
        {
            private readonly string _id;

            public IdentifierToken(string id)
            {
                _id = id;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return idLookup[_id];
            }
        }

        private struct DoubleToken : IDoubleInputToken
        {
            private readonly double _d;

            public DoubleToken(double d)
            {
                _d = d;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return _d;
            }
        }
 
    }

    [NodeName("Angle(deg.)")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("An angle in degrees.")]
    public partial class dynAngleInput : dynDouble
    {
        public dynAngleInput()
        {
            RegisterAllPorts();
        }

        public override double Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (base.Value == value)
                    return;

                base.Value = value;
                //RaisePropertyChanged("Value");
            }
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val);
            }
            catch
            {
                return 0;
            }
        }

    }

    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Change a number value with a slider.")]
    public partial class dynDoubleSliderInput : dynDouble
    {
        //Slider tb_slider;
        //dynTextBox mintb;
        //dynTextBox maxtb;
        //dynTextBox valtb;

        private double max;
        private double min;

        public dynDoubleSliderInput()
        {
            RegisterAllPorts();
            
            Min = 0.0;
            Max = 100.0;
            Value = 50.0;
        }

        public override double Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                //RaisePropertyChanged("Value"); //already called in base --SJE

                Debug.WriteLine(string.Format("Min:{0},Max:{1},Value:{2}", Min.ToString(CultureInfo.InvariantCulture), Max.ToString(CultureInfo.InvariantCulture), Value.ToString(CultureInfo.InvariantCulture)));
            }
        }
        
        public double Max
        {
            get { return max; }
            set
            {
                max = value;

                if (max < Value)
                    Value = max;

                RaisePropertyChanged("Max");
            }
        }

        public double Min
        {
            get { return min; }
            set
            {
                min = value;

                if (min > Value)
                    Value = min;

                RaisePropertyChanged("Min");
            } 
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(double).FullName))
                {
                    double value = Value;
                    double min = Min;
                    double max = Max;

                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                            value = DeserializeValue(attr.Value);
                        else if (attr.Name.Equals("min"))
                        {
                            min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }

                    Min = min;
                    Max = max;
                    Value = value;
                }
            }
        }
    }

    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Selection between a true and false.")]
    [NodeSearchTags("true", "truth", "false")]
    public partial class dynBoolSelector : dynBool
    {
        public dynBoolSelector()
        {
            RegisterAllPorts();
        }

        protected override bool DeserializeValue(string val)
        {
            try
            {
                return val.ToLower().Equals("true");
            }
            catch
            {
                return false;
            }
        }

    }

    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Creates a string.")]
    public partial class dynStringInput : dynString
    {
        //dynTextBox tb;

        public override string Value
        {
            get
            {
                return HttpUtility.UrlDecode(base.Value);
            }
            set
            {
                base.Value = value;
            }
        }

        public dynStringInput()
        {
            RegisterAllPorts();
            Value = "";
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(attr.Value);
                        }
                    }
                }
            }
        }
    }

    [NodeName("Directory")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    public partial class dynStringDirectory : dynStringFilename
    {
        protected override string DeserializeValue(string val)
        {
            if (Directory.Exists(val))
            {
                return val;
            }
            else
            {
                return "";
            }
        }

    }

    [NodeName("File Path")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    public partial class dynStringFilename : dynBasicInteractive<string>
    {
        //TextBox tb;

        public dynStringFilename()
        {
            RegisterAllPorts();
        }

        protected override string DeserializeValue(string val)
        {
            if (File.Exists(val))
            {
                return val;
            }
            else
            {
                return "";
            }
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (string.IsNullOrEmpty(Value))
                throw new Exception("No file selected.");

            return FScheme.Value.NewString(Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }
    }

    #endregion

    #region Strings and Conversions

    [NodeName("Concat Strings")]
    [NodeDescription("Concatenates two or more strings")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynConcatStrings : dynVariableInput
    {
        public dynConcatStrings()
        {
            InPortData.Add(new PortData("s1", "String #1", typeof(Value.String)));
            InPortData.Add(new PortData("s2", "String #2", typeof(Value.String)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.String)));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "s";
        }

        protected override string GetTooltipRootName()
        {
            return "String #";
        }

        protected override int GetInputNameIndex()
        {
            return InPortData.Count + 1;
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(2))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewString(string.Concat(args.Cast<Value.String>().Select(x => x.Item)));
        }
    }

    [NodeName("String to Number")]
    [NodeDescription("Converts a string to a number")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynString2Num : dynBuiltinFunction
    {
        public dynString2Num()
            : base(FScheme.StringToNum)
        {
            InPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            OutPortData.Add(new PortData("n", "A number", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("Number to String")]
    [NodeDescription("Converts a number to a string")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynNum2String : dynBuiltinFunction
    {
        public dynNum2String()
            : base(FScheme.NumToString)
        {
            InPortData.Add(new PortData("n", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("s", "A string", typeof(Value.String)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }
    }

    [NodeName("String Length")]
    [NodeDescription("Calculates the length of a string.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynStringLen : dynNodeWithOneOutput
    {
        public dynStringLen()
        {
            InPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            OutPortData.Add(new PortData("len(s)", "Length of given string", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewNumber(((Value.String)args[0]).Item.Length);
        }
    }

    [NodeName("To String")]
    [NodeDescription("Converts anything into it's string representation")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynToString : dynNodeWithOneOutput
    {
        public dynToString()
        {
            InPortData.Add(new PortData("input", "Anything", typeof(Value.Number))); // proxy for any type
            OutPortData.Add(new PortData("string", "The string representation of the input", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewString(dynNodeViewModel.BuildValueString(args[0],0,10000,0, 25));
        }
    }

    [NodeName("Split String")]
    [NodeDescription("Splits given string around given delimiter into a list of sub strings.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynSplitString : dynNodeWithOneOutput
    {
        public dynSplitString()
        {
            InPortData.Add(new PortData("str", "String to split", typeof(Value.String)));
            InPortData.Add(new PortData("del", "Delimiter", typeof(Value.String)));
            OutPortData.Add(new PortData("strs", "List of split strings", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string str = ((Value.String)args[0]).Item;
            string del = ((Value.String)args[1]).Item;

            return Value.NewList(
                Utils.SequenceToFSharpList(
                    str.Split(new string[] { del }, StringSplitOptions.None)
                       .Select(Value.NewString)
                )
            );
        }
    }

    [NodeName("Join Strings")]
    [NodeDescription("Joins the given list of strings around the given delimiter.")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynJoinStrings : dynNodeWithOneOutput
    {
        public dynJoinStrings()
        {
            InPortData.Add(new PortData("strs", "List of strings to join.", typeof(Value.List)));
            InPortData.Add(new PortData("del", "Delimier", typeof(Value.String), Value.NewString("")));
            OutPortData.Add(new PortData("str", "Joined string", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var strs = ((Value.List)args[0]).Item;
            var del = ((Value.String)args[1]).Item;

            return Value.NewString(
                string.Join(del, strs.Select(x => ((Value.String)x).Item))
            );
        }
    }

    [NodeName("String Case")]
    [NodeDescription("Converts a string to uppercase or lowercase")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynStringCase : dynNodeWithOneOutput
    {
        public dynStringCase()
        {
            InPortData.Add(new PortData("str", "String to convert", typeof(Value.String)));
            InPortData.Add(new PortData("upper?", "True = Uppercase, False = Lowercase", typeof(Value.Number)));
            OutPortData.Add(new PortData("s", "Converted string", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string s = ((Value.String)args[0]).Item;
            bool upper = ((Value.Number)args[1]).Item == 1.0;

            return Value.NewString(
                upper ? s.ToUpper() : s.ToLower()
            );
        }
    }

    [NodeName("Substring")]
    [NodeDescription("Gets a substring of a given string")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynSubstring : dynNodeWithOneOutput
    {
        public dynSubstring()
        {
            InPortData.Add(new PortData("str", "String to take substring from", typeof(Value.String)));
            InPortData.Add(new PortData("start", "Starting index of substring", typeof(Value.Number)));
            InPortData.Add(new PortData("length", "Length of substring", typeof(Value.Number)));
            OutPortData.Add(new PortData("sub", "Substring", typeof(Value.String)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            string s = ((Value.String)args[0]).Item;
            double start = ((Value.Number)args[1]).Item;
            double length = ((Value.Number)args[2]).Item;

            return Value.NewString(s.Substring((int)start, (int)length));
        }
    }

    #endregion

    /// <summary>
    /// A class used to store a name and associated item for a drop down menu
    /// </summary>
    public class DynamoDropDownItem
    {
        public string Name { get; set; }
        public object Item { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public DynamoDropDownItem(string name, object item)
        {
            Name = name;
            Item = item;
        }
    }

    /// <summary>
    /// Base class for all nodes using a drop down
    /// </summary>
    public abstract partial class dynDropDrownBase : dynNodeWithOneOutput
    {
        private ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                if (value > Items.Count - 1)
                {
                    selectedIndex = -1;
                }
                else
                    selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("index", SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode elNode)
        {
            try
            {
                SelectedIndex = Convert.ToInt32(elNode.Attributes["index"].Value);
            }
            catch { }
        }

        public virtual void PopulateItems()
        {
            //override in child classes
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItemsHash is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateItems();
        }

        /// <summary>
        /// The base behavior for the drop down node is to return the item at the selected index in the Items collection.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewContainer(Items[SelectedIndex].Item);
        }
    }
}

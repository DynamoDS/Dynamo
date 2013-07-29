//Copyright 2013 Ian Keough

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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using System.Web;

using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Value = Dynamo.FScheme.Value;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using ComboBox = System.Windows.Controls.ComboBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

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
        public string Symbol { get; protected internal set; }

        internal dynBuiltinFunction(string symbol)
        {
            Symbol = symbol;
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
            {
                return base.Compile(portNames);
            }
            else
                return new FunctionNode(Symbol, portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {

            var val = ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol(Symbol))
                .Item.Invoke(args);

            var symbol = ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol(Symbol)).Item;

            return val;
        }
    }

    #endregion

    public abstract class dynVariableInput : dynNodeWithOneOutput
    {
        protected dynVariableInput()
        {
            
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            System.Windows.Controls.Button addButton = new dynNodeButton();
            addButton.Content = "+";
            addButton.Width = 20;
            //addButton.Height = 20;
            addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            addButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            System.Windows.Controls.Button subButton = new dynNodeButton();
            subButton.Content = "-";
            subButton.Width = 20;
            //subButton.Height = 20;
            subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            subButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            
            WrapPanel wp = new WrapPanel();
            wp.VerticalAlignment = VerticalAlignment.Top;
            wp.HorizontalAlignment = HorizontalAlignment.Center;
            wp.Children.Add(addButton);
            wp.Children.Add(subButton);

            nodeUI.inputGrid.Children.Add(wp);

            //nodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //nodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //nodeUI.inputGrid.Children.Add(addButton);
            //System.Windows.Controls.Grid.SetColumn(addButton, 0);

            //nodeUI.inputGrid.Children.Add(subButton);
            //System.Windows.Controls.Grid.SetColumn(subButton, 1);

            addButton.Click += delegate { AddInput(); RegisterAllPorts(); };
            subButton.Click += delegate { RemoveInput(); RegisterAllPorts(); };
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        private int lastEvaledAmt;
        public override bool RequiresRecalc
        {
            get
            {
                return lastEvaledAmt != InPortData.Count || base.RequiresRecalc;
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadNode(XmlNode elNode)
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
            lastEvaledAmt = InPortData.Count;
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
            : base("reverse")
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
            : base("sort-with")
        {
            InPortData.Add(new PortData("c(x, y)", "Comparitor", typeof(object)));
            InPortData.Add(new PortData("list", "List to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Sort-By")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Returns a sorted list, using the given key mapper.")]
    public class dynSortBy : dynBuiltinFunction
    {
        public dynSortBy()
            : base("sort-by")
        {
            InPortData.Add(new PortData("c(x)", "Key Mapper", typeof(object)));
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
            : base("sort")
        {
            InPortData.Add(new PortData("list", "List of numbers or strings to sort", typeof(Value.List)));
            OutPortData.Add(new PortData("sorted", "Sorted list", typeof(Value.List)));

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
            : base("foldl")
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
            : base("filter")
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
            : base("build-list")
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
            var start = (int)((Value.Number)args[0]).Item;
            var amount = (int)((Value.Number)args[1]).Item;
            var step = (int)((Value.Number)args[2]).Item;

            return Value.NewList(Utils.SequenceToFSharpList(MakeSequence(start, amount, step)));
        }

        private IEnumerable<Value> MakeSequence(int start, int amount, int step)
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        public override void LoadNode(XmlNode elNode)
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

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
            {
                return base.Compile(portNames);
            }
            else
                return new FunctionNode("map", portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol("map"))
                .Item.Invoke(args);
        }
    }

    [NodeName("Cartesian Product")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [NodeSearchTags("cross")]
    public class dynCartProd : dynVariableInput
    {
        public dynCartProd()
        {
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        public override void LoadNode(XmlNode elNode)
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

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
            {
                return base.Compile(portNames);
            }
            else
                return new FunctionNode("cartesian-product", portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol("cartesian-product"))
                .Item.Invoke(args);
        }
    }

    [NodeName("Map")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Maps a sequence")]
    public class dynMap : dynBuiltinFunction
    {
        public dynMap()
            : base("map")
        {
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to map over.", typeof(Value.List)));
            OutPortData.Add(new PortData("mapped", "Mapped sequence", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("True For All")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Tests to see if all elements in a sequence satisfy the given predicate.")]
    public class dynAndMap : dynBuiltinFunction
    {
        public dynAndMap()
            : base("andmap")
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
            : base("ormap")
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
    [NodeDescription("ADds an element to the beginning of a list.")]
    public class dynList : dynBuiltinFunction
    {
        public dynList()
            : base("cons")
        {
            InPortData.Add(new PortData("item", "The new Head of the list", typeof(object)));
            InPortData.Add(new PortData("list", "The new Tail of the list", typeof(object)));
            OutPortData.Add(new PortData("list", "Result List", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Take From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Takes elements from a list")]
    public class dynTakeList : dynBuiltinFunction
    {
        public dynTakeList()
            : base("take")
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
            : base("drop")
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
            : base("get")
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract the element from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted element", typeof(object)));

            RegisterAllPorts();
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

    [NodeName("Remove Every Nth")]
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

            return Value.NewList(Utils.SequenceToFSharpList(lst.Where((_, i) => i % n != 0)));
        }
    }

    [NodeName("Empty List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("An empty list")]
    [IsInteractive(false)]
    public class dynEmpty : dynNodeWithOneOutput
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
            : base("empty?")
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
            : base("len")
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("length", "Length of the list", typeof(object)));

            RegisterAllPorts();
        }
    }

    [NodeName("Append to List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Appends two list")]
    public class dynAppend : dynBuiltinFunction
    {
        public dynAppend()
            : base("append")
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
            : base("first")
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
            : base("rest")
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
        public dynTranspose() : base("transpose")
        {
            InPortData.Add(new PortData("lists", "The list of lists to transpose.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "Transposed list of lists.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Build Sublists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Build sublists from a list using a list-building syntax.")]
    public class dynSublists : dynBasicInteractive<string>
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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new dynTextBox
            {
                Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += processTextForNewInputs;

            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Top;

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                //Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            if (Value != "")
                tb.Commit();
        }

        public override void LoadNode(XmlNode elNode)
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
        protected dynComparison(string op) : this(op, op) { }

        protected dynComparison(string op, string name)
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
        public dynLessThan() : base("<") { }
    }

    [NodeName("Less Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("<=")]
    public class dynLessThanEquals : dynComparison
    {
        public dynLessThanEquals() : base("<=", "≤") { }
    }

    [NodeName("Greater Than")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">")]
    public class dynGreaterThan : dynComparison
    {
        public dynGreaterThan() : base(">") { }
    }

    [NodeName("Greater Than Or Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">=", "Greater Than Or Equal")]
    public class dynGreaterThanEquals : dynComparison
    {
        public dynGreaterThanEquals() : base(">=", "≥") { }
    }

    [NodeName("Equal")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    public class dynEqual : dynComparison
    {
        public dynEqual() : base("=") { }
    }

    [NodeName("And")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean AND.")]
    public class dynAnd : dynBuiltinFunction
    {
        public dynAnd()
            : base("and")
        {
            InPortData.Add(new PortData("a", "operand", typeof(Value.Number)));
            InPortData.Add(new PortData("b", "operand", typeof(Value.Number)));
            OutPortData.Add(new PortData("a∧b", "result", typeof(Value.Number)));
            RegisterAllPorts();
        }


        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
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
                    foreach (var data in Enumerable.Range(0, InPortData.Count))
                    {
                        //Fetch the corresponding port
                        //var port = InPorts[i];

                        //If this port has connectors...
                        //if (port.Connectors.Any())
                        if (HasInput(data))
                        {
                            //Compile input and connect it
                            node.ConnectInput(
                               InPortData[data].NickName,
                               Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1)
                            );
                        }
                    }

                    RequiresRecalc = false;
                    OnEvaluate();

                    result = new Dictionary<int, INode>();
                    result[outPort] = node;
                }
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean OR.")]
    public class dynOr : dynBuiltinFunction
    {
        public dynOr()
            : base("or")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a∨b", "result", typeof(bool)));
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {

        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
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
                    foreach (var data in Enumerable.Range(0, InPortData.Count))
                    {
                        //Fetch the corresponding port
                        //var port = InPorts[i];

                        //If this port has connectors...
                        //if (port.Connectors.Any())
                        if (HasInput(data))
                        {
                            //Compile input and connect it
                            node.ConnectInput(
                               InPortData[data].NickName,
                               Inputs[data].Item2.Build(preBuilt, Inputs[data].Item1)
                            );
                        }
                    }

                    RequiresRecalc = false;
                    OnEvaluate();

                    result = new Dictionary<int, INode>();
                    result[outPort] = node;
                }
                preBuilt[this] = result;
            }
            return result[outPort];
        }
    }

    [NodeName("Xor")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean XOR.")]
    public class dynXor : dynBuiltinFunction
    {
        public dynXor()
            : base("xor")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData.Add(new PortData("a⊻b", "result", typeof(bool)));
            RegisterAllPorts();
        }
    }

    [NodeName("Not")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_CONDITIONAL)]
    [NodeDescription("Boolean NOT.")]
    public class dynNot : dynBuiltinFunction
    {
        public dynNot()
            : base("not")
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(1))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadNode(XmlNode elNode)
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

    [NodeName("Average")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Averages a list of numbers.")]
    [NodeSearchTags("avg")]
    public class dynAverage : dynMathBase 
    {

        List<Value.Number> values = new List<Value.Number>();

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

            IEnumerable<double> vals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
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
    public class dynSmooth: dynMathBase
    {

        Queue<Value.Number> values = new Queue<Value.Number>();
        int maxNumValues = 10;
        int currentNumValues = 0;


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
            {
                values.Enqueue((Value.Number)args[0]); // add current values to queue until it fills up
            }
            else
            {
                values.Dequeue();//throw out the first value once we are up to the full queue amount
            }
            

            var average = values.Average(num => num.Item);
            

            return Value.NewNumber(average);
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
            InPortData.Add(new PortData("true", "True block", typeof(object)));
            InPortData.Add(new PortData("false", "False block", typeof(object)));
            OutPortData.Add(new PortData("result", "Result", typeof(object)));
            RegisterAllPorts();
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("All inputs must be connected.");
                throw new Exception("If Node requires all inputs to be connected.");
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
    public class dynBreakpoint : dynNodeWithOneOutput
    {
        System.Windows.Controls.Button button;

        public dynBreakpoint()
        {
            InPortData.Add(new PortData("", "Object to inspect", typeof(object)));
            OutPortData.Add(new PortData("", "Object inspected", typeof(object)));
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            button = new dynNodeButton();
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            button.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            //inputGrid.RowDefinitions.Add(new RowDefinition());
            nodeUI.inputGrid.Children.Add(button);
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetRow(button, 0);
            button.Content = "Continue";

            Enabled = false;

            button.Click += new RoutedEventHandler(button_Click);

            var bindingVal = new System.Windows.Data.Binding("Enabled")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this
            };
            button.SetBinding(UIElement.IsEnabledProperty, bindingVal);
        }

        private bool Enabled { get; set; }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Deselect();
            Enabled = false;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = args[0];

            Controller.DynamoViewModel.Log(FScheme.print(result));

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

    public class dynTextBox : TextBox
    {
        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255,255,255));
        private static Brush highlighted = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255));

        public dynTextBox()
        {
            //turn off the border
            Background = clear;
            BorderThickness = new Thickness(1);
            GotFocus += OnGotFocus;
            LostFocus += OnLostFocus;
            LostKeyboardFocus += OnLostFocus;
        }

        private void OnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = clear;
        }

        private void OnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            Background = highlighted;
        }

        private bool numeric;
        public bool IsNumeric
        {
            get { return numeric; }
            set
            {
                numeric = value;
                if (value && Text.Length > 0)
                {
                    Text = dynSettings.RemoveChars(
                       Text,
                       Text.ToCharArray()
                          .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                          .Select(c => c.ToString())
                    );
                }
            }
        }

        private bool pending;
        public bool Pending
        {
            get { return pending; }
            set
            {
                if (value)
                {
                    FontStyle = FontStyles.Italic;
                }
                else
                {
                    FontStyle = FontStyles.Normal;
                }
                pending = value;
            }
        }

        public void Commit()
        {
            var expr = GetBindingExpression(TextProperty);
            if (expr != null)
                expr.UpdateSource();

            if (OnChangeCommitted != null)
            {
                OnChangeCommitted();
            }
            Pending = false;

            //dynSettings.Bench.mainGrid.Focus();
        }

        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                Commit();
            }
        }

        private bool shouldCommit()
        {
            return !dynSettings.Controller.DynamoViewModel.DynamicRunEnabled;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            Pending = true;

            if (IsNumeric)
            {
                var p = CaretIndex;

                //base.Text = dynSettings.RemoveChars(
                //   Text,
                //   Text.ToCharArray()
                //      .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                //      .Select(c => c.ToString())
                //);

                CaretIndex = p;
            }
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                dynSettings.ReturnFocusToSearch();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            Commit();
        }
    }

    public class dynStringTextBox : dynTextBox
    {

        public dynStringTextBox()
        {
            Commit();
            Pending = false;
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Return || e.Key == Key.Enter)
            //{
            //    dynSettings.ReturnFocusToSearch();
            //}
        }

    }


    [IsInteractive(true)]
    public abstract class dynBasicInteractive<T> : dynNodeWithOneOutput
    {
        private T _value = default(T);
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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit...",
                IsCheckable = false
            };

            nodeUI.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", Value.ToString());
            dynEl.AppendChild(outEl);
        }

        public override void LoadNode(XmlNode elNode)
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
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

    public abstract class dynString : dynBasicInteractive<string>
    {
        public override Value Evaluate(FSharpList<Value> args)
        {
            return FScheme.Value.NewString(Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {

            dynEditWindow editWindow = new dynEditWindow();
            
            editWindow.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            editWindow.editText.SetBinding(TextBox.TextProperty, bindingVal);

            if (editWindow.ShowDialog() != true)
            {
                return;
            }
        }
    }

    #endregion

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Creates a number.")]
    public class dynDoubleInput : dynNodeWithOneOutput
    {
        public dynDoubleInput()
        {
            OutPortData.Add(new PortData("", "", typeof(Value.Number)));

            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new dynStringTextBox
            {
                AcceptsReturn = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                IsNumeric = true,
                Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            nodeUI.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleInputDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            tb.Text = Value ?? "0.0";
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
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }

                RequiresRecalc = value != null;
                RaisePropertyChanged("Value");
            }
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value);
            dynEl.AppendChild(outEl);
        }

        public override void LoadNode(XmlNode elNode)
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
                                return new Sequence(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken);
                            }

                            return new Sequence(startToken, new DoubleToken(1), endToken) as IDoubleSequence;
                        }
                        else
                        {
                            IDoubleInputToken endToken = ParseToken(rangeIdentifiers[1], idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
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
            if (double.TryParse(id, out dbl))
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

            private Value _result;

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

            private Value _result;

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

            private Value _result;

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

                    if (step < 0)
                    {
                        step *= -1;
                        var tmp = end;
                        end = start;
                        start = tmp;
                    }

                    var countingUp = start < end;

                    return FScheme.Value.NewList(Utils.SequenceToFSharpList(
                        countingUp ? CreateRange(start, step, end) : CreateRange(end, step, start).Reverse()));
                }
                return _result;
            }

            private static IEnumerable<Value> CreateRange(double start, double step, double end)
            {
                for (var i = start; i <= end; i += step)
                    yield return FScheme.Value.NewNumber(i);
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

        private class DoubleInputDisplay : DoubleDisplay
        {
            public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                double dbl;
                if (double.TryParse(value as string, out dbl))
                {
                    return base.Convert(dbl, targetType, parameter, culture);
                }
                return value ?? "";
            }

            public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }
    }

    [NodeName("Angle(deg.)")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("An angle in degrees.")]
    public class dynAngleInput : dynDouble
    {
        public dynAngleInput()
        {
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = true;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new RadianToDegreesConverter(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            //tb.Text = "0.0";
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
    public class dynDoubleSliderInput : dynDouble
    {
        Slider tb_slider;
        dynTextBox mintb;
        dynTextBox maxtb;
        dynTextBox valtb;

        private double max;
        private double min;

        public dynDoubleSliderInput()
        {
            RegisterAllPorts();
            
            Min = 0.0;
            Max = 100.0;
            Value = 50.0;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            tb_slider = new Slider();
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            
            tb_slider.MinWidth = 150;

            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None;

            tb_slider.PreviewMouseUp += delegate
            {
                dynSettings.ReturnFocusToSearch();
            };

            mintb = new dynTextBox();
            mintb.Width = double.NaN;

            mintb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            // input value textbox
            valtb = new dynTextBox();
            valtb.Width = double.NaN;
            valtb.Margin = new Thickness(0,0,10,0);

            maxtb = new dynTextBox();
            maxtb.Width = double.NaN;

            maxtb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            var sliderGrid = new Grid();
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)});
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sliderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            sliderGrid.Children.Add(valtb);
            sliderGrid.Children.Add(mintb);
            sliderGrid.Children.Add(tb_slider);
            sliderGrid.Children.Add(maxtb);

            Grid.SetColumn(valtb, 0);
            Grid.SetColumn(mintb, 1);
            Grid.SetColumn(tb_slider, 2);
            Grid.SetColumn(maxtb, 3);
            nodeUI.inputGrid.Children.Add(sliderGrid);

            maxtb.DataContext = this;
            tb_slider.DataContext = this;
            mintb.DataContext = this;
            valtb.DataContext = this;

            // value input
            var inputBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay()
            };
            valtb.SetBinding(dynTextBox.TextProperty, inputBinding);

            // slider value 
            var sliderBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            };
            tb_slider.SetBinding(Slider.ValueProperty, sliderBinding);

            // max value
            var bindingMax = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            maxtb.SetBinding(dynTextBox.TextProperty, bindingMax);

            // max slider value
            var bindingMaxSlider = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MaximumProperty, bindingMaxSlider);


            // min value
            var bindingMin = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            mintb.SetBinding(dynTextBox.TextProperty, bindingMin);

            // min slider value
            var bindingMinSlider = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.OneWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MinimumProperty, bindingMinSlider);
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
                RaisePropertyChanged("Value");

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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        public override void LoadNode(XmlNode elNode)
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
    public class dynBoolSelector : dynBool
    {
        System.Windows.Controls.RadioButton rbTrue;
        System.Windows.Controls.RadioButton rbFalse;

        public dynBoolSelector()
        {
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            rbTrue = new System.Windows.Controls.RadioButton();
            rbFalse = new System.Windows.Controls.RadioButton();
            rbTrue.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            rbFalse.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            //use a unique name for the button group
            //so other instances of this element don't get confused
            string groupName = Guid.NewGuid().ToString();
            rbTrue.GroupName = groupName;
            rbFalse.GroupName = groupName;

            rbTrue.Content = "1";
            rbTrue.Padding = new Thickness(5, 0, 12, 0);
            rbFalse.Content = "0";
            rbFalse.Padding = new Thickness(5,0,0,0);

            var wp = new WrapPanel {HorizontalAlignment = HorizontalAlignment.Center};
            wp.Children.Add(rbTrue);
            wp.Children.Add(rbFalse);
            nodeUI.inputGrid.Children.Add(wp);

            //rbFalse.IsChecked = true;
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);

            rbFalse.DataContext = this;
            rbTrue.DataContext = this;

            var rbTrueBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            rbTrue.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbTrueBinding);

            var rbFalseBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new InverseBoolDisplay()
            };
            rbFalse.SetBinding(System.Windows.Controls.RadioButton.IsCheckedProperty, rbFalseBinding);
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

        void rbFalse_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Value = false;
            dynSettings.ReturnFocusToSearch();
        }

        void rbTrue_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            //Value = true;
            dynSettings.ReturnFocusToSearch();
        }
    }

    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Creates a string.")]
    public class dynStringInput : dynString
    {
        dynTextBox tb;

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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            //add a text box to the input grid of the control
            tb = new dynStringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        public override void LoadNode(XmlNode elNode)
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

    public class dynNodeButton : System.Windows.Controls.Button
    {
        public dynNodeButton() : base()
        {
            var dict = new ResourceDictionary();
            var uri = new Uri("/DynamoElements;component/Themes/DynamoModern.xaml", UriKind.Relative);
            dict.Source = uri;
            Style = (Style)dict["SNodeTextButton"];

            this.Margin = new Thickness(1,0,1,0);
        }

    }

    [NodeName("Directory")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Allows you to select a directory on the system to get its path.")]
    public class dynStringDirectory : dynStringFilename
    {
        protected override void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.SelectedPath;
            }
        }

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
    public class dynStringFilename : dynBasicInteractive<string>
    {
        TextBox tb;

        public dynStringFilename()
        {
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a button to the inputGrid on the dynElement
            var readFileButton = new dynNodeButton();
            
            //readFileButton.Margin = new System.Windows.Thickness(4);
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new TextBox();
            if(string.IsNullOrEmpty(Value))
                Value = "No file selected.";

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate { tb.ScrollToHorizontalOffset(double.PositiveInfinity); dynSettings.ReturnFocusToSearch(); };

            StackPanel sp = new StackPanel();
            sp.Children.Add(readFileButton);
            sp.Children.Add(tb);
            nodeUI.inputGrid.Children.Add(sp);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplay()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
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

        protected virtual void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = false
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                Value = openDialog.FileName;
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

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(2))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadNode(XmlNode elNode)
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
            : base("string->num")
        {
            InPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            OutPortData.Add(new PortData("n", "A number", typeof(Value.Number)));

            RegisterAllPorts();
        }
    }

    [NodeName("Number to String")]
    [NodeDescription("Converts a number to a string")]
    [NodeCategory(BuiltinNodeCategories.CORE_STRINGS)]
    public class dynNum2String : dynBuiltinFunction
    {
        public dynNum2String()
            : base("num->string")
        {
            InPortData.Add(new PortData("n", "A number", typeof(Value.Number)));
            OutPortData.Add(new PortData("s", "A string", typeof(Value.String)));
            RegisterAllPorts();
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
            InPortData.Add(new PortData("del", "Delimier", typeof(Value.String)));
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

    #region Value Conversion
    [ValueConversion(typeof(double), typeof(String))]
    public class DoubleDisplay : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            string val = ((double) value).ToString("0.000",CultureInfo.CurrentCulture);
            Debug.WriteLine("Converting {0} -> {1}", value, val);
            return value == null ? "" : val;

        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            //return value.ToString();

            double val = 0.0;
            double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out val);
            Debug.WriteLine("Converting {0} -> {1}", value, val);
            return val;
        }
    }


    public class RadianToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double degrees = System.Convert.ToDouble(value, culture) * 180.0 / Math.PI;
            return degrees;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double radians = System.Convert.ToDouble(value, culture) * Math.PI / 180.0;
            return radians;
        }
    }

    public class StringDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source -> target
            return value==null?"": HttpUtility.UrlDecode(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target -> source
            return HttpUtility.UrlEncode(value.ToString());
        }
    }

    public class FilePathDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //source->target

            var maxChars = 30;
            //var str = value.ToString();
            var str = HttpUtility.UrlDecode(value.ToString());

            if (string.IsNullOrEmpty(str))
            {
                return "No file selected.";
            }
            else if (str.Length > maxChars)
            {
                return str.Substring(0, 10 ) + "..." + str.Substring(str.Length - maxChars+10, maxChars-10);
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //target->source
            return HttpUtility.UrlEncode(value.ToString());
        }
    }

    public class InverseBoolDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool)value;
            }
            return value;
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
    public abstract class dynDropDrownBase : dynNodeWithOneOutput
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

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            base.SetupCustomUIElements(nodeUI);

            //add a drop down list to the window
            var combo = new ComboBox
                {
                    Width = 300,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
            nodeUI.inputGrid.Children.Add(combo);
            System.Windows.Controls.Grid.SetColumn(combo, 0);
            System.Windows.Controls.Grid.SetRow(combo, 0);

            combo.DropDownOpened += new EventHandler(combo_DropDownOpened);
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    RequiresRecalc = true;
            };

            combo.DataContext = this;
            //bind this combo box to the selected item hash

            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new System.Windows.Data.Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = this
            };
            combo.SetBinding(ComboBox.SelectedIndexProperty, indexBinding);
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("index", SelectedIndex.ToString());
        }

        public override void LoadNode(XmlNode elNode)
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

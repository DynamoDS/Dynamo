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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using Binding = System.Windows.Forms.Binding;
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

        protected abstract string getInputRootName();
        protected virtual int getNewInputIndex()
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
            InPortData.Add(new PortData(getInputRootName() + getNewInputIndex(), "", typeof(object)));
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadElement(XmlNode elNode)
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
            InPortData.Add(new PortData("index0", "First item", typeof(object)));
            OutPortData.Add(new PortData("list", "A list", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string getInputRootName()
        {
            return "index";
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
    [NodeDescription("Filters a sequence by a given predicate")]
    public class dynFilter : dynBuiltinFunction
    {
        public dynFilter()
            : base("filter")
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(Value.List)));
            OutPortData.Add(new PortData("filtered", "Filtered Sequence", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Number Sequence")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Creates a sequence of numbers")]
    [NodeSearchTags("range")]
    public class dynBuildSeq : dynBuiltinFunction
    {
        public dynBuildSeq()
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

    [NodeName("Combine")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    [NodeSearchTags("zip")]
    public class dynCombine : dynVariableInput
    {
        public dynCombine()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "First list", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "Second list", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
            this.ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string getInputRootName()
        {
            return "list";
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

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            var inputAttr = elNode.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(getInputRootName() + getNewInputIndex(), "", typeof(object)));
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
            InPortData.Add(new PortData("list1", "First list", typeof(Value.List)));
            InPortData.Add(new PortData("list2", "Second list", typeof(Value.List)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.List)));

            RegisterAllPorts();
        }

        protected override string getInputRootName()
        {
            return "list";
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

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("inputs", (InPortData.Count - 1).ToString());
        }

        public override void LoadElement(XmlNode elNode)
        {
            var inputAttr = elNode.Attributes["inputs"];
            int inputs = inputAttr == null ? 2 : Convert.ToInt32(inputAttr.Value);
            if (inputs == 1)
                RemoveInput();
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(getInputRootName() + getNewInputIndex(), "", typeof(object)));
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

    [NodeName("Split Pair")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Deconstructs a list pair.")]
    public class dynDeCons : dynNodeModel
    {
        public dynDeCons()
        {
            InPortData.Add(new PortData("list", "", typeof(Value.List)));
            OutPortData.Add(new PortData("first", "", typeof(object)));
            OutPortData.Add(new PortData("rest", "", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var list = (Value.List)args[0];

            outPuts[OutPortData[0]] = list.Item.Head;
            outPuts[OutPortData[1]] = Value.NewList(list.Item.Tail);
        }
    }

    [NodeName("Make Pair")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Constructs a list pair.")]
    public class dynList : dynBuiltinFunction
    {
        public dynList()
            : base("cons")
        {
            InPortData.Add(new PortData("first", "The new Head of the list", typeof(object)));
            InPortData.Add(new PortData("rest", "The new Tail of the list", typeof(object)));
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

    [NodeName("Get From List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets an element from a list at a specified index.")]
    public class dynGetFromList : dynBuiltinFunction
    {
        public dynGetFromList()
            : base("get")
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(Value.List)));
            OutPortData.Add(new PortData("element", "Extracted element", typeof(object)));

            RegisterAllPorts();
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

        public override Value Evaluate(FSharpList<Value> args)
        {
            return Value.NewList(FSharpList<Value>.Empty);
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

    [NodeName("First in List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets the first element of a list")]
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

    [NodeName("List Rest")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Gets the list with the first element removed.")]
    public class dynRest : dynBuiltinFunction
    {
        public dynRest()
            : base("rest")
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            OutPortData.Add(new PortData("rest", "List without the first element.", typeof(Value.List)));

            RegisterAllPorts();
        }
    }

    [NodeName("Slice List")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Create a lists of lists with each sub-list containing n elements.")]
    public class dynSlice : dynNodeWithOneOutput
    {
        public dynSlice()
        {
            InPortData.Add(new PortData("list", "A list", typeof(Value.List)));
            InPortData.Add(new PortData("n", "The number of elements in each sub-list.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "The list of lists.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if(!args[0].IsList)
                throw new Exception("A list is required to slice.");

            FSharpList<Value> lst = ((Value.List)args[0]).Item;
            double n = (double)((Value.Number)args[1]).Item;

            //if we have less elements in the 
            //incoming list than the slice size,
            //just return the list
            if (lst.Count<Value>() < n)
            {
                return Value.NewList(lst);
            }

            List<Value> finalList = new List<Value>();
            List<Value> currList = new List<Value>();
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

            if (currList.Count<Value>() > 0)
            {
                finalList.Add(Value.NewList(Utils.MakeFSharpList(currList.ToArray())));
            }

            return Value.NewList(Utils.MakeFSharpList<Value>(finalList.ToArray()));

        }
    }

    [NodeName("Transpose Lists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Swaps rows and columns in a list of lists.")]
    public class dynTranspose : dynNodeWithOneOutput
    {
        public dynTranspose()
        {
            InPortData.Add(new PortData("lists", "The list of lists to transpose.", typeof(Value.List)));
            OutPortData.Add(new PortData("", "Transposed list of lists.", typeof(Value.List)));
            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var lists = ((Value.List)args[0]).Item;

            return FScheme.Map(
                FSharpList<Value>.Cons(
                    Value.NewFunction(
                        FSharpFunc<FSharpList<Value>, Value>.FromConverter(Value.NewList)),
                    lists));
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
            InPortData.Add(new PortData("shift", "The shift to apply to the sub-list.", typeof(Value.List)));

            OutPortData.RemoveAt(0); //remove the existing blank output
            OutPortData.Add(new PortData("list", "The sublists.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            var tb = new dynTextBox
            {
                Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF))
            };

            tb.OnChangeCommitted += processTextForNewInputs;

            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;

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

        public override void LoadElement(XmlNode elNode)
        {
            base.LoadElement(elNode);
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

        static readonly Regex IdentifierPattern = new Regex(@"(?<id>[a-zA-Z_][^ ]*)|{(?<id>\w(?:[^}\\]|(?:\\}))*)}");
        static readonly string[] RangeSeparatorTokens = new[] { "..", "-", ":" };

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

    [NodeName("Flatten")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS)]
    [NodeDescription("Flatten a list of lists into one list.")]
    public class dynFlattenList : dynNodeWithOneOutput
    {
        public dynFlattenList()
        {
            InPortData.Add(new PortData("list", "The list of lists to flatten.", typeof(Value.List)));
            OutPortData.Add(new PortData("list", "The flattened list.", typeof(Value.List)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            if (!args[0].IsList)
                throw new Exception("A list is required to flatten.");

            FSharpList<Value> list = ((Value.List)args[0]).Item;
            var vals = list.ToList().SelectMany(x => ((Value.List)x).Item);

            return Value.NewList(Utils.SequenceToFSharpList(vals));
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
                result[outPort] = new NumberNode(Math.PI);
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
    [NodeDescription("Executes Values in a sequence")]
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

        protected override string getInputRootName()
        {
            return "expr";
        }

        protected override int getNewInputIndex()
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

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Apply")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies arguments to a function")]
    public class dynApply1 : dynVariableInput
    {
        public dynApply1()
        {
            InPortData.Add(new PortData("func", "Procedure", typeof(object)));
            OutPortData.Add(new PortData("result", "Result", typeof(object)));

            RegisterAllPorts();
        }

        protected override string getInputRootName()
        {
            return "arg";
        }

        protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("All inputs must be connected.");
                throw new Exception("Apply Node requires all inputs to be connected.");
            }
            return base.Build(preBuilt, outPort);
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return new ApplierNode(portNames);
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(1))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadElement(XmlNode elNode)
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

            enabled = false;

            button.Click += new RoutedEventHandler(button_Click);
        }

        private bool _enabled;
        private bool enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                button.IsEnabled = value;
            }
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Deselect();
            enabled = false;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var result = args[0];

            Controller.DynamoViewModel.Log(FScheme.print(result));

            if (Controller.DynamoViewModel.RunInDebug)
            {
                enabled = true;
                Select();
                Controller.DynamoViewModel.ShowElement(this);

                while (enabled)
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
            var expr = GetBindingExpression(TextBox.TextProperty);
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
                //base.Text = value;
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

                base.Text = dynSettings.RemoveChars(
                   Text,
                   Text.ToCharArray()
                      .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                      .Select(c => c.ToString())
                );

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

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", Value.ToString());
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
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

        public override void editWindowItem_Click(object sender, RoutedEventArgs e)
        {

            dynEditWindow editWindow = new dynEditWindow();

            //set the text of the edit window to begin
            editWindow.editText.Text = base.Value.ToString();

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            Value = DeserializeValue(editWindow.editText.Text);
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
        public override string Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = EscapeString(value);
            }
        }

        // Taken from:
        // http://stackoverflow.com/questions/6378681/how-can-i-use-net-style-escape-sequences-in-runtime-values
        private static string EscapeString(string s)
        {
            if (s == null)
                return "";

            Contract.Requires(s != null);
            Contract.Ensures(Contract.Result<string>() != null);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\')
                {
                    i++;
                    if (i == s.Length)
                        throw new ArgumentException("Escape sequence starting at end of string", s);
                    switch (s[i])
                    {
                        case '\\':
                            sb.Append('\\');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        //TODO: ADD MORE CASES HERE
                    }
                }
                else
                    sb.Append(s[i]);
            }
            return sb.ToString();
        }

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

            //set the text of the edit window to begin
            editWindow.editText.Text = base.Value.ToString();

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            Value = DeserializeValue(editWindow.editText.Text);
        }
    }

    #endregion

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
    [NodeDescription("Creates a number.")]
    public class dynDoubleInput : dynDouble
    {

        public dynDoubleInput()
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
                Converter = new DoubleDisplay(),
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);

            tb.Text = "0.0";
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

            tb.Text = "0.0";
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
        TextBox displayBox;
        private double max;
        private double min;

        public dynDoubleSliderInput()
        {
            RegisterAllPorts();
            Value = 50.0;
            Min = 0.0;
            Max = 100.0;
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a slider control to the input grid of the control
            tb_slider = new System.Windows.Controls.Slider();
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb_slider.Width = 100;

            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.None;

            tb_slider.ValueChanged += delegate
            {
                var pos = Mouse.GetPosition(nodeUI.elementCanvas);
                Canvas.SetLeft(displayBox, pos.X);
                Canvas.SetTop(displayBox, Height);
            };

            tb_slider.PreviewMouseDown += delegate
            {
                if (nodeUI.IsEnabled && !nodeUI.elementCanvas.Children.Contains(displayBox))
                {
                    nodeUI.elementCanvas.Children.Add(displayBox);
                    var pos = Mouse.GetPosition(nodeUI.elementCanvas);
                    Canvas.SetLeft(displayBox, pos.X);
                }
            };

            tb_slider.PreviewMouseUp += delegate
            {
                if (nodeUI.elementCanvas.Children.Contains(displayBox))
                    nodeUI.elementCanvas.Children.Remove(displayBox);

                dynSettings.ReturnFocusToSearch();
            };

            mintb = new dynTextBox();
            mintb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            mintb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            mintb.Width = double.NaN;
            mintb.IsNumeric = true;

            mintb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            mintb.OnChangeCommitted += delegate
            {
                try
                {
                    Min = Convert.ToDouble(mintb.Text);
                }
                catch
                {
                    Min = 0;
                }
            };
            //mintb.Pending = false;

            maxtb = new dynTextBox();
            maxtb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            maxtb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            maxtb.Width = double.NaN;
            maxtb.IsNumeric = true;

            maxtb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            maxtb.OnChangeCommitted += delegate
            {
                try
                {
                    Max = Convert.ToDouble(maxtb.Text, CultureInfo.InvariantCulture);
                }
                catch
                {
                    Max = 100;
                }
            };

            WrapPanel wp = new WrapPanel();
            wp.Children.Add(mintb);
            wp.Children.Add(tb_slider);
            wp.Children.Add(maxtb);
            nodeUI.inputGrid.Children.Add(wp);

            displayBox = new TextBox()
            {
                IsReadOnly = true,
                Background = Brushes.White,
                Foreground = Brushes.Black
            };

            Canvas.SetTop(displayBox, nodeUI.Height);
            Canvas.SetZIndex(displayBox, int.MaxValue);

            displayBox.DataContext = this;
            maxtb.DataContext = this;
            tb_slider.DataContext = this;
            mintb.DataContext = this;

            var bindingValue = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
            };
            displayBox.SetBinding(TextBox.TextProperty, bindingValue);

            var sliderBinding = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            tb_slider.SetBinding(Slider.ValueProperty, sliderBinding);

            var bindingMax = new System.Windows.Data.Binding("Max")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MaximumProperty, bindingMax);
            maxtb.SetBinding(dynTextBox.TextProperty, bindingMax);

            var bindingMin = new System.Windows.Data.Binding("Min")
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb_slider.SetBinding(Slider.MinimumProperty, bindingMin);
            mintb.SetBinding(dynTextBox.TextProperty, bindingMin);
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
            }
        }
        
        public double Max
        {
            get { return max; }
            set
            {
                max = value;
                RaisePropertyChanged("Max");
            }
        }

        public double Min
        {
            get { return min; }
            set
            {
                min = value;
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

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("min", Min.ToString(CultureInfo.InvariantCulture));
            outEl.SetAttribute("max", Max.ToString(CultureInfo.InvariantCulture));
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(double).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                            Value = DeserializeValue(attr.Value);
                        else if (attr.Name.Equals("min"))
                        {
                            Min = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            Max = Convert.ToDouble(attr.Value, CultureInfo.InvariantCulture);
                        }
                    }
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

            RowDefinition rd = new RowDefinition();
            ColumnDefinition cd1 = new ColumnDefinition();
            ColumnDefinition cd2 = new ColumnDefinition();
            nodeUI.inputGrid.ColumnDefinitions.Add(cd1);
            nodeUI.inputGrid.ColumnDefinitions.Add(cd2);
            nodeUI.inputGrid.RowDefinitions.Add(rd);

            nodeUI.inputGrid.Children.Add(rbTrue);
            nodeUI.inputGrid.Children.Add(rbFalse);

            System.Windows.Controls.Grid.SetColumn(rbTrue, 0);
            System.Windows.Controls.Grid.SetRow(rbTrue, 0);
            System.Windows.Controls.Grid.SetColumn(rbFalse, 1);
            System.Windows.Controls.Grid.SetRow(rbFalse, 0);

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
        //TextBlock tb;

        public dynStringInput()
        {
            RegisterAllPorts();
            Value = "";
        }

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            //add a text box to the input grid of the control
            tb = new dynStringTextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 120,
                VerticalAlignment = VerticalAlignment.Top
            };

            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                //Converter = new StringDisplay(),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        public override string Value
        {
            get { return base.Value; }
            set
            {
                if (base.Value == value)
                    return;

                base.Value = value;
            }
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", System.Web.HttpUtility.UrlEncode(Value.ToString()));
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(System.Web.HttpUtility.UrlDecode(attr.Value));
                            //tb.Text = Utilities.Ellipsis(Value, 30);
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

    [NodeName("Filename")]
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

            //NodeUI.SetRowAmount(2);
            //nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());
            //nodeUI.inputGrid.RowDefinitions.Add(new RowDefinition());

            //nodeUI.inputGrid.Children.Add(tb);
            //nodeUI.inputGrid.Children.Add(readFileButton);

            //System.Windows.Controls.Grid.SetRow(readFileButton, 0);
            //System.Windows.Controls.Grid.SetRow(tb, 1);

            //NodeUI.topControl.Height = 60;
            //NodeUI.UpdateLayout();

            tb.DataContext = this;
            var bindingVal = new System.Windows.Data.Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new FilePathDisplay()
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        public override string Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;

                //tb.Text = string.IsNullOrEmpty(Value)
                //   ? "No file selected."
                //   : Value;
            }
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

        void readFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();

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
            InPortData.Add(new PortData("s1", "First string", typeof(Value.String)));
            InPortData.Add(new PortData("s2", "Second string", typeof(Value.String)));
            OutPortData.Add(new PortData("combined", "Combined lists", typeof(Value.String)));

            RegisterAllPorts();
        }

        protected override string getInputRootName()
        {
            return "s";
        }

        protected override int getNewInputIndex()
        {
            return InPortData.Count + 1;
        }

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 2)
                base.RemoveInput();
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(2))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                dynEl.AppendChild(input);
            }
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Input")
                {
                    var attr = subNode.Attributes["name"].Value;

                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            RegisterAllPorts();
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            if (SaveResult)
                return base.Compile(portNames);
            else
                return new FunctionNode("concat-strings", portNames);
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            return ((Value.Function)Controller.FSchemeEnvironment.LookupSymbol("concat-strings"))
                .Item.Invoke(args);
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value==null?"":((double)value).ToString("0.0000", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

    public class RadianToDegreesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double radians = System.Convert.ToDouble(value, culture) * 180.0 / Math.PI;
            return radians;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double degrees = System.Convert.ToDouble(value, culture) * Math.PI / 180.0;
            return degrees;
        }
    }

    public class StringDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value==null?"": value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilePathDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var maxChars = 30;
            var str = value.ToString();

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
            return null;
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
}

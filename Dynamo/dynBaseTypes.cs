//Copyright 2012 Ian Keough

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
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;

using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;

namespace Dynamo.Nodes
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static class BuiltinNodeCategories
    {
        public const string MATH = "Math";
        public const string COMPARISON = "Comparison";
        public const string BOOLEAN = "Logic";
        public const string PRIMITIVES = "Primitives";
        public const string REVIT = "Revit";
        public const string MISC = "Miscellaneous";
        public const string LIST = "Lists";
    }

    #region FScheme Builtin Interop

    public abstract class dynBuiltinFunction : dynNode
    {
        public string Symbol;

        internal dynBuiltinFunction(string symbol)
        {
            this.Symbol = symbol;
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            if (this.SaveResult)
            {
                return new ExternalMacroNode(
                   new ExternMacro(this.macroEval),
                   portNames
                );
            }
            else
                return new FunctionNode(this.Symbol, portNames);
        }

        private Expression macroEval(FSharpList<Expression> args, ExecutionEnvironment environment)
        {
            if (this.RequiresRecalc || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.OnEvaluate();
            return this.oldValue;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var fun = ((Expression.Function)this.Bench.Environment
               .LookupSymbol(this.Symbol)).Item;

            return fun
               .Invoke(ExecutionEnvironment.IDENT)
               .Invoke(
                  Utils.convertSequence(args.Select(
                     this.macroEnvironment.Evaluate
                  ))
               );
        }
    }

    public abstract class dynBuiltinMacro : dynBuiltinFunction
    {
        internal dynBuiltinMacro(string symbol) : base(symbol) { }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var macro = ((Expression.Special)this.Bench.Environment
               .LookupSymbol(this.Symbol)).Item;

            return macro
               .Invoke(ExecutionEnvironment.IDENT)
               .Invoke(this.macroEnvironment.Env)
               .Invoke(args);
        }
    }

    #endregion

    public abstract class dynVariableInput : dynNode
    {
        protected dynVariableInput()
        {
            System.Windows.Controls.Button addButton = new System.Windows.Controls.Button();
            addButton.Content = "+";
            addButton.Width = 20;
            addButton.Height = 20;
            addButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            addButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            System.Windows.Controls.Button subButton = new System.Windows.Controls.Button();
            subButton.Content = "-";
            subButton.Width = 20;
            subButton.Height = 20;
            subButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            subButton.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            NodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            NodeUI.inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            NodeUI.inputGrid.Children.Add(addButton);
            System.Windows.Controls.Grid.SetColumn(addButton, 0);

            NodeUI.inputGrid.Children.Add(subButton);
            System.Windows.Controls.Grid.SetColumn(subButton, 1);

            addButton.Click += new RoutedEventHandler(AddInput);
            subButton.Click += new RoutedEventHandler(RemoveInput);
        }

        protected abstract string getInputRootName();
        protected virtual int getNewInputIndex()
        {
            return this.InPortData.Count;
        }

        private int lastEvaledAmt;
        public override bool RequiresRecalc
        {
            get
            {
                return lastEvaledAmt != this.InPortData.Count || base.RequiresRecalc;
            }
            set
            {
                base.RequiresRecalc = value;
            }
        }

        protected virtual void RemoveInput(object sender, RoutedEventArgs args)
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
                NodeUI.ReregisterInputs();
            }
        }

        protected virtual void AddInput(object sender, RoutedEventArgs args)
        {
            InPortData.Add(new PortData(this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
            NodeUI.ReregisterInputs();
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
                    this.InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            NodeUI.ReregisterInputs();
        }

        protected override void OnEvaluate()
        {
            this.lastEvaledAmt = this.InPortData.Count;
        }
    }

    [NodeName("Identity")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Identity function")]
    public class dynIdentity : dynNode
    {
        public dynIdentity()
        {
            InPortData.Add(new PortData("x", "in", typeof(bool)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("x", "out", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return args[0];
        }
    }

    #region Lists

    [NodeName("Reverse")]
    [NodeDescription("Reverses a list")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    public class dynReverse : dynBuiltinFunction
    {
        public dynReverse()
            : base("reverse")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("rev", "Reversed list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("List")]
    [NodeDescription("Makes a new list out of the given inputs")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    public class dynNewList : dynVariableInput
    {
        public dynNewList()
        {
            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("list", "A list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override string getInputRootName()
        {
            return "index";
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            if (this.SaveResult)
                return base.Compile(portNames);
            else
                return new FunctionNode("list", portNames);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var fun = ((Expression.Function)this.Bench.Environment.LookupSymbol("list")).Item;
            return fun.Invoke(ExecutionEnvironment.IDENT).Invoke(args);
        }
    }

    [NodeName("Sort-With")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Returns a sorted list, using the given comparitor.")]
    public class dynSortWith : dynBuiltinMacro
    {
        public dynSortWith()
            : base("sort-with")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));
            InPortData.Add(new PortData("c(x, y)", "Comparitor", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("sorted", "Sorted list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Sort-By")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Returns a sorted list, using the given key mapper.")]
    public class dynSortBy : dynBuiltinMacro
    {
        public dynSortBy()
            : base("sort-by")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));
            InPortData.Add(new PortData("c(x)", "Key Mapper", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("sorted", "Sorted list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Sort")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Returns a sorted list of numbers or strings.")]
    public class dynSort : dynBuiltinFunction
    {
        public dynSort()
            : base("sort")
        {
            InPortData.Add(new PortData("list", "List of numbers or strings to sort", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("sorted", "Sorted list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Reduce")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Reduces a sequence.")]
    [NodeSearchTags("fold")]
    public class dynFold : dynBuiltinMacro
    {
        public dynFold()
            : base("fold")
        {
            InPortData.Add(new PortData("f(x, a)", "Reductor Funtion", typeof(object)));
            InPortData.Add(new PortData("a", "Seed", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("out", "Result", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Filter")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Filters a sequence by a given predicate")]
    public class dynFilter : dynBuiltinMacro
    {
        public dynFilter()
            : base("filter")
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("filtered", "Filtered Sequence", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Build Sequence")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Creates a sequence of numbers")]
    [NodeSearchTags("range")]
    public class dynBuildSeq : dynBuiltinFunction
    {
        public dynBuildSeq()
            : base("build-seq")
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(double)));
            InPortData.Add(new PortData("end", "Number to end the sequence at", typeof(double)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("seq", "New sequence", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Combine")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Applies a combinator to each element in two sequences")]
    [NodeSearchTags("zip")]
    public class dynCombine : dynVariableInput
    {
        public dynCombine()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "First list", typeof(object)));
            InPortData.Add(new PortData("list2", "Second list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("combined", "Combined lists", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override string getInputRootName()
        {
            return "list";
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count > 3)
                base.RemoveInput(sender, args);
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(3))
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

                    if (!attr.Equals("comb"))
                        this.InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            NodeUI.ReregisterInputs();
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            if (this.SaveResult)
            {
                return new ExternalMacroNode(
                   new ExternMacro(this.macroEval),
                   portNames
                );
            }
            else
                return new FunctionNode("combine", portNames);
        }

        private Expression macroEval(FSharpList<Expression> args, ExecutionEnvironment environment)
        {
            if (this.RequiresRecalc || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.OnEvaluate();
            return this.oldValue;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var macro = ((Expression.Special)this.Bench.Environment
               .LookupSymbol("combine")).Item;

            return macro
               .Invoke(ExecutionEnvironment.IDENT)
               .Invoke(this.macroEnvironment.Env)
               .Invoke(args);
        }
    }

    [NodeName("Cartesian Product")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [NodeSearchTags("cross")]
    public class dynCartProd : dynVariableInput
    {
        public dynCartProd()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "First list", typeof(object)));
            InPortData.Add(new PortData("list2", "Second list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("combined", "Combined lists", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override string getInputRootName()
        {
            return "list";
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count > 3)
                base.RemoveInput(sender, args);
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(3))
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

                    if (!attr.Equals("comb"))
                        this.InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            NodeUI.ReregisterInputs();
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            if (this.SaveResult)
            {
                return new ExternalMacroNode(
                   new ExternMacro(this.macroEval),
                   portNames
                );
            }
            else
                return new FunctionNode("cartesian-product", portNames);
        }

        private Expression macroEval(FSharpList<Expression> args, ExecutionEnvironment environment)
        {
            if (this.RequiresRecalc || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.OnEvaluate();
            return this.oldValue;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var macro = ((Expression.Special)this.Bench.Environment
               .LookupSymbol("cartesian-product")).Item;

            return macro
               .Invoke(ExecutionEnvironment.IDENT)
               .Invoke(this.macroEnvironment.Env)
               .Invoke(args);
        }
    }

    [NodeName("Map")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Maps a sequence")]
    public class dynMap : dynBuiltinMacro
    {
        public dynMap()
            : base("map")
        {
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to map over.", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("mapped", "Mapped sequence", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Cons")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Creates a pair")]
    public class dynList : dynBuiltinFunction
    {
        public dynList()
            : base("cons")
        {
            InPortData.Add(new PortData("first", "The new Head of the list", typeof(object)));
            InPortData.Add(new PortData("rest", "The new Tail of the list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("list", "Result List", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Take")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Takes elements from a list")]
    public class dynTakeList : dynBuiltinFunction
    {
        public dynTakeList()
            : base("take")
        {
            InPortData.Add(new PortData("amt", "Amount of elements to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("elements", "List of extraced elements", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Drop")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Drops elements from a list")]
    public class dynDropList : dynBuiltinFunction
    {
        public dynDropList()
            : base("drop")
        {
            InPortData.Add(new PortData("amt", "Amount of elements to drop", typeof(object)));
            InPortData.Add(new PortData("list", "The list to drop elements from", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("elements", "List of remaining elements", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Get")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Gets an element from a list at a specified index.")]
    public class dynGetFromList : dynBuiltinFunction
    {
        public dynGetFromList()
            : base("get")
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("element", "Extracted element", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Empty")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("An empty list")]
    [IsInteractive(false)]
    public class dynEmpty : dynNode
    {
        public dynEmpty()
        {
            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("empty", "An empty list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewList(FSharpList<Expression>.Empty);
        }

        protected internal override INode Build()
        {
            return new SymbolNode("empty");
        }
    }

    [NodeName("Is Empty?")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Checks to see if the given list is empty.")]
    public class dynIsEmpty : dynBuiltinFunction
    {
        public dynIsEmpty()
            : base("empty?")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("empty?", "Is the given list empty?", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Length")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Gets the length of a list")]
    [NodeSearchTags("count")]
    public class dynLength : dynBuiltinFunction
    {
        public dynLength()
            : base("len")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("length", "Length of the list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Append")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Appends two list")]
    public class dynAppend : dynBuiltinFunction
    {
        public dynAppend()
            : base("append")
        {
            InPortData.Add(new PortData("listA", "First list", typeof(object)));
            InPortData.Add(new PortData("listB", "Second list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("A+B", "A appended onto B", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("First")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Gets the first element of a list")]
    public class dynFirst : dynBuiltinFunction
    {
        public dynFirst()
            : base("first")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("first", "First element in the list", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Rest")]
    [NodeCategory(BuiltinNodeCategories.LIST)]
    [NodeDescription("Gets the list with the first element removed.")]
    public class dynRest : dynBuiltinFunction
    {
        public dynRest()
            : base("rest")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("rest", "List without the first element.", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
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
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            outPortData = new PortData("x" + name + "y", "comp", typeof(double));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("<")]
    [NodeCategory(BuiltinNodeCategories.COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("less", "than")]
    public class dynLessThan : dynComparison
    {
        public dynLessThan() : base("<") { }
    }

    [NodeName("≤")]
    [NodeCategory(BuiltinNodeCategories.COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("<=", "less", "than", "equal")]
    public class dynLessThanEquals : dynComparison
    {
        public dynLessThanEquals() : base("<=", "≤") { }
    }

    [NodeName(">")]
    [NodeCategory(BuiltinNodeCategories.COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags("greater", "than")]
    public class dynGreaterThan : dynComparison
    {
        public dynGreaterThan() : base(">") { }
    }

    [NodeName("≥")]
    [NodeCategory(BuiltinNodeCategories.COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    [NodeSearchTags(">=", "greater", "than", "equal")]
    public class dynGreaterThanEquals : dynComparison
    {
        public dynGreaterThanEquals() : base(">=", "≥") { }
    }

    [NodeName("=")]
    [NodeCategory(BuiltinNodeCategories.COMPARISON)]
    [NodeDescription("Compares two numbers.")]
    public class dynEqual : dynComparison
    {
        public dynEqual() : base("=") { }
    }

    [NodeName("And")]
    [NodeCategory(BuiltinNodeCategories.BOOLEAN)]
    [NodeDescription("Boolean AND.")]
    public class dynAnd : dynBuiltinMacro
    {
        public dynAnd()
            : base("and")
        {
            InPortData.Add(new PortData("a", "operand", typeof(double)));
            InPortData.Add(new PortData("b", "operand", typeof(double)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("a∧b", "result", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Or")]
    [NodeCategory(BuiltinNodeCategories.BOOLEAN)]
    [NodeDescription("Boolean OR.")]
    public class dynOr : dynBuiltinMacro
    {
        public dynOr()
            : base("or")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("a∨b", "result", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Xor")]
    [NodeCategory(BuiltinNodeCategories.BOOLEAN)]
    [NodeDescription("Boolean XOR.")]
    public class dynXor : dynBuiltinMacro
    {
        public dynXor()
            : base("xor")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("a⊻b", "result", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Not")]
    [NodeCategory(BuiltinNodeCategories.BOOLEAN)]
    [NodeDescription("Boolean NOT.")]
    public class dynNot : dynBuiltinMacro
    {
        public dynNot()
            : base("not")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("!a", "result", typeof(bool));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    #endregion

    #region Math

    [NodeName("+")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Adds two numbers.")]
    [NodeSearchTags("plus", "addition", "sum")]
    public class dynAddition : dynBuiltinFunction
    {
        public dynAddition()
            : base("+")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("x+y", "sum", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("−")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Subtracts two numbers.")]
    [NodeSearchTags("subtraction", "minus", "difference", "-")]
    public class dynSubtraction : dynBuiltinFunction
    {
        public dynSubtraction()
            : base("-")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("x-y", "difference", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("×")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Multiplies two numbers.")]
    [NodeSearchTags("times", "multiply", "multiplication", "product", "*", "x")]
    public class dynMultiplication : dynBuiltinFunction
    {
        public dynMultiplication()
            : base("*")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("x∙y", "product", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("÷")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Divides two numbers.")]
    [NodeSearchTags("divide", "division", "quotient", "/")]
    public class dynDivision : dynBuiltinFunction
    {
        public dynDivision()
            : base("/")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));

            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("x÷y", "result", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Round")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Rounds a number to the nearest integer value.")]
    public class dynRound : dynNode
    {
        public dynRound()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("int", "Rounded number", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Round(((Expression.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Floor")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Rounds a number to the nearest smaller integer.")]
    [NodeSearchTags("round")]
    public class dynFloor : dynNode
    {
        public dynFloor()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("int", "Number rounded down", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Floor(((Expression.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Ceiling")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Rounds a number to the nearest larger integer value.")]
    [NodeSearchTags("round")]
    public class dynCeiling : dynNode
    {
        public dynCeiling()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("int", "Number rounded up", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Ceiling(((Expression.Number)args[0]).Item)
            );
        }
    }

    [NodeName("Random")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    public class dynRandom : dynNode
    {
        public dynRandom()
        {
            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("rand", "Random number between 0.0 and 1.0.", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(random.NextDouble());
        }
    }

    [NodeName("π")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Pi constant")]
    [NodeSearchTags("pi", "trigonometry", "circle")]
    [IsInteractive(false)]
    public class dynPi : dynNode
    {
        public dynPi()
        {
            this.NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("3.14159...", "pi", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override bool RequiresRecalc
        {
            get
            {
                return false;
            }
            set { }
        }

        protected internal override INode Build()
        {
            return new NumberNode(Math.PI);
        }
    }

    [NodeName("Sine")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Computes the sine of the given angle.")]
    public class dynSin : dynNode
    {
        public dynSin()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("sin(θ)", "Sine value of the given angle", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Expression.NewList(
                   FSchemeInterop.Utils.convertSequence(
                      ((Expression.List)input).Item.Select(
                         x =>
                            Expression.NewNumber(Math.Sin(((Expression.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Expression.Number)input).Item;
                return Expression.NewNumber(Math.Sin(theta));
            }
        }
    }

    [NodeName("Cosine")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Computes the cosine of the given angle.")]
    public class dynCos : dynNode
    {
        public dynCos()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("cos(θ)", "Cosine value of the given angle", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Expression.NewList(
                   FSchemeInterop.Utils.convertSequence(
                      ((Expression.List)input).Item.Select(
                         x =>
                            Expression.NewNumber(Math.Cos(((Expression.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Expression.Number)input).Item;
                return Expression.NewNumber(Math.Cos(theta));
            }
        }
    }

    [NodeName("Tangent")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("Computes the tangent of the given angle.")]
    public class dynTan : dynNode
    {
        public dynTan()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("tan(θ)", "Tangent value of the given angle", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var input = args[0];

            if (input.IsList)
            {
                return Expression.NewList(
                   FSchemeInterop.Utils.convertSequence(
                      ((Expression.List)input).Item.Select(
                         x =>
                            Expression.NewNumber(Math.Tan(((Expression.Number)x).Item))
                      )
                   )
                );
            }
            else
            {
                double theta = ((Expression.Number)input).Item;
                return Expression.NewNumber(Math.Tan(theta));
            }
        }
    }

    #endregion

    #region Control Flow

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Perform All")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Executes expressions in a sequence")]
    public class dynBegin : dynVariableInput
    {
        public dynBegin()
        {
            InPortData.Add(new PortData("expr1", "Expression #1", typeof(object)));
            InPortData.Add(new PortData("expr2", "Expression #2", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("last", "Result of final expression", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count > 2)
                base.RemoveInput(sender, args);
        }

        protected override string getInputRootName()
        {
            return "expr";
        }

        protected override int getNewInputIndex()
        {
            return this.InPortData.Count + 1;
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            return new BeginNode(portNames);
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Apply")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Applies arguments to a function")]
    public class dynApply1 : dynVariableInput
    {
        public dynApply1()
        {
            InPortData.Add(new PortData("func", "Procedure", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("result", "Result", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override string getInputRootName()
        {
            return "arg";
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            return new ApplierNode(portNames.Skip(1));
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count > 1)
                base.RemoveInput(sender, args);
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
                        this.InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            NodeUI.ReregisterInputs();
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("If")]
    [NodeCategory(BuiltinNodeCategories.BOOLEAN)]
    [NodeDescription("Conditional statement")]
    public class dynConditional : dynNode
    {
        public dynConditional()
        {
            InPortData.Add(new PortData("test", "Test block", typeof(bool)));
            InPortData.Add(new PortData("true", "True block", typeof(object)));
            InPortData.Add(new PortData("false", "False block", typeof(object)));

            NodeUI.nickNameBlock.FontSize = 20;

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("result", "Result", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            return new ConditionalNode();
        }
    }

    [NodeName("Debug Breakpoint")]
    [NodeCategory(BuiltinNodeCategories.MISC)]
    [NodeDescription("Halts execution until user clicks button.")]
    public class dynBreakpoint : dynNode
    {
        System.Windows.Controls.Button button;

        public dynBreakpoint()
        {
            //add a text box to the input grid of the control
            button = new System.Windows.Controls.Button();
            button.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            button.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            //inputGrid.RowDefinitions.Add(new RowDefinition());
            NodeUI.inputGrid.Children.Add(button);
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetRow(button, 0);
            button.Content = "Continue";

            this.enabled = false;

            button.Click += new RoutedEventHandler(button_Click);

            InPortData.Add(new PortData("", "Object to inspect", typeof(object)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("", "Object inspected", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
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
            this.NodeUI.Deselect();
            enabled = false;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var result = args[0];

            dynSettings.Instance.Bench.Dispatcher.Invoke(new Action(
               delegate
               {
                   dynSettings.Instance.Bench.Log(FScheme.print(result));
               }
            ));

            if (dynSettings.Instance.Bench.RunInDebug)
            {
                button.Dispatcher.Invoke(new Action(
                   delegate
                   {
                       enabled = true;
                       this.NodeUI.Select();
                       dynSettings.Instance.Bench.ShowElement(this);
                   }
                ));

                while (enabled)
                {
                    Thread.Sleep(1);
                }
            }

            return result;
        }
    }

    #endregion

    #region Mutative Math

    //MDJ dynOptimizer added 11/22-11 (or dynEvaluate?)
    [NodeName("Optimizer")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("An element which evaluates one inpute against another and passes out the larger of the two values.")]
    public class dynOptimizer : dynNode
    {
        TextBox tb;

        public dynOptimizer()
        {
            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0.0";
            //tb.IsReadOnly = true;
            //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);
            tb.TextChanged += delegate { this.RequiresRecalc = true; };

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            InPortData.Add(new PortData("N", "New Value", typeof(double)));
            //InPortData.Add(new PortData(null, "I", "Initial Value", typeof(dynDouble)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("dbl", "The larger value of the input vs. the current value", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", this.tb.Text);
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == typeof(double).FullName)
                {
                    this.tb.Text = subNode.Attributes[0].Value;
                }
            }
        }

        public double currentValue = 0.0;// instead of initialValue for now

        public double CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                //NotifyPropertyChanged("CurrentValue");
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double newValue = ((Expression.Number)args.Head).Item;
            if (newValue > this.CurrentValue)
            {
                this.CurrentValue = newValue;
                this.tb.Text = this.CurrentValue.ToString();
            }
            return Expression.NewNumber(this.CurrentValue);
        }
    }

    //MDJ dynIncrementer added 11/22-11
    [NodeName("Incrementer")]
    [NodeCategory(BuiltinNodeCategories.MATH)]
    [NodeDescription("An element which watches one input then if that changes, increments the output integer until it hits a max value.")]
    public class dynIncrementer : dynNode
    {
        TextBox tb;

        public dynIncrementer()
        {
            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0";
            tb.TextChanged += delegate { this.RequiresRecalc = true; };
            //tb.IsReadOnly = true;
            //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            InPortData.Add(new PortData("m", "Max Iterations", typeof(double)));
            InPortData.Add(new PortData("v", "Value", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("v", "Value", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public double currentValue = 0.0;// instead of initialValue for now

        public double CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                //NotifyPropertyChanged("CurrentValue");
            }
        }


        public int numIterations = 0;

        public int NumIterations
        {
            get { return numIterations; }
            set
            {
                numIterations = value;
                //NotifyPropertyChanged("NumIterations");
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            double maxIterations = ((Expression.Number)args[0]).Item;
            double newValue = ((Expression.Number)args[1]).Item;
            if (newValue != this.CurrentValue)
            {
                this.NumIterations++;
                this.CurrentValue = newValue;
                this.tb.Dispatcher.Invoke(new Action(
                   delegate { this.tb.Text = this.NumIterations.ToString(); }
                ));
            }
            return Expression.NewNumber(this.NumIterations);
        }
    }

    #endregion

    #region Interactive Primitive Types

    #region Base Classes

    class dynTextBox : TextBox
    {
        public event Action OnChangeCommitted;

        private static Brush clear = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        //private static Brush waiting = Brushes.Orange;

        public dynTextBox()
        {
            //turn off the border
            Background = clear;
            BorderThickness = new Thickness(0);
        }

        private bool numeric;
        public bool IsNumeric
        {
            get { return numeric; }
            set
            {
                numeric = value;
                if (value && this.Text.Length > 0)
                {
                    this.Text = dynBench.RemoveChars(
                       this.Text,
                       this.Text.ToCharArray()
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
                    this.FontStyle = FontStyles.Italic;
                    this.FontWeight = FontWeights.Bold;
                }
                else
                {
                    this.FontStyle = FontStyles.Normal;
                    this.FontWeight = FontWeights.Normal;
                }
                pending = value;
            }
        }

        private void commit()
        {
            if (this.OnChangeCommitted != null)
            {
                this.OnChangeCommitted();
            }
            this.Pending = false;
        }

        new public string Text
        {
            get { return base.Text; }
            set
            {
                base.Text = value;
                this.commit();
            }
        }

        private bool shouldCommit()
        {
            return !dynSettings.Instance.Bench.DynamicRunEnabled;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            this.Pending = true;

            if (this.IsNumeric)
            {
                var p = this.CaretIndex;

                base.Text = dynBench.RemoveChars(
                   this.Text,
                   this.Text.ToCharArray()
                      .Where(c => !char.IsDigit(c) && c != '-' && c != '.')
                      .Select(c => c.ToString())
                );

                this.CaretIndex = p;
            }
        }

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return || e.Key == System.Windows.Input.Key.Enter)
            {
                this.commit();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            this.commit();
        }
    }

    [IsInteractive(true)]
    public abstract class dynBasicInteractive<T> : dynNode
    {
        private T _value = default(T);
        public virtual T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (this._value == null || !this._value.Equals(value))
                {
                    this._value = value;
                    this.RequiresRecalc = value != null;
                }
            }
        }

        protected abstract T DeserializeValue(string val);

        public dynBasicInteractive()
        {
            Type type = typeof(T);
            outPortData = new PortData("", type.Name, type);
        }

        private PortData outPortData;
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", this.Value.ToString());
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(T).FullName))
                {
                    this.Value = this.DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return this.Value.ToString();
        }
    }

    public abstract class dynDouble : dynBasicInteractive<double>
    {
        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(this.Value);
        }
    }

    public abstract class dynBool : dynBasicInteractive<bool>
    {
        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(this.Value ? 1 : 0);
        }
    }

    public abstract class dynString : dynBasicInteractive<string>
    {
        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewString(this.Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }
    }

    #endregion

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    [NodeDescription("An element which creates an unsigned floating point number.")]
    public class dynDoubleInput : dynDouble
    {
        dynTextBox tb;

        public dynDoubleInput()
        {
            //add a text box to the input grid of the control
            tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = true;
            tb.Text = "0.0";
            tb.OnChangeCommitted += delegate { this.Value = this.DeserializeValue(this.tb.Text); };

            NodeUI.RegisterInputsAndOutput();

            //take out the left and right margins
            //and make this so it's not so wide
            NodeUI.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            NodeUI.topControl.Width = 100;

            NodeUI.UpdateLayout();
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
                this.tb.Text = value.ToString();
                //this.tb.Pending = false;
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

    //MDJ - added by Matt Jezyk 10.27.2011
    [NodeName("Number Slider")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    [NodeDescription("An element which creates an unsigned floating point number, but using SLIDERS!.")]
    public class dynDoubleSliderInput : dynDouble
    {
        Slider tb_slider;
        dynTextBox mintb;
        dynTextBox maxtb;

        public dynDoubleSliderInput()
        {
            //add a slider control to the input grid of the control
            tb_slider = new System.Windows.Controls.Slider();
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb_slider);
            System.Windows.Controls.Grid.SetColumn(tb_slider, 1);
            System.Windows.Controls.Grid.SetRow(tb_slider, 0);
            tb_slider.Value = 0.0;
            tb_slider.Maximum = 100.0;
            tb_slider.Minimum = 0.0;
            tb_slider.Ticks = new System.Windows.Media.DoubleCollection(10);
            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
            tb_slider.ValueChanged += delegate { this.Value = this.tb_slider.Value; };

            mintb = new dynTextBox();
            mintb.MaxLength = 3;
            mintb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            mintb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            mintb.Width = double.NaN;
            mintb.IsNumeric = true;
            mintb.Text = "0";
            mintb.OnChangeCommitted += delegate
            {
                try
                {
                    this.tb_slider.Minimum = Convert.ToDouble(mintb.Text);
                }
                catch
                {
                    this.tb_slider.Minimum = 0;
                }
            };
            //mintb.Pending = false;

            maxtb = new dynTextBox();
            maxtb.MaxLength = 3;
            maxtb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            maxtb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            maxtb.Width = double.NaN;
            maxtb.IsNumeric = true;
            maxtb.Text = "100";
            maxtb.OnChangeCommitted += delegate
            {
                try
                {
                    this.tb_slider.Maximum = Convert.ToDouble(maxtb.Text);
                }
                catch
                {
                    this.tb_slider.Maximum = 0;
                }
            };
            //maxtb.Pending = false;

            NodeUI.SetColumnAmount(3);
            NodeUI.inputGrid.Children.Add(mintb);
            NodeUI.inputGrid.Children.Add(maxtb);

            System.Windows.Controls.Grid.SetColumn(mintb, 0);
            System.Windows.Controls.Grid.SetColumn(maxtb, 2);

            NodeUI.RegisterInputsAndOutput();
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

        public override double Value
        {
            set
            {
                if (base.Value == value)
                    return;

                if (value > this.tb_slider.Maximum)
                {
                    this.maxtb.Text = value.ToString();
                    //this.maxtb.Pending = false;
                }
                if (value < this.tb_slider.Minimum)
                {
                    this.mintb.Text = value.ToString();
                    //this.mintb.Pending = false;
                }

                base.Value = value;
                this.tb_slider.Value = value;
            }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", this.Value.ToString());
            outEl.SetAttribute("min", this.tb_slider.Minimum.ToString());
            outEl.SetAttribute("max", this.tb_slider.Maximum.ToString());
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
                            this.Value = this.DeserializeValue(attr.Value);
                        else if (attr.Name.Equals("min"))
                        {
                            //this.tb_slider.Minimum = Convert.ToDouble(attr.Value);
                            this.mintb.Text = attr.Value;
                        }
                        else if (attr.Name.Equals("max"))
                        {
                            //this.tb_slider.Maximum = Convert.ToDouble(attr.Value);
                            this.maxtb.Text = attr.Value;
                        }
                    }
                }
            }
        }
    }

    [NodeName("Boolean")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    [NodeDescription("An element which allows selection between a true and false.")]
    [NodeSearchTags("true", "truth", "false")]
    public class dynBoolSelector : dynBool
    {
        System.Windows.Controls.RadioButton rbTrue;
        System.Windows.Controls.RadioButton rbFalse;

        public dynBoolSelector()
        {
            //inputGrid.Margin = new System.Windows.Thickness(5,5,20,5);

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
            rbFalse.Content = "0";

            RowDefinition rd = new RowDefinition();
            ColumnDefinition cd1 = new ColumnDefinition();
            ColumnDefinition cd2 = new ColumnDefinition();
            NodeUI.inputGrid.ColumnDefinitions.Add(cd1);
            NodeUI.inputGrid.ColumnDefinitions.Add(cd2);
            NodeUI.inputGrid.RowDefinitions.Add(rd);

            NodeUI.inputGrid.Children.Add(rbTrue);
            NodeUI.inputGrid.Children.Add(rbFalse);

            System.Windows.Controls.Grid.SetColumn(rbTrue, 0);
            System.Windows.Controls.Grid.SetRow(rbTrue, 0);
            System.Windows.Controls.Grid.SetColumn(rbFalse, 1);
            System.Windows.Controls.Grid.SetRow(rbFalse, 0);

            rbFalse.IsChecked = true;
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);
            //OutPortData[0].Object = false;

            NodeUI.RegisterInputsAndOutput();
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

        public override bool Value
        {
            set
            {
                base.Value = value;
                if (value)
                {
                    this.rbFalse.IsChecked = false;
                    this.rbTrue.IsChecked = true;
                }
                else
                {
                    this.rbFalse.IsChecked = true;
                    this.rbTrue.IsChecked = false;
                }
            }
        }

        void rbFalse_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Value = false;
        }

        void rbTrue_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Value = true;
        }
    }

    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    [NodeDescription("An element which creates a string value.")]
    public class dynStringInput : dynString
    {
        dynTextBox tb;

        public dynStringInput()
        {
            //add a text box to the input grid of the control
            tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "";

            tb.OnChangeCommitted += delegate { this.Value = this.tb.Text; };

            NodeUI.RegisterInputsAndOutput();
        }

        public override string Value
        {
            set
            {
                if (base.Value == value)
                    return;

                base.Value = value;
                this.tb.Text = value;
            }
        }

        void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Value = this.tb.Text;
        }

        void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Keys.Enter))
                this.Value = this.tb.Text;
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }
    }

    [NodeName("Filename")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    [NodeDescription("Allows you to select a file on the system to get its filename.")]
    public class dynStringFilename : dynString
    {
        System.Windows.Controls.TextBox tb;

        public dynStringFilename()
        {
            //add a button to the inputGrid on the dynElement
            System.Windows.Controls.Button readFileButton = new System.Windows.Controls.Button();
            readFileButton.Margin = new System.Windows.Thickness(0, 0, 0, 0);
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
            readFileButton.Content = "Browse...";
            readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tb = new TextBox();
            tb.Text = "No file selected.";
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);
            tb.IsReadOnly = true;
            tb.IsReadOnlyCaretVisible = false;
            tb.TextChanged += delegate { tb.ScrollToHorizontalOffset(double.PositiveInfinity); };

            NodeUI.SetRowAmount(2);

            NodeUI.inputGrid.Children.Add(tb);
            NodeUI.inputGrid.Children.Add(readFileButton);

            System.Windows.Controls.Grid.SetRow(readFileButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            NodeUI.RegisterInputsAndOutput();

            NodeUI.topControl.Height = 60;
            NodeUI.UpdateLayout();
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

                this.tb.Text = string.IsNullOrEmpty(this.Value)
                   ? "No file selected."
                   : this.Value;
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
                this.Value = openDialog.FileName;
            }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            if (string.IsNullOrEmpty(this.Value))
                throw new Exception("No file selected.");

            return base.Evaluate(args);
        }
    }

    #endregion

    #region Strings and Conversions

    [NodeName("Concatenate Strings")]
    [NodeDescription("Concatenates two or more strings")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    public class dynConcatStrings : dynVariableInput
    {
        public dynConcatStrings()
        {
            InPortData.Add(new PortData("s1", "First string", typeof(string)));
            InPortData.Add(new PortData("s2", "Second string", typeof(string)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("combined", "Combined lists", typeof(string));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        protected override string getInputRootName()
        {
            return "s";
        }

        protected override int getNewInputIndex()
        {
            return this.InPortData.Count + 1;
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count > 2)
                base.RemoveInput(sender, args);
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

                    this.InPortData.Add(new PortData(subNode.Attributes["name"].Value, "", typeof(object)));
                }
            }
            NodeUI.ReregisterInputs();
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            if (this.SaveResult)
                return base.Compile(portNames);
            else
                return new FunctionNode("concat-strings", portNames);
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var fun = ((Expression.Function)this.Bench.Environment.LookupSymbol("concat-strings")).Item;
            return fun.Invoke(ExecutionEnvironment.IDENT).Invoke(args);
        }
    }

    [NodeName("String -> Number")]
    [NodeDescription("Converts a string to a number")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    public class dynString2Num : dynBuiltinFunction
    {
        public dynString2Num()
            : base("string->num")
        {
            InPortData.Add(new PortData("s", "A string", typeof(string)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("n", "A number", typeof(double));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Number -> String")]
    [NodeDescription("Converts a number to a string")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    public class dynNum2String : dynBuiltinFunction
    {
        public dynNum2String()
            : base("num->string")
        {
            InPortData.Add(new PortData("n", "A number", typeof(double)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("s", "A string", typeof(string));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }
    }

    [NodeName("Split String")]
    [NodeDescription("Separates a string based on the given delimiter")]
    [NodeCategory(BuiltinNodeCategories.PRIMITIVES)]
    public class dynSplitString : dynNode
    {
        public dynSplitString()
        {
            InPortData.Add(new PortData("str", "String to split", typeof(string)));
            InPortData.Add(new PortData("del", "Delimiter", typeof(string)));

            NodeUI.RegisterInputsAndOutput();
        }

        private PortData outPortData = new PortData("strs", "A list of strings", typeof(object));
        public override PortData OutPortData
        {
            get { return outPortData; }
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewList(
                Utils.convertSequence(
                    ((Expression.String)args[0]).Item.Split(
                        ((Expression.String)args[1]).Item.ToCharArray()
                    ).Select(Expression.NewString)
                )
            );
        }
    }

    #endregion
}


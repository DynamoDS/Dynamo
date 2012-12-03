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
using System.Web;

using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;

using Microsoft.FSharp.Collections;

using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;
using System.Diagnostics.Contracts;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;

namespace Dynamo.Elements
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static class BuiltinElementCategories
    {
        public const string MATH = "Math";
        public const string COMPARISON = "Comparison";
        public const string BOOLEAN = "Logic";
        public const string PRIMITIVES = "Primitives";
        public const string REVIT = "Revit";
        public const string MISC = "Miscellaneous";
        public const string LIST = "Lists";
        public const string ANALYSIS = "Analysis";
        public const string MEASUREMENT = "Measurement";
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
            if (this.IsDirty || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.runCount++;
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
                     x => this.macroEnvironment.Evaluate(x)
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
            : base()
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

            inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition());

            inputGrid.Children.Add(addButton);
            System.Windows.Controls.Grid.SetColumn(addButton, 0);

            inputGrid.Children.Add(subButton);
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
        public override bool IsDirty
        {
            get
            {
                return lastEvaledAmt != this.InPortData.Count || base.IsDirty;
            }
            set
            {
                base.IsDirty = value;
            }
        }

        protected virtual void RemoveInput(object sender, RoutedEventArgs args)
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
                base.ReregisterInputs();
            }
        }

        protected virtual void AddInput(object sender, RoutedEventArgs args)
        {
            InPortData.Add(new PortData(this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
            base.ReregisterInputs();
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
            base.ReregisterInputs();
        }

        protected override void OnEvaluate()
        {
            this.lastEvaledAmt = this.InPortData.Count;
        }
    }

    [ElementName("Identity")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Identity function")]
    [RequiresTransaction(false)]
    public class dynIdentity : dynNode
    {
        public dynIdentity()
        {
            InPortData.Add(new PortData("x", "in", typeof(bool)));
            OutPortData = new PortData("x", "out", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return args[0];
        }
    }

    #region Lists

    [ElementName("Reverse")]
    [ElementDescription("Reverses a list")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [RequiresTransaction(false)]
    public class dynReverse : dynBuiltinFunction
    {
        public dynReverse()
            : base("reverse")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));

            OutPortData = new PortData("rev", "Reversed list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("List")]
    [ElementDescription("Makes a new list out of the given inputs")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [RequiresTransaction(false)]
    public class dynNewList : dynVariableInput
    {
        public dynNewList()
        {
            InPortData.Add(new PortData("item(s)", "Item(s) to build a list out of", typeof(object)));
            OutPortData = new PortData("list", "A list", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        protected override string getInputRootName()
        {
            return "index";
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 2)
                InPortData[0] = new PortData("item(s)", "Item(s) to build a list out of", typeof(object));
            if (InPortData.Count > 1)
                base.RemoveInput(sender, args);
        }

        protected override void AddInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 1)
                InPortData[0] = new PortData("index0", "First item", typeof(object));
            base.AddInput(sender, args);
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

    [ElementName("Sort-With")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Returns a sorted list, using the given comparitor.")]
    [RequiresTransaction(false)]
    public class dynSortWith : dynBuiltinMacro
    {
        public dynSortWith()
            : base("sort-with")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));
            InPortData.Add(new PortData("c(x, y)", "Comparitor", typeof(object)));

            OutPortData = new PortData("sorted", "Sorted list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Sort-By")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Returns a sorted list, using the given key mapper.")]
    [RequiresTransaction(false)]
    public class dynSortBy : dynBuiltinMacro
    {
        public dynSortBy()
            : base("sort-by")
        {
            InPortData.Add(new PortData("list", "List to sort", typeof(object)));
            InPortData.Add(new PortData("c(x)", "Key Mapper", typeof(object)));

            OutPortData = new PortData("sorted", "Sorted list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Sort")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Returns a sorted list of numbers or strings.")]
    [RequiresTransaction(false)]
    public class dynSort : dynBuiltinFunction
    {
        public dynSort()
            : base("sort")
        {
            InPortData.Add(new PortData("list", "List of numbers or strings to sort", typeof(object)));

            OutPortData = new PortData("sorted", "Sorted list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Reduce")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Reduces a sequence.")]
    [ElementSearchTags("fold")]
    [RequiresTransaction(false)]
    public class dynFold : dynBuiltinMacro
    {
        public dynFold()
            : base("fold")
        {
            InPortData.Add(new PortData("f(x, a)", "Reductor Funtion", typeof(object)));
            InPortData.Add(new PortData("a", "Seed", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence", typeof(object)));
            OutPortData = new PortData("out", "Result", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Filter")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Filters a sequence by a given predicate")]
    [RequiresTransaction(false)]
    public class dynFilter : dynBuiltinMacro
    {
        public dynFilter()
            : base("filter")
        {
            InPortData.Add(new PortData("p(x)", "Predicate", typeof(object)));
            InPortData.Add(new PortData("seq", "Sequence to filter", typeof(object)));
            OutPortData = new PortData("filtered", "Filtered Sequence", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Number Sequence")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Creates a sequence of numbers")]
    [ElementSearchTags("range")]
    [RequiresTransaction(false)]
    public class dynBuildSeq : dynBuiltinFunction
    {
        public dynBuildSeq()
            : base("build-seq")
        {
            InPortData.Add(new PortData("start", "Number to start the sequence at", typeof(double)));
            InPortData.Add(new PortData("end", "Number to end the sequence at", typeof(double)));
            InPortData.Add(new PortData("step", "Space between numbers", typeof(double)));
            OutPortData = new PortData("seq", "New sequence", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Combine")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Applies a combinator to each element in two sequences")]
    [ElementSearchTags("zip")]
    [RequiresTransaction(false)]
    public class dynCombine : dynVariableInput
    {
        public dynCombine()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "First list", typeof(object)));
            InPortData.Add(new PortData("list2", "Second list", typeof(object)));
            OutPortData = new PortData("combined", "Combined lists", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        protected override string getInputRootName()
        {
            return "list";
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 2)
                base.RemoveInput(sender, args);
        }

        protected override void AddInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput(sender, args);
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
                this.RemoveInput(this, null);
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
                }
                base.ReregisterInputs();
            }
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
            if (this.IsDirty || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.runCount++;
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

    [ElementName("Cartesian Product")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Applies a combinator to each pair in the cartesian product of two sequences")]
    [ElementSearchTags("cross")]
    [RequiresTransaction(false)]
    public class dynCartProd : dynVariableInput
    {
        public dynCartProd()
        {
            InPortData.Add(new PortData("comb", "Combinator", typeof(object)));
            InPortData.Add(new PortData("list1", "First list", typeof(object)));
            InPortData.Add(new PortData("list2", "Second list", typeof(object)));
            OutPortData = new PortData("combined", "Combined lists", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        protected override string getInputRootName()
        {
            return "list";
        }

        protected override void RemoveInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 3)
                InPortData[1] = new PortData("lists", "List of lists to combine", typeof(object));
            if (InPortData.Count > 2)
                base.RemoveInput(sender, args);
        }

        protected override void AddInput(object sender, RoutedEventArgs args)
        {
            if (InPortData.Count == 2)
                InPortData[1] = new PortData("list1", "First list", typeof(object));
            base.AddInput(sender, args);
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
                this.RemoveInput(this, null);
            else
            {
                for (; inputs > 2; inputs--)
                {
                    InPortData.Add(new PortData(this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
                }
                base.ReregisterInputs();
            }
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
            if (this.IsDirty || this.oldValue == null)
            {
                this.macroEnvironment = environment;
                this.oldValue = this.eval(args);
            }
            else
                this.runCount++;
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

    [ElementName("Map")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Maps a sequence")]
    [RequiresTransaction(false)]
    public class dynMap : dynBuiltinMacro
    {
        public dynMap()
            : base("map")
        {
            InPortData.Add(new PortData("f(x)", "The procedure used to map elements", typeof(object)));
            InPortData.Add(new PortData("seq", "The sequence to map over.", typeof(object)));
            OutPortData = new PortData("mapped", "Mapped sequence", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Cons")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Creates a pair")]
    [RequiresTransaction(false)]
    public class dynList : dynBuiltinFunction
    {
        public dynList()
            : base("cons")
        {
            InPortData.Add(new PortData("first", "The new Head of the list", typeof(object)));
            InPortData.Add(new PortData("rest", "The new Tail of the list", typeof(object)));
            OutPortData = new PortData("list", "Result List", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Take")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Takes elements from a list")]
    [RequiresTransaction(false)]
    public class dynTakeList : dynBuiltinFunction
    {
        public dynTakeList()
            : base("take")
        {
            InPortData.Add(new PortData("amt", "Amount of elements to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(object)));
            OutPortData = new PortData("elements", "List of extraced elements", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Drop")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Drops elements from a list")]
    [RequiresTransaction(false)]
    public class dynDropList : dynBuiltinFunction
    {
        public dynDropList()
            : base("drop")
        {
            InPortData.Add(new PortData("amt", "Amount of elements to drop", typeof(object)));
            InPortData.Add(new PortData("list", "The list to drop elements from", typeof(object)));
            OutPortData = new PortData("elements", "List of remaining elements", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Get")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Gets an element from a list at a specified index.")]
    [RequiresTransaction(false)]
    public class dynGetFromList : dynBuiltinFunction
    {
        public dynGetFromList()
            : base("get")
        {
            InPortData.Add(new PortData("index", "Index of the element to extract", typeof(object)));
            InPortData.Add(new PortData("list", "The list to extract elements from", typeof(object)));
            OutPortData = new PortData("element", "Extracted element", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Empty")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("An empty list")]
    [RequiresTransaction(false)]
    [IsInteractive(false)]
    public class dynEmpty : dynNode
    {
        public dynEmpty()
        {
            OutPortData = new PortData("empty", "An empty list", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override bool IsDirty
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

    [ElementName("Is Empty?")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Checks to see if the given list is empty.")]
    [RequiresTransaction(false)]
    public class dynIsEmpty : dynBuiltinFunction
    {
        public dynIsEmpty()
            : base("empty?")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));
            OutPortData = new PortData("empty?", "Is the given list empty?", typeof(bool));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Count")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Gets the length of a list")]
    [RequiresTransaction(false)]
    public class dynLength : dynBuiltinFunction
    {
        public dynLength()
            : base("len")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));
            OutPortData = new PortData("length", "Length of the list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Append")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Appends two list")]
    [RequiresTransaction(false)]
    public class dynAppend : dynBuiltinFunction
    {
        public dynAppend()
            : base("append")
        {
            InPortData.Add(new PortData("listA", "First list", typeof(object)));
            InPortData.Add(new PortData("listB", "Second list", typeof(object)));
            OutPortData = new PortData("A+B", "A appended onto B", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("First")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Gets the first element of a list")]
    [RequiresTransaction(false)]
    public class dynFirst : dynBuiltinFunction
    {
        public dynFirst()
            : base("first")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));
            OutPortData = new PortData("first", "First element in the list", typeof(object));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Rest")]
    [ElementCategory(BuiltinElementCategories.LIST)]
    [ElementDescription("Gets the list with the first element removed.")]
    [RequiresTransaction(false)]
    public class dynRest : dynBuiltinFunction
    {
        public dynRest()
            : base("rest")
        {
            InPortData.Add(new PortData("list", "A list", typeof(object)));
            OutPortData = new PortData("rest", "List without the first element.", typeof(object));

            base.RegisterInputsAndOutputs();
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
            OutPortData = new PortData("x" + name + "y", "comp", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("<")]
    [ElementCategory(BuiltinElementCategories.COMPARISON)]
    [ElementDescription("Compares two numbers.")]
    [ElementSearchTags("less", "than")]
    [RequiresTransaction(false)]
    public class dynLessThan : dynComparison
    {
        public dynLessThan() : base("<") { }
    }

    [ElementName("≤")]
    [ElementCategory(BuiltinElementCategories.COMPARISON)]
    [ElementDescription("Compares two numbers.")]
    [ElementSearchTags("<=", "less", "than", "equal")]
    [RequiresTransaction(false)]
    public class dynLessThanEquals : dynComparison
    {
        public dynLessThanEquals() : base("<=", "≤") { }
    }

    [ElementName(">")]
    [ElementCategory(BuiltinElementCategories.COMPARISON)]
    [ElementDescription("Compares two numbers.")]
    [ElementSearchTags("greater", "than")]
    [RequiresTransaction(false)]
    public class dynGreaterThan : dynComparison
    {
        public dynGreaterThan() : base(">") { }
    }

    [ElementName("≥")]
    [ElementCategory(BuiltinElementCategories.COMPARISON)]
    [ElementDescription("Compares two numbers.")]
    [ElementSearchTags(">=", "greater", "than", "equal")]
    [RequiresTransaction(false)]
    public class dynGreaterThanEquals : dynComparison
    {
        public dynGreaterThanEquals() : base(">=", "≥") { }
    }

    [ElementName("=")]
    [ElementCategory(BuiltinElementCategories.COMPARISON)]
    [ElementDescription("Compares two numbers.")]
    [RequiresTransaction(false)]
    public class dynEqual : dynComparison
    {
        public dynEqual() : base("=") { }
    }

    [ElementName("And")]
    [ElementCategory(BuiltinElementCategories.BOOLEAN)]
    [ElementDescription("Boolean AND.")]
    [RequiresTransaction(false)]
    public class dynAnd : dynBuiltinMacro
    {
        public dynAnd()
            : base("and")
        {
            InPortData.Add(new PortData("a", "operand", typeof(double)));
            InPortData.Add(new PortData("b", "operand", typeof(double)));
            OutPortData = new PortData("a∧b", "result", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Or")]
    [ElementCategory(BuiltinElementCategories.BOOLEAN)]
    [ElementDescription("Boolean OR.")]
    [RequiresTransaction(false)]
    public class dynOr : dynBuiltinMacro
    {
        public dynOr()
            : base("or")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData = new PortData("a∨b", "result", typeof(bool));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Xor")]
    [ElementCategory(BuiltinElementCategories.BOOLEAN)]
    [ElementDescription("Boolean XOR.")]
    [RequiresTransaction(false)]
    public class dynXor : dynBuiltinMacro
    {
        public dynXor()
            : base("xor")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            InPortData.Add(new PortData("b", "operand", typeof(bool)));
            OutPortData = new PortData("a⊻b", "result", typeof(bool));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Not")]
    [ElementCategory(BuiltinElementCategories.BOOLEAN)]
    [ElementDescription("Boolean NOT.")]
    [RequiresTransaction(false)]
    public class dynNot : dynBuiltinMacro
    {
        public dynNot()
            : base("not")
        {
            InPortData.Add(new PortData("a", "operand", typeof(bool)));
            OutPortData = new PortData("!a", "result", typeof(bool));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    #endregion

    #region Math

    [ElementName("+")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Adds two numbers.")]
    [ElementSearchTags("plus", "addition", "sum")]
    [RequiresTransaction(false)]
    public class dynAddition : dynBuiltinFunction
    {
        public dynAddition()
            : base("+")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x+y", "sum", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("−")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Subtracts two numbers.")]
    [ElementSearchTags("subtraction", "minus", "difference", "-")]
    [RequiresTransaction(false)]
    public class dynSubtraction : dynBuiltinFunction
    {
        public dynSubtraction()
            : base("-")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x-y", "difference", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("×")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Multiplies two numbers.")]
    [ElementSearchTags("times", "multiply", "multiplication", "product", "*", "x")]
    [RequiresTransaction(false)]
    public class dynMultiplication : dynBuiltinFunction
    {
        public dynMultiplication()
            : base("*")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x∙y", "product", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("÷")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Divides two numbers.")]
    [ElementSearchTags("divide", "division", "quotient", "/")]
    [RequiresTransaction(false)]
    public class dynDivision : dynBuiltinFunction
    {
        public dynDivision()
            : base("/")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x÷y", "result", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Mod")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Remainder of division of two numbers.")]
    [ElementSearchTags("%", "modulo", "remainder")]
    [RequiresTransaction(false)]
    public class dynModulo : dynBuiltinFunction
    {
        public dynModulo()
            : base("%")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x%y", "result", typeof(double));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Pow")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Raises a number to the power of another.")]
    [ElementSearchTags("power", "exponentiation", "^")]
    [RequiresTransaction(false)]
    public class dynPow : dynBuiltinFunction
    {
        public dynPow()
            : base("pow")
        {
            InPortData.Add(new PortData("x", "operand", typeof(double)));
            InPortData.Add(new PortData("y", "operand", typeof(double)));
            OutPortData = new PortData("x^y", "result", typeof(double));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Round")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Rounds a number to the nearest integer value.")]
    [RequiresTransaction(false)]
    public class dynRound : dynNode
    {
        public dynRound()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));
            OutPortData = new PortData("int", "Rounded number", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Round(((Expression.Number)args[0]).Item)
            );
        }
    }

    [ElementName("Floor")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Rounds a number to the nearest smaller integer.")]
    [ElementSearchTags("round")]
    [RequiresTransaction(false)]
    public class dynFloor : dynNode
    {
        public dynFloor()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));
            OutPortData = new PortData("int", "Number rounded down", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Floor(((Expression.Number)args[0]).Item)
            );
        }
    }

    [ElementName("Ceiling")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Rounds a number to the nearest larger integer value.")]
    [ElementSearchTags("round")]
    [RequiresTransaction(false)]
    public class dynCeiling : dynNode
    {
        public dynCeiling()
        {
            InPortData.Add(new PortData("dbl", "A number", typeof(double)));
            OutPortData = new PortData("int", "Number rounded up", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewNumber(
               Math.Ceiling(((Expression.Number)args[0]).Item)
            );
        }
    }

    [ElementName("Random")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Generates a uniform random number in the range [0.0, 1.0).")]
    [RequiresTransaction(false)]
    public class dynRandom : dynNode
    {
        public dynRandom()
        {
            OutPortData = new PortData("rand", "Random number between 0.0 and 1.0.", typeof(double));

            base.RegisterInputsAndOutputs();
        }

        private static Random random = new Random();

        public override bool IsDirty
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

    [ElementName("π")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Pi constant")]
    [RequiresTransaction(false)]
    [ElementSearchTags("pi", "trigonometry", "circle")]
    [IsInteractive(false)]
    public class dynPi : dynNode
    {
        public dynPi()
        {
            OutPortData = new PortData("3.14159...", "pi", typeof(double));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }

        public override bool IsDirty
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

    [ElementName("Sine")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Computes the sine of the given angle.")]
    [RequiresTransaction(false)]
    public class dynSin : dynNode
    {
        public dynSin()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));
            OutPortData = new PortData("sin(θ)", "Sine value of the given angle", typeof(double));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Cosine")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Computes the cosine of the given angle.")]
    [RequiresTransaction(false)]
    public class dynCos : dynNode
    {
        public dynCos()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));
            OutPortData = new PortData("cos(θ)", "Cosine value of the given angle", typeof(double));

            base.RegisterInputsAndOutputs();
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

    [ElementName("Tangent")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Computes the tangent of the given angle.")]
    [RequiresTransaction(false)]
    public class dynTan : dynNode
    {
        public dynTan()
        {
            InPortData.Add(new PortData("θ", "Angle in radians", typeof(double)));
            OutPortData = new PortData("tan(θ)", "Tangent value of the given angle", typeof(double));

            base.RegisterInputsAndOutputs();
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
    [ElementName("Perform All")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Executes expressions in a sequence")]
    [ElementSearchTags("begin")]
    [RequiresTransaction(false)]
    public class dynBegin : dynVariableInput
    {
        public dynBegin()
        {
            InPortData.Add(new PortData("expr1", "Expression #1", typeof(object)));
            InPortData.Add(new PortData("expr2", "Expression #2", typeof(object)));
            OutPortData = new PortData("last", "Result of final expression", typeof(object));

            base.RegisterInputsAndOutputs();
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
    [ElementName("Apply")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Applies arguments to a function")]
    [RequiresTransaction(false)]
    public class dynApply1 : dynVariableInput
    {
        public dynApply1()
        {
            InPortData.Add(new PortData("func", "Procedure", typeof(object)));
            OutPortData = new PortData("result", "Result", typeof(object));

            base.RegisterInputsAndOutputs();
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
            base.ReregisterInputs();
        }
    }

    //TODO: Setup proper IsDirty smart execution management
    [ElementName("If")]
    [ElementCategory(BuiltinElementCategories.BOOLEAN)]
    [ElementDescription("Conditional statement")]
    [RequiresTransaction(false)]
    public class dynConditional : dynNode
    {
        public dynConditional()
        {
            InPortData.Add(new PortData("test", "Test block", typeof(bool)));
            InPortData.Add(new PortData("true", "True block", typeof(object)));
            InPortData.Add(new PortData("false", "False block", typeof(object)));

            OutPortData = new PortData("result", "Result", typeof(object));

            this.nickNameBlock.FontSize = 20;

            base.RegisterInputsAndOutputs();
        }

        protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        {
            return new ConditionalNode();
        }
    }

    [ElementName("Debug Breakpoint")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [ElementDescription("Halts execution until user clicks button.")]
    [RequiresTransaction(false)]
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
            inputGrid.Children.Add(button);
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetRow(button, 0);
            button.Content = "Continue";

            this.enabled = false;

            button.Click += new RoutedEventHandler(button_Click);

            InPortData.Add(new PortData("", "Object to inspect", typeof(object)));
            OutPortData = new PortData("", "Object inspected", typeof(object));

            base.RegisterInputsAndOutputs();
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
            this.Deselect();
            enabled = false;
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var result = args[0];

            this.Dispatcher.Invoke(new Action(
               delegate
               {
                   dynElementSettings.SharedInstance.Bench.Log(FScheme.print(result));
               }
            ));

            if (dynElementSettings.SharedInstance.Bench.RunInDebug)
            {
                button.Dispatcher.Invoke(new Action(
                   delegate
                   {
                       enabled = true;
                       this.Select();
                       dynElementSettings.SharedInstance.Bench.ShowElement(this);
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
    [ElementName("Optimizer")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Evaluates one input against another and passes out the larger of the two values.")]
    [RequiresTransaction(false)]
    public class dynOptimizer : dynNode
    {
        TextBox tb;

        public dynOptimizer()
        {
            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0.0";
            //tb.IsReadOnly = true;
            //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);
            tb.TextChanged += delegate { this.IsDirty = true; };

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            InPortData.Add(new PortData("N", "New Value", typeof(double)));
            //InPortData.Add(new PortData(null, "I", "Initial Value", typeof(dynDouble)));


            //outport declared in the abstract

            OutPortData = new PortData("dbl", "The larger value of the input vs. the current value", typeof(double));

            base.RegisterInputsAndOutputs();
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
    [ElementName("Incrementer")]
    [ElementCategory(BuiltinElementCategories.MATH)]
    [ElementDescription("Watches one input then if that changes, increments the output integer until it hits a max value.")]
    [RequiresTransaction(false)]
    public class dynIncrementer : dynNode
    {
        TextBox tb;

        public dynIncrementer()
        {
            //add a text box to the input grid of the control
            tb = new System.Windows.Controls.TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "0";
            tb.TextChanged += delegate { this.IsDirty = true; };
            //tb.IsReadOnly = true;
            //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            InPortData.Add(new PortData("m", "Max Iterations", typeof(double)));
            InPortData.Add(new PortData("v", "Value", typeof(double)));

            OutPortData = new PortData("v", "Value", typeof(double));

            base.RegisterInputsAndOutputs();

            //OutPortData[0].Object = numIterations;
            //OutPortData[1].Object = currentValue;
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
            return !dynElementSettings.SharedInstance.Bench.DynamicRunEnabled;
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
                    this.IsDirty = value != null;
                }
            }
        }

        protected abstract T DeserializeValue(string val);

        public dynBasicInteractive()
        {
            Type type = typeof(T);
            OutPortData = new PortData("", type.Name, type);

            //add an edit window option to the 
            //main context window
            System.Windows.Controls.MenuItem editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;

            this.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
        }

        public virtual void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            //override in child classes
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
            this.Value = this.DeserializeValue(editWindow.editText.Text);
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

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            return Expression.NewString(this.Value);
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
            this.Value = this.DeserializeValue(editWindow.editText.Text);
        }
    }

    #endregion

    [ElementName("Number")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("Creates a number.")]
    [RequiresTransaction(false)]
    public class dynDoubleInput : dynDouble
    {
        //dynTextBox tb;
        TextBlock nodeLabel;

        public dynDoubleInput()
        {
            //add a text box to the input grid of the control
            /*tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = true;
            tb.Text = "0.0";
            tb.OnChangeCommitted += delegate { this.Value = this.DeserializeValue(this.tb.Text); };
            */

            nodeLabel = new System.Windows.Controls.TextBlock();
            nodeLabel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            nodeLabel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            nodeLabel.Text = "0.0";
            nodeLabel.FontSize = 24;

            inputGrid.Children.Add(nodeLabel);
            System.Windows.Controls.Grid.SetColumn(nodeLabel, 0);
            System.Windows.Controls.Grid.SetRow(nodeLabel, 0);

            base.RegisterInputsAndOutputs();

            //take out the left and right margins
            //and make this so it's not so wide
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
            this.topControl.Width = 100;
            this.topControl.Height = 50;

            this.UpdateLayout();
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
                //this.tb.Text = value.ToString();
                this.nodeLabel.Text = dynUtils.Ellipsis(value.ToString(), 5);
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
    [ElementName("Number Slider")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("Creates a number, but using SLIDERS!.")]
    [RequiresTransaction(false)]
    public class dynDoubleSliderInput : dynDouble
    {
        Slider tb_slider;
        dynTextBox mintb;
        dynTextBox maxtb;
        TextBox displayBox;

        public dynDoubleSliderInput()
        {
            //add a slider control to the input grid of the control
            tb_slider = new System.Windows.Controls.Slider();
            tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb_slider);
            System.Windows.Controls.Grid.SetColumn(tb_slider, 1);
            System.Windows.Controls.Grid.SetRow(tb_slider, 0);
            tb_slider.Value = 0.0;
            tb_slider.Maximum = 100.0;
            tb_slider.Minimum = 0.0;
            tb_slider.Ticks = new System.Windows.Media.DoubleCollection(10);
            tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
            tb_slider.ValueChanged += delegate
            {
                this.Value = this.tb_slider.Value;

                var pos = Mouse.GetPosition(elementCanvas);
                Canvas.SetLeft(displayBox, pos.X);
            };

            tb_slider.PreviewMouseDown += delegate
            {
                if (this.IsEnabled && !elementCanvas.Children.Contains(displayBox))
                {
                    elementCanvas.Children.Add(displayBox);

                    var pos = Mouse.GetPosition(elementCanvas);
                    Canvas.SetLeft(displayBox, pos.X);
                }
            };

            tb_slider.PreviewMouseUp += delegate
            {
                if (elementCanvas.Children.Contains(displayBox))
                    elementCanvas.Children.Remove(displayBox);
            };

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

            this.SetColumnAmount(3);
            inputGrid.Children.Add(mintb);
            inputGrid.Children.Add(maxtb);

            //make the middle column containing the slider
            //take up most of the width
            inputGrid.ColumnDefinitions[1].Width = new GridLength(.75 * this.topControl.Width);

            System.Windows.Controls.Grid.SetColumn(mintb, 0);
            System.Windows.Controls.Grid.SetColumn(maxtb, 2);

            base.RegisterInputsAndOutputs();

            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);


            displayBox = new TextBox()
            {
                IsReadOnly = true,
                Background = Brushes.White,
                Foreground = Brushes.Black
            };
            Canvas.SetTop(displayBox, this.Height);
            Canvas.SetZIndex(displayBox, int.MaxValue);

            var binding = new System.Windows.Data.Binding("Value")
            {
                Source = tb_slider,
                Mode = System.Windows.Data.BindingMode.OneWay,
                Converter = new DoubleDisplay()
            };
            displayBox.SetBinding(TextBox.TextProperty, binding);
        }

        #region Value Conversion
        [ValueConversion(typeof(double), typeof(String))]
        private class DoubleDisplay : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((double)value).ToString("F4");
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }
        #endregion

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

    [ElementName("Boolean")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("Selection between a true and false.")]
    [ElementSearchTags("true", "truth", "false")]
    [RequiresTransaction(false)]
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
            inputGrid.ColumnDefinitions.Add(cd1);
            inputGrid.ColumnDefinitions.Add(cd2);
            inputGrid.RowDefinitions.Add(rd);

            inputGrid.Children.Add(rbTrue);
            inputGrid.Children.Add(rbFalse);

            System.Windows.Controls.Grid.SetColumn(rbTrue, 0);
            System.Windows.Controls.Grid.SetRow(rbTrue, 0);
            System.Windows.Controls.Grid.SetColumn(rbFalse, 1);
            System.Windows.Controls.Grid.SetRow(rbFalse, 0);

            rbFalse.IsChecked = true;
            rbTrue.Checked += new System.Windows.RoutedEventHandler(rbTrue_Checked);
            rbFalse.Checked += new System.Windows.RoutedEventHandler(rbFalse_Checked);
            //OutPortData[0].Object = false;

            base.RegisterInputsAndOutputs();
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

    [ElementName("String")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("Creates a string.")]
    [RequiresTransaction(false)]
    public class dynStringInput : dynString
    {
        //dynTextBox tb;
        TextBlock tb;

        public dynStringInput()
        {
            //add a text box to the input grid of the control
            //tb = new dynTextBox();
            tb = new TextBlock();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "";

            //tb.OnChangeCommitted += delegate { this.Value = this.tb.Text; };

            base.RegisterInputsAndOutputs();

            //remove the margins
            this.inputGrid.Margin = new Thickness(10, 5, 10, 5);
        }

        public override string Value
        {
            set
            {
                if (base.Value == value)
                    return;

                base.Value = value;

                this.tb.Text = dynUtils.Ellipsis(this.Value, 30);
            }
        }

        /*
        void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Value = this.tb.Text;
        }

        void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Keys.Enter))
                this.Value = this.tb.Text;
        }*/

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", System.Web.HttpUtility.UrlEncode(this.Value.ToString()));
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
                            this.Value = this.DeserializeValue(System.Web.HttpUtility.UrlDecode(attr.Value));
                            this.tb.Text = dynUtils.Ellipsis(this.Value, 30);
                        }

                    }
                }
            }
        }
    }

    [ElementName("Filename")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("Allows you to select a file on the system to get its filename.")]
    [RequiresTransaction(false)]
    public class dynStringFilename : dynBasicInteractive<string>
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

            this.SetRowAmount(2);

            this.inputGrid.Children.Add(tb);
            this.inputGrid.Children.Add(readFileButton);

            System.Windows.Controls.Grid.SetRow(readFileButton, 0);
            System.Windows.Controls.Grid.SetRow(tb, 1);

            base.RegisterInputsAndOutputs();

            this.topControl.Height = 60;
            this.UpdateLayout();
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

            return Expression.NewString(this.Value);
        }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }
    }

    #endregion

    #region Strings and Conversions

    [ElementName("Concatenate Strings")]
    [ElementDescription("Concatenates two or more strings")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [RequiresTransaction(false)]
    public class dynConcatStrings : dynVariableInput
    {
        public dynConcatStrings()
        {
            InPortData.Add(new PortData("s1", "First string", typeof(object)));
            InPortData.Add(new PortData("s2", "Second string", typeof(object)));
            OutPortData = new PortData("combined", "Combined lists", typeof(object));

            base.RegisterInputsAndOutputs();
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
            base.ReregisterInputs();
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

    [ElementName("String -> Number")]
    [ElementDescription("Converts a string to a number")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [RequiresTransaction(false)]
    public class dynString2Num : dynBuiltinFunction
    {
        public dynString2Num()
            : base("string->num")
        {
            this.InPortData.Add(new PortData("s", "A string", typeof(string)));
            this.OutPortData = new PortData("n", "A number", typeof(double));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Number -> String")]
    [ElementDescription("Converts a number to a string")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    [RequiresTransaction(false)]
    public class dynNum2String : dynBuiltinFunction
    {
        public dynNum2String()
            : base("num->string")
        {
            this.InPortData.Add(new PortData("n", "A number", typeof(double)));
            this.OutPortData = new PortData("s", "A string", typeof(string));

            base.RegisterInputsAndOutputs();
        }
    }

    [ElementName("Split String")]
    [ElementDescription("Splits given string around given delimiter into a list of sub strings.")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynSplitString : dynNode
    {
        public dynSplitString()
        {
            InPortData.Add(new PortData("str", "String to split", typeof(string)));
            InPortData.Add(new PortData("del", "Delimiter", typeof(string)));
            OutPortData = new PortData("strs", "List of split strings", typeof(IList<string>));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string str = ((Expression.String)args[0]).Item;
            string del = ((Expression.String)args[1]).Item;

            return Expression.NewList(
                Utils.convertSequence(
                    str.Split(new string[] { del }, StringSplitOptions.None)
                       .Select(Expression.NewString)
                )
            );
        }
    }

    [ElementName("Join Strings")]
    [ElementDescription("Joins the given list of strings around the given delimiter.")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynJoinStrings : dynNode
    {
        public dynJoinStrings()
        {
            InPortData.Add(new PortData("strs", "List of strings to join.", typeof(IList<string>)));
            InPortData.Add(new PortData("del", "Delimier", typeof(string)));
            OutPortData = new PortData("str", "Joined string", typeof(string));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var strs = ((Expression.List)args[0]).Item;
            var del = ((Expression.String)args[1]).Item;

            return Expression.NewString(
                string.Join(del, strs.Select(x => ((Expression.String)x).Item))
            );
        }
    }

    [ElementName("String Case")]
    [ElementDescription("Converts a string to uppercase or lowercase")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynStringCase : dynNode
    {
        public dynStringCase()
        {
            InPortData.Add(new PortData("str", "String to convert", typeof(string)));
            InPortData.Add(new PortData("upper?", "True = Uppercase, False = Lowercase", typeof(bool)));
            OutPortData = new PortData("s", "Converted string", typeof(string));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string s = ((Expression.String)args[0]).Item;
            bool upper = ((Expression.Number)args[1]).Item == 1.0;

            return Expression.NewString(
                upper ? s.ToUpper() : s.ToLower()
            );
        }
    }

    [ElementName("Substring")]
    [ElementDescription("Gets a substring of a given string")]
    [ElementCategory(BuiltinElementCategories.MISC)]
    public class dynSubstring : dynNode
    {
        public dynSubstring()
        {
            InPortData.Add(new PortData("str", "String to take substring from", typeof(string)));
            InPortData.Add(new PortData("start", "Starting index of substring", typeof(double)));
            InPortData.Add(new PortData("length", "Length of substring", typeof(double)));
            OutPortData = new PortData("sub", "Substring", typeof(string));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            string s = ((Expression.String)args[0]).Item;
            double start = ((Expression.Number)args[1]).Item;
            double length = ((Expression.Number)args[2]).Item;

            return Expression.NewString(s.Substring((int)start, (int)length));
        }
    }

    #endregion
}

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
using Autodesk.Revit.UI;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;

namespace Dynamo.Elements
{
   public static class BuiltinElementCategories
   {
      public const string MATH = "Math";
      public const string COMPARISON = "Comparison";
      public const string BOOLEAN = "Boolean";
      public const string PRIMITIVES = "Primitives";
      public const string REVIT = "Revit";
      public const string MISC = "Miscellaneous";
      public const string LIST = "Lists";
   }

   #region FScheme Builtin Interop

   public abstract class dynBuiltinFunction : dynElement
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

   public abstract class dynVariableInput : dynElement
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

   [ElementName("ident")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Identity function")]
   [RequiresTransaction(false)]
   public class dynIdentity : dynElement
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

   [ElementName("list")]
   [ElementDescription("Makes a new list out of the given inputs")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [RequiresTransaction(false)]
   public class dynNewList : dynVariableInput
   {
      public dynNewList()
      {
         OutPortData = new PortData("list", "A list", typeof(object));

         base.RegisterInputsAndOutputs();
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

   [ElementName("sort-with")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Returns a sorted list, using the given comparitor.")]
   [RequiresTransaction(false)]
   public class dynSortWith : dynBuiltinFunction
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

   [ElementName("sort-by")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Returns a sorted list, using the given key mapper.")]
   [RequiresTransaction(false)]
   public class dynSortBy : dynBuiltinFunction
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

   [ElementName("sort")]
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

   [ElementName("fold")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Reduces a sequence.")]
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

   [ElementName("filter")]
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

   [ElementName("build-sequence")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Creates a sequence of numbers")]
   [RequiresTransaction(false)]
   public class dynBuildSeq : dynBuiltinMacro
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

   [ElementName("combine")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Applies a combinator to each element in two sequences")]
   [ElementSearchTags("zip")]
   [RequiresTransaction(false)]
   public class dynCombine : dynBuiltinMacro
   {
      public dynCombine()
         : base("combine")
      {
         InPortData.Add(new PortData("f(A, B)", "Combinator", typeof(object)));
         InPortData.Add(new PortData("listA", "First list", typeof(object)));
         InPortData.Add(new PortData("listB", "Second list", typeof(object)));
         OutPortData = new PortData("[f(A,B)]", "Combined lists", typeof(object));

         base.RegisterInputsAndOutputs();
      }
   }

   [ElementName("map")]
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

   [ElementName("cons")]
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

   [ElementName("take")]
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

   [ElementName("drop")]
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

   [ElementName("get")]
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

   [ElementName("empty")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("An empty list")]
   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynEmpty : dynElement
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

   [ElementName("isEmpty?")]
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

   [ElementName("len")]
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

   [ElementName("append")]
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

   [ElementName("first")]
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

   [ElementName("rest")]
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

   [ElementName("and")]
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
         OutPortData = new PortData("a ^ b", "result", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }
   }

   [ElementName("or")]
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
         OutPortData = new PortData("a V b", "result", typeof(bool));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }
   }

   [ElementName("not")]
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

   [ElementName("random")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Generates a uniform random number in the range [0.0, 1.0).")]
   [RequiresTransaction(false)]
   public class dynRandom : dynElement
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
   public class dynPi : dynElement
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

   [ElementName("sine")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Computes the sine of the given angle.")]
   [RequiresTransaction(false)]
   public class dynSin : dynElement
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

   [ElementName("cosine")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Computes the cosine of the given angle.")]
   [RequiresTransaction(false)]
   public class dynCos : dynElement
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

   [ElementName("tangent")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Computes the tangent of the given angle.")]
   [RequiresTransaction(false)]
   public class dynTan : dynElement
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
   [ElementName("begin")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Executes expressions in a sequence")]
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
   [ElementName("apply")]
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
   [ElementName("if")]
   [ElementCategory(BuiltinElementCategories.BOOLEAN)]
   [ElementDescription("Conditional statement")]
   [RequiresTransaction(false)]
   public class dynConditional : dynElement
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
   public class dynBreakpoint : dynElement
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
   [ElementDescription("An element which evaluates one inpute against another and passes out the larger of the two values.")]
   [RequiresTransaction(false)]
   public class dynOptimizer : dynElement
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
   [ElementDescription("An element which watches one input then if that changes, increments the output integer until it hits a max value.")]
   [RequiresTransaction(false)]
   public class dynIncrementer : dynElement
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
      public event Action OnCommit;

      public dynTextBox()
      {
         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         Background = backgroundBrush;
         BorderThickness = new Thickness(0);
      }

      protected override void OnTextChanged(TextChangedEventArgs e)
      {
         if (!dynElementSettings.SharedInstance.Bench.DynamicRunEnabled && this.OnCommit != null)
            this.OnCommit();
      }

      protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
      {
         if (e.Key == System.Windows.Input.Key.Return || e.Key == System.Windows.Input.Key.Enter)
         {
            if (this.OnCommit != null)
               this.OnCommit();
         }
      }

      protected override void OnLostFocus(RoutedEventArgs e)
      {
         if (this.OnCommit != null)
            this.OnCommit();
      }
   }
   
   [IsInteractive(true)]
   public abstract class dynBasicInteractive<T> : dynElement
   {
      private T _value = default(T);
      protected virtual T Value 
      {
         get
         {
            return this._value;
         }
         set
         {
            if (this._value == null || !this._value.Equals(value))
            {
               this.IsDirty = value != null;
               this._value = value;
            }
         }
      }

      protected abstract T DeserializeValue(string val);

      public dynBasicInteractive()
      {
         Type type = typeof(T);
         OutPortData = new PortData("", type.Name, type);
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

   [ElementName("Number")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("An element which creates an unsigned floating point number.")]
   [RequiresTransaction(false)]
   public class dynDoubleInput : dynDouble
   {
      dynTextBox tb;

      public dynDoubleInput()
      {
         //add a text box to the input grid of the control
         tb = new dynTextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "0.0";

         tb.OnCommit += delegate { this.Value = this.DeserializeValue(this.tb.Text); };

         base.RegisterInputsAndOutputs();
      }

      protected override double Value
      {
         get
         {
            return base.Value;
         }
         set
         {
            base.Value = value;
            this.tb.Text = value.ToString();
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
   [ElementDescription("An element which creates an unsigned floating point number, but using SLIDERS!.")]
   [RequiresTransaction(false)]
   public class dynDoubleSliderInput : dynDouble
   {
      Slider tb_slider;
      dynTextBox mintb;
      dynTextBox maxtb;

      public dynDoubleSliderInput()
      {
         mintb = new dynTextBox();
         mintb.MaxLength = 3;
         mintb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         mintb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         mintb.Width = double.NaN;
         mintb.Text = "0";
         
         mintb.OnCommit += delegate 
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

         maxtb = new dynTextBox();
         maxtb.MaxLength = 3;
         maxtb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         maxtb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         maxtb.Width = double.NaN;
         maxtb.Text = "100";
         maxtb.OnCommit += delegate 
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

         this.SetColumnAmount(3);
         inputGrid.Children.Add(mintb);
         inputGrid.Children.Add(maxtb);

         System.Windows.Controls.Grid.SetColumn(mintb, 0);
         System.Windows.Controls.Grid.SetColumn(maxtb, 2);

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
         tb_slider.ValueChanged += delegate { this.Value = this.tb_slider.Value; };
         
         base.RegisterInputsAndOutputs();
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

      protected override double Value
      {
         set
         {
            base.Value = value;
            this.tb_slider.Value = value;
         }
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
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
                     this.tb_slider.Minimum = Convert.ToDouble(attr.Value);
                     this.mintb.Text = attr.Value;
                  }
                  else if (attr.Name.Equals("max"))
                  {
                     this.tb_slider.Maximum = Convert.ToDouble(attr.Value);
                     this.maxtb.Text = attr.Value;
                  }
               }
            }
         }
      }
   }

   [ElementName("Boolean")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("An element which allows selection between a true and false.")]
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

      protected override bool Value
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
   [ElementDescription("An element which creates a string value.")]
   [RequiresTransaction(false)]
   public class dynStringInput : dynString
   {
      dynTextBox tb;

      public dynStringInput()
      {
         //add a text box to the input grid of the control
         tb = new dynTextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "";

         tb.OnCommit += delegate { this.Value = this.tb.Text; };

         base.RegisterInputsAndOutputs();
      }

      protected override string Value
      {
         set
         {
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

   [ElementName("Filename")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("Allows you to select a file on the system to get its filename.")]
   [RequiresTransaction(false)]
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

         this.SetRowAmount(2);

         this.inputGrid.Children.Add(tb);
         this.inputGrid.Children.Add(readFileButton);

         System.Windows.Controls.Grid.SetRow(readFileButton, 0);
         System.Windows.Controls.Grid.SetRow(tb, 1);

         base.RegisterInputsAndOutputs();

         this.topControl.Height = 60;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
      }

      protected override string Value
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
}


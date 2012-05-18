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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml;
using Autodesk.Revit.DB;
//MDJ - i think this is needed for DMU stuff
using Autodesk.Revit.DB.Analysis; //MDJ  - added for spatialfeildmanager access
using Autodesk.Revit.UI;
using Coding4Fun.Kinect.Wpf;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.Research.Kinect.Nui;
using Expression = Dynamo.FScheme.Expression;
using TextBox = System.Windows.Controls.TextBox;
using System.IO.Ports;

namespace Dynamo.Elements
{

   #region interfaces
   public interface IDynamic
   {
      void Draw();
      void Destroy();
   }
   #endregion

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

   public abstract class dynBasicInteractive<T> : dynElement
   {
      protected abstract T Value { get; }

      protected abstract void DeserializeValue(string val);

      public dynBasicInteractive()
      {
         Type type = typeof(T);
         OutPortData = new PortData(null, "", type.Name, type);
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
            if (subNode.Name == typeof(T).FullName)
            {
               this.DeserializeValue(subNode.Attributes[0].Value);
            }
         }
      }
   }

   public abstract class dynDouble : dynBasicInteractive<double>
   {
      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewNumber(
            (double)this.Dispatcher.Invoke(new Func<double>(
               delegate { return this.Value; }
            ))
         );
      }
   }

   public abstract class dynBool : dynBasicInteractive<bool>
   {
      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewNumber(
            (bool)this.Dispatcher.Invoke(new Func<bool>(
               delegate { return this.Value; }
            )) ? 1 : 0
         );
      }
   }

   public abstract class dynString : dynBasicInteractive<string>
   {
      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewString(
            (string)this.Dispatcher.Invoke(new Func<string>(
               delegate { return this.Value; }
            ))
         );
      }
   }

   public class dynAction : dynElement
   {
      public dynAction()
      {
         InPortData.Add(new PortData(null, "act", "The action to perform.", typeof(dynAction)));
      }

      public virtual void PerformAction()
      {

      }

      public override void Draw()
      {
         base.Draw();
      }

      public override void Destroy()
      {

         base.Destroy();
      }

      public override void Update()
      {
         //raise the event for the base class
         //to build, sending this as the 
         OnDynElementReadyToBuild(EventArgs.Empty);
      }
   }

   #region SJE

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
         InPortData.Add(new PortData(null, this.getInputRootName() + this.getNewInputIndex(), "", typeof(object)));
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
               this.InPortData.Add(new PortData(null, subNode.Attributes["name"].Value, "", typeof(object)));
            }
         }
         base.ReregisterInputs();
      }

      protected override void OnEvaluate()
      {
         this.lastEvaledAmt = this.InPortData.Count;
      }
   }

   [ElementName("list")]
   [ElementDescription("Makes a new list out of the given inputs")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [RequiresTransaction(false)]
   public class dynNewList : dynVariableInput
   {
      public dynNewList()
      {
         OutPortData = new PortData(null, "list", "A list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected override string getInputRootName()
      {
         return "index";
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("list", portNames);
      }
   }

   public abstract class dynComparison : dynElement
   {
      private string op;

      protected dynComparison(string op) : this(op, op) { }

      protected dynComparison(string op, string name)
      {
         InPortData.Add(new PortData(null, "x", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "y", "operand", typeof(double)));
         OutPortData = new PortData(null, "x" + name + "y", "comp", typeof(double));

         this.op = op;

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode(this.op, portNames);
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
   public class dynAnd : dynElement
   {
      public dynAnd()
      {
         InPortData.Add(new PortData(null, "a", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "b", "operand", typeof(double)));
         OutPortData = new PortData(null, "a ^ b", "result", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("and", portNames);
      }
   }

   [ElementName("or")]
   [ElementCategory(BuiltinElementCategories.BOOLEAN)]
   [ElementDescription("Boolean OR.")]
   [RequiresTransaction(false)]
   public class dynOr : dynElement
   {
      public dynOr()
      {
         InPortData.Add(new PortData(null, "a", "operand", typeof(bool)));
         InPortData.Add(new PortData(null, "b", "operand", typeof(bool)));
         OutPortData = new PortData(null, "a V b", "result", typeof(bool));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("or", portNames);
      }
   }

   [ElementName("not")]
   [ElementCategory(BuiltinElementCategories.BOOLEAN)]
   [ElementDescription("Boolean NOT.")]
   [RequiresTransaction(false)]
   public class dynNot : dynElement
   {
      public dynNot()
      {
         InPortData.Add(new PortData(null, "a", "operand", typeof(bool)));
         OutPortData = new PortData(null, "!a", "result", typeof(bool));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("not", portNames);
      }
   }

   [ElementName("+")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Adds two numbers.")]
   [ElementSearchTags("plus", "addition", "sum")]
   [RequiresTransaction(false)]
   public class dynAddition : dynElement
   {
      public dynAddition()
      {
         InPortData.Add(new PortData(null, "x", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "y", "operand", typeof(double)));
         OutPortData = new PortData(null, "x+y", "sum", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("+", portNames);
      }
   }

   [ElementName("−")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Subtracts two numbers.")]
   [ElementSearchTags("subtraction", "minus", "difference", "-")]
   [RequiresTransaction(false)]
   public class dynSubtraction : dynElement
   {
      public dynSubtraction()
      {
         InPortData.Add(new PortData(null, "x", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "y", "operand", typeof(double)));
         OutPortData = new PortData(null, "x-y", "difference", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("-", portNames);
      }
   }

   [ElementName("×")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Multiplies two numbers.")]
   [ElementSearchTags("times", "multiply", "multiplication", "product", "*", "x")]
   [RequiresTransaction(false)]
   public class dynMultiplication : dynElement
   {
      public dynMultiplication()
      {
         InPortData.Add(new PortData(null, "x", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "y", "operand", typeof(double)));
         OutPortData = new PortData(null, "x∙y", "product", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("*", portNames);
      }
   }

   [ElementName("÷")]
   [ElementCategory(BuiltinElementCategories.MATH)]
   [ElementDescription("Divides two numbers.")]
   [ElementSearchTags("divide", "division", "quotient", "/")]
   [RequiresTransaction(false)]
   public class dynDivision : dynElement
   {
      public dynDivision()
      {
         InPortData.Add(new PortData(null, "x", "operand", typeof(double)));
         InPortData.Add(new PortData(null, "y", "operand", typeof(double)));
         OutPortData = new PortData(null, "x÷y", "result", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("/", portNames);
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
         OutPortData = new PortData(null, "rand", "Random number between 0.0 and 1.0.", typeof(double));

         base.RegisterInputsAndOutputs();
      }

      private static Random random = new Random();

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
         OutPortData = new PortData(null, "3.14159...", "pi", typeof(double));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      //public override Expression Evaluate(FSharpList<Expression> args)
      //{
      //   return Expression.NewNumber(Math.PI);
      //}

      protected internal override INode Build()
      {
         return new NumberNode(Math.PI);
      }
   }

   [ElementName("sort-with")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Returns a sorted list, using the given comparitor.")]
   [RequiresTransaction(false)]
   public class dynSortWith : dynElement
   {
      public dynSortWith()
      {
         InPortData.Add(new PortData(null, "list", "List to sort", typeof(object)));
         InPortData.Add(new PortData(null, "c(x, y)", "Comparitor", typeof(object)));

         OutPortData = new PortData(null, "sorted", "Sorted list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("sort", portNames);
      }
   }

   [ElementName("sort-by")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Returns a sorted list, using the given key mapper.")]
   [RequiresTransaction(false)]
   public class dynSortBy : dynElement
   {
      public dynSortBy()
      {
         InPortData.Add(new PortData(null, "list", "List to sort", typeof(object)));
         InPortData.Add(new PortData(null, "c(x)", "Key Mapper", typeof(object)));

         OutPortData = new PortData(null, "sorted", "Sorted list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("sort-by", portNames);
      }
   }

   [ElementName("sort")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Returns a sorted list of numbers or strings.")]
   [RequiresTransaction(false)]
   public class dynSort : dynElement
   {
      public dynSort()
      {
         InPortData.Add(new PortData(null, "list", "List of numbers or strings to sort", typeof(object)));

         OutPortData = new PortData(null, "sorted", "Sorted list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("sort", portNames);
      }
   }

   [ElementName("fold")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Reduces a sequence.")]
   [RequiresTransaction(false)]
   public class dynFold : dynElement
   {
      public dynFold()
      {
         InPortData.Add(new PortData(null, "f(x, a)", "Reductor Funtion", typeof(object)));
         InPortData.Add(new PortData(null, "a", "Seed", typeof(object)));
         InPortData.Add(new PortData(null, "seq", "Sequence", typeof(object)));
         OutPortData = new PortData(null, "out", "Result", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("fold", portNames);
      }
   }

   [ElementName("filter")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Filters a sequence by a given predicate")]
   [RequiresTransaction(false)]
   public class dynFilter : dynElement
   {
      public dynFilter()
      {
         InPortData.Add(new PortData(null, "p(x)", "Predicate", typeof(object)));
         InPortData.Add(new PortData(null, "seq", "Sequence to filter", typeof(object)));
         OutPortData = new PortData(null, "filtered", "Filtered Sequence", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("filter", portNames);
      }
   }

   [ElementName("build-sequence")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Creates a sequence of numbers")]
   [RequiresTransaction(false)]
   public class dynBuildSeq : dynElement
   {
      public dynBuildSeq()
      {
         InPortData.Add(new PortData(null, "start", "Number to start the sequence at", typeof(double)));
         InPortData.Add(new PortData(null, "end", "Number to end the sequence at", typeof(double)));
         InPortData.Add(new PortData(null, "step", "Space between numbers", typeof(double)));
         OutPortData = new PortData(null, "seq", "New sequence", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("build-seq", portNames);
      }
   }

   [ElementName("combine")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Applies a combinator to each element in two sequences")]
   [ElementSearchTags("zip")]
   [RequiresTransaction(false)]
   public class dynCombine : dynElement
   {
      public dynCombine()
      {
         InPortData.Add(new PortData(null, "f(A, B)", "Combinator", typeof(object)));
         InPortData.Add(new PortData(null, "listA", "First list", typeof(object)));
         InPortData.Add(new PortData(null, "listB", "Second list", typeof(object)));
         OutPortData = new PortData(null, "[f(A,B)]", "Combined lists", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("combine", portNames);
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
         InPortData.Add(new PortData(null, "θ", "Angle in radians", typeof(double)));
         OutPortData = new PortData(null, "sin(θ)", "Sine value of the given angle", typeof(double));

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
         InPortData.Add(new PortData(null, "θ", "Angle in radians", typeof(double)));
         OutPortData = new PortData(null, "cos(θ)", "Cosine value of the given angle", typeof(double));

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

   [ElementName("map")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Maps a sequence")]
   [RequiresTransaction(false)]
   public class dynMap : dynElement
   {
      public dynMap()
      {
         InPortData.Add(new PortData(null, "f(x)", "The procedure used to map elements", typeof(object)));
         InPortData.Add(new PortData(null, "seq", "The sequence to map over.", typeof(object)));
         OutPortData = new PortData(null, "mapped", "Mapped sequence", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("map", portNames);
      }
   }

   [ElementName("cons")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Creates a pair")]
   [RequiresTransaction(false)]
   public class dynList : dynElement
   {
      public dynList()
      {
         InPortData.Add(new PortData(null, "first", "The new Head of the list", typeof(object)));
         InPortData.Add(new PortData(null, "rest", "The new Tail of the list", typeof(object)));
         OutPortData = new PortData(null, "list", "Result List", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("cons", portNames);
      }
   }

   [ElementName("take")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Takes elements from a list")]
   [RequiresTransaction(false)]
   public class dynTakeList : dynElement
   {
      public dynTakeList()
      {
         InPortData.Add(new PortData(null, "amt", "Amount of elements to extract", typeof(object)));
         InPortData.Add(new PortData(null, "list", "The list to extract elements from", typeof(object)));
         OutPortData = new PortData(null, "elements", "List of extraced elements", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("take", portNames);
      }
   }


   [ElementName("drop")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Drops elements from a list")]
   [RequiresTransaction(false)]
   public class dynDropList : dynElement
   {
      public dynDropList()
      {
         InPortData.Add(new PortData(null, "amt", "Amount of elements to drop", typeof(object)));
         InPortData.Add(new PortData(null, "list", "The list to drop elements from", typeof(object)));
         OutPortData = new PortData(null, "elements", "List of remaining elements", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("drop", portNames);
      }
   }


   [ElementName("get")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Gets an element from a list at a specified index.")]
   [RequiresTransaction(false)]
   public class dynGetFromList : dynElement
   {
      public dynGetFromList()
      {
         InPortData.Add(new PortData(null, "index", "Index of the element to extract", typeof(object)));
         InPortData.Add(new PortData(null, "list", "The list to extract elements from", typeof(object)));
         OutPortData = new PortData(null, "element", "Extracted element", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("get", portNames);
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
         OutPortData = new PortData(null, "empty", "An empty list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewList(FSharpList<Expression>.Empty);
      }
   }

   [ElementName("isEmpty?")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Checks to see if the given list is empty.")]
   [RequiresTransaction(false)]
   public class dynIsEmpty : dynElement
   {
      public dynIsEmpty()
      {
         InPortData.Add(new PortData(null, "list", "A list", typeof(object)));
         OutPortData = new PortData(null, "empty?", "Is the given list empty?", typeof(bool));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("empty?", portNames);
      }
   }

   [ElementName("len")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Gets the length of a list")]
   [RequiresTransaction(false)]
   public class dynLength : dynElement
   {
      public dynLength()
      {
         InPortData.Add(new PortData(null, "list", "A list", typeof(object)));
         OutPortData = new PortData(null, "length", "Length of the list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("len", portNames);
      }
   }

   [ElementName("append")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Appends two list")]
   [RequiresTransaction(false)]
   public class dynAppend : dynElement
   {
      public dynAppend()
      {
         InPortData.Add(new PortData(null, "listA", "First list", typeof(object)));
         InPortData.Add(new PortData(null, "listB", "Second list", typeof(object)));
         OutPortData = new PortData(null, "A+B", "A appended onto B", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("append", portNames);
      }
   }

   [ElementName("first")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Gets the first element of a list")]
   [RequiresTransaction(false)]
   public class dynFirst : dynElement
   {
      public dynFirst()
      {
         InPortData.Add(new PortData(null, "list", "A list", typeof(object)));
         OutPortData = new PortData(null, "first", "First element in the list", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("first", portNames);
      }
   }

   [ElementName("rest")]
   [ElementCategory(BuiltinElementCategories.LIST)]
   [ElementDescription("Gets the list with the first element removed.")]
   [RequiresTransaction(false)]
   public class dynRest : dynElement
   {
      public dynRest()
      {
         InPortData.Add(new PortData(null, "list", "A list", typeof(object)));
         OutPortData = new PortData(null, "rest", "List without the first element.", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode("rest", portNames);
      }
   }


   [ElementName("begin")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Executes expressions in a sequence")]
   [RequiresTransaction(false)]
   public class dynBegin : dynVariableInput
   {
      public dynBegin()
      {
         InPortData.Add(new PortData(null, "expr1", "Expression #1", typeof(object)));
         InPortData.Add(new PortData(null, "expr2", "Expression #2", typeof(object)));
         OutPortData = new PortData(null, "last", "Result of final expression", typeof(object));

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


   //[ElementName("apply2")]
   //[ElementCategory(BuiltinElementCategories.MISC)]
   //[ElementDescription("Applies arguments to a function")]
   //[RequiresTransaction(false)]
   //public class dynApply2 : dynElement
   //{
   //   public dynApply2()
   //   {
   //      InPortData.Add(new PortData(null, "func", "Procedure", typeof(object)));
   //      InPortData.Add(new PortData(null, "arg1", "Argument #1", typeof(object)));
   //      InPortData.Add(new PortData(null, "arg2", "Argumnet #2", typeof(object)));
   //      OutPortData = new PortData(null, "result", "Result", typeof(object)));

   //      base.RegisterInputsAndOutputs();
   //   }

   //   protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
   //   {
   //      var node = new ApplierNode();
   //      node.AddInput("arg2");
   //      return node;
   //   }
   //}


   [ElementName("apply")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Applies arguments to a function")]
   [RequiresTransaction(false)]
   public class dynApply1 : dynVariableInput
   {
      public dynApply1()
      {
         InPortData.Add(new PortData(null, "func", "Procedure", typeof(object)));
         OutPortData = new PortData(null, "result", "Result", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      protected override string getInputRootName()
      {
         return "arg";
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         //var node =  new ApplierNode();
         //foreach (var name in portNames.Skip(1))
         //{
         //   node.AddInput(name);
         //}
         //return node;
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
                  this.InPortData.Add(new PortData(null, subNode.Attributes["name"].Value, "", typeof(object)));
            }
         }
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
         InPortData.Add(new PortData(null, "x", "in", typeof(bool)));
         OutPortData = new PortData(null, "x", "out", typeof(object));

         base.RegisterInputsAndOutputs();
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return args[0];
      }
   }


   [ElementName("if")]
   [ElementCategory(BuiltinElementCategories.BOOLEAN)]
   [ElementDescription("Conditional statement")]
   [RequiresTransaction(false)]
   public class dynConditional : dynElement
   {
      public dynConditional()
      {
         InPortData.Add(new PortData(null, "test", "Test block", typeof(bool)));
         InPortData.Add(new PortData(null, "true", "True block", typeof(object)));
         InPortData.Add(new PortData(null, "false", "False block", typeof(object)));

         OutPortData = new PortData(null, "result", "Result", typeof(object));

         this.nickNameBlock.FontSize = 20;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new ConditionalNode();
      }
   }


   [ElementName("Variable")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("A function variable")]
   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynSymbol : dynElement
   {
      TextBox tb;

      public dynSymbol()
      {
         OutPortData = new PortData(null, "", "Symbol", typeof(object));

         //add a text box to the input grid of the control
         tb = new System.Windows.Controls.TextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "";
         //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         base.RegisterInputsAndOutputs();
      }

      public string Symbol
      {
         get { return this.tb.Text; }
         set { this.tb.Text = value; }
      }

      protected internal override INode Build()
      {
         return new SymbolNode(
            (string)this.Dispatcher.Invoke(new Func<string>(
               () => this.Symbol
            ))
         );
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
         XmlElement outEl = xmlDoc.CreateElement("Symbol");
         outEl.SetAttribute("value", this.Symbol);
         dynEl.AppendChild(outEl);
      }

      public override void LoadElement(XmlNode elNode)
      {
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name == "Symbol")
            {
               this.Symbol = subNode.Attributes[0].Value;
            }
         }
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

         InPortData.Add(new PortData(null, "", "Object to inspect", typeof(object)));
         OutPortData = new PortData(null, "", "Object inspected", typeof(object));

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


   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynAnonFunction : dynElement
   {
      private INode entryPoint;

      public dynAnonFunction(IEnumerable<string> inputs, string output, INode entryPoint)
      {
         int i = 1;
         foreach (string input in inputs)
         {
            InPortData.Add(new PortData(null, input, "Input #" + i++, typeof(object)));
         }

         OutPortData = new PortData(null, output, "function output", typeof(object));

         this.entryPoint = entryPoint;

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new AnonymousFunctionNode(portNames, this.entryPoint);
      }
   }


   [RequiresTransaction(false)]
   [IsInteractive(false)]
   public class dynFunction : dynElement
   {
      public string Symbol;

      public dynFunction(IEnumerable<string> inputs, string output, string symbol)
      {
         this.SetInputs(inputs);

         OutPortData = new PortData(null, output, "function output", typeof(object));

         this.Symbol = symbol;

         this.NickName = this.Symbol;

         this.MouseDoubleClick += delegate
         {
            dynElementSettings.SharedInstance.Bench.DisplayFunction(this.Symbol);
         };

         base.RegisterInputsAndOutputs();
      }

      public dynFunction()
      {
         this.MouseDoubleClick += delegate
         {
            dynElementSettings.SharedInstance.Bench.DisplayFunction(this.Symbol);
         };
      }

      public override bool IsDirty
      {
         get
         {
            var ws = dynElementSettings.SharedInstance.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            bool dirtyInternals = ws.elements.Any(e => e.IsDirty);
            return dirtyInternals || base.IsDirty;
         }
         set
         {
            base.IsDirty = value;
         }
      }

      public void SetInputs(IEnumerable<string> inputs)
      {
         int i = 0;
         foreach (string input in inputs)
         {
            PortData data = new PortData(null, input, "Input #" + (i + 1), typeof(object));

            if (this.InPortData.Count > i)
            {
               InPortData[i] = data;
            }
            else
            {
               InPortData.Add(data);
            }

            i++;
         }

         if (i < InPortData.Count)
         {
            InPortData.RemoveRange(i, InPortData.Count - i);
         }
      }

      public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
      {
         //Debug.WriteLine(pd.Object.GetType().ToString());
         XmlElement outEl = xmlDoc.CreateElement("Symbol");
         outEl.SetAttribute("value", this.Symbol);
         dynEl.AppendChild(outEl);

         outEl = xmlDoc.CreateElement("Output");
         outEl.SetAttribute("value", OutPortData.NickName);
         dynEl.AppendChild(outEl);

         outEl = xmlDoc.CreateElement("Inputs");
         foreach (var input in InPortData.Select(x => x.NickName))
         {
            var inputEl = xmlDoc.CreateElement("Input");
            inputEl.SetAttribute("value", input);
            outEl.AppendChild(inputEl);
         }
         dynEl.AppendChild(outEl);
      }

      public override void LoadElement(XmlNode elNode)
      {
         foreach (XmlNode subNode in elNode.ChildNodes)
         {
            if (subNode.Name.Equals("Symbol"))
            {
               this.Symbol = subNode.Attributes[0].Value;
            }
            else if (subNode.Name.Equals("Output"))
            {
               var data = new PortData(null, subNode.Attributes[0].Value, "function output", typeof(object));

               OutPortData = data;
            }
            else if (subNode.Name.Equals("Inputs"))
            {
               int i = 0;
               foreach (XmlNode inputNode in subNode.ChildNodes)
               {
                  var data = new PortData(null, inputNode.Attributes[0].Value, "Input #" + (i + 1), typeof(object));

                  if (InPortData.Count > i)
                  {
                     InPortData[i] = data;
                  }
                  else
                  {
                     InPortData.Add(data);
                  }

                  i++;
               }
            }
         }

         base.RegisterInputsAndOutputs();
      }

      protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
      {
         return new FunctionNode(this.Symbol, portNames);
      }
   }

   #endregion

   [ElementName("Number")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("An element which creates an unsigned floating point number.")]
   [RequiresTransaction(false)]
   public class dynDoubleInput : dynDouble
   {
      TextBox tb;

      public dynDoubleInput()
      {
         //add a text box to the input grid of the control
         tb = new System.Windows.Controls.TextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "0.0";
         tb.TextChanged += delegate { this.IsDirty = true; };
         //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         //OutPortData[0].Object = 0.0;

         base.RegisterInputsAndOutputs();
      }

      protected override double Value
      {
         get
         {
            try
            {
               return Convert.ToDouble(this.tb.Text);
            }
            catch
            {
               this.tb.Text = "0.0";
               return 0.0;
            }
         }
      }

      protected override void DeserializeValue(string val)
      {
         this.tb.Text = val;
      }
   }

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

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         InPortData.Add(new PortData(null, "N", "New Value", typeof(double)));
         //InPortData.Add(new PortData(null, "I", "Initial Value", typeof(dynDouble)));


         //outport declared in the abstract

         OutPortData = new PortData(null, "dbl", "The larger value of the input vs. the current value", typeof(double));

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

      #region Old Code

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      //public override void Draw()
      //{
      //   Process();
      //   base.Draw();
      //}

      //void Process()
      //{
      //   if (CheckInputs())
      //   {
      //      double newValue = (double)InPortData[0].Object; // new value is port 0
      //      // double initialValue = (double)InPortData[1].Object; // init is port 1

      //      if (newValue > CurrentValue) // if 
      //      {
      //         CurrentValue = newValue; // hill climber
      //         OutPortData[0].Object = CurrentValue;
      //         tb.Text = currentValue.ToString();
      //      }
      //   }
      //}


      //void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   try
      //   {
      //      //CurrentValue = Convert.ToDouble(tb.Text);

      //      //trigger the ready to build event here
      //      //because there are no inputs
      //      //OnDynElementReadyToBuild(EventArgs.Empty);

      //      this.CurrentValue = Convert.ToDouble(this.tb.Text);
      //   }
      //   catch
      //   {
      //      this.CurrentValue = 0.0;
      //      //OutPortData[0].Object = CurrentValue;
      //   }
      //}

      //void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      //{
      //   //if enter is pressed, update the value
      //   if (e.Key == System.Windows.Input.Key.Enter)
      //   {
      //      TextBox tb = sender as TextBox;

      //      try
      //      {
      //         //CurrentValue = Convert.ToDouble(tb.Text);

      //         //trigger the ready to build event here
      //         //because there are no inputs
      //         //OnDynElementReadyToBuild(EventArgs.Empty);

      //         this.CurrentValue = Convert.ToDouble(this.tb.Text);
      //      }
      //      catch
      //      {
      //         this.CurrentValue = 0.0;
      //         //OutPortData[0].Object = CurrentValue;
      //      }
      //   }
      //}

      //public override void Update()
      //{
      //   tb.Text = CurrentValue.ToString();

      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      #endregion

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         double newValue = (args.Head as Expression.Number).Item;
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
         //tb.IsReadOnly = true;
         //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         InPortData.Add(new PortData(null, "m", "Max Iterations", typeof(double)));
         InPortData.Add(new PortData(null, "v", "Value", typeof(double)));

         OutPortData = new PortData(null, "v", "Value", typeof(double));

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

      #region Old Code

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      //public override void Draw()
      //{
      //   Process();
      //   base.Draw();
      //}

      //void Process()
      //{


      //   if (CheckInputs())
      //   {
      //      int maxIterations = (int)InPortData[0].Object; // max iterations is port 0
      //      double newValue = (double)InPortData[1].Object; // new value is port 1

      //      if (NumIterations < maxIterations)// march up unto; max
      //      {
      //         if (newValue != CurrentValue) // if they vary at all, count that as a change and incretemnt counter.
      //         {
      //            CurrentValue = newValue;
      //            NumIterations++;//main thing we want is to increment

      //            OutPortData[0].Object = NumIterations;//pass out num iterations to port 0
      //            OutPortData[1].Object = CurrentValue;//pass through value
      //            tb.Text = NumIterations.ToString(); //show the counter value
      //         }
      //         return;
      //      }
      //      return;
      //   }
      //}


      //void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   try
      //   {
      //      NumIterations = Convert.ToInt32(tb.Text);

      //      //trigger the ready to build event here
      //      //because there are no inputs
      //      //OnDynElementReadyToBuild(EventArgs.Empty);
      //   }
      //   catch
      //   {
      //      NumIterations = 0;
      //      //OutPortData[0].Object = NumIterations;
      //   }
      //}

      //void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      //{
      //   //if enter is pressed, update the value
      //   if (e.Key == System.Windows.Input.Key.Enter)
      //   {
      //      TextBox tb = sender as TextBox;

      //      try
      //      {
      //         NumIterations = Convert.ToInt32(tb.Text);

      //         //trigger the ready to build event here
      //         //because there are no inputs
      //         //OnDynElementReadyToBuild(EventArgs.Empty);
      //      }
      //      catch
      //      {
      //         NumIterations = 0;
      //         //OutPortData[0].Object = NumIterations;
      //      }

      //   }

      //}

      //public override void Update()
      //{
      //   tb.Text = NumIterations.ToString();

      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      #endregion

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         double maxIterations = (args[0] as Expression.Number).Item;
         double newValue = (args[1] as Expression.Number).Item;
         if (newValue != this.CurrentValue)
         {
            this.NumIterations++;
            this.CurrentValue = newValue;
            this.tb.Text = this.NumIterations.ToString();
         }
         return Expression.NewNumber(this.NumIterations);
      }
   }

   [ElementName("Analysis Results by Selection")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("An element which allows you to select an analysis result object from the document and reference it in Dynamo.")]
   [RequiresTransaction(true)]
   public class dynAnalysisResultsBySelection : dynElement
   {
      public dynAnalysisResultsBySelection()
      {
         OutPortData = new PortData(null, "ar", "Analysis Results referenced by this operation.", typeof(Element));
         //OutPortData[0].Object = this.Tree;

         //add a button to the inputGrid on the dynElement
         System.Windows.Controls.Button analysisResultButt = new System.Windows.Controls.Button();
         this.inputGrid.Children.Add(analysisResultButt);
         analysisResultButt.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         analysisResultButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         analysisResultButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         analysisResultButt.Click += new System.Windows.RoutedEventHandler(analysisResultButt_Click);
         analysisResultButt.Content = "Select AR";
         analysisResultButt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         analysisResultButt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         base.RegisterInputsAndOutputs();

      }
      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      public Element pickedAnalysisResult;

      public Element PickedAnalysisResult
      {
         get { return pickedAnalysisResult; }
         set
         {
            pickedAnalysisResult = value;
            NotifyPropertyChanged("PickedAnalysisResult");
         }
      }

      private ElementId analysisResultID;

      private ElementId AnalysisResultID
      {
         get { return analysisResultID; }
         set
         {
            analysisResultID = value;
            NotifyPropertyChanged("AnalysisResultID");
         }
      }
      void analysisResultButt_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         //read from the state objects
         //if (CheckInputs())
         //{
         PickedAnalysisResult =
            Dynamo.Utilities.SelectionHelper.RequestAnalysisResultInstanceSelection(
               dynElementSettings.SharedInstance.Doc,
               "Select Analysis Result Object",
               dynElementSettings.SharedInstance
            );

         if (PickedAnalysisResult != null)
         {
            AnalysisResultID = PickedAnalysisResult.Id;
            //Process(); // don't need to pass in anything because analysis rssult and tree already have accesors.
         }
         //}
      }

      #region Old Code

      //public override void Draw()
      //{

      //   // watch?
      //   //currentBranch.Leaves.Add(fi);

      //   Process(); // don't need to pass in anything because analysis rssult and tree already have accesors.
      //   base.Draw();
      //}

      //public void Process()
      //{
      //   if (PickedAnalysisResult != null)
      //   {
      //      if (PickedAnalysisResult.Id.IntegerValue == AnalysisResultID.IntegerValue) // sanity check
      //      {
      //         SpatialFieldManager dmu_sfm = dynElementSettings.SharedInstance.SpatialFieldManagerUpdated as SpatialFieldManager;

      //         if (pickedAnalysisResult.Id.IntegerValue == dmu_sfm.Id.IntegerValue)
      //         {
      //            TaskDialog.Show("ah hah", "picked sfm equals saved one from dmu");
      //         }
      //         //need to put a watcher on this to ensure deletion works 
      //         this.Tree.Clear(); // clean out old refs
      //         this.Tree.Trunk.Branches.Add(new DataTreeBranch());
      //         this.Tree.Trunk.Branches[0].Leaves.Add(PickedAnalysisResult);


      //         ////let's look a the collection of AR data!
      //         //this.Tree.Trunk.Branches.Add(new DataTreeBranch());


      //         //SpatialFieldManager avf = PickedAnalysisResult as SpatialFieldManager;
      //         //IList<int>resultsList = new List<int>();


      //         //resultsList = avf.GetRegisteredResults();
      //         //int numMeasurements = avf.NumberOfMeasurements;
      //         //this.Tree.Trunk.Branches[1].Leaves.Add(numMeasurements);

      //         ////avf.

      //         //foreach (int value in resultsList)
      //         //{
      //         //    this.Tree.Trunk.Branches[1].Leaves.Add(value);
      //         //}

      //         OutPortData[0].Object = this.Tree;
      //         OnDynElementReadyToBuild(EventArgs.Empty);//kick it
      //      }
      //   }
      //}

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      //public override void Destroy()
      //{
      //   //base.Destroy();
      //}

      #endregion

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         if (PickedAnalysisResult != null)
         {
            if (PickedAnalysisResult.Id.IntegerValue == AnalysisResultID.IntegerValue) // sanity check
            {
               SpatialFieldManager dmu_sfm = dynElementSettings.SharedInstance.SpatialFieldManagerUpdated as SpatialFieldManager;

               if (pickedAnalysisResult.Id.IntegerValue == dmu_sfm.Id.IntegerValue)
               {
                  TaskDialog.Show("ah hah", "picked sfm equals saved one from dmu");
               }

               return Expression.NewContainer(this.PickedAnalysisResult);
            }
         }

         throw new Exception("No data selected!");
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

      public dynDoubleSliderInput()
      {
         //add a slider control to the input grid of the control
         tb_slider = new System.Windows.Controls.Slider();
         tb_slider.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb_slider.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb_slider);
         System.Windows.Controls.Grid.SetColumn(tb_slider, 0);
         System.Windows.Controls.Grid.SetRow(tb_slider, 0);
         tb_slider.Value = 0.0;
         tb_slider.Maximum = 100.0;
         tb_slider.Minimum = 0.0;
         tb_slider.Ticks = new System.Windows.Media.DoubleCollection(10);
         tb_slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
         tb_slider.ValueChanged += delegate { this.IsDirty = true; };
         //tb_slider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(tb_slider_ValueChanged);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //InPortData.Add(new PortData(null, "Lower", "Lower", typeof(dynDouble)));
         //InPortData.Add(new PortData(null, "Upper", "Upper", typeof(dynDouble)));

         //OutPortData[0].Object = 0.0;

         base.RegisterInputsAndOutputs();
      }

      protected override void DeserializeValue(string val)
      {
         try
         {
            this.tb_slider.Value = Convert.ToDouble(val);
         }
         catch { }
      }

      protected override double Value
      {
         get { return this.tb_slider.Value; }
      }

      #region Old Code

      //double currentValue;

      //void tb_slider_ValueChanged(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   try
      //   {
      //      this.currentValue = tb_slider.Value;

      //      //trigger the ready to build event here
      //      //because there are no inputs
      //      //OnDynElementReadyToBuild(EventArgs.Empty);
      //   }
      //   catch
      //   {
      //      this.currentValue = 0.0;
      //   }
      //}

      //public override void Update()
      //{
      //if (CheckInputs())
      //{
      //set the bounds
      //tb_slider.Minimum = (double)InPortData[0].Object;
      //tb_slider.Maximum = (double)InPortData[1].Object;
      //}

      //tb_slider.Value = (double)OutPortData[0].Object;

      //OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      #endregion
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

      protected override void DeserializeValue(string val)
      {
         try
         {
            if (val.Equals("True"))
            {
               this.currentValue = true;
               this.rbFalse.IsChecked = false;
               this.rbTrue.IsChecked = true;
            }
            else
            {
               this.currentValue = false;
               this.rbFalse.IsChecked = true;
               this.rbTrue.IsChecked = false;
            }
         }
         catch { }
      }

      protected override bool Value
      {
         get
         {
            return this.currentValue;
         }
      }

      bool currentValue = false;

      void rbFalse_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         this.currentValue = false;
         this.IsDirty = true;
      }

      void rbTrue_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         this.currentValue = true;
         this.IsDirty = true;
      }
   }

   //[ElementName("Int")]
   //[ElementDescription("An element which creates a signed integer value.")]
   //[RequiresTransaction(false)]
   //public class dynIntInput : dynDouble
   //{
   //   TextBox tb;

   //   public dynIntInput()
   //   {
   //      //add a text box to the input grid of the control
   //      tb = new System.Windows.Controls.TextBox();
   //      tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
   //      tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
   //      inputGrid.Children.Add(tb);
   //      System.Windows.Controls.Grid.SetColumn(tb, 0);
   //      System.Windows.Controls.Grid.SetRow(tb, 0);
   //      tb.Text = "0";
   //      //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
   //      //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

   //      //turn off the border
   //      SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
   //      tb.Background = backgroundBrush;
   //      tb.BorderThickness = new Thickness(0);

   //      OutPortData[0].Object = 0;

   //      base.RegisterInputsAndOutputs();
   //   }

   //   #region Old Code

   //   //int currentValue;

   //   //void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
   //   //{
   //   //   try
   //   //   {
   //   //      //OutPortData[0].Object = Convert.ToInt32(tb.Text);

   //   //      this.currentValue = Convert.ToInt32(tb.Text);

   //   //      //trigger the ready to build event here
   //   //      //because there are no inputs
   //   //      //OnDynElementReadyToBuild(EventArgs.Empty);
   //   //   }
   //   //   catch
   //   //   {
   //   //      //OutPortData[0].Object = 0;
   //   //      this.currentValue = 0;
   //   //   }
   //   //}

   //   //void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
   //   //{
   //   //   //if enter is pressed, update the value
   //   //   if (e.Key == System.Windows.Input.Key.Enter)
   //   //   {
   //   //      TextBox tb = sender as TextBox;

   //   //      try
   //   //      {
   //   //         //OutPortData[0].Object = Convert.ToInt32(tb.Text);

   //   //         this.currentValue = Convert.ToInt32(tb.Text);

   //   //         //trigger the ready to build event here
   //   //         //because there are no inputs
   //   //         //OnDynElementReadyToBuild(EventArgs.Empty);
   //   //      }
   //   //      catch
   //   //      {
   //   //         //OutPortData[0].Object = 0;
   //   //         this.currentValue = 0;
   //   //      }
   //   //   }
   //   //}

   //   //public override void Update()
   //   //{
   //   //   //tb.Text = OutPortData[0].Object.ToString();
   //   //   tb.Text = this.currentValue.ToString();

   //   //   //OnDynElementReadyToBuild(EventArgs.Empty);
   //   //}

   //   #endregion

   //   protected override double Value
   //   {
   //      get
   //      {
   //         try
   //         {
   //            return Convert.ToInt32(tb.Text);
   //         }
   //         catch
   //         {
   //            tb.Text = "0";
   //            return 0;
   //         }
   //      }
   //   }
   //}

   [ElementName("String")]
   [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
   [ElementDescription("An element which creates a string value.")]
   [RequiresTransaction(false)]
   public class dynStringInput : dynString
   {
      TextBox tb;

      public dynStringInput()
      {
         //add a text box to the input grid of the control
         tb = new System.Windows.Controls.TextBox();
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         inputGrid.Children.Add(tb);
         System.Windows.Controls.Grid.SetColumn(tb, 0);
         System.Windows.Controls.Grid.SetRow(tb, 0);
         tb.Text = "";
         //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
         //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

         //turn off the border
         SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
         tb.Background = backgroundBrush;
         tb.BorderThickness = new Thickness(0);

         //OutPortData[0].Object = "";

         base.RegisterInputsAndOutputs();
      }

      #region Old Code

      //string currentValue;

      //void tb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   try
      //   {
      //      //OutPortData[0].Object = tb.Text;

      //      this.currentValue = tb.Text;

      //      //trigger the ready to build event here
      //      //because there are no inputs
      //      //OnDynElementReadyToBuild(EventArgs.Empty);
      //   }
      //   catch
      //   {
      //      //OutPortData[0].Object = 0;
      //      this.currentValue = "";
      //   }
      //}

      //void tb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
      //{
      //   //if enter is pressed, update the value
      //   if (e.Key == System.Windows.Input.Key.Enter)
      //   {
      //      TextBox tb = sender as TextBox;

      //      try
      //      {
      //         //OutPortData[0].Object = tb.Text;

      //         this.currentValue = tb.Text;

      //         //trigger the ready to build event here
      //         //because there are no inputs
      //         //OnDynElementReadyToBuild(EventArgs.Empty);
      //      }
      //      catch
      //      {
      //         //OutPortData[0].Object = 0;
      //         this.currentValue = "";
      //      }
      //   }
      //}

      //public override void Update()
      //{
      //   //tb.Text = OutPortData[0].Object.ToString();

      //   tb.Text = this.currentValue;

      //   //OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      #endregion

      protected override string Value
      {
         get { return this.tb.Text; }
      }

      protected override void DeserializeValue(string val)
      {
         this.tb.Text = val;
      }
   }


   #region element types

   public enum COMPort { COM3, COM4 };

   [ElementName("Arduino")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("An element which allows you to read from an Arduino microcontroller.")]
   [RequiresTransaction(false)]
   public class dynArduino : dynElement
   {
      SerialPort port;
      //string lastData = "";
      //COMPort portState;
      System.Windows.Controls.MenuItem com4Item;
      System.Windows.Controls.MenuItem com3Item;

      public dynArduino()
      {
         //InPortData.Add(new PortData(null, "loop", "The loop to execute.", typeof(dynLoop)));
         //InPortData.Add(new PortData(null, "i/o", "Switch Arduino on?", typeof(bool)));
         //InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(double)));

         OutPortData = new PortData(null, "output", "Serial output", typeof(double));
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();

         port = new SerialPort("COM3", 9600);
         port.NewLine = "\r\n";
         port.DtrEnable = true;
         //port.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

         com3Item = new System.Windows.Controls.MenuItem();
         com3Item.Header = "COM3";
         com3Item.IsCheckable = true;
         com3Item.IsChecked = true;
         com3Item.Checked += new System.Windows.RoutedEventHandler(com3Item_Checked);

         com4Item = new System.Windows.Controls.MenuItem();
         com4Item.Header = "COM4";
         com4Item.IsCheckable = true;
         com4Item.IsChecked = false;
         com4Item.Checked += new System.Windows.RoutedEventHandler(com4Item_Checked);

         this.MainContextMenu.Items.Add(com3Item);
         this.MainContextMenu.Items.Add(com4Item);
         //portState = COMPort.COM3;
         port.PortName = "COM3";
      }

      void com4Item_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         //portState = COMPort.COM4;
         com4Item.IsChecked = true;
         com3Item.IsChecked = false;
      }

      void com3Item_Checked(object sender, System.Windows.RoutedEventArgs e)
      {
         //portState = COMPort.COM3;
         com4Item.IsChecked = false;
         com3Item.IsChecked = true;
      }

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      //add one branch
      //      //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
      //      //this.Tree.Trunk.Branches[0].Leaves.Add(null);

      //      if (port != null)
      //      {
      //         bool isOpen = Convert.ToBoolean(InPortData[0].Object);

      //         if (isOpen == true)
      //         {
      //            if (!port.IsOpen)
      //            {
      //               if (portState == COMPort.COM3)
      //                  port.PortName = "COM3";
      //               else
      //                  port.PortName = "COM4";

      //               port.Open();
      //            }

      //            //get the analog value from the serial port
      //            GetArduinoData();

      //            //i don't know why this works 
      //            //but OnDynElementReadyToBuild doesn't
      //            //this.UpdateOutputs();
      //            //OnDynElementReadyToBuild(EventArgs.Empty);
      //         }
      //         else if (isOpen == false)
      //         {
      //            if (port.IsOpen)
      //               port.Close();
      //         }

      //      }

      //   }
      //}

      private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
      {
         if (CheckInputs())
         {
            //add one branch
            //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
            //this.Tree.Trunk.Branches[0].Leaves.Add(null);

            if (port != null)
            {
               bool isOpen = true;// Convert.ToBoolean(InPortData[0].Object);

               if (isOpen == true)
               {
                  if (!port.IsOpen)
                  {
                     port.Open();
                  }

                  //get the analog value from the serial port
                  GetArduinoData();

                  //i don't know why this works 
                  //but OnDynElementReadyToBuild doesn't
                  this.UpdateOutputs();
                  //OnDynElementReadyToBuild(EventArgs.Empty);
               }
               else if (isOpen == false)
               {
                  if (port.IsOpen)
                     port.Close();
               }

            }

         }
      }

      private void GetArduinoData()
      {
         //string data = port.ReadExisting();
         //lastData += data;
         //string[] allData = lastData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
         //if (allData.Length > 0)
         //{
         //lastData = allData[allData.Length - 1];
         //this.Tree.Trunk.Branches[0].Leaves[0] = lastData;
         //this.OutPortData[0].Object = Convert.ToDouble(lastData);
         //}

         //int data = 255;
         while (port.BytesToRead > 0)
         {
            this.data = port.ReadByte();
         }

         //this.OutPortData[0].Object = Convert.ToDouble(data);
      }

      //public override void Update()
      //{
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      int data;

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         return Expression.NewNumber(this.data);
      }

   }

   //[ElementName("Kinect")]
   //[ElementCategory(BuiltinElementCategories.MISC)]
   //[ElementDescription("An element which allows you to read from a Kinect.")]
   //[RequiresTransaction(true)]
   //public class dynKinect : dynElement
   //{
   //   //Kinect Runtime
   //   Runtime nui;
   //   Image image1;
   //   PlanarImage planarImage;
   //   XYZ rightHandLoc = new XYZ();
   //   ReferencePoint rightHandPt;
   //   System.Windows.Shapes.Ellipse rightHandEllipse;

   //   public dynKinect()
   //   {
   //      //InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(dynTimer)));
   //      InPortData.Add(new PortData(null, "X scale", "The amount to scale the skeletal measurements in the X direction.", typeof(double)));
   //      InPortData.Add(new PortData(null, "Y scale", "The amount to scale the skeletal measurements in the Y direction.", typeof(double)));
   //      InPortData.Add(new PortData(null, "Z scale", "The amount to scale the skeletal measurements in the Z direction.", typeof(double)));

   //      OutPortData = new PortData(null, "Hand", "Reference point representing hand location", typeof(ReferencePoint)));
   //      //OutPortData[0].Object = this.Tree;

   //      image1 = new Image();
   //      image1.Width = 320;
   //      image1.Height = 240;
   //      image1.Margin = new Thickness(5);
   //      image1.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
   //      image1.Name = "image1";
   //      image1.VerticalAlignment = System.Windows.VerticalAlignment.Top;
   //      //image1.Margin = new Thickness(0, 0, 0, 0);

   //      Canvas trackingCanvas = new Canvas();
   //      trackingCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
   //      trackingCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

   //      //add an ellipse to track the hand
   //      rightHandEllipse = new System.Windows.Shapes.Ellipse();
   //      rightHandEllipse.Height = 10;
   //      rightHandEllipse.Width = 10;
   //      rightHandEllipse.Name = "rightHandEllipse";
   //      SolidColorBrush yellowBrush = new SolidColorBrush(System.Windows.Media.Colors.OrangeRed);
   //      rightHandEllipse.Fill = yellowBrush;

   //      this.inputGrid.Children.Add(image1);
   //      this.inputGrid.Children.Add(trackingCanvas);
   //      trackingCanvas.Children.Add(rightHandEllipse);

   //      base.RegisterInputsAndOutputs();

   //      this.Width = 450;
   //      this.Height = 240 + 5;

   //      this.Loaded += new RoutedEventHandler(topControl_Loaded);
   //   }

   //   void topControl_Loaded(object sender, RoutedEventArgs e)
   //   {
   //      SetupKinect();
   //      //nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
   //      //nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
   //      //nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
   //      //nui.VideoStream.Open(ImageStreamType.Video, 2, Microsoft.Research.Kinect.Nui.ImageResolution.Resolution640x480, ImageType.Color);
   //      nui.DepthStream.Open(ImageStreamType.Depth, 2, Microsoft.Research.Kinect.Nui.ImageResolution.Resolution320x240, ImageType.Depth);
   //   }

   //   public override Expression Evaluate(FSharpList<Expression> args)
   //   {
   //      if (rightHandPt == null)
   //      {
   //         //create a reference point to track the right hand
   //         rightHandPt = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(rightHandLoc);
   //         System.Windows.Point relativePoint =
   //            rightHandEllipse.TransformToAncestor(dynElementSettings.SharedInstance.Bench.workBench)
   //                            .Transform(new System.Windows.Point(0, 0));

   //         Canvas.SetLeft(rightHandEllipse, relativePoint.X);
   //         Canvas.SetTop(rightHandEllipse, relativePoint.Y);

   //         //add the right hand point at the base of the tree
   //         //this.Tree.Trunk.Leaves.Add(rightHandPt);
   //      }

   //      //if (CheckInputs())
   //      //{
   //      double xScale = (args[1] as Expression.Number).Item;
   //      double yScale = (args[2] as Expression.Number).Item;
   //      double zScale = (args[3] as Expression.Number).Item;

   //      //update the image
   //      image1.Source = nui.DepthStream.GetNextFrame(0).ToBitmapSource();

   //      //get skeletonData
   //      SkeletonFrame allSkeletons = nui.SkeletonEngine.GetNextFrame(0);

   //      if (allSkeletons != null)
   //      {
   //         //get the first tracked skeleton
   //         SkeletonData skeleton = (from s in allSkeletons.Skeletons
   //                                  where s.TrackingState == SkeletonTrackingState.Tracked
   //                                  select s).FirstOrDefault();

   //         if (skeleton != null)
   //         {
   //            Joint HandRight = skeleton.Joints[JointID.HandRight];
   //            rightHandLoc = new XYZ(HandRight.Position.X * xScale, HandRight.Position.Z * zScale, HandRight.Position.Y * yScale);

   //            SetEllipsePosition(rightHandEllipse, HandRight);

   //            XYZ vec = rightHandLoc - rightHandPt.Position;
   //            Debug.WriteLine(vec.ToString());

   //            //move the reference point
   //            dynElementSettings.SharedInstance.Doc.Document.Move(rightHandPt, vec);

   //            dynElementSettings.SharedInstance.Doc.RefreshActiveView();
   //         }
   //      }
   //      //}

   //      return Expression.NewContainer(this.rightHandPt);
   //   }

   //   public override void Draw()
   //   {
   //      if (rightHandPt == null)
   //      {
   //         //create a reference point to track the right hand
   //         rightHandPt = dynElementSettings.SharedInstance.Doc.Document.FamilyCreate.NewReferencePoint(rightHandLoc);
   //         System.Windows.Point relativePoint = rightHandEllipse.TransformToAncestor(dynElementSettings.SharedInstance.Bench.workBench)
   //                       .Transform(new System.Windows.Point(0, 0));
   //         Canvas.SetLeft(rightHandEllipse, relativePoint.X);
   //         Canvas.SetTop(rightHandEllipse, relativePoint.Y);

   //         //add the right hand point at the base of the tree
   //         this.Tree.Trunk.Leaves.Add(rightHandPt);
   //      }

   //      if (CheckInputs())
   //      {
   //         double xScale = Convert.ToDouble(InPortData[1].Object);
   //         double yScale = Convert.ToDouble(InPortData[2].Object);
   //         double zScale = Convert.ToDouble(InPortData[3].Object);

   //         //update the image
   //         image1.Source = nui.DepthStream.GetNextFrame(0).ToBitmapSource();

   //         //get skeletonData
   //         SkeletonFrame allSkeletons = nui.SkeletonEngine.GetNextFrame(0);

   //         if (allSkeletons != null)
   //         {
   //            //get the first tracked skeleton
   //            SkeletonData skeleton = (from s in allSkeletons.Skeletons
   //                                     where s.TrackingState == SkeletonTrackingState.Tracked
   //                                     select s).FirstOrDefault();

   //            if (skeleton != null)
   //            {
   //               Joint HandRight = skeleton.Joints[JointID.HandRight];
   //               rightHandLoc = new XYZ(HandRight.Position.X * xScale, HandRight.Position.Z * zScale, HandRight.Position.Y * yScale);

   //               SetEllipsePosition(rightHandEllipse, HandRight);

   //               XYZ vec = rightHandLoc - rightHandPt.Position;
   //               Debug.WriteLine(vec.ToString());

   //               //move the reference point
   //               dynElementSettings.SharedInstance.Doc.Document.Move(rightHandPt, vec);

   //               dynElementSettings.SharedInstance.Doc.RefreshActiveView();
   //            }
   //         }
   //      }

   //   }

   //   public override void Destroy()
   //   {
   //      //don't call destroy
   //      //base.Destroy();
   //   }

   //   public override void Update()
   //   {
   //      OnDynElementReadyToBuild(EventArgs.Empty);
   //   }

   //   private void SetEllipsePosition(FrameworkElement ellipse, Joint joint)
   //   {
   //      var scaledJoint = joint.ScaleTo(320, 240, .5f, .5f);

   //      //System.Windows.Point relativePoint = ellipse.TransformToAncestor(dynElementSettings.SharedInstance.Bench.workBench)
   //      //                  .Transform(new System.Windows.Point(scaledJoint.Position.X, scaledJoint.Position.Y));

   //      //Canvas.SetLeft(ellipse, relativePoint.X);
   //      //Canvas.SetTop(ellipse, relativePoint.Y);

   //      Canvas.SetLeft(ellipse, scaledJoint.Position.X);
   //      Canvas.SetTop(ellipse, scaledJoint.Position.Y);
   //   }

   //   //void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
   //   //{
   //   //    PlanarImage image = e.ImageFrame.Image;
   //   //    image1.Source = e.ImageFrame.ToBitmapSource();
   //   //}

   //   void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
   //   {
   //      planarImage = e.ImageFrame.Image;
   //      image1.Source = e.ImageFrame.ToBitmapSource();

   //   }

   //   void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
   //   {
   //      SkeletonFrame allSkeletons = e.SkeletonFrame;

   //      //get the first tracked skeleton
   //      SkeletonData skeleton = (from s in allSkeletons.Skeletons
   //                               where s.TrackingState == SkeletonTrackingState.Tracked
   //                               select s).FirstOrDefault();

   //      Joint HandRight = skeleton.Joints[JointID.HandRight];
   //      rightHandLoc = new XYZ(HandRight.Position.X, HandRight.Position.Y, HandRight.Position.Z);

   //      //move the reference point
   //      dynElementSettings.SharedInstance.Doc.Document.Move(rightHandPt, rightHandLoc - rightHandPt.Position);
   //   }

   //   private void SetupKinect()
   //   {
   //      if (Runtime.Kinects.Count == 0)
   //      {
   //         this.inputGrid.ToolTip = "No Kinect connected";
   //      }
   //      else
   //      {
   //         //use first Kinect
   //         nui = Runtime.Kinects[0];         //Initialize to return both Color & Depth images
   //         //nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepth);
   //         nui.Initialize(RuntimeOptions.UseDepth | RuntimeOptions.UseSkeletalTracking);
   //      }

   //   }
   //}

   //[ElementName("Timer")]
   //[ElementDescription("An element which allows you to specify an update frequency in milliseconds.")]
   //[RequiresTransaction(false)]
   //public class dynTimer : dynElement, IDynamic
   //{
   //   Stopwatch sw;
   //   bool timing = false;

   //   public dynTimer()
   //   {
   //      InPortData.Add(new PortData(null, "n", "How often to receive updates in milliseconds.", typeof(dynInt)));
   //      InPortData.Add(new PortData(null, "i/o", "Turn the timer on or off", typeof(dynBool)));
   //      OutPortData = new PortData(null, "tim", "The timer, counting in milliseconds.", typeof(dynTimer)));
   //      OutPortData[0].Object = this;

   //      base.RegisterInputsAndOutputs();

   //      sw = new Stopwatch();

   //   }

   //   void KeepTime()
   //   {
   //      if (CheckInputs())
   //      {
   //         bool on = Convert.ToBoolean(InPortData[1].Object);
   //         if (on)
   //         {
   //            int interval = Convert.ToInt16(InPortData[0].Object);

   //            if (sw.ElapsedMilliseconds > interval)
   //            {
   //               sw.Stop();
   //               sw.Reset();
   //               OnDynElementReadyToBuild(EventArgs.Empty);
   //               sw.Start();
   //            }
   //         }
   //      }
   //   }

   //   public override void Draw()
   //   {
   //      if (CheckInputs())
   //      {
   //         bool isTiming = Convert.ToBoolean(InPortData[0].Object);

   //         if (timing)
   //         {
   //            if (!isTiming)  //if you are timing and we turn off the timer
   //            {
   //               timing = false; //stop
   //               sw.Stop();
   //               sw.Reset();
   //            }
   //         }
   //         else
   //         {
   //            if (isTiming)
   //            {
   //               timing = true;  //if you are not timing and we turn on the timer
   //               sw.Start();
   //               while (timing)
   //               {
   //                  KeepTime();
   //               }
   //            }
   //         }
   //      }
   //   }

   //   public override void Update()
   //   {
   //      OnDynElementReadyToBuild(EventArgs.Empty);
   //   }

   //}

   //[ElementName("Watch")]
   //[ElementDescription("Create an element for watching the results of other operations.")]
   //[RequiresTransaction(false)]
   //public class dynWatch : dynElement, IDynamic, INotifyPropertyChanged
   //{
   //   //System.Windows.Controls.Label label;
   //   //System.Windows.Controls.ListBox listBox;
   //   System.Windows.Controls.TextBox tb;

   //   string watchValue;

   //   public event PropertyChangedEventHandler PropertyChanged;

   //   private void NotifyPropertyChanged(String info)
   //   {
   //      if (PropertyChanged != null)
   //      {
   //         PropertyChanged(this, new PropertyChangedEventArgs(info));
   //      }
   //   }

   //   public string WatchValue
   //   {
   //      get { return watchValue; }
   //      set
   //      {
   //         watchValue = value;
   //         NotifyPropertyChanged("WatchValue");
   //      }
   //   }

   //   public dynWatch()
   //   {

   //      //add a list box
   //      System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
   //      tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
   //      tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

   //      //turn off the border
   //      SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
   //      tb.Background = backgroundBrush;
   //      tb.BorderThickness = new Thickness(0);

   //      WatchValue = "Ready to watch!";

   //      //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx

   //      System.Windows.Data.Binding b = new System.Windows.Data.Binding("WatchValue");
   //      b.Source = this;
   //      //label.SetBinding(System.Windows.Controls.Label.ContentProperty, b);
   //      tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

   //      //this.inputGrid.Children.Add(label);
   //      this.inputGrid.Children.Add(tb);
   //      tb.TextWrapping = System.Windows.TextWrapping.Wrap;
   //      tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
   //      //tb.AcceptsReturn = true;

   //      InPortData.Add(new PortData(null, "", "The Element to watch", typeof(dynElement)));


   //      base.RegisterInputsAndOutputs();

   //      //resize the panel
   //      this.topControl.Height = 100;
   //      this.topControl.Width = 300;
   //      UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
   //      Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
   //      //this.UpdateLayout();
   //   }

   //   public override void Draw()
   //   {
   //      if (CheckInputs())
   //      {
   //         WatchValue = InPortData[0].Object.ToString();
   //         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
   //         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });

   //         //DataTree tree = InPortData[0].Object as DataTree;

   //         //if (tree != null)
   //         //{
   //         //    WatchValue = tree.ToString();

   //         //    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
   //         //    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
   //         //}
   //         //else
   //         //{
   //         //    //find the object as a string
   //         //    WatchValue = InPortData[0].Object.ToString();
   //         //    UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
   //         //    Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
   //         //}
   //      }
   //   }


   //   public override void Destroy()
   //   {
   //      base.Destroy();
   //   }

   //   public override void Update()
   //   {
   //      //this.topControl.Height = 400;
   //      OnDynElementReadyToBuild(EventArgs.Empty);
   //   }
   //}

   [ElementName("Extract Solar Radiation Value")]
   [ElementCategory(BuiltinElementCategories.REVIT)]
   [ElementDescription("Create an element for extracting and computing the average solar radiation value based on a csv file.")]
   [RequiresTransaction(false)]
   public class dynComputeSolarRadiationValue : dynElement
   {
      //System.Windows.Controls.Label label;
      //System.Windows.Controls.ListBox listBox;
      System.Windows.Controls.TextBox tb;

      string watchValue;
      double sumValue = 0.0;

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      public string WatchValue
      {
         get { return watchValue; }
         set
         {
            watchValue = value;
            NotifyPropertyChanged("WatchValue");
         }
      }


      public double SumValue
      {
         get { return sumValue; }
         set
         {
            sumValue = value;
            NotifyPropertyChanged("SumValue");
         }
      }

      public dynComputeSolarRadiationValue()
      {
         //add a list box
         //label = new System.Windows.Controls.Label();
         tb = new System.Windows.Controls.TextBox();
         tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
         tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

         WatchValue = "Ready to compute solar radiation value!";

         //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx

         System.Windows.Data.Binding b = new System.Windows.Data.Binding("WatchValue");
         b.Source = this;
         //label.SetBinding(System.Windows.Controls.Label.ContentProperty, b);
         tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

         this.inputGrid.Children.Add(tb);
         tb.TextWrapping = System.Windows.TextWrapping.Wrap;
         tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
         //tb.AcceptsReturn = true;

         InPortData.Add(new PortData(null, "s", "The solar radiation data file", typeof(DataTree)));

         OutPortData = new PortData(null, "s", "The solar radiation computed data", typeof(double));
         //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
         //this.Tree.Trunk.Branches[0].Leaves.Add(SumValue); //MDJ TODO - cleanup input tree and output tree
         //OutPortData[0].Object = this.Tree;
         //OutPortData[0].Object = SumValue;


         base.RegisterInputsAndOutputs();

         //resize the panel
         this.topControl.Height = 100;
         this.topControl.Width = 300;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
         //this.UpdateLayout();
      }

      public void Process(DataTreeBranch bIn)
      {
         string line = "";
         double doubleSRValue = 0;

         // SR export schema:
         //Source,Date,Time,Model,Type,Study Date Range,Study Time Range,Longitude,Latitude,Unit
         //Vasari v1.0,11/19/2011,2:33 PM,insolationProjectMockUp.rvt,Cumulative,"1/1/2010,12/31/2010","10:00 AM,4:00 PM",-71.0329971313477,42.2130012512207,BTU/ft²
         //
         //Analysis point index,Insolation value,point x,point y,point z,normal x,normal y,normal z
         //1,153823.9528125,7.23744587802689,-32.6932900007427,70.7843137254902,0.2871833,-0.2871833,0.9138116
         //2,159066.52853125,4.74177488560379,-30.1976190083196,72.3529411764706,0.2871833,-0.2871833,0.9138116

         //DataTree treeIn = InPortData[0].Object as DataTree;
         //    if (treeIn != null)
         //    {

         foreach (object o in bIn.Leaves)
         {
            line = o.ToString();

            string[] values = line.Split(',');
            //index = int.Parse(values[0]); // seems a little hacky

            //int i = 0;
            int intTest = 0;// used in TryParse below. returns 0 if not an int and >0 if an int.

            if (int.TryParse(values[0], out intTest)) // test the first value. if the first value is an int, then we know we are passed the header lines and into data
            {
               // string stringSRValue = values[1];

               doubleSRValue = double.Parse(values[1]); // the 2nd value is the one we want

               SumValue = SumValue + doubleSRValue; // compute the sum but adding current value with previous values
            }
         }

         foreach (DataTreeBranch nextBranch in bIn.Branches)
         {
            Process(nextBranch);
         }
      }

      #region OldCode

      //public override void Draw()
      //{
      //   if (CheckInputs())
      //   {
      //      DataTree tree = InPortData[0].Object as DataTree;

      //      if (tree != null)
      //      {
      //         SumValue = 0.0; // reset to ensue we don't count on refresh
      //         Process(tree.Trunk.Branches[0]); // add em back up
      //         WatchValue = "Computed Sum of SR Values: " + SumValue.ToString() + "\n";
      //         //WatchValue = WatchValue + tree.ToString(); // MDJ presume data is in a data tree, one line in each datatree leaf
      //         //this.Tree.Clear();
      //         // this.Tree.Trunk.Branches[0].Leaves.Add(SumValue);
      //         try
      //         {
      //            OutPortData[0].Object = SumValue;
      //         }
      //         catch (Exception e)
      //         {
      //            TaskDialog.Show("Error", e.ToString());
      //         }

      //         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
      //      }
      //      else
      //      {
      //         TaskDialog.Show("Error", "Please use this element for computing and showing solar radiation data");
      //         //find the object as a string
      //         WatchValue = InPortData[0].Object.ToString();
      //         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
      //      }
      //   }
      //}

      //public override void Destroy()
      //{
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   //this.topControl.Height = 400;
      //   //OnDynElementReadyToBuild(EventArgs.Empty);
      //}

      #endregion

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         string data = ((Expression.String)args[0]).Item;

         SumValue = 0.0; // reset to ensue we don't count on refresh


         double doubleSRValue = 0;

         foreach (string line in data.Split(new char[] { '\r', '\n' }).Where(x => x.Length > 0))
         {
            string[] values = line.Split(',');

            //int i = 0;
            int intTest = 0;// used in TryParse below. returns 0 if not an int and >0 if an int.

            if (int.TryParse(values[0], out intTest)) // test the first value. if the first value is an int, then we know we are passed the header lines and into data
            {
               // string stringSRValue = values[1];

               doubleSRValue = double.Parse(values[1]); // the 2nd value is the one we want

               SumValue = SumValue + doubleSRValue; // compute the sum but adding current value with previous values
            }
         }

         return Expression.NewNumber(SumValue);
      }
   }


   //SJE
   //TODO: Fix when Refresh() is implemented
   //      When the file has been updated, call refresh
   [ElementName("Read File")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Create an element for reading and watching data in a file on disk.")]
   [RequiresTransaction(false)]
   public class dynFileReader : dynElement
   {
      //System.Windows.Controls.Label label;
      //System.Windows.Controls.ListBox listBox;
      //System.Windows.Controls.TextBox tb;

      //string dataFromFileString;
      //string filePath = "";

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      //public string DataFromFileString
      //{
      //   get { return dataFromFileString; }
      //   set
      //   {
      //      dataFromFileString = value;
      //      NotifyPropertyChanged("DataFromFileString");
      //   }
      //}

      //public string FilePath
      //{
      //   get { return filePath; }
      //   set
      //   {
      //      filePath = value;
      //      NotifyPropertyChanged("FilePath");
      //   }
      //}

      public dynFileReader()
      {
         //StackPanel myStackPanel;

         ////Define a StackPanel
         //myStackPanel = new StackPanel();
         //myStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
         //System.Windows.Controls.Grid.SetRow(myStackPanel, 1);

         //this.inputGrid.Children.Add(myStackPanel);

         ////add a button to the inputGrid on the dynElement
         //System.Windows.Controls.Button readFileButton = new System.Windows.Controls.Button();

         //System.Windows.Controls.Grid.SetColumn(readFileButton, 0); // trying to get this button to be on top... grrr.
         //System.Windows.Controls.Grid.SetRow(readFileButton, 0);
         //readFileButton.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         //readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         //readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         //readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
         //readFileButton.Content = "Read File";
         //readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         //readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         //myStackPanel.Children.Add(readFileButton);


         ////add a list box
         ////label = new System.Windows.Controls.Label();
         //System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox();
         ////tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
         ////tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

         //DataFromFileString = "Ready to read file!";

         ////http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx


         ////this.inputGrid.Children.Add(label);
         ////this.inputGrid.Children.Add(tb);
         ////tb.Visibility = System.Windows.Visibility.Hidden;
         ////System.Windows.Controls.Grid.SetColumn(tb, 0);
         ////System.Windows.Controls.Grid.SetRow(tb, 1);
         //tb.TextWrapping = System.Windows.TextWrapping.Wrap;
         //tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
         //tb.Height = 100;
         ////tb.AcceptsReturn = true;

         //System.Windows.Data.Binding b = new System.Windows.Data.Binding("DataFromFileString");
         //b.Source = this;
         //tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

         //myStackPanel.Children.Add(tb);
         //myStackPanel.Height = 200;

         //InPortData.Add(new PortData(null, "", "The Element to watch", typeof(dynElement)));
         InPortData.Add(new PortData(null, "path", "Path to the file", typeof(string)));
         //InPortData.Add(new PortData(null, "tim", "How often to receive updates.", typeof(dynTimer)));

         OutPortData = new PortData(null, "contents", "File contents", typeof(string));
         //this.Tree.Trunk.Branches.Add(new DataTreeBranch());
         //this.Tree.Trunk.Branches[0].Leaves.Add(DataFromFileString);
         //OutPortData[0].Object = this.Tree;

         base.RegisterInputsAndOutputs();

         //resize the panel
         //this.topControl.Height = 200;
         //this.topControl.Width = 300;
         //UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         //Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
         //this.UpdateLayout();
      }


      //void readFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
      //{
      //   // string txtPath = "C:\\xfer\\dev\\dynamo_git\\test\\text_files\test.txt";
      //   string txtPath = "";

      //   System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog();

      //   if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      //   {
      //      txtPath = openDialog.FileName;
      //      DataFromFileString = txtPath;
      //      readFile(txtPath);
      //      OnDynElementReadyToBuild(EventArgs.Empty);

      //   }


      //   if (string.IsNullOrEmpty(DataFromFileString))
      //   {
      //      string fileError = "Data file could not be opened.";
      //      TaskDialog.Show("Error", fileError);
      //      dynElementSettings.SharedInstance.Writer.WriteLine(fileError);
      //      dynElementSettings.SharedInstance.Writer.WriteLine(txtPath);

      //   }

      //}

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         string arg = ((Expression.String)args[0]).Item;

         StreamReader reader = new StreamReader(new FileStream(arg, FileMode.Open, FileAccess.Read, FileShare.Read));
         string contents = reader.ReadToEnd();
         reader.Close();

         return Expression.NewString(contents);
      }

      //void readFile(string filePath)
      //{
      //   List<string> txtFileList = new List<string>();
      //   string txtFileString = "";

      //   this.Tree.Clear();
      //   //add one branch
      //   this.Tree.Trunk.Branches.Add(new DataTreeBranch());
      //   //this.Tree.Trunk.Branches[0].Leaves.Add("test");

      //   try
      //   {

      //      //this.AddFileWatch(txtPath);
      //      DataFromFileString = ""; //clear old data

      //      //Thread.Sleep(5000); // watch out for potential file contentions

      //      FileStream fs = new FileStream(@filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

      //      // MDJ hack - probably should not create a fs and streamwriter object in a loop, just make them earlier somewhere
      //      StreamReader reader = new StreamReader(fs);
      //      // using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
      //      // {
      //      string line;
      //      while ((line = reader.ReadLine()) != null)
      //      {
      //         //DataFromFileString = DataFromFileString + line;
      //         // this.Tree.Trunk.Leaves.Add(line);
      //         this.Tree.Trunk.Branches[0].Leaves.Add(line);///mdj ask if there is a better way here.
      //         txtFileList.Add(line); // Add to list.
      //         txtFileString = txtFileString + line;
      //         dynElementSettings.SharedInstance.Writer.WriteLine("Reading: " + line);
      //      }
      //      reader.Close();
      //      //reader.Dispose();

      //      //}
      //      // DataFromFileString = this.Tree.ToString();
      //      DataFromFileString = txtFileString;
      //      FilePath = filePath;

      //      OutPortData[0].Object = this.Tree;

      //   }

      //   catch (Exception e)
      //   {
      //      Thread.Sleep(1000); // watch out for potential file contentions
      //   }
      //}

      //public void AddFileWatch(string filePath)
      //{
      //   // Create a new FileSystemWatcher and set its properties.
      //   FileSystemWatcher watcher = new FileSystemWatcher();
      //   watcher.Path = filePath;

      //   try
      //   {
      //      //MDJ hard crash - threading / context issue?

      //      /* Watch for changes in LastAccess and LastWrite times, and
      //         the renaming of files or directories. */
      //      watcher.NotifyFilter = NotifyFilters.LastAccess; //| NotifyFilters.LastWrite
      //      // | NotifyFilters.FileName | NotifyFilters.DirectoryName;
      //      // Only watch text files.
      //      watcher.Filter = "*.csv";

      //      // Add event handlers.
      //      watcher.Changed += new FileSystemEventHandler(OnChanged);
      //      // watcher.Created += new FileSystemEventHandler(OnChanged);
      //      // watcher.Deleted += new FileSystemEventHandler(OnChanged);
      //      // watcher.Renamed += new RenamedEventHandler(OnRenamed);

      //      // Begin watching.
      //      watcher.EnableRaisingEvents = true;

      //   }

      //   catch (Exception e)
      //   {
      //      TaskDialog.Show("Error", e.ToString());
      //   }
      //}

      //// mdj to do - figure out how to dispose of this FileSystemWatcher
      //// Define the event handlers.
      //private static void OnChanged(object source, FileSystemEventArgs e)
      //{
      //   // Specify what is done when a file is changed, created, or deleted.
      //   TaskDialog.Show("File Changed", "File: " + e.FullPath + " " + e.ChangeType);
      //}

      //private static void OnRenamed(object source, RenamedEventArgs e)
      //{
      //   // Specify what is done when a file is renamed.
      //   TaskDialog.Show("File Renamed", "File: +e.OldFullPath + renamed to + e.FullPath");

      //}

      //public override void Draw()
      //{
      //   try
      //   {
      //      //bool boolWatch = Convert.ToBoolean(InPortData[0].Object);

      //      if (CheckInputs() && (File.Exists(FilePath))) // (boolWatch == true)
      //      {

      //         readFile(FilePath);
      //      }

      //      // OutPortData[0].Object = this.Tree;//Hack - seems like overkill to put in the draw loop
      //      //OnDynElementUpdated(EventArgs.Empty);
      //      //OnDynElementReadyToBuild(EventArgs.Empty);

      //      //this.UpdateOutputs();
      //      //OnDynElementReadyToBuild(EventArgs.Empty);

      //      UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
      //      Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
      //   }
      //   catch (Exception e)
      //   {
      //      TaskDialog.Show("Exception", e.ToString());
      //   }
      //}

      //public override void Destroy()
      //{
      //   base.Destroy();
      //}

      //public override void Update()
      //{
      //   //this.topControl.Height = 400;
      //   OnDynElementReadyToBuild(EventArgs.Empty);
      //}
   }


   //SJE
   //TODO: Fix when Refresh() is implemented
   //      When the file has been updated, call refresh
   [ElementName("Watch File")]
   [ElementCategory(BuiltinElementCategories.MISC)]
   [ElementDescription("Create an element for reading and watching data in a file on disk.")]
   [RequiresTransaction(false)]
   public class dynFileWatcher : dynElement
   {
      //System.Windows.Controls.Label label;
      //System.Windows.Controls.ListBox listBox;
      System.Windows.Controls.TextBox tb;

      string dataFromFileString;
      string filePath = "";

      //public event PropertyChangedEventHandler PropertyChanged;

      //private void NotifyPropertyChanged(String info)
      //{
      //   if (PropertyChanged != null)
      //   {
      //      PropertyChanged(this, new PropertyChangedEventArgs(info));
      //   }
      //}

      public string DataFromFileString
      {
         get { return dataFromFileString; }
         set
         {
            dataFromFileString = value;
            NotifyPropertyChanged("DataFromFileString");
         }
      }

      public string FilePath
      {
         get { return filePath; }
         set
         {
            filePath = value;
            NotifyPropertyChanged("FilePath");
         }
      }

      private FileSystemWatcher watcher;

      public dynFileWatcher()
      {
         StackPanel myStackPanel;

         //Define a StackPanel
         myStackPanel = new StackPanel();
         myStackPanel.Orientation = System.Windows.Controls.Orientation.Vertical;
         System.Windows.Controls.Grid.SetRow(myStackPanel, 1);

         this.inputGrid.Children.Add(myStackPanel);

         //add a button to the inputGrid on the dynElement
         System.Windows.Controls.Button readFileButton = new System.Windows.Controls.Button();

         System.Windows.Controls.Grid.SetColumn(readFileButton, 0); // trying to get this button to be on top... grrr.
         System.Windows.Controls.Grid.SetRow(readFileButton, 0);
         readFileButton.Margin = new System.Windows.Thickness(0, 0, 0, 0);
         readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
         readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
         readFileButton.Click += new System.Windows.RoutedEventHandler(readFileButton_Click);
         readFileButton.Content = "Read File";
         readFileButton.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
         readFileButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;

         myStackPanel.Children.Add(readFileButton);


         //add a list box
         //label = new System.Windows.Controls.Label();
         tb = new System.Windows.Controls.TextBox();
         //tb.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
         //tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;

         DataFromFileString = "Ready to read file!";

         //http://learnwpf.com/post/2006/06/12/How-can-I-create-a-data-binding-in-code-using-WPF.aspx


         //this.inputGrid.Children.Add(label);
         //this.inputGrid.Children.Add(tb);
         //tb.Visibility = System.Windows.Visibility.Hidden;
         //System.Windows.Controls.Grid.SetColumn(tb, 0);
         //System.Windows.Controls.Grid.SetRow(tb, 1);
         tb.TextWrapping = System.Windows.TextWrapping.Wrap;
         tb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
         tb.Height = 100;
         //tb.AcceptsReturn = true;

         System.Windows.Data.Binding b = new System.Windows.Data.Binding("DataFromFileString");
         b.Source = this;
         tb.SetBinding(System.Windows.Controls.TextBox.TextProperty, b);

         myStackPanel.Children.Add(tb);
         myStackPanel.Height = 200;

         OutPortData = new PortData(null, "contents", "downstream data", typeof(object));

         base.RegisterInputsAndOutputs();

         //resize the panel
         this.topControl.Height = 200;
         this.topControl.Width = 300;
         UpdateLayoutDelegate uld = new UpdateLayoutDelegate(CallUpdateLayout);
         Dispatcher.Invoke(uld, System.Windows.Threading.DispatcherPriority.Background, new object[] { this });
         //this.UpdateLayout();
      }


      void readFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         // string txtPath = "C:\\xfer\\dev\\dynamo_git\\test\\text_files\test.txt";
         System.Windows.Forms.OpenFileDialog openDialog = new OpenFileDialog();

         if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {
            FilePath = openDialog.FileName;
            AddFileWatch(FilePath);
         }

         if (string.IsNullOrEmpty(DataFromFileString))
         {
            string fileError = "Data file could not be opened.";
            TaskDialog.Show("Error", fileError);
            dynElementSettings.SharedInstance.Writer.WriteLine(fileError);
            dynElementSettings.SharedInstance.Writer.WriteLine(FilePath);
         }
      }

      public void AddFileWatch(string filePath)
      {
         if (this.watcher != null)
         {
            this.watcher.Changed -= new FileSystemEventHandler(OnChanged);
            this.watcher.Dispose();
         }

         // Create a new FileSystemWatcher and set its properties.
         this.watcher = new FileSystemWatcher(
            Path.GetDirectoryName(filePath),
            Path.GetFileName(filePath)
         );

         try
         {
            //MDJ hard crash - threading / context issue?

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            // | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            //watcher.Filter = "*.csv";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            // watcher.Created += new FileSystemEventHandler(OnChanged);
            // watcher.Deleted += new FileSystemEventHandler(OnChanged);
            // watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            fileChanged = true;
         }

         catch (Exception e)
         {
            TaskDialog.Show("Error", e.ToString());
         }
      }

      private bool fileChanged = false;

      // mdj to do - figure out how to dispose of this FileSystemWatcher
      // Define the event handlers.
      private void OnChanged(object source, FileSystemEventArgs e)
      {
         fileChanged = true;

         // Specify what is done when a file is changed, created, or deleted.
         //this.Dispatcher.BeginInvoke(new Action(
         //   () =>
         //      dynElementSettings.SharedInstance.Bench.Log("File Changed: " + e.FullPath + " " + e.ChangeType)
         //));
      }

      public override Expression Evaluate(FSharpList<Expression> args)
      {
         FileStream fs;

         int tick = 0;
         while (!fileChanged || isFileInUse(@FilePath, out fs))
         {
            Thread.Sleep(10);
            tick += 10;

            if (tick >= 5000)
            {
               throw new Exception("File watcher timeout!");
            }
         }

         StreamReader reader = new StreamReader(fs);

         string result = reader.ReadToEnd();

         reader.Close();

         fileChanged = false;

         return Expression.NewString(result);
      }

      private bool isFileInUse(string path, out FileStream stream)
      {
         try
         {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
         }
         catch (IOException)
         {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            stream = null;
            return true;
         }

         //file is not locked
         return false;
      }
   }


   //MDJ

   #endregion

   #region class attributes
   [AttributeUsage(AttributeTargets.All)]
   public class ElementNameAttribute : System.Attribute
   {
      public string ElementName { get; set; }

      public ElementNameAttribute(string elementName)
      {
         this.ElementName = elementName;
      }
   }

   [AttributeUsage(AttributeTargets.All)]
   public class ElementCategoryAttribute : System.Attribute
   {
      public string ElementCategory { get; set; }

      public ElementCategoryAttribute(string category)
      {
         this.ElementCategory = category;
      }
   }

   [AttributeUsage(AttributeTargets.All)]
   public class ElementSearchTagsAttribute : System.Attribute
   {
      public List<string> Tags { get; set; }

      public ElementSearchTagsAttribute(params string[] tags)
      {
         this.Tags = tags.ToList();
      }
   }

   [AttributeUsage(AttributeTargets.All)]
   public class IsInteractiveAttribute : System.Attribute
   {
      public bool IsInteractive { get; set; }

      public IsInteractiveAttribute(bool isInteractive)
      {
         this.IsInteractive = isInteractive;
      }
   }

   [AttributeUsage(AttributeTargets.All)]
   public class RequiresTransactionAttribute : System.Attribute
   {
      private bool requiresTransaction;

      public bool RequiresTransaction
      {
         get { return requiresTransaction; }
         set { requiresTransaction = value; }
      }

      public RequiresTransactionAttribute(bool requiresTransaction)
      {
         this.requiresTransaction = requiresTransaction;
      }
   }

   [AttributeUsage(AttributeTargets.All)]
   public class ElementDescriptionAttribute : System.Attribute
   {
      private string description;

      public string ElementDescription
      {
         get { return description; }
         set { description = value; }
      }

      public ElementDescriptionAttribute(string description)
      {
         this.description = description;
      }
   }
   #endregion

}


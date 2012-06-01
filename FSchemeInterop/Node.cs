using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
   //Namespace for all mutable Nodes.
   namespace Node
   {
      public class ApplierNodeException : Exception
      {
         public ApplierNodeException(string details) : base(details) { }
      }

      //Common Node interface. All nodes can be compiled into FScheme Expressions.
      public interface INode
      {
         //Converts this Node into an FScheme Expression.
         Expression Compile();
      }

      public class ExpressionNode : INode
      {
         Expression expr;

         public ExpressionNode(Expression v)
         {
            this.expr = v;
         }

         public Expression Compile()
         {
            return this.expr;
         }
      }

      //Node representing an FScheme Number.
      public class NumberNode : INode
      {
         double num;

         public NumberNode(double v)
         {
            this.num = v;
         }

         public Expression Compile()
         {
            return Expression.NewNumber(this.num);
         }
      }

      //Node representing an FScheme String.
      public class StringNode : INode
      {
         string str;

         public StringNode(string s)
         {
            this.str = s;
         }

         public Expression Compile()
         {
            return Expression.NewString(this.str);
         }
      }

      //Node representing an FScheme Container for objects.
      public class ObjectNode : INode
      {
         object obj;

         public ObjectNode(object o)
         {
            this.obj = o;
         }

         public Expression Compile()
         {
            return Expression.NewContainer(this.obj);
         }
      }

      //Node representing an FScheme Symbol.
      public class SymbolNode : INode
      {
         string symbol;

         public SymbolNode(string s)
         {
            this.symbol = s;
         }

         public Expression Compile()
         {
            return Expression.NewSymbol(this.symbol);
         }
      }

      //Node representing an FScheme If statement.
      public class ConditionalNode : FunctionNode
      {
         public ConditionalNode()
            : base("if", new List<string>() { "test", "true", "false" }) { }

         public override Expression Compile()
         {
            return Utils.mkExprList(
               Expression.NewSymbol("if"),
               this.arguments["test"].Compile(),
               this.arguments["true"].Compile(),
               this.arguments["false"].Compile()
            );
         }
      }

      public class BeginNode : FunctionNode
      {
         public BeginNode()
            : base("begin", new List<string>() { "expr1", "expr2" }) 
         { }

         public BeginNode(IEnumerable<string> inputs)
            : base("begin", inputs) 
         { }

         public override Expression Compile()
         {
            return Expression.NewList(
               FSharpList<Expression>.Cons(
                  Expression.NewSymbol("begin"),
                  Utils.convertSequence(this.Inputs.Select(x => this.arguments[x].Compile()))
               )
            );

            //return Utils.mkExprList(
            //   Expression.NewSymbol("begin"),
            //   this.arguments["expr1"].Compile(),
            //   this.arguments["expr2"].Compile()
            //);
         }
      }

      public abstract class ProcedureCallNode : INode
      {
         protected abstract Expression Body { get; }

         protected virtual List<string> Inputs
         {
            get
            {
               return this.inputs;
            }
            set
            {
               this.inputs = value;
            }
         }

         //List of input parameters this function takes.
         private List<string> inputs;

         //Dictionary mapping inputs to nodes, used to evaluate arguments.
         protected Dictionary<string, INode> arguments = new Dictionary<string, INode>();

         public ProcedureCallNode(IEnumerable<string> inputNames)
         {
            this.Inputs = inputNames.ToList();
         }

         public ProcedureCallNode(params string[] inputNames)
            : this(inputNames.ToList())
         { }

         //Connects a Node to one of our inputs.
         public virtual void ConnectInput(string inputName, INode inputNode)
         {
            this.arguments[inputName] = inputNode;
         }

         //Disconnects one of our inputs.
         public virtual void DisconnectInput(string inputName)
         {
            this.arguments.Remove(inputName);
         }

         //Adds another input parameter with the given name.
         public virtual void AddInput(string inputName)
         {
            this.inputs.Add(inputName);
         }

         //Adds another input parameter with a default name.
         public virtual void AddInput()
         {
            this.AddInput("arg" + (this.inputs.Count + 1));
         }

         //Removes an input parameter of the given name.
         public virtual void RemoveInput(string inputName)
         {
            this.inputs.Remove(inputName);
         }

         //Removes the last input parameter.
         public virtual void RemoveInput()
         {
            this.inputs.RemoveAt(this.inputs.Count - 1);
         }

         /// <summary>
         /// Compiles the node into an FScheme Expression.
         /// </summary>
         /// <returns></returns>
         public virtual Expression Compile()
         {
            return this.toExpression(
               this.Body,
               this.Inputs,
               this.inputs.Count
            );
         }

         //Function used to construct our expression. This is used to properly create a curried function call, which will be
         //able to support partial function application.
         protected Expression toExpression(Expression function, IEnumerable<string> parameters, int expectedArgs)
         {
            //If no arguments have been supplied and if we are expecting arguments, simply return the function.
            if (this.arguments.Keys.Count == 0 && expectedArgs > 0)
               return function;

            //If the number of expected arguments is greater than how many arguments have been supplied, we perform a partial
            //application, returning a function which takes the remaining arguments.
            if (this.arguments.Keys.Count < expectedArgs)
            {
               //Get all of the missing arguments.
               IEnumerable<string> missingArgs = parameters.Where(
                  input => !this.arguments.ContainsKey(input)
               );
               //Return a function that...
               return Utils.MakeAnon(
                  //...takes all of the missing arguments...
                  missingArgs.ToList(),
                  Expression.NewList(
                     FSharpList<Expression>.Cons(
                  //...and calls this function...
                        function,
                        Utils.convertSequence(
                  //...with the arguments which were supplied.
                           parameters.Select(input => missingArgs.Contains(input)
                                                   ? Expression.NewSymbol(input)
                                                   : this.arguments[input].Compile()
                           )
                        )
                     )
                  )
               );
            }

            //If all the arguments were supplied, just return a standard function call expression.
            else
            {
               return Expression.NewList(
                  FSharpList<Expression>.Cons(
                     function,
                     Utils.convertSequence(
                        parameters.Select(input => this.arguments[input].Compile())
                     )
                  )
               );
            }
         }
      }

      //Node representing an FScheme Function Application.
      public class ApplierNode : ProcedureCallNode
      {
         //The procedure to be executed.
         protected INode procedure;

         protected override List<string> Inputs
         {
            get
            {
               return base.Inputs.Skip(1).ToList();
            }
            set
            {
               base.Inputs = value;
            }
         }

         protected override Expression Body
         {
            get { return this.procedure.Compile(); }
         }

         public ApplierNode(IEnumerable<string> inputs)
            : base(new List<string>() { "func" }.Concat(inputs))
         { }

         public ApplierNode()
            : base(new List<string>() { "func" })
         { }

         //ConnectInput(string, INode) overridden from FunctionNode
         public override void ConnectInput(string inputName, INode inputNode)
         {
            //Special case: if we are connecting to procedure, update our internal procedure reference.
            if (inputName.Equals("func"))
            {
               this.procedure = inputNode;
            }
            base.ConnectInput(inputName, inputNode);
         }

         //AddInput() overridden from FunctionNode
         public override void AddInput()
         {
            base.Inputs.Add("arg" + this.Inputs.Count + 1);
         }

         //RemoveInput() overridden from FunctinNode
         public override void RemoveInput()
         {
            //TODO
            if (this.Inputs.Count == 0)
               throw new ApplierNodeException("Cannot remove Procedure parameter from Apply Node.");
            else
               base.RemoveInput();
         }

         //RemoveInput(string) overridden from FunctionNode
         public override void RemoveInput(string inputName)
         {
            //TODO
            if (inputName.Equals("procedure"))
               throw new ApplierNodeException("Cannot remove Procedure parameter from Apply Node.");
            else
               base.RemoveInput(inputName);
         }
      }

      //Node representing an FScheme Function Call, where the function being called is referenced
      //by a symbol.
      public class FunctionNode : ProcedureCallNode
      {
         public string Symbol { get; private set; }

         //Symbol referenced by this function.
         protected override Expression Body
         {
            get
            {
               return new SymbolNode(this.Symbol).Compile();
            }
         }

         public FunctionNode(string funcName, IEnumerable<string> inputNames)
            : base(inputNames)
         {
            this.Symbol = funcName;
         }

         public FunctionNode(string funcName)
            : this(funcName, new List<string>()) { }
      }

      //Node representing an anonymous FScheme Function object.
      public class AnonymousFunctionNode : ProcedureCallNode
      {
         //The Node representing the FScheme Expression to be evaluated as the body
         //of this function.
         public INode EntryPoint { get; private set; }

         protected override Expression Body
         {
            get
            {
               return Utils.MakeAnon(this.Inputs, this.EntryPoint.Compile());
            }
         }

         public AnonymousFunctionNode(IEnumerable<string> inputList, INode entryPoint)
            : base(inputList)
         {
            this.EntryPoint = entryPoint;
         }

         public AnonymousFunctionNode(INode entryPoint)
            : this(new List<string>(), entryPoint) { }
      }

      public class ExternalFunctionNode : ProcedureCallNode
      {
         public FScheme.ExternFunc EntryPoint { get; private set; }

         protected override Expression Body
         {
            get
            {
               return FuncContainer.MakeFunction(this.EntryPoint);
            }
         }

         public ExternalFunctionNode(FScheme.ExternFunc f, IEnumerable<string> inputList)
            : base(inputList)
         {
            this.EntryPoint = f;
         }

         public ExternalFunctionNode(FScheme.ExternFunc f)
            : this(f, new List<string>())
         { }
      }

      public class ExternalMacroNode : ProcedureCallNode
      {
         public ExternMacro EntryPoint { get; private set; }

         protected override Expression Body
         {
            get 
            {
               return FuncContainer.MakeMacro(this.EntryPoint); 
            }
         }

         public ExternalMacroNode(ExternMacro m, IEnumerable<string> inputList)
            : base(inputList)
         {
            this.EntryPoint = m;
         }

         public ExternalMacroNode(ExternMacro m)
            : this(m, new List<string>())
         { }
      }
   }
}

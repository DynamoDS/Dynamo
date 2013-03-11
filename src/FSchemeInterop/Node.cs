//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;

using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop.Node
{
    public class ApplierNodeException : Exception
    {
        public ApplierNodeException(string details) : base(details) { }
    }

    ///<summary>
    ///Common Node interface. All nodes can be compiled into FScheme.Expressions.
    ///</summary>
    public abstract class INode
    {
        /// <summary>
        /// Converts this Node into an FScheme Expression.
        /// </summary>
        /// <returns></returns>
        public Expression Compile()
        {
            var symbols = new Dictionary<INode, string>();
            var letEntries = new Dictionary<INode, List<INode>>();

            if (!GraphAnalysis.LetOptimizations(this, out symbols, out letEntries))
                throw new Exception("Can't compile INode, graph is not a DAG.");

            return compile(symbols, letEntries, new HashSet<string>());
        }

        private static Expression wrapLets(
            Expression body,
            Dictionary<INode, string> symbols,
            List<INode> bindings)
        {
            return Expression.NewLet(
                Utils.SequenceToFSharpList(bindings.Select(x => symbols[x])),
                Utils.SequenceToFSharpList(
                    Enumerable.Repeat(
                        Expression.NewBegin(FSharpList<Expression>.Empty), 
                        bindings.Count)),
                body);
        }

        private Expression __compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            string symbol;
            if (symbols.TryGetValue(this, out symbol))
            {
                var body = Expression.NewId(symbol);
                if (!initializedIds.Contains(symbol))
                {
                    symbols.Remove(this);
                    var binding = compile(symbols, letEntries, initializedIds);
                    symbols[this] = symbol;

                    body = Expression.NewBegin(Utils.MakeFSharpList(
                        Expression.NewSetId(symbol,binding),
                        body));

                    initializedIds.Add(symbol);
                }
                return body;
            }
            else
                return compileBody(symbols, letEntries, initializedIds);
        }

        public Expression compile(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            Expression body = __compileBody(symbols, letEntries, initializedIds);

            List<INode> bindings;
            if (letEntries.TryGetValue(this, out bindings) && bindings.Any())
                body = wrapLets(body, symbols, bindings);

            return body;
        }

        protected abstract Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds);

        protected internal List<INode> children = new List<INode>();
        protected internal List<INode> parents = new List<INode>();

        /// <summary>
        /// All inputs to this node.
        /// </summary>
        public IEnumerable<INode> Children { get { return children; } }

        public int ChildCount { get { return children.Count; } }
        public int ParentCount { get { return parents.Count; } }

        /// <summary>
        /// All outputs from this node.
        /// </summary>
        public IEnumerable<INode> Parents { get { return parents; } }
    }

    //public class ExpressionNode : INode
    //{
    //    Expression expr;

    //    public ExpressionNode(Expression v)
    //    {
    //        expr = v;
    //    }

    //    public Expression Compile()
    //    {
    //        return expr;
    //    }
    //}

    //Node representing an FScheme Number.
    public class NumberNode : INode
    {
        double num;

        public NumberNode(double v)
        {
            num = v;
        }

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewNumber_E(num);
        }
    }

    //Node representing an FScheme String.
    public class StringNode : INode
    {
        string str;

        public StringNode(string s)
        {
            str = s;
        }

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewString_E(str);
        }
    }

    //Node representing an FScheme Container for objects.
    public class ObjectNode : INode
    {
        object obj;

        public ObjectNode(object o)
        {
            obj = o;
        }

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewContainer_E(obj);
        }
    }

    //Node representing an FScheme Symbol.
    public class SymbolNode : INode
    {
        string symbol;

        public SymbolNode(string s)
        {
            symbol = s;
        }

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewId(symbol);
        }
    }

    //Node representing an FScheme If statement.
    public class ConditionalNode : FunctionNode
    {
        public ConditionalNode(IEnumerable<string> inputs)
            : base("if", inputs.Take(3))
        {
            var inputList = inputs.ToList();

            if (inputList.Count != 3)
                throw new Exception("Conditional Node takes exactly 3 inputs.");

            _test = inputList[0];
            _true = inputList[1];
            _false = inputList[2];
        }

        public ConditionalNode()
            : base("if", new List<string>() { "test", "true", "false" }) { }

        string _test;
        string _true;
        string _false;

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewIf(
                arguments[_test].compile(symbols, letEntries, initializedIds),
                arguments[_true].compile(symbols, letEntries, initializedIds),
                arguments[_false].compile(symbols, letEntries, initializedIds));
        }
    }

    public class BeginNode : FunctionNode
    {
        public BeginNode()
            : base("begin", new List<string>())
        { }

        public BeginNode(IEnumerable<string> inputs)
            : base("begin", inputs)
        { }

        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewBegin(
                Utils.SequenceToFSharpList(
                    Inputs.Select(
                        x => arguments[x].compile(symbols, letEntries, initializedIds))));
        }
    }

    public abstract class InputNode : INode
    {
        protected virtual List<string> Inputs
        {
            get
            {
                return inputs;
            }
            set
            {
                inputs = value;
            }
        }

        //List of input parameters this function takes.
        protected List<string> inputs;

        //Dictionary mapping inputs to nodes, used to evaluate arguments.
        protected Dictionary<string, INode> arguments = new Dictionary<string, INode>();

        public InputNode(IEnumerable<string> inputNames)
        {
            Inputs = inputNames.ToList();
        }

        public InputNode(params string[] inputNames)
            : this(inputNames.ToList())
        { }

        //Connects a Node to one of our inputs.
        public virtual void ConnectInput(string inputName, INode inputNode)
        {
            arguments[inputName] = inputNode;
            children.Add(inputNode);
            inputNode.parents.Add(this);
        }

        //Disconnects one of our inputs.
        public virtual void DisconnectInput(string inputName)
        {
            var child = arguments[inputName];
            child.parents.Remove(this);
            children.Remove(child);

            arguments.Remove(inputName);
        }

        //Adds another input parameter with the given name.
        public virtual void AddInput(string inputName)
        {
            inputs.Add(inputName);
        }

        //Adds another input parameter with a default name.
        public virtual void AddInput()
        {
            AddInput("arg" + (inputs.Count + 1));
        }

        //Removes an input parameter of the given name.
        public virtual void RemoveInput(string inputName)
        {
            if (arguments.ContainsKey(inputName))
            {
                DisconnectInput(inputName);
            }
            inputs.Remove(inputName);
        }

        //Removes the last input parameter.
        public virtual void RemoveInput()
        {
            RemoveInput(inputs[inputs.Count - 1]);
        }
    }

    public abstract class ProcedureCallNode : InputNode
    {
        protected abstract Expression GetBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds);

        public ProcedureCallNode(IEnumerable<string> inputNames) 
            : base(inputNames) 
        { }
        
        /// <summary>
        /// Compiles the node into an FScheme Expression.
        /// </summary>
        /// <returns></returns>
        protected override Expression compileBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return toExpression(
                GetBody(symbols, letEntries, initializedIds), 
                Inputs, 
                inputs.Count, 
                symbols, 
                letEntries, 
                initializedIds);
        }

        //Function used to construct our expression. This is used to properly create a curried function call, which will be
        //able to support partial function application.
        protected Expression toExpression(
            Expression function, 
            IEnumerable<string> parameters,
            int expectedArgs,
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            //If no arguments have been supplied and if we are expecting arguments, simply return the function.
            if (arguments.Keys.Count == 0 && expectedArgs > 0)
                return function;

            //If the number of expected arguments is greater than how many arguments have been supplied, we perform a partial
            //application, returning a function which takes the remaining arguments.
            if (arguments.Keys.Count < expectedArgs)
            {
                //Get all of the missing arguments.
                IEnumerable<string> missingArgs = parameters.Where(
                   input => !arguments.ContainsKey(input)
                );
                //Return a function that...
                return Utils.MakeAnon(
                    //...takes all of the missing arguments...
                   missingArgs.ToList(),
                   Expression.NewList_E(
                      FSharpList<Expression>.Cons(
                    //...and calls this function...
                         function,
                         Utils.SequenceToFSharpList(
                    //...with the arguments which were supplied.
                             parameters.Select(
                                input =>
                                    missingArgs.Contains(input)
                                    ? Expression.NewId(input)
                                    : arguments[input].compile(symbols, letEntries, initializedIds))))));
            }

            //If all the arguments were supplied, just return a standard function call expression.
            else
            {
                return Expression.NewList_E(
                   FSharpList<Expression>.Cons(
                      function,
                      Utils.SequenceToFSharpList(
                         parameters.Select(input => arguments[input].compile(symbols, letEntries, initializedIds))
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

        protected string procedureInput;

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

        protected override Expression GetBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return procedure.compile(symbols, letEntries, initializedIds);
        }

        public ApplierNode(IEnumerable<string> inputs)
            : base(inputs)
        {
            procedureInput = inputs.First();
        }

        //ConnectInput(string, INode) overridden from FunctionNode
        public override void ConnectInput(string inputName, INode inputNode)
        {
            //Special case: if we are connecting to procedure, update our internal procedure reference.
            if (inputName.Equals(procedureInput))
            {
                procedure = inputNode;
            }
            base.ConnectInput(inputName, inputNode);
        }

        //AddInput() overridden from FunctionNode
        public override void AddInput()
        {
            base.Inputs.Add("arg" + Inputs.Count + 1);
        }

        //RemoveInput() overridden from FunctinNode
        public override void RemoveInput()
        {
            //TODO
            if (Inputs.Count == 0)
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
        protected override Expression GetBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewId(Symbol);
        }

        public FunctionNode(string funcName, IEnumerable<string> inputNames)
            : base(inputNames)
        {
            Symbol = funcName;
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

        protected override Expression GetBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Utils.MakeAnon(Inputs, EntryPoint.compile(symbols, letEntries, initializedIds));
        }

        public AnonymousFunctionNode(IEnumerable<string> inputList, INode entryPoint)
            : base(inputList)
        {
            EntryPoint = entryPoint;
        }

        public AnonymousFunctionNode(INode entryPoint)
            : this(new List<string>(), entryPoint) { }
    }

    public class ExternalFunctionNode : ProcedureCallNode
    {
        public Converter<FSharpList<Value>, Value> EntryPoint { get; private set; }

        protected override Expression GetBody(
            Dictionary<INode, string> symbols,
            Dictionary<INode, List<INode>> letEntries,
            HashSet<string> initializedIds)
        {
            return Expression.NewFunction_E(
                Utils.ConvertToFSchemeFunc(EntryPoint));
        }

        public ExternalFunctionNode(Converter<FSharpList<Value>, Value> f, IEnumerable<string> inputList)
            : base(inputList)
        {
            EntryPoint = f;
        }

        public ExternalFunctionNode(Converter<FSharpList<Value>, Value> f)
            : this(f, new List<string>())
        { }
    }

    /// <summary>
    /// Utility functions involving Node graph traversal.
    /// </summary>
    public static class GraphAnalysis
    {
        private static Tree<T> lca<T>(IEnumerable<Tree<T>> descendants)
        {
            var trees = new Stack<Tree<T>>(descendants);

            if (!trees.Any()) 
                return null;

            while (trees.Count > 1)
            {
                var stackA = new Stack<Tree<T>>();
                for (var pathA = trees.Pop(); pathA != null; pathA = pathA.Parent)
                {
                    stackA.Push(pathA);
                }

                var stackB = new Stack<Tree<T>>();
                for (var pathB = trees.Pop(); pathB != null; pathB = pathB.Parent)
                {
                    stackB.Push(pathB);
                }

                Tree<T> result = null;
                while (stackA.Any() && stackB.Any())
                {
                    var a = stackA.Pop();
                    var b = stackB.Pop();
                    if (a == b)
                    {
                        result = a;
                    }
                    else
                        break;
                }

                if (result == null)
                    return null;

                trees.Push(result);
            }

            return trees.Pop();
        }

        /*
        private static bool checkConstant(dynNode node)
        {
            return node.GetType()
                .GetCustomAttributes(typeof(IsConstantAttribute), false)
                .Any(x => (x as IsConstantAttribute).IsConstant);
        }
        */

        public static bool LetOptimizations(
            INode entry, 
            out Dictionary<INode, string> symbols, 
            out Dictionary<INode, List<INode>> letEntries)
        {
            symbols = new Dictionary<INode, string>();
            letEntries = new Dictionary<INode, List<INode>>();
            
            var nodeStack = new Stack<INode>();

            var multiOuts = new Stack<INode>();

            var isDag = TopologicalTraversal(
                entry, 
                x => 
                {
                    nodeStack.Push(x);
                    //TODO: Add CanOptimize property to INode
                    if (x.ParentCount > 1 && x.Children.Any())
                        multiOuts.Push(x);
                },
                x => x.Children);

            if (!isDag)
                return false;

            var lcaTree = Tree<INode>.MakeTree(null);
            var treeLookup = new Dictionary<INode, Tree<INode>>();

            foreach (var node in nodeStack)
            {
                if (node.Parents.Any())
                {
                    treeLookup[node] =
                        lca(node.Parents.Select(x => treeLookup[x]))
                        .AddChild(node);
                }
                else
                {
                    treeLookup[node] = lcaTree.AddChild(node);
                }
            }

            foreach (var node in multiOuts)
            {
                symbols[node] = Guid.NewGuid().ToString();

                var parent = treeLookup[node].Parent;

                if (parent.Value != null)
                {
                    List<INode> childList;
                    if (letEntries.TryGetValue(parent.Value, out childList))
                        childList.Add(node);
                    else
                        letEntries[parent.Value] = new List<INode>() { node };
                }
            }

            return true;
        }

        /// <summary>
        /// Traverses the graph in depth-first topological order.
        /// </summary>
        /// <returns></returns>
        public static bool TopologicalTraversal<T>(T entryPoint, Action<T> onMark, Converter<T, IEnumerable<T>> childAccessor)
        {
            return visit(entryPoint, new HashSet<T>(), new HashSet<T>(), onMark, childAccessor);
        }

        private static bool visit<T>(T node, HashSet<T> temps, HashSet<T> perms, Action<T> onMark, Converter<T, IEnumerable<T>> childAccessor)
        {
            if (temps.Contains(node))
                return false;

            if (!perms.Contains(node))
            {
                temps.Add(node);
                foreach (var child in childAccessor(node))
                {
                    if (!visit(child, temps, perms, onMark, childAccessor))
                        return false;
                }
                perms.Add(node);
                temps.Remove(node);
                onMark(node);
            }

            return true;
        }

        private class Tree<T>
        {
            public T Value { get; private set; }
            public Tree<T> Parent { get; private set; }

            private HashSet<Tree<T>> children = new HashSet<Tree<T>>();
            public IEnumerable<Tree<T>> Children { get { return children; } }

            public bool IsRoot { get { return Parent == null; } }

            Tree(T value) : this(value, null) { }

            Tree(T value, Tree<T> parent)
            {
                Value = value;
                Parent = parent;
            }

            public static Tree<T> MakeTree(T root)
            {
                return new Tree<T>(root);
            }

            public Tree<T> AddChild(T child)
            {
                var newChild = new Tree<T>(child, this);
                children.Add(newChild);
                return newChild;
            }

            public override string ToString()
            {
                return "( " + (Value == null ? "null" : Value.ToString()) + ": " + String.Join(" ", children.Select(x => x.ToString())) + ")";
            }
        }
    }
}

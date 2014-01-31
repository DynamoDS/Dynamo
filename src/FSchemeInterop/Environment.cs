using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Value = Dynamo.FScheme.Value;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
    //Class wrapping an FScheme environment.
    internal class EnvironmentWrapper
    {
        //Our FScheme environment.
        private FScheme.FSchemeEnvironment env;

        /// <summary>
        /// Environment containing compile-time symbols.
        /// </summary>
        public FSharpRef<FSharpList<FSharpList<string>>> CompilationEnvironment 
        {
            get { return env.cEnv; }
        }

        /// <summary>
        /// Environment containing runtime values. indices correspond to symbols
        /// in CompilationEnvironemnt.
        /// </summary>
        public FSharpRef<FSharpList<FSharpRef<FSharpRef<Value>[]>>> RuntimeEnvironment
        {
            get { return env.rEnv; }
        }

        //Sets the environment contained by this EnvironmentWrapper
        //to the one provided by FScheme by default.
        public EnvironmentWrapper()
        {
            env = FScheme.CreateEnvironments();
        }

        /// <summary>
        /// Removes an identifier from this environment.
        /// </summary>
        /// <param name="symbol">Identifier to remove.</param>
        public void Delete(string symbol)
        {
            var removedindices = new HashSet<int>();

            this.CompilationEnvironment.Value = Utils.ToFSharpList(
                this.CompilationEnvironment.Value.Select(
                    x => Utils.ToFSharpList(
                        x.Where((y, i) => 
                            {
                                var remove = !symbol.Equals(y);
                                if (remove)
                                    removedindices.Add(i);
                                return remove; 
                            })))
                .Where(x => x.Any()));

            this.RuntimeEnvironment.Value.Head.Value = this.RuntimeEnvironment.Value.Head.Value.Where(
                (_, i) => !removedindices.Contains(i)
            ).ToArray();
        }

        public override string ToString()
        {
            return this.CompilationEnvironment.ToString() + "\n" + this.RuntimeEnvironment.ToString();
        }
    }

    //Class representing an FScheme Execution Environment. Used to evaluate FScheme Expressions.
    public class ExecutionEnvironment
    {
        //Environment used to store symbols.
        private EnvironmentWrapper env;

        //private FSharpRef<FSharpMap<string, FSharpRef<Value>>> frozenEnv;

        //Default constructor, simply creates a default environment.
        public ExecutionEnvironment()
        {
            this.env = new EnvironmentWrapper();
            //this.frozenEnv = FScheme.EnvironmentMap;
        }

        //Binds symbols of the given string to the given body.
        public void DefineSymbol(string name, Expression body)
        {
            Evaluate(Expression.NewDefine(name, body));

            //frozenEnv.Value = frozenEnv.Value.Add(name, env.Lookup(name));
        }

        //private void add(string name, Value v)
        //{
        //    var box = env.Add(name, v);

        //    //if (frozenEnv.Value.ContainsKey(name))
        //    //    frozenEnv.Value[name].Value = v;
        //    //else
        //    //    frozenEnv.Value = frozenEnv.Value.Add(name, box);
        //}

        //Binds symbols of the given string to the given External Function.
        public void DefineExternal(string name, Converter<FSharpList<Value>, Value> func)
        {
            DefineExternal(
                name,
                Utils.ConvertToFSharpFunc(func));
        }

        //Binds symbols of the given string to the given External Function.
        public void DefineExternal(string name, FSharpFunc<FSharpList<Value>, Value> func)
        {
            //add(name, Value.NewFunction(func));
            Evaluate(Expression.NewDefine(name, Expression.NewFunction_E(func)));
        }

        /// <summary>
        /// Evaluates the given expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public Value Evaluate(Expression expr)
        {
            return FScheme.EvaluateInEnvironment
                .Invoke(this.env.CompilationEnvironment)
                .Invoke(this.env.RuntimeEnvironment)
                .Invoke(expr);
        }

        ///Removes the given symbol from the environment.
        public void RemoveSymbol(string p)
        {
            //TODO: Implement

            //this.env.Delete(p); //no worky?
            
            //frozenEnv.Value = frozenEnv.Value.Remove(p);
        }

        /// <summary>
        /// Looks up the value associated with the given symbol in this environment.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Value LookupSymbol(string p)
        {
            //try
            //{
            //    return this.frozenEnv.Value[p];
            //}
            //catch (Exception)
            //{
            //    throw new Exception("Could not find key " + p + " in environment");
            //}
            //return env.Lookup(p);
            return Evaluate(Expression.NewId(p));
        }

        public override string ToString()
        {
            return env.ToString();
        }
    }
}

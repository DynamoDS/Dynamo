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

using Value = Dynamo.FScheme.Value;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
    //Class wrapping an FScheme environment.
    internal class EnvironmentWrapper
    {
        //Our FScheme compilation environment.
        private FSharpRef<FSharpList<FSharpList<string>>> cEnv;

        //Our FScheme runtime environment.
        private FSharpRef<FSharpList<FSharpRef<FSharpRef<Value>[]>>> rEnv;

        /// <summary>
        /// Environment containing compile-time symbols.
        /// </summary>
        public FSharpRef<FSharpList<FSharpList<string>>> CompilationEnvironment 
        {
            get { return cEnv; }
        }

        /// <summary>
        /// Environment containing runtime values. Indeces correspond to symbols
        /// in CompilationEnvironemnt.
        /// </summary>
        public FSharpRef<FSharpList<FSharpRef<FSharpRef<Value>[]>>> RuntimeEnvironment
        {
            get { return rEnv; }
        }

        //Sets the environment contained by this EnvironmentWrapper
        //to the one provided by FScheme by default.
        public EnvironmentWrapper()
        {
            this.cEnv = FScheme.compileEnvironment;
            this.rEnv = FScheme.environment;
        }

        //Indexor providing a quick way to lookup symbols in the environment.
        public FSharpRef<Value> this[string symbol]
        {
            get
            {
                return this.Lookup(symbol);
            }
        }

        //Looks up the given symbol in this environment.
        //TODO: Use "Try" pattern to avoid bad lookups
        public FSharpRef<Value> Lookup(string symbol)
        {
            var ce = cEnv.Value;
            var lookup = FScheme.FindInCompilerEnv(symbol, ce);
            if (FSharpOption<Tuple<int, int>>.get_IsSome(lookup))
            {
                var indeces = lookup.Value;
                return rEnv.Value[indeces.Item1].Value[indeces.Item2];
            }
            return null;
        }

        //Adds a symbol to this environment. Simulates a "define".
        public void Add(string symbol, Value expr)
        {
            var idx = ListModule.TryFindIndex(
                (Converter<string, bool>)symbol.Equals,
                this.cEnv.Value.Head);

            if (FSharpOption<int>.get_IsSome(idx))
            {
                this.rEnv.Value.Head.Value[idx.Value].Value = expr;
            }
            else
            {
                //Update compilation environment
                this.cEnv.Value =
                    FSharpList<FSharpList<string>>.Cons(
                        ListModule.Append(this.cEnv.Value.Head, Utils.MakeFSharpList(symbol)),
                        this.cEnv.Value.Tail);

                //Update runtime environment
                var rEnvNew = this.rEnv.Value.Head.Value;
                var lastIndex = rEnvNew.Length;
                Array.Resize(ref rEnvNew, rEnvNew.Length + 1);
                this.rEnv.Value.Head.Value = rEnvNew;

                this.rEnv.Value.Head.Value[lastIndex] = new FSharpRef<Value>(expr);
            }
        }

        /// <summary>
        /// Removes an identifier from this environment.
        /// </summary>
        /// <param name="symbol">Identifier to remove.</param>
        public void Delete(string symbol)
        {
            var removedIndeces = new HashSet<int>();
            
            this.cEnv.Value = Utils.SequenceToFSharpList(
                this.cEnv.Value.Select(
                    x => Utils.SequenceToFSharpList(
                        x.Where((y, i) => 
                            {
                                var remove = !symbol.Equals(y);
                                if (remove)
                                    removedIndeces.Add(i);
                                return remove; 
                            })))
                .Where(x => x.Any()));

            this.rEnv.Value.Head.Value = this.rEnv.Value.Head.Value.Where(
                (_, i) => !removedIndeces.Contains(i)
            ).ToArray();
        }
    }

    //Class representing an FScheme Execution Environment. Used to evaluate FScheme Expressions.
    public class ExecutionEnvironment
    {
        //Environment used to store symbols.
        private EnvironmentWrapper env;

        //Default constructor, simply creates a default environment.
        public ExecutionEnvironment()
        {
            this.env = new EnvironmentWrapper();
        }

        //Binds symbols of the given string to the given body.
        public void DefineSymbol(string name, Expression body)
        {
            Evaluate(Expression.NewDefine(name, body));
        }

        //Binds symbols of the given string to the given Expression.
        //private void DefineExternal(string name, Expression func)
        //{
        //   this.env.Add(name, func);
        //}

        //Binds symbols of the given string to the given External Function.
        public void DefineExternal(string name, Converter<FSharpList<Value>, Value> func)
        {
            DefineExternal(
                name,
                Utils.ConvertToFSchemeFunc(func));
        }

        //Binds symbols of the given string to the given External Function.
        public void DefineExternal(string name, FSharpFunc<FSharpList<Value>, Value> func)
        {
            this.env.Add(name, Value.NewFunction(func));
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
            this.env.Delete(p);
        }

        /// <summary>
        /// Looks up the value associated with the given symbol in this environment.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public FSharpRef<Value> LookupSymbol(string p)
        {
            return this.env.Lookup(p);
        }
    }
}

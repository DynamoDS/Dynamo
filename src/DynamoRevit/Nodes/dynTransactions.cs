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
using Dynamo;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;
using Autodesk.Revit.DB;
using System.Diagnostics;
using Dynamo.Controls;
using System.Windows.Threading;

namespace Dynamo.Nodes
{
    [NodeName("Transaction")]
    [NodeCategory(BuiltinNodeCategories.CORE_TIME)]
    [NodeDescription("Executes Expression inside of a Revit API transaction")]
    public class dynTransaction: dynNodeWithOneOutput
    {
        public dynTransaction()
        {
            InPortData.Add(new PortData("expr", "Expression to run in a transaction.", typeof(object)));
            OutPortData.Add(new PortData("result", "Result of the expression.", typeof(Value.List)));

            RegisterAllPorts();
        }

        void setDirty(bool val)
        {
            if (ReportingEnabled)
            {
                DisableReporting();
                this.RequiresRecalc = val;
                EnableReporting();
            }
            else
                this.RequiresRecalc = val;
        }

        protected override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("Input must be connected.");
                throw new Exception("Transaction Node requires all inputs to be connected.");
            }
            return base.Build(preBuilt, outPort);
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return new TransactionProcedureNode(this, InPortData.Select(x => x.NickName));
        }

        private class TransactionProcedureNode : InputNode
        {
            private dynTransaction node;
            
            public TransactionProcedureNode(dynTransaction node, IEnumerable<string> inputNames)
                : base(inputNames)
            {
                this.node = node;
            }

            protected override Expression compileBody(
                Dictionary<INode, string> symbols,
                Dictionary<INode, List<INode>> letEntries,
                HashSet<string> initializedIds)
            {
                var arg =  arguments["expr0"].compile(symbols, letEntries, initializedIds);
                
                //idle :: (() -> A) -> A
                //Evaluates the given function in the Revit Idle thread.
                var idle = Expression.NewFunction_E(
                    FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                        args =>
                        {
                            var f = (args[0] as Value.Function).Item;

                            if (dynSettings.Controller.DynamoViewModel.RunInDebug)
                                return f.Invoke(FSharpList<Value>.Empty);
                            else
                            {
                                return IdlePromise<Value>.ExecuteOnIdle(
                                    () => f.Invoke(FSharpList<Value>.Empty));
                            }
                        }));

                //startTransaction :: () -> ()
                //Starts a Dynamo Transaction.
                var startTransaction = Expression.NewFunction_E(
                    FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                        _ =>
                        {
                            if (node.Controller.RunCancelled)
                                throw new CancelEvaluationException(false);

                            if (!dynSettings.Controller.DynamoViewModel.RunInDebug)
                            {
                                dynRevitSettings.Controller.InIdleThread = true;
                                dynRevitSettings.Controller.InitTransaction();
                            }

                            return Value.NewDummy("started transaction");
                        }));

                //endTransaction :: () -> ()
                //Ends a Dynamo Transaction.
                var endTransaction = Expression.NewFunction_E(
                    FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                        _ =>
                        {
                            if (!dynRevitSettings.Controller.DynamoViewModel.RunInDebug)
                            {
                                dynRevitSettings.Controller.EndTransaction();
                                dynRevitSettings.Controller.InIdleThread = false;

                                dynSettings.Controller.DynamoViewModel.OnRequestLayoutUpdate(this, EventArgs.Empty);
                                
                                node.ValidateConnections();
                            }
                            else
                                node.setDirty(false);

                            return Value.NewDummy("ended transaction");
                        }));

                /*  (define (idleArg)
                 *    (startTransaction)
                 *    (let ((a <arg>))
                 *      (endTransaction)
                 *      a))
                 */              
                var idleArg = Expression.NewFun(
                    FSharpList<FScheme.Parameter>.Empty,
                    Expression.NewBegin(
                        Utils.SequenceToFSharpList<Expression>(new List<Expression>() {
                            Expression.NewList_E(
                                Utils.SequenceToFSharpList<Expression>(
                                    new List<Expression>() { startTransaction })),
                            Expression.NewLet(
                                Utils.SequenceToFSharpList<string>(
                                    new List<string>() { "__result" }),
                                Utils.SequenceToFSharpList<Expression>(
                                    new List<Expression>() { arg }),
                                Expression.NewBegin(
                                    Utils.SequenceToFSharpList<Expression>(
                                        new List<Expression>() {
                                            Expression.NewList_E(
                                                Utils.SequenceToFSharpList<Expression>(
                                                    new List<Expression>() { endTransaction })),
                                            Expression.NewId("__result") 
                                        }))) 
                        })));

                // (idle idleArg)
                return Expression.NewList_E(
                    Utils.SequenceToFSharpList<Expression>(new List<Expression>() {
                        idle,
                        idleArg 
                    }));
            }
        }
    }
}

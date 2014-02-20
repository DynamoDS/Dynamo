using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.FSchemeInterop.Node;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using RevitServices.Threading;
using Expression = Dynamo.FScheme.Expression;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    [NodeName("Transaction")]
    [NodeCategory(BuiltinNodeCategories.REVIT_API)]
    [NodeDescription("Executes Expression inside of a Revit API transaction")]
    public class Transaction : NodeModel
    {
        public Transaction()
        {
            InPortData.Add(
                new PortData("expr", "Expression to run in a transaction.", typeof(object)));
            OutPortData.Add(new PortData("result", "Result of the expression.", typeof(Value.List)));

            RegisterAllPorts();
        }

        public override bool RequiresRecalc
        {
            get { return Inputs[0].Item2.RequiresRecalc; }
            set { }
        }

        protected override INode Build(
            Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            if (!Enumerable.Range(0, InPortData.Count).All(HasInput))
            {
                Error("Input must be connected.");
                throw new Exception("Transaction Node requires all inputs to be connected.");
            }
            return new PreviewUpdate(this) { WrappedNode = base.Build(preBuilt, outPort) };
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return new TransactionProcedureNode(this, InPortData.Select(x => x.NickName));
        }

        private class TransactionProcedureNode : InputNode
        {
            private readonly Transaction _node;

            public TransactionProcedureNode(Transaction node, IEnumerable<string> inputNames)
                : base(inputNames)
            {
                _node = node;
            }

            protected override Expression compileBody(
                Dictionary<INode, string> symbols, Dictionary<INode, List<INode>> letEntries,
                HashSet<string> initializedIds, HashSet<string> conditionalIds)
            {
                var arg = arguments.First()
                                   .Value.compile(
                                       symbols,
                                       letEntries,
                                       initializedIds,
                                       conditionalIds);

                //idle :: (() -> A) -> A
                //Evaluates the given function in the Revit Idle thread.
                var idle =
                    Expression.NewFunction_E(
                        FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                            args =>
                            {
                                var f = (args[0] as Value.Function).Item;

                                if (dynSettings.Controller.DynamoViewModel.RunInDebug)
                                {
                                    _node.OldValue = f.Invoke(FSharpList<Value>.Empty);
                                    return _node.OldValue;
                                }

                                return RevitServices.Threading.IdlePromise<Value>.ExecuteOnIdle(
                                () =>
                                    {
                                        _node.OldValue = f.Invoke(FSharpList<Value>.Empty);
                                        return _node.OldValue;
                                    });
                            }));

                //startTransaction :: () -> ()
                //Starts a Dynamo Transaction.
                var startTransaction =
                    Expression.NewFunction_E(
                        FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                            _ =>
                            {
                                if (_node.Controller.RunCancelled)
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
                var endTransaction =
                    Expression.NewFunction_E(
                        FSharpFunc<FSharpList<Value>, Value>.FromConverter(
                            _ =>
                            {
                                if (!dynRevitSettings.Controller.DynamoViewModel.RunInDebug)
                                {
                                    dynRevitSettings.Controller.EndTransaction();
                                    dynRevitSettings.Controller.InIdleThread = false;

                                    dynSettings.Controller.DynamoModel.OnRequestLayoutUpdate(
                                        this,
                                        EventArgs.Empty);

                                    _node.ValidateConnections();
                                }

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
                        new[]
                        {
                            Expression.NewList_E(
                                new[] { startTransaction }.SequenceToFSharpList()),
                            Expression.NewLet(
                                new[] { "__result" }.SequenceToFSharpList(),
                                new[] { arg }.SequenceToFSharpList(),
                                Expression.NewBegin(
                                    new[]
                                    {
                                        Expression.NewList_E(
                                            new[] { endTransaction }.SequenceToFSharpList()),
                                        Expression.NewId("__result")
                                    }.SequenceToFSharpList()))
                        }.SequenceToFSharpList()));

                // (idle idleArg)
                return
                    Expression.NewList_E(
                        new[] { idle, idleArg }.SequenceToFSharpList());
            }
        }
    }
}

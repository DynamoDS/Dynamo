using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;

using NCalc;

namespace Dynamo.Nodes
{
    [NodeName("Formula")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Design and compute mathematical expressions.")]
    [NodeSearchTags("Equation", "Arithmetic")]
    [IsInteractive(true)]
    public partial class dynFormula : dynMathBase
    {
        private string _formula = "";
        public string Formula
        {
            get
            {
                return _formula;
            }

            set
            {
                if (_formula == null || !_formula.Equals(value))
                {
                    _formula = value;
                    if (value != null)
                    {
                        DisableReporting();
                        processFormula();
                        RaisePropertyChanged("Formula");
                        RequiresRecalc = true;
                        EnableReporting();
                        if (WorkSpace != null)
                            WorkSpace.Modified();
                    }
                }
            }
        }

        public dynFormula()
        {
            OutPortData.Add(new PortData("", "Result of math computation", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("formula", Formula);
        }

        public override void LoadNode(XmlNode elNode)
        {
            Formula = elNode.Attributes["formula"].Value ?? "";
        }

        private static HashSet<string> RESERVED_NAMES = new HashSet<string>() { 
            "abs", "acos", "asin", "atan", "ceiling", "cos",
            "exp", "floor", "ieeeremainder", "log", "log10",
            "max", "min", "pow", "round", "sign", "sin", "sqrt",
            "tan", "truncate", "in", "if", "pi", "π"
        };

        private void processFormula()
        {
            Expression e;
            try
            {
                e = new Expression(Formula, EvaluateOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return;
            }

            if (e.HasErrors())
            {
                Error(e.Error);
                return;
            }

            var parameters = new List<Tuple<string, Type>>();
            var paramSet = new HashSet<string>();

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                if (!paramSet.Contains(name) && !RESERVED_NAMES.Contains(name))
                {
                    paramSet.Add(name);
                    parameters.Add(Tuple.Create(name, typeof(Value.Function)));
                }

                foreach (var p in args.Parameters)
                {
                    p.Evaluate();
                }

                args.Result = 0;
            };

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                if (!paramSet.Contains(name))
                {
                    paramSet.Add(name);
                    parameters.Add(Tuple.Create(name, typeof(Value.Number)));
                }

                args.Result = 0;
            };

            try
            {
                e.Evaluate();
            }
            catch { }

            InPortData.Clear();

            foreach (var p in parameters)
            {
                InPortData.Add(new PortData(p.Item1, "variable", p.Item2));
            }

            RegisterInputs();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var e = new Expression(Formula, EvaluateOptions.IgnoreCase);

            e.Parameters["pi"] = 3.14159265358979;

            var functionLookup = new Dictionary<string, Value>();

            foreach (var arg in args.Select((arg, i) => new { Value = arg, Index = i }))
            {
                var parameter = InPortData[arg.Index].NickName;
                if (arg.Value.IsFunction)
                    functionLookup[parameter] = arg.Value;
                else
                    e.Parameters[parameter] = ((Value.Number)arg.Value).Item;
            }

            e.EvaluateFunction += delegate(string name, FunctionArgs fArgs)
            {
                if (functionLookup.ContainsKey(name))
                {
                    var func = ((Value.Function)functionLookup[name]).Item;
                    fArgs.Result = ((Value.Number)func.Invoke(
                        Utils.SequenceToFSharpList(
                            fArgs.Parameters.Select(
                                p => Value.NewNumber(Convert.ToDouble(p.Evaluate())))))).Item;
                }
                else
                {
                    fArgs.HasResult = false;
                }
            };

            return Value.NewNumber(Convert.ToDouble(e.Evaluate()));
        }
    }
}

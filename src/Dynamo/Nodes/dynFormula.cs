using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

using Microsoft.FSharp.Collections;

using Dynamo.Controls;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;

using NCalc;

namespace Dynamo.Nodes
{
    [NodeName("Formula")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH)]
    [NodeDescription("Design and compute mathematical expressions. Uses NCalc Syntax: http://ncalc.codeplex.com.")]
    [NodeSearchTags("Equation", "Arithmetic")]
    [IsInteractive(true)]
    public class dynFormula : dynMathBase
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

        public override void SetupCustomUIElements(dynNodeView nodeUI)
        {
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            nodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = false;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = this;
            var bindingVal = new Binding("Formula")
            {
                Mode = BindingMode.TwoWay,
                NotifyOnValidationError = false,
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
            tb.SetBinding(TextBox.TextProperty, bindingVal);
        }

        public override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            dynEl.SetAttribute("formula", Formula);
        }

        public override void LoadNode(XmlNode elNode)
        {
            Formula = elNode.Attributes["formula"].Value ?? "";
        }

        private static HashSet<string> RESERVED_FUNC_NAMES = new HashSet<string> { 
            "abs", "acos", "asin", "atan", "ceiling", "cos",
            "exp", "floor", "ieeeremainder", "log", "log10",
            "max", "min", "pow", "round", "sign", "sin", "sqrt",
            "tan", "truncate", "in", "if"
        };

        private static HashSet<string> RESERVED_PARAM_NAMES = new HashSet<string> {
            "pi", "π"
        };

        private void processFormula()
        {
            Expression e;
            try
            {
                e = new Expression(Formula.ToLower(), EvaluateOptions.IgnoreCase);
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
                if (!paramSet.Contains(name) && !RESERVED_FUNC_NAMES.Contains(name))
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
                if (!paramSet.Contains(name) && !RESERVED_PARAM_NAMES.Contains(name))
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
            var e = new Expression(Formula.ToLower(), EvaluateOptions.IgnoreCase);

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

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
    [NodeDescription("Design and compute mathematical expressions.")]
    [NodeSearchTags("Equation", "Arithmetic")]
    [IsInteractive(true)]
    public class dynFormula : dynNodeWithOneOutput
    {
        private string _formula;
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
                    RequiresRecalc = value != null;
                    RaisePropertyChanged("Formula");
                }
            }
        }

        public dynFormula()
        {
            OutPortData.Add(new PortData("", "Result of math computation", typeof(Value.Number)));
            RegisterAllPorts();
        }

        public override void SetupCustomUIElements(dynNodeView NodeUI)
        {
            var tb = new dynTextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            NodeUI.inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.IsNumeric = false;
            tb.Background = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            tb.OnChangeCommitted += delegate
            {
                processFormula();
                dynSettings.ReturnFocusToSearch();
            };

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

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            dynEl.SetAttribute("formula", Formula);
        }

        public override void LoadElement(XmlNode elNode)
        {
            Formula = elNode.Attributes["formula"].Value;
            processFormula();
        }

        private void processFormula()
        {
            Expression e;
            try
            {
                e = new Expression(Formula);
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

            var parameters = new SortedList<int, Tuple<string, Type>>();
            var paramSet = new HashSet<string>();

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                if (!paramSet.Contains(name))
                {
                    paramSet.Add(name);
                    parameters.Add(Formula.IndexOf(name), Tuple.Create(name, typeof(Value.Function)));
                    foreach (var p in args.Parameters)
                    {
                        p.Evaluate();
                    }
                    args.Result = 0;
                }
            };

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                if (!paramSet.Contains(name))
                {
                    paramSet.Add(name);
                    parameters.Add(Formula.IndexOf(name), Tuple.Create(name, typeof(Value.Number)));
                    args.Result = 0;
                }
            };

            e.Evaluate();

            InPortData.Clear();

            foreach (var p in parameters.Values)
            {
                InPortData.Add(new PortData(p.Item1, "variable", p.Item2));
            }

            RegisterInputs();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var e = new Expression(Formula);

            var parameterLookup = new Dictionary<string, Value>();

            foreach (var arg in args.Select((arg, i) => new { Value = arg, Index = i }))
            {
                var parameter = InPortData[arg.Index].NickName;
                parameterLookup[parameter] = arg.Value;
            }

            e.EvaluateParameter += delegate(string name, ParameterArgs pArgs)
            {
                pArgs.Result = ((Value.Number)parameterLookup[name]).Item;
            };

            e.EvaluateFunction += delegate(string name, FunctionArgs fArgs)
            {
                var func = ((Value.Function)parameterLookup[name]).Item;
                fArgs.Result = ((Value.Number)func.Invoke(
                    Utils.SequenceToFSharpList(
                        fArgs.Parameters.Select(
                            p => Value.NewNumber((double)p.Evaluate()))))).Item;
            };

            dynamic result = e.Evaluate();

            return Value.NewNumber(result);
        }
    }
}

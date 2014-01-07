using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml;
using Dynamo.Models;
using Microsoft.FSharp.Collections;
using Dynamo.FSchemeInterop;
using Value = Dynamo.FScheme.Value;
using Dynamo.Utilities;

using NCalc;

namespace Dynamo.Nodes
{
    [NodeName("Formula")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_ARITHMETIC)]
    [NodeDescription("Design and compute mathematical expressions. Uses NCalc Syntax: http://ncalc.codeplex.com.")]
    [NodeSearchTags("Equation", "Arithmetic")]
    [IsInteractive(true)]
    public partial class Formula : MathBase
    {
        private string _formulaString = "";
        public string FormulaString
        {
            get
            {
                return _formulaString;
            }

            set
            {
                if (_formulaString == null || !_formulaString.Equals(value))
                {
                    _formulaString = value;
                    if (value != null)
                    {
                        ElementState oldState = this.State;
                        {
                            DisableReporting();
                            ProcessFormula();
                            RaisePropertyChanged("FormulaString");
                            RequiresRecalc = true;
                            EnableReporting();
                            if (WorkSpace != null)
                                WorkSpace.Modified();
                        }

                        if (oldState != this.State)
                            RaisePropertyChanged("State");
                    }
                }
            }
        }

        public Formula()
        {
            OutPortData.Add(new PortData("", "Result of math computation", typeof(Value.Number)));
            RegisterAllPorts();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            var formStringNode = xmlDoc.CreateElement("FormulaText");
            formStringNode.InnerText = FormulaString;
            nodeElement.AppendChild(formStringNode);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            if (nodeElement.Attributes != null)
            {
                var formulaAttr = nodeElement.Attributes["formula"];
                if (formulaAttr != null)
                {
                    FormulaString = formulaAttr.Value;
                    return;
                }
            }

            var formStringNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(childNode => childNode.Name == "FormulaText");
            FormulaString = formStringNode != null 
                ? formStringNode.InnerText 
                : nodeElement.InnerText;
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("formulaString", FormulaString);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                FormulaString = helper.ReadString("formulaString");
            }
        }

        #endregion

        private static readonly HashSet<string> ReservedFuncNames = new HashSet<string> { 
            "abs", "acos", "asin", "atan", "ceiling", "cos",
            "exp", "floor", "ieeeremainder", "log", "log10",
            "max", "min", "pow", "round", "sign", "sin", "sqrt",
            "tan", "truncate", "in", "if"
        };

        private static readonly HashSet<string> ReservedParamNames = new HashSet<string> {
            "pi", "π"
        };

        private void ProcessFormula()
        {
            Expression e;
            try
            {
                e = new Expression(
                    FormulaString.ToLower()
                        .Replace(" and ", "+").Replace("&&", "+")
                        .Replace(" or ", "+").Replace("||", "+"), 
                    EvaluateOptions.IgnoreCase);
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
                if (!paramSet.Contains(name) && !ReservedFuncNames.Contains(name))
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
                if (!paramSet.Contains(name) && !ReservedParamNames.Contains(name))
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
            
            RegisterInputPorts();
            ClearError();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var e = new Expression(FormulaString.ToLower(), EvaluateOptions.IgnoreCase);

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

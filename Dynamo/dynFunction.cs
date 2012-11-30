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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;
using Dynamo.FSchemeInterop;
using System.Windows.Media.Effects;

namespace Dynamo.Elements
{
    [RequiresTransaction(false)]
    [IsInteractive(false)]
    public class dynFunction : dynBuiltinMacro
    {
        public dynFunction(IEnumerable<string> inputs, string output, string symbol)
            : base(symbol)
        {
            //Set inputs and output
            this.SetInputs(inputs);
            OutPortData = new PortData(output, "function output", typeof(object));

            //Set the nickname
            this.NickName = symbol;

            //Add a drop-shadow.
            ((DropShadowEffect)this.elementRectangle.Effect).Opacity = 1;

            //Setup double-click behavior
            this.MouseDoubleClick += delegate
            {
                dynElementSettings.SharedInstance.Bench.DisplayFunction(symbol);
            };

            base.RegisterInputsAndOutputs();
        }

        public dynFunction()
            : base(null)
        {
            //Setup double-click behavior
            this.MouseDoubleClick += delegate
            {
                dynElementSettings.SharedInstance.Bench.DisplayFunction(this.Symbol);
            };

            //Add a drop-shadow
            ((DropShadowEffect)this.elementRectangle.Effect).Opacity = 1;
        }

        protected internal override bool RequiresManualTransaction()
        {
            //Check if we already know we require a Manual Transaction
            bool baseManual = base.RequiresManualTransaction();
            if (baseManual)
                return true;

            //Initialize our recursive function detection construct
            bool start = _startTag;
            _startTag = true;

            //If we've already been here, then we know we're safe already, no need to check internals.
            if (_taggedSymbols.Contains(this.Symbol))
                return false;
            //Remember we've been here.
            _taggedSymbols.Add(this.Symbol);

            if (!this.Bench.dynFunctionDict.ContainsKey(this.Symbol))
            {
                this.Bench.Log("WARNING -- No implementation found for node: " + this.Symbol);
                this.Error("Could not find .dyf definition file for this node.");

                if (!start)
                {
                    _startTag = false;
                    _taggedSymbols.Clear();
                }

                return false;
            }

            //Grab the workspace inside this function, and check if any of it's internals require a manual transaction.
            var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            bool manualInternals = ws.GetTopMostElements().Any(x => x.RequiresManualTransaction());

            //If we started the traversal here, then end the recursive function detection.
            if (!start)
            {
                _startTag = false;
                _taggedSymbols.Clear();
            }

            //Fin
            return manualInternals;
        }

        protected internal override bool RequiresTransaction()
        {
            //Check if we already know we require a Transaction
            bool baseManual = base.RequiresTransaction();
            if (baseManual)
                return true;

            //Initialize our recursive function detection construct
            bool start = _startTag;
            _startTag = true;

            //If we've already been here, then we know we're safe already, no need to check internals.
            if (_taggedSymbols.Contains(this.Symbol))
                return false;
            //Remember we've been here.
            _taggedSymbols.Add(this.Symbol);

            if (!this.Bench.dynFunctionDict.ContainsKey(this.Symbol))
            {
                this.Bench.Log("WARNING -- No implementation found for node: " + this.Symbol);
                this.Error("Could not find .dyf definition file for this node.");

                if (!start)
                {
                    _startTag = false;
                    _taggedSymbols.Clear();
                }

                return false;
            }

            //Grab the workspace inside this function, and check if any of it's internals require a transaction.
            var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            bool manualInternals = ws.GetTopMostElements().Any(x => x.RequiresTransaction());

            //If we started the traversal here, then end the recursive function detection.
            if (!start)
            {
                _startTag = false;
                _taggedSymbols.Clear();
            }

            //Fin
            return manualInternals;
        }

        public override bool IsDirty
        {
            get
            {
                //Do we already know we're dirty?
                bool baseDirty = base.IsDirty;
                if (baseDirty)
                    return true;

                //Initialize recursive function detection construct.
                bool start = _startTag;
                _startTag = true;

                //If we've already been here, then we're not dirty.
                if (_taggedSymbols.Contains(this.Symbol))
                    return false;
                //Remember we've been here.
                _taggedSymbols.Add(this.Symbol);

                if (!this.Bench.dynFunctionDict.ContainsKey(this.Symbol))
                {
                    this.Bench.Log("WARNING -- No implementation found for node: " + this.Symbol);
                    this.Error("Could not find .dyf definition file for this node.");

                    if (!start)
                    {
                        _startTag = false;
                        _taggedSymbols.Clear();
                    }

                    return false;
                }

                //TODO: bugged? 
                //Solution: pass func workspace to dynFunction, hook the Modified event, set IsDirty to true when modified.
                var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
                bool dirtyInternals = ws.GetTopMostElements().Any(e => e.IsDirty);

                //If we started the traversal here, clean up.
                if (!start)
                {
                    _startTag = false;
                    _taggedSymbols.Clear();
                }

                return dirtyInternals;
            }
            set
            {
                //Set the base value.
                base.IsDirty = value;
                //If we're clean, then notify all internals.
                if (!value)
                {
                    //Recursion detection start.
                    bool start = _startTag;
                    _startTag = true;

                    //If we've been here, then we're done.
                    if (_taggedSymbols.Contains(this.Symbol))
                        return;
                    //Remember
                    _taggedSymbols.Add(this.Symbol);

                    if (!this.Bench.dynFunctionDict.ContainsKey(this.Symbol))
                    {
                        this.Bench.Log("WARNING -- No implementation found for node: " + this.Symbol);
                        this.Error("Could not find .dyf definition file for this node.");

                        if (!start)
                        {
                            _startTag = false;
                            _taggedSymbols.Clear();
                        }

                        return;
                    }

                    //Notifiy all internals that we're clean.
                    var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
                    foreach (var e in ws.Elements)
                        e.IsDirty = false;

                    //If we started traversal here, cleanup.
                    if (!start)
                    {
                        _startTag = false;
                        _taggedSymbols.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the inputs of this function.
        /// </summary>
        /// <param name="inputs"></param>
        public void SetInputs(IEnumerable<string> inputs)
        {
            int i = 0;
            foreach (string input in inputs)
            {
                PortData data = new PortData(input, "Input #" + (i + 1), typeof(object));

                if (this.InPortData.Count > i)
                {
                    InPortData[i] = data;
                }
                else
                {
                    InPortData.Add(data);
                }

                i++;
            }

            if (i < InPortData.Count)
            {
                InPortData.RemoveRange(i, InPortData.Count - i);
            }
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", this.Symbol);
            dynEl.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Output");
            outEl.SetAttribute("value", OutPortData.NickName);
            dynEl.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (var input in InPortData.Select(x => x.NickName))
            {
                var inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name.Equals("Symbol"))
                {
                    this.Symbol = subNode.Attributes[0].Value;
                }
                else if (subNode.Name.Equals("Output"))
                {
                    var data = new PortData(subNode.Attributes[0].Value, "function output", typeof(object));

                    OutPortData = data;
                }
                else if (subNode.Name.Equals("Inputs"))
                {
                    int i = 0;
                    foreach (XmlNode inputNode in subNode.ChildNodes)
                    {
                        var data = new PortData(inputNode.Attributes[0].Value, "Input #" + (i + 1), typeof(object));

                        if (InPortData.Count > i)
                        {
                            InPortData[i] = data;
                        }
                        else
                        {
                            InPortData.Add(data);
                        }

                        i++;
                    }
                }
            }

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            var procedure = this.Bench.Environment.LookupSymbol(this.Symbol);
            if (procedure.IsFunction)
            {
                return (procedure as Expression.Function).Item
                   .Invoke(ExecutionEnvironment.IDENT)
                   .Invoke(
                      Utils.convertSequence(
                         args.Select(
                            input => this.macroEnvironment.Evaluate(input)
                         )
                      )
                   );
            }
            else
                return base.Evaluate(args);
        }

        //protected internal override INode Build()
        //{
        //   if (this.SaveResult && !this.IsDirty)
        //      return new ExpressionNode(this.oldValue);
        //   else
        //      return base.Build();
        //}

        //protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        //{
        //   return new FunctionNode(this.Symbol, portNames);
        //}

        public override void Destroy()
        {
            bool start = _startTag;
            _startTag = true;

            if (_taggedSymbols.Contains(this.Symbol))
                return;
            _taggedSymbols.Add(this.Symbol);

            if (!this.Bench.dynFunctionDict.ContainsKey(this.Symbol))
            {
                this.Bench.Log("WARNING -- No implementation found for node: " + this.Symbol);
                this.Error("Could not find .dyf definition file for this node.");

                if (!start)
                {
                    _startTag = false;
                    _taggedSymbols.Clear();
                }

                return;
            }

            var ws = this.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            foreach (var el in ws.Elements)
                el.Destroy();

            if (!start)
            {
                _startTag = false;
                _taggedSymbols.Clear();
            }

            //var ws = dynElementSettings.SharedInstance.Bench.dynFunctionDict[this.Symbol]; //TODO: Refactor
            //foreach (var el in ws.Elements)
            //{
            //   if (!(el is dynFunction) || !((dynFunction)el).Symbol.Equals(this.Symbol))
            //      el.Destroy();
            //}
        }
    }

    [ElementName("Variable")]
    [ElementCategory(BuiltinElementCategories.PRIMITIVES)]
    [ElementDescription("A function variable")]
    [RequiresTransaction(false)]
    [IsInteractive(false)]
    public class dynSymbol : dynNode
    {
        TextBox tb;

        public dynSymbol()
        {
            OutPortData = new PortData("", "Symbol", typeof(object));

            //add a text box to the input grid of the control
            tb = new TextBox();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            inputGrid.Children.Add(tb);
            System.Windows.Controls.Grid.SetColumn(tb, 0);
            System.Windows.Controls.Grid.SetRow(tb, 0);
            tb.Text = "";
            //tb.KeyDown += new System.Windows.Input.KeyEventHandler(tb_KeyDown);
            //tb.LostFocus += new System.Windows.RoutedEventHandler(tb_LostFocus);

            //turn off the border
            SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            tb.Background = backgroundBrush;
            tb.BorderThickness = new Thickness(0);

            base.RegisterInputsAndOutputs();
        }

        public override bool IsDirty
        {
            get
            {
                return false;
            }
            set { }
        }

        public string Symbol
        {
            get { return this.tb.Text; }
            set { this.tb.Text = value; }
        }

        protected internal override INode Build()
        {
            return new SymbolNode(
               (string)this.Dispatcher.Invoke(new Func<string>(
                  () => this.Symbol
               ))
            );
        }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", this.Symbol);
            dynEl.AppendChild(outEl);
        }

        public override void LoadElement(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    this.Symbol = subNode.Attributes[0].Value;
                }
            }
        }
    }

    #region Disabled Anonymous Function Node
    //[RequiresTransaction(false)]
    //[IsInteractive(false)]
    //public class dynAnonFunction : dynElement
    //{
    //   private INode entryPoint;

    //   public dynAnonFunction(IEnumerable<string> inputs, string output, INode entryPoint)
    //   {
    //      int i = 1;
    //      foreach (string input in inputs)
    //      {
    //         InPortData.Add(new PortData(null, input, "Input #" + i++, typeof(object)));
    //      }

    //      OutPortData = new PortData(null, output, "function output", typeof(object));

    //      this.entryPoint = entryPoint;

    //      base.RegisterInputsAndOutputs();
    //   }

    //   protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
    //   {
    //      return new AnonymousFunctionNode(portNames, this.entryPoint);
    //   }
    //}
    #endregion
}

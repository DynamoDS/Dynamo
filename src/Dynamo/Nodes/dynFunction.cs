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
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Utilities;
using System.Windows.Media.Effects;
using Dynamo.Nodes;

namespace Dynamo
{
    namespace Nodes
    {
        [IsInteractive(false)]
        public class dynFunction : dynBuiltinFunction
        {
            public dynFunction(IEnumerable<string> inputs, IEnumerable<string> outputs, FunctionDefinition def)
                : base(def.FunctionId.ToString())
            {
                _def = def;

                //Set inputs and output
                SetInputs(inputs);
                foreach (var output in outputs)
                    OutPortData.Add(new PortData(output, "function output", typeof (object)));

                RegisterAllPorts();

                ArgumentLacing = LacingStrategy.Disabled;
            }

            public dynFunction()
                : base(null)
            {

            }

            public override void SetupCustomUIElements(dynNodeView ui)
            {
                ((DropShadowEffect) ui.elementRectangle.Effect).Opacity = 1;
                ((DropShadowEffect) ui.elementRectangle.Effect).Color = Colors.WhiteSmoke;
                ((DropShadowEffect) ui.elementRectangle.Effect).BlurRadius = 20;
                ((DropShadowEffect) ui.elementRectangle.Effect).ShadowDepth = 0;

                ui.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(ui_MouseDoubleClick);

            }

            void ui_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                Controller.DynamoViewModel.GoToWorkspaceCommand.Execute(_def.FunctionId);
                e.Handled = true;
            }

            FunctionDefinition _def;
            public FunctionDefinition Definition
            {
                get { return _def; }
                internal set
                {
                    _def = value;
                    if (value != null)
                        Symbol = value.FunctionId.ToString();
                }
            }

            public override bool RequiresRecalc
            {
                get
                {
                    //Do we already know we're dirty?
                    bool baseDirty = base.RequiresRecalc;
                    if (baseDirty)
                        return true;

                    return Definition.RequiresRecalc 
                        || Definition.Dependencies.Any(x => x.RequiresRecalc);
                }
                set
                {
                    //Set the base value.
                    base.RequiresRecalc = value;
                    //If we're clean, then notify all internals.
                    if (!value)
                    {
                        if (dynSettings.Controller.Running)
                            dynSettings.FunctionWasEvaluated.Add(Definition);
                        else
                        {
                            //Recursion detection start.
                            Definition.RequiresRecalc = false;

                            foreach (var dep in Definition.Dependencies)
                                dep.RequiresRecalc = false;
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
                    if (InPortData.Count > i)
                    {
                        InPortData[i].NickName = input;
                    }
                    else
                    {
                        InPortData.Add(new PortData(input, "Input #" + (i + 1), typeof(object)));
                    }

                    i++;
                }

                if (i < InPortData.Count)
                {
                    for (var k = i; k < InPortData.Count; k++)
                        InPorts[k].KillAllConnectors();

                    //MVVM: confirm that extension methods on observable collection do what we expect
                    InPortData.RemoveRange(i, InPortData.Count - i);
                }
            }

            public void SetOutputs(IEnumerable<string> outputs)
            {
                int i = 0;
                foreach (string output in outputs)
                {
                    if (OutPortData.Count > i)
                    {
                        OutPortData[i].NickName = output;
                    }
                    else
                    {
                        OutPortData.Add(new PortData(output, "Output #" + (i + 1), typeof(object)));
                    }

                    i++;
                }

                if (i < OutPortData.Count)
                {
                    for (var k = i; k < OutPortData.Count; k++)
                        OutPorts[k].KillAllConnectors();

                    OutPortData.RemoveRange(i, OutPortData.Count - i);
                }
            }

            public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
            {
                //Debug.WriteLine(pd.Object.GetType().ToString());
                XmlElement outEl = xmlDoc.CreateElement("ID");
                
                outEl.SetAttribute("value", Symbol);
                dynEl.AppendChild(outEl);

                outEl = xmlDoc.CreateElement("Name");
                outEl.SetAttribute("value", NickName);
                dynEl.AppendChild(outEl);

                outEl = xmlDoc.CreateElement("Inputs");
                foreach (var input in InPortData.Select(x => x.NickName))
                {
                    var inputEl = xmlDoc.CreateElement("Input");
                    inputEl.SetAttribute("value", input);
                    outEl.AppendChild(inputEl);
                }
                dynEl.AppendChild(outEl);

                outEl = xmlDoc.CreateElement("Outputs");
                foreach (var output in OutPortData.Select(x => x.NickName))
                {
                    var outputEl = xmlDoc.CreateElement("Output");
                    outputEl.SetAttribute("value", output);
                    outEl.AppendChild(outputEl);
                }
                dynEl.AppendChild(outEl);
            }

            public override void LoadElement(XmlNode elNode)
            {
                foreach (XmlNode subNode in elNode.ChildNodes)
                {
                    if (subNode.Name.Equals("ID"))
                    {
                        Symbol = subNode.Attributes[0].Value;
                        //Definition = dynSettings.FunctionDict.Values.FirstOrDefault(
                        //    x => x.Workspace.Name == subNode.Attributes[0].Value);
                    }
                    else if (subNode.Name.Equals("Name"))
                    {
                        NickName = subNode.Attributes[0].Value;
                    }
                    else if (subNode.Name.Equals("Outputs"))
                    {
                        int i = 0;
                        foreach (XmlNode outputNode in subNode.ChildNodes)
                        {
                            var data = new PortData(outputNode.Attributes[0].Value, "Output #" + (i + 1), typeof(object));

                            if (OutPortData.Count > i)
                            {
                                OutPortData[i] = data;
                            }
                            else
                            {
                                OutPortData.Add(data);
                            }

                            i++;
                        }
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
                    #region Legacy output support
                    else if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(subNode.Attributes[0].Value, "function output", typeof(object));

                        if (OutPortData.Any())
                            OutPortData[0] = data;
                        else
                            OutPortData.Add(data);
                    }
                    #endregion
                }

                RegisterAllPorts();
            }
        }

        [NodeName("Output")]
        [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
        [NodeDescription("A function output")]
        [IsInteractive(false)]
        public class dynOutput : dynNodeModel
        {
            TextBox tb;
            private string symbol = "";

            public dynOutput()
            {
                InPortData.Add(new PortData("", "", typeof(object)));

                RegisterAllPorts();
            }

            public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
            {
                //add a text box to the input grid of the control
                tb = new TextBox();
                tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                NodeUI.inputGrid.Children.Add(tb);
                System.Windows.Controls.Grid.SetColumn(tb, 0);
                System.Windows.Controls.Grid.SetRow(tb, 0);

                //turn off the border
                SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                tb.Background = backgroundBrush;
                tb.BorderThickness = new Thickness(0);

                tb.DataContext = this;
                var bindingSymbol = new System.Windows.Data.Binding("Symbol")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay()
                };
                tb.SetBinding(TextBox.TextProperty, bindingSymbol);

                tb.TextChanged += tb_TextChanged;
            }

            void tb_TextChanged(object sender, TextChangedEventArgs e)
            {
                Symbol = tb.Text;
            }

            public override bool RequiresRecalc
            {
                get
                {
                    return false;
                }
                set { }
            }

            public string Symbol
            {
                get
                {
                    return symbol;
                }
                set
                {
                    symbol = value;
                    RaisePropertyChanged("Symbol");
                }
            }

            public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
            {
                //Debug.WriteLine(pd.Object.GetType().ToString());
                XmlElement outEl = xmlDoc.CreateElement("Symbol");
                outEl.SetAttribute("value", Symbol);
                dynEl.AppendChild(outEl);
            }

            public override void LoadElement(XmlNode elNode)
            {
                foreach (XmlNode subNode in elNode.ChildNodes)
                {
                    if (subNode.Name == "Symbol")
                    {
                        Symbol = subNode.Attributes[0].Value;
                    }
                }
            }
        }

        [NodeName("Input")]
        [NodeCategory(BuiltinNodeCategories.CORE_PRIMITIVES)]
        [NodeDescription("A function parameter")]
        [NodeSearchTags("variable", "argument", "parameter")]
        [IsInteractive(false)]
        public class dynSymbol : dynNodeModel
        {
            TextBox tb;
            private string symbol = "";

            public dynSymbol()
            {
                OutPortData.Add(new PortData("", "Symbol", typeof(object)));

                RegisterAllPorts();
            }

            public override void SetupCustomUIElements(Controls.dynNodeView NodeUI)
            {
                //add a text box to the input grid of the control
                tb = new TextBox();
                tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                NodeUI.inputGrid.Children.Add(tb);
                System.Windows.Controls.Grid.SetColumn(tb, 0);
                System.Windows.Controls.Grid.SetRow(tb, 0);

                //turn off the border
                SolidColorBrush backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                tb.Background = backgroundBrush;
                tb.BorderThickness = new Thickness(0);

                tb.DataContext = this;
                var bindingSymbol = new System.Windows.Data.Binding("Symbol")
                {
                    Mode = BindingMode.TwoWay
                };
                tb.SetBinding(TextBox.TextProperty, bindingSymbol);

                tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);

            }

            void tb_TextChanged(object sender, TextChangedEventArgs e)
            {
                Symbol = tb.Text;
            }

            public override bool RequiresRecalc
            {
                get
                {
                    return false;
                }
                set { }
            }

            //MVVM: removed direct set of tb.text
            public string Symbol
            {
                get
                {
                    //return tb.Text;
                    return symbol;
                }
                set
                {
                    //tb.Text = value;
                    symbol = value;
                    RaisePropertyChanged("Symbol");
                }
            }

            protected internal override INode Build(Dictionary<dynNodeModel, Dictionary<int, INode>> preBuilt, int outPort)
            {
                Dictionary<int, INode> result;
                if (!preBuilt.TryGetValue(this, out result))
                {
                    result = new Dictionary<int, INode>();
                    result[outPort] = new SymbolNode(GUID.ToString());
                    preBuilt[this] = result;
                }
                return result[outPort];
            }

            public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
            {
                //Debug.WriteLine(pd.Object.GetType().ToString());
                XmlElement outEl = xmlDoc.CreateElement("Symbol");
                outEl.SetAttribute("value", Symbol);
                dynEl.AppendChild(outEl);
            }

            public override void LoadElement(XmlNode elNode)
            {
                foreach (XmlNode subNode in elNode.ChildNodes)
                {
                    if (subNode.Name == "Symbol")
                    {
                        Symbol = subNode.Attributes[0].Value;
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

        //      entryPoint = entryPoint;

        //      NodeUI.RegisterInputsAndOutput();
        //   }

        //   protected internal override ProcedureCallNode Compile(IEnumerable<string> portNames)
        //   {
        //      return new AnonymousFunctionNode(portNames, entryPoint);
        //   }
        //}
        #endregion
    }

    public class FunctionDefinition
    {
        internal FunctionDefinition() : this(Guid.NewGuid()) { }

        internal FunctionDefinition(Guid id)
        {
            FunctionId = id;
            RequiresRecalc = true;
        }

        public Guid FunctionId { get; private set; }
        public dynWorkspaceModel Workspace { get; internal set; }
        public List<Tuple<int, dynNodeModel>> OutPortMappings { get; internal set; }
        public List<Tuple<int, dynNodeModel>> InPortMappings { get; internal set; }
        public bool RequiresRecalc { get; internal set; }

        public IEnumerable<FunctionDefinition> Dependencies
        {
            get
            {
                return findAllDependencies(new HashSet<FunctionDefinition>());
            }
        }

        private IEnumerable<FunctionDefinition> findAllDependencies(HashSet<FunctionDefinition> dependencySet)
        {
            var query = Workspace.Nodes
                .Where(node => node is dynFunction)
                .Select(node => (node as dynFunction).Definition)
                .Where(def => !dependencySet.Contains(def));

            foreach (var definition in query)
            {
                yield return definition;
                dependencySet.Add(definition);
                foreach (var def in definition.findAllDependencies(dependencySet))
                    yield return def;
            }
        }
    }
}

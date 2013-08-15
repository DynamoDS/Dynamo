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
using System.Windows.Input;
using System.Xml;

using Dynamo.Connectors;
using DynamoPython;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using Microsoft.FSharp.Collections;

using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
    
    [NodeName("Python Script")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_PYTHON)]
    [NodeDescription("Runs an embedded IronPython script")]
    public class dynPython : dynNodeWithOneOutput, IDrawable
    {
        private bool dirty = true;
        private Value lastEvalValue;

        /// <summary>
        /// Allows a scripter to have a persistent reference to previous runs.
        /// </summary>
        private Dictionary<string, dynamic> stateDict = new Dictionary<string, dynamic>();

        private string script = "#The input to this node will be stored in the IN variable.\ndataEnteringNode = IN\n\n#Assign your output to the OUT variable\nOUT = 0";

        public RenderDescription RenderDescription{get;set;}

        public dynPython()
        {
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override void SetupCustomUIElements(Controls.dynNodeView nodeUI)
        {
            //topControl.Height = 200;
            //topControl.Width = 300;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem();
            editWindowItem.Header = "Edit...";
            editWindowItem.IsCheckable = false;
            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += new RoutedEventHandler(editWindowItem_Click);
            nodeUI.UpdateLayout();

            nodeUI.MouseDown += new MouseButtonEventHandler(nodeUI_MouseDown);
        }

        void nodeUI_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                editWindowItem_Click(this, null);
                e.Handled = true;
            }
        }

        //TODO: Make this smarter
        public override bool RequiresRecalc
        {
            get
            {
                return true;
            }
            set { }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            XmlElement script = xmlDoc.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = this.script;
            dynEl.AppendChild(script);
        }

        protected override void LoadNode(XmlNode elNode)
        {
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Script")
                    //this.tb.Text = subNode.InnerText;
                    script = subNode.InnerText;
            }
        }

        private List<Binding> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = this.InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new Binding(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new Binding("__persistent__", this.stateDict));

            return bindings;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Value result = PythonEngine.Evaluator(dirty, script, makeBindings(args));
            lastEvalValue = result;
            return result;
        }

        private dynScriptEditWindow editWindow;
        private bool initWindow = false;

        void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            if (!initWindow)
            {
                editWindow = new dynScriptEditWindow();
                // callbacks for autocompletion
                editWindow.editText.TextArea.TextEntering += textEditor_TextArea_TextEntering;
                editWindow.editText.TextArea.TextEntered += textEditor_TextArea_TextEntered;

                const string pythonHighlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
                var elem = GetType().Assembly.GetManifestResourceStream("DynamoPython.Resources." + pythonHighlighting);

                editWindow.editText.SyntaxHighlighting =
                HighlightingLoader.Load(new XmlTextReader(elem ),
                HighlightingManager.Instance);
            }

            //set the text of the edit window to begin
            editWindow.editText.Text = script;

            if (editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            script = editWindow.editText.Text;

            this.dirty = true;
        }


        #region Autocomplete

        CompletionWindow completionWindow;
        private IronPythonCompletionProvider completionProvider = new IronPythonCompletionProvider();

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == ".")
                {
                    completionWindow = new CompletionWindow(editWindow.editText.TextArea);
                    var data = completionWindow.CompletionList.CompletionData;

                    var completions =
                        completionProvider.GetCompletionData(editWindow.editText.Text.Substring(0,
                                                                                                editWindow.editText
                                                                                                          .CaretOffset));

                    if (completions.Length == 0)
                        return;

                    foreach (var ele in completions)
                    {
                        data.Add(ele);
                    }

                    completionWindow.Show();

                    completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                }
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("Failed to perform python autocomplete with exception:");
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            try {
                if (e.Text.Length > 0 && completionWindow != null)
                {
                    if (!char.IsLetterOrDigit(e.Text[0]))
                    {
                        completionWindow.CompletionList.RequestInsertion(e);
                    }
                }
            }
            catch (Exception ex)
            {
                DynamoLogger.Instance.Log("Failed to perform python autocomplete with exception:");
                DynamoLogger.Instance.Log(ex.Message);
                DynamoLogger.Instance.Log(ex.StackTrace);
            }
        }

        #endregion

        public void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new RenderDescription();
            else
                this.RenderDescription.ClearAll();

            if(lastEvalValue != null)
                PythonEngine.Drawing(lastEvalValue, this.RenderDescription);
        }
    }

    [NodeName("Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_PYTHON)]
    [NodeDescription("Runs a IronPython script from a string")]
    public class dynPythonString : dynNodeWithOneOutput
    {

        /// <summary>
        /// Allows a scripter to have a persistent reference to previous runs.
        /// </summary>
        private Dictionary<string, dynamic> stateDict = new Dictionary<string, dynamic>();

        public dynPythonString()
        {
            InPortData.Add(new PortData("script", "Script to run", typeof(Value.String)));
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private List<Binding> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = 
               this.InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new Binding(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new Binding("__persistent__", this.stateDict));

            return bindings;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var script = ((Value.String) args[0]).Item;
            var bindings = makeBindings(args);
            var value = PythonEngine.Evaluator( RequiresRecalc, script, bindings);
            return value;
        }
    }
}

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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Dynamo.Controls;
using Dynamo.Models;
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
    public class Python : NodeWithOneOutput
    {
        private bool _dirty = true;
        private Value _lastEvalValue;

        /// <summary>
        /// Allows a scripter to have a persistent reference to previous runs.
        /// </summary>
        private readonly Dictionary<string, dynamic> _stateDict = new Dictionary<string, dynamic>();

        private string _script;

        public Python()
        {
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();
            InitializeDefaultScript();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private void InitializeDefaultScript()
        {
            _script = "# Default imports\n";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies.Any(x => x.FullName.Contains("RevitAPI")) && assemblies.Any(x => x.FullName.Contains("RevitAPIUI")))
            {
                _script = _script
                    + "import clr\n"
                    + "clr.AddReference('RevitAPI')\n"
                    + "clr.AddReference('RevitAPIUI')\n"
                    + "from Autodesk.Revit.DB import *\n"
                    + "import Autodesk\n";
            }

            string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\dll";

            if (!assemblies.Any(x => x.FullName.Contains("LibGNet")))
            {
                //LibG could not be found, possibly because we haven't used a node
                //that requires it yet. Let's load it...
                string libGPath = Path.Combine(dllDir, "LibGNet.dll");
                Assembly.LoadFrom(libGPath);

                //refresh the collection of loaded assemblies
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }

            if (assemblies.Any(x => x.FullName.Contains("LibGNet")))
            {
                _script = _script + "import sys\n"
                    + "import clr\n"
                    + "path = r'C:\\Autodesk\\Dynamo\\Core'\n"
                    + "exec_path = r'" + dllDir + "'\n"
                    + "sys.path.append(path)\n"
                    + "sys.path.append(exec_path)\n"
                    + "clr.AddReference('LibGNet')\n"
                    + "from Autodesk.LibG import *\n";
            }

            _script = _script + "\n"
                + "#The input to this node will be stored in the IN variable.\n"
                + "dataEnteringNode = IN\n\n"
                + "#Assign your output to the OUT variable\n"
                + "OUT = 0";
        }

        public override void SetupCustomUIElements(object ui)
        {
            var nodeUI = ui as dynNodeView;

            //topControl.Height = 200;
            //topControl.Width = 300;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new System.Windows.Controls.MenuItem
            {
                Header = "Edit...",
                IsCheckable = false
            };
            nodeUI.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += editWindowItem_Click;
            nodeUI.UpdateLayout();

            nodeUI.MouseDown += nodeUI_MouseDown;
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

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement script = xmlDoc.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = _script;
            nodeElement.AppendChild(script);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Script")
                    //this.tb.Text = subNode.InnerText;
                    _script = subNode.InnerText;
            }
        }

        private IEnumerable<KeyValuePair<string, dynamic>> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new KeyValuePair<string, dynamic>(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new KeyValuePair<string, dynamic>("__persistent__", _stateDict));

            return bindings;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Value result = PythonEngine.Evaluator(_dirty, _script, makeBindings(args));
            _lastEvalValue = result;

            Draw();

            return result;
        }

        private dynScriptEditWindow _editWindow;

        void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            _editWindow = new dynScriptEditWindow();
            // callbacks for autocompletion
            _editWindow.editText.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _editWindow.editText.TextArea.TextEntered += textEditor_TextArea_TextEntered;

            const string pythonHighlighting = "ICSharpCode.PythonBinding.Resources.Python.xshd";
            var elem =
                GetType()
                    .Assembly.GetManifestResourceStream(
                        "DynamoPython.Resources." + pythonHighlighting);

            _editWindow.editText.SyntaxHighlighting =
                HighlightingLoader.Load(
                    new XmlTextReader(elem),
                    HighlightingManager.Instance);

            //set the text of the edit window to begin
            _editWindow.editText.Text = _script;

            if (_editWindow.ShowDialog() != true)
            {
                return;
            }

            //set the value from the text in the box
            _script = _editWindow.editText.Text;

            _dirty = true;
        }

        #region Autocomplete

        CompletionWindow _completionWindow;
        private readonly IronPythonCompletionProvider _completionProvider = new IronPythonCompletionProvider();

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                if (e.Text == ".")
                {
                    _completionWindow = new CompletionWindow(_editWindow.editText.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;

                    var completions =
                        _completionProvider.GetCompletionData(_editWindow.editText.Text.Substring(0,
                                                                                                _editWindow.editText
                                                                                                          .CaretOffset));

                    if (completions.Length == 0)
                        return;

                    foreach (var ele in completions)
                    {
                        data.Add(ele);
                    }

                    _completionWindow.Show();

                    _completionWindow.Closed += delegate
                        {
                            _completionWindow = null;
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
                if (e.Text.Length > 0 && _completionWindow != null)
                {
                    if (!char.IsLetterOrDigit(e.Text[0]))
                    {
                        _completionWindow.CompletionList.RequestInsertion(e);
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
            if(_lastEvalValue != null)
                PythonEngine.Drawing(_lastEvalValue, GUID.ToString());
        }
    }

    [NodeName("Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.SCRIPTING_PYTHON)]
    [NodeDescription("Runs a IronPython script from a string")]
    public class PythonString : NodeWithOneOutput
    {

        /// <summary>
        /// Allows a scripter to have a persistent reference to previous runs.
        /// </summary>
        private readonly Dictionary<string, dynamic> _stateDict = new Dictionary<string, dynamic>();

        public PythonString()
        {
            InPortData.Add(new PortData("script", "Script to run", typeof(Value.String)));
            InPortData.Add(new PortData("IN", "Input", typeof(object)));
            OutPortData.Add(new PortData("OUT", "Result of the python script", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private IEnumerable<KeyValuePair<string, dynamic>> makeBindings(IEnumerable<Value> args)
        {
            //Zip up our inputs
            var bindings = 
               InPortData
               .Select(x => x.NickName)
               .Zip(args, (s, v) => new KeyValuePair<string, dynamic>(s, Converters.convertFromValue(v)))
               .Concat(PythonBindings.Bindings)
               .ToList();

            bindings.Add(new KeyValuePair<string, dynamic>("__persistent__", _stateDict));

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

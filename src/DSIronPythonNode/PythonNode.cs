using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.Utilities;
using IronPython.Hosting;
using ProtoCore.AST.AssociativeAST;

namespace DSIronPythonNode
{
    public class IronPythonEvaluator
    {
        public static object EvaluateIronPythonScript(string code, IList names, IList values)
        {
            var amt = Math.Min(names.Count, values.Count);

            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            for (int i = 0; i < amt; i++)
            {
                scope.SetVariable((string)names[i], values[i]);
            }

            engine.CreateScriptSourceFromString(code).Execute(scope);

            return scope.ContainsVariable("OUT") ? scope.GetVariable("OUT") : null;
        }
    }

    [Browsable(false)]
    public class PythonNode : VariableInputNode
    {
        public PythonNode()
        {
            RegisterAllPorts();
            _script = InitializeDefaultScript();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        private string _script;
        public string Script
        {
            get
            {
                return _script;
            }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    RaisePropertyChanged("Script");
                }
            }
        }

        private string InitializeDefaultScript()
        {
            return "# Default imports\n\n"
                + "#The input to this node will be stored in the IN0...INX variable(s).\n"
                + "dataEnteringNode = IN0\n\n"
                + "#Assign your output to the OUT variable\n"
                + "OUT = 0";
        }

        public override void SetupCustomUIElements(dynNodeView view)
        {
            var editWindowItem = new MenuItem
            {
                Header = "Edit...",
                IsCheckable = false
            };
            view.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            view.UpdateLayout();

            view.MouseDown += view_MouseDown;
        }

        void view_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent();
                e.Handled = true;
            }
        }

        private void EditScriptContent()
        {
            var editWindow = new ScriptEditWindow();
            editWindow.Initialize(GUID, "ScriptContent", Script);
            editWindow.ShowDialog();
        }

        protected override string InputRootName
        {
            get { return "IN"; }
        }

        protected override string TooltipRootName
        {
            get { return "Input #"; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(
                    GetAstIdentifierForOutputIndex(0),
                    AstFactory.BuildFunctionCall(
                        "DSIronPythonNode.IronPythonEvaluator.EvaluateIronPythonScript",
                        new List<AssociativeNode>
                        {
                            AstFactory.BuildStringNode(_script),
                            AstFactory.BuildExprList(
                                new List<AssociativeNode> { AstFactory.BuildStringNode("IN") }),
                            AstFactory.BuildExprList(
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildExprList(inputAstNodes)
                                })
                        }))
            };
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "ScriptContent")
            {
                _script = value;
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        #region Save/Load

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement script = xmlDoc.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = _script;
            nodeElement.AppendChild(script);

            // save the number of inputs
            nodeElement.SetAttribute("inputs", (InPortData.Count).ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            var inputAttr = nodeElement.Attributes["inputs"];
            int inputs = inputAttr == null ? 1 : Convert.ToInt32(inputAttr.Value);
            this.SetNumInputs(inputs);

            var scriptNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "Script");
            if (scriptNode != null)
            {
                _script = scriptNode.InnerText;
            }
        }

        #endregion

        #region SerializeCore/DeserializeCore

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            var helper = new XmlElementHelper(element);
            helper.SetAttribute("Script", this.Script);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            var helper = new XmlElementHelper(element);
            var script = helper.ReadString("Script", string.Empty);
            this._script = script;
        }

        #endregion
    }
}

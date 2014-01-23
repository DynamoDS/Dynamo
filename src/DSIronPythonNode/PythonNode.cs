using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Utilities;
using IronPython.Hosting;
using ProtoCore.AST.AssociativeAST;

namespace DSIronPythonNode
{
    public class IronPythonEvaluator
    {
        public static object EvaluateIronPythonScript(string code, IList names, IList values)
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            var amt = Math.Min(names.Count, values.Count);

            for (int i = 0; i < amt; i++)
            {
                scope.SetVariable((string)names[i], values[i]);
            }

            engine.CreateScriptSourceFromString(code).Execute(scope);

            return scope.ContainsVariable("OUT") ? scope.GetVariable("OUT") : null;
        }
    }

    public abstract class PythonNodeBase : VariableInputNode
    {
        protected PythonNodeBase()
        {
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string InputRootName
        {
            get { return "IN"; }
        }

        protected override string TooltipRootName
        {
            get { return "Input #"; }
        }

        protected AssociativeNode CreateOutputAST(
            AssociativeNode codeInputNode, List<AssociativeNode> inputAstNodes,
            List<Tuple<string, AssociativeNode>> additionalBindings)
        {
            var names = additionalBindings.Select(x => x.Item1).ToList();
            names.Add("IN");

            var vals = additionalBindings.Select(x => x.Item2).ToList();
            vals.Add(AstFactory.BuildExprList(inputAstNodes));

            return AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildFunctionCall(
                    "DSIronPythonNode.IronPythonEvaluator.EvaluateIronPythonScript",
                    new List<AssociativeNode>
                    {
                        codeInputNode,
                        AstFactory.BuildExprList(
                            names.Select(x => AstFactory.BuildStringNode(x) as AssociativeNode)
                                 .ToList()),
                        AstFactory.BuildExprList(vals)
                    }));
        }
    }

    [Browsable(false)]
    public class PythonNode : PythonNodeBase
    {
        public PythonNode()
        {
            _script = "# Default imports\n\n"
                + "#The input to this node will be stored in the IN0...INX variable(s).\n"
                + "dataEnteringNode = IN0\n\n" + "#Assign your output to the OUT variable\n"
                + "OUT = 0";
            OutPortData.Add(new PortData("OUT", "Result of the python script"));
            RegisterAllPorts();
        }

        private string _script;

        public string Script
        {
            get { return _script; }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    RaisePropertyChanged("Script");
                }
            }
        }

        public override void SetupCustomUIElements(dynNodeView view)
        {
            var editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            view.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            view.UpdateLayout();

            view.MouseDown += view_MouseDown;
        }

        private void view_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                EditScriptContent();
                e.Handled = true;
            }
        }

        private void EditScriptContent()
        {
            var editWindow = new ScriptEditorWindow();
            editWindow.Initialize(GUID, "ScriptContent", Script);
            editWindow.ShowDialog();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                CreateOutputAST(
                    AstFactory.BuildStringNode(_script),
                    inputAstNodes,
                    new List<Tuple<string, AssociativeNode>>())
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

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            base.SaveNode(xmlDoc, nodeElement, context);

            XmlElement script = xmlDoc.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = _script;
            nodeElement.AppendChild(script);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            var scriptNode =
                nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "Script");
            
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
            helper.SetAttribute("Script", Script);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context);
            var helper = new XmlElementHelper(element);
            var script = helper.ReadString("Script", string.Empty);
            _script = script;
        }

        #endregion
    }

    [Browsable(false)]
    public class PythonStringNode : PythonNodeBase
    {
        public PythonStringNode()
        {
            InPortData.Add(new PortData("script", "Python script to run."));
            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override int GetInputIndex()
        {
            return base.GetInputIndex() - 1;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                CreateOutputAST(
                    inputAstNodes[0],
                    inputAstNodes.Skip(1).ToList(),
                    new List<Tuple<string, AssociativeNode>>())
            };
        }
    }

}

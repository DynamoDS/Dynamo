using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using DSIronPython;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using ProtoCore.AST.AssociativeAST;
using Autodesk.DesignScript.Runtime;

namespace DSIronPythonNode
{
    public class PythonNodeViewCustomization : VariableInputNodeViewCustomization, INodeViewCustomization<PythonNode>
    {
        private DynamoViewModel dynamoViewModel;
        private PythonNode model;

        public void CustomizeView(PythonNode nodeModel, NodeView nodeView)
        {
            base.CustomizeView(nodeModel, nodeView);

            this.model = nodeModel;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            var editWindowItem = new MenuItem { Header = "Edit...", IsCheckable = false };
            nodeView.MainContextMenu.Items.Add(editWindowItem);
            editWindowItem.Click += delegate { EditScriptContent(); };
            nodeView.UpdateLayout();

            nodeView.MouseDown += view_MouseDown;
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
            var editWindow = new ScriptEditorWindow(dynamoViewModel);
            editWindow.Initialize(model.GUID, "ScriptContent", model.Script);
            bool? acceptChanged = editWindow.ShowDialog();
            if (acceptChanged.HasValue && acceptChanged.Value)
            {
                model.RequiresRecalc = true;
            }
        }
    }

    public abstract class PythonNodeBase : VariableInputNode
    {
        protected PythonNodeBase(WorkspaceModel workspace) : base(workspace)
        {
            OutPortData.Add(new PortData("OUT", "Result of the python script"));
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override string GetInputName(int index)
        {
            return string.Format("IN[{0}]", index);
        }

        protected override string GetInputTooltip(int index)
        {
            return "Input #" + index;
        }

        protected AssociativeNode CreateOutputAST(
            AssociativeNode codeInputNode, List<AssociativeNode> inputAstNodes,
            List<Tuple<string, AssociativeNode>> additionalBindings)
        {
            var names =
                additionalBindings.Select(
                    x => AstFactory.BuildStringNode(x.Item1) as AssociativeNode).ToList();
            names.Add(AstFactory.BuildStringNode("IN"));

            var vals = additionalBindings.Select(x => x.Item2).ToList();
            vals.Add(AstFactory.BuildExprList(inputAstNodes));

            Func<string, IList, IList, object> backendMethod =
                IronPythonEvaluator.EvaluateIronPythonScript;

            return AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildFunctionCall(
                    backendMethod,
                    new List<AssociativeNode>
                    {
                        codeInputNode,
                        AstFactory.BuildExprList(names),
                        AstFactory.BuildExprList(vals)
                    }));
        }
    }

    [NodeName("Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("Runs an embedded IronPython script.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonNode : PythonNodeBase
    {
        public PythonNode(WorkspaceModel workspace) : base(workspace)
        {
            script = "import clr\nclr.AddReference('ProtoGeometry')\n"
                + "from Autodesk.DesignScript.Geometry import *\n"
                + "#The inputs to this node will be stored as a list in the IN variable.\n"
                + "dataEnteringNode = IN\n\n"
                + "#Assign your output to the OUT variable\n"
                + "OUT = 0";

            AddInput();

            RegisterAllPorts();
        }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                if (script != value)
                {
                    script = value;
                    RaisePropertyChanged("Script");
                }
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                CreateOutputAST(
                    AstFactory.BuildStringNode(script),
                    inputAstNodes,
                    new List<Tuple<string, AssociativeNode>>())
            };
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "ScriptContent")
            {
                script = value;
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
            script.InnerText = this.script;
            nodeElement.AppendChild(script);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);

            var scriptNode =
                nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "Script");
            
            if (scriptNode != null)
            {
                script = scriptNode.InnerText;
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
            this.script = script;
        }

        #endregion
    }

    [NodeName("Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("Runs a IronPython script from a string.")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonStringNode : PythonNodeBase
    {
        public PythonStringNode(WorkspaceModel workspace) : base(workspace)
        {
            InPortData.Add(new PortData("script", "Python script to run."));
            AddInput();
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

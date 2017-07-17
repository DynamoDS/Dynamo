using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Autodesk.DesignScript.Runtime;

using DSIronPython;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;
using System.IO;

namespace PythonNodeModels
{
    public abstract class PythonNodeBase : VariableInputNode
    {
        protected PythonNodeBase()
        {
            OutPortData.Add(new PortData("OUT", Properties.Resources.PythonNodePortDataOutputToolTip));
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
    [NodeDescription("PythonScriptDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonNode : PythonNodeBase
    {
        public PythonNode()
        {
            var pythonTemplatePath = Dynamo.Models.DynamoModel.PublicPreferenceSettings.PythonTemplateFilePath;
            if (!String.IsNullOrEmpty(pythonTemplatePath) && File.Exists(pythonTemplatePath))
                script = File.ReadAllText(pythonTemplatePath);
            else
                script = "#" + Properties.Resources.PythonScriptEditorImports + Environment.NewLine + 
                    "import clr" + Environment.NewLine + 
                    "clr.AddReference('ProtoGeometry')" + Environment.NewLine +
                    "from Autodesk.DesignScript.Geometry import *" + Environment.NewLine +
                    "#" + Properties.Resources.PythonScriptEditorInputComment + Environment.NewLine +
                    "dataEnteringNode = IN" + Environment.NewLine + Environment.NewLine + 
                    "#" + Properties.Resources.PythonScriptEditorCodeComment + Environment.NewLine + Environment.NewLine +
                    "#" + Properties.Resources.PythonScriptEditorOutputComment + Environment.NewLine +
                    "OUT = yourOutputVariableHere";

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

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "ScriptContent")
            {
                script = value;
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
        }

        #region SerializeCore/DeserializeCore

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement script = element.OwnerDocument.CreateElement("Script");
            //script.InnerText = this.tb.Text;
            script.InnerText = this.script;
            element.AppendChild(script);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            var scriptNode =
                nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "Script");

            if (scriptNode != null)
            {
                script = scriptNode.InnerText;
            }
        }

        #endregion
    }

    [NodeName("Python Script From String")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("PythonScriptFromStringDescription", typeof(Properties.Resources))]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonStringNode : PythonNodeBase
    {
        public PythonStringNode()
        {
            InPortData.Add(new PortData("script", Properties.Resources.PythonStringPortDataScriptToolTip));
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
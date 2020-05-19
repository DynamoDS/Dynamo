using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using DSCPython;
using DSIronPython;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;

namespace PythonNodeModels
{
    /// <summary>
    /// Enum of possible values of python engine versions
    /// </summary>
    public enum PythonEngineVersion
    {
        IronPython2,
        CPython3
    }

    public abstract class PythonNodeBase : VariableInputNode
    {
        private PythonEngineVersion engine = PythonEngineVersion.IronPython2;

        [JsonConverter(typeof(StringEnumConverter))]
        /// <summary>
        /// Return the user selected python engine enum.
        /// </summary>
        public PythonEngineVersion Engine
        {
            get { return engine; }
            set
            {
                if (engine != value)
                {
                    engine = value;
                    RaisePropertyChanged(nameof(Engine));
                    OnNodeModified();
                }
            }
        }

        protected PythonNodeBase()
        {
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("OUT", Properties.Resources.PythonNodePortDataOutputToolTip)));
            ArgumentLacing = LacingStrategy.Disabled;
        }

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        protected PythonNodeBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
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

            Func<string, IList, IList, object> pythonEvaluatorMethod;

            if (Engine == PythonEngineVersion.IronPython2)
            {
                pythonEvaluatorMethod = IronPythonEvaluator.EvaluateIronPythonScript;
            }
            else if (Engine == PythonEngineVersion.CPython3)
            {
                pythonEvaluatorMethod = CPythonEvaluator.EvaluatePythonScript;
            }
            else
            {
                throw new InvalidOperationException("Unknown Python engine " + Engine);
            }

            return AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildFunctionCall(
                    pythonEvaluatorMethod,
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
    [OutPortTypes("var[]..[]")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonNode : PythonNodeBase
    {
        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "PythonScriptNode";
            }
        }

        /// <summary>
        /// The default Python code template. Code comments are saved in *.resx for localisation.
        /// </summary>
        private string defaultPythonTemplateCode
        {
            get
            {
                return "# " + Properties.Resources.PythonScriptEditorImports + Environment.NewLine +
                        "import sys" + Environment.NewLine +
                        "import clr" + Environment.NewLine +
                        "clr.AddReference('ProtoGeometry')" + Environment.NewLine +
                        "from Autodesk.DesignScript.Geometry import *" + Environment.NewLine + Environment.NewLine +
                        "# " + Properties.Resources.PythonScriptEditorInputComment + Environment.NewLine +
                        "dataEnteringNode = IN" + Environment.NewLine + Environment.NewLine +
                        "# " + Properties.Resources.PythonScriptEditorCodeComment + Environment.NewLine + Environment.NewLine +
                        "# " + Properties.Resources.PythonScriptEditorOutputComment + Environment.NewLine +
                        "OUT = 0";
            }
        }

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        private PythonNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public PythonNode()
        {
            var pythonTemplatePath = PreferenceSettings.GetPythonTemplateFilePath();
            if (!String.IsNullOrEmpty(pythonTemplatePath) && File.Exists(pythonTemplatePath))
                script = File.ReadAllText(pythonTemplatePath);
            else
                script = defaultPythonTemplateCode;

            AddInput();
        }

        private string script;

        [JsonProperty("Code")]
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

        [Obsolete]
        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);

            XmlElement script = element.OwnerDocument.CreateElement("Script");
            script.InnerText = this.script;
            element.AppendChild(script);
        }

        [Obsolete]
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
    [OutPortTypes("var[]..[]")]
    [SupressImportIntoVM]
    [IsDesignScriptCompatible]
    public sealed class PythonStringNode : PythonNodeBase
    {
        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        private PythonStringNode(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public PythonStringNode()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("script", Properties.Resources.PythonStringPortDataScriptToolTip)));
            AddInput();
            RegisterAllPorts();
        }

        protected override void RemoveInput()
        {
            if (InPorts.Count > 1)
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
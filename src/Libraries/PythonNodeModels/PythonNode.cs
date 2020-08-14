﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoCore.AST.AssociativeAST;

namespace PythonNodeModels
{
    /// <summary>
    /// Event arguments used to send the original and migrated code to the ScriptEditor
    /// </summary>
    internal class PythonCodeMigrationEventArgs : EventArgs
    {
        public string OldCode { get; private set; }
        public string NewCode { get; private set; }
        public PythonCodeMigrationEventArgs(string oldCode, string newCode)
        {
            OldCode = oldCode;
            NewCode = newCode;
        }
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

            // Here we switched to use the AstFactory.BuildFunctionCall version that accept
            // class name and function name. They will be set by PythonEngineSelector by the engine value. 
            PythonEngineSelector.Instance.GetEvaluatorInfo(Engine, out string evaluatorClass, out string evaluationMethod);

            return AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildFunctionCall(
                    evaluatorClass,
                    evaluationMethod,
                    new List<AssociativeNode>
                    {
                        codeInputNode,
                        AstFactory.BuildExprList(names),
                        AstFactory.BuildExprList(vals)
                    }));
        }

        internal event EventHandler MigrationAssistantRequested;
        internal void RequestCodeMigration(EventArgs e)
        {
            MigrationAssistantRequested?.Invoke(this, e);
        }

    }

    [NodeName("Python Script")]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [NodeDescription("PythonScriptDescription", typeof(Properties.Resources))]
    [NodeSearchTags("PythonSearchTags", typeof(Properties.Resources))]
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
                    new List<Tuple<string, AssociativeNode>>()
                    {
                        Tuple.Create<string, AssociativeNode>(nameof(Name), AstFactory.BuildStringNode(Name))
                    })
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

        [Obsolete("This method is part of the temporary IronPython to CPython3 migration feature and will be removed in future versions of Dynamo.")]
        /// <summary>
        /// Updates the Script property of the node and raise the migration event notifications.
        /// NOTE: This is a temporary method used during the Python 2 to Python 3 transistion period,
        /// it will be removed when the transistion period is over.
        /// </summary>
        /// <param name="newCode">The new migrated code</param>
        internal void MigrateCode(string newCode)
        {        
            var e = new PythonCodeMigrationEventArgs(Script, newCode); 
            Script = newCode;
            OnCodeMigrated(e);
        }

        /// <summary>
        /// Fires when the Script content is migrated to Python 3.
        /// NOTE: This is a temporary event used during the Python 2 to Python 3 transistion period,
        /// it will be removed when the transistion period is over.
        /// </summary>
        internal event EventHandler<PythonCodeMigrationEventArgs> CodeMigrated;
        private void OnCodeMigrated(PythonCodeMigrationEventArgs e)
        {
            CodeMigrated?.Invoke(this, e);
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
    [NodeSearchTags("PythonSearchTags", typeof(Properties.Resources))]
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
                    new List<Tuple<string, AssociativeNode>>()
                    {
                        Tuple.Create<string, AssociativeNode>(nameof(Name), AstFactory.BuildStringNode(Name))
                    })
            };
        }
    }
}
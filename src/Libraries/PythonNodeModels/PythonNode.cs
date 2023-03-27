using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Autodesk.DesignScript.Runtime;
using Dynamo.Configuration;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.PythonServices;
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
        private string engine = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        // Set the default EngineName value to IronPython2 so that older graphs can show the migration warnings.
        [DefaultValue("IronPython2")]

        // When removing this property also replace the serialized property in EngineName
        // (i.e remove XmlIgnore and add [JsonProperty("Engine", DefaultValueHandling = DefaultValueHandling.Populate)]
        /// <summary>
        /// Return the user selected python engine enum.
        /// </summary>
        [Obsolete("This property will be deprecated in Dynamo 3.0. Please use EngineName instead")]
        public PythonEngineVersion Engine
        {
            get
            {
                if (!Enum.TryParse(engine, out PythonEngineVersion engineVersion) ||
                    engineVersion == PythonEngineVersion.Unspecified)
                {
                    //if this is a valid dynamically loaded engine, return unknown, and serialize the name.
                    if (PythonEngineManager.Instance.AvailableEngines.Any(x=>x.Name == engine))
                    {
                        return PythonEngineVersion.Unknown;
                    }
                    // This is a first-time case for newly created nodes only
                    SetEngineByDefault();
                }
                return engineVersion;
            }
            set
            {
                engine = value.ToString();
                RaisePropertyChanged(nameof(EngineName));
            }
        }

        [XmlIgnore]
        // Set the default EngineName value to IronPython2 so that older graphs can show the migration warnings.
        [DefaultValue("IronPython2")]
        /// <summary>
        /// Return the user selected python engine enum.
        /// </summary>
        public string EngineName
        {
            get
            {
                // This is a first-time case for newly created nodes only
                if (string.IsNullOrEmpty(engine))
                {
                    SetEngineByDefault();
                }
                return engine;
            }
            set
            {
                if (engine != value)
                {
                    engine = value;
                    RaisePropertyChanged(nameof(EngineName));
                }
            }
        }

        /// <summary>
        /// Available Python engines.
        /// </summary>
        [Obsolete(@"This method will be removed in future versions of Dynamo.
        Please use PythonEngineManager.Instance.AvailableEngines instead")]
        public static ObservableCollection<PythonEngineVersion> AvailableEngines
        {
            get
            {
                return new ObservableCollection<PythonEngineVersion>(PythonEngineManager.Instance.AvailableEngines.
                    Select(x => Enum.TryParse(x.Name, out PythonEngineVersion version) ? version : PythonEngineVersion.Unspecified));
            }
        }

        /// <summary>
        /// Set the engine to be used by default for this node, based on user and system settings.
        /// </summary>
        private void SetEngineByDefault()
        {
            var version = PreferenceSettings.GetDefaultPythonEngine();
            var systemDefault = DynamoModel.DefaultPythonEngine;
            if (!string.IsNullOrEmpty(version))
            {
                engine = version;
            }
            else if (!string.IsNullOrEmpty(systemDefault))
            {
                engine = systemDefault;
            }
            else
            {
                // Use CPython as default
                engine = PythonEngineManager.CPython3EngineName;
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

            return AstFactory.BuildAssignment(
                GetAstIdentifierForOutputIndex(0),
                AstFactory.BuildFunctionCall(
                    "PythonEvaluator",
                    "Evaluate",
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(EngineName),
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

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == nameof(EngineName))
            {
                EngineName = value;
                return true;

            }
            return base.UpdateValueCore(updateValueParams);
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
            XmlElement engine = element.OwnerDocument.CreateElement(nameof(EngineName));
            engine.InnerText = EngineName;
            element.AppendChild(engine);

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
            var engineNode =
              nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == nameof(EngineName));

            if (engineNode != null)
            {
                string oldEngine = EngineName;
                EngineName = engineNode.InnerText;
                if (context == SaveContext.Undo && oldEngine != this.EngineName)
                {
                    // For Python nodes, changing the Engine property does not set the node Modified flag.
                    // This is done externally in all event handlers, so for Undo we do it here.
                    OnNodeModified();
                }
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

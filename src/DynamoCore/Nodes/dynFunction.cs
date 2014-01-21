using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.FSchemeInterop.Node;
using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeDescription("A node with customized internal functionality.")]
    [IsInteractive(false)]
    public partial class Function : NodeWithOneOutput
    {
        protected internal Function(
            IEnumerable<string> inputs, IEnumerable<string> outputs, FunctionDefinition def)
        {
            _def = def;

            Symbol = def.FunctionId.ToString();

            //Set inputs and output
            SetInputs(inputs);
            foreach (var output in outputs)
                OutPortData.Add(new PortData(output, "function output", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public Function() { }

        public new string Name
        {
            get { return this.Definition.WorkspaceModel.Name; }
            set
            {
                this.Definition.WorkspaceModel.Name = value;
                this.RaisePropertyChanged("Name");
            }
        }

        public override string Description
        {
            get
            {
                if (this.Definition == null)
                    return string.Empty;
                return this.Definition.WorkspaceModel.Description;
            }
            set
            {
                this.Definition.WorkspaceModel.Description = value;
                this.RaisePropertyChanged("Description");
            }
        }

        public string Symbol { get; protected internal set; }

        public new string Category
        {
            get
            {

                if (
                    dynSettings.Controller.CustomNodeManager.NodeInfos.ContainsKey(
                        this.Definition.FunctionId))
                    return
                        dynSettings.Controller.CustomNodeManager.NodeInfos[
                            this.Definition.FunctionId].Category;
                else
                {
                    return "Custom Nodes";
                }
            }
        }

        private FunctionDefinition _def;

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

                        //TODO: move this to RequiresRecalc property of FunctionDefinition?
                        foreach (var dep in Definition.Dependencies)
                            dep.RequiresRecalc = false;
                    }
                }
            }
        }

        protected override InputNode Compile(IEnumerable<string> portNames)
        {
            return SaveResult ? base.Compile(portNames) : new FunctionNode(Symbol, portNames);
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
                //for (var k = i; k < InPortData.Count; k++)
                //    InPorts[k].KillAllConnectors();

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
                //for (var k = i; k < OutPortData.Count; k++)
                //    OutPorts[k].KillAllConnectors();

                OutPortData.RemoveRange(i, OutPortData.Count - i);
            }
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("ID");

            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Name");
            outEl.SetAttribute("value", NickName);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Description");
            outEl.SetAttribute("value", Description);
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Inputs");
            foreach (var input in InPortData.Select(x => x.NickName))
            {
                var inputEl = xmlDoc.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                outEl.AppendChild(inputEl);
            }
            nodeElement.AppendChild(outEl);

            outEl = xmlDoc.CreateElement("Outputs");
            foreach (var output in OutPortData.Select(x => x.NickName))
            {
                var outputEl = xmlDoc.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outEl.AppendChild(outputEl);
            }
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Name"))
                {
                    NickName = subNode.Attributes[0].Value;
                }
            }

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("ID"))
                {
                    Symbol = subNode.Attributes[0].Value;
                    Guid funcId;
                    if (!VerifySymbol(out funcId))
                    {

                        LoadProxyCustomNode(funcId);
                        return;
                    }
                }
            }

            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals("Outputs"))
                {
                    int i = 0;
                    foreach (XmlNode outputNode in subNode.ChildNodes)
                    {
                        var data = new PortData(
                            outputNode.Attributes[0].Value, "Output #" + (i + 1), typeof(object));

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
                        var data = new PortData(
                            inputNode.Attributes[0].Value, "Input #" + (i + 1), typeof(object));

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
                    var data = new PortData(
                        subNode.Attributes[0].Value, "function output", typeof(object));

                    if (OutPortData.Any())
                        OutPortData[0] = data;
                    else
                        OutPortData.Add(data);
                }

                #endregion
            }

            RegisterAllPorts();

            //argument lacing on functions should be set to disabled
            //by default in the constructor, but for any workflow saved
            //before this was the case, we need to ensure it here.
            ArgumentLacing = LacingStrategy.Disabled;

            // we've found a custom node, we need to attempt to load its guid.  
            // if it doesn't exist (i.e. its a legacy node), we need to assign it one
            // deterministically
            Guid funId;
            try
            {
                funId = Guid.Parse(Symbol);
            }
            catch (FormatException)
            {
                funId = GuidUtility.Create(
                    GuidUtility.UrlNamespace, nodeElement.Attributes["nickname"].Value);
                Symbol = funId.ToString();
            }

            Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funId);
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("functionId", Symbol);
                helper.SetAttribute("functionName", NickName);
                helper.SetAttribute("functionDesc", Description);

                XmlDocument xmlDoc = element.OwnerDocument;
                foreach (var input in InPortData.Select(x => x.NickName))
                {
                    var inputEl = xmlDoc.CreateElement("functionInput");
                    inputEl.SetAttribute("inputValue", input);
                    element.AppendChild(inputEl);
                }

                foreach (var input in OutPortData.Select(x => x.NickName))
                {
                    var outputEl = xmlDoc.CreateElement("functionOutput");
                    outputEl.SetAttribute("outputValue", input);
                    element.AppendChild(outputEl);
                }
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                NickName = helper.ReadString("functionName");

                Symbol = helper.ReadString("functionId");
                Guid funcId;
                if (!VerifySymbol(out funcId))
                {
                    LoadProxyCustomNode(funcId);
                    return;
                }
                XmlNodeList inNodes = element.SelectNodes("functionInput");
                XmlNodeList outNodes = element.SelectNodes("functionOutput");
                int i = 0;
                foreach (XmlNode inputNode in inNodes)
                {
                    string name = inputNode.Attributes[0].Value;
                    var data = new PortData(name, "Input #" + (i + 1), typeof(object));
                    if (InPortData.Count > i)
                        InPortData[i] = data;
                    else
                        InPortData.Add(data);
                    i++;
                }
                i = 0;
                foreach (XmlNode outputNode in outNodes)
                {
                    string name = outputNode.Attributes[0].Value;
                    var data = new PortData(name, "Output #" + (i + 1), typeof(object));
                    if (OutPortData.Count > i)
                        OutPortData[i] = data;
                    else
                        OutPortData.Add(data);
                    i++;
                }

                //Added it the same way as LoadNode. But unsure of when 'Output' ChildNodes will
                //be added to element. As of now I dont think it is added during serialize

                #region Legacy output support

                foreach (XmlNode subNode in element.ChildNodes)
                {
                    if (subNode.Name.Equals("Output"))
                    {
                        var data = new PortData(
                            subNode.Attributes[0].Value, "function output", typeof(object));

                        if (OutPortData.Any())
                            OutPortData[0] = data;
                        else
                            OutPortData.Add(data);
                    }
                }

                #endregion


                RegisterAllPorts();

                Guid funId;
                try
                {
                    funId = Guid.Parse(Symbol);
                }
                catch
                {
                    funId = GuidUtility.Create(GuidUtility.UrlNamespace, NickName);
                    Symbol = funId.ToString();
                }

                Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funId);
                Description = helper.ReadString("functionDesc");
            }
        }

        #endregion

        private bool VerifySymbol(out Guid funcId)
        {
            Guid.TryParse(Symbol, out funcId);

            // if the dyf does not exist on the search path...
            if (dynSettings.Controller.CustomNodeManager.Contains(funcId))
                return true;

            var manager = dynSettings.Controller.CustomNodeManager;

            // if there is a node with this name, use it instead
            if (manager.Contains(this.NickName))
            {
                var guid = manager.GetGuidFromName(this.NickName);
                this.Symbol = guid.ToString();
                return true;
            }

            return false;
        }

        private void LoadProxyCustomNode(Guid funcId)
        {
            var proxyDef = new FunctionDefinition(funcId)
            {
                WorkspaceModel =
                    new CustomNodeWorkspaceModel(
                        NickName, "Custom Nodes")
                    {
                        FileName = null
                    }
            };

            SetInputs(new List<string>());
            SetOutputs(new List<string>());
            RegisterAllPorts();
            State = ElementState.ERROR;

            var userMsg = "Failed to load custom node: " + NickName +
                          ".  Replacing with proxy custom node.";

            DynamoLogger.Instance.Log(userMsg);

            // tell custom node loader, but don't provide path, forcing user to resave explicitly
            dynSettings.Controller.CustomNodeManager.SetFunctionDefinition(funcId, proxyDef);
            Definition = dynSettings.Controller.CustomNodeManager.GetFunctionDefinition(funcId);
            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override void Evaluate(
            FSharpList<FScheme.Value> args, Dictionary<PortData, FScheme.Value> outPuts)
        {
            if (OutPortData.Count > 1)
            {
                var query = (Evaluate(args) as FScheme.Value.List).Item.Zip(
                    OutPortData, (value, data) => new { value, data });

                foreach (var result in query)
                    outPuts[result.data] = result.value;
            }
            else
                base.Evaluate(args, outPuts);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            return ((FScheme.Value.Function)Controller.FSchemeEnvironment.LookupSymbol(Symbol))
                .Item.Invoke(args);
        }
    }

    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function parameter, use with custom nodes")]
    [NodeSearchTags("variable", "argument", "parameter")]
    [IsInteractive(false)]
    public partial class Symbol : NodeModel
    {
        public Symbol()
        {
            OutPortData.Add(new PortData("", "Symbol", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override bool RequiresRecalc
        {
            get { return false; }
            set { }
        }

        private string _inputSymbol = "";

        public string InputSymbol
        {
            get { return _inputSymbol; }
            set
            {
                _inputSymbol = value;
                ReportModification();
                RaisePropertyChanged("InputSymbol");
            }
        }

        protected internal override INode Build(
            Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
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

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    InputSymbol = subNode.Attributes[0].Value;
                }
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }
    }

    [NodeName("Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function output, use with custom nodes")]
    [IsInteractive(false)]
    public partial class Output : NodeModel
    {
        public Output()
        {
            InPortData.Add(new PortData("", "", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override bool RequiresRecalc
        {
            get { return false; }
            set { }
        }

        private string _symbol = "";

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                ReportModification();
                RaisePropertyChanged("Symbol");
            }
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    Symbol = subNode.Attributes[0].Value;
                }
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }
    }
}

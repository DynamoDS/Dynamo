using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VMDataBridge;

namespace CoreNodeModels
{
    [NodeName("Remember")]
    [NodeDescription(nameof(Properties.Resources.RememberDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [InPortNames(">")]
    [InPortTypes("object")]
    [InPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberInputToolTip))]
    [OutPortNames(">")]
    [OutPortTypes("object")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberOuputToolTip))]
    [IsDesignScriptCompatible]
    [DynamoServices.RegisterForTrace]
    public class Remember : NodeModel
    {
        private string _cache = "";
        private string _updatedToolTipText = "";

        [JsonProperty("Cache")]
        public string Cache
        {
            get { return _cache; }
            set
            {
                var valueToSet = value == null ? "" : value;
                if (valueToSet != _cache)
                {
                    _cache = valueToSet;
                    MarkNodeAsModified();
                }
            }
        }

        [JsonConstructor]
        private Remember(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PropertyChanged += OnPropertyChanged;
        }

        public Remember()
        {
            RegisterAllPorts();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "State":
                    if (State == ElementState.Warning)
                    {
                        Cache = "";
                    }
                    break;

                case "ToolTipText":
                    if (State == ElementState.Warning && ToolTipText != _updatedToolTipText)
                    {
                        string[] errorMessages =
                            ToolTipText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                        _updatedToolTipText = errorMessages.Last();
                        ToolTipText = _updatedToolTipText;
                    }

                    break;

                default:
                    // Nothing to handle
                    break;
            }
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        private static readonly string BuiltinDictionaryTypeName = typeof(DesignScript.Builtin.Dictionary).FullName;
        private static readonly string BuiltinDictionaryGet = nameof(DesignScript.Builtin.Dictionary.ValueAtKey);

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();

            var funtionInputs = new List<AssociativeNode> { inputAstNodes[0], AstFactory.BuildStringNode(Cache) };

            //First build the function call
            var functionCall = AstFactory.BuildFunctionCall(
               new Func<object, string, Dictionary<string, object>>(DSCore.Data.Remember), funtionInputs);

            var functionCallIndent = AstFactory.BuildIdentifier(GUID + "_func");

            resultAst.Add(AstFactory.BuildAssignment(functionCallIndent, functionCall));

            //Next add the first key value pair to the output port
            var getFirstKey = AstFactory.BuildFunctionCall(BuiltinDictionaryTypeName, BuiltinDictionaryGet,
                new List<AssociativeNode> { functionCallIndent, AstFactory.BuildStringNode(">") });

            resultAst.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), getFirstKey));

            //Second get the key value pair to pass to the databridge callback
            var getSecondKey = AstFactory.BuildFunctionCall(BuiltinDictionaryTypeName, BuiltinDictionaryGet,
                new List<AssociativeNode> { functionCallIndent, AstFactory.BuildStringNode("Cache") });

            resultAst.Add(AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(GUID + "_db"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), getSecondKey)));

            return resultAst;
        }

        private void DataBridgeCallback(object callbackObject)
        {
            if (DSCore.Data.CanObjectBeCached(callbackObject))
            {
                Cache = callbackObject as String;
            }
        }
    }
}

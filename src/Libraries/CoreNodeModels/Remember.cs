using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VMDataBridge;

namespace CoreNodeModels
{
    [NodeName("Remember")]
    [NodeDescription(nameof(Properties.Resources.RememberDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [InPortNames(">")]
    [InPortTypes("var[]..[]")]
    [InPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberInputToolTip))]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberOuputToolTip))]
    [IsDesignScriptCompatible]
    public class Remember : NodeModel
    {
        private string cache = "";

        public string Cache
        {
            get { return cache; }
            set
            {
                var valueToSet = value == null ? "" : value;
                if (valueToSet != cache)
                {
                    cache = valueToSet;
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
                case nameof(State):
                    if (State == ElementState.Warning)
                    {
                        Cache = "";
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
            PropertyChanged -= OnPropertyChanged;
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

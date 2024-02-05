using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;


namespace CoreNodeModels
{
    [NodeName("DefineData")]
    [NodeDescription(nameof(Properties.Resources.RememberDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [InPortNames("InputValue", "TypeID", "Context")]
    [InPortTypes("var[]..[]", "string", "boolean")]
    [InPortDescriptions(typeof(Properties.Resources),
        nameof(Properties.Resources.RememberInputToolTip),
        nameof(Properties.Resources.RememberInputToolTip),
        nameof(Properties.Resources.RememberInputToolTip))]
    [OutPortNames("OutputValue")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberOuputToolTip))]
    [IsDesignScriptCompatible]
    internal class DefineData : NodeModel
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
        private DefineData(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            PropertyChanged += OnPropertyChanged;
        }

        public DefineData()
        {
            RegisterAllPorts();
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {

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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();

            // Function call inputs - reference to the function, and the function arguments coming from the inputs
            var function = new Func<object, string, bool, bool>(DSCore.Data.IsSupportedDataType);
            var funtionInputs = new List<AssociativeNode> { inputAstNodes[0], inputAstNodes[1], inputAstNodes[2] };

            //First build the function call
            var functionCall = AstFactory.BuildFunctionCall(function, funtionInputs);

            resultAst.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall));

            // Now build the cal for the DataBridge ??
            var functionCallIndentifier = AstFactory.BuildIdentifier(GUID + "_func");

            resultAst.Add(AstFactory.BuildAssignment(
                    functionCallIndentifier,
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes))));


            return resultAst;
        }


        /// <summary>
        /// Not sure at the moment how relevant is the databridge for this node type 
        /// </summary>
        /// <param name="data"></param>
        private void DataBridgeCallback(object data)
        {
            var inputs = data as ArrayList;

            var inputObject = inputs[0];
            var dataType = inputs[1] as string;
            var context = (bool)inputs[2];

            if (!InPorts[0].IsConnected && !InPorts[1].IsConnected && !InPorts[2].IsConnected)
            {
                return;
            }

        }
    }
}

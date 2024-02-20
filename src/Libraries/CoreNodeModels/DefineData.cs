using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;


namespace CoreNodeModels
{
    [NodeName("DefineData")]
    [NodeDescription(nameof(Properties.Resources.RememberDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [InPortNames(">")]
    [InPortTypes("var[]..[]")]
    [InPortDescriptions(typeof(Properties.Resources),
        nameof(Properties.Resources.RememberInputToolTip))]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberOuputToolTip))]
    [IsDesignScriptCompatible]
    public class DefineData : DSDropDownBase
    {
        private bool context;
        private List<DynamoDropDownItem> serializedItems;


        public bool Context
        {
            get => context;
            set => context = value;
        }

        /// <summary>
        /// Copy of <see cref="DSDropDownBase.Items"/> to be serialized./>
        /// </summary>
        [JsonProperty]
        protected List<DynamoDropDownItem> SerializedItems
        {
            get => serializedItems;
            set
            {
                serializedItems = value;

                Items.Clear();

                foreach (DynamoDropDownItem item in serializedItems)
                {
                    Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Construct a new Custom Dropdown Menu node
        /// </summary>
        public DefineData() : base(">")
        {
            RegisterAllPorts();
            PropertyChanged += OnPropertyChanged;

            foreach (var dataType in Enum.GetValues(typeof(DSCore.Data.DataType)))
            {
                string displayName = dataType.ToString();
                string value = displayName;

                Items.Add(new DynamoDropDownItem(displayName, value));
            }

            SelectedIndex = 0;
        }

        [JsonConstructor]
        private DefineData(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(">", inPorts, outPorts)
        {
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

            // function call inputs - reference to the function, and the function arguments coming from the inputs
            // the object to be (type) evaluated
            // the expected datatype
            // if the input is an ArrayList or not
            var function = new Func<object, string, bool, bool>(DSCore.Data.IsSupportedDataType);
            var funtionInputs = new List<AssociativeNode> {
                inputAstNodes[0],
                AstFactory.BuildStringNode(Items[0].Item.ToString()),
                AstFactory.BuildBooleanNode(Context) };


            var functionCall = AstFactory.BuildFunctionCall(function, funtionInputs);
            var functionCallIdentifier = AstFactory.BuildIdentifier(GUID + "_func");

            // build the function call
            resultAst.Add(AstFactory.BuildAssignment(functionCallIdentifier, functionCall));

            // build the output call
            resultAst.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall));

            // build the call for the DataBridge 
            resultAst.Add(AstFactory.BuildAssignment(functionCallIdentifier,
                DataBridge.GenerateBridgeDataAst(GUID.ToString(),
                AstFactory.BuildExprList(inputAstNodes))));


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

            if (!InPorts[0].IsConnected)
            {
                return;
            }

        }


        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            return SelectionState.Restore;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            serializedItems = Items.ToList();
        }
    }
}

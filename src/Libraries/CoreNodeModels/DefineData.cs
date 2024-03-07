using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using DSCore;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;


namespace CoreNodeModels
{
    [NodeName("DefineData")]
    [NodeDescription(nameof(Properties.Resources.RememberDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.RememberOuputToolTip))]
    [IsDesignScriptCompatible]
    public class DefineData : DSDropDownBase
    {
        private bool context;
        private List<DynamoDropDownItem> serializedItems;

        [JsonProperty]
        public bool IsList { get; set; }

        [JsonProperty]
        public bool AutoMode { get; set; }

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
        /// Construct a new DefineData Dropdown Menu node
        /// </summary>
        public DefineData() : base(">")
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", Properties.Resources.WatchPortDataInputToolTip)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Properties.Resources.WatchPortDataResultToolTip)));

            RegisterAllPorts();

            PropertyChanged += OnPropertyChanged;

            //Items.Add(new DynamoDropDownItem("Select a type", null));

            foreach (var dataType in Data.GetDataNodeDynamoTypeList())
            {
                var displayName = dataType.Name;
                var value = dataType;

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

        private static readonly string BuiltinDictionaryTypeName = typeof(DesignScript.Builtin.Dictionary).FullName;
        private static readonly string BuiltinDictionaryGet = nameof(DesignScript.Builtin.Dictionary.ValueAtKey);

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();

            // function call inputs - reference to the function, and the function arguments coming from the inputs
            // the object to be (type) evaluated
            // the expected datatype
            // if the input is an ArrayList or not
            var function = new Func<object, string, bool, Dictionary<string, object>>(DSCore.Data.IsSupportedDataNodeType);
            var funtionInputs = new List<AssociativeNode> {
                inputAstNodes[0],
                AstFactory.BuildStringNode((Items[SelectedIndex].Item as Data.DataNodeDynamoType).Type.ToString()),
                AstFactory.BuildBooleanNode(IsList) };


            var functionCall = AstFactory.BuildFunctionCall(function, funtionInputs);
            var functionCallIdentifier = AstFactory.BuildIdentifier(GUID + "_func");

            resultAst.Add(AstFactory.BuildAssignment(functionCallIdentifier, functionCall));

            //Next add the first key value pair to the output port
            var getFirstKey = AstFactory.BuildFunctionCall(BuiltinDictionaryTypeName, BuiltinDictionaryGet,
                new List<AssociativeNode> { functionCallIdentifier, AstFactory.BuildStringNode(">") });

            resultAst.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), getFirstKey));

            //Second get the key value pair to pass to the databridge callback
            var getSecondKey = AstFactory.BuildFunctionCall(BuiltinDictionaryTypeName, BuiltinDictionaryGet,
                new List<AssociativeNode> { functionCallIdentifier, AstFactory.BuildStringNode("Validation") });

            resultAst.Add(AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(GUID + "_db"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), getSecondKey)));

            return resultAst;
        }


        /// <summary>
        /// Not sure at the moment how relevant is the databridge for this node type 
        /// </summary>
        /// <param name="data"></param>
        private void DataBridgeCallback(object data)
        {
            var validationResult = (bool) data; // the result of the validation function - true/false

            // do things with the dropdown or throw error
            if (!validationResult && !AutoMode)
            {
                SelectedIndex = 0;
            }
            if(!validationResult && AutoMode)
            {
                // set the type to the dropdown
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

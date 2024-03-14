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
using static DSCore.Data;


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
        private List<DynamoDropDownItem> serializedItems;
        private bool isAutoMode;
        private bool isList;

        /// <summary>
        /// AutoMode property
        /// </summary>
        [JsonProperty]
        public bool IsAutoMode
        {
            get { return isAutoMode; }
            set
            {
                isAutoMode = value;
                OnNodeModified();
                RaisePropertyChanged(nameof(IsAutoMode));
            }
        }

        /// <summary>
        /// IsList property
        /// </summary>
        [JsonProperty]
        public bool IsList
        {
            get { return isList; }
            set
            {
                isList = value;
                OnNodeModified();
                RaisePropertyChanged(nameof(IsList));
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
            if(inputAstNodes == null)
            {
                throw new ArgumentNullException("The node was called with null stuff");
            }

            var resultAst = new List<AssociativeNode>();

            // function call inputs - reference to the function, and the function arguments coming from the inputs
            // the object to be (type) evaluated
            // the expected datatype
            // if the input is an ArrayList or not
            var function = new Func<object, string, bool, bool, Dictionary<string, object>>(DSCore.Data.IsSupportedDataNodeType);
            var funtionInputs = new List<AssociativeNode> {
                inputAstNodes[0],
                AstFactory.BuildStringNode((Items[SelectedIndex].Item as Data.  DataNodeDynamoType).Type.ToString()),
                AstFactory.BuildBooleanNode(IsList),
                AstFactory.BuildBooleanNode(IsAutoMode)
            };


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
            if (data == null) return;

            (bool IsValid, bool UpdateList, DataNodeDynamoType InputType) resultData = (ValueTuple<bool, bool, DataNodeDynamoType>)data;

            if (IsAutoMode && resultData.UpdateList)
            {
                IsList = !IsList;
            }

            if (!resultData.IsValid)
            {
                if (IsAutoMode)
                {
                    // Assign to the correct value, if the object was of supported type
                    if (resultData.InputType != null)
                    {
                        var index = Items.IndexOf(Items.First(i => i.Name.Equals(resultData.InputType.Name)));
                        SelectedIndex = index;
                    }
                }
                else
                {
                    // Throw an exception/warning and go back to the default dropdown value
                    SelectedIndex = 0;
                }
            }

        }


        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            foreach (var dataType in Data.GetDataNodeDynamoTypeList())
            {
                var displayName = dataType.Name;
                var value = dataType;

                Items.Add(new DynamoDropDownItem(displayName, value));
            }

            SelectedIndex = 0;

            return SelectionState.Restore;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            serializedItems = Items.ToList();
        }
    }
}

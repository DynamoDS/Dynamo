using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using DSCore;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;
using VMDataBridge;
using static DSCore.Data;


namespace CoreNodeModels
{
    [NodeName("DefineData")]
    [NodeDescription(nameof(Properties.Resources.DefineDataDescription), typeof(Properties.Resources))]
    [NodeCategory("Core.Data")]
    [InPortNames(">")]
    [InPortTypes("var[]..[]")]
    [InPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.DefineDataInputTooltip))]
    [OutPortNames(">")]
    [OutPortTypes("var[]..[]")]
    [OutPortDescriptions(typeof(Properties.Resources), nameof(Properties.Resources.DefineDataOutputTooltip))]
    [IsDesignScriptCompatible]
    [AlsoKnownAs("Data.DefineData")]
    public class DefineData : DSDropDownBase
    {
        private bool isAutoMode = true; // default start with auto-detect 'on'
        private bool isList;
        private string playerValue = "";
        private string displayValue = Properties.Resources.DefineDataDisplayValueMessage;

        /// <summary>
        /// The IsAutoMode property enables the node to automatically validate and process input data.
        /// AutoMode = true: the node checks input types for serialization compatibility, supports single values and non-nested lists,
        /// and distinguishes between homogeneous and certain heterogeneous collections through inheritance.
        /// Invalid or unsupported data types result in error messages,
        /// while successful validation updates node properties and UI elements to reflect the processed data.
        /// AutoMode = false: the node enters a manual processing mode,
        /// where it strictly validates that the TypeID and Context predefined on the node match the attached input data.
        /// Mismatches in expected data types or contexts—such as receiving a list instead of a single item,
        /// or input data not matching the specified TypeID—result in errors, without automatically adjusting node settings.
        /// This manual mode maintains the node's current configurations, ensuring an output is passed only when valid data is processed,
        /// and retains the node's state in warning without resetting selections for invalid data.
        /// </summary>
        [JsonProperty]
        public bool IsAutoMode
        {
            get => isAutoMode;
            set
            {
                isAutoMode = value;
                OnNodeModified();
                RaisePropertyChanged(nameof(IsAutoMode));
            }
        }

        /// <summary>
        /// IsList property defines if the input is of a type ArrayList.
        /// The node supports only non-nested lists of homogeneous or heterogenous collections through inheritance
        /// </summary>
        [JsonProperty]
        public bool IsList
        {
            get => isList;
            set
            {
                isList = value;
                OnNodeModified();
                RaisePropertyChanged(nameof(IsList));
            }
        }

        /// <summary>
        /// This is a mediator property handling the displayed value on the dropdown
        /// </summary>
        ///
        [JsonProperty]
        public string DisplayValue
        {
            get => displayValue;
            set
            {
                if (displayValue != value)
                {
                    displayValue = value;
                    RaisePropertyChanged(nameof(DisplayValue));
                }
            }
        }


        [JsonIgnore]
        public override bool IsInputNode => true;

        [JsonIgnore]
        public string PlayerValue
        {
            get { return playerValue; }
            set
            {
                if (Equals(this.playerValue, null) || !this.playerValue.Equals(value))
                {
                    playerValue = value ?? "";
                    MarkNodeAsModified();
                }
            }
        }

        /// <summary>
        /// Construct a new DefineData Dropdown Menu node
        /// </summary>
        public DefineData() : base(">")
        {
            PropertyChanged += OnPropertyChanged;

            //Items.Add(new DynamoDropDownItem("Select a type", null));

            foreach (var dataType in Data.DataNodeDynamoTypeList)
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

        private static readonly string BuiltinDictionaryTypeName = typeof(DesignScript.Builtin.Dictionary).FullName;
        private static readonly string BuiltinDictionaryGet = nameof(DesignScript.Builtin.Dictionary.ValueAtKey);

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var resultAst = new List<AssociativeNode>();

            // function call inputs - reference to the function, and the function arguments coming from the inputs
            // the object to be (type) evaluated
            // the expected datatype
            // if the input is an ArrayList or not
            var function = new Func<object, string, bool, bool, string, Dictionary<string, object>>(EvaluateDefineDataNode);
            var functionInputs = new List<AssociativeNode> {
                inputAstNodes[0],
                AstFactory.BuildStringNode((Items[SelectedIndex].Item as Data.DataNodeDynamoType).Type.ToString()),
                AstFactory.BuildBooleanNode(IsList),
                AstFactory.BuildBooleanNode(IsAutoMode),
                AstFactory.BuildStringNode(PlayerValue)
            };

            var functionCall = AstFactory.BuildFunctionCall(function, functionInputs);
            var functionCallIdentifier = AstFactory.BuildIdentifier(GUID + "_func");

            resultAst.Add(AstFactory.BuildAssignment(functionCallIdentifier, functionCall));

            //Next add the first key value pair to the output port
            var getFirstKey = AstFactory.BuildFunctionCall(
                BuiltinDictionaryTypeName,
                BuiltinDictionaryGet,
                [functionCallIdentifier, AstFactory.BuildStringNode(">")]);

            resultAst.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), getFirstKey));

            //Second get the key value pair to pass to the databridge callback
            var getSecondKey = AstFactory.BuildFunctionCall(
                BuiltinDictionaryTypeName,
                BuiltinDictionaryGet,
                [functionCallIdentifier, AstFactory.BuildStringNode("Validation")]);

            resultAst.Add(AstFactory.BuildAssignment(
                AstFactory.BuildIdentifier(GUID + "_db"),
                DataBridge.GenerateBridgeDataAst(GUID.ToString(), getSecondKey)));

            return resultAst;
        }


        private void DataBridgeCallback(object data)
        {
            //Todo If the playerValue is not empty string then we can change the UI to reflect the value is coming from the player
            //Todo if the function call throws we don't get back to DatabridgeCallback.  Not sure if we need to handle this case

            //Now we reset this value to empty string so that the next time a value is set from upstream nodes we can know that it is not coming from the player
            playerValue = "";

            if (data == null)
            {
                if (IsAutoMode)
                {
                    DisplayValue = string.Empty; // show blank if we are in locked mode (as we cannot interact with the node)
                    //DisplayValue = Properties.Resources.DefineDataDisplayValueMessage;
                }
                else
                {
                    DisplayValue = SelectedString;
                }
                return;
            }

            // If data is not null
            (bool IsValid, bool UpdateList, DataNodeDynamoType InputType) = (ValueTuple<bool, bool, DataNodeDynamoType>)data;

            if (IsAutoMode)
            {
                if (UpdateList)
                {
                    IsList = !IsList;
                }

                if (InputType != null)
                {
                    if (!IsValid)
                    {
                        // Assign to the correct value, if the object was of supported type
                        var index = Items.IndexOf(Items.First(i => i.Name.Equals(InputType.Name)));
                        SelectedIndex = index;
                    }
                    if (!DisplayValue.Equals(InputType.Name))
                    {
                        DisplayValue = InputType.Name;
                    }
                }
            }
            else
            {
                DisplayValue = SelectedString;
            }
        }


        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            foreach (var dataType in Data.DataNodeDynamoTypeList)
            {
                var displayName = dataType.Name;
                var value = dataType;

                Items.Add(new DynamoDropDownItem(displayName, value));
            }

            SelectedIndex = 0;

            return SelectionState.Restore;
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            switch (name)
            {
                case "Value":
                    PlayerValue = value;
                    return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }
    }
}

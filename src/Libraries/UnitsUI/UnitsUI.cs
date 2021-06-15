using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

using DSCore;

using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Migration;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using UnitsUI.Properties;
using Newtonsoft.Json;
using ProtoCore.AST.ImperativeAST;
using UnitsUI.Controls;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using Dynamo.Utilities;
using Dynamo.Engine.CodeGeneration;
using System.Collections;
using VMDataBridge;
using UnitsUI.Converters;

namespace UnitsUI
{

    [NodeName("Number From Feet and Inches")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("LengthFromStringDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("LengthFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [OutPortTypes("number")]
    [IsDesignScriptCompatible]
    public class LengthFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private LengthFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);
        }

        public LengthFromString():base()
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("double", Resources.LengthFromStringPortDataLengthToolTip)));
            RegisterAllPorts();
        }

        [NodeMigration(version: "0.6.2")]
        public void MigrateLengthFromFeetToMeters(XmlNode node)
        {
            //length values were previously stored as decimal feet
            //convert them internally to SI meters.
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "System.Double")
                {
                    if (child.Attributes != null && child.Attributes.Count > 0)
                    {
                        var valueAttrib = child.Attributes["value"];
                        valueAttrib.Value = (double.Parse(valueAttrib.Value) / Length.ToFoot).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        [NodeMigration(version: "0.7.5.0")]
        public static NodeMigrationData Migrate_0750(NodeMigrationData data)
        {
            var migrationData = new NodeMigrationData(data.Document);
            var oldNode = data.MigratedNodes.ElementAt(0);
            var newNode = MigrationManager.CloneAndChangeName(oldNode, "UnitsUI.LengthFromString", "Number From Feet and Inches", true);

            migrationData.AppendNode(newNode);
            return migrationData;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Measure.UnitValue);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), doubleNode) };
        }
    }



    [NodeName("Area From String")]
    [NodeCategory("Units.Area.Create")]
    [NodeDescription("AreaFromStringDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("AreaFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class AreaFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private AreaFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            Measure = Area.FromDouble(0.0, AreaUnit.SquareMeter);
            Warning("AreaFromString is obsolete.", true);
        }

        public AreaFromString()
        {
            Measure = Area.FromDouble(0.0, AreaUnit.SquareMeter);
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("area", Resources.AreaFromStringPortDataAreaToolTip)));
            RegisterAllPorts();

            Warning("AreaFromString is obsolete.", true);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Value);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), doubleNode) };
        }
    }

  

    [NodeName("Volume From String")]
    [NodeCategory("Units.Volume.Create")]
    [NodeDescription("VolumeFromStringDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("VolumeFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class VolumeFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private VolumeFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            Measure = Volume.FromDouble(0.0, VolumeUnit.CubicMeter);
            Warning("VolumeFromString is obsolete.", true);
        }

        public VolumeFromString()
        {
            Measure = Volume.FromDouble(0.0, VolumeUnit.CubicMeter);
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("volume", Resources.VolumeFromStringPortDataVolumeToolTip)));
            RegisterAllPorts();

            Warning("VolumeFromString is obsolete.", true);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Value);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), doubleNode) };
        }
    }

    [NodeName("Unit Types")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("UnitTypesDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("UnitTypesSearchTags", typeof(UnitsUI.Properties.Resources))]
    [OutPortTypes("type")]
    [IsDesignScriptCompatible]
    public class UnitTypes : AllChildrenOfType<SIUnit>
    {
        public UnitTypes() : base()
        {
        }

        [JsonConstructor]
        private UnitTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var typeName = AstFactory.BuildStringNode(Items[SelectedIndex].Name);
                var assemblyName = AstFactory.BuildStringNode("DynamoUnits");
                node = AstFactory.BuildFunctionCall(new Func<string, string, object>(Types.FindTypeByNameInAssembly), new List<AssociativeNode>() { typeName, assemblyName });
            }
           
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Parse Unit Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("Parse string to unit value")]
    [IsDesignScriptCompatible]
    public class UnitInput : CoreNodeModels.Input.String
    {
        public override string NodeType
        {
            get
            {
                return "UnitInputNode";
            }
        }
        
        private DynamoUnits.Unit selectedUnit;
        private List<DynamoUnits.Unit> items;

        [JsonIgnore]
        public List<DynamoUnits.Unit> Items
        {
            get { return items; }
            private set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }
        
        [JsonProperty("UnitType"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedUnit
        {
            get { return selectedUnit; }
            set
            {
                selectedUnit = value;
                RaisePropertyChanged("SelectedUnit");
            }
        }

        [JsonConstructor]
        private UnitInput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Items =
                DynamoUnits.Utilities.ConvertUnitsDictionaryToList(
                    DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits());
            SelectedUnit = Items[73];
            Value = "";
            ShouldDisplayPreviewCore = true;
        }

        public UnitInput()
        {
            this.OutPorts.Clear();
            this.OutPorts.Add(new PortModel(PortType.Output, (NodeModel)this, new PortData("Value", "Value")));
            this.OutPorts.Add(new PortModel(PortType.Output, (NodeModel)this, new PortData("TypeId", "TypeId")));
            RegisterAllPorts();
            Items =
                DynamoUnits.Utilities.ConvertUnitsDictionaryToList(
                    DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits());
            SelectedUnit = Items[73];
            
            Value = "";
            ShouldDisplayPreviewCore = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if(string.IsNullOrEmpty(Value))
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };

            var expression = AstFactory.BuildStringNode(Value);

            var unitID =
                AstFactory.BuildStringNode(SelectedUnit.TypeId);

           var node = AstFactory.BuildFunctionCall(
                new Func<string, string, double>(DynamoUnits.Utilities.ParseExpressionByUnitId),
                new List<AssociativeNode> { unitID, expression });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node), AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), unitID) };
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "Value")
            {
                Value = value;
                return true; // UpdateValueCore handled.
            }

            // There's another 'UpdateValueCore' method in 'String' base class,
            // since they are both bound to the same property, 'StringInput' 
            // should be given a chance to handle the property value change first
            // before the base class 'String'.
            return base.UpdateValueCore(updateValueParams);
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement(typeof(string).FullName);

            var helper = new XmlElementHelper(outEl);
            helper.SetAttribute("value", SerializeValue());
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(attr.Value);
                        }
                    }
                }
            }
        }
    }

   

    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeName("Convert Units")]
    [NodeDescription("Convert Between Many Units")]
    //[NodeSearchTags("DynamoConvertSearchTags", typeof(Properties.Resources))]
    [OutPortTypes("number")]
    [IsDesignScriptCompatible]
    public class ForgeDynamoConvert : NodeModel
    {
        private DynamoUnits.Unit selectedFromConversion;
        private DynamoUnits.Unit selectedToConversion;
        private DynamoUnits.Quantity selectedQuantityConversion;
        private bool isSelectionFromBoxEnabled;
        private string selectionFromBoxToolTip;
        private List<DynamoUnits.Quantity> quantityConversionSource;
        private List<DynamoUnits.Unit> selectedFromConversionSource;
        private List<DynamoUnits.Unit> selectedToConversionSource;

        /// <summary>
        /// The NodeType property provides a name which maps to the
        /// server type for the node. This property should only be
        /// used for serialization.
        /// </summary>
        public override string NodeType
        {
            get
            {
                return "ConvertUnitsNode";
            }
        }

        [JsonIgnore]
        public List<DynamoUnits.Quantity> QuantityConversionSource
        {
            get { return quantityConversionSource; }
            private set
            {
                quantityConversionSource = value;
                RaisePropertyChanged("SelectedQuantityConversionSource");
            }
        }

        [JsonIgnore]
        public List<DynamoUnits.Unit> SelectedFromConversionSource
        {
            get { return selectedFromConversionSource; }
            set
            {
                selectedFromConversionSource = value;
                RaisePropertyChanged("SelectedFromConversionSource");
            }
        }

        [JsonIgnore]
        public List<DynamoUnits.Unit> SelectedToConversionSource
        {
            get { return selectedToConversionSource; }
            set
            {
                selectedToConversionSource = value;
                RaisePropertyChanged("SelectedToConversionSource");
            }
        }

        [JsonProperty("MeasurementType"), JsonConverter(typeof(ForgeQuantityConverter))]
        public DynamoUnits.Quantity SelectedQuantityConversion
        {
            get { return selectedQuantityConversion; }
            set
            {
                selectedQuantityConversion = value;
                SelectedFromConversionSource =
                    selectedQuantityConversion.Units;
                SelectedToConversionSource =
                    selectedQuantityConversion.Units;
                SelectedFromConversion = SelectedFromConversionSource.First();
                SelectedToConversion = SelectedToConversionSource.First();

                RaisePropertyChanged("SelectedQuantityConversion");
            }
        }

        [JsonProperty("FromConversion"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedFromConversion
        {
            get { return selectedFromConversion; }
            set
            {
                selectedFromConversion = value;
                this.OnNodeModified();
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        [JsonProperty("ToConversion"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                selectedToConversion = value;
                this.OnNodeModified();
                RaisePropertyChanged("SelectedToConversion");
            }
        }

        [JsonIgnore]
        public bool IsSelectionFromBoxEnabled
        {
            get { return isSelectionFromBoxEnabled; }
            set
            {
                isSelectionFromBoxEnabled = value;
                RaisePropertyChanged("IsSelectionFromBoxEnabled");
            }
        }

        [JsonIgnore]
        public string SelectionFromBoxToolTip
        {
            get { return selectionFromBoxToolTip; }
            set
            {
                selectionFromBoxToolTip = value;
                RaisePropertyChanged("SelectionFromBoxToolTip");
            }
        }

        [JsonConstructor]
        private ForgeDynamoConvert(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            QuantityConversionSource =
                DynamoUnits.Utilities.CovertQuantityDictionaryToList(
                    DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities());
            SelectedQuantityConversion = QuantityConversionSource[36];
            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
        }

        public ForgeDynamoConvert()
        {
            QuantityConversionSource =
                DynamoUnits.Utilities.CovertQuantityDictionaryToList(
                    DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities());
            SelectedQuantityConversion = QuantityConversionSource[36];
            AssociativeNode defaultNode = new DoubleNode(0.0);
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", "Tooltip", defaultNode)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", "Tooltip")));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (null == inputAstNodes || inputAstNodes.Count == 0 || inputAstNodes[0] is ProtoCore.AST.AssociativeAST.NullNode)
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            
            var conversionToNode =
                AstFactory.BuildStringNode(SelectedToConversion.TypeId);

            var conversionFromNode =
                AstFactory.BuildStringNode(SelectedFromConversion.TypeId);
            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(
                        new Func<double, string, string, double>(DynamoUnits.Utilities.ConvertByUnitIds),
                        new List<AssociativeNode> { inputAstNodes[0], conversionFromNode, conversionToNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        public void ToggleDropdownValues()
        {
            var temp = this.SelectedFromConversion;
            this.SelectedFromConversion = this.SelectedToConversion;
            this.SelectedToConversion = temp;
        }

        #region Serialization/Deserialization Methods

        //protected override void SerializeCore(XmlElement element, SaveContext context)
        //{
        //    base.SerializeCore(element, context); // Base implementation must be called.

        //    var helper = new XmlElementHelper(element);
        //    helper.SetAttribute("conversionQuantity", SelectedQuantityConversion.ToString());
        //    helper.SetAttribute("conversionFrom", SelectedFromConversion.ToString());
        //    helper.SetAttribute("conversionTo", SelectedToConversion.ToString());
        //}

        //protected override void DeserializeCore(XmlElement element, SaveContext context)
        //{
        //    base.DeserializeCore(element, context); //Base implementation must be called.
        //    var helper = new XmlElementHelper(element);
        //    var quantityConversion = helper.ReadString("conversionQuantity");
        //    var selectedQuantityConversionFrom = helper.ReadString("conversionFrom");
        //    var selectedQuantityConversionTo = helper.ReadString("conversionTo");

        //    SelectedQuantityConversion = Enum.Parse(typeof(ConversionQuantityUnit), quantityConversion) is ConversionQuantityUnit ?
        //                        (ConversionQuantityUnit)Enum.Parse(typeof(ConversionQuantityUnit), quantityConversion) : ConversionQuantityUnit.Length;

        //    SelectedFromConversion = Enum.Parse(typeof(ConversionUnit), selectedQuantityConversionFrom) is ConversionUnit ?
        //                     (ConversionUnit)Enum.Parse(typeof(ConversionUnit), selectedQuantityConversionFrom) : ConversionUnit.Meters;

        //    SelectedToConversion = Enum.Parse(typeof(ConversionUnit), selectedQuantityConversionTo) is ConversionUnit ?
        //                        (ConversionUnit)Enum.Parse(typeof(ConversionUnit), selectedQuantityConversionTo) : ConversionUnit.Meters;

        //}

        //protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        //{
        //    switch (updateValueParams.PropertyName)
        //    {
        //        case "Values":
        //            // Here we expect a string that represents an array of [ quantity, from, to ] values which are separated by ";"
        //            // For example "Length;Meters;Feet"
        //            var vals = updateValueParams.PropertyValue.Split(';');
        //            ConversionQuantityUnit quantity;
        //            ConversionUnit from, to;
        //            if (vals.Length == 3 && Enum.TryParse(vals[0], out quantity)
        //                && Enum.TryParse(vals[1], out from) && Enum.TryParse(vals[2], out to))
        //            {
        //                SelectedQuantityConversion = quantity;
        //                SelectedFromConversion = from;
        //                SelectedToConversion = to;
        //            }

        //            return true;
        //    }

        //    return base.UpdateValueCore(updateValueParams);
        //}

        #endregion
    }

    public class ForgeConverterViewModel : NotificationObject
    {
        private readonly ForgeDynamoConvert dynamoConvertModel;
        public DelegateCommand ToggleButtonClick { get; set; }
        private readonly NodeViewModel nodeViewModel;
        private readonly NodeModel nodeModel;

        public DynamoUnits.Quantity SelectedQuantityConversion
        {
            get { return dynamoConvertModel.SelectedQuantityConversion; }
            set
            {
                dynamoConvertModel.SelectedQuantityConversion = value;
            }
        }

        public DynamoUnits.Unit SelectedFromConversion
        {
            get { return dynamoConvertModel.SelectedFromConversion; }
            set
            {
                if (value == null)
                    return;
                
                dynamoConvertModel.SelectedFromConversion = value;
            }
        }

        public DynamoUnits.Unit SelectedToConversion
        {
            get { return dynamoConvertModel.SelectedToConversion; }
            set
            {
                if (value == null)
                    return;
                
                dynamoConvertModel.SelectedToConversion = value;
            }
        }

        public List<DynamoUnits.Quantity> QuantityConversionSource
        {
            get { return dynamoConvertModel.QuantityConversionSource; }
        }

        public List<DynamoUnits.Unit> SelectedFromConversionSource
        {
            get { return dynamoConvertModel.SelectedFromConversionSource; }
            set
            {
                dynamoConvertModel.SelectedFromConversionSource = value;
            }
        }

        public List<DynamoUnits.Unit> SelectedToConversionSource
        {
            get { return dynamoConvertModel.SelectedToConversionSource; }
            set
            {
                dynamoConvertModel.SelectedToConversionSource = value;
            }
        }

        public bool IsSelectionFromBoxEnabled
        {
            get { return dynamoConvertModel.IsSelectionFromBoxEnabled; }
            set
            {
                dynamoConvertModel.IsSelectionFromBoxEnabled = value;
            }
        }

        public string SelectionFromBoxToolTip
        {
            get { return dynamoConvertModel.SelectionFromBoxToolTip; }
            set
            {
                dynamoConvertModel.SelectionFromBoxToolTip = value;
            }
        }

        public ForgeConverterViewModel(ForgeDynamoConvert model, NodeView nodeView)
        {
            dynamoConvertModel = model;
            nodeViewModel = nodeView.ViewModel;
            nodeModel = nodeView.ViewModel.NodeModel;
            model.PropertyChanged += model_PropertyChanged;
            ToggleButtonClick = new DelegateCommand(OnToggleButtonClick, CanToggleButton);
        }

        private void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedQuantityConversionSource":
                    RaisePropertyChanged("SelectedQuantityConversionSource");
                    break;
                case "SelectedQuantityConversion":
                    RaisePropertyChanged("SelectedQuantityConversion");
                    break;
                case "SelectedFromConversionSource":
                    RaisePropertyChanged("SelectedFromConversionSource");
                    break;
                case "SelectedToConversionSource":
                    RaisePropertyChanged("SelectedToConversionSource");
                    break;
                case "SelectedFromConversion":
                    RaisePropertyChanged("SelectedFromConversion");
                    break;
                case "SelectedToConversion":
                    RaisePropertyChanged("SelectedToConversion");
                    break;
                case "IsSelectionFromBoxEnabled":
                    RaisePropertyChanged("IsSelectionFromBoxEnabled");
                    break;
                case "SelectionFromBoxToolTip":
                    RaisePropertyChanged("SelectionFromBoxToolTip");
                    break;
            }
        }

        /// <summary>
        /// Called when Toggle button is clicked.
        /// Switches the combo box values
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnToggleButtonClick(object obj)
        {
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
            dynamoConvertModel.ToggleDropdownValues();
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
        }

        private bool CanToggleButton(object obj)
        {
            return true;
        }
    }

   

    

    [NodeName("Units")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [OutPortTypes("Unit")]
    [IsDesignScriptCompatible]
    public class Units : DSDropDownBase
    {
        public Units() : base("Unit") { }

        [JsonConstructor]
        private Units(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Unit", inPorts, outPorts) { }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var args = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode((string)Items[SelectedIndex].Item)
                };

                var func = new Func<string, DynamoUnits.Unit>(DynamoUnits.Unit.ByTypeID);
                node = AstFactory.BuildFunctionCall(func, args);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits();

            var units = dictionary.Values.ToArray();
            for (int i = 0; i < units.Length - 1; i++)
            {
                if (DynamoUnits.Utilities.TypeIdShortName(units[i].getTypeId()).Equals(DynamoUnits.Utilities.TypeIdShortName(units[i + 1].getTypeId())))
                    continue;

                Items.Add(new DynamoDropDownItem(units[i].getName(), units[i].getTypeId()));
            }

            Items.Add(new DynamoDropDownItem(units.Last().getName(), units.Last().getTypeId()));

            //Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
    }

    [NodeName("Quantities")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [OutPortTypes("Quantity")]
    [IsDesignScriptCompatible]
    public class Quantities : DSDropDownBase
    {
        public Quantities() : base("Quantity") { }

        [JsonConstructor]
        private Quantities(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Quantity", inPorts, outPorts) { }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var args = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode((string)Items[SelectedIndex].Item)
                };

                var func = new Func<string, DynamoUnits.Quantity>(DynamoUnits.Quantity.ByTypeID);
                node = AstFactory.BuildFunctionCall(func, args);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities();

            var quantities = dictionary.Values.ToArray();
            for (int i = 0; i < quantities.Length - 1; i++)
            {
                if (DynamoUnits.Utilities.TypeIdShortName(quantities[i].getTypeId()).Equals(DynamoUnits.Utilities.TypeIdShortName(quantities[i + 1].getTypeId())))
                    continue;

                Items.Add(new DynamoDropDownItem(quantities[i].getName(), quantities[i].getTypeId()));
            }

            Items.Add(new DynamoDropDownItem(quantities.Last().getName(), quantities.Last().getTypeId()));

            //Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }
    }

    [NodeName("Symbols")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [OutPortTypes("Symbol")]
    [IsDesignScriptCompatible]
    public class Symbols : DSDropDownBase
    {
        public Symbols() : base("Symbol") { }

        [JsonConstructor]
        private Symbols(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base("Symbol", inPorts, outPorts) { }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            if (SelectedIndex < 0 || SelectedIndex >= Items.Count)
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var args = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode((string)Items[SelectedIndex].Item)
                };

                var func = new Func<string, DynamoUnits.UnitSymbol>(DynamoUnits.UnitSymbol.ByTypeID);
                node = AstFactory.BuildFunctionCall(func, args);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            var dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllSymbols();

            var symbols = dictionary.Values.ToArray();
            for (int i = 0; i < symbols.Length - 1; i++)
            {
                if (DynamoUnits.Utilities.TypeIdShortName(symbols[i].getTypeId()).Equals(DynamoUnits.Utilities.TypeIdShortName(symbols[i + 1].getTypeId())))
                    continue;


                Items.Add(new DynamoDropDownItem(SymbolDisplayText(symbols[i]), symbols[i].getTypeId()));
            }

            Items.Add(new DynamoDropDownItem(SymbolDisplayText(symbols.Last()), symbols.Last().getTypeId()));

            //Items = Items.OrderBy(x => x.Name).ToObservableCollection();
            return SelectionState.Restore;
        }

        private string SymbolDisplayText(ForgeUnitsCLR.Symbol symbol)
        {
            var symbolStr = symbol.getUnit().getName() + ": ";
            var symbolTxt = symbol.getPrefixOrSuffix() != null ? symbol.getPrefixOrSuffix().getText() : "";

            return symbolStr + symbolTxt;
        }
    }

    [NodeName("Unit Value Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("UnitValueOutputDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("UnitValueOutputSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class UnitValueOutput : NodeModel
    {
        //public event Action<object> EvaluationComplete;

        //[JsonIgnore]
        //public new object CachedValue;

        ///// <summary>
        /////     Has the Watch node been run once?  If not, the CachedValue
        /////     is technically not accurate.
        ///// </summary>
        //[JsonIgnore]
        //public bool HasRunOnce { get; private set; }
        [JsonIgnore]
        private string formattedString;
    
        public double Value { get; set; }
        public Unit Unit { get; set; }
        public UnitSymbol Symbol { get; set; }
        public int Precision { get; set; }

        public bool Decimal { get; set; }
        public string FormattedString
        {
            get => formattedString;
            set
            {
                formattedString = value;
                RaisePropertyChanged(nameof(FormattedString));
            }
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            VMDataBridge.DataBridge.Instance.RegisterCallback(GUID.ToString(), DataBridgeCallback);
        }

        private void DataBridgeCallback(object data)
        {
           // this.CachedValue = data;
           // this.HasRunOnce = true;

           //// if (EvaluationComplete != null)
           // {
           //     EvaluationComplete(data);


                ArrayList inputs = data as ArrayList;
                Value = Convert.ToDouble(inputs[0]);
                Unit = inputs[1] as Unit;
                Symbol = inputs[2] as UnitSymbol;
                Precision = Convert.ToInt32(inputs[3]);
                Decimal = Convert.ToBoolean(inputs[4]);

                FormattedString = DynamoUnits.Utilities.ReturnFormattedString(Value, Unit, Symbol, Precision, Decimal);
            //}
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        [JsonConstructor]
        private UnitValueOutput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
        }

        public UnitValueOutput()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(nameof(Value), "Tooltip")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(nameof(Unit), "Tooltip")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(nameof(Symbol), "Tooltip")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(nameof(Precision), "Tooltip")));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData(nameof(Decimal), "Tooltip")));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || 
                !InPorts[1].IsConnected ||
                !InPorts[2].IsConnected ||
                !InPorts[3].IsConnected ||
                !InPorts[4].IsConnected)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            //if (IsPartiallyApplied)
            //{
            //    return new[]
            //    {
            //        AstFactory.BuildAssignment(
            //            GetAstIdentifierForOutputIndex(0),
            //            AstFactory.BuildFunctionObject(
            //                new ProtoCore.AST.AssociativeAST.IdentifierListNode
            //                {
            //                    LeftNode = AstFactory.BuildIdentifier("DataBridge"),
            //                    RightNode = AstFactory.BuildIdentifier("BridgeData")
            //                },
            //                2,
            //                new[] { 0 },
            //                new List<AssociativeNode>
            //                {
            //                    AstFactory.BuildStringNode(GUID.ToString()),
            //                    AstFactory.BuildNullNode()
            //                }))
            //    };
            //}

            return new[]{ 
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    VMDataBridge.DataBridge.GenerateBridgeDataAst(GUID.ToString(), AstFactory.BuildExprList(inputAstNodes))),
                 AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
            };

        }

        //private string ReturnFormattedString(double numValue, Unit unit, UnitSymbol unitSymbol, int precision, bool decimalFormat)
        //{
        //    string outputString = string.Empty;
        //    if (decimalFormat)
        //        outputString = UnitSymbol.StringifyDecimal(numValue, precision, unitSymbol, true);
        //    else
        //        outputString = UnitSymbol.StringifyFraction(numValue, precision, unitSymbol);
        //    return outputString;
        //}
    }

    class UnitValueOutputViewCustomization : INodeViewCustomization<UnitValueOutput>
    {
        private NodeModel nodeModel;
        private NodeViewModel nodeViewModel;
        private UnitValueOutput model;

        public void CustomizeView(UnitValueOutput model, NodeView nodeView)
        {

            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                TextWrapping = TextWrapping.Wrap,
                MinHeight = Configurations.PortHeightInPixels,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch

            };
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            tb.IsReadOnly = true;

            tb.DataContext = model;
            tb.BindToProperty(new Binding(nameof(UnitValueOutput.FormattedString))
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            nodeView.inputGrid.Children.Add(tb);
        }

        public void Dispose()
        {

        }
    }
}

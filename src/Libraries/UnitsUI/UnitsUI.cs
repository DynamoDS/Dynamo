using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using DSCore;
using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using UnitsUI.Properties;
using Newtonsoft.Json;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using Dynamo.Utilities;
using UnitsUI.Converters;
using Utilities = DynamoUnits.Utilities;

namespace UnitsUI
{

    [NodeName("Number From Feet and Inches")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("LengthFromStringDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("LengthFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [OutPortTypes("number")]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class LengthFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private LengthFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts)
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);
            Warning("Number From Feet and Inches " + Properties.Resources.LegthFromStringObsoleteMessage, true);
        }

        public LengthFromString():base()
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("double", Resources.LengthFromStringPortDataLengthToolTip)));
            RegisterAllPorts();
            Warning("Number From Feet and Inches " + Properties.Resources.LegthFromStringObsoleteMessage, true);
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
    [NodeDeprecated]
    public class UnitTypes : AllChildrenOfType<SIUnit>
    {
        public UnitTypes() : base()
        {
            Warning("Unit Types " + Properties.Resources.UnitTypesObsoleteMessage, true);
        }

        [JsonConstructor]
        private UnitTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Warning("Unit Types " + Properties.Resources.UnitTypesObsoleteMessage, true);
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
    [NodeDescription("ParseUnitInputDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("ParseUnitInputSearchTags", typeof(UnitsUI.Properties.Resources))]
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
        
        private List<DynamoUnits.Unit> items;

        /// <summary>
        /// List of unit type ids.
        /// </summary>
        [JsonIgnore]
        public List<DynamoUnits.Unit> Items
        {
            get { return items; }
            private set
            {
                items = value;
                RaisePropertyChanged(nameof(Items));
            }
        }

        private DynamoUnits.Unit selectedUnit;
        /// <summary>
        /// Selected unit type id to convert to.
        /// </summary>
        [JsonProperty("UnitType"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedUnit
        {
            get { return selectedUnit; }
            set
            {
                selectedUnit = value;
                RaisePropertyChanged(nameof(SelectedUnit));
            }
        }

        [JsonConstructor]
        private UnitInput(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            try
            {
                Items =
                    DynamoUnits.Utilities.ConvertUnitsDictionaryToList(
                        DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits());
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            ShouldDisplayPreviewCore = true;
        }

        public UnitInput()
        {
            this.OutPorts.Clear();
            this.OutPorts.Add(new PortModel(PortType.Output, (NodeModel)this, new PortData("Value", Resources.ParseUnitInputValueTooltip)));
            this.OutPorts.Add(new PortModel(PortType.Output, (NodeModel)this, new PortData("Unit", Resources.ParseUnitInputUnitTooltip)));
            RegisterAllPorts();
            try
            {
                Items =
                    DynamoUnits.Utilities.ConvertUnitsDictionaryToList(
                        DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits());
                SelectedUnit = Items.Find(u => u.Name == "Feet");
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            Value = "";
            ShouldDisplayPreviewCore = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if(string.IsNullOrEmpty(Value) || SelectedUnit is null)
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };

            var expression = AstFactory.BuildStringNode(Value);

            var unitID =
                AstFactory.BuildStringNode(SelectedUnit?.TypeId);

           var node = AstFactory.BuildFunctionCall(
                new Func<string, string, double>(DynamoUnits.Utilities.ParseExpressionByUnitId),
                new List<AssociativeNode> { unitID, expression });

           var node2 = AstFactory.BuildFunctionCall(
               new Func<string, DynamoUnits.Unit>(DynamoUnits.Unit.ByTypeID),
               new List<AssociativeNode> { unitID });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node), AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(1), node2) };
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
    [NodeName("Convert By Units")]
    [NodeDescription("ConvertUnitsDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("ConvertUnitsSearchTags", typeof(UnitsUI.Properties.Resources))]
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

        /// <summary>
        /// This corresponds to the list of available quantities we can select to convert in.
        /// Examples of this would be 'length', 'time', 'volume'.
        /// </summary>
        [JsonIgnore]
        public List<DynamoUnits.Quantity> QuantityConversionSource
        {
            get { return quantityConversionSource; }
            private set
            {
                quantityConversionSource = value;
                RaisePropertyChanged(nameof(QuantityConversionSource));
            }
        }

        /// <summary>
        /// This corresponds to the a list of available units we are converting FROM.
        /// Examples of this are 'meters', 'millimeters', 'feet'.
        /// </summary>
        [JsonIgnore]
        public List<DynamoUnits.Unit> SelectedFromConversionSource
        {
            get { return selectedFromConversionSource; }
            set
            {
                selectedFromConversionSource = value;
                RaisePropertyChanged(nameof(SelectedFromConversionSource));
            }
        }

        /// <summary>
        /// This corresponds to the a list of available units we are converting TO.
        /// Examples of this are 'meters', 'millimeters', 'feet'.
        /// </summary>
        [JsonIgnore]
        public List<DynamoUnits.Unit> SelectedToConversionSource
        {
            get { return selectedToConversionSource; }
            set
            {
                selectedToConversionSource = value;
                RaisePropertyChanged(nameof(SelectedToConversionSource));
            }
        }
        /// <summary>
        /// This corresponds to selected quantity we will be converting in.
        /// An example of this is 'length'.
        /// </summary>
        [JsonProperty("MeasurementType"), JsonConverter(typeof(ForgeQuantityConverter))]
        public DynamoUnits.Quantity SelectedQuantityConversion
        {
            get { return selectedQuantityConversion; }
            set
            {
                selectedQuantityConversion = value;
                SelectedFromConversionSource =
                    selectedQuantityConversion?.Units;
                SelectedToConversionSource =
                    selectedQuantityConversion?.Units;
                SelectedFromConversion = SelectedFromConversionSource?.First();
                SelectedToConversion = SelectedToConversionSource?.First();

                RaisePropertyChanged(nameof(SelectedQuantityConversion));
            }
        }
        /// <summary>
        /// This corresponds to the selected unit we are converting FROM.
        /// An example of this would be 'meters'.
        /// </summary>
        [JsonProperty("FromConversion"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedFromConversion
        {
            get { return selectedFromConversion; }
            set
            {
                selectedFromConversion = value;
                this.OnNodeModified();
                RaisePropertyChanged(nameof(SelectedFromConversion));
            }
        }
        /// <summary>
        /// This corresponds to the selected unit we are converting TO.
        /// An example of this would be 'feet'.
        /// </summary>
        [JsonProperty("ToConversion"), JsonConverter(typeof(ForgeUnitConverter))]
        public DynamoUnits.Unit SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                selectedToConversion = value;
                this.OnNodeModified();
                RaisePropertyChanged(nameof(SelectedToConversion));
            }
        }
        /// <summary>
        /// Property alerting the UI whether a selection from the dropdowns is enabled.
        /// </summary>
        [JsonIgnore]
        public bool IsSelectionFromBoxEnabled
        {
            get { return isSelectionFromBoxEnabled; }
            set
            {
                isSelectionFromBoxEnabled = value;
                RaisePropertyChanged(nameof(IsSelectionFromBoxEnabled));
            }
        }

        /// <summary>
        /// Provides the SelectionFromBox with a tooltip describing the conversion.
        /// </summary>
        [JsonIgnore]
        public string SelectionFromBoxToolTip
        {
            get { return selectionFromBoxToolTip; }
            set
            {
                selectionFromBoxToolTip = value;
                RaisePropertyChanged(nameof(SelectionFromBoxToolTip));
            }
        }

        [JsonConstructor]
        private ForgeDynamoConvert(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            try
            {
                QuantityConversionSource =
                    DynamoUnits.Utilities.CovertQuantityDictionaryToList(
                        DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities());
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            AssociativeNode defaultNode = new DoubleNode(0.0);
            if (inPorts != null) inPorts.First().DefaultValue = defaultNode;

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
        }

        public ForgeDynamoConvert()
        {
            try
            {
                QuantityConversionSource =
                    DynamoUnits.Utilities.CovertQuantityDictionaryToList(
                        DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities());
                SelectedQuantityConversion = QuantityConversionSource.Find(q => q.Name == "Length");
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            AssociativeNode defaultNode = new DoubleNode(0.0);
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", Resources.CovertUnitInputTooltip, defaultNode)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Resources.CovertUnitOutputTooltip)));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (null == inputAstNodes || inputAstNodes.Count == 0 || inputAstNodes[0] is ProtoCore.AST.AssociativeAST.NullNode || SelectedToConversion is null || SelectedFromConversion is null)
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            
            var conversionToNode =
                AstFactory.BuildStringNode(SelectedToConversion?.TypeId);

            var conversionFromNode =
                AstFactory.BuildStringNode(SelectedFromConversion?.TypeId);
            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(
                        new Func<double, string, string, double>(DynamoUnits.Utilities.ConvertByUnitIds),
                        new List<AssociativeNode> { inputAstNodes[0], conversionFromNode, conversionToNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
        /// <summary>
        /// This methods flips the 'to' and 'from' unit of the conversion.
        /// </summary>
        public void ToggleDropdownValues()
        {
            (this.SelectedFromConversion, this.SelectedToConversion) =
                (this.SelectedToConversion, this.SelectedFromConversion);
        }
    }

    [NodeName("Units")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("UnitsUIDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("UnitsUISearchTags", typeof(UnitsUI.Properties.Resources))]
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

            Dictionary<string, ForgeUnitsCLR.Unit> dictionary = null;
            try
            {
                dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllUnits();
            }
            catch
            {
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (dictionary == null)
            {
                return SelectionState.Restore;
            }

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
    [NodeDescription("QuantitiesUIDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("QuantitiesUISearchTags", typeof(UnitsUI.Properties.Resources))]
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

            Dictionary<string, ForgeUnitsCLR.Quantity> dictionary = null;
            try
            {
                dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllQuantities();
            }
            catch
            {
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (dictionary == null)
            {
                return SelectionState.Restore;
            }

            var quantities = dictionary.Values.ToArray();
            for (int i = 0; i < quantities.Length - 1; i++)
            {
                if (DynamoUnits.Utilities.TypeIdShortName(quantities[i].getTypeId()).Equals(DynamoUnits.Utilities.TypeIdShortName(quantities[i + 1].getTypeId())))
                    continue;

                Items.Add(new DynamoDropDownItem(quantities[i].getName(), quantities[i].getTypeId()));
            }

            Items.Add(new DynamoDropDownItem(quantities.Last().getName(), quantities.Last().getTypeId()));

            return SelectionState.Restore;
        }
    }

    [NodeName("Symbols")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("SymbolsUIDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("SymbolsUISearchTags", typeof(UnitsUI.Properties.Resources))]
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

                var func = new Func<string, DynamoUnits.Symbol>(DynamoUnits.Symbol.ByTypeID);
                node = AstFactory.BuildFunctionCall(func, args);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override SelectionState PopulateItemsCore(string currentSelection)
        {
            Items.Clear();

            Dictionary<string, ForgeUnitsCLR.Symbol> dictionary = null;
            try
            {
                dictionary = DynamoUnits.Utilities.ForgeUnitsEngine.getAllSymbols();
            }
            catch
            {
                Warning(Properties.Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (dictionary == null)
            {
                return SelectionState.Restore;
            }

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
            var symbolTxt = symbol.getPrefixOrSuffix() != null ? Encoding.UTF8.GetString(Encoding.Default.GetBytes(symbol.getPrefixOrSuffix().getText())) : "";

            return symbolStr + symbolTxt;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using CoreNodeModels;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using Utilities = DynamoUnits.Utilities;
using UnitsUI.Converters;
using UnitsUI.Properties;
using DSCore;
using Dynamo.Utilities;

namespace UnitsUI
{
    /// <summary>
    /// Class defining the behaviour of a nodemodel which has no inputs, but rather uses wpf comboboxes to select items from collections.
    /// It then outputs a formatted string from the selections made.
    /// </summary>
    [Obsolete("This abstract class will be removed in Dynamo 3.0 - This abstract class will be removed in Dynamo 3.0 - please use the UnitInput node")]
    public abstract class MeasurementInputBase : NodeModel
    {
        [JsonIgnore]
        public SIUnit Measure { get; protected set; }
        /// <summary>
        /// Numerical value entered into the textbox (to be formatted).
        /// </summary>
        public double Value
        {
            get
            {
                return Measure.Value;
            }
            set
            {
                Measure.Value = value;
                RaisePropertyChanged(nameof(Value));
            }
        }

        public MeasurementInputBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        public MeasurementInputBase() : base() { }

        internal void ForceValueRaisePropertyChanged()
        {
            RaisePropertyChanged(nameof(Value));
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            XmlElement outEl = nodeElement.OwnerDocument.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                // this node now stores a double, having previously stored a measure type
                // by checking for the measure type as well we allow for loading of older files.
                if (subNode.Name.Equals(typeof(double).FullName) || subNode.Name.Equals("Dynamo.Measure.Foot"))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        protected double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == nameof(Value))
            {
                var converter = new InternalMeasureConverter();
                this.Value = ((double)converter.ConvertBack(value, typeof(double), Measure, null));
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }
        //stick this here until we get rid of all these obsolete types
        //since the actual MeasureConverter depends on WPF.
        private class InternalMeasureConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return parameter.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var measure = (SIUnit)parameter;
                measure.SetValueFromString(value.ToString());
                return measure.Value;
            }
        }



    }

    [NodeName("Number From Feet and Inches")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("LengthFromStringDescription", typeof(Resources))]
    [NodeSearchTags("LengthFromStringSearchTags", typeof(Resources))]
    [OutPortTypes("number")]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class LengthFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private LengthFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);
            Warning("Number From Feet and Inches " + Resources.LegthFromStringObsoleteMessage, true);
        }

        public LengthFromString() : base()
        {
            Measure = Length.FromDouble(0.0, LengthUnit.FractionalFoot);

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("double", Resources.LengthFromStringPortDataLengthToolTip)));
            RegisterAllPorts();
            Warning("Number From Feet and Inches " + Resources.LegthFromStringObsoleteMessage, true);
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
    [NodeDescription("AreaFromStringDescription", typeof(Resources))]
    [NodeSearchTags("AreaFromStringSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class AreaFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private AreaFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
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
    [NodeDescription("VolumeFromStringDescription", typeof(Resources))]
    [NodeSearchTags("VolumeFromStringSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class VolumeFromString : MeasurementInputBase
    {
        [JsonConstructor]
        private VolumeFromString(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
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
    [NodeDescription("UnitTypesDescription", typeof(Resources))]
    [NodeSearchTags("UnitTypesSearchTags", typeof(Resources))]
    [OutPortTypes("type")]
    [IsDesignScriptCompatible]
    [NodeDeprecated]
    public class UnitTypes : AllChildrenOfType<SIUnit>
    {
        public UnitTypes() : base()
        {
            Warning("Unit Types " + Resources.UnitTypesObsoleteMessage, true);
        }

        [JsonConstructor]
        private UnitTypes(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            Warning("Unit Types " + Resources.UnitTypesObsoleteMessage, true);
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
    [NodeDescription("ParseUnitInputDescription", typeof(Resources))]
    [NodeSearchTags("ParseUnitInputSearchTags", typeof(Resources))]
    [IsDesignScriptCompatible]
    public class UnitInput : CoreNodeModels.Input.String
    {
        private IEnumerable<DynamoUnits.Unit> items;
        private const string defaultUnit = "Feet";

        /// <summary>
        /// Collection of unit type ids.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DynamoUnits.Unit> Items
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
                Items = DynamoUnits.Utilities.GetAllUnits();
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
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
                Items = DynamoUnits.Utilities.GetAllUnits();
                SelectedUnit = Items.ToList().Find(u => u.Name == defaultUnit);
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            Value = "";
            ShouldDisplayPreviewCore = true;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (string.IsNullOrEmpty(Value) || SelectedUnit is null)
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
    [NodeDescription("ConvertUnitsDescription", typeof(Resources))]
    [NodeSearchTags("ConvertUnitsSearchTags", typeof(Resources))]
    [OutPortTypes("number")]
    [IsDesignScriptCompatible]
    public class DynamoUnitConvert : NodeModel
    {
        private DynamoUnits.Unit selectedFromConversion;
        private DynamoUnits.Unit selectedToConversion;
        private DynamoUnits.Quantity selectedQuantityConversion;
        private IEnumerable<DynamoUnits.Quantity> quantityConversionSource;
        private IEnumerable<DynamoUnits.Unit> selectedFromConversionSource;
        private IEnumerable<DynamoUnits.Unit> selectedToConversionSource;
        private const string defaultSelectedQuantity = "Length";

        /// <summary>
        /// This corresponds to the list of available quantities we can select to convert in.
        /// Examples of this would be 'length', 'time', 'volume'.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<DynamoUnits.Quantity> QuantityConversionSource
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
        public IEnumerable<DynamoUnits.Unit> SelectedFromConversionSource
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
        public IEnumerable<DynamoUnits.Unit> SelectedToConversionSource
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

        [JsonConstructor]
        private DynamoUnitConvert(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            try
            {
                QuantityConversionSource = DynamoUnits.Utilities.GetAllQuantities();
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            AssociativeNode defaultNode = new DoubleNode(0.0);
            if (inPorts != null) inPorts.First().DefaultValue = defaultNode;

            ShouldDisplayPreviewCore = true;
        }

        public DynamoUnitConvert()
        {
            try
            {
                QuantityConversionSource = DynamoUnits.Utilities.GetAllQuantities();
                SelectedQuantityConversion = QuantityConversionSource.ToList().Find(q => q.Name == defaultSelectedQuantity);
            }
            catch
            {
                //continue with empty node and warning 
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            AssociativeNode defaultNode = new DoubleNode(0.0);
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", Resources.CovertUnitInputTooltip, defaultNode)));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", Resources.CovertUnitOutputTooltip)));

            ShouldDisplayPreviewCore = true;
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
        public void SwitchUnitsDropdownValues()
        {
            (this.SelectedFromConversion, this.SelectedToConversion) =
                (this.SelectedToConversion, this.SelectedFromConversion);
        }
    }

    [NodeName("Units")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("UnitsUIDescription", typeof(Resources))]
    [NodeSearchTags("UnitsUISearchTags", typeof(Resources))]
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

            IEnumerable<Unit> units = null;
            try
            {
                units = DynamoUnits.Utilities.GetAllUnits();
            }
            catch
            {
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (units == null)
            {
                return SelectionState.Restore;
            }

            foreach (var unit in units)
            {
                Items.Add(new DynamoDropDownItem(unit.Name, unit.TypeId));
            }

            return SelectionState.Restore;
        }
    }

    [NodeName("Quantities")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("QuantitiesUIDescription", typeof(Resources))]
    [NodeSearchTags("QuantitiesUISearchTags", typeof(Resources))]
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

            IEnumerable<Quantity> quantities = null;
            try
            {
                quantities = DynamoUnits.Utilities.GetAllQuantities();
            }
            catch
            {
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (quantities == null)
            {
                return SelectionState.Restore;
            }

            foreach (var quantity in quantities)
            {

                Items.Add(new DynamoDropDownItem(quantity.Name, quantity.TypeId));
            }

            return SelectionState.Restore;
        }
    }

    [NodeName("Symbols")]
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeDescription("SymbolsUIDescription", typeof(Resources))]
    [NodeSearchTags("SymbolsUISearchTags", typeof(Resources))]
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

            IEnumerable<Symbol> symbols = null;
            try
            {
                symbols = DynamoUnits.Utilities.GetAllSymbols();
            }
            catch
            {
                Warning(Resources.SchemaLoadWarning + Utilities.SchemaDirectory, true);
            }

            if (symbols == null)
            {
                return SelectionState.Restore;
            }

            foreach (var symbol in symbols)
            {

                Items.Add(new DynamoDropDownItem(SymbolDisplayText(symbol), symbol.TypeId));
            }

            return SelectionState.Restore;
        }

        private string SymbolDisplayText(Symbol symbol)
        {
            return symbol.Unit.Name + ": " + symbol.Text; ;
        }
    }
}

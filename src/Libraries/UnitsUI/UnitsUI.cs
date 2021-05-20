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

namespace UnitsUI
{
    public abstract class MeasurementInputBaseNodeViewCustomization : INodeViewCustomization<MeasurementInputBase>
    {
        private MeasurementInputBase mesBaseModel;
        private DynamoViewModel dynamoViewModel;
        private DynamoTextBox tb;

        public void CustomizeView(MeasurementInputBase model, NodeView nodeView)
        {
            this.mesBaseModel = model;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            //add an edit window option to the 
            //main context window
            var editWindowItem = new MenuItem()
            {
                Header = Properties.Resources.EditHeader,
                IsCheckable = false,
                Tag = nodeView.ViewModel.DynamoViewModel
            };

            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            //add a text box to the input grid of the control
            this.tb = new DynamoTextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.VerticalAlignment = VerticalAlignment.Center;
            nodeView.inputGrid.Children.Add(tb);
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));

            tb.DataContext = model;
            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new MeasureConverter(),
                ConverterParameter = model.Measure,
                NotifyOnValidationError = false,
                Source = model,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            tb.OnChangeCommitted += TextChangehandler; 

            (nodeView.ViewModel.DynamoViewModel.Model.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;
        }

        private void TextChangehandler()
        {
            mesBaseModel.OnNodeModified();
        }

        void PreferenceSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AreaUnit" ||
                e.PropertyName == "VolumeUnit" ||
                e.PropertyName == "LengthUnit" ||
                e.PropertyName == "NumberFormat")
            {
                this.mesBaseModel.ForceValueRaisePropertyChanged();

                this.mesBaseModel.OnNodeModified();
            }
        }

        private void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.dynamoViewModel;
            var editWindow = new EditWindow(viewModel) { DataContext = this.mesBaseModel };
            editWindow.BindToProperty(null, new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new MeasureConverter(),
                ConverterParameter = this.mesBaseModel.Measure,
                NotifyOnValidationError = false,
                Source = this.mesBaseModel,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            editWindow.ShowDialog();
        }

        public void Dispose()
        {
            tb.OnChangeCommitted -= TextChangehandler;
        }
    }

    public abstract class MeasurementInputBase : NodeModel
    {
        [JsonIgnore]
        public SIUnit Measure { get; protected set; }

        public double Value
        {
            get
            {
                return Measure.Value;
            }
            set
            {
                Measure.Value = value;
                RaisePropertyChanged("Value");
            }
        }

        public MeasurementInputBase(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):base(inPorts, outPorts) { }

        public MeasurementInputBase() : base() { }

        internal void ForceValueRaisePropertyChanged()
        {
            RaisePropertyChanged("Value");
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

            if (name == "Value")
            {
                var converter = new MeasureConverter();
                this.Value = ((double)converter.ConvertBack(value, typeof(double), Measure, null));
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }

    }

    public class LengthFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                                         INodeViewCustomization<LengthFromString>
    {
        public void CustomizeView(LengthFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
        }
    }

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

    public class AreaFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                                     INodeViewCustomization<AreaFromString>
    {
        public void CustomizeView(AreaFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
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

    public class VolumeFromStringNodeViewCustomization : MeasurementInputBaseNodeViewCustomization,
                                                 INodeViewCustomization<VolumeFromString>
    {
        public void CustomizeView(VolumeFromString model, NodeView nodeView)
        {
            base.CustomizeView(model, nodeView);
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

    public class StringInputNodeViewCustomization : INodeViewCustomization<UnitInput>
    {
        private DynamoViewModel dynamoViewModel;
        private UnitInput nodeModel;
        private MenuItem editWindowItem;

        public void CustomizeView(UnitInput stringInput, NodeView nodeView)
        {
            this.nodeModel = stringInput;
            this.dynamoViewModel = nodeView.ViewModel.DynamoViewModel;

            this.editWindowItem = new MenuItem
            {
                Header = Dynamo.Wpf.Properties.Resources.StringInputNodeEditMenu,
                IsCheckable = false
            };
            nodeView.MainContextMenu.Items.Add(editWindowItem);

            editWindowItem.Click += editWindowItem_Click;

            var grid = new Grid()
            {
                Height = Double.NaN,
                Width = Double.NaN
            };

            RowDefinition rowDef1 = new RowDefinition();
            RowDefinition rowDef2 = new RowDefinition();

            grid.RowDefinitions.Add(rowDef1);
            grid.RowDefinitions.Add(rowDef2);
            
            
            //add a text box to the input grid of the control
            var tb = new StringTextBox
            {
                TextWrapping = TextWrapping.Wrap,
                MinHeight = Configurations.PortHeightInPixels,
                MaxWidth = 200,
                VerticalAlignment = VerticalAlignment.Stretch
                
            };
            tb.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            
            grid.Children.Add(tb);
            
            Grid.SetColumn(tb, 0);
            Grid.SetRow(tb, 0);

            tb.DataContext = stringInput;
            tb.BindToProperty(new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
                Converter = new StringDisplay(),
                Source = stringInput,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            });

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = System.Double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(combo);
            Grid.SetColumn(combo, 0);
            Grid.SetRow(combo, 1);

            //combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    nodeModel.OnNodeModified();
            };

            combo.DataContext = nodeModel;

            // bind this combo box to the selected item hash
            var bindingVal = new System.Windows.Data.Binding("Items")
            {
                Source = nodeModel
            };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            // bind the selected index to the model property SelectedIndex
            var indexBinding = new Binding("SelectedUnit")
            {
                Mode = BindingMode.TwoWay,
                Source = nodeModel
            };
            combo.SetBinding(Selector.SelectedItemProperty, indexBinding);

            nodeView.inputGrid.Children.Add(grid);
        }

        public void editWindowItem_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow(this.dynamoViewModel) { DataContext = this.nodeModel };
            editWindow.BindToProperty(
                null,
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    Converter = new StringDisplay(),
                    NotifyOnValidationError = false,
                    Source = this.nodeModel,
                    UpdateSourceTrigger = UpdateSourceTrigger.Explicit
                });

            editWindow.ShowDialog();
        }

        public void Dispose()
        {
            editWindowItem.Click -= editWindowItem_Click;
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

    class ConverterNodeViewCustomization : INodeViewCustomization<ForgeDynamoConvert>
    {
        private NodeModel nodeModel;
        private ForgeDynamoConverterControl converterControl;
        private NodeViewModel nodeViewModel;
        private ForgeDynamoConvert convertModel;
        private ForgeConverterViewModel converterViewModel;

        public void CustomizeView(ForgeDynamoConvert model, NodeView nodeView)
        {
            nodeModel = nodeView.ViewModel.NodeModel;
            nodeViewModel = nodeView.ViewModel;
            convertModel = model;
            converterControl = new ForgeDynamoConverterControl(model, nodeView)
            {
                DataContext = new ForgeConverterViewModel(model, nodeView),
            };
            converterViewModel = converterControl.DataContext as ForgeConverterViewModel;
            nodeView.inputGrid.Children.Add(converterControl);
            converterControl.Loaded += converterControl_Loaded;
            converterControl.SelectConversionQuantity.PreviewMouseUp += SelectConversionQuantity_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp += SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp += SelectConversionTo_MouseLeftButtonDown;
        }

        private void SelectConversionQuantity_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void SelectConversionFrom_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void SelectConversionTo_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            nodeViewModel.WorkspaceViewModel.HasUnsavedChanges = true;
            //var undoRecorder = nodeViewModel.WorkspaceViewModel.Model.UndoRecorder;
            //WorkspaceModel.RecordModelForModification(nodeModel, undoRecorder);
        }

        private void converterControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        public void Dispose()
        {
            converterControl.SelectConversionQuantity.PreviewMouseUp -= SelectConversionQuantity_PreviewMouseUp;
            converterControl.SelectConversionFrom.PreviewMouseUp -= SelectConversionFrom_PreviewMouseUp;
            converterControl.SelectConversionTo.PreviewMouseUp -= SelectConversionTo_MouseLeftButtonDown;
        }
    }

    internal class ForgeQuantityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DynamoUnits.Quantity);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            return DynamoUnits.Quantity.ByTypeID(typedId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var quantity = (DynamoUnits.Quantity)value;
            writer.WriteValue(quantity.TypeId);
        }
    }

    internal class ForgeUnitConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DynamoUnits.Unit);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string typedId = System.Convert.ToString(reader.Value, CultureInfo.InvariantCulture);
            return DynamoUnits.Unit.ByTypeID(typedId);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var unit = (DynamoUnits.Unit)value;
            writer.WriteValue(unit.TypeId);
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
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

using DSCore;

using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Migration;
using Dynamo.Nodes;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using UnitsUI.Properties;
using Newtonsoft.Json;

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

            tb.OnChangeCommitted += () => model.OnNodeModified();

            (nodeView.ViewModel.DynamoViewModel.Model.PreferenceSettings).PropertyChanged += PreferenceSettings_PropertyChanged;
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
            tb.OnChangeCommitted += () => mesBaseModel.OnNodeModified();
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

            OutPorts.Add(new PortModel(PortType.Output, this, new PortData(Resources.LengthFromStringPortDataLengthToolTip, Resources.LengthFromStringPortDataLengthToolTip)));
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
            Warning("AreaFromString is obsolete.", true);
        }

        public VolumeFromString()
        {
            Measure = Volume.FromDouble(0.0, VolumeUnit.CubicMeter);
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("volume", Resources.VolumeFromStringPortDataVolumeToolTip)));
            RegisterAllPorts();

            Warning("AreaFromString is obsolete.", true);
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
}

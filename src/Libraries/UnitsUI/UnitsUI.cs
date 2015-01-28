﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml;

using DSCore;

using DSCoreNodesUI;

using Dynamo;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using Dynamo.UI.Prompts;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using UnitsUI.Properties;
using ProtoCore.Namespace;

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
                Header = "Edit...",
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

    [NodeName("Length From String")]
    [NodeCategory("Units.Length.Create")]
    [NodeDescription("LengthFromStringDescription",typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("LengthFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class LengthFromString : MeasurementInputBase
    {
        public LengthFromString()
        {
            Measure = Length.FromDouble(0.0);
            OutPortData.Add(new PortData("length", Resources.LengthFromStringPortDataLengthToolTip));
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.2")]
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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Value);
            var functionCall = AstFactory.BuildFunctionCall(new Func<double,Length>(Length.FromDouble), new List<AssociativeNode> { doubleNode });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
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
    [NodeDescription("AreaFromStringDescription",typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("AreaFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class AreaFromString : MeasurementInputBase
    {
        public AreaFromString()
        {
            Measure = Area.FromDouble(0.0);
            OutPortData.Add(new PortData("area", Resources.AreaFromStringPortDataAreaToolTip));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Value);
            var functionCall = AstFactory.BuildFunctionCall(new Func<double,Area>(Area.FromDouble), new List<AssociativeNode> { doubleNode });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
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
    [NodeDescription("VolumeFromStringDescription",typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("VolumeFromStringSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class VolumeFromString : MeasurementInputBase
    {
        public VolumeFromString()
        {
            Measure = Volume.FromDouble(0.0);
            OutPortData.Add(new PortData("volume", Resources.VolumeFromStringPortDataVolumeToolTip));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var doubleNode = AstFactory.BuildDoubleNode(Value);
            var functionCall = AstFactory.BuildFunctionCall(new Func<double, Volume>(Volume.FromDouble), new List<AssociativeNode> { doubleNode });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Unit Types")]
    [NodeCategory("Units")]
    [NodeDescription("UnitTypesDescription", typeof(UnitsUI.Properties.Resources))]
    [NodeSearchTags("UnitTypesSearchTags", typeof(UnitsUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class UnitTypes : AllChildrenOfType<SIUnit>
    {
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var typeName = AstFactory.BuildStringNode(Items[SelectedIndex].Name);
            var assemblyName = AstFactory.BuildStringNode("DynamoUnits");
            var functionCall = AstFactory.BuildFunctionCall(new Func<string, string, object>(Types.FindTypeByNameInAssembly), new List<AssociativeNode>() { typeName, assemblyName });
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }
}

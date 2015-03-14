using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoConversions;
using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;

namespace DSCoreNodesUI
{
    [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
    [NodeName("Convert Between Units")]
    [NodeDescription("ConversionNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("Convert", "Units", "Length", "Area", "Volume")]
    [IsDesignScriptCompatible]
    public class DynamoConvert : NodeModel
    {
        private ConversionUnit selectedFromConversion;
        private ConversionUnit selectedToConversion;
        private ConversionMetricUnit selectedMetricConversion;
        private bool isSelectionFromBoxEnabled;
        private string selectionFromBoxToolTip;
        private List<ConversionUnit> selectedFromConversionSource;
        private List<ConversionUnit> selectedToConversionSource;

        public List<ConversionUnit> SelectedFromConversionSource
        {
            get { return selectedFromConversionSource; }
            set
            {
                selectedFromConversionSource = value;
                RaisePropertyChanged("SelectedFromConversionSource");
            }
        }

        public List<ConversionUnit> SelectedToConversionSource
        {
            get { return selectedToConversionSource; }
            set
            {
                selectedToConversionSource = value;
                RaisePropertyChanged("SelectedToConversionSource");
            }
        }

        public ConversionMetricUnit SelectedMetricConversion
        {
            get { return selectedMetricConversion; }
            set
            {
                selectedMetricConversion = (ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString()); ;               
                SelectedFromConversionSource =
                    Conversions.ConversionMetricLookup[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit),value.ToString())];
                SelectedToConversionSource =
                    Conversions.ConversionMetricLookup[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];
                SelectedFromConversion = Conversions.ConversionDefaults[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];
                SelectedToConversion = Conversions.ConversionDefaults[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];

                 RaisePropertyChanged("SelectedMetricConversion");
            }
        }

        public ConversionUnit SelectedFromConversion
        {
            get { return selectedFromConversion; }
            set
            {
                selectedFromConversion = (ConversionUnit) Enum.Parse(typeof (ConversionUnit), value.ToString());
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        public ConversionUnit SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                selectedToConversion = (ConversionUnit) Enum.Parse(typeof (ConversionUnit), value.ToString());
                RaisePropertyChanged("SelectedToConversion");
            }
        }

        public bool IsSelectionFromBoxEnabled
        {
            get { return isSelectionFromBoxEnabled; }
            set
            {
                isSelectionFromBoxEnabled = value;
                RaisePropertyChanged("IsSelectionFromBoxEnabled");
            }
        }

        public string SelectionFromBoxToolTip
        {
            get { return selectionFromBoxToolTip; }
            set
            {
                selectionFromBoxToolTip = value;
                RaisePropertyChanged("SelectionFromBoxToolTip");
            }
        }
      
        public DynamoConvert()
        {           
            SelectedMetricConversion = ConversionMetricUnit.Length;  
            AssociativeNode defaultNode = new DoubleNode(0.0);
            InPortData.Add(new PortData("", "A numeric value for conversion.", defaultNode));
            OutPortData.Add(new PortData("", "A converted numeric value."));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
            RegisterAllPorts();
        }
      
        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {       
            var conversionToNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[(ConversionUnit) SelectedToConversion]);

            var conversionFromNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[(ConversionUnit) SelectedFromConversion]);
            AssociativeNode node = null;
           
            node = AstFactory.BuildFunctionCall(
                        new Func<double, double, double, double>(Conversions.ConvertUnitTypes),
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

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var helper = new XmlElementHelper(element);
            helper.SetAttribute("conversionMetric", SelectedMetricConversion.ToString());
            helper.SetAttribute("conversionFrom", SelectedFromConversion.ToString());
            helper.SetAttribute("conversionTo", SelectedToConversion.ToString());
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            var helper = new XmlElementHelper(element);
            var metricConversion = helper.ReadString("conversionMetric");
            var selectedMetricConversionFrom = helper.ReadString("conversionFrom");
            var selectedMetricConversionTo = helper.ReadString("conversionTo");

            SelectedMetricConversion = Enum.Parse(typeof(ConversionMetricUnit), metricConversion) is ConversionMetricUnit ?
                                (ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), metricConversion) : ConversionMetricUnit.Length;

            SelectedFromConversion = Enum.Parse(typeof(ConversionUnit), selectedMetricConversionFrom) is ConversionUnit ?
                             (ConversionUnit)Enum.Parse(typeof(ConversionUnit), selectedMetricConversionFrom) : ConversionUnit.Meters;

            SelectedToConversion = Enum.Parse(typeof(ConversionUnit), selectedMetricConversionTo) is ConversionUnit ?
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), selectedMetricConversionTo) : ConversionUnit.Meters;

        }

        #endregion
    }      
}


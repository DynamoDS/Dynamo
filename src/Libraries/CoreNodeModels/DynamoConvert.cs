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
    [NodeCategory(BuiltinNodeCategories.CORE)]
    [NodeName("Convert")]
    [NodeDescription("ConversionNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [IsDesignScriptCompatible]
    public class DynamoConvert : NodeModel
    {
        private object selectedFromConversion;
        private object selectedToConversion;
        private object selectedMetricConversion;
        private bool isSelectionFromBoxEnabled;
        private string selectionFromBoxToolTip;
        private object selectedFromConversionSource;
        private object selectedToConversionSource;

        public object SelectedFromConversionSource
        {
            get { return selectedFromConversionSource; }
            set
            {
                selectedFromConversionSource = value;
                RaisePropertyChanged("SelectedFromConversionSource");
            }
        }
        
        public object SelectedToConversionSource
        {
            get { return selectedToConversionSource; }
            set
            {
                selectedToConversionSource = value;
                RaisePropertyChanged("SelectedToConversionSource");
            }
        }
      
        public virtual object SelectedMetricConversion
        {
            get { return selectedMetricConversion; }
            set
            {
                selectedMetricConversion = value;
                if (value != null)
                {
                    SelectedFromConversionSource =
                        Conversions.ConversionMetricLookup[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit),value.ToString())];
                    SelectedToConversionSource =
                        Conversions.ConversionMetricLookup[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];
                    SelectedFromConversion = Conversions.ConversionDefaults[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];
                    SelectedToConversion = Conversions.ConversionDefaults[(ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), value.ToString())];

                    RaisePropertyChanged("SelectedMetricConversion");
                }      
            }
        }

        public virtual object SelectedFromConversion
        {
            get { return selectedFromConversion; }
            set
            {
                if (value != null)
                {
                    selectedFromConversion = (ConversionUnit) Enum.Parse(typeof (ConversionUnit), value.ToString());
                    RaisePropertyChanged("SelectedFromConversion");
                }
                else
                    selectedFromConversion = null;
            }
        }

        public virtual object SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                if (value != null)
                {
                    selectedToConversion = (ConversionUnit) Enum.Parse(typeof (ConversionUnit), value.ToString());
                    RaisePropertyChanged("SelectedToConversion");                    
                }
                else
                    selectedToConversion = null;
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
            InPortData.Add(new PortData("", "A numeric value for conversion."));
            OutPortData.Add(new PortData("", "A converted numeric value."));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
            RegisterAllPorts();
        }

        public DynamoConvert(string value)
        {
            SelectedMetricConversion = ConversionMetricUnit.Length;
            SelectionFromBoxToolTip = Properties.Resources.SelectFromComboBoxToolTip;
            InPortData.Add(new PortData("", "A numeric value for conversion."));
            OutPortData.Add(new PortData("", "A converted numeric value."));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = false;
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


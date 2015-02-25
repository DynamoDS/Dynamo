using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;
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

        private object selectedFromConversionSource;

        public object SelectedFromConversionSource
        {
            get { return selectedFromConversionSource; }
            set
            {
                selectedFromConversionSource = value;
                RaisePropertyChanged("SelectedFromConversionSource");
            }
        }

        private object selectedToConversionSource;
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
                    selectedFromConversion = (ConversionUnit) Enum.Parse(typeof (ConversionUnit), value.ToString());
                else
                    selectedFromConversion = null;
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        public virtual object SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                if (value != null)
                    selectedToConversion = (ConversionUnit)Enum.Parse(typeof(ConversionUnit), value.ToString());
                else
                    selectedToConversion = null;
                RaisePropertyChanged("SelectedToConversion");
            }
        }

      
        public DynamoConvert()
        {           
            SelectedMetricConversion = ConversionMetricUnit.Length;
           
            InPortData.Add(new PortData("", "A numeric value for conversion."));
            OutPortData.Add(new PortData("", "A converted numeric value."));

            ShouldDisplayPreviewCore = false;

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

            var xmlDocument = element.OwnerDocument;
            var subNode = xmlDocument.CreateElement("Range");
            subNode.SetAttribute("conversionMetric", SelectedMetricConversion.ToString());
            subNode.SetAttribute("conversionFrom", SelectedFromConversion.ToString());
            subNode.SetAttribute("conversionTo", SelectedToConversion.ToString());
            
            element.AppendChild(subNode);
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.

            foreach (XmlNode subNode in element.ChildNodes)
            {            
                if (subNode.Attributes == null || (subNode.Attributes.Count <= 0))
                    continue;

                foreach (XmlAttribute attr in subNode.Attributes)
                {
                    switch (attr.Name)
                    {
                        case "conversionMetric":
                            SelectedMetricConversion = Enum.Parse(typeof(ConversionMetricUnit), attr.Value) is ConversionMetricUnit ?
                                (ConversionMetricUnit)Enum.Parse(typeof(ConversionMetricUnit), attr.Value) : ConversionMetricUnit.Length;
                            break;
                        case "conversionFrom":
                            SelectedFromConversion = Enum.Parse(typeof(ConversionUnit), attr.Value) is ConversionUnit ? 
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), attr.Value) : ConversionUnit.Meters;                               
                            break;
                        case "conversionTo":
                            SelectedToConversion = Enum.Parse(typeof(ConversionUnit), attr.Value) is ConversionUnit ? 
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), attr.Value) : ConversionUnit.Meters;
                            break;                       
                        default:
                            Log(string.Format("{0} attribute could not be deserialized for {1}", attr.Name, GetType()));
                            break;
                    }
                }

                break;
            }
        }

        #endregion
    }      
}


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
    [NodeDescription("A node for converting to and from SI values.")]
    [IsDesignScriptCompatible]
    public class DynamoConvert : NodeModel
    {
        private ConversionUnit selectedFromConversion;
        private ConversionUnit selectedToConversion;
       
        public ConversionUnit SelectedFromConversion
        {
            get { return selectedFromConversion; }
            set
            {
                selectedFromConversion = value;
                RaisePropertyChanged("SelectedFromConversion");
            }
        }

        public ConversionUnit SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                selectedToConversion = value;
                RaisePropertyChanged("SelectedToConversion");
            }
        }

        public DynamoConvert()
        {
            SelectedFromConversion = ConversionUnit.Meters;
            SelectedToConversion = ConversionUnit.Meters;

            InPortData.Add(new PortData("", "A numeric value for conversion."));
            OutPortData.Add(new PortData("", "A converted numeric value."));

            ShouldDisplayPreviewCore = false;

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var conversionToNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[SelectedToConversion]);

            var conversionFromNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[SelectedFromConversion]);
            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(new Func<double, double, double, double>(Conversions.ConvertToSI), new List<AssociativeNode> { inputAstNodes[0], conversionFromNode, conversionToNode });


            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var xmlDocument = element.OwnerDocument;
            var subNode = xmlDocument.CreateElement("Range");
            subNode.SetAttribute("conversionFrom", SelectedFromConversion.ToString(CultureInfo.InvariantCulture));
            subNode.SetAttribute("conversionTo", SelectedToConversion.ToString(CultureInfo.InvariantCulture));
            
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
                        case "conversionFrom":
                            SelectedFromConversion = Enum.Parse(typeof(ConversionUnit), attr.Value) is ConversionUnit ? 
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), attr.Value) : ConversionUnit.Feet;                               
                            break;
                        case "conversionTo":
                            SelectedToConversion = Enum.Parse(typeof(ConversionUnit), attr.Value) is ConversionUnit ? 
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), attr.Value) : ConversionUnit.Feet;
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


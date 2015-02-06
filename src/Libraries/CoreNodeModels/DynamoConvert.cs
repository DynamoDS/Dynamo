using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Dynamo.Models;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DynamoConversionsUI
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
                RaisePropertyChanged("SelectedConversion");
            }
        }

        public ConversionUnit SelectedToConversion
        {
            get { return selectedToConversion; }
            set
            {
                selectedToConversion = value;
                RaisePropertyChanged("SelectedConversion");
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

        public override IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(
            List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
        {
            var conversionToNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[SelectedToConversion]);

            var conversionFromNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[SelectedFromConversion]);
            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(new Func<double, double, double, double>(Conversions.ConvertToSI), new List<AssociativeNode> { inputAstNodes[0], conversionFromNode, conversionToNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }      
}


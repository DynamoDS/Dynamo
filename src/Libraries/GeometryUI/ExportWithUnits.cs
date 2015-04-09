using System;
using System.Collections.Generic;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoConversions;
using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;

namespace GeometryUI
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("ExportToSAT")]
    //[NodeDescription("ConversionNodeDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeSearchTags("Export", "SAT")]
    [IsDesignScriptCompatible]
    public class ExportWithUnits : NodeModel
    {
        private ConversionUnit selectedExportedUnit;
        private bool isSelectionFromBoxEnabled;
        private string selectionFromBoxToolTip;
        private List<ConversionUnit> selectedExportedUnitsSource;

        public List<ConversionUnit> SelectedExportedUnitsSource
        {
            get { return selectedExportedUnitsSource; }
            set
            {
                selectedExportedUnitsSource = value;
                RaisePropertyChanged("SelectedExportedUnitsSource");
            }
        }

        public ConversionUnit SelectedExportedUnit
        {
            get { return selectedExportedUnit; }
            set
            {
                selectedExportedUnit = (ConversionUnit)Enum.Parse(typeof(ConversionUnit), value.ToString());
                this.OnNodeModified();
                RaisePropertyChanged("SelectedExportedUnit");
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

        public ExportWithUnits()
        {
            SelectedExportedUnit = ConversionUnit.Feet;
            SelectedExportedUnitsSource =
                Conversions.ConversionMetricLookup[ConversionMetricUnit.Length];

            AssociativeNode defaultNode = new DoubleNode(0.0);
            InPortData.Add(new PortData("geometry", "A piece of geometry to export.", defaultNode));
            InPortData.Add(new PortData("filePath", "File to export the geometry to.", defaultNode));
            OutPortData.Add(new PortData("string", "The file path of the exported file. Note this may change from the input in it contains non-ASCII characters."));

            ShouldDisplayPreviewCore = true;
            IsSelectionFromBoxEnabled = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var conversionToNode =
                AstFactory.BuildDoubleNode(Conversions.ConversionDictionary[(ConversionUnit)SelectedExportedUnit]);

            AssociativeNode node = null;

            //node = AstFactory.BuildFunctionCall(
            //            new Func<double, double, double, double>(Conversions.ConvertUnitTypes),
            //            new List<AssociativeNode> { inputAstNodes[0], conversionFromNode, conversionToNode });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); // Base implementation must be called.

            var helper = new XmlElementHelper(element);
            helper.SetAttribute("exportedUnit", SelectedExportedUnit.ToString());
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called.
            var helper = new XmlElementHelper(element);
            var exportedUnit = helper.ReadString("exportedUnit");

            SelectedExportedUnit = Enum.Parse(typeof(ConversionUnit), exportedUnit) is ConversionUnit ?
                                (ConversionUnit)Enum.Parse(typeof(ConversionUnit), exportedUnit) : ConversionUnit.Feet;
        }

        #endregion
    }
}


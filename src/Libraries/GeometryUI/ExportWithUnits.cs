using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using DynamoConversions;

using GeometryUI.Properties;

using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;

namespace GeometryUI
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("ExportToSAT")]
    [NodeSearchTags("Export", "SAT")]
    [IsDesignScriptCompatible]
    public class ExportWithUnits : NodeModel
    {
        private ConversionUnit selectedExportedUnit;
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

        public ExportWithUnits()
        {
            SelectedExportedUnit = ConversionUnit.Feet;
            SelectedExportedUnitsSource =
                Conversions.ConversionMetricLookup[ConversionMetricUnit.Length];

            AssociativeNode geometryNode = new ArrayNode();
            AssociativeNode stringNode = new StringNode();
            InPortData.Add(new PortData("geometry", Resources.ExportToSatGeometryInputDescription, geometryNode));
            InPortData.Add(new PortData("filePath", Resources.ExportToSatFilePathDescription, stringNode));
            OutPortData.Add(new PortData("string", Resources.ExportToSatFilePathOutputDescription));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            double unitsMM = Conversions.ConversionDictionary[SelectedExportedUnit]*1000.0;

            var geometryListNode = inputAstNodes[0];
            var filePathNode = inputAstNodes[1];
            var unitsMMNode = AstFactory.BuildDoubleNode(unitsMM);
            
            AssociativeNode node = null;

            node = AstFactory.BuildFunctionCall(
                        new Func<IEnumerable<Geometry>, string, double, string>(Geometry.ExportToSAT),
                        new List<AssociativeNode> { geometryListNode, filePathNode, unitsMMNode });

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


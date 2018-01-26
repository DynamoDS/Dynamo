using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;

using Dynamo.Models;
using Dynamo.Utilities;
using DynamoConversions;

using GeometryUI.Properties;

using ProtoCore.AST.AssociativeAST;
using System.Xml;
using System.Globalization;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Newtonsoft.Json;

namespace GeometryUI
{
    [NodeCategory(BuiltinNodeCategories.GEOMETRY)]
    [NodeName("ExportToSAT")]
    [NodeDescription("ExportToSATDescripiton", typeof(GeometryUI.Properties.Resources))]
    [NodeSearchTags("ExportWithUnitsSearchTags", typeof(GeometryUI.Properties.Resources))]
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

        [JsonConstructor]
        private ExportWithUnits(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts):
            base(inPorts, outPorts)
        {
            SelectedExportedUnit = ConversionUnit.Feet;
            SelectedExportedUnitsSource =
                Conversions.ConversionMetricLookup[ConversionMetricUnit.Length];
            ShouldDisplayPreviewCore = true;
        }

        public ExportWithUnits()
        {
            SelectedExportedUnit = ConversionUnit.Feet;
            SelectedExportedUnitsSource =
                Conversions.ConversionMetricLookup[ConversionMetricUnit.Length];

            InPorts.Add(new PortModel(PortType.Input, this, new PortData("geometry", Resources.ExportToSatGeometryInputDescription)));
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("filePath", Resources.ExportToSatFilePathDescription, new StringNode())));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("string", Resources.ExportToSatFilePathOutputDescription)));

            ShouldDisplayPreviewCore = true;
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (!InPorts[0].IsConnected || !InPorts[1].IsConnected)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

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


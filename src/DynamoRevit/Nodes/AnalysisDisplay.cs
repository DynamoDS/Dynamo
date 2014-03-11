using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using RevitServices.Persistence;
using System.Xml;

namespace Dynamo.Nodes
{
     
    internal abstract class DisplayAnalysisBase : RevitTransactionNodeWithOneOutput
    {
        protected Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager { get; set; }

        private List<int> _pastResultsIds = new List<int>();
        public List<int> PastResultIds { get { return _pastResultsIds; } }

        // TODO: this should be overriding OnEvaluate, but OnEvaluate is used differently throughout the codebase
        // too late to test this for 0.6.3
        protected void ClearPreviousResults()  
        {
            // remove the display results registered for this node
            if (this.SpatialFieldManager != null)
            {
                PastResultIds.ForEach(SpatialFieldManager.RemoveSpatialFieldPrimitive);
                PastResultIds.Clear();
            }
        }
    }

    [NodeName("Analysis Display Manager")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Gets or creates the SpatialFieldManager on the active view.")]
    class SpatialFieldManager : RevitTransactionNodeWithOneOutput
    {
        //AnalysisDisplayStyle analysisDisplayStyle;

        public SpatialFieldManager()
        {
            InPortData.Add(new PortData("n", "The number of values to be stored at each point of the analysis display", typeof(Value.Number)));
            OutPortData.Add(new PortData("manager", "Spatial field manager for the active view", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager;
            var activeView = DocumentManager.Instance.CurrentUIDocument.ActiveView;
            SpatialFieldManager = Autodesk.Revit.DB.Analysis.SpatialFieldManager.GetSpatialFieldManager(activeView);
            
            if (SpatialFieldManager != null)
            {
                dynRevitSettings.SpatialFieldManagerUpdated = SpatialFieldManager;
            }
            else
            {
                SpatialFieldManager = Autodesk.Revit.DB.Analysis.SpatialFieldManager.CreateSpatialFieldManager(activeView, (int)((Value.Number)args[0]).Item);
            }

            return Value.NewContainer(SpatialFieldManager);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 1, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    [NodeName("Analysis Display Style")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Create an analysis display style for displaying analysis results.")]
    class AnalysisResultsDisplayStyleColor : NodeWithOneOutput
    {
        const string DISPLAY_STYLE_NAME = "dynamo_color";

        public AnalysisResultsDisplayStyleColor()
        {
            OutPortData.Add(new PortData("ads", "Colored surface Analysis Display Style", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            AnalysisDisplayStyle ads = CreateDisplayStyle();

            return Value.NewContainer(ads);

        }

        AnalysisDisplayStyle CreateDisplayStyle()
        {
            AnalysisDisplayStyle analysisDisplayStyle = null;

            Document doc = DocumentManager.Instance.CurrentUIDocument.Document;

            // Look for an existing analysis display style with a specific name
            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            ICollection<Element> collection =
                collector1.OfClass(typeof(AnalysisDisplayStyle)).ToElements();
            var displayStyle = from element in collection
                               where element.Name == DISPLAY_STYLE_NAME
                               select element;

            // If display style does not already exist in the document, create it
            if (displayStyle.Count() == 0)
            {
                var coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                coloredSurfaceSettings.ShowGridLines = false;
               
                var colorSettings = new AnalysisDisplayColorSettings();
                var orange = new Color(255, 205, 0);
                var purple = new Color(200, 0, 200);
                colorSettings.MaxColor = orange;
                colorSettings.MinColor = purple;

                var legendSettings = new AnalysisDisplayLegendSettings{
                    NumberOfSteps = 10,
                    Rounding = 0.05,
                    ShowDataDescription = false,
                    ShowLegend = true};
                

                var collector2 = new FilteredElementCollector(doc);
                ICollection<Element> elementCollection = collector2.OfClass(typeof(TextNoteType)).ToElements();

                var textElements = from element in collector2
                                   select element;
                // if LegendText exists, use it for this Display Style
                if (textElements.Any())
                {
                    var textType =
                        textElements.Cast<TextNoteType>().ElementAt<TextNoteType>(0);
                    legendSettings.TextTypeId = textType.Id;
                }
                analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, DISPLAY_STYLE_NAME, coloredSurfaceSettings, colorSettings, legendSettings);
            }
            else
            {
                analysisDisplayStyle =
                    displayStyle.Cast<AnalysisDisplayStyle>().ElementAt<AnalysisDisplayStyle>(0);
            }

            return analysisDisplayStyle;
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 0, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }

    [NodeName("Display Analysis Surface")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at specified UV values on a face.")]
    [AlsoKnownAs("dynAnalysisResults", "AnalysisResults")]
    class SpatialFieldFace : DisplayAnalysisBase 
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results by Face";

        public SpatialFieldFace()
        {
            InPortData.Add(new PortData("vals", "List of values, corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("uvs", "Sample locations (UVs) on the face.", typeof(Value.List)));
            InPortData.Add(new PortData("manager", "A SpatialFieldManager object.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face on which to map the analytical data.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();
            SpatialFieldManager = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            var reference = (args[3] as Value.Container).Item as Reference;

            var document = DocumentManager.Instance.CurrentUIDocument.Document;
            var face = (reference == null) ?
                ((args[3] as Value.Container).Item as Face) :
                document.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

           
            //if we received a face instead of a reference
            //then use the reference from the face
            if (reference == null && face != null)
                reference = face.Reference;

            if (reference == null)
                throw new Exception("Could not resolved a referenced for the face.");

            int idx = SpatialFieldManager.AddSpatialFieldPrimitive(reference);

            //unwrap the sample locations
            IEnumerable<UV> pts = ((Value.List)args[1]).Item.Select(
               x => (UV)((Value.Container)x).Item
            );
            var samplePts = new FieldDomainPointsByUV(pts.ToList<UV>());
            
            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
            );

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = nvals.Select(n => new ValueAtPoint(new List<double> {n})).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!SpatialFieldManager.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = SpatialFieldManager.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = SpatialFieldManager.GetResultSchema(i);
                    if (arsTest.Name == DYNAMO_ANALYSIS_RESULTS_NAME)
                    {
                        schemaIndex = i;
                        break;
                    }
                }
            }
            else
            {
                var ars = new AnalysisResultSchema(DYNAMO_ANALYSIS_RESULTS_NAME, "Resulting analyses from Dynamo.");
                schemaIndex = SpatialFieldManager.RegisterResult(ars);
            }

            SpatialFieldManager.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            PastResultIds.Add(idx);

            return Value.NewContainer(idx);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "FaceAnalysisDisplay.ByViewFacePointsAndValues",
                "FaceAnalysisDisplay.ByViewFacePointsAndValues@var,FaceReference,double[][],double[]");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId oldInPort3 = new PortId(oldNodeId, 3, PortType.INPUT);
            XmlElement connector3 = data.FindFirstConnector(oldInPort3);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);
            PortId newInPort3 = new PortId(dsRevitNodeId, 3, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort3);
            data.ReconnectToPort(connector1, newInPort2);
            data.ReconnectToPort(connector2, newInPort0);
            data.ReconnectToPort(connector3, newInPort1);

            return migratedData;
        }

    }

    [NodeName("Display Analysis Points")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at specified XYZ locations.")]
    class SpatialFieldPoints : DisplayAnalysisBase
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results at Points";

        public SpatialFieldPoints()
        {
            InPortData.Add(new PortData("values", "A list of numeric values. Each sample point may have multiple values as specified on the analysis display manager object.", typeof(Value.List)));
            InPortData.Add(new PortData("points", "Locations of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("manager", "A SpatialFieldManager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();

            //allow the user to pass in a list or a single item.
            IEnumerable<double> nvals;
            if (args[0].IsList)
            {
                //unwrap the values
                nvals = ((Value.List) args[0]).Item.Select(
                    x => (double) ((Value.Number) x).Item
                    );
            }
            else
            {
                //put the one value into the list
                nvals = new List<double>{((Value.Number) args[0]).Item};
            }

            //unwrap the sample locations
            IEnumerable<XYZ> pts = ((Value.List)args[1]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );
            SpatialFieldManager = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            int idx = SpatialFieldManager.AddSpatialFieldPrimitive();

            var samplePts = new FieldDomainPointsByXYZ(pts.ToList<XYZ>());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = nvals.Select(n => new ValueAtPoint(new List<double> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!SpatialFieldManager.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = SpatialFieldManager.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = SpatialFieldManager.GetResultSchema(i);
                    if (arsTest.Name == DYNAMO_ANALYSIS_RESULTS_NAME)
                    {
                        schemaIndex = i;
                        break;
                    }
                }
            }
            else
            {
                var ars = new AnalysisResultSchema(DYNAMO_ANALYSIS_RESULTS_NAME, "Resulting analyses from Dynamo.");
                schemaIndex = SpatialFieldManager.RegisterResult(ars);
            }

            SpatialFieldManager.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            PastResultIds.Add(idx);

            return Value.NewContainer(idx);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "PointAnalysisDisplay.ByViewPointsAndValues",
                "PointAnalysisDisplay.ByViewPointsAndValues@var,Point[],double[]");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort2);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort0);

            return migratedData;
        }
    }

    [NodeName("Display Analysis Vectors")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical vectors specified XYZ locations.")]
    class SpatialFieldVectors : DisplayAnalysisBase
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results Vectors";

        public SpatialFieldVectors()
        {
            InPortData.Add(new PortData("values", "A list of XYZs corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("points", "Locations (XYZs) of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("manager", "A SpatialFieldManager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();

            //unwrap the values
            IEnumerable<XYZ> nvals = ((Value.List)args[0]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );

            //unwrap the sample locations
            IEnumerable<XYZ> pts = ((Value.List)args[1]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );
            SpatialFieldManager = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            int idx = SpatialFieldManager.AddSpatialFieldPrimitive();

            var samplePts = new FieldDomainPointsByXYZ(pts.ToList<XYZ>());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<VectorAtPoint> valList = nvals.Select(n => new VectorAtPoint(new List<XYZ> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!SpatialFieldManager.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = SpatialFieldManager.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = SpatialFieldManager.GetResultSchema(i);
                    if (arsTest.Name == DYNAMO_ANALYSIS_RESULTS_NAME)
                    {
                        schemaIndex = i;
                        break;
                    }
                }
            }
            else
            {
                var ars = new AnalysisResultSchema(DYNAMO_ANALYSIS_RESULTS_NAME, "Resulting analyses from Dynamo.");
                schemaIndex = SpatialFieldManager.RegisterResult(ars);
            }

            SpatialFieldManager.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            PastResultIds.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (PastResultIds.Count > 0)
            {
                foreach (var id in PastResultIds)
                {
                    SpatialFieldManager.RemoveSpatialFieldPrimitive(id);
                }
            }

            PastResultIds.Clear();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            //create the node itself
            XmlElement dsRevitNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(dsRevitNode, "RevitNodes.dll",
                "VectorAnalysisDisplay.ByViewPointsAndVectorValues", 
                "VectorAnalysisDisplay.ByViewPointsAndVectorValues@var,Point[],Vector[]");

            migratedData.AppendNode(dsRevitNode);
            string dsRevitNodeId = MigrationManager.GetGuidFromXmlElement(dsRevitNode);

            //create and reconnect the connecters
            PortId oldInPort0 = new PortId(oldNodeId, 0, PortType.INPUT);
            XmlElement connector0 = data.FindFirstConnector(oldInPort0);

            PortId oldInPort1 = new PortId(oldNodeId, 1, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            PortId oldInPort2 = new PortId(oldNodeId, 2, PortType.INPUT);
            XmlElement connector2 = data.FindFirstConnector(oldInPort2);

            PortId newInPort0 = new PortId(dsRevitNodeId, 0, PortType.INPUT);
            PortId newInPort1 = new PortId(dsRevitNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(dsRevitNodeId, 2, PortType.INPUT);

            data.ReconnectToPort(connector0, newInPort2);
            data.ReconnectToPort(connector1, newInPort1);
            data.ReconnectToPort(connector2, newInPort0);

            return migratedData;
        }
    }

    [NodeName("Display Analysis Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical data along a curve.")]
    class SpatialFieldCurve : DisplayAnalysisBase
    {
        const string DYNAMO_TEMP_CURVES_SCHEMA = "Dynamo Analysis Results by Curve";

        int schemaId = -1;

        public SpatialFieldCurve()
        {
            InPortData.Add(new PortData("vals", "List of analytical values along this curve.", typeof(Value.List)));
            InPortData.Add(new PortData("curve", "The curve on which to map the results.", typeof(Value.Container)));
            InPortData.Add(new PortData("manager", "A SpatialFieldManager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();

            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(q => (double)((Value.Number)q).Item);

            var curve = (Curve)((Value.Container)args[1]).Item;
            SpatialFieldManager = (Autodesk.Revit.DB.Analysis.SpatialFieldManager)((Value.Container)args[2]).Item;

            if (!SpatialFieldManager.IsResultSchemaNameUnique(DYNAMO_TEMP_CURVES_SCHEMA, -1))
            {
                IList<int> arses = SpatialFieldManager.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = SpatialFieldManager.GetResultSchema(i);
                    if (arsTest.Name == DYNAMO_TEMP_CURVES_SCHEMA)
                    {
                        schemaId = i;
                        break;
                    }
                }
            }
            else
            {
                var ars = new AnalysisResultSchema(DYNAMO_TEMP_CURVES_SCHEMA, "Temporary curves from Dynamo.");
                schemaId = SpatialFieldManager.RegisterResult(ars);
            }

            Transform trf = Transform.Identity;

            //http://thebuildingcoder.typepad.com/blog/2012/09/sphere-creation-for-avf-and-filtering.html#3
            var create = DocumentManager.Instance.CurrentUIDocument.Application.Application.Create;

            Transform t = curve.ComputeDerivatives(0, true);

            XYZ x = t.BasisX.Normalize();
            XYZ y = t.BasisX.IsAlmostEqualTo(XYZ.BasisZ) ? 
                t.BasisX.CrossProduct(XYZ.BasisY).Normalize() : 
                t.BasisX.CrossProduct(XYZ.BasisZ).Normalize();
            XYZ z = x.CrossProduct(y);
            var application = DocumentManager.Instance.CurrentUIApplication.Application;
            Autodesk.Revit.DB.Ellipse arc1 = application.Create.NewEllipse(t.Origin, .1, .1, y, z, -Math.PI, 0);
            Autodesk.Revit.DB.Ellipse arc2 = application.Create.NewEllipse(t.Origin, .1, .1, y, z, 0, Math.PI);

            var pathLoop = new Autodesk.Revit.DB.CurveLoop();
            pathLoop.Append(curve);
            var profileLoop = new Autodesk.Revit.DB.CurveLoop();
            profileLoop.Append(arc1);
            profileLoop.Append(arc2);

            double curveDomain = curve.get_EndParameter(1) - curve.get_EndParameter(0);

            int idx = -1;
            var s = GeometryCreationUtilities.CreateSweptGeometry(pathLoop, 0, 0, new List<Autodesk.Revit.DB.CurveLoop>{profileLoop});
            foreach (Face face in s.Faces)
            {
                //divide the V domain by the number of incoming
                BoundingBoxUV domain = face.GetBoundingBox();
                double vSpan = domain.Max.V - domain.Min.V;

                //analysis values
                idx = SpatialFieldManager.AddSpatialFieldPrimitive(face, trf);

                //a list to hold the analysis points
                IList<UV> uvPts = new List<UV>();

                //a list to hold the analysis values
                IList<ValueAtPoint> valList = new List<ValueAtPoint>();

                //int count = nvals.Count();

                //this is creating a lot of sample points, but if we used less
                //sampling points, AVF would draw the two surfaces as if there was a hard
                //edge between them. this provides a better blend.
                int count = 10;
                for (int i = 0; i < count; i ++)
                {
                    //get a UV point on the face
                    //find its XYZ location and project to 
                    //the underlying curve. find the value which corresponds
                    //to the location on the curve
                    var uv = new UV(domain.Min.U, domain.Min.V + vSpan / count*(double) i);
                    var uv1 = new UV(domain.Max.U, domain.Min.V + vSpan / count * (double)i);
                    uvPts.Add(uv);
                    uvPts.Add(uv1);

                    XYZ facePt = face.Evaluate(uv);
                    IntersectionResult ir = curve.Project(facePt);
                    double curveParam = curve.ComputeNormalizedParameter(ir.Parameter);

                    if (curveParam < 0)
                        curveParam = 0;

                    if (curveParam > 1)
                        curveParam = 1;

                    var valueIndex = (int)Math.Floor(curveParam * (double)nvals.Count());
                    if (valueIndex >= nvals.Count())
                        valueIndex = nvals.Count() - 1;
                    
                    //create list of values at this point - currently supporting only one
                    //var doubleList = new List<double> { nvals.ElementAt(i) };
                    var doubleList = new List<double> { nvals.ElementAt(valueIndex) };

                    //add value at point object containing the value list
                    valList.Add(new ValueAtPoint(doubleList));
                    valList.Add(new ValueAtPoint(doubleList));
                }

                var pnts = new FieldDomainPointsByUV(uvPts);
                var vals = new FieldValues(valList);

                SpatialFieldManager.UpdateSpatialFieldPrimitive(
                    idx, pnts, vals, schemaId);

                PastResultIds.Add(idx);
            }

            return Value.NewNumber(idx);
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            XmlElement dummyNode = MigrationManager.CreateDummyNode(oldNode, 3, 1);
            migrationData.AppendNode(dummyNode);

            return migrationData;
        }
    }
}

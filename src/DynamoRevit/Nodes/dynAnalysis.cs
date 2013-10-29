using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;

namespace Dynamo.Nodes
{
     
    internal abstract class DisplayAnalysisBase : RevitTransactionNodeWithOneOutput
    {
        protected Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager { get; set; }

        private List<int> _pastResultsIds = new List<int>();
        public List<int> PastResultIds { get { return _pastResultsIds; } }

        // TODO: this should be overriding OnEvaluate, but OnEvaluate is used differently throughout the codebase
        // too late to test this for 0.6.2
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

    [NodeName("Spatial Field Manager")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Gets or creates the SpatialFieldManager on the active view.")]
    class SpatialFieldManager : RevitTransactionNodeWithOneOutput
    {
        //AnalysisDisplayStyle analysisDisplayStyle;

        public SpatialFieldManager()
        {
            InPortData.Add(new PortData("n", "Number of samples to be stored in the spatial field manager.", typeof(Value.Number)));
            OutPortData.Add(new PortData("sfm", "Spatial field manager for the active view", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            Autodesk.Revit.DB.Analysis.SpatialFieldManager SpatialFieldManager;

            SpatialFieldManager = Autodesk.Revit.DB.Analysis.SpatialFieldManager.GetSpatialFieldManager(dynRevitSettings.Doc.ActiveView);
            
            if (SpatialFieldManager != null)
            {
                dynRevitSettings.SpatialFieldManagerUpdated = SpatialFieldManager;
            }
            else
            {
                SpatialFieldManager = Autodesk.Revit.DB.Analysis.SpatialFieldManager.CreateSpatialFieldManager(dynRevitSettings.Doc.ActiveView, (int) ((Value.Number)args[0]).Item );
            }

            return Value.NewContainer(SpatialFieldManager);
            
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

            Document doc = dynRevitSettings.Doc.Document;

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
            InPortData.Add(new PortData("sfm", "A SpatialFieldManager object.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face on which to map the analytical data.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();
            SpatialFieldManager = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            var reference = (args[3] as Value.Container).Item as Reference;
            var face = (reference == null) ?
                ((args[3] as Value.Container).Item as Face) :
                dynRevitSettings.Doc.Document.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

           
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

    }

    [NodeName("Display Analysis Points")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at specified XYZ locations.")]
    class SpatialFieldPoints : DisplayAnalysisBase
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results at Points";

        public SpatialFieldPoints()
        {
            InPortData.Add(new PortData("vals", "A list of numeric values, corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Locations (XYZs) of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A SpatialFieldManager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            this.ClearPreviousResults();

            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
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

    }

    [NodeName("Display Analysis Vectors")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical vectors specified XYZ locations.")]
    class SpatialFieldVectors : DisplayAnalysisBase
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results Vectors";

        public SpatialFieldVectors()
        {
            InPortData.Add(new PortData("vals", "A list of XYZs corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Locations (XYZs) of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A SpatialFieldManager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
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
            InPortData.Add(new PortData("sfm", "A SpatialFieldManager object.", typeof(Value.Container)));
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

            var create = dynRevitSettings.Doc.Application.Application.Create;

            Transform t = curve.ComputeDerivatives(0, true);

            XYZ x = t.BasisX.Normalize();
            XYZ y = t.BasisX.IsAlmostEqualTo(XYZ.BasisZ) ? 
                t.BasisX.CrossProduct(XYZ.BasisY).Normalize() : 
                t.BasisX.CrossProduct(XYZ.BasisZ).Normalize();
            XYZ z = x.CrossProduct(y);

            Autodesk.Revit.DB.Ellipse arc1 = dynRevitSettings.Revit.Application.Create.NewEllipse(t.Origin, .1, .1, y,z,-Math.PI, 0);
            Autodesk.Revit.DB.Ellipse arc2 = dynRevitSettings.Revit.Application.Create.NewEllipse(t.Origin, .1, .1, y, z, 0, Math.PI);

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

    }

    [NodeName("Ray Bounce")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_SURFACE)]
    [NodeDescription("Conduct a ray trace analysis from an origin and direction, providing the maximum number of bounces.")]
    class RayBounce:RevitTransactionNode
    {
        private Face currFace;

        private PortData intersections = new PortData("intersections", "The collection of intersection points.",
                                                      typeof (Value.List));

        private PortData elements = new PortData("elements", "The elements intersected by the ray.", typeof (Value.List));

        public RayBounce()
        {
            InPortData.Add(new PortData("origin", "The origin of the ray.", typeof(Value.Container)));
            InPortData.Add(new PortData("direction", "The direction of the ray.", typeof(Value.Container)));
            InPortData.Add(new PortData("maxBounces", "The maximum number of bounces allowed.", typeof(Value.Number)));
            InPortData.Add(new PortData("view", "The view in which to conduct the analysis.", typeof(Value.Container)));

            OutPortData.Add(intersections);
            OutPortData.Add(elements);

            RegisterAllPorts();
        }

        public override void  Evaluate(FSharpList<Value> args, Dictionary<PortData,Value> outPuts)
        {
            var origin = (XYZ) ((Value.Container) args[0]).Item;
            var direction = (XYZ)((Value.Container)args[1]).Item;
            var rayLimit = ((Value.Number)args[2]).Item;
            var view = (View3D)((Value.Container) args[3]).Item;

            XYZ startpt = origin;
            int rayCount = 0;

            var bouncePts = FSharpList<Value>.Empty;
            var bounceElements = FSharpList<Value>.Empty;
            bouncePts = FSharpList<Value>.Cons(Value.NewContainer(origin), bouncePts);

            for (int ctr = 1; ctr <= rayLimit; ctr++)
            {
                var referenceIntersector = new ReferenceIntersector(view);
                IList<ReferenceWithContext> references = referenceIntersector.Find(startpt, direction);
                ReferenceWithContext rClosest = null;
                rClosest = FindClosestReference(references);
                if (rClosest == null)
                {
                    break;
                }
                else
                {
                    var reference = rClosest.GetReference();
                    var referenceElement = dynRevitSettings.Doc.Document.GetElement(reference);
                    bounceElements = FSharpList<Value>.Cons(Value.NewContainer(referenceElement), bounceElements);
                    var referenceObject = referenceElement.GetGeometryObjectFromReference(reference);
                    var endpt = reference.GlobalPoint;
                    if (startpt.IsAlmostEqualTo(endpt))
                    {
                        break;
                    }
                    else
                    {
                        rayCount = rayCount + 1;
                        currFace = referenceObject as Face;
                        var endptUV = reference.UVPoint;
                        var FaceNormal = currFace.ComputeDerivatives(endptUV).BasisZ;  // face normal where ray hits
                        FaceNormal = rClosest.GetInstanceTransform().OfVector(FaceNormal); // transformation to get it in terms of document coordinates instead of the parent symbol
                        var directionMirrored = direction - 2 * direction.DotProduct(FaceNormal) * FaceNormal; //http://www.fvastro.org/presentations/ray_tracing.htm
                        direction = directionMirrored; // get ready to shoot the next ray
                        startpt = endpt;

                        bouncePts = FSharpList<Value>.Cons(Value.NewContainer(endpt), bouncePts);
                    }
                }
            }
            bouncePts.Reverse();
            bounceElements.Reverse();

            outPuts[intersections] = Value.NewList(bouncePts);
            outPuts[elements] = Value.NewList(bounceElements);
        }

        /// <summary>
        /// Find the first intersection with a face
        /// </summary>
        /// <param name="references"></param>
        /// <returns></returns>
        public Autodesk.Revit.DB.ReferenceWithContext FindClosestReference(IList<ReferenceWithContext> references)
        {
            ReferenceWithContext rClosest = null;

            double face_prox = System.Double.PositiveInfinity;
            double edge_prox = System.Double.PositiveInfinity;
            foreach (ReferenceWithContext r in references)
            {
                Reference reference = r.GetReference();
                Element referenceElement = dynRevitSettings.Doc.Document.GetElement(reference);
                GeometryObject referenceGeometryObject = referenceElement.GetGeometryObjectFromReference(reference);
                currFace = null;
                currFace = referenceGeometryObject as Face;
                Edge edge = null;
                edge = referenceGeometryObject as Edge;
                if (currFace != null)
                {
                    if ((r.Proximity < face_prox) && (r.Proximity > System.Double.Epsilon))
                    {
                        rClosest = r;
                        face_prox = Math.Abs(r.Proximity);
                    }
                }
                else if (edge != null)
                {
                    if ((r.Proximity < edge_prox) && (r.Proximity > System.Double.Epsilon))
                    {
                        edge_prox = Math.Abs(r.Proximity);
                    }
                }
            }
            if (edge_prox <= face_prox)
            {
                // stop bouncing if there is an edge at least as close as the nearest face - there is no single angle of reflection for a ray striking a line
                //m_outputInfo.Add("there is an edge at least as close as the nearest face - there is no single angle of reflection for a ray striking a line");
                rClosest = null;
            }
            return rClosest;
        }
    }


    //It would be really nice if we could get this to work
    //unfortunately mapping analysis results along a curve requires
    //a curve reference, which we can only get from a model curve
    //which requires a sketch plane. all of this adds additional complication
    //for the user
    /*
    [NodeName("Spatial Field Curve Parameters")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at specified XYZ locations.")]
    [AlsoKnownAs("dynAnalysisResults")]
    class dynSpatialFieldCurveParameters : dynRevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results at Points";

        List<int> Ids = new List<int>();
        private SpatialFieldManager SpatialFieldManager;

        public dynSpatialFieldCurveParameters()
        {
            InPortData.Add(new PortData("vals", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("t", "Parameters along the curve.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A SpatialFieldManager object.", typeof(Value.Container)));
            InPortData.Add(new PortData("curve", "A curve on which to map the analysis results.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
            );

            //unwrap the sample locations
            IEnumerable<double> parameters = ((Value.List)args[1]).Item.Select(
               x => ((Value.Number)x).Item
            );
            SpatialFieldManager = ((Value.Container)args[2]).Item as SpatialFieldManager;
            var curve = (Curve) ((Value.Container) args[3]).Item;

            int idx = SpatialFieldManager.AddSpatialFieldPrimitive(curve.Reference);

            var samplePts = new FieldDomainPointsByParameter(parameters.ToList<double>());

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

            Ids.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (Ids.Count > 0)
            {
                foreach (var id in Ids)
                {
                    SpatialFieldManager.RemoveSpatialFieldPrimitive(id);
                }
            }

            Ids.Clear();
        }
    }
    */

    /*[NodeName("Vector Spatial Field")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE)]
    [NodeDescription("Gets or creates the spatial field manager on the view.")]
    [RequiresTransaction(true)]
    class dynVectorSpatialField : dynNode
    {
        AnalysisDisplayStyle analysisDisplayStyle;

        public dynVectorSpatialField()
        {
            InPortData.Add(new PortData("n", "Number of samples at a location.", typeof(int)));
            OutPortData.Add(new PortData("sfm", "Spatial field manager for the active view", typeof(object));

            NodeUI.RegisterInputsAndOutput();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            SpatialFieldManager SpatialFieldManager;

            if (analysisDisplayStyle == null)
            {
                CreateDisplayStyle();
            }

            SpatialFieldManager = SpatialFieldManager.GetSpatialFieldManager(dynRevitSettings.Doc.ActiveView);
            
            if (SpatialFieldManager != null)
            {
                dynSettings.Instance.SpatialFieldManagerUpdated = SpatialFieldManager;
            }
            else
            {
                SpatialFieldManager = SpatialFieldManager.CreateSpatialFieldManager(dynRevitSettings.Doc.ActiveView, Convert.ToInt16(((Value.Number)args[0]).Item));
            }

<<<<<<< HEAD
            return Value.NewContainer(SpatialFieldManager);

=======
            return Value.NewContainer(SpatialFieldManager);
            
>>>>>>> Now builds and runs!
        }

        void CreateDisplayStyle()
        {
            Document doc = dynRevitSettings.Doc.Document;

            analysisDisplayStyle = null;
            // Look for an existing analysis display style with a specific name
            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            ICollection<Element> collection =
                collector1.OfClass(typeof(AnalysisDisplayStyle)).ToElements();
            var displayStyle = from element in collection
                               where element.Name == "Dynamo_Vector"
                               select element;

            // If display style does not already exist in the document, create it
            if (displayStyle.Count() == 0)
            {
                AnalysisDisplayVectorSettings vectorSettings = new AnalysisDisplayVectorSettings();
                vectorSettings.VectorPosition = AnalysisDisplayStyleVectorPosition.FromDataPoint;
                vectorSettings.VectorOrientation = AnalysisDisplayStyleVectorOrientation.Linear;
                vectorSettings.ArrowheadScale = AnalysisDisplayStyleVectorArrowheadScale.NoScaling;
                vectorSettings.ArrowLineWeight = 5;

                AnalysisDisplayColorSettings colorSettings = new AnalysisDisplayColorSettings();
                Color orange = new Color(255, 205, 0);
                Color purple = new Color(200, 0, 200);
                colorSettings.MaxColor = orange;
                colorSettings.MinColor = purple;

                AnalysisDisplayLegendSettings legendSettings = new AnalysisDisplayLegendSettings();
                legendSettings.NumberOfSteps = 10;
                legendSettings.Rounding = 0.05;
                legendSettings.ShowDataDescription = false;
                legendSettings.ShowLegend = true;

                FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                ICollection<Element> elementCollection = collector2.OfClass(typeof(TextNoteType)).ToElements();

                var textElements = from element in collector2
                                   select element;
                // if LegendText exists, use it for this Display Style
                if (textElements.Count() > 0)
                {
                    TextNoteType textType =
                        textElements.Cast<TextNoteType>().ElementAt<TextNoteType>(0);
                    legendSettings.TextTypeId = textType.Id;
                }
                analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, "Dynamo_Vector", vectorSettings, colorSettings, legendSettings);
            }
            else
            {
                analysisDisplayStyle =
                    displayStyle.Cast<AnalysisDisplayStyle>().ElementAt<AnalysisDisplayStyle>(0);
            }
            // now assign the display style to the view
            doc.ActiveView.AnalysisDisplayStyleId = analysisDisplayStyle.Id;
        }
    }*/
}

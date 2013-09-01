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
     
    [NodeName("Spatial Field Manager")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Gets or creates the spatial field manager on the active view.")]
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
            Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm;

            sfm = Autodesk.Revit.DB.Analysis.SpatialFieldManager.GetSpatialFieldManager(dynRevitSettings.Doc.ActiveView);
            
            if (sfm != null)
            {
                dynRevitSettings.SpatialFieldManagerUpdated = sfm;
            }
            else
            {
                sfm = Autodesk.Revit.DB.Analysis.SpatialFieldManager.CreateSpatialFieldManager(dynRevitSettings.Doc.ActiveView, (int) ((Value.Number)args[0]).Item );
            }

            return Value.NewContainer(sfm);
            
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
            SpatialFieldManager sfm;

            if (analysisDisplayStyle == null)
            {
                CreateDisplayStyle();
            }

            sfm = SpatialFieldManager.GetSpatialFieldManager(dynRevitSettings.Doc.ActiveView);
            
            if (sfm != null)
            {
                dynSettings.Instance.SpatialFieldManagerUpdated = sfm;
            }
            else
            {
                sfm = SpatialFieldManager.CreateSpatialFieldManager(dynRevitSettings.Doc.ActiveView, Convert.ToInt16(((Value.Number)args[0]).Item));
            }

<<<<<<< HEAD
            return Value.NewContainer(sfm);

=======
            return Value.NewContainer(sfm);
            
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

    [NodeName("Spatial Field Face")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at speficied UV values on a face.")]
    [AlsoKnownAs("dynAnalysisResults", "AnalysisResults")]
    class SpatialFieldFace : RevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results by Face";

        List<int> idxs = new List<int>();
        private Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm;

        public SpatialFieldFace()
        {
            InPortData.Add(new PortData("vals", "List of values, corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("uvs", "Sample locations (UVs) on the face.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A Spatial Field Manager object.", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "The face on which to map the analytical data.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            sfm = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

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

            int idx = sfm.AddSpatialFieldPrimitive(reference);

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
            if (!sfm.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = sfm.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = sfm.GetResultSchema(i);
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
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            idxs.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (idxs.Count > 0)
            {
                foreach (var id in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(id);
                }
            }

            idxs.Clear();
        }
    }

    [NodeName("Spatial Field Points")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical results at speficied XYZ locations.")]
    class SpatialFieldPoints : RevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results at Points";

        List<int> idxs = new List<int>();
        private Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm;

        public SpatialFieldPoints()
        {
            InPortData.Add(new PortData("vals", "A list of numeric values, corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Locations (XYZs) of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A Spatial Field Manager object.", typeof(Value.Container)));
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
            IEnumerable<XYZ> pts = ((Value.List)args[1]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );
            sfm = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            int idx = sfm.AddSpatialFieldPrimitive();

            var samplePts = new FieldDomainPointsByXYZ(pts.ToList<XYZ>());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = nvals.Select(n => new ValueAtPoint(new List<double> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!sfm.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = sfm.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = sfm.GetResultSchema(i);
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
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            idxs.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (idxs.Count > 0)
            {
                foreach (var id in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(id);
                }
            }

            idxs.Clear();
        }
    }

    [NodeName("Spatial Field Vectors")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical vectors speficied XYZ locations.")]
    class SpatialFieldVectors : RevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results Vectors";

        List<int> idxs = new List<int>();
        private Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm;

        public SpatialFieldVectors()
        {
            InPortData.Add(new PortData("vals", "A list of XYZs corresponding in length, to the list of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Locations (XYZs) of sample points.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A Spatial Field Manager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Container)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //unwrap the values
            IEnumerable<XYZ> nvals = ((Value.List)args[0]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );

            //unwrap the sample locations
            IEnumerable<XYZ> pts = ((Value.List)args[1]).Item.Select(
               x => (XYZ)((Value.Container)x).Item
            );
            sfm = ((Value.Container)args[2]).Item as Autodesk.Revit.DB.Analysis.SpatialFieldManager;

            int idx = sfm.AddSpatialFieldPrimitive();

            var samplePts = new FieldDomainPointsByXYZ(pts.ToList<XYZ>());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<VectorAtPoint> valList = nvals.Select(n => new VectorAtPoint(new List<XYZ> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!sfm.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = sfm.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = sfm.GetResultSchema(i);
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
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            idxs.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (idxs.Count > 0)
            {
                foreach (var id in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(id);
                }
            }

            idxs.Clear();
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
    [NodeDescription("Visualize analytical results at speficied XYZ locations.")]
    [AlsoKnownAs("dynAnalysisResults")]
    class dynSpatialFieldCurveParameters : dynRevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results at Points";

        List<int> idxs = new List<int>();
        private SpatialFieldManager sfm;

        public dynSpatialFieldCurveParameters()
        {
            InPortData.Add(new PortData("vals", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("t", "Parameters along the curve.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "A Spatial Field Manager object.", typeof(Value.Container)));
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
            sfm = ((Value.Container)args[2]).Item as SpatialFieldManager;
            var curve = (Curve) ((Value.Container) args[3]).Item;

            int idx = sfm.AddSpatialFieldPrimitive(curve.Reference);

            var samplePts = new FieldDomainPointsByParameter(parameters.ToList<double>());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = nvals.Select(n => new ValueAtPoint(new List<double> { n })).ToList();
            var sampleValues = new FieldValues(valList);

            int schemaIndex = 0;
            if (!sfm.IsResultSchemaNameUnique(DYNAMO_ANALYSIS_RESULTS_NAME, -1))
            {
                IList<int> arses = sfm.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = sfm.GetResultSchema(i);
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
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, samplePts, sampleValues, schemaIndex);

            idxs.Add(idx);

            return Value.NewContainer(idx);
        }

        public void ClearReferences()
        {
            if (idxs.Count > 0)
            {
                foreach (var id in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(id);
                }
            }

            idxs.Clear();
        }
    }
    */

    [NodeName("Spatial Field Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Visualize analytical data along a curve.")]
    class SpatialFieldCurve : RevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_TEMP_CURVES_SCHEMA = "Dynamo Analysis Results by Curve";

        List<int> idxs = new List<int>();
        int schemaId = -1;
        private Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm;

        public SpatialFieldCurve()
        {
            InPortData.Add(new PortData("vals", "List of analytical values along this curve.", typeof(Value.List)));
            InPortData.Add(new PortData("curve", "The curve on which to map the results.", typeof(Value.Container)));
            InPortData.Add(new PortData("sfm", "A Spatial Field Manager object.", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "The index of the resulting analysis results object.", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(q => (double)((Value.Number)q).Item);

            var curve = (Curve)((Value.Container)args[1]).Item;
            sfm = (Autodesk.Revit.DB.Analysis.SpatialFieldManager)((Value.Container)args[2]).Item;

            if (!sfm.IsResultSchemaNameUnique(DYNAMO_TEMP_CURVES_SCHEMA, -1))
            {
                IList<int> arses = sfm.GetRegisteredResults();
                foreach (int i in arses)
                {
                    AnalysisResultSchema arsTest = sfm.GetResultSchema(i);
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
                schemaId = sfm.RegisterResult(ars);
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
                idx = sfm.AddSpatialFieldPrimitive(face, trf);

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

                sfm.UpdateSpatialFieldPrimitive(
                    idx, pnts, vals, schemaId);

                idxs.Add(idx);
            }

            return Value.NewNumber(idx);
        }

        public void ClearReferences()
        {
            if (idxs.Count > 0)
            {
                foreach (var id in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(id);
                }
            }

            idxs.Clear();
        }
    }
}

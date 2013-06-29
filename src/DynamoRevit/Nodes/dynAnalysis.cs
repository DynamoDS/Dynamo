using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
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
    [NodeDescription("Gets or creates the spatial field manager on the view.")]
    class dynSpatialFieldManager : dynRevitTransactionNodeWithOneOutput
    {
        //AnalysisDisplayStyle analysisDisplayStyle;

        public dynSpatialFieldManager()
        {
            InPortData.Add(new PortData("n", "Number of samples at a location.", typeof(Value.Number)));
            OutPortData.Add(new PortData("sfm", "Spatial field manager for the active view", typeof(Value.Container)));

            RegisterAllPorts();

        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            SpatialFieldManager sfm;

            sfm = SpatialFieldManager.GetSpatialFieldManager(dynRevitSettings.Doc.ActiveView);
            
            if (sfm != null)
            {
                dynRevitSettings.SpatialFieldManagerUpdated = sfm;
            }
            else
            {
                sfm = SpatialFieldManager.CreateSpatialFieldManager(dynRevitSettings.Doc.ActiveView, Convert.ToInt16(((Value.Number)args[0]).Item));
            }

            return Value.NewContainer(sfm);
            
        }
    }

    [NodeName("Analysis Display Style")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Create an analysis display style for displaying analysis results.")]
    class dynAnalysisResultsDisplayStyleColor : dynNodeWithOneOutput
    {
        const string DISPLAY_STYLE_NAME = "dynamo_color";

        public dynAnalysisResultsDisplayStyleColor()
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
    [NodeDescription("An analysis results object to be used with a spatial field manager.")]
    [AlsoKnownAs("dynAnalysisResults")]
    class dynSpatialFieldFace : dynRevitTransactionNodeWithOneOutput
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results";

        int idx = -1;

        public dynSpatialFieldFace()
        {
            InPortData.Add(new PortData("vals", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("uvs", "Sample locations as a list of UVs.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "face", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var sfm = ((Value.Container)args[2]).Item as SpatialFieldManager;

            //first, cleanup the old one
            if (idx != -1)
            {
                sfm.RemoveSpatialFieldPrimitive(idx);
            }

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

            idx = sfm.AddSpatialFieldPrimitive(reference);

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

            return Value.NewContainer(idx);
        }
    }

    /*
    [NodeName("Spatial Field Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("An analysis results curve to be used with a spatial field manager.")]
    [AlsoKnownAs("dynAnalysisResultsCurve")]
    class dynSpatialFieldCurve : dynNodeWithOneOutput
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results Curve";

        //int idx = -1;
        List<int> idxs = new List<int>();

        public dynSpatialFieldCurve()
        {
            InPortData.Add(new PortData("values", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("t", "Sample parameters along the curve." +
                                             "", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            InPortData.Add(new PortData("curve", "Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Container)));

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
            IEnumerable<double> pts = ((Value.List)args[1]).Item.Select(
               x => ((Value.Number)x).Item
            );

            var sfm = (SpatialFieldManager)((Value.Container)args[2]).Item;
            var curve = (Curve)((Value.Container)args[3]).Item;

            int index = sfm.AddSpatialFieldPrimitive(curve, Transform.Identity);

            IList<double> doubleList = new List<double>();
            doubleList.Add(curve.get_EndParameter(0)); // vectors will be at each end of the analytical model curve
            doubleList.Add(curve.get_EndParameter(1));

            var pointsByParameter = new FieldDomainPointsByParameter(doubleList);

            var xyzList = new List<XYZ> {curve.ComputeDerivatives(0, true).BasisX.Normalize()};
            IList<VectorAtPoint> vectorList = new List<VectorAtPoint>();
            vectorList.Add(new VectorAtPoint(xyzList));
            xyzList.Clear();
            xyzList.Add(curve.ComputeDerivatives(1, true).BasisX.Normalize().Negate());
            vectorList.Add(new VectorAtPoint(xyzList));
            var fieldPoints = new FieldDomainPointsByXYZ(xyzList);
            var fieldValues = new FieldValues(vectorList);
            int n = 0;
            sfm.UpdateSpatialFieldPrimitive(index, fieldPoints, fieldValues, n);

            //------------------------------------------------------
            
            //first, cleanup the old one
            //if (idx != -1)
            //{
            //    sfm.RemoveSpatialFieldPrimitive(idx);
            //}

            //cleanup the old spatial field primitives
            idxs.Where(x => x != -1).ToList().ForEach(sfm.RemoveSpatialFieldPrimitive);
            idxs.Clear();

            //Transform t = Transform.Identity;
            //t.set_Basis(0, (curve.get_EndPoint(1)-curve.get_EndPoint(0)).Normalize());
            //XYZ curveY = t.BasisX.CrossProduct(new XYZ(0, 0, 1));
            //t.set_Basis(1, curveY);
            //t.set_Basis(2, t.BasisX.CrossProduct(t.BasisY));
            //t.Origin = curve.get_EndPoint(0);

            int idx = sfm.AddSpatialFieldPrimitive(curve, Transform.Identity);
            idxs.Add(idx);

            var samplePts = new FieldDomainPointsByParameter(pts.ToList());

            //for every sample location add a list
            //of valueatpoint objects. for now, we only
            //support one value per point
            //IList<ValueAtPoint> valList = nvals.Select(n => new ValueAtPoint(new List<double> {n})).ToList();
            IList<VectorAtPoint> valList = nvals.Select(n => new VectorAtPoint(new List<XYZ> {new XYZ(0,0,1)})).ToList();
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

            return Value.NewContainer(idx);

        }
    }
    */

    [NodeName("Spatial Field Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Draw temporary curves in the family.")]
    class dynSpatialFieldCurve : dynRevitTransactionNodeWithOneOutput, IClearable
    {
        const string DYNAMO_TEMP_CURVES_SCHEMA = "Dynamo Temporary Curves";

        List<int> idxs = new List<int>();
        int schemaId = -1;
        private SpatialFieldManager sfm;

        public dynSpatialFieldCurve()
        {
            InPortData.Add(new PortData("curve", "The curve on which to map the results.", typeof(Value.Container)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Number)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            var curve = (Curve)((Value.Container)args[0]).Item;
            sfm = (SpatialFieldManager)((Value.Container)args[1]).Item;

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

            Ellipse arc1 = dynRevitSettings.Revit.Application.Create.NewEllipse(t.Origin, .25, .25, y,z,-Math.PI, 0);
            Ellipse arc2 = dynRevitSettings.Revit.Application.Create.NewEllipse(t.Origin, .25, .25, y, z, 0, Math.PI);

            var pathLoop = new CurveLoop();
            pathLoop.Append(curve);
            var profileLoop = new CurveLoop();
            profileLoop.Append(arc1);
            profileLoop.Append(arc2);

            int idx = -1;
            var s = GeometryCreationUtilities.CreateSweptGeometry(pathLoop, 0, 0, new List<CurveLoop>{profileLoop});
            foreach (Face face in s.Faces)
            {
                idx = sfm.AddSpatialFieldPrimitive(face, trf);

                //need to use double parameters because
                //we're drawing lines
                IList<UV> uvPts = new List<UV>();
                uvPts.Add(face.GetBoundingBox().Min);

                var pnts = new FieldDomainPointsByUV(uvPts);

                var doubleList = new List<double> {0};

                IList<ValueAtPoint> valList
                    = new List<ValueAtPoint>();

                valList.Add(new ValueAtPoint(doubleList));

                var vals = new FieldValues(valList);

                sfm.UpdateSpatialFieldPrimitive(
                    idx, pnts, vals, schemaId);

                idxs.Add(idx);
            }

            return Value.NewNumber(idx);
        }

        public void ClearReferences()
        {
            //first, cleanup the old one
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

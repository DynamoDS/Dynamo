using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Connectors;
using Dynamo.FSchemeInterop;
using Dynamo.FSchemeInterop.Node;
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
    class dynSpatialFieldManager : dynNodeWithOneOutput
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
    [NodeDescription("Create an analysis display style for displaying results color-mapped on a surface.")]
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
                AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                coloredSurfaceSettings.ShowGridLines = false;
               
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
                analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, DISPLAY_STYLE_NAME, coloredSurfaceSettings, colorSettings, legendSettings);
            }
            else
            {
                analysisDisplayStyle =
                    displayStyle.Cast<AnalysisDisplayStyle>().ElementAt<AnalysisDisplayStyle>(0);
            }
            // now assign the display style to the view
            //doc.ActiveView.AnalysisDisplayStyleId = analysisDisplayStyle.Id;
            
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
    class dynAnalysisResults : dynNodeWithOneOutput
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results";

        int idx = -1;

        public dynAnalysisResults()
        {
            InPortData.Add(new PortData("vals", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Sample locations as a list of UVs.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "Face", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            SpatialFieldManager sfm = ((Value.Container)args[2]).Item as SpatialFieldManager;

            //first, cleanup the old one
            if (idx != -1)
            {
                sfm.RemoveSpatialFieldPrimitive(idx);
            }

            Reference reference = ((Value.Container)args[3]).Item as Reference;
            idx = sfm.AddSpatialFieldPrimitive(reference);

            Face face = dynRevitSettings.Doc.Document.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

            //unwrap the sample locations
            IEnumerable<UV> pts = ((Value.List)args[1]).Item.Select(
               x => (UV)((Value.Container)x).Item
            );
            FieldDomainPointsByUV sample_pts = new FieldDomainPointsByUV(pts.ToList<UV>());
            
            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
            );

            //for every sample location add a list
            //of valueatpoint objets. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = new List<ValueAtPoint>();
            foreach (var n in nvals)
            {
                valList.Add(new ValueAtPoint(new List<double>{n}));
            }
            FieldValues sample_values = new FieldValues(valList);

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
                AnalysisResultSchema ars = new AnalysisResultSchema(DYNAMO_ANALYSIS_RESULTS_NAME, "Resulting analyses from Dynamo.");
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, sample_pts, sample_values, schemaIndex);

            return Value.NewContainer(idx);

        }
    }

    [NodeName("Spatial Field Curve")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("An analysis results curve to be used with a spatial field manager.")]
    class dynAnalysisResultsCurve : dynNodeWithOneOutput
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results Curve";

        int idx = -1;

        public dynAnalysisResultsCurve()
        {
            InPortData.Add(new PortData("vals", "List of values.", typeof(Value.List)));
            InPortData.Add(new PortData("pts", "Sample locations as a list of UVs.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            InPortData.Add(new PortData("face", "Curve", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            SpatialFieldManager sfm = ((Value.Container)args[2]).Item as SpatialFieldManager;


            // Place analysis results in the form of vectors at each of a beam or column's analytical model curve
            Curve curve = ((Value.Container)args[3]).Item as Curve;


            int index = sfm.AddSpatialFieldPrimitive(curve, Transform.Identity);

            IList<double> doubleList = new List<double>();
            doubleList.Add(curve.get_EndParameter(0)); // vectors will be at each end of the analytical model curve
            doubleList.Add(curve.get_EndParameter(1));
            FieldDomainPointsByParameter pointsByParameter = new FieldDomainPointsByParameter(doubleList);

            List<XYZ> xyzList = new List<XYZ>();
            xyzList.Add(curve.ComputeDerivatives(0, true).BasisX.Normalize()); // vectors will be tangent to the curve at its ends
            IList<VectorAtPoint> vectorList = new List<VectorAtPoint>();
            vectorList.Add(new VectorAtPoint(xyzList));
            xyzList.Clear();
            xyzList.Add(curve.ComputeDerivatives(1, true).BasisX.Normalize().Negate());
            vectorList.Add(new VectorAtPoint(xyzList));
            FieldDomainPointsByXYZ feildPoints = new FieldDomainPointsByXYZ(xyzList);
            FieldValues fieldValues = new FieldValues(vectorList);
            int n = 0;
            sfm.UpdateSpatialFieldPrimitive(index, feildPoints, fieldValues, n);

            /*
            //first, cleanup the old one
            if (idx != -1)
            {
                sfm.RemoveSpatialFieldPrimitive(idx);
            }

            Reference reference = ((Value.Container)args[3]).Item as Reference;
            idx = sfm.AddSpatialFieldPrimitive(reference);

            Face face = dynRevitSettings.Doc.Document.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

            //unwrap the sample locations
            IEnumerable<UV> pts = ((Value.List)args[1]).Item.Select(
               x => (UV)((Value.Container)x).Item
            );
            FieldDomainPointsByUV sample_pts = new FieldDomainPointsByUV(pts.ToList<UV>());

            //unwrap the values
            IEnumerable<double> nvals = ((Value.List)args[0]).Item.Select(
               x => (double)((Value.Number)x).Item
            );

            //for every sample location add a list
            //of valueatpoint objets. for now, we only
            //support one value per point
            IList<ValueAtPoint> valList = new List<ValueAtPoint>();
            foreach (var n in nvals)
            {
                valList.Add(new ValueAtPoint(new List<double> { n }));
            }
            FieldValues sample_values = new FieldValues(valList);

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
                AnalysisResultSchema ars = new AnalysisResultSchema(DYNAMO_ANALYSIS_RESULTS_NAME, "Resulting analyses from Dynamo.");
                schemaIndex = sfm.RegisterResult(ars);
            }

            sfm.UpdateSpatialFieldPrimitive(idx, sample_pts, sample_values, schemaIndex);
            */

            return Value.NewContainer(idx);

        }
    }

    [NodeName("Temp Curves")]
    [NodeCategory(BuiltinNodeCategories.ANALYZE_DISPLAY)]
    [NodeDescription("Draw temporary curves in the family.")]
    class dynTemporaryCurves : dynNodeWithOneOutput
    {
        const string DYNAMO_TEMP_CURVES_SCHEMA = "Dynamo Temporary Curves";

        List<int> idxs = new List<int>();
        int schemaId = -1;

        public dynTemporaryCurves()
        {
            InPortData.Add(new PortData("lst", "List of sets of xys that will define line segments.", typeof(Value.List)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Value.Container)));
            OutPortData.Add(new PortData("idx", "Analysis results object index", typeof(Value.Container)));

            RegisterAllPorts();
        }

        public override Value Evaluate(FSharpList<Value> args)
        {
            SpatialFieldManager sfm = ((Value.Container)args[1]).Item as SpatialFieldManager;

            //first, cleanup the old one
            if(idxs.Count > 0)
            {
                foreach (int idx in idxs)
                {
                    sfm.RemoveSpatialFieldPrimitive(idx);
                }
            }

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
                AnalysisResultSchema ars = new AnalysisResultSchema(DYNAMO_TEMP_CURVES_SCHEMA, "Temporary curves from Dynamo.");
                schemaId = sfm.RegisterResult(ars);
            }

            Transform trf = Transform.Identity;

            //get the list of pairs
            FSharpList<Value> listOfPairs = ((Value.List)args[0]).Item;
            foreach (Value expr in listOfPairs)
            {
                FSharpList<Value> pair = ((Value.List)expr).Item;

                XYZ start = (XYZ)((Value.Container)pair[0]).Item;
                XYZ end = (XYZ)((Value.Container)pair[1]).Item;
                XYZ start1 = start + XYZ.BasisZ * .1;
                XYZ end1 = end + XYZ.BasisZ * .1;

                //http://thebuildingcoder.typepad.com/blog/2012/09/sphere-creation-for-avf-and-filtering.html#3

                var create = dynRevitSettings.Doc.Application.Application.Create;

                Line l1 = create.NewLineBound(start, end);
                Line l2 = create.NewLineBound(end, end1);
                Line l3 = create.NewLineBound(end1, start1);
                Line l4 = create.NewLineBound(start1, start);

                List<CurveLoop> loops = new List<CurveLoop>();
                CurveLoop cl = new CurveLoop();
                cl.Append(l1);
                cl.Append(l2);
                cl.Append(l3);
                cl.Append(l4);
                loops.Add(cl);
                Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(loops, (end-start).CrossProduct(start1-start), .01);
                
                foreach (Face face in s.Faces)
                {
                    int idx = sfm.AddSpatialFieldPrimitive(face, trf);

                    //need to use double parameters because
                    //we're drawing lines
                    IList<UV> uvPts = new List<UV>();
                    uvPts.Add(face.GetBoundingBox().Min);

                    FieldDomainPointsByUV pnts
                      = new FieldDomainPointsByUV(uvPts);

                    List<double> doubleList
                      = new List<double>();

                    doubleList.Add(0);

                    IList<ValueAtPoint> valList
                      = new List<ValueAtPoint>();

                    valList.Add(new ValueAtPoint(doubleList));

                    FieldValues vals = new FieldValues(valList);

                    sfm.UpdateSpatialFieldPrimitive(
                      idx, pnts, vals, schemaId);

                    idxs.Add(idx);
                }
            }

            return Value.NewList(Utils.SequenceToFSharpList(idxs.Select(x => Value.NewNumber(x))));

        }
    }
}

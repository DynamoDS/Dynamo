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
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.Elements
{
    
    [ElementName("Spatial Field")]
    [ElementCategory(BuiltinElementCategories.ANALYSIS)]
    [ElementDescription("Gets or creates the spatial field manager on the view.")]
    [RequiresTransaction(true)]
    class dynSpatialFieldManager:dynNode
    {
        AnalysisDisplayStyle analysisDisplayStyle;

        public dynSpatialFieldManager()
        {
            InPortData.Add(new PortData("n", "Number of samples at a location.", typeof(int)));
            OutPortData = new PortData("sfm", "Spatial field manager for the active view", typeof(object));

            base.RegisterInputsAndOutputs();

        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            SpatialFieldManager sfm;

            if (analysisDisplayStyle == null)
            {
                CreateDisplayStyle();
            }

            sfm = SpatialFieldManager.GetSpatialFieldManager(dynElementSettings.SharedInstance.Doc.ActiveView);
            
            if (sfm != null)
            {
                dynElementSettings.SharedInstance.SpatialFieldManagerUpdated = sfm;
            }
            else
            {
                sfm = SpatialFieldManager.CreateSpatialFieldManager(dynElementSettings.SharedInstance.Doc.ActiveView, Convert.ToInt16(((Expression.Number)args[0]).Item));
            }

            return Expression.NewContainer(sfm);
            
        }

        public void CreateDisplayStyle()
        {
            Document doc = dynElementSettings.SharedInstance.Doc.Document;

            AnalysisDisplayStyle analysisDisplayStyle = null;
            // Look for an existing analysis display style with a specific name
            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
            ICollection<Element> collection =
                collector1.OfClass(typeof(AnalysisDisplayStyle)).ToElements();
            var displayStyle = from element in collection
                               where element.Name == "Dynamo"
                               select element;

            // If display style does not already exist in the document, create it
            if (displayStyle.Count() == 0)
            {
                AnalysisDisplayColoredSurfaceSettings coloredSurfaceSettings = new AnalysisDisplayColoredSurfaceSettings();
                coloredSurfaceSettings.ShowGridLines = true;
                
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
                //var textElements = from element in collector2
                //                   where element.Name == "LegendText"
                //                   select element;
                var textElements = from element in collector2
                                   select element;
                // if LegendText exists, use it for this Display Style
                if (textElements.Count() > 0)
                {
                    TextNoteType textType =
                        textElements.Cast<TextNoteType>().ElementAt<TextNoteType>(0);
                    legendSettings.TextTypeId = textType.Id;
                }
                analysisDisplayStyle = AnalysisDisplayStyle.CreateAnalysisDisplayStyle(doc, "Dynamo", coloredSurfaceSettings, colorSettings, legendSettings);
            }
            else
            {
                analysisDisplayStyle =
                    displayStyle.Cast<AnalysisDisplayStyle>().ElementAt<AnalysisDisplayStyle>(0);
            }
            // now assign the display style to the view
            doc.ActiveView.AnalysisDisplayStyleId = analysisDisplayStyle.Id;
        }
    }

    [ElementName("Analysis Results")]
    [ElementCategory(BuiltinElementCategories.ANALYSIS)]
    [ElementDescription("An analysis results object to be used with a spatial field manager.")]
    [RequiresTransaction(true)]
    class dynAnalysisResults : dynNode
    {
        const string DYNAMO_ANALYSIS_RESULTS_NAME = "Dynamo Analysis Results";

        int idx = -1;

        public dynAnalysisResults()
        {
            InPortData.Add(new PortData("lst", "List of values.", typeof(object)));
            InPortData.Add(new PortData("sfm", "Spatial Field Manager", typeof(Element)));
            InPortData.Add(new PortData("face", "Face", typeof(Reference)));
            OutPortData = new PortData("idx", "Analysis results object index", typeof(object));

            base.RegisterInputsAndOutputs();
        }

        public override Expression Evaluate(FSharpList<Expression> args)
        {
            SpatialFieldManager sfm = ((Expression.Container)args[1]).Item as SpatialFieldManager;

            //first, cleanup the old one
            if (idx != -1)
            {
                sfm.RemoveSpatialFieldPrimitive(idx);
            }

            Reference reference = ((Expression.Container)args[2]).Item as Reference;
            idx = sfm.AddSpatialFieldPrimitive(reference);

            Face face = dynElementSettings.SharedInstance.Doc.Document.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;

            IList<UV> uvPts = new List<UV>();
            BoundingBoxUV bb = face.GetBoundingBox();
            UV min = bb.Min;
            UV max = bb.Max;
            uvPts.Add(new UV(min.U, min.V));
            uvPts.Add(new UV(max.U, max.V));

            FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);

            //Build a sequence that unwraps the input list from it's Expression form.
            IEnumerable<double> nvals = ((Expression.List)args[0]).Item.Select(
               x => (double)((Expression.Number)x).Item
            );

            List<double> doubleList = new List<double>();
            foreach (var n in nvals)
            {
                doubleList.Add(n);
            }

            IList<ValueAtPoint> valList = new List<ValueAtPoint>();
            valList.Add(new ValueAtPoint(doubleList));
            valList.Add(new ValueAtPoint(doubleList));

            FieldValues vals = new FieldValues(valList);

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
            
            sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);

            return Expression.NewContainer(idx);

        }
    }
}

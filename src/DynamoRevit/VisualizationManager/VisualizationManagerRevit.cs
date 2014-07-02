using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Utilities;
using ProtoCore.Mirror;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;
using PolyCurve = Autodesk.DesignScript.Geometry.PolyCurve;

namespace Dynamo
{
    class VisualizationManagerRevit : VisualizationManager
    {
        private ElementId keeperId = ElementId.InvalidElementId;

        public ElementId KeeperId
        {
            get { return keeperId; }
        }

        public VisualizationManagerRevit() : base()
        {
            var context = dynSettings.Controller.Context;
            if (context == Context.VASARI_2014 || 
                context == Context.REVIT_2015)
            {
                AlternateDrawingContextAvailable = true;
                DrawToAlternateContext = false;

                AlternateContextName = context;

                RenderComplete += VisualizationManagerRenderComplete;
                RequestAlternateContextClear += CleanupVisualizations;
                dynSettings.Controller.DynamoModel.CleaningUp += CleanupVisualizations;
            }
            else
            {
                AlternateDrawingContextAvailable = false;
            }
        }

        private void CleanupVisualizations(object sender, EventArgs e)
        {
            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    if (keeperId != ElementId.InvalidElementId)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        /// <summary>
        ///     Handler for the visualization manager's RenderComplete event.
        ///     Sends goemetry to the GeomKeeper, if available, for preview in Revit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisualizationManagerRenderComplete(object sender, EventArgs e)
        {
            ////do not draw to geom keeper if the user has selected
            ////not to draw to the alternate context or if it is not available

            if (!AlternateDrawingContextAvailable
                || !DrawToAlternateContext)
                return;

            var values = dynSettings.Controller.DynamoModel.Nodes
                .Where(x => x.IsVisible).Where(x => x.CachedValue != null)
                .Select(x => x.CachedValue);

            var geoms = new List<GeometryObject>();
            values.ToList().ForEach(md=>RevitGeometryFromMirrorData(md, ref geoms));

            Draw(geoms);
        }

        private void Draw(IEnumerable<GeometryObject> geoms)
        {
            Type geometryElementType = typeof(GeometryElement);
            MethodInfo[] geometryElementTypeMethods =
                geometryElementType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            MethodInfo method =
                geometryElementTypeMethods.FirstOrDefault(x => x.Name == "SetForTransientDisplay");

            if (method == null)
                return;

            var styles = new FilteredElementCollector(DocumentManager.Instance.CurrentUIDocument.Document);
            styles.OfClass(typeof(GraphicsStyle));

            Element gStyle = styles.ToElements().FirstOrDefault(x => x.Name == "Dynamo");

            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(
                () =>
                {
                    TransactionManager.Instance.EnsureInTransaction(
                        DocumentManager.Instance.CurrentDBDocument);

                    if (keeperId != ElementId.InvalidElementId && 
                        DocumentManager.Instance.CurrentDBDocument.GetElement(keeperId) != null)
                    {
                        DocumentManager.Instance.CurrentUIDocument.Document.Delete(keeperId);
                        keeperId = ElementId.InvalidElementId;
                    }

                    var argsM = new object[4];
                    argsM[0] = DocumentManager.Instance.CurrentUIDocument.Document;
                    argsM[1] = ElementId.InvalidElementId;
                    argsM[2] = geoms;
                    if (gStyle != null)
                        argsM[3] = gStyle.Id;
                    else
                        argsM[3] = ElementId.InvalidElementId;

                    keeperId = (ElementId)method.Invoke(null, argsM);

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        /// <summary>
        /// Convert mirror data objects for nodes to Revit types.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="geoms"></param>
        private static void RevitGeometryFromMirrorData(MirrorData data, ref List<GeometryObject> geoms)
        {
            if (data.IsCollection)
            {
                foreach (var md in data.GetElements())
                {
                    try
                    {
                        RevitGeometryFromMirrorData(md, ref geoms);
                    }
                    catch (Exception ex)
                    {
                        dynSettings.DynamoLogger.Log(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    var geom = data.Data as PolyCurve;
                    if (geom != null)
                    {
                        geoms.AddRange(geom.ToRevitType());
                    }

                    var point = data.Data as Point;
                    if (point != null)
                    {
                        geoms.Add(DocumentManager.Instance.CurrentUIApplication.Application.Create.NewPoint(point.ToXyz()));
                    }

                    var curve = data.Data as Curve;
                    if (curve != null)
                    {
                        geoms.Add(curve.ToRevitType());
                    }
                }
                catch (Exception ex)
                {
                    dynSettings.DynamoLogger.Log(ex.Message);
                }
            }
        }
    }
}

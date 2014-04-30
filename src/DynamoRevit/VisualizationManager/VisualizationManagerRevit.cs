using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Dynamo.Nodes;
using Dynamo.Utilities;
using RevitServices.Persistence;
using RevitServices.Transactions;

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
            if (dynSettings.Controller.Context == Context.VASARI_2014)
            {
                AlternateDrawingContextAvailable = true;
                DrawToAlternateContext = false;

                AlternateContextName = "Vasari";

                VisualizationUpdateComplete += visualizationManager_VisualizationUpdateComplete;
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
        ///     Handler for the visualization manager's VisualizationUpdateComplete event.
        ///     Sends goemetry to the GeomKeeper, if available, for preview in Revit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visualizationManager_VisualizationUpdateComplete(object sender, EventArgs e)
        {
            ////do not draw to geom keeper if the user has selected
            ////not to draw to the alternate context or if it is not available
            //if (!AlternateDrawingContextAvailable
            //    || !DrawToAlternateContext)
            //    return;

            //IEnumerable<FScheme.Value> values = dynSettings.Controller.DynamoModel.Nodes
            //    .Where(x => x.IsVisible).Where(x => x.OldValue != null)
            //    //.Where(x => x.OldValue is Value.Container || x.OldValue is Value.List)
            //    .Select(x => x.OldValue.Data as FScheme.Value);

            //List<GeometryObject> geoms = values.ToList().SelectMany(RevitGeometryFromNodes).ToList();

            //Draw(geoms);
        }

        private void Draw(List<GeometryObject> geoms)
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

                    if (keeperId != ElementId.InvalidElementId)
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

                    //keeperId = GeometryElement.SetForTransientDisplay(dynRevitSettings.Doc.Document, ElementId.InvalidElementId, geoms,
                    //                                       ElementId.InvalidElementId);

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        ///// <summary>
        /////     Utility method to get the Revit geometry associated with nodes.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //private static List<GeometryObject> RevitGeometryFromNodes(FScheme.Value value)
        //{
        //    var geoms = new List<GeometryObject>();

        //    if (value == null)
        //        return geoms;

        //    if (value.IsList)
        //    {
        //        foreach (FScheme.Value valInner in ((FScheme.Value.List)value).Item)
        //            geoms.AddRange(RevitGeometryFromNodes(valInner));
        //        return geoms;
        //    }

        //    var container = value as FScheme.Value.Container;
        //    if (container == null)
        //        return geoms;

        //    var geom = ((FScheme.Value.Container)value).Item as GeometryObject;
        //    if (geom != null && !(geom is Face))
        //        geoms.Add(geom);

        //    var ps = ((FScheme.Value.Container)value).Item as ParticleSystem;
        //    if (ps != null)
        //    {
        //        geoms.AddRange(
        //            ps.Springs.Select(
        //                spring =>
        //                    Line.CreateBound(
        //                        spring.getOneEnd().getPosition(),
        //                        spring.getTheOtherEnd().getPosition())));
        //    }

        //    var cl = ((FScheme.Value.Container)value).Item as CurveLoop;
        //    if (cl != null)
        //        geoms.AddRange(cl);

        //    //draw xyzs as Point objects
        //    var pt = ((FScheme.Value.Container)value).Item as XYZ;
        //    if (pt != null)
        //    {
        //        Type pointType = typeof(Point);
        //        MethodInfo[] pointTypeMethods = pointType.GetMethods(
        //            BindingFlags.Static | BindingFlags.Public);
        //        MethodInfo method = pointTypeMethods.FirstOrDefault(x => x.Name == "CreatePoint");

        //        if (method != null)
        //        {
        //            var args = new object[3];
        //            args[0] = pt.X;
        //            args[1] = pt.Y;
        //            args[2] = pt.Z;
        //            geoms.Add((Point)method.Invoke(null, args));
        //        }
        //    }

        //    return geoms;
        //}

    }
}

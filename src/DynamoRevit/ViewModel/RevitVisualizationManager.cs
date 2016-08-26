using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using Dynamo.Core;
using Dynamo.Models;

using ProtoCore.Mirror;
using Revit.GeometryConversion;

using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

using Curve = Autodesk.DesignScript.Geometry.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;
using PolyCurve = Autodesk.DesignScript.Geometry.PolyCurve;

namespace Dynamo
{
    class RevitVisualizationManager : VisualizationManager
    {
        private ElementId keeperId = ElementId.InvalidElementId;
        private ElementId directShapeId = ElementId.InvalidElementId;

        public ElementId KeeperId
        {
            get { return keeperId; }
        }

        public RevitVisualizationManager(DynamoModel dynamoModel) : base(dynamoModel)
        {
            if (dynamoModel.Context == Context.VASARI_2014 ||
                dynamoModel.Context == Context.REVIT_2015)
            {
                AlternateDrawingContextAvailable = true;
                DrawToAlternateContext = false;

                AlternateContextName = dynamoModel.Context;

                RenderComplete += VisualizationManagerRenderComplete;
                RequestAlternateContextClear += CleanupVisualizations;
                dynamoModel.CleaningUp += CleanupVisualizations;
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

                    DirectShape directShape;
                    DocumentManager.Instance.CurrentDBDocument.TryGetElement(
                        directShapeId,
                        out directShape);
                    if (directShape != null)
                    {
                        // Set the direct shape to empty.
                        DocumentManager.Instance.CurrentDBDocument.Delete(directShapeId);
                        directShapeId = ElementId.InvalidElementId;
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

            var values = dynamoModel.Nodes
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
                geometryElementType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            MethodInfo method =
                geometryElementTypeMethods.FirstOrDefault(x => x.Name == "SetForTransientDisplay");

            if (method == null)
                return;

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
                    argsM[3] = ElementId.InvalidElementId;

                    keeperId = (ElementId)method.Invoke(null, argsM);

                    TransactionManager.Instance.ForceCloseTransaction();
                });
        }

        private void DrawSurfsAndSolids(List<GeometryObject> geoms)
        {
            Type dsType = typeof(DirectShape);
            MethodInfo[] dsStaticTypeMethods =
                dsType.GetMethods(BindingFlags.Static | BindingFlags.Public);

            MethodInfo[] dsInstanceTypeMethods =
                dsType.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            MethodInfo createMethod =
                dsStaticTypeMethods.FirstOrDefault(x => x.Name == "CreateElement");
            MethodInfo setMethod =
                dsInstanceTypeMethods.FirstOrDefault(x => x.Name == "SetShape" && x.GetParameters().Count() == 1);

            if (setMethod == null || createMethod == null)
                return;

            RevitServices.Threading.IdlePromise.ExecuteOnIdleAsync(
            () =>
            {
                TransactionManager.Instance.EnsureInTransaction(
                DocumentManager.Instance.CurrentDBDocument);

                Element directShape;
                DocumentManager.Instance.CurrentDBDocument.TryGetElement(
                        directShapeId,
                        out directShape);
                if (directShape == null)
                {
                    var categoryId = new ElementId(BuiltInCategory.OST_GenericModel);

                    directShape = (DirectShape)createMethod.Invoke(null, new object[] {DocumentManager.Instance.CurrentDBDocument, categoryId, "A", "B" });
                    directShapeId = directShape.Id;
                }

                setMethod.Invoke(
                    directShape,
                    new object[] { geoms });
                
                TransactionManager.Instance.ForceCloseTransaction();
            });
        }

        /// <summary>
        /// Convert mirror data objects for nodes to Revit types.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="geoms"></param>
        private void RevitGeometryFromMirrorData(MirrorData data, ref List<GeometryObject> geoms)
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
                        this.dynamoModel.Logger.Log(ex.Message);
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

                    var surf = data.Data as Surface;
                    if (surf != null)
                    {
                        geoms.AddRange(surf.ToRevitType());
                    }

                    var solid = data.Data as Autodesk.DesignScript.Geometry.Solid;
                    if (solid != null)
                    {
                        geoms.AddRange(solid.ToRevitType());
                    }

                }
                catch (Exception ex)
                {
                    this.dynamoModel.Logger.Log(ex.Message);
                }
            }
        }
    }
}

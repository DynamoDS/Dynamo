using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

using Dynamo.Core;
using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.Mirror;
using Revit.GeometryConversion;

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
        private MethodInfo method;

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

                RequestAlternateContextClear += CleanupVisualizations;
                dynamoModel.CleaningUp += CleanupVisualizations;
            }
            else
            {
                AlternateDrawingContextAvailable = false;
            }
        }

#if ENABLE_DYNAMO_SCHEDULER

        protected override void HandleRenderPackagesReadyCore()
        {
            // Trigger an update of visualization in alternate context.
            VisualizationManagerRenderComplete(this, EventArgs.Empty);
        }

#endif

        private void CleanupVisualizations(object sender, EventArgs e)
        {
            CleanupVisualizations();
        }

        private void CleanupVisualizations(DynamoModel model)
        {
            CleanupVisualizations();
        }

        private void CleanupVisualizations()
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

            var values = dynamoModel.Nodes
                .Where(x => x.IsVisible).Where(x => x.CachedValue != null)
                .Select(x => x.CachedValue);

            var geoms = new List<GeometryObject>();
            foreach (var value in values)
            {
                RevitGeometryFromMirrorData(value, ref geoms);
            }

            Draw(geoms);
        }

        private void Draw(IEnumerable<GeometryObject> geoms)
        {
            if (method == null)
            {
                Type geometryElementType = typeof(GeometryElement);
                MethodInfo[] geometryElementTypeMethods =
                    geometryElementType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                method = geometryElementTypeMethods.FirstOrDefault(x => x.Name == "SetForTransientDisplay");

                if (method == null)
                    return;
            }

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
                    if (data.Data == null)
                    {
                        return;
                    }

                    var geom = data.Data as PolyCurve;
                    if (geom != null)
                    {
                        // We extract the curves explicitly rather than using PolyCurve's ToRevitType
                        // extension method.  There is a potential issue with CurveLoop which causes
                        // this method to introduce corrupt GNodes.  
                        foreach (var c in geom.Curves())
                        {
                            // Tesselate the curve.  This greatly improves performance when
                            // we're dealing with NurbsCurve's with high knot count, commonly
                            // results of surf-surf intersections.
                            Tesselate(c, ref geoms);
                        }
                        
                        return;
                    }

                    var point = data.Data as Point;
                    if (point != null)
                    {
                        geoms.Add(DocumentManager.Instance.CurrentUIApplication.Application.Create.NewPoint(point.ToXyz()));
                        return;
                    }

                    var curve = data.Data as Curve;
                    if (curve != null)
                    {
                        // Tesselate the curve.  This greatly improves performance when
                        // we're dealing with NurbsCurve's with high knot count, commonly
                        // results of surf-surf intersections.
                        Tesselate(curve, ref geoms);
                        return;
                    }

                    var surf = data.Data as Surface;
                    if (surf != null)
                    {
                        geoms.AddRange(surf.ToRevitType());
                        return;
                    }

                    var solid = data.Data as Autodesk.DesignScript.Geometry.Solid;
                    if (solid != null)
                    {
                        geoms.AddRange(solid.ToRevitType());
                        return;
                    }

                }
                catch (Exception ex)
                {
                    this.dynamoModel.Logger.Log(ex.Message);
                }
            }
        }

        private void Tesselate(Autodesk.DesignScript.Geometry.Curve curve, ref List<GeometryObject> geoms)
        {
            // use the ASM tesselation of the curve
            var pkg = new RenderPackage();
            curve.Tessellate(pkg, 0.1);

            // get necessary info to enumerate and convert the lines
            var lineCount = pkg.LineStripVertices.Count - 3;
            var verts = pkg.LineStripVertices;

            // we scale the tesselation rather than the curve
            var conv = UnitConverter.DynamoToHostFactor;

            // add the revit Lines to geometry collection
            for (var i = 0; i < lineCount; i += 3)
            {
                var xyz0 = new XYZ(verts[i] * conv, verts[i + 1] * conv, verts[i + 2] * conv);
                var xyz1 = new XYZ(verts[i + 3] * conv, verts[i + 4] * conv, verts[i + 5] * conv);

                geoms.Add(Autodesk.Revit.DB.Line.CreateBound(xyz0, xyz1));
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Visualization;
using ProtoCore.Mirror;

namespace Dynamo.Scheduler
{
    class UpdateRenderPackageParams
    {
        internal IRenderPackageFactory RenderPackageFactory { get; set; }
        internal string PreviewIdentifierName { get; set; }
        internal NodeModel Node { get; set; }
        internal EngineController EngineController { get; set; }
        internal IEnumerable<KeyValuePair<Guid, string>> DrawableIdMap { get; set; }

        internal bool ForceUpdate { get; set; }
    }

    /// <summary>
    /// An asynchronous task to regenerate render packages for a given node. 
    /// During execution the task retrieves the corresponding IGraphicItem from 
    /// EngineController through a set of drawable identifiers supplied by the 
    /// node. These IGraphicItem objects then fill the IRenderPackage objects 
    /// with tessellated geometric data. Each of the resulting IRenderPackage 
    /// objects is then tagged with a label.
    /// </summary>
    /// 
    class UpdateRenderPackageAsyncTask : AsyncTask
    {
        private const byte DefR = 0;
        private const byte DefG = 0;
        private const byte DefB = 0;
        private const byte DefA = 255;
        private const byte MidTone = 180;

        #region Class Data Members and Properties

        protected Guid nodeGuid;

        private bool displayLabels;
        private bool isNodeSelected;
        private string previewIdentifierName;
        private EngineController engineController;
        private IEnumerable<KeyValuePair<Guid, string>> drawableIdMap;
        private readonly RenderPackageCache renderPackageCache;
        private IRenderPackageFactory factory;

        internal RenderPackageCache RenderPackages
        {
            get { return renderPackageCache; }
        }

        public override TaskPriority Priority
        {
            get { return TaskPriority.Normal; }
        }

        #endregion

        #region Public Class Operational Methods

        internal UpdateRenderPackageAsyncTask(IScheduler scheduler)
            : base(scheduler)
        {
            nodeGuid = Guid.Empty;
            renderPackageCache = new RenderPackageCache();
        }

        internal bool Initialize(UpdateRenderPackageParams initParams)
        {
            if (initParams == null)
                throw new ArgumentNullException("initParams");
            if (initParams.Node == null)
                throw new ArgumentNullException("initParams.Node");
            if (initParams.EngineController == null)
                throw new ArgumentNullException("initParams.EngineController");
            if (initParams.DrawableIdMap == null)
                throw new ArgumentNullException("initParams.DrawableIdMap");

            var nodeModel = initParams.Node;
            if (nodeModel.WasRenderPackageUpdatedAfterExecution && !initParams.ForceUpdate)
                return false; // Not has not been updated at all.

            // If a node is in either of the following states, then it will not 
            // produce any geometric output. Bail after clearing the render packages.
            if (nodeModel.IsInErrorState || !nodeModel.IsVisible)
                return false;

            // Without AstIdentifierForPreview, a node cannot have MirrorData.
            if (string.IsNullOrEmpty(nodeModel.AstIdentifierForPreview.Value))
                return false;

            drawableIdMap = initParams.DrawableIdMap;
            if (!drawableIdMap.Any())
                return false; // Nothing to be drawn.

            displayLabels = nodeModel.DisplayLabels;
            isNodeSelected = nodeModel.IsSelected;
            factory = initParams.RenderPackageFactory;
            engineController = initParams.EngineController;
            previewIdentifierName = initParams.PreviewIdentifierName;

            nodeGuid = nodeModel.GUID;
            nodeModel.WasRenderPackageUpdatedAfterExecution = true;
            return true;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void HandleTaskExecutionCore()
        {
            if (nodeGuid == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "UpdateRenderPackageAsyncTask.Initialize not called");
            }

            var idEnum = drawableIdMap.GetEnumerator();
            while (idEnum.MoveNext())
            {
                var mirrorData = engineController.GetMirror(idEnum.Current.Value);
                if (mirrorData == null)
                    continue;

                GetRenderPackagesFromMirrorData(
                    idEnum.Current.Key,
                    mirrorData.GetData(), 
                    previewIdentifierName, 
                    displayLabels);
            }
        }

        private void GetRenderPackagesFromMirrorData(
            Guid outputPortId,
            MirrorData mirrorData, 
            string tag, 
            bool displayLabels)
        {
            if (mirrorData.IsNull)
            {
                return;
            }

            var package = factory.CreateRenderPackage();
            package.DisplayLabels = displayLabels;
            package.Description = tag;
            package.IsSelected = isNodeSelected;

            GetRenderPackagesFromMirrorDataImp(mirrorData, package, tag);

            if (package.MeshVertexColors.Any())
            {
                package.RequiresPerVertexColoration = true;
            }

            renderPackageCache.Add(package, outputPortId);
        }

        private void GetRenderPackagesFromMirrorDataImp(
            MirrorData mirrorData,
            IRenderPackage package,
            string labelKey)
        {

            if (mirrorData.IsCollection)
            {
                int count = 0;
                foreach (var el in mirrorData.GetElements())
                {
                    if (el.IsCollection || el.Data is IGraphicItem)
                    {
                        string newLabel = labelKey + ":" + count;
                        GetRenderPackagesFromMirrorDataImp(el, package, newLabel);
                    }
                    count += 1;
                }
            }
            else
            {
                if (!(mirrorData.Data is IGraphicItem graphicItem))
                {
                    return;
                }

                var packageWithTransform = package as ITransformable;

                try
                {
                    var previousPointVertexCount = package.PointVertexCount;
                    var previousLineVertexCount = package.LineVertexCount;
                    var previousMeshVertexCount = package.MeshVertexCount;

                    //Plane tessellation needs to be handled here vs in LibG currently
                    if (graphicItem is Plane plane)
                    {
                        CreatePlaneTessellation(package, plane);
                    }
                    else
                    {
                        graphicItem.Tessellate(package, factory.TessellationParameters);

                        //Now we validate that tessellation call has added colors for each new vertex.
                        //If any vertex colors previously existed, this will ensure the vertex color and vertex counts stays in sync.
                        //If no pixel colors exist we leave the color array empty.  A default value will applied when the render package is displayed.
                        EnsureColorExistsPerVertex(package, previousPointVertexCount, previousLineVertexCount, previousMeshVertexCount);

                        if (factory.TessellationParameters.ShowEdges)
                        {
                            if (graphicItem is Topology topology)
                            {
                                if (graphicItem is Surface surf)
                                {
                                    foreach (var curve in surf.PerimeterCurves())
                                    {
                                        curve.Tessellate(package, factory.TessellationParameters);
                                        curve.Dispose();
                                    }
                                }
                                else
                                {
                                    var edges = topology.Edges;
                                    foreach (var geom in edges.Select(edge => edge.CurveGeometry))
                                    {
                                        geom.Tessellate(package, factory.TessellationParameters);
                                        geom.Dispose();
                                    }
                                    edges.ForEach(x => x.Dispose());
                                }
                            }
                        }

                    }

                    //If the package has a transform that is not the identity matrix
                    //then set requiresCustomTransform to true.
                    if (packageWithTransform != null && !packageWithTransform.RequiresCustomTransform && packageWithTransform.Transform.SequenceEqual(
                        new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }) == false)
                    {
                        (packageWithTransform).RequiresCustomTransform = true;
                    }

                    if (package is IRenderLabels packageLabels)
                    {
                        if (package.MeshVertexCount > previousMeshVertexCount)
                        {
                            packageLabels.AddLabel(labelKey,VertexType.Mesh, package.MeshVertexCount);
                        }
                        else if (package.LineVertexCount > previousLineVertexCount)
                        {
                            packageLabels.AddLabel(labelKey, VertexType.Line, package.LineVertexCount);
                        }
                        else if (package.PointVertexCount > previousPointVertexCount)
                        {
                            packageLabels.AddLabel(labelKey, VertexType.Point, package.PointVertexCount);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(
                        "PushGraphicItemIntoPackage: " + e);
                }
            }
        }

        private static void CreatePlaneTessellation(IRenderPackage package, Plane plane)
        {
            package.RequiresPerVertexColoration = true;

            var s = 2.5;

            var cs = CoordinateSystem.ByPlane(plane);
            var a = Point.ByCartesianCoordinates(cs, s, s, 0);
            var b = Point.ByCartesianCoordinates(cs, -s, s, 0);
            var c = Point.ByCartesianCoordinates(cs, -s, -s, 0);
            var d = Point.ByCartesianCoordinates(cs, s, -s, 0);
            //Todo Dispose cs, a, b, c, d?

            package.AddTriangleVertex(a.X, a.Y, a.Z);
            package.AddTriangleVertex(b.X, b.Y, b.Z);
            package.AddTriangleVertex(c.X, c.Y, c.Z);

            package.AddTriangleVertex(c.X, c.Y, c.Z);
            package.AddTriangleVertex(d.X, d.Y, d.Z);
            package.AddTriangleVertex(a.X, a.Y, a.Z);

            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);
            package.AddTriangleVertexUV(0, 0);

            // Draw plane edges
            package.AddLineStripVertex(a.X, a.Y, a.Z);
            package.AddLineStripVertex(b.X, b.Y, b.Z);
            package.AddLineStripVertex(b.X, b.Y, b.Z);
            package.AddLineStripVertex(c.X, c.Y, c.Z);
            package.AddLineStripVertex(c.X, c.Y, c.Z);
            package.AddLineStripVertex(d.X, d.Y, d.Z);
            package.AddLineStripVertex(d.X, d.Y, d.Z);
            package.AddLineStripVertex(a.X, a.Y, a.Z);

            // Draw normal
            package.AddLineStripVertex(plane.Origin.X, plane.Origin.Y, plane.Origin.Z);
            var nEnd = plane.Origin.Add(plane.Normal.Scale(2.5));
            package.AddLineStripVertex(nEnd.X, nEnd.Y, nEnd.Z);

            for (var i = 0; i < 5; i++)
            {
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(MidTone, MidTone, MidTone, 255);
                package.AddLineStripVertexColor(MidTone, MidTone, MidTone, 255);
            }

            for (var i = 0; i < 6; i++)
            {
                package.AddTriangleVertexNormal(plane.Normal.X, plane.Normal.Y, plane.Normal.Z);
                package.AddTriangleVertexColor(0, 0, 0, 10);
            }
        }

        private static void EnsureColorExistsPerVertex(IRenderPackage package, int previousPointVertexCount, int previousLineVertexCount, int previousMeshVertexCount)
        {
            var packageSupplement = package as IRenderPackageSupplement;
            
            //Use legacy slow path if package does not implement IRenderPackageSupplement
            if (packageSupplement == null)
            {
                LegacyPackageEnsureColorExistsPerVertex(package, previousPointVertexCount, previousLineVertexCount, previousMeshVertexCount);
                return;
            }
            
            if (package.PointVertexCount > previousPointVertexCount)
            {
                var count = package.PointVertexCount - packageSupplement.PointVertexColorCount;
                if (count > 0)
                {
                    packageSupplement.AppendPointVertexColorRange(CreateColorByteArrayOfSize(count, DefR, DefG, DefB, DefA));
                }
                
            }

            if (package.LineVertexCount > previousLineVertexCount)
            {
                var count = package.LineVertexCount - packageSupplement.LineVertexColorCount;
                if (count > 0)
                {
                    packageSupplement.AppendLineVertexColorRange(CreateColorByteArrayOfSize(count, DefR, DefG, DefB, DefA));
                }
            }

            if (package.MeshVertexCount > previousMeshVertexCount)
            {
                var count = package.MeshVertexCount - packageSupplement.MeshVertexColorCount;
                if (count > 0)
                {
                    packageSupplement.AppendMeshVertexColorRange(CreateColorByteArrayOfSize(count, DefR, DefG, DefB, DefA));
                }
            }
        }

        private static void LegacyPackageEnsureColorExistsPerVertex(IRenderPackage pkg, int previousPointVertexCount, int previousLineVertexCount, int previousMeshVertexCount)
        {
            if (pkg.PointVertexCount > previousPointVertexCount
                && pkg.PointVertexColors.Any()
                && pkg.PointVertexColors.Count() / 4 > pkg.PointVertexCount)
            {
                for (var i = pkg.PointVertexColors.Count() / 4; i < pkg.PointVertexCount; i++)
                {
                    pkg.AddPointVertexColor(DefR, DefG, DefB, DefA);
                }
            }

            if (pkg.LineVertexCount > previousLineVertexCount
                && pkg.LineStripVertexColors.Any()
                && pkg.LineStripVertexColors.Count() / 4 > pkg.LineVertexCount)
            {
                for (var i = pkg.LineStripVertexColors.Count() / 4; i < pkg.LineVertexCount; i++)
                {
                    pkg.AddLineStripVertexColor(DefR, DefG, DefB, DefA);
                }
            }

            if (pkg.MeshVertexCount > previousMeshVertexCount
                && pkg.MeshVertexColors.Any()
                && pkg.MeshVertexColors.Count() / 4 > pkg.MeshVertexCount)
            {
                for (var i = pkg.MeshVertexColors.Count() / 4; i < pkg.MeshVertexCount; i++)
                {
                    pkg.AddTriangleVertexColor(DefR, DefG, DefB, DefA);
                }
            }
        }

        private static byte[] CreateColorByteArrayOfSize(int size, byte red, byte green, byte blue, byte alpha)
        {
            var arr = new byte[size * 4];
            for (var i = 0; i < arr.Count(); i += 4)
            {
                arr[i] = red;
                arr[i + 1] = green;
                arr[i + 2] = blue;
                arr[i + 3] = alpha;
            }
            return arr;
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        protected override TaskMergeInstruction CanMergeWithCore(AsyncTask otherTask)
        {
            var theOtherTask = otherTask as UpdateRenderPackageAsyncTask;
            if (theOtherTask == null)
                return base.CanMergeWithCore(otherTask);

            // If the two UpdateRenderPackageAsyncTask are for different nodes,
            // then there is no comparison to be made, keep both the tasks.
            // 
            if (nodeGuid != theOtherTask.nodeGuid)
                return TaskMergeInstruction.KeepBoth;

            // Comparing to another NotifyRenderPackagesReadyAsyncTask, the one 
            // that gets scheduled more recently stay, while the earlier one 
            // gets dropped. If this task has a higher tick count, keep this.
            // 
            if (ScheduledTime.TickCount > theOtherTask.ScheduledTime.TickCount)
                return TaskMergeInstruction.KeepThis;

            return TaskMergeInstruction.KeepOther; // Otherwise, keep the other.
        }

        #endregion
    }
}

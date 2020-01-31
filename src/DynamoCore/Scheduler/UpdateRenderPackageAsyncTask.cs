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

            if (mirrorData.IsCollection)
            {
                int count = 0;
                foreach (var el in mirrorData.GetElements())
                {
                    if (el.IsCollection || el.Data is IGraphicItem)
                    {
                        string newTag = tag + ":" + count;
                        GetRenderPackagesFromMirrorData(outputPortId, el, newTag, displayLabels);
                    }
                    count = count + 1;
                }
            }
            else
            {
                var graphicItem = mirrorData.Data as IGraphicItem;
                if (graphicItem == null)
                {
                    return;
                }

                var package = factory.CreateRenderPackage();
                var packageWithTransform = package as ITransformable;
                package.Description = tag;

                try
                {
                    graphicItem.Tessellate(package, factory.TessellationParameters);
                    if (package.MeshVertexColors.Any())
                    {
                        package.RequiresPerVertexColoration = true;
                    }

                    //If the package has a transform that is not the identity matrix
                    //then set requiresCustomTransform to true.
                    if (packageWithTransform != null && packageWithTransform.Transform.SequenceEqual(
                        new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }) == false)
                    {
                        (packageWithTransform).RequiresCustomTransform = true;
                    }

                    if (factory.TessellationParameters.ShowEdges)
                    {
                        var topology = graphicItem as Topology;
                        if (topology != null)
                        {
                            var surf = graphicItem as Surface;
                            if (surf != null)
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
                    
                    var plane = graphicItem as Plane;
                    if (plane != null)
                    {
                        package.RequiresPerVertexColoration = true;

                        var s = 2.5;

                        var cs = CoordinateSystem.ByPlane(plane);
                        var a = Point.ByCartesianCoordinates(cs, s, s, 0);
                        var b = Point.ByCartesianCoordinates(cs, -s, s, 0);
                        var c = Point.ByCartesianCoordinates(cs, -s, -s, 0);
                        var d = Point.ByCartesianCoordinates(cs, s, -s, 0);

                        // Get rid of the original plane geometry.
                        package.Clear();

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

                        for (var i = 0; i < package.LineVertexCount / 2; i++)
                        {
                            package.AddLineStripVertexCount(2);
                        }

                        for (var i = 0; i < package.LineVertexCount; i ++)
                        {
                            package.AddLineStripVertexColor(MidTone, MidTone, MidTone, 255);
                        }

                        for (var i = 0; i < package.MeshVertexCount; i++)
                        {
                            package.AddTriangleVertexNormal(plane.Normal.X, plane.Normal.Y, plane.Normal.Z);
                        }

                        for (var i = 0; i < package.MeshVertexCount; i++)
                        {
                            package.AddTriangleVertexColor(0, 0, 0, 10);
                        }
                    }

                    // The default color coming from the geometry library for
                    // curves is 255,255,255,255 (White). Because we want a default
                    // color of 0,0,0,255 (Black), we adjust the color components here.
                    if (graphicItem is Curve || graphicItem is Surface || graphicItem is Solid || graphicItem is Point)
                    {
                        if (package.LineVertexCount > 0 && !package.LineStripVertexColors.Any())
                        {
                            package.ApplyLineVertexColors(CreateColorByteArrayOfSize(package.LineVertexCount, DefR, DefG, DefB, DefA));
                        }

                        if (package.PointVertexCount > 0 && !package.PointVertexColors.Any())
                        {
                            package.ApplyPointVertexColors(CreateColorByteArrayOfSize(package.PointVertexCount, DefR, DefG, DefB, DefA));
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(
                        "PushGraphicItemIntoPackage: " + e);
                }

                package.DisplayLabels = displayLabels;
                package.IsSelected = isNodeSelected;

                renderPackageCache.Add(package, outputPortId);
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

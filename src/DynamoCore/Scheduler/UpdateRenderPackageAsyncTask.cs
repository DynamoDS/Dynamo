using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
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
        /// <summary>
        /// Set to true to ignore the preview state of the node
        /// </summary>
        internal bool IgnoreIsVisible { get; set; }
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

            // If a node is in an error state it won't produce any geometric output.
            // Bail after clearing the render packages.
            if (nodeModel.IsInErrorState)
                return false;

            // If a node is not set as visible and the override is not set to true then
            // bail after clearing the render packages.
            if (!nodeModel.IsVisible && initParams.IgnoreIsVisible == false)
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

            //Initialize the package here assuming we will only generate a single renderPackage for this node
            //We set the AllowLegacyColorOperations so that we can catch tessellation implementations which 
            //use the deprecated calls.  At that point we will roll back the changes to the renderPackage
            //and call tessellate with a new renderPackage object with AllowLegacyColorOperations set to true.
            if (package is IRenderPackageSupplement packageSupplement)
            {
                packageSupplement.AllowLegacyColorOperations = false;
            }

            GetRenderPackagesFromMirrorDataImp(outputPortId, mirrorData, package, tag);

            if (package.MeshVertexColors.Any())
            {
                package.RequiresPerVertexColoration = true;
            }

            if (package.HasRenderingData)
            {
                renderPackageCache.Add(package, outputPortId);
            }
        }

        private void GetRenderPackagesFromMirrorDataImp(
            Guid outputPortId,
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
                        GetRenderPackagesFromMirrorDataImp(outputPortId, el, package, newLabel);
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

                GetTessellationDataFromGraphicItem(outputPortId, graphicItem, labelKey, ref package);
            }
        }

        private void GetTessellationDataFromGraphicItem(Guid outputPortId, IGraphicItem graphicItem, string labelKey, ref IRenderPackage package)
        {
            var packageWithTransform = package as ITransformable;
            var packageWithInstances = package as IInstancingRenderPackage;
            var packageWithLabelInstances = package as IRenderInstancedLabels;

            try
            {
                var previousPointVertexCount = package.PointVertexCount;
                var previousLineVertexCount = package.LineVertexCount;
                var previousMeshVertexCount = package.MeshVertexCount;

                //Todo Plane tessellation needs to be handled here vs in LibG currently
                bool instancingEnabled = factory.TessellationParameters.UseRenderInstancing;
                if (graphicItem is Plane plane)
                {
                    CreatePlaneTessellation(package, plane);
                }
                else if (graphicItem is IInstanceableGraphicItem instanceableItem &&
                    instanceableItem.InstanceInfoAvailable 
                    && packageWithInstances != null
                    && instancingEnabled)
                {
                    //if we have not generated the base tessellation for this type yet, generate it
                    if (!packageWithInstances.ContainsTessellationId(instanceableItem.BaseTessellationGuid))
                  
                    {
                        instanceableItem.AddBaseTessellation(packageWithInstances, factory.TessellationParameters);
                        var prevLineIndex = package.LineVertexCount;
                        //if edges is on, then also add edges to base tessellation.
                        if (factory.TessellationParameters.ShowEdges)
                        {
                            //TODO if we start to instance more types, expand this edge generation.
                            //and the swtich case below.
                            if (graphicItem is Topology topology)
                            {
                                Topology topologyInIdentityCS = null;
                                switch (topology)
                                {
                                    case Cuboid _:
                                        topologyInIdentityCS = Cuboid.ByLengths();
                                        break;
                                }
                                //if topologyInIdentityCS is still null or Edges is null 
                                //don't attempt to add any graphic edges.
                                var edges = topologyInIdentityCS?.Edges;
                                if (edges != null)
                                {
                                    foreach (var geom in edges.Select(edge => edge.CurveGeometry))
                                    {
                                        geom.Tessellate(package, factory.TessellationParameters);
                                        geom.Dispose();
                                    }

                                    edges.ForEach(x => x.Dispose());
                                    packageWithInstances.AddInstanceGuidForLineVertexRange(prevLineIndex, package.LineVertexCount - 1, instanceableItem.BaseTessellationGuid);
                                }
                                topologyInIdentityCS?.Dispose();
                            }
                        }
                    }

                    instanceableItem.AddInstance(packageWithInstances, factory.TessellationParameters, labelKey);
                    //for the instance we just added we need to add labels if autogen labels is true.
                    if (package is IRenderLabels labelPackage && labelPackage.AutoGenerateLabels && packageWithLabelInstances != null)
                    {
                        if (package.MeshVertexCount > 0)
                        {
                            packageWithLabelInstances.AddInstancedLabel(labelKey,
                                VertexType.MeshInstance,
                                package.MeshVertexCount, 
                                packageWithLabelInstances.InstanceCount(instanceableItem.BaseTessellationGuid),
                                instanceableItem.BaseTessellationGuid) ;
                        }
                        else if (package.LineVertexCount > 0)
                        {
                            packageWithLabelInstances.AddInstancedLabel(labelKey, 
                                VertexType.LineInstance,
                                package.LineVertexCount,
                                packageWithLabelInstances.InstanceCount(instanceableItem.BaseTessellationGuid),
                                instanceableItem.BaseTessellationGuid);
                        }
                    }
                    return;
                }
                else
                {
                    try
                    {
                        graphicItem.Tessellate(package, factory.TessellationParameters);
                    }
                    catch (LegacyRenderPackageMethodException)
                    {
                        //At this point we have detected an implementation of Tessellate which is using legacy color methods
                        //We roll back the primary renderPackage to it previous state before calling tessellation again.
                        package = RollBackPackage(package, previousPointVertexCount, previousLineVertexCount,
                            previousMeshVertexCount);

                        //Now we create a renderPackage object that will allow legacy color methods
                        var legacyPackage = factory.CreateRenderPackage();
                        legacyPackage.DisplayLabels = displayLabels;
                        legacyPackage.Description = labelKey;
                        legacyPackage.IsSelected = isNodeSelected;

                        //Use it to get fill the tessellation data and add it to the render cache.
                        GetTessellationDataFromGraphicItem(outputPortId, graphicItem, labelKey, ref legacyPackage);

                        if (legacyPackage.MeshVertexColors.Any())
                        {
                            legacyPackage.RequiresPerVertexColoration = true;
                        }

                        renderPackageCache.Add(legacyPackage, outputPortId);
                        
                        return;
                    }

                    //Now we validate that tessellation call has added colors for each new vertex.
                    //If any vertex colors previously existed, this will ensure the vertex color and vertex counts stays in sync.
                    EnsureColorExistsPerVertex(package, previousPointVertexCount, previousLineVertexCount,
                        previousMeshVertexCount);

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
                if (packageWithTransform != null && !packageWithTransform.RequiresCustomTransform &&
                    packageWithTransform.Transform.SequenceEqual(
                        new double[] {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1}) == false)
                {
                    (packageWithTransform).RequiresCustomTransform = true;
                }

                //Do not add replication labels if the tessellation call set DisplayLabels to true;
                if (package is IRenderLabels packageLabels && packageLabels.AutoGenerateLabels)
                {
                    if (package.MeshVertexCount > previousMeshVertexCount)
                    {
                        packageLabels.AddLabel(labelKey, VertexType.Mesh, package.MeshVertexCount);
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

        private static void CreatePlaneTessellation(IRenderPackage package, Plane plane)
        {
            package.RequiresPerVertexColoration = true;

            var s = 2.5;

            var cs = CoordinateSystem.ByPlane(plane);
            var a = Point.ByCartesianCoordinates(cs, s, s, 0);
            var b = Point.ByCartesianCoordinates(cs, -s, s, 0);
            var c = Point.ByCartesianCoordinates(cs, -s, -s, 0);
            var d = Point.ByCartesianCoordinates(cs, s, -s, 0);

            //Add two triangles to represent the plane
            package.AddTriangleVertex(a.X, a.Y, a.Z);
            package.AddTriangleVertex(b.X, b.Y, b.Z);
            package.AddTriangleVertex(c.X, c.Y, c.Z);

            package.AddTriangleVertex(c.X, c.Y, c.Z);
            package.AddTriangleVertex(d.X, d.Y, d.Z);
            package.AddTriangleVertex(a.X, a.Y, a.Z);

            //Add the mesh vertex UV, normal, and color data for the 6 triangle vertices
            for (var i = 0; i < 6; i++)
            {
                package.AddTriangleVertexUV(0, 0);
                package.AddTriangleVertexNormal(plane.Normal.X, plane.Normal.Y, plane.Normal.Z);
                package.AddTriangleVertexColor(0, 0, 0, 10);
            }

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

            //Add the line vertex data for the plane line geometry (4 plane edges and 1 normal).
            for (var i = 0; i < 5; i++)
            {
                package.AddLineStripVertexCount(2);
                package.AddLineStripVertexColor(MidTone, MidTone, MidTone, 255);
                package.AddLineStripVertexColor(MidTone, MidTone, MidTone, 255);
            }

            //dispose helper geometry
            a.Dispose();
            b.Dispose();
            c.Dispose();
            d.Dispose();
            cs.Dispose();
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
            // We sequence the if statements here to avoid calling Count on PointVertexColors if we don't have to.
            // For Helix IRenderPackage implementation we have to regenerate a new IEnumerable each time we access PointVertexColors.
            // The data is stored in a Helix specific data structure for efficient transfer to the view later.
            // The same applies for line and mesh

            if (pkg.PointVertexCount > previousPointVertexCount)
            {
                var colorCount = pkg.PointVertexColors.Count() / 4;
                if(colorCount < pkg.PointVertexCount)
                {
                    for (var i = colorCount; i < pkg.PointVertexCount; i++)
                    {
                        pkg.AddPointVertexColor(DefR, DefG, DefB, DefA);
                    }
                }
            }

            if (pkg.LineVertexCount > previousLineVertexCount)
            {
                var colorCount = pkg.LineStripVertexColors.Count() / 4;
                if (colorCount < pkg.LineVertexCount)
                {
                    for (var i = colorCount; i < pkg.LineVertexCount; i++)
                    {
                        pkg.AddLineStripVertexColor(DefR, DefG, DefB, DefA);
                    }
                }
            }

            if (pkg.MeshVertexCount > previousMeshVertexCount)
            {
                var colorCount = pkg.MeshVertexColors.Count() / 4;
                if (colorCount < pkg.MeshVertexCount)
                {
                    for (var i = colorCount; i < pkg.MeshVertexCount; i++)
                    {
                        pkg.AddTriangleVertexColor(DefR, DefG, DefB, DefA);
                    }
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

        private IRenderPackage RollBackPackage(IRenderPackage package, int previousPointVertexCount, int previousLineVertexCount, int previousMeshVertexCount)
        {
            var newPackage = factory.CreateRenderPackage();

            newPackage.Description = package.Description;
            newPackage.IsSelected = package.IsSelected;
            newPackage.DisplayLabels = package.DisplayLabels;
            
            

            var ptVertices = package.PointVertices.ToArray();
            for (var i = 0; i < previousPointVertexCount * 3 && i < ptVertices.Length; i += 3)
            {
                newPackage.AddPointVertex(ptVertices[i], ptVertices[i+1],ptVertices[i+2]);
            }

            var ptColors = package.PointVertexColors.ToArray();
            for (var i = 0; i < previousPointVertexCount * 4 && i < ptColors.Length; i += 4)
            {
                newPackage.AddPointVertexColor(ptColors[i], ptColors[i + 1], ptColors[i + 2], ptColors[i + 3]);
            }

            var lineVertices = package.LineStripVertices.ToArray();
            for (var i = 0; i < previousLineVertexCount * 3 && i < lineVertices.Length; i += 3)
            {
                newPackage.AddLineStripVertex(lineVertices[i], lineVertices[i + 1], lineVertices[i + 2]);
            }

            var lineColors = package.LineStripVertexColors.ToArray();
            for (var i = 0; i < previousLineVertexCount * 4 && i < lineColors.Length; i += 4)
            {
                newPackage.AddLineStripVertexColor(lineColors[i], lineColors[i + 1], lineColors[i + 2], lineColors[i + 3]);
            }

            var lineVerticesCounts = package.LineStripVertexCounts.ToArray();
            var count = 0;
            var index = 0;
            while (count < lineVerticesCounts.Length && count < previousLineVertexCount)
            {
                newPackage.AddLineStripVertexCount(lineVerticesCounts[index]);
                count += lineVerticesCounts[index];
                index++;
            }

            var meshVertices = package.MeshVertices.ToArray();
            for (var i = 0; i < previousMeshVertexCount * 3 && i < meshVertices.Length; i += 3)
            {
                newPackage.AddTriangleVertex(meshVertices[i], meshVertices[i + 1], meshVertices[i + 2]);
            }

            var meshNormals = package.MeshNormals.ToArray();
            for (var i = 0; i < previousMeshVertexCount * 3 && i < meshNormals.Length; i += 3)
            {
                newPackage.AddTriangleVertexNormal(meshNormals[i], meshNormals[i + 1], meshNormals[i + 2]);
            }

            var meshUV = package.MeshTextureCoordinates.ToArray();
            for (var i = 0; i < previousMeshVertexCount * 2 && i < meshUV.Length; i += 2)
            {
                newPackage.AddTriangleVertexUV(meshUV[i], meshUV[i + 1]);
            }

            var meshColors = package.MeshVertexColors.ToArray();
            for (var i = 0; i < previousMeshVertexCount * 4 && i < meshColors.Length; i += 4)
            {
                newPackage.AddTriangleVertexColor(meshColors[i], meshColors[i + 1], meshColors[i + 2], meshColors[i + 3]);
            }
            
            //Todo Need to obsolete ITransformable
            if(package is ITransformable transformable && newPackage is ITransformable newTransformable)
            {
                newTransformable.RequiresCustomTransform = transformable.RequiresCustomTransform;
                var t = transformable.Transform;
                if (t.Length == 16)
                {
                    newTransformable.SetTransform(t[0], t[1], t[2], t[3], t[4], t[5], t[6], t[7], t[8], t[9], t[10], t[11], t[12], t[13], t[14], t[15]);
                }
            }
            
            if(package is IRenderPackageSupplement packageSupplement  && newPackage is IRenderPackageSupplement newPackageSupplement)
            {
                var textureMapsList = packageSupplement.TextureMapsList;
                var textureMapsStrideList = packageSupplement.TextureMapsStrideList;
                var meshVerticesRange = packageSupplement.MeshVerticesRangesAssociatedWithTextureMaps;
                packageSupplement.AllowLegacyColorOperations = false;

                for (var i = 0; i < textureMapsList.Count; i++)
                {
                    newPackageSupplement.AddTextureMapForMeshVerticesRange(meshVerticesRange[i].Item1, meshVerticesRange[i].Item2, textureMapsList[i], textureMapsStrideList[i]);
                }
            }

            if (package is IRenderLabels renderLabels && newPackage is IRenderLabels newRenderLabels)
            {
                var labelData = renderLabels.LabelData;

                foreach (var (label, pt) in labelData)
                {
                    newRenderLabels.AddLabel(label, pt[0], pt[1], pt[2]);
                }
            }

            return newPackage;
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

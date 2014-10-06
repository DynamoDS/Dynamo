#if ENABLE_DYNAMO_SCHEDULER

using System;
using System.Collections;
using System.Linq;

using GraphLayout;

using ProtoCore.Mirror;
using System.Collections.Generic;

using Dynamo.DSEngine;
using Dynamo.Models;
using Autodesk.DesignScript.Interfaces;

namespace Dynamo.Core.Threading
{
    class UpdateRenderPackageParams
    {
        internal int MaxTesselationDivisions { get; set; }
        internal string PreviewIdentifierName { get; set; }
        internal NodeModel Node { get; set; }
        internal EngineController EngineController { get; set; }
        internal IEnumerable<string> DrawableIds { get; set; }
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
        #region Class Data Members and Properties

        private int maxTesselationDivisions;
        private bool displayLabels;
        private bool isNodeSelected;
        private string previewIdentifierName;
        private EngineController engineController;
        private IEnumerable<string> drawableIds;
        private readonly List<IRenderPackage> renderPackages;

        internal IEnumerable<IRenderPackage> RenderPackages
        {
            get { return renderPackages; }
        }

        #endregion

        #region Public Class Operational Methods

        internal UpdateRenderPackageAsyncTask(DynamoScheduler scheduler)
            : base(scheduler)
        {
            renderPackages = new List<IRenderPackage>();
        }

        internal bool Initialize(UpdateRenderPackageParams initParams)
        {
            if (initParams == null)
                throw new ArgumentNullException("initParams");
            if (initParams.Node == null)
                throw new ArgumentNullException("initParams.Node");
            if (initParams.EngineController == null)
                throw new ArgumentNullException("initParams.EngineController");
            if (initParams.DrawableIds == null)
                throw new ArgumentNullException("initParams.DrawableIds");

            var nodeModel = initParams.Node;
            if (!nodeModel.IsUpdated && (!nodeModel.RequiresRecalc))
                return false; // Not has not been updated at all.

            drawableIds = initParams.DrawableIds;
            if (!drawableIds.Any())
                return false; // Nothing to be drawn.

            displayLabels = nodeModel.DisplayLabels;
            isNodeSelected = nodeModel.IsSelected;
            maxTesselationDivisions = initParams.MaxTesselationDivisions;
            engineController = initParams.EngineController;
            previewIdentifierName = initParams.PreviewIdentifierName;
            return true;
        }

        #endregion

        #region Protected Overridable Methods

        protected override void ExecuteCore()
        {
            var data = from varName in drawableIds
                       select engineController.GetMirror(varName)
                       into mirror
                       where mirror != null
                       select mirror.GetData();

            var labelMap = new List<string>();
            foreach (var mirrorData in data)
            {
                AddToLabelMap(mirrorData, labelMap, previewIdentifierName);
            }

            int count = 0;
            foreach (var drawableId in drawableIds)
            {
                var graphItems = engineController.GetGraphicItems(drawableId);
                if (graphItems == null)
                    continue;

                foreach (var graphicItem in graphItems)
                {
                    var package = new RenderPackage(isNodeSelected, displayLabels)
                    {
                        Tag = labelMap.Count > count ? labelMap[count] : "?",
                    };

                    try
                    {
                        graphicItem.Tessellate(package, tol: -1.0,
                            maxGridLines: maxTesselationDivisions);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "PushGraphicItemIntoPackage: " + e);
                    }

                    package.ItemsCount++;
                    renderPackages.Add(package);
                    count++;
                }
            }
        }

        protected override void HandleTaskCompletionCore()
        {
        }

        #endregion

        #region Private Class Helper Methods

        // Add labels for each of a mirror data object's inner data object to a label map.
        private static void AddToLabelMap(MirrorData data, List<string> map, string tag)
        {
            if (data.IsCollection)
            {
                var index = 0;
                var elements = data.GetElements();
                foreach (var element in elements)
                {
                    var newTag = string.Format("{0}:{1}", tag, index++);
                    AddToLabelMap(element, map, newTag);
                }
            }
            else if (data.Data is IEnumerable)
            {
                AddToLabelMap(data.Data as IEnumerable, map, tag);
            }
            else
            {
                map.Add(tag);
            }
        }

        // Add labels for each object in an enumerable to a label map
        private static void AddToLabelMap(IEnumerable list, List<string> map, string tag)
        {
            int count = 0;
            foreach (var obj in list)
            {
                var newTag = string.Format("{0}:{1}", tag, count++);

                if (obj is IEnumerable)
                {
                    AddToLabelMap(obj as IEnumerable, map, newTag);
                }
                else
                {
                    map.Add(newTag);
                }
            }
        }

        #endregion
    }
}

#endif

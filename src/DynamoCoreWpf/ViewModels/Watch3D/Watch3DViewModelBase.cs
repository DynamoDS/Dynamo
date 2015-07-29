﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using Autodesk.DesignScript.Interfaces;
using Dynamo.Core;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.ViewModels;

namespace Dynamo.Wpf.ViewModels.Watch3D
{
    public struct Watch3DViewModelStartupParams
    {
        public DynamoModel Model { get; set; }
        public IRenderPackageFactory Factory { get; set; }
        public DynamoViewModel ViewModel { get; set; }
        public bool IsActiveAtStart { get; set; }
        public string Name { get; set; }
        public ILogger Logger { get; set; }
    }

    /// <summary>
    /// The Watch3DViewModelBase is the base class for all 3D previews in Dynamo.
    /// Classes which derive from this base are used to prepare geometry for 
    /// rendering by various render targets. The base class handles the registration
    /// of all necessary event handlers on models, workspaces, and nodes.
    /// </summary>
    public class Watch3DViewModelBase : NotificationObject
    {
        protected DynamoModel model;
        protected DynamoViewModel viewModel;
        protected IRenderPackageFactory factory;
        protected int renderingTier;
        protected List<NodeModel> recentlyAddedNodes = new List<NodeModel>();
        protected bool active;
        private readonly List<IRenderPackage> currentTaggedPackages = new List<IRenderPackage>();
        protected ILogger logger;

        /// <summary>
        /// A flag which indicates whether geometry should be processed.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (active == value)
                {
                    return;
                }

                active = value;
                RaisePropertyChanged("Active");

                OnActiveStateChanged();
            }
        }

        /// <summary>
        /// A name which identifies this view model when multiple
        /// Watch3DViewModel objects exist.
        /// </summary>
        public string Name { get; protected set; }

        private bool canNavigateBackground = false;
        public bool CanNavigateBackground
        {
            get { return canNavigateBackground; }
            set
            {
                canNavigateBackground = value;
                RaisePropertyChanged("CanNavigateBackground");
            }
        }

        protected Watch3DViewModelBase(Watch3DViewModelStartupParams parameters)
        {
            model = parameters.Model;
            viewModel = parameters.ViewModel;
            factory = parameters.Factory;
            Active = parameters.IsActiveAtStart;
            Name = parameters.Name;
            logger = parameters.Logger;

            RegisterEventHandlers();
        }

        protected virtual void OnStartup()
        {
            // Override in inherited classes.
        }

        protected virtual void OnShutdown()
        {
            // Override in inherited classes.
        }

        protected virtual void OnBeginUpdate(IEnumerable<IRenderPackage> packages)
        {
            // Override in inherited classes.
        }

        protected virtual void OnClear()
        {
            // Override in inherited classes.
        }

        protected virtual void OnActiveStateChanged()
        {
            UnregisterEventHandlers();
            OnClear();
        }

        private void RegisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged += SelectionChangedHandler;

            LogVisualizationCapabilities();

            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            viewModel.RenderPackageFactoryViewModel.PropertyChanged += OnRenderPackageFactoryViewModelPropertyChanged;

            RegisterModelEventhandlers(model);

            RegisterWorkspaceEventHandlers(model);

        }

        private void UnregisterEventHandlers()
        {
            DynamoSelection.Instance.Selection.CollectionChanged -= SelectionChangedHandler;

            viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            viewModel.RenderPackageFactoryViewModel.PropertyChanged -= OnRenderPackageFactoryViewModelPropertyChanged;

            UnregisterModelEventHandlers(model);

            UnregisterWorkspaceEventHandlers(model);
        }

        private void OnModelShutdownStarted(DynamoModel model)
        {
            UnregisterEventHandlers();
            OnShutdown();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsPanning":
                case "IsOrbiting":
                    RaisePropertyChanged("LeftClickCommand");
                    break;
            }
        }

        private void LogVisualizationCapabilities()
        {
            renderingTier = (RenderCapability.Tier >> 16);
            var pixelShader3Supported = RenderCapability.IsPixelShaderVersionSupported(3, 0);
            var pixelShader4Supported = RenderCapability.IsPixelShaderVersionSupported(4, 0);
            var softwareEffectSupported = RenderCapability.IsShaderEffectSoftwareRenderingSupported;
            var maxTextureSize = RenderCapability.MaxHardwareTextureSize;

            model.Logger.Log(string.Format("RENDER : Rendering Tier: {0}", renderingTier), LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Pixel Shader 3 Supported: {0}", pixelShader3Supported),
                LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Pixel Shader 4 Supported: {0}", pixelShader4Supported),
                LogLevel.File);
            model.Logger.Log(
                string.Format("RENDER : Software Effect Rendering Supported: {0}", softwareEffectSupported), LogLevel.File);
            model.Logger.Log(string.Format("RENDER : Maximum hardware texture size: {0}", maxTextureSize),
                LogLevel.File);
        }

        private void RegisterModelEventhandlers(DynamoModel model)
        {
            model.WorkspaceCleared += OnWorkspaceCleared;
            model.ShutdownStarted += OnModelShutdownStarted;
            model.CleaningUp += OnClear;
            model.EvaluationCompleted += OnEvaluationCompleted;
            model.PropertyChanged += OnModelPropertyChanged;
        }

        protected virtual void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void OnEvaluationCompleted(object sender, EvaluationCompletedEventArgs e)
        {
            // Override in derived classes
        }

        private void UnregisterModelEventHandlers(DynamoModel model)
        {
            model.WorkspaceCleared -= OnWorkspaceCleared;
            model.ShutdownStarted -= OnModelShutdownStarted;
            model.CleaningUp -= OnClear;
        }

        private void UnregisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded -= OnWorkspaceAdded;
            model.WorkspaceRemoved -= OnWorkspaceRemoved;
            model.WorkspaceOpening -= OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving -= OnWorkspaceSaving;
            }
        }

        private void RegisterWorkspaceEventHandlers(DynamoModel model)
        {
            model.WorkspaceAdded += OnWorkspaceAdded;
            model.WorkspaceRemoved += OnWorkspaceRemoved;
            model.WorkspaceOpening += OnWorkspaceOpening;

            foreach (var ws in model.Workspaces)
            {
                ws.Saving += OnWorkspaceSaving;
                ws.NodeAdded += OnNodeAddedToWorkspace;
                ws.NodeRemoved += OnNodeRemovedFromWorkspace;

                foreach (var node in ws.Nodes)
                {
                    node.PropertyChanged += OnNodePropertyChanged;
                    node.RenderPackagesUpdated += OnRenderPackagesUpdated;
                }
            }
        }

        private void OnRenderPackageFactoryViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ShowEdges":
                    model.PreferenceSettings.ShowEdges =
                        factory.TessellationParameters.ShowEdges;
                    break;
            }

            model.CurrentWorkspace.Nodes.Select(n => n.IsUpdated = true);

            foreach (var node in
                model.CurrentWorkspace.Nodes)
            {
                node.RequestVisualUpdateAsync(model.Scheduler, model.EngineController,
                        factory);
            }
        }

        protected virtual void OnWorkspaceCleared(object sender, EventArgs e)
        {
            OnClear();
        }

        protected virtual void OnWorkspaceOpening(XmlDocument doc)
        {
            // Override in derived classes.
        }

        private void OnWorkspaceAdded(WorkspaceModel workspace)
        {
            workspace.Saving += OnWorkspaceSaving;
            workspace.NodeAdded += OnNodeAddedToWorkspace;
            workspace.NodeRemoved += OnNodeRemovedFromWorkspace;

            foreach (var node in workspace.Nodes)
            {
                RegisterNodeEventHandlers(node);
            }
        }

        private void OnWorkspaceRemoved(WorkspaceModel workspace)
        {
            workspace.Saving -= OnWorkspaceSaving;
            workspace.NodeAdded -= OnNodeAddedToWorkspace;
            workspace.NodeRemoved -= OnNodeRemovedFromWorkspace;

            foreach (var node in workspace.Nodes)
            {
                UnregisterNodeEventHandlers(node);
            }

            OnClear();
        }

        protected virtual void OnWorkspaceSaving(XmlDocument doc)
        {
            // Override in derived classes
        }

        private void OnNodeAddedToWorkspace(NodeModel node)
        {
            RegisterNodeEventHandlers(node);
        }

        private void OnNodeRemovedFromWorkspace(NodeModel node)
        {
            UnregisterNodeEventHandlers(node);
            DeleteGeometryForIdentifier(node.AstIdentifierBase);
        }

        protected virtual void DeleteGeometryForIdentifier(string identifier, bool requestUpdate = true)
        {
            // Override in derived classes.
        }

        private void RegisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged += OnNodePropertyChanged;
            node.RenderPackagesUpdated += OnRenderPackagesUpdated;

            RegisterPortEventHandlers(node);
        }

        private void UnregisterNodeEventHandlers(NodeModel node)
        {
            node.PropertyChanged -= OnNodePropertyChanged;
            node.RenderPackagesUpdated -= OnRenderPackagesUpdated;

            UnregisterPortEventHandlers(node);
        }

        protected void RegisterPortEventHandlers(NodeModel node)
        {
            foreach (var p in node.InPorts)
            {
                p.PortDisconnected += PortDisconnectedHandler;
                p.PortConnected += PortConnectedHandler;
            }
        }

        private void UnregisterPortEventHandlers(NodeModel node)
        {
            foreach (var p in node.InPorts)
            {
                p.PortDisconnected -= PortDisconnectedHandler;
                p.PortConnected -= PortConnectedHandler;
            }
        }

        protected virtual void PortConnectedHandler(PortModel arg1, ConnectorModel arg2)
        {
            // Do nothing for a standard node.
        }

        protected virtual void PortDisconnectedHandler(PortModel port)
        {
            DeleteGeometryForIdentifier(port.Owner.AstIdentifierBase);
        }

        protected virtual void SelectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Override in derived classes
        }

        protected virtual void OnRenderPackagesUpdated(NodeModel updatedNode, IEnumerable<IRenderPackage> packages)
        {
            OnBeginUpdate(packages);
        }

        public virtual void GenerateViewGeometryFromRenderPackagesAndRequestUpdate(
            IEnumerable<IRenderPackage> taskPackages)
        {
            // Override in derived classes
        }

        protected virtual void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Override in derived classes.
        }

        internal virtual void ExportToSTL(string path, string modelName)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Display a label for one or several render packages 
        /// based on the paths of those render packages.
        /// </summary>
        //public void TagRenderPackageForPath(string path)
        //{
        //    var packages = new List<IRenderPackage>();

        //    //This also isn't thread safe
        //    foreach (var node in model.CurrentWorkspace.Nodes)
        //    {
        //        lock (node.RenderPackagesMutex)
        //        {
        //            //Note(Luke): this seems really inefficent, it's doing an O(n) search for a tag
        //            //This is also a target for memory optimisation

        //            packages.AddRange(
        //                node.RenderPackages.Where(x => x.Description == path || x.Description.Contains(path + ":")));
        //        }
        //    }

        //    if (packages.Any())
        //    {
        //        //clear any labels that might have been drawn on this
        //        //package already and add the one we want
        //        if (currentTaggedPackages.Any())
        //        {
        //            currentTaggedPackages.ForEach(x => x.DisplayLabels = false);
        //            currentTaggedPackages.Clear();
        //        }

        //        packages.ToList().ForEach(x => x.DisplayLabels = true);
        //        currentTaggedPackages.AddRange(packages);

        //        var allPackages = new List<IRenderPackage>();
        //        var selPackages = new List<IRenderPackage>();

        //        foreach (var node in model.CurrentWorkspace.Nodes)
        //        {
        //            lock (node.RenderPackagesMutex)
        //            {
        //                allPackages.AddRange(
        //                    node.RenderPackages.Where(x => x.HasRenderingData && !x.IsSelected));
        //                selPackages.AddRange(
        //                    node.RenderPackages.Where(x => x.HasRenderingData && x.IsSelected));
        //            }
        //        }

        //        //OnResultsReadyToVisualize(
        //        //    new VisualizationEventArgs(allPackages, selPackages, Guid.Empty));
        //    }
        //}
    }
}

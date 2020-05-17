using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Dynamo.Extensions;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.ZeroTouch;
using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Dynamo.Visualization;
using Dynamo.Wpf.Extensions;
using Dynamo.Wpf.ViewModels.Watch3D;
using DoubleSlider = CoreNodeModels.Input.DoubleSlider;

namespace Dynamo.Manipulation
{
    public class DynamoManipulationExtension : IViewExtension
    {
        private IEnumerable<NodeModel> manipulatorNodes;
        private ManipulatorDaemon manipulatorDaemon;
        private ViewLoadedParams viewLoadedParams;
        private IWorkspaceModel workspaceModel;
        //Keeps track of Node Guids for which event handler is attached.
        private HashSet<Guid> trackedNodeGuids = new HashSet<Guid>();

        internal IWatch3DViewModel BackgroundPreviewViewModel { get; private set; }
        internal IRenderPackageFactory RenderPackageFactory { get; private set; }
        internal IWorkspaceModel WorkspaceModel 
        {
            get { return workspaceModel; }
            private set { SetWorkSpaceModel(value); } 
        }

        /// <summary>
        /// Sets the workspace model property and updates event handlers accordingly.
        /// </summary>
        /// <param name="wsm">Workspace Model to set</param>
        private void SetWorkSpaceModel(IWorkspaceModel wsm)
        {
            var settings = GetRunSettings(workspaceModel);
            if (settings != null)
            {
                //Remove RunSettings event handler from old run settings.
                settings.PropertyChanged -= OnRunSettingsPropertyChanged;
            }

            workspaceModel = wsm;

            settings = GetRunSettings(wsm);
            if (settings != null)
            {
                //Register RunSettings event handler
                settings.PropertyChanged += OnRunSettingsPropertyChanged;
            }
        }

        /// <summary>
        /// Event handler to monitor RunType property of RunSettings.
        /// </summary>
        private void OnRunSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "RunType")
                return;

            var settings = sender as RunSettings;
            if (settings == null) return;

            if (settings.RunType != RunType.Automatic)
            {
                manipulatorDaemon.KillAll();//remove all manipulators
            }
            else if(settings.RunEnabled)
            {
                CreateManipulators(WorkspaceModel.CurrentSelection);
            }
        }

        /// <summary>
        /// Returns RunSettings object for a given Workspace Model. 
        /// </summary>
        /// <param name="wsm">IWorkspaceModel object</param>
        /// <returns>RunSettings</returns>
        private RunSettings GetRunSettings(IWorkspaceModel wsm)
        {
            var homeWSM = wsm as HomeWorkspaceModel;
            return homeWSM == null ? null : homeWSM.RunSettings;
        }

        /// <summary>
        /// Checks if manipulators are allowed in current workspace or not.
        /// </summary>
        /// <returns></returns>
        bool EnableManipulators()
        {
            //If we are not in Home workspace, or not in Automatic execution mode do nothing.
            var settings = GetRunSettings(WorkspaceModel);
            
            return settings != null && settings.RunType == RunType.Automatic;
        }

        internal ICommandExecutive CommandExecutive { get; private set; }

        public Dictionary<Type, IEnumerable<INodeManipulatorFactory>> ManipulatorCreators { get; set; }

        #region IViewExtension implementation

        public string UniqueId
        {
            get { return "58B0496A-E3F8-43D9-86D2-94823D1D0F98"; }
        }

        public string Name
        {
            get { return "DynamoManipulationExtension"; }
        }

        public void Dispose()
        {
            manipulatorDaemon.KillAll();

            UnregisterEventHandlers();
        }

        public void Startup(ViewStartupParams viewStartupParams)
        {
            manipulatorDaemon = ManipulatorDaemon.Create(new NodeManipulatorFactoryLoader());
        }

        public void Loaded(ViewLoadedParams viewStartupParams)
        {
            viewLoadedParams = viewStartupParams;

            WorkspaceModel = viewStartupParams.CurrentWorkspaceModel;
            BackgroundPreviewViewModel = viewStartupParams.BackgroundPreviewViewModel;
            RenderPackageFactory = viewStartupParams.RenderPackageFactory;
            CommandExecutive = viewStartupParams.CommandExecutive;

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            viewLoadedParams.SelectionCollectionChanged += UpdateManipulators;
            viewLoadedParams.CurrentWorkspaceChanged += OnCurrentWorkspaceChanged;

            BackgroundPreviewViewModel.CanNavigateBackgroundPropertyChanged += Watch3DViewModelNavigateBackgroundPropertyChanged;
            BackgroundPreviewViewModel.ViewMouseDown += Watch3DViewModelOnViewMouseDown;
        }

        private void UnregisterEventHandlers()
        {
            viewLoadedParams.SelectionCollectionChanged -= UpdateManipulators;
            viewLoadedParams.CurrentWorkspaceChanged -= OnCurrentWorkspaceChanged;

            BackgroundPreviewViewModel.CanNavigateBackgroundPropertyChanged -= Watch3DViewModelNavigateBackgroundPropertyChanged;
            BackgroundPreviewViewModel.ViewMouseDown -= Watch3DViewModelOnViewMouseDown;
        }

        private void Watch3DViewModelOnViewMouseDown(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            BackgroundPreviewViewModel.HighlightNodeGraphics(manipulatorNodes);
        }

        private void Watch3DViewModelNavigateBackgroundPropertyChanged(bool canNavigateBackground)
        {
            if (canNavigateBackground)
            {
                // if switching to geometry view
                // Get the Model3D objects corresponding to the nodes and highlight them
                var nodes = GetZeroTouchNodesForMatchingNames();
                manipulatorNodes = nodes.Where(InspectInputsForNode);

                BackgroundPreviewViewModel.HighlightNodeGraphics(manipulatorNodes);
            }
            else
            {
                // if switching to node view
                BackgroundPreviewViewModel.UnHighlightNodeGraphics(manipulatorNodes);
            }

        }

        private IEnumerable<DSFunctionBase> GetZeroTouchNodesForMatchingNames()
        {
            var nodes = WorkspaceModel.Nodes;
            var nodeNames = manipulatorDaemon.NodeNames;
            var nodeModels = new List<DSFunctionBase>();
            foreach (var nodeName in nodeNames)
            {
                var creationName = nodeName;

                // Find zero touch nodes that match a given name
                var ztNodes = nodes.OfType<DSFunction>().Where(n => n.CreationName == creationName);

                nodeModels.AddRange(ztNodes);
            }
            return nodeModels;
        }

        /// <summary>
        /// For each node in nodes, inspect inputs and filter only those nodes
        /// that have at least one slider input or no input
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool InspectInputsForNode(NodeModel node)
        {
            bool manipulable = false;
            for (int i = 0; i < node.InPorts.Count; i++)
            {
                Tuple<int, NodeModel> val;
                if (node.InputNodes.TryGetValue(i, out val))
                {
                    if (val != null)
                    {
                        if (val.Item2 is DoubleSlider)
                        {
                            manipulable = true;
                            break;
                        }
                    }
                    else
                    {
                        manipulable = true;
                        break;
                    }
                }
                else
                {
                    manipulable = true;
                    break;
                }
            }
            return manipulable;
        }

        public void Shutdown()
        {
            
        }

        #endregion

        private void OnCurrentWorkspaceChanged(IWorkspaceModel wsm)
        {
            // update the workspace when a new Dynamo file is opened
            // so that it always references the current workspace
            WorkspaceModel = wsm;
            manipulatorNodes = new List<NodeModel>();

            // kill all manipulators in current workspace
            if (manipulatorDaemon != null)
            {
                manipulatorDaemon.KillAll();
            }
        }

        private void UpdateManipulators(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                if (e.OldItems != null)
                {
                    foreach (var nm in e.OldItems.OfType<NodeModel>())
                    {
                        manipulatorDaemon.KillManipulators(nm);
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                manipulatorDaemon.KillAll();
            }

            if (e.NewItems == null || !EnableManipulators()) return;

            CreateManipulators(e.NewItems.OfType<NodeModel>());
        }

        private void CreateManipulators(IEnumerable<NodeModel> nodes)
        {
            foreach (var nm in nodes)
            {
                TryCreateManipulator(nm);
            }
        }

        /// <summary>
        /// Tries to create a manipulator for the given node. This method
        /// also registers the property changed event handler.
        /// </summary>
        /// <param name="node">The input node</param>
        /// <returns>true if manipulator was created</returns>
        private bool TryCreateManipulator(NodeModel node)
        {
            //Can't create manipulator for node that is not selected.
            if (!node.IsSelected) return false;

            //Make sure we register property changed notification if node is selected.
            if (trackedNodeGuids.Add(node.GUID))
            {
                node.PropertyChanged += OnNodePropertyChanged;
            }
            
            //No manipulator for frozen node or node in state of error
            if (node.IsFrozen || node.IsInErrorState) return false;

            //If the node already has a manipulator, then skip creating new one.
            if (manipulatorDaemon.HasNodeManipulator(node)) return false;

            //Finally let the Daemon create the manipulator.
            manipulatorDaemon.CreateManipulator(node, this);
            return true;
        }

        /// <summary>
        /// Kills the manipulators on the given node. This method will unregister the
        /// property changed event handler if the node is not selected.
        /// </summary>
        /// <param name="node">Input node</param>
        private void KillManipulators(NodeModel node)
        {
            //Remove the event handler
            if (!node.IsSelected)
            {
                node.PropertyChanged -= OnNodePropertyChanged;
                trackedNodeGuids.Remove(node.GUID);
            }
            manipulatorDaemon.KillManipulators(node);
        }

        /// <summary>
        /// Event handler for property changed. Kills manipulator on node if 
        /// its frozen and creates manipulator if node is selected and unfrozen.
        /// </summary>
        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as NodeModel;
            if (null == node) return;

            if(e.PropertyName == "IsFrozen")
            {
                if(node.IsFrozen)
                {
                    KillManipulators(node);
                }
                else if(node.IsSelected)
                {
                    TryCreateManipulator(node);
                }
            }
        }

    }
}

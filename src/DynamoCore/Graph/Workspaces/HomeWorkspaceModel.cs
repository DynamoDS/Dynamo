using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Extensions;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Linting;
using Dynamo.Models;
using Dynamo.Scheduler;
using Newtonsoft.Json;
using ProtoCore;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    ///     This class contains methods and properties that defines a <see cref="WorkspaceModel"/>.
    /// </summary>
    public class HomeWorkspaceModel : WorkspaceModel
    {
        #region Class Data Members and Properties

        private string thumbnail;
        private Uri graphDocumentationURL;
        private DynamoScheduler scheduler;
        private PulseMaker pulseMaker;
        private bool verboseLogging;
        private bool graphExecuted;

        // Event to handle closing of the workspace references extension when the workspace is closed. 
        internal static event Action WorkspaceClosed;

        private IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> historicalTraceData;

        private bool graphRunInProgress;

        /// <summary>
        /// A flag which indicates whether Dynamo is currently running a graph.
        /// This flag is set to true when execution starts and is set to false
        /// when execution is completed.
        /// </summary>
        internal bool GraphRunInProgress
        {
            get { return graphRunInProgress; }
            set
            {
                if (graphRunInProgress == value) return;

                graphRunInProgress = value;
                RaisePropertyChanged(nameof(GraphRunInProgress));
            }
        }

        /// <summary>
        ///     Returns <see cref="EngineController"/> object assosiated with thisPreloadedTraceData home workspace
        /// to coordinate the interactions between some DesignScript
        /// sub components like library managment, live runner and so on.
        /// </summary>
        [JsonIgnore]
        public EngineController EngineController { get; private set; }

        /// <summary>
        ///     Flag specifying if this workspace is operating in "test mode".
        /// </summary>
        [JsonIgnore]
        public bool IsTestMode { get; set; }

        /// <summary>
        ///     Indicates whether a run has completed successfully.   
        /// 
        ///     This flag is critical to ensuring that crashing run-auto files
        ///     are not left in run-auto upon reopening.  
        /// </summary>
        [JsonIgnore]
        public bool HasRunWithoutCrash { get; set; }

        /// <summary>
        ///     Before the Workspace has been built the first time, we do not respond to 
        ///     NodeModification events.
        /// </summary>
        private bool silenceNodeModifications = true;

        /// <summary>
        /// Indicates if detailed descriptions should be logged
        /// </summary>
        [JsonIgnore]
        public readonly bool VerboseLogging;

        /// <summary>
        ///     Returns <see cref="EngineController"/> object containing properties to control
        /// how execution is carried out.
        /// </summary>
        [JsonIgnore]
        public RunSettings RunSettings { get; set; }

        /// <summary>
        /// Evaluation count is incremented whenever the graph is evaluated. 
        /// It is set to zero when the graph is Cleared.
        /// </summary>
        [JsonIgnore]
        public long EvaluationCount { get; private set; }

        /// <summary>
        /// Link to documentation page for this workspace
        /// </summary>
        public Uri GraphDocumentationURL
        {
            get { return graphDocumentationURL; }
            set
            {
                if (graphDocumentationURL == value)
                    return;

                graphDocumentationURL = value;
                RaisePropertyChanged(nameof(GraphDocumentationURL));
            }
        }


        /// <summary>
        /// Workspace thumbnail as Base64 string.
        /// Returns null if provide value is not Base64 encoded.
        /// </summary>
        public string Thumbnail
        {
            get { return thumbnail; }
            set
            {
                try
                {
                    // if value is not a valid Base64 string this will throw, and we return null.
                    byte[] data = Convert.FromBase64String(value);
                    thumbnail = value;
                    RaisePropertyChanged(nameof(Thumbnail));
                }
                catch
                {
                    return;
                }
            }
        }

        /// <summary>
        /// List of user defined data from extensions and view extensions stored in the graph
        /// </summary>
        internal ICollection<ExtensionData> ExtensionData
        {
            get; set;
        }

        /// <summary>
        /// In near future, the file loading mechanism will be completely moved 
        /// into WorkspaceModel, that's the time we removed this property setter below.
        /// </summary>
        internal IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> PreloadedTraceData
        {
            get
            {
                return preloadedTraceData;
            }

            set
            {
                if (value != null && (preloadedTraceData != null))
                {
                    const string message = "PreloadedTraceData cannot be set twice";
                    throw new InvalidOperationException(message);
                }

                preloadedTraceData = value;
            }
        }

        private IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> preloadedTraceData;

        internal bool IsEvaluationPending
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Notifies listeners that graph evaluation is started.
        /// At this stage, the graph has not been analyzed yet
        /// so Dynamo could find that the Graph does not need to be executed at all,
        /// or only part of it could be executed (delta compute)
        /// </summary>
        public event EventHandler<EventArgs> EvaluationStarted;

        /// <summary>
        /// Triggers EvaluationStarted event.
        /// </summary>
        /// <param name="e">The event data.</param>
        public virtual void OnEvaluationStarted(EventArgs e)
        {
            this.HasRunWithoutCrash = false;
            GraphRunInProgress = true;

            try
            {
                // Do not allow graph runs to be triggered from the EvaluationStarted event. We want to avoid potential infinite loops.
                // Use a simple internal flag like ForceBlockRun (RunEnabled would notify WPF of changes and that would incur peformance penalties without any benefit)
                RunSettings.ForceBlockRun = true;
                EvaluationStarted?.Invoke(this, e);
            }
            finally
            {
                RunSettings.ForceBlockRun = false;
            }
        }

        /// <summary>
        /// Notifies listeners that graph evaluation is completed.
        /// </summary>
        public event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;

        /// <summary>
        /// Triggers EvaluationCompleted event.
        /// </summary>
        /// <param name="e">The event data containing information 
        /// if graph evaluation has completed successfully.</param>
        public virtual void OnEvaluationCompleted(EvaluationCompletedEventArgs e)
        {
            this.HasRunWithoutCrash = e.EvaluationSucceeded;
            GraphRunInProgress = false;

            var handler = EvaluationCompleted;
            if (handler != null) handler(this, e);

            this.ScaleFactorChanged = false;
        }

        /// <summary>
        /// Notifies listeners when all tasks in scheduler related to current evalution are completed.
        /// </summary>
        public event EventHandler<EvaluationCompletedEventArgs> RefreshCompleted;

        /// <summary>
        /// Triggers RefreshCompleted event
        /// </summary>
        /// <param name="e">The event data containing information 
        /// if graph evaluation has completed successfully.</param>
        public virtual void OnRefreshCompleted(EvaluationCompletedEventArgs e)
        {
            var handler = RefreshCompleted;
            if (handler != null) handler(this, e);
        }

        internal event EventHandler<DeltaComputeStateEventArgs> SetNodeDeltaState;
        internal virtual void OnSetNodeDeltaState(DeltaComputeStateEventArgs e)
        {
            var handler = SetNodeDeltaState;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty instance of the <see cref="HomeWorkspaceModel"/> class
        /// </summary>
        /// <param name="engine"><see cref="EngineController"/> object assosiated with this home workspace
        /// to coordinate the interactions between some DesignScript sub components.</param>
        /// <param name="scheduler"><see cref="DynamoScheduler"/> object to add tasks in queue to execute</param>
        /// <param name="factory">Node factory to create nodes</param>
        /// <param name="verboseLogging">Indicates if detailed descriptions should be logged</param>
        /// <param name="isTestMode">Indicates if current code is running in tests</param>
        /// <param name="fileName">Name of file where the workspace is saved</param>
        [Obsolete("please use the version with linterManager parameter.")]
        public HomeWorkspaceModel(EngineController engine, DynamoScheduler scheduler,
            NodeFactory factory, bool verboseLogging, bool isTestMode, string fileName = "")
            : this(engine,
                scheduler,
                factory,
                Enumerable.Empty<KeyValuePair<Guid, List<CallSite.RawTraceData>>>(),
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                Enumerable.Empty<PresetModel>(),
                new ElementResolver(),
                new WorkspaceInfo() { FileName = fileName, Name = "Home" },
                verboseLogging,
                isTestMode) { }

        /// <summary>
        /// Initializes a new empty instance of the <see cref="HomeWorkspaceModel"/> class
        /// </summary>
        /// <param name="engine"><see cref="EngineController"/> object assosiated with this home workspace
        /// to coordinate the interactions between some DesignScript sub components.</param>
        /// <param name="scheduler"><see cref="DynamoScheduler"/> object to add tasks in queue to execute</param>
        /// <param name="factory">Node factory to create nodes</param>
        /// <param name="verboseLogging">Indicates if detailed descriptions should be logged</param>
        /// <param name="isTestMode">Indicates if current code is running in tests</param>
        /// <param name="linterManager">The linter manager from the DynamoModel that owns this workspace</param>
        /// <param name="fileName">Name of file where the workspace is saved</param>
        public HomeWorkspaceModel(EngineController engine, DynamoScheduler scheduler,
            NodeFactory factory, bool verboseLogging, bool isTestMode, LinterManager linterManager, string fileName = "")
            : this(engine,
                scheduler,
                factory,
                Enumerable.Empty<KeyValuePair<Guid, List<CallSite.RawTraceData>>>(),
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                Enumerable.Empty<PresetModel>(),
                new ElementResolver(),
                new WorkspaceInfo() { FileName = fileName, Name = "Home" },
                verboseLogging,
                isTestMode,
                linterManager)
        { }

        [Obsolete("please use the version with linterManager parameter.")]
        public HomeWorkspaceModel(Guid guid, EngineController engine,
            DynamoScheduler scheduler,
            NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> traceData,
            IEnumerable<NodeModel> nodes,
            IEnumerable<NoteModel> notes,
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver,
            WorkspaceInfo info,
            bool verboseLogging,
            bool isTestMode):this(engine, scheduler, factory, traceData, nodes, notes, 
                annotations, presets, resolver, info, verboseLogging, isTestMode)
        { Guid = guid; }

        public HomeWorkspaceModel(Guid guid, EngineController engine,
            DynamoScheduler scheduler,
            NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> traceData,
            IEnumerable<NodeModel> nodes,
            IEnumerable<NoteModel> notes,
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver,
            WorkspaceInfo info,
            bool verboseLogging,
            bool isTestMode,
            LinterManager linterManager) : this(engine, scheduler, factory, traceData, nodes, notes,
                annotations, presets, resolver, info, verboseLogging, isTestMode, linterManager)
        { Guid = guid; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeWorkspaceModel"/> class
        /// by given information about it and specified item collections
        /// </summary>
        /// <param name="engine"><see cref="EngineController"/> object assosiated with this home workspace
        /// to coordinate the interactions between some DesignScript sub components.</param>
        /// <param name="scheduler"><see cref="DynamoScheduler"/> object to add tasks in queue to execute</param>
        /// <param name="factory">Node factory to create nodes</param>
        /// <param name="traceData">Preloaded trace data</param>
        /// <param name="nodes">Node collection of the workspace</param>
        /// <param name="notes">Note collection of the workspace</param>
        /// <param name="annotations">Group collection of the workspace</param>
        /// <param name="presets">Preset collection of the workspace</param>
        /// <param name="elementResolver">ElementResolver responsible for resolving 
        /// a partial class name to its fully resolved name</param>
        /// <param name="info">Information for creating custom node workspace</param>
        /// <param name="verboseLogging">Indicates if detailed descriptions should be logged</param>
        /// <param name="isTestMode">Indicates if current code is running in tests</param>
        [Obsolete("please use the version with linterManager parameter.")]
        public HomeWorkspaceModel(EngineController engine, 
            DynamoScheduler scheduler, 
            NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> traceData, 
            IEnumerable<NodeModel> nodes, 
            IEnumerable<NoteModel> notes, 
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver,
            WorkspaceInfo info, 
            bool verboseLogging,
            bool isTestMode)
            : base(nodes, notes,annotations, info, factory,presets, resolver)
        {
            InitializeHomeWorkspace(engine, traceData, scheduler, info, verboseLogging, isTestMode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeWorkspaceModel"/> class
        /// by given information about it and specified item collections
        /// </summary>
        /// <param name="engine"><see cref="EngineController"/> object assosiated with this home workspace
        /// to coordinate the interactions between some DesignScript sub components.</param>
        /// <param name="scheduler"><see cref="DynamoScheduler"/> object to add tasks in queue to execute</param>
        /// <param name="factory">Node factory to create nodes</param>
        /// <param name="traceData">Preloaded trace data</param>
        /// <param name="nodes">Node collection of the workspace</param>
        /// <param name="notes">Note collection of the workspace</param>
        /// <param name="annotations">Group collection of the workspace</param>
        /// <param name="presets">Preset collection of the workspace</param>
        /// <param name="elementResolver">ElementResolver responsible for resolving 
        /// a partial class name to its fully resolved name</param>
        /// <param name="info">Information for creating custom node workspace</param>
        /// <param name="verboseLogging">Indicates if detailed descriptions should be logged</param>
        /// <param name="isTestMode">Indicates if current code is running in tests</param>
        /// <param name="linterManager">The linter manager from the DynamoModel that owns this workspace</param>
        public HomeWorkspaceModel(EngineController engine,
            DynamoScheduler scheduler,
            NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> traceData,
            IEnumerable<NodeModel> nodes,
            IEnumerable<NoteModel> notes,
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver,
            WorkspaceInfo info,
            bool verboseLogging,
            bool isTestMode,
            LinterManager linterManager)
            : base(nodes, notes, annotations, info, factory, presets, resolver, linterManager)
        {
            InitializeHomeWorkspace(engine, traceData, scheduler, info, verboseLogging, isTestMode);
        }

        private void InitializeHomeWorkspace
            (EngineController engine, 
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> traceData, 
            DynamoScheduler scheduler,
            WorkspaceInfo info, 
            bool verboseLogging, 
            bool isTestMode)
        {
            Debug.WriteLine("Creating a home workspace...");

            EvaluationCount = 0;

            // This protects the user from a file that might have crashed during
            // its last run.  As a side effect, this also causes all files set to
            // run auto but lacking the HasRunWithoutCrash flag to run manually.
            if (info.RunType == RunType.Automatic && !info.HasRunWithoutCrash)
            {
                info.RunType = RunType.Manual;
            }

            RunSettings = new RunSettings(info.RunType, info.RunPeriod);

            PreloadedTraceData = traceData;

            this.scheduler = scheduler;
            this.verboseLogging = verboseLogging;
            IsTestMode = isTestMode;
            EngineController = engine;
            this.ExtensionData = new List<ExtensionData>();

            // The first time the preloaded trace data is set, we cache
            // the data as historical. This will be used after the initial
            // run of this workspace, when the PreloadedTraceData has been
            // nulled, to check for node deletions and reconcile the trace data.
            // We do a deep copy of this data because the PreloadedTraceData is
            // later set to null before the graph update.
            var copiedData = new List<KeyValuePair<Guid, List<CallSite.RawTraceData>>>();
            foreach (var kvp in PreloadedTraceData)
            {
                List<CallSite.RawTraceData> callSiteTraceData = new List<CallSite.RawTraceData>();
                callSiteTraceData.AddRange(kvp.Value);
                copiedData.Add(new KeyValuePair<Guid, List<CallSite.RawTraceData>>(kvp.Key, callSiteTraceData));
            }
            historicalTraceData = copiedData;
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (EngineController != null)
            {
                EngineController.MessageLogged -= Log;
                EngineController.LibraryServices.LibraryLoaded -= LibraryLoaded;
                EngineController.Dispose();
            }

            if (pulseMaker == null) return;

            pulseMaker.Stop();
        }

        protected override void OnNodeRemoved(NodeModel node)
        {
            base.OnNodeRemoved(node);
            EngineController.NodeDeleted(node);
        }

        private void LibraryLoaded(object sender, LibraryServices.LibraryLoadedEventArgs e)
        {
            // Need to make compiled custom nodes available before running the graph.
            EngineController.OnRequestCustomNodeRegistration();
            // Mark all nodes as dirty so that AST for the whole graph will be
            // regenerated.
            MarkNodesAsModifiedAndRequestRun(Nodes);
        }

        /// <summary>
        ///     Invoked when a change to the workspace that requires re-execution
        ///     has taken place.  If in run-automatic, a new run will take place,
        ///     otherwise nothing will happen.
        /// </summary>
        internal override void RequestRun()
        {
            base.RequestRun();

            if (RunSettings.RunType != RunType.Manual)
            {
                if (!DelayGraphExecution)
                {
                    Run();
                }
            }
        }

        /// <summary>
        /// Called when a Node is modified in the workspace
        /// </summary>
        /// <param name="node">The node itself</param>
        protected override void NodeModified(NodeModel node)
        {
            base.NodeModified(node);

            if (!silenceNodeModifications)
            {
                RequestRun();
            }
        }

        /// <summary>
        /// When a node is frozen, then the node and its dependencies should be 
        /// deleted from the current AST
        /// </summary>
        /// <param name="node">The node.</param>
        protected override void OnToggleNodeFreeze(NodeModel node)
        {
            EngineController.DeleteFrozenNodesFromAST(node);
        }

        /// <summary>
        /// Called when a node is added to the workspace and event handlers are to be added
        /// </summary>
        /// <param name="node">The node itself</param>
        protected override void RegisterNode(NodeModel node)
        {
            base.RegisterNode(node);

            node.RequestSilenceNodeModifiedEvents += NodeOnRequestSilenceNodeModifiedEvents;
        }

        /// <summary>
        /// Called when a node is disposed and removed from the workspace
        /// </summary>
        /// <param name="node">The node itself</param>
        protected override void DisposeNode(NodeModel node)
        {
            node.RequestSilenceNodeModifiedEvents -= NodeOnRequestSilenceNodeModifiedEvents;

            base.DisposeNode(node);
        }

        /// <summary>
        /// Called when the RequestSilenceNodeModifiedEvents event is emitted from a Node
        /// </summary>
        /// <param name="node">The node itself</param>
        /// <param name="value">A boolean value indicating whether to silence or not</param>
        private void NodeOnRequestSilenceNodeModifiedEvents(NodeModel node, bool value)
        {
            this.silenceNodeModifications = value;
        }

        #region Public Operational Methods

        /// <summary>
        ///     Clears this workspace of nodes, notes, and connectors.
        /// </summary>
        public override void Clear()
        {
            WorkspaceClosed?.Invoke();
            UndefineCBNFunctionDefinitions();

            base.Clear();
            PreloadedTraceData = null;
            RunSettings.Reset();
            EvaluationCount = 0;
        }

        internal void UndefineCBNFunctionDefinitions()
        {
            var cbns = Nodes.OfType<CodeBlockNodeModel>();
            foreach (var cbn in cbns)
            {
                cbn.UndefineFunctionDefinitions();
            }
        }

        /// <summary>
        /// Start periodic evaluation using the currently set RunPeriod
        /// </summary>
        internal void StartPeriodicEvaluation()
        {
            if (pulseMaker == null)
            {
                pulseMaker = new PulseMaker();
            }

            pulseMaker.RunStarted += PulseMakerRunStarted;
            RefreshCompleted += pulseMaker.OnRefreshCompleted;

            if (pulseMaker.TimerPeriod != 0)
            {
                throw new InvalidOperationException(
                    "Periodic evaluation cannot be started without stopping");
            }

            pulseMaker.Start(RunSettings.RunPeriod);
        }

        /// <summary>
        /// Stop the on-going periodic evaluation, if there is any.
        /// </summary>
        /// 
        internal void StopPeriodicEvaluation()
        {
            if (pulseMaker == null || (pulseMaker.TimerPeriod == 0)) return;

            pulseMaker.RunStarted -= PulseMakerRunStarted;
            RefreshCompleted -= pulseMaker.OnRefreshCompleted;
            pulseMaker.Stop();
        }

        #endregion

        [Obsolete("Method will be deprecated in Dynamo 3.0.")]
        protected override bool PopulateXmlDocument(XmlDocument document)
        {
            if (!base.PopulateXmlDocument(document))
                return false;

            var root = document.DocumentElement;
            if (root == null)
                return false;

            root.SetAttribute("RunType", RunSettings.RunType.ToString());
            root.SetAttribute("RunPeriod", RunSettings.RunPeriod.ToString(CultureInfo.InvariantCulture));
            root.SetAttribute("HasRunWithoutCrash", HasRunWithoutCrash.ToString(CultureInfo.InvariantCulture));

            return true;
        }

        private void PulseMakerRunStarted()
        {
            var nodesToUpdate = Nodes.Where(n => n.CanUpdatePeriodically);
            MarkNodesAsModifiedAndRequestRun(nodesToUpdate, true);
        }


        #region evaluation

        /// <summary>
        /// Call this method to reset the virtual machine, avoiding a race 
        /// condition by using a thread join inside the vm executive.
        /// TODO(Luke): Push this into a resync call with the engine controller
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="markNodesAsDirty">Set this parameter to true to force 
        ///     reset of the execution substrait. Note that setting this parameter 
        ///     to true will have a negative performance impact.</param>
        internal void ResetEngine(EngineController controller, bool markNodesAsDirty = false)
        {
            if (EngineController != null)
            {
                EngineController.MessageLogged -= Log;
                EngineController.LibraryServices.LibraryLoaded -= LibraryLoaded;
            }

            EngineController = controller;
            controller.MessageLogged += Log;
            controller.LibraryServices.LibraryLoaded += LibraryLoaded;

            if (markNodesAsDirty)
            {
                // Mark all nodes as dirty so that AST for the whole graph will be
                // regenerated.
                MarkNodesAsModifiedAndRequestRun(Nodes);
            }

            if (RunSettings.RunType == RunType.Automatic)
                Run();
        }

        /// <summary>
        /// Mark the input nodes as modified
        /// </summary>
        /// <param name="nodes">The nodes to modify</param>
        /// <param name="forceExecute">The argument to NodeModel.MarkNodeAsModified</param>
        internal void MarkNodesAsModifiedAndRequestRun(IEnumerable<NodeModel> nodes, bool forceExecute = false)
        {
            if (nodes == null)
                throw new ArgumentNullException("nodes");

            foreach (var node in nodes)
            {
                node.MarkNodeAsModified(forceExecute);
            } 

            if (nodes.Any())
            {
                RequestRun();
            } 
        }

        /// <summary>
        /// This callback method is invoked in the context of ISchedulerThread 
        /// when UpdateGraphAsyncTask is completed.
        /// </summary>
        /// <param name="task">The original UpdateGraphAsyncTask instance.</param>
        private void OnUpdateGraphCompleted(AsyncTask task)
        {
            var updateTask = (UpdateGraphAsyncTask)task;
            var messages = new Dictionary<Guid, string>();

            // Runtime warnings take precedence over build warnings.
            foreach (var warning in updateTask.RuntimeWarnings)
            {
                var message = string.Join(Environment.NewLine, warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            foreach (var warning in updateTask.BuildWarnings)
            {
                // If there is already runtime warnings for 
                // this node, then ignore the build warnings.
                // But for cyclic dependency warnings, it is
                // easier to understand to report a build warning.
                string message = string.Empty;
                if (messages.ContainsKey(warning.Key))
                {
                    if (!warning.Value.Any(w => w.ID == ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency))
                        continue;

                    messages.Remove(warning.Key);
                    message = string.Join("\n", warning.Value.
                        Where(w => w.ID == ProtoCore.BuildData.WarningID.InvalidStaticCyclicDependency).
                        Select(w => w.Message));
                }
                else
                {
                    message = string.Join("\n", warning.Value.Select(w => w.Message));
                }

                if (!string.IsNullOrEmpty(message))
                {
                    messages.Add(warning.Key, message);
                }
            }

            var workspace = updateTask.TargetedWorkspace;
            foreach (var message in messages)
            {
                var guid = message.Key;
                var node = workspace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node == null)
                    continue;
                using (node.PropertyChangeManager.SetPropsToSuppress(nameof(NodeModel.ToolTipText), nameof(NodeModel.Infos), nameof(NodeModel.State)))
                {
                    node.Warning(message.Value); // Update node warning message.
                }
            }

            RunSettings.RunTypesEnabled = true; //Re-enable Run Type ComboBox

            //set the node execution preview to false;
            OnSetNodeDeltaState(new DeltaComputeStateEventArgs(new List<Guid>(), graphExecuted));

            // This method is guaranteed to be called in the context of 
            // ISchedulerThread (for Revit's case, it is the idle thread).
            // Dispatch the failure message display for execution on UI thread.
            // 
            EvaluationCompletedEventArgs e = task.Exception == null || IsTestMode
                ? new EvaluationCompletedEventArgs(true,messages.Keys,null)
                : new EvaluationCompletedEventArgs(true, messages.Keys, task.Exception);

            EvaluationCount ++;

            OnEvaluationCompleted(e);

            if (EngineController.IsDisposed) return;

            EngineController.ReconcileTraceDataAndNotify();

            // Refresh values of nodes that took part in update.
            var nodes = new HashSet<NodeModel>(updateTask.ExecutedNodes.Concat(updateTask.ModifiedNodes));
            foreach (var executedNode in nodes)
            {
                executedNode.RequestValueUpdate(EngineController);
            }

            scheduler.Tasks.AllComplete(_ =>
            {
                OnRefreshCompleted(e);
            });
        }

        /// <summary>
        /// This method is typically called from the main application thread (as 
        /// a result of user actions such as button click or node UI changes) to
        /// schedule an update of the graph. This call may or may not represent 
        /// an actual update. In the event that the user action does not result 
        /// in actual graph update (e.g. moving of node on UI), the update task 
        /// will not be scheduled for execution.
        /// </summary>
        public void Run()
        {
            if (!RunSettings.RunEnabled) return;

            // ForceBlockRun (an internal flag) has higher priority than RunEnabled. Mostly used by the FileTrust feature.
            if (RunSettings.ForceBlockRun) return;

            graphExecuted = true;

            // When Dynamo is shut down, the workspace is cleared, which results
            // in Modified() being called. But, we don't want to run when we are
            // shutting down so we check whether an engine controller is available.
            if (this.EngineController == null)
            {
                return;
            }

            var traceData = PreloadedTraceData;
            if ((traceData != null) && traceData.Any())
            {
                // If we do have preloaded trace data, set it here first.
                var setTraceDataTask = new SetTraceDataAsyncTask(scheduler);
                if (setTraceDataTask.Initialize(EngineController, this))
                    scheduler.ScheduleForExecution(setTraceDataTask);
            }

            // If one or more custom node have been updated, make sure they
            // are compiled first before the home workspace gets evaluated.
            // 
            EngineController.ProcessPendingCustomNodeSyncData(scheduler);
            
            var task = new UpdateGraphAsyncTask(scheduler, verboseLogging);

            // Start of graph evaluation (includes task.Initialize)
            // Set these flags to false so that we do not get into infinite loop scenarios
            // if listeners of EvaluationStarted decide to call RequestRun
            OnEvaluationStarted(EventArgs.Empty);

            if (task.Initialize(EngineController, this))
            {
                task.Completed += OnUpdateGraphCompleted;

                // Reset node states
                foreach (var node in Nodes)
                {
                    node.WasInvolvedInExecution = false;
                }

                // After the first update graph task has been scheduled (i.e the workspace has been built for the first time),
                // we can set the silenceNodeModifications flag back to false.
                silenceNodeModifications = false;

                scheduler.ScheduleForExecution(task);
            }
            else
            {
                // Notify handlers that evaluation did not take place.
                var e = new EvaluationCompletedEventArgs(false);
                OnEvaluationCompleted(e);
            }
        }

        /// <summary>
        /// This function gets the set of nodes that will get executed in the next run.
        /// This function will be called when the nodes are modified or when showrunpreview is set
        /// the executing nodes will be sent via SetNodeDeltaState event.
        /// </summary>
        /// <param name="showRunPreview">This parameter controls the delta state computation </param>
        internal void GetExecutingNodes(bool showRunPreview)
        {
            //The Graph is executed and Show node execution is checked on the Settings menu
            if (graphExecuted && showRunPreview)
            {
                var task = new PreviewGraphAsyncTask(scheduler, VerboseLogging);

                if (task.Initialize(EngineController, this) != null)
                {
                    task.Completed += OnPreviewGraphCompleted;
                    scheduler.ScheduleForExecution(task);
                }
            }
            //Show node execution is checked but the graph has not RUN
            else
            {
                var deltaComputeStateArgs = new DeltaComputeStateEventArgs(new List<Guid>(), graphExecuted);
                OnSetNodeDeltaState(deltaComputeStateArgs); 
            }
        }

        private void OnPreviewGraphCompleted(AsyncTask asyncTask)
        {
            if (asyncTask is PreviewGraphAsyncTask updateTask)
            {
                var nodeGuids = updateTask.previewGraphData;
                var deltaComputeStateArgs = new DeltaComputeStateEventArgs(nodeGuids, graphExecuted);
                OnSetNodeDeltaState(deltaComputeStateArgs);
            }
        }


       
        #endregion

        /// <summary>
        /// Returns a list of ISerializable items which exist in the preloaded 
        /// trace data but do not exist in the current CallSite data.
        /// </summary>
        /// <returns></returns>
        internal IList<ISerializable> GetOrphanedSerializablesAndClearHistoricalTraceData()
        {
            var orphans = new List<ISerializable>();

            if (historicalTraceData == null) return orphans;

            // If a Guid exists in the historical trace data
            // and there is no corresponding node in the workspace
            // then add the serializables for that guid to the list of
            // orphans.

            foreach (var nodeData in historicalTraceData)
            {
                var nodeGuid = nodeData.Key;

                if (Nodes.All(n => n.GUID != nodeGuid))
                {
                    orphans.AddRange(nodeData.Value.SelectMany(CallSite.GetAllSerializablesFromSingleRunTraceData).ToList());
                }
            }

            // When reconciliation is complete, wipe the historical data.
            // This avoids this data being re-used after a future update.

            historicalTraceData = null;

            return orphans;
        }

        /// <summary>
        /// Recompile all CBNs in home workspace in a second pass after all other function definitions
        /// have been compiled. 
        /// Note: This method is intended to be called only during opening of an existing workspace.
        /// </summary>
        internal void ReCompileCodeBlockNodesForFunctionDefinitions()
        {
            var cbns = Nodes.OfType<CodeBlockNodeModel>();

            foreach(var cbn in cbns)
            {
                // Recompile rest of the code against function definitions for each CBN.
                cbn.ProcessCodeDirect(cbn.RecompileCodeBlockAST);
            }
            // This method is intended to be called only during opening of an existing workspace
            // and therefore if the workspace is set as dirty on account of CBN precompilation, 
            // the workspace should be reverted to a clean state as it's undesirable to have unsaved
            // changes for a workspace that is newly opened.
            HasUnsavedChanges = false;
        }

        internal bool TryGetMatchingWorkspaceData(string uniqueId, out Dictionary<string, string> data)
        {
            data = new Dictionary<string, string>();
            if (!ExtensionData.Any()) return false;

            var extensionData = ExtensionData.Where(x => x.ExtensionGuid == uniqueId)
                .FirstOrDefault();

            if (extensionData is null) return false;

            data = extensionData.Data;
            return true;
        }

        internal void UpdateExtensionData(string uniqueId, Dictionary<string, string> data)
        {
            var extensionData = ExtensionData.Where(x => x.ExtensionGuid == uniqueId)
                .FirstOrDefault();

            if (extensionData is null) return;

            extensionData.Data = data;
        }

        internal void CreateNewExtensionData(string uniqueId, string name, string version, Dictionary<string, string> data)
        {
            // TODO: Figure out how to add extension version when creating new ExtensionData 
            var extensionData = new ExtensionData(uniqueId, name, version, data);
            ExtensionData.Add(extensionData);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;

using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;
using Dynamo.Nodes;
using Dynamo.Engine;

using ProtoCore;
using ProtoCore.Namespace;

namespace Dynamo.Models
{
    public interface IEngineControllerManager
    {
        EngineController EngineController { get; }
    }

    public interface IHomeWorkspaceModel : IWorkspaceModel, IEngineControllerManager
    {
        IScheduler Scheduler { get; }
        event EventHandler<EvaluationStartedEventArgs> EvaluationStarted;
        event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;
        event EventHandler<EvaluationCompletedEventArgs> RefreshCompleted;
    }

    public class HomeWorkspaceModel : WorkspaceModel, IHomeWorkspaceModel, ITraceReconciliationProcessor
    {
        #region Class Data Members and Properties

        private PulseMaker pulseMaker;
        private readonly bool verboseLogging;
        private bool graphExecuted;
        private IEnumerable<KeyValuePair<Guid, List<string>>> historicalTraceData;

        /// <summary>
        /// The EngineController for this particular home workspace.
        /// </summary>
        public EngineController EngineController { get; private set; }

        /// <summary>
        /// The Scheduler for this particular 
        /// </summary>
        private DynamoScheduler scheduler;
        public IScheduler Scheduler { get { return scheduler;  } }

        /// <summary>
        ///     Flag specifying if this workspace is operating in "test mode".
        /// </summary>
        public bool IsTestMode { get; set; }

        /// <summary>
        ///     Indicates whether a run has completed successfully.   
        /// 
        ///     This flag is critical to ensuring that crashing run-auto files
        ///     are not left in run-auto upon reopening.  
        /// </summary>
        public bool HasRunWithoutCrash { get; private set; }

        /// <summary>
        ///     Before the Workspace has been built the first time, we do not respond to 
        ///     NodeModification events.
        /// </summary>
        private bool silenceNodeModifications = true;
   
        public readonly bool VerboseLogging;

        public readonly RunSettings RunSettings;

        /// <summary>
        /// Evaluation count is incremented whenever the graph is evaluated. 
        /// It is set to zero when the graph is Cleared.
        /// </summary>
        public long EvaluationCount { get; private set; }

        /// <summary>
        /// In near future, the file loading mechanism will be completely moved 
        /// into WorkspaceModel, that's the time we removed this property setter below.
        /// </summary>
        internal IEnumerable<KeyValuePair<Guid, List<string>>> PreloadedTraceData
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
        private IEnumerable<KeyValuePair<Guid, List<string>>> preloadedTraceData;

        public event EventHandler<EvaluationStartedEventArgs> EvaluationStarted;
        public virtual void OnEvaluationStarted(EvaluationStartedEventArgs e)
        {
            this.HasRunWithoutCrash = false;

            var handler = EvaluationStarted;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<EvaluationCompletedEventArgs> EvaluationCompleted;
        public virtual void OnEvaluationCompleted(EvaluationCompletedEventArgs e)
        {
            this.HasRunWithoutCrash = e.EvaluationSucceeded;

            var handler = EvaluationCompleted;
            if (handler != null) handler(this, e);

        }

        public event EventHandler<EvaluationCompletedEventArgs> RefreshCompleted;
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

        public HomeWorkspaceModel(EngineController engine, DynamoScheduler scheduler, 
            NodeFactory factory, bool verboseLogging, bool isTestMode, string fileName="")
            : this(
                engine,
                scheduler,
                factory,
                Enumerable.Empty<KeyValuePair<Guid, List<string>>>(),
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                Enumerable.Empty<PresetModel>(),
                new ElementResolver(),
                new WorkspaceInfo(){FileName = fileName, Name = "Home"},
                verboseLogging, 
                isTestMode) { }
       
        public HomeWorkspaceModel(
            EngineController engine, 
            DynamoScheduler scheduler, 
            NodeFactory factory,
            IEnumerable<KeyValuePair<Guid, List<string>>> traceData, 
            IEnumerable<NodeModel> e, 
            IEnumerable<NoteModel> n, 
            IEnumerable<AnnotationModel> a,
            IEnumerable<PresetModel> presets,
            ElementResolver resolver,
            WorkspaceInfo info, 
            bool verboseLogging,
            bool isTestMode)
            : base(e, n,a, info, factory,presets, resolver)
        {
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
            this.EngineController = engine;
            this.EngineController.MessageLogged += Log;
            EngineController.TraceReconciliationComplete += EngineControllerTraceReconciliationComplete;

            if (this.RunSettings.RunType == RunType.Periodic)
            {
                this.StartPeriodicEvaluation();
            }

            // The first time the preloaded trace data is set, we cache
            // the data as historical. This will be used after the initial
            // run of this workspace, when the PreloadedTraceData has been
            // nulled, to check for node deletions and reconcile the trace data.
            // We do a deep copy of this data because the PreloadedTraceData is
            // later set to null before the graph update.
            var copiedData = new List<KeyValuePair<Guid, List<string>>>();
            foreach (var kvp in PreloadedTraceData)
            {
                var strings = kvp.Value.Select(string.Copy).ToList();
                copiedData.Add(new KeyValuePair<Guid, List<string>>(kvp.Key, strings));
            }
            historicalTraceData = copiedData;
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (EngineController != null)
            {
                EngineController.TraceReconciliationComplete -= EngineControllerTraceReconciliationComplete;
                EngineController.MessageLogged -= Log;
                EngineController.LibraryServices.LibraryLoaded -= LibraryLoaded;
                EngineController.Dispose();
                EngineController = null;
            }

            if (scheduler != null)
            {
                scheduler.Shutdown();
                scheduler = null;
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
            // Mark all nodes as dirty so that AST for the whole graph will be
            // regenerated.
            MarkNodesAsModifiedAndRequestRun(Nodes);
        }

        /// <summary>
        ///     Invoked when a change to the workspace that requires re-execution
        ///     has taken place.  If in run-automatic, a new run will take place,
        ///     otherwise nothing will happen.
        /// </summary>
        protected override void RequestRun()
        {
            base.RequestRun();

            if (RunSettings.RunType != RunType.Manual)
            {
                Run();
            }
        }

        protected override void NodeModified(NodeModel node)
        {
            base.NodeModified(node);

            if (!silenceNodeModifications)
            {
                RequestRun();
            }
        }

        private void PulseMakerRunStarted()
        {
            var nodesToUpdate = Nodes.Where(n => n.CanUpdatePeriodically);
            MarkNodesAsModifiedAndRequestRun(nodesToUpdate, true);
        }

        #region Public Operational Methods

        /// <summary>
        ///     Clears this workspace of nodes, notes, and connectors.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            PreloadedTraceData = null;
            RunSettings.Reset();
            EvaluationCount = 0;
        }

        /// <summary>
        /// Start periodic evaluation using the currently set RunPeriod
        /// </summary>
        public void StartPeriodicEvaluation()
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
        public void StopPeriodicEvaluation()
        {
            if (pulseMaker == null || (pulseMaker.TimerPeriod == 0)) return;

            pulseMaker.RunStarted -= PulseMakerRunStarted;
            RefreshCompleted -= pulseMaker.OnRefreshCompleted;
            pulseMaker.Stop();
        }

        #endregion

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

        #region Evaluation

        /// <summary>
        /// Completely reset the runtime state of the EngineController and reload all loaded libraries
        /// excluding custom nodes.
        /// </summary>
        internal void ResetEngine(LibraryServices libraryServices, string geometryFactoryPath, bool markNodesAsDirty = false )
        {
            if (EngineController != null)
            {
                EngineController.MessageLogged -= Log;
                EngineController.Dispose();
                EngineController = null;
            }

            EngineController = new EngineController(
                libraryServices,
                geometryFactoryPath,
                this.verboseLogging);

            EngineController.MessageLogged += Log;

            if (markNodesAsDirty)
            {
                // Mark all nodes as dirty so that AST for the whole graph will be
                // regenerated.
                MarkNodesAsModifiedAndRequestRun(Nodes);
            } 
            else if (RunSettings.RunType == RunType.Automatic)
            {
                Run();
            }
        }

        /// <summary>
        /// Mark the input nodes as modified
        /// </summary>
        /// <param name="nodes">The nodes to modify</param>
        /// <param name="forceExecute">The argument to NodeModel.MarkNodeAsModified</param>
        public void MarkNodesAsModifiedAndRequestRun(IEnumerable<NodeModel> nodes, bool forceExecute = false)
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
                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            foreach (var warning in updateTask.BuildWarnings)
            {
                // If there is already runtime warnings for 
                // this node, then ignore the build warnings.
                if (messages.ContainsKey(warning.Key))
                    continue;

                var message = string.Join("\n", warning.Value.Select(w => w.Message));
                messages.Add(warning.Key, message);
            }

            var workspace = updateTask.TargetedWorkspace;
            foreach (var message in messages)
            {
                var guid = message.Key;
                var node = workspace.Nodes.FirstOrDefault(n => n.GUID == guid);
                if (node == null)
                    continue;

                node.Warning(message.Value); // Update node warning message.
            }

            // Notify listeners (optional) of completion.
            RunSettings.RunEnabled = true; // Re-enable 'Run' button.

            //set the node execution preview to false;
            OnSetNodeDeltaState(new DeltaComputeStateEventArgs(new List<Guid>(), graphExecuted));

            // This method is guaranteed to be called in the context of 
            // ISchedulerThread (for Revit's case, it is the idle thread).
            // Dispatch the failure message display for execution on UI thread.
            // 
            EvaluationCompletedEventArgs e = task.Exception == null || IsTestMode
                ? new EvaluationCompletedEventArgs(this, true, task.ExecutionEndTime.TickCount - task.ExecutionStartTime.TickCount)
                : new EvaluationCompletedEventArgs(this, true, task.ExecutionEndTime.TickCount - task.ExecutionStartTime.TickCount, task.Exception);

            EvaluationCount ++;

            OnEvaluationCompleted(e);

            if (EngineController.IsDisposed) return;

            EngineController.ReconcileTraceDataAndNotify();

            // Refresh values of nodes that took part in update.
            foreach (var modifiedNode in updateTask.ModifiedNodes)
            {
                modifiedNode.RequestValueUpdateAsync(scheduler, EngineController);
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
            if (task.Initialize(EngineController, this))
            {
                task.Completed += OnUpdateGraphCompleted;
                RunSettings.RunEnabled = false; // Disable 'Run' button.

                // Reset node states
                foreach (var node in Nodes)
                {
                    node.IsUpdated = false;
                }

                // The workspace has been built for the first time
                silenceNodeModifications = false;

                OnEvaluationStarted(new EvaluationStartedEventArgs(this));
                scheduler.ScheduleForExecution(task);
            }
            else
            {
                // Notify handlers that evaluation did not take place.
                var e = new EvaluationCompletedEventArgs(this, false, 0);
                OnEvaluationCompleted(e);
            }
        }

        /// <summary>
        /// This function gets the set of nodes that will get executed in the next run.
        /// This function will be called when the nodes are modified or when showrunpreview is set
        /// the executing nodes will be sent via SetNodeDeltaState event.
        /// </summary>
        /// <param name="showRunPreview">This parameter controls the delta state computation </param>
        public void GetExecutingNodes(bool showRunPreview)
        {
            var task = new PreviewGraphAsyncTask(scheduler, VerboseLogging);
                        
            //The Graph is executed and Show node execution is checked on the Settings menu
            if (graphExecuted && showRunPreview)
            {
                if (task.Initialize(EngineController, this) != null)
                {
                    task.Completed += OnPreviewGraphCompleted;
                    scheduler.ScheduleForExecution(task);
                }
            }
            //Show node exection is checked but the graph has not RUN
            else
            {
                var deltaComputeStateArgs = new DeltaComputeStateEventArgs(new List<Guid>(), graphExecuted);
                OnSetNodeDeltaState(deltaComputeStateArgs); 
            }
        }

        private void OnPreviewGraphCompleted(AsyncTask asyncTask)
        {
            var updateTask = asyncTask as PreviewGraphAsyncTask;
            if (updateTask != null)
            {
                var nodeGuids = updateTask.previewGraphData;
                var deltaComputeStateArgs = new DeltaComputeStateEventArgs(nodeGuids,graphExecuted);
                OnSetNodeDeltaState(deltaComputeStateArgs);               
            }            
        }
       
        #endregion

        #region Trace 

        /// <summary>
        /// Add trace data to the document before being saved
        /// </summary>
        /// <param name="document">The XmlDocument being saved</param>
        protected override void SaveCustomData(XmlDocument document)
        {
            var runtimeCore = EngineController.LiveRunnerRuntimeCore;

            if (document.DocumentElement == null)
            {
                const string message = "Workspace should have been saved before this";
                throw new InvalidOperationException(message);
            }

            try
            {
                if (runtimeCore == null) // No execution yet as of this point.
                    return;

                // Selecting all nodes that are either a DSFunction,
                // a DSVarArgFunction or a CodeBlockNodeModel into a list.
                var nodeGuids =
                    Nodes.Where(
                        n => n is DSFunction || n is DSVarArgFunction || n is CodeBlockNodeModel)
                        .Select(n => n.GUID);

                var nodeTraceDataList = runtimeCore.RuntimeData.GetTraceDataForNodes(nodeGuids, runtimeCore.DSExecutable);

                if (nodeTraceDataList.Any())
                    Dynamo.Nodes.Utilities.SaveTraceDataToXmlDocument(document, nodeTraceDataList);
            }
            catch (Exception exception)
            {
                // We'd prefer file saving process to not crash Dynamo,
                // otherwise user will lose the last hope in retaining data.
                Log(exception.Message);
                Log(exception.StackTrace);
            }
        }

        private void EngineControllerTraceReconciliationComplete(TraceReconciliationEventArgs obj)
        {
            Debug.WriteLine("TRACE RECONCILIATION: {0} total serializables were orphaned.", obj.CallsiteToOrphanMap.SelectMany(kvp => kvp.Value).Count());

            // The orphans will come back here as a dictionary of lists of ISerializables jeyed by their callsite id.
            // This dictionary gets redistributed into a dictionary keyed by the workspace id.

            var workspaceOrphanMap = new Dictionary<Guid, List<ISerializable>>();

            // Get the orphaned serializables to this workspace
            var wsOrphans = this.GetOrphanedSerializablesAndClearHistoricalTraceData().ToList();

            if (wsOrphans.Any())
            {
                if (!workspaceOrphanMap.ContainsKey(this.Guid))
                {
                    workspaceOrphanMap.Add(this.Guid, wsOrphans);
                }
                else
                {
                    workspaceOrphanMap[this.Guid].AddRange(wsOrphans);
                }
            }

            foreach (var kvp in obj.CallsiteToOrphanMap)
            {
                if (!kvp.Value.Any()) continue;

                var nodeGuid = this.EngineController.LiveRunnerRuntimeCore.RuntimeData.CallSiteToNodeMap[kvp.Key];

                // TODO: MAGN-7314
                // Find the owning workspace for a node.
                var ownedNode = this.Nodes.Any(n => n.GUID == nodeGuid);
                if (!ownedNode) continue;

                // Add the node's orphaned serializables to the workspace
                // orphan map.
                if (workspaceOrphanMap.ContainsKey(this.Guid))
                {
                    workspaceOrphanMap[this.Guid].AddRange(kvp.Value);
                }
                else
                {
                    workspaceOrphanMap.Add(this.Guid, kvp.Value);
                }
            }

            this.OnTraceReconciliationComplete(workspaceOrphanMap);
        }

        public virtual void OnTraceReconciliationComplete(Dictionary<Guid, List<ISerializable>> orphanedSerializables)
        {
            // Override in derived classes to deal with orphaned serializables.
        }

        /// <summary>
        /// Returns a list of ISerializable items which exist in the preloaded 
        /// trace data but do not exist in the current CallSite data.
        /// </summary>
        /// <returns></returns>
        public IList<ISerializable> GetOrphanedSerializablesAndClearHistoricalTraceData()
        {
            var orphans = new List<ISerializable>();

            if (historicalTraceData == null)
                return orphans;

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

        #endregion

    }
}

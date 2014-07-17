using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Utilities;

// TODO(LC)
// Use TSO ordering for Render manager
// Add render latency estimation metric
// Cleanup the RenderExce method

namespace Dynamo
{
#if false

    /// <summary>
    /// The description of a render request
    /// </summary>
    internal struct RenderTask
    {
        /// <summary>
        /// A list of the nodes for whom rendering is requested
        /// </summary>
        public List<NodeModel> NodesToUpdateRendering;

        /// <summary>
        /// The ID associated with this task
        /// </summary>
        public long TaskID;

        public RenderTask(List<NodeModel> nodesToUpdateRendering, long taskID)
        {
            NodesToUpdateRendering = nodesToUpdateRendering;
            this.TaskID = taskID;
        }
    }

    internal class RenderManager
    {
        private readonly DynamoController controller;
        private readonly Thread controllerThread;
        public RenderManager(DynamoController controller)
        {
            this.controller = controller;
            controllerThread = new Thread(RenderLoopController);
            controllerThread.Name = "RenderManager controller thread";
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }

        /// <summary>
        /// An event triggered on the completion of visualization update.
        /// </summary>
        public event RenderCompleteEventHandler RenderComplete;

        /// <summary>
        /// Called when the update of visualizations is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRenderComplete(object sender, RenderCompletionEventArgs e)
        {
            if (RenderComplete != null)
                RenderComplete(sender, e);
        }


        public event RenderFailedEventHandler RenderFailed;

        /// <summary>
        /// Called when the update of visualizations is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRenderFailed(object sender, RenderFailedEventArgs e)
        {
            if (RenderFailed != null)
                RenderFailed(sender, e);
        }



        private List<RenderTask> renderQueue = new List<RenderTask>();

        /// <summary>
        /// Schedule the render task specified for execution
        /// </summary>
        /// <param name="renderTask"></param>
        public void RequestRenderAsync(RenderTask renderTask)
        {
            //Simplist strategy for now, always render everything
            lock (renderQueue)
            {
                renderQueue.Add(renderTask);
            }
        }

        private void RenderLoopController()
        {
            while (true)
            {
                RenderTask task = new RenderTask();
                bool hasTask = false;

                lock (renderQueue)
                {
                    if (this.renderQueue.Count > 0)
                    {
                        task = renderQueue[0];
                        renderQueue.RemoveAt(0);
                        hasTask = true;
                    }
                }

                if (hasTask)
                {
                    Render(task.TaskID, task.NodesToUpdateRendering, false);
                }
                else
                {
                    //TODO(Luke): Fix this with a proper messaging loop rather than
                    //a polling model
                    Thread.Sleep(10);
                }


            }

        }

        /// <summary>
        /// Finds all nodes marked as upated in the graph and calls their,
        /// update methods in paralell.
        /// </summary>
        private void Render(long taskID, IEnumerable<NodeModel> toUpdate = null, bool incrementId = true)
        {
            if (controller == null)
                return;

            if (DynamoController.IsTestMode)
                RenderExec(toUpdate, incrementId, taskID);
            else
            {
                Thread currentRenderThread = new Thread( 
                    () => RenderExec(toUpdate, incrementId, taskID));
                currentRenderThread.Name = "Render thread";
                currentRenderThread.Start();
            }
        }

        private void RenderExec(IEnumerable<NodeModel> nodes, bool incrementId, long taskID)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                //If the the event arguments contains a list of nodes,
                //then update those nodes, otherwise process any nodes
                //that are marked for updating.

                var toUpdate = nodes ??
                               controller.DynamoModel.Nodes.Where(node => node.IsUpdated || node.RequiresRecalc);

                var nodeModels = toUpdate as IList<NodeModel> ?? toUpdate.ToList();
                if (!nodeModels.Any())
                    return;

                //TODO(Luke): Parrallel for once we're stable
                nodeModels.ToList().ForEach(RenderSpecificNodeSync);

                sw.Stop();
                Debug.WriteLine(string.Format("RENDER: {0} ellapsed for updating render packages.", sw.Elapsed));

                OnRenderComplete(this, new RenderCompletionEventArgs(taskID));

            }
            catch (Exception ex)
            {
                OnRenderFailed(this, new RenderFailedEventArgs(taskID));

                dynSettings.DynamoLogger.Log(ex);
            }
        }


        /// <summary>
        /// Internal method that synchronously updates a specific node
        /// </summary>
        /// <param name="node"></param>
        private void RenderSpecificNodeSync(NodeModel node)
        {
            node.UpdateRenderPackage();
        }

    }

#else

    /// <summary>
    /// The description of a render request
    /// </summary>
    internal class RenderTask
    {
        private long taskId = 0;
        private IEnumerable<NodeModel> nodesToRender = null;

        public RenderTask()
        {
        }

        public RenderTask(long taskId, IEnumerable<NodeModel> nodesToRender)
        {
            this.taskId = taskId;
            this.nodesToRender = nodesToRender;
        } 

        /// <summary>
        /// The ID associated with this render task
        /// </summary>
        internal long TaskId
        {
            get { return this.taskId; }
        }

        /// <summary>
        /// A list of the nodes for whom rendering is requested
        /// </summary>
        internal IEnumerable<NodeModel> NodesToRender
        {
            get { return nodesToRender; }
        }
    }

    /// <summary>
    /// The RenderManager class handles render requests from VisualizationManager, 
    /// schedule them to update render packages of nodes before sending them back 
    /// to VisualizationManager for display. If this class is modified, please be 
    /// sure that VisualizationManagerUITests are all passing.
    /// </summary>
    /// 
    class RenderManager
    {
        #region Private Class Data Members

        private readonly DynamoController controller = null;
        private readonly Thread backgroundRenderThread = null;
        private List<RenderTask> renderQueue = new List<RenderTask>();

        private readonly int ShutDownEventId = 0;
        private readonly int TaskAvailableEventId = 1;

        private ManualResetEvent[] waitHandles = new ManualResetEvent[]
        {
            new ManualResetEvent(false), // Shutdown event
            new ManualResetEvent(false)  // Task available event
        };

        #endregion

        #region Public Render Manager APIs and Events

        public RenderManager(DynamoController controller)
        {
            this.controller = controller;
            backgroundRenderThread = new Thread(RenderThreadProc);
            backgroundRenderThread.Name = "RenderManager background rendering thread";
            backgroundRenderThread.IsBackground = true;
            backgroundRenderThread.Start();
        }

        /// <summary>
        /// Schedule the render task specified for execution
        /// </summary>
        /// <param name="renderTask"></param>
        public void RequestRenderAsync(RenderTask renderTask)
        {
            if (DynamoController.IsTestMode)
            {
                // We can't go through the same queuing mechanism while the 
                // RenderManager is running in unit-test scenario since the 
                // completion event handler will never get to run on main thread 
                // (and therefore results will not be populated).
                // 
                this.RenderExec(renderTask, false);
                return;
            }

            // Simplist strategy for now, always render everything. Note that 
            // we set the "task available" event while still having the lock on 
            // renderQueue so that there won't be race condition between this 
            // call and the RenderThreadProc.
            lock (renderQueue)
            {
                renderQueue.Add(renderTask);
                waitHandles[TaskAvailableEventId].Set();
            }
        }

        /// <summary>
        /// Call this method to properly shutdown the main render thread. This 
        /// call will not return to the caller until the thread terminates.
        /// </summary>
        public void Shutdown()
        {
            waitHandles[ShutDownEventId].Set(); // Begin shutting down render thread.
            backgroundRenderThread.Join(); // Wait for thread to terminate.
        }

        /// <summary>
        /// An event triggered on the completion of visualization update.
        /// </summary>
        public event RenderCompleteEventHandler RenderComplete;

        /// <summary>
        /// An event triggered when visualization update has failed.
        /// </summary>
        public event RenderFailedEventHandler RenderFailed;

        #endregion

        /// <summary>
        /// Called when the update of visualizations is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyRenderComplete(RenderCompletionEventArgs e)
        {
            if (RenderComplete != null)
                RenderComplete(this, e);
        }

        /// <summary>
        /// Called when the update of visualizations is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyRenderFailed(RenderFailedEventArgs e)
        {
            if (RenderFailed != null)
                RenderFailed(this, e);
        }

        private void RenderThreadProc()
        {
            while (true)
            {
                int handleIndex = WaitHandle.WaitAny(waitHandles);
                if (handleIndex == ShutDownEventId)
                    break; // Requested to shutdown render thread loop.

                // If the thread loop gets here, it means a render task is queued
                // up in the render queue. Note that we do not reset the wait 
                // handle, because a new task may have been queued before we are 
                // done with the current task (in which case the render loop is 
                // to continue picking up the next render task after this one is 
                // done).

                RenderTask nextRenderTask = null;

                lock (renderQueue)
                {
                    if (this.renderQueue.Count > 0)
                    {
                        nextRenderTask = renderQueue[0];
                        renderQueue.RemoveAt(0);
                    }

                    // Reset the event while still having the lock on renderQueue.
                    // This way there won't be race condition between the main 
                    // thread (the caller that appends the new task) and the render
                    // thread (the one that dequeues a render task).
                    if (nextRenderTask == null)
                    {
                        // There isn't any render task, reset the wait handle.
                        waitHandles[TaskAvailableEventId].Reset();
                    }
                }

                if (nextRenderTask != null)
                    RenderExec(nextRenderTask, false);
            }
        }

        /// <summary>
        /// Finds all nodes that are marked updated in the graph and call their 
        /// UpdateRenderPackage method in parallel.
        /// </summary>
        /// <param name="renderTask">The RenderTask for this render pass.</param>
        /// <param name="incrementId">Set this parameter to true to increment the 
        /// task Id, or false otherwise. This parameter is not currently used.
        /// </param>
        /// 
        private void RenderExec(RenderTask renderTask, bool incrementId)
        {
            var nodes = renderTask.NodesToRender;
            var taskId = renderTask.TaskId;

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                // If the the event arguments contains a list of nodes, then 
                // update those nodes, otherwise process any nodes that are 
                // marked for updating.
                // 
                var toUpdate = nodes ?? controller.DynamoModel.Nodes.Where((n) =>
                {
                    return (n.IsUpdated || n.RequiresRecalc);
                });

                if (!toUpdate.Any()) // If there's nothing to update, we won't 
                    return;          // even notify the event listener(s).

                // TODO(Luke): Parrallel for once we're stable
                toUpdate.ToList().ForEach(n => n.UpdateRenderPackage());

                sw.Stop();
                Debug.WriteLine(string.Format("RENDER: {0} ellapsed for updating render packages.", sw.Elapsed));

                NotifyRenderComplete(new RenderCompletionEventArgs(taskId));
            }
            catch (Exception ex)
            {
                NotifyRenderFailed(new RenderFailedEventArgs(taskId));
                dynSettings.DynamoLogger.Log(ex);
            }
        }
    }

#endif
}

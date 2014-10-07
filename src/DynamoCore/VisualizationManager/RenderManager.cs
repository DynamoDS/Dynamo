#if !ENABLE_DYNAMO_SCHEDULER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dynamo.Models;

// TODO(LC)
// Use TSO ordering for Render vizManager
// Add render latency estimation metric
// Cleanup the RenderExce method

namespace Dynamo
{
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
        private readonly DynamoModel dynamoModel;
        private readonly VisualizationManager visualizationManager;
        private readonly Thread controllerThread;
        private bool isShuttingDown = false;

        public RenderManager(VisualizationManager vizManager, DynamoModel dynamoModel)
        {
            this.visualizationManager = vizManager;
            this.dynamoModel = dynamoModel;

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
            while (!this.isShuttingDown)
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
            if (dynamoModel == null)
                return;

            if (DynamoModel.IsTestMode)
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
                               dynamoModel.Nodes.Where(node => node.IsUpdated || node.RequiresRecalc);

                var nodeModels = toUpdate as IList<NodeModel> ?? toUpdate.ToList();
                if (!nodeModels.Any())
                    return;

                //TODO(Luke): Parrallel for once we're stable
                nodeModels.ToList().ForEach(x => RenderSpecificNodeSync(x, visualizationManager.MaxTesselationDivisions ));

                sw.Stop();
                Debug.WriteLine(string.Format("RENDER: {0} ellapsed for updating render packages.", sw.Elapsed));

                OnRenderComplete(this, new RenderCompletionEventArgs(taskID));

            }
            catch (Exception ex)
            {
                OnRenderFailed(this, new RenderFailedEventArgs(taskID));

                dynamoModel.Logger.Log(ex);
            }
        }


        /// <summary>
        /// Internal method that synchronously updates a specific node
        /// </summary>
        /// <param name="node"></param>
        private void RenderSpecificNodeSync(NodeModel node, int maxTesselationDivs)
        {
            node.UpdateRenderPackage(maxTesselationDivs);
        }

        public void CleanUp()
        {
            isShuttingDown = true;
            controllerThread.Join(); 
        }
    }
}

#endif

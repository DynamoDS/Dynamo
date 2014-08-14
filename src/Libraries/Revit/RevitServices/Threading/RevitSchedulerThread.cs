using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Dynamo.Core.Threading;
using Dynamo.Interfaces;

namespace RevitServices.Threading
{
    public class RevitSchedulerThread : ISchedulerThread
    {
        private DynamoScheduler scheduler;
        private readonly UIApplication revitApplication;

        public RevitSchedulerThread(UIApplication revitApplication)
        {
            this.revitApplication = revitApplication;
        }

        public void Initialize(DynamoScheduler owningScheduler)
        {
            scheduler = owningScheduler;
            revitApplication.Idling += OnRevitIdle; // Register idle handler.
        }

        public void Shutdown()
        {
            revitApplication.Idling -= OnRevitIdle; // Stop getting called.
        }

        /// <summary>
        /// When Revit goes idle it gets here and process all tasks in the scheduler 
        /// queue. The control returns to Revit only when all tasks are processed. 
        /// This method will be called again the next time Revit goes into idle state.
        /// </summary>
        /// <param name="sender">Reference to the current Revit application.</param>
        /// <param name="e">Idling event argument.</param>
        /// 
        private void OnRevitIdle(object sender, IdlingEventArgs e)
        {
            const bool waitIfTaskQueueIsEmpty = false;
            while (scheduler.ProcessNextTask(waitIfTaskQueueIsEmpty))
            {
                // Does nothing here, loop ends when all tasks processed.
            }
        }
    }
}

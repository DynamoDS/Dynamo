using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Analytics.NET.ADP;
using Autodesk.Analytics.Core;

namespace Dynamo.Logging
{
    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    class AnalyticsService
    {
        private static IAnalyticsUI adpAnalyticsUI = new ADPAnalyticsUI();

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        /// <param name="model">DynamoModel</param>
        internal static void Start(DynamoModel model)
        {
            var client = new DynamoAnalyticsClient(model);
            Analytics.Start(client);
            model.WorkspaceAdded += OnWorkspaceAdded;
        }

        static void OnWorkspaceAdded(WorkspaceModel obj)
        {
            if (obj is CustomNodeWorkspaceModel)
                Analytics.TrackScreenView("CustomWorkspace");
            else
                Analytics.TrackScreenView("Workspace");
        }

        /// <summary>
        /// Indicates whether the user has opted-in to ADP analytics.
        /// </summary>
        internal static bool IsADPOptedIn 
        {
            get
            {
                return adpAnalyticsUI.IsOptedIn();
            }
            set
            {
                adpAnalyticsUI.SetOptedIn(value);
            }
        }

        /// <summary>
        /// Shuts down the client. Application life cycle end is tracked.
        /// </summary>
        internal static void ShutDown()
        {
            Analytics.ShutDown();
        }
    }
}

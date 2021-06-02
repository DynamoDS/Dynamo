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
        private static ADPAnalyticsUI adpAnalyticsUI = new ADPAnalyticsUI();

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        /// <param name="model">DynamoModel</param>
        /// <param name="isHeadless">Analytics won't be started if IsHeadless, but ADP may be loaded to be disabled.</param>
        /// <param name="isTestMode">Analytics won't be started if isTestMode, ADP will not be loaded.</param>
        internal static void Start(DynamoModel model, bool isHeadless, bool isTestMode)
        {
            if (isHeadless)
            {
                return;
            }
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

        internal static bool DisableADPForProcessLifetime()
        {
            return adpAnalyticsUI.DisableADPForProcessLifetime();
        }

        internal static bool IsADPDisabledForProcessLifetime()
        {
            return adpAnalyticsUI.IsADPDisabledForProcessLifetime();
        }

        private static bool disableAnalytics;

        /// <summary>
        /// Disables all analytics collection (Google, ADP, etc.) for the lifetime of the process.
        /// </summary>
        public static bool DisableAnalytics
        {
            get
            {
                return disableAnalytics;
            }
            set
            {
                disableAnalytics = value;
                // Can fail in edge conditions but not mandatory to happen.
                DisableADPForProcessLifetime();
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

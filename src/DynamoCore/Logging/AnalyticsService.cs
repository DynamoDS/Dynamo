using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Autodesk.Analytics.ADP;
using Autodesk.Analytics.Core;
using System;

namespace Dynamo.Logging
{
    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    internal class AnalyticsService
    {
        // Use the Analytics.Core interface so that we do not have to load the ADP assembly at this time.
        private static IAnalyticsUI adpAnalyticsUI;

        /// <summary>
        /// Indicates that it is the Host's responsibility to shut down Analytics.
        /// Sometimes we want to keep Analytics service running even when we don't have a DynamoModel started.
        /// </summary>
        internal static bool ShutDownByHost { get; set; }

        /// <summary>
        /// Starts the Analytics client. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        internal static void Start()
        {
            // Initialize the concrete class only when we initialize the Service.
            // This will also load the Analytics.Net.ADP assembly
            // We must initialize the ADPAnalyticsUI instance before the Analytics.Start call.
            adpAnalyticsUI = new ADPAnalyticsUI();

            Analytics.Start(new DynamoAnalyticsClient(DynamoModel.HostAnalyticsInfo));
        }

        internal static void AddModelEvents(DynamoModel model)
        {
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
        /// As of ADP4 this will return true for most users.
        /// </summary>
        internal static bool IsADPOptedIn
        {
            get
            {
                if (Analytics.DisableAnalytics ||
                    adpAnalyticsUI == null)
                {
                    return false;
                }
                return adpAnalyticsUI.IsOptedIn(5,500);
            }
            
            set
            {
                if (Analytics.DisableAnalytics ||
                    adpAnalyticsUI == null)
                {
                    return;
                }

                adpAnalyticsUI.SetOptedIn(value);
            }
            
        }

        internal static bool IsADPAvailable()
        {
            if (Analytics.DisableAnalytics ||
                adpAnalyticsUI == null)
            {
                return false;
            }

            return adpAnalyticsUI.IsProviderAvailable();
        }

        /// <summary>
        /// Shuts down the client. Application life cycle end is tracked.
        /// </summary>
        internal static void ShutDown()
        {
            Analytics.ShutDown();
        }

        /// <summary>
        /// Show the ADP dynamic consents dialog.
        /// </summary>
        /// <param name="host">main window</param>
        internal static void ShowADPConsentDialog(IntPtr? host)
        {
            if (!Analytics.DisableAnalytics && adpAnalyticsUI != null)
            {
                adpAnalyticsUI.ShowOptInDialog(System.Threading.Thread.CurrentThread.CurrentUICulture.Name, false, host);
            }
        }

        internal static string GetUserIDForSession()
        {
            if (Analytics.client is DynamoAnalyticsClient dac)
            {
                return dac.Session?.UserId;
            }
            return null;
        }
    }
}

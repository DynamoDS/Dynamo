using Dynamo.Graph.Workspaces;
using Dynamo.Models;
using Analytics.NET.ADP;
using Autodesk.Analytics.Core;
using System;

namespace Dynamo.Logging
{
    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    class AnalyticsService
    {
        // Use the Analytics.Core interface so that we do not have to load the ADP assembly at this time.
        private static IAnalyticsUI adpAnalyticsUI;

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        /// <param name="model">DynamoModel</param>
        /// <param name="isHeadless">Analytics won't be started if IsHeadless, but ADP may be loaded to be disabled.</param>
        /// <param name="isTestMode">Analytics won't be started if isTestMode, ADP will not be loaded.</param>
        internal static void Start(DynamoModel model, bool isHeadless, bool isTestMode)
        {
            if (isTestMode)
            {
                if (Analytics.DisableAnalytics)
                {
                    model.Logger.Log("Incompatible configuration: [IsTestMode] and [Analytics disabled] ");
                }
                return;
            }

            if (isHeadless)
            {
                return;
            }

            // Initialize the concrete class only when we initialize the Service.
            // This will also load the Analytics.Net.ADP assembly
            // We must initialize the ADPAnalyticsUI instance before the Analytics.Start call.
            adpAnalyticsUI = new ADPAnalyticsUI();

            Analytics.Start(new DynamoAnalyticsClient(model));
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
        /// Use IsADPCollectionEnabled with a specific event to avoid unneccsary computations.
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
                //TODO add this overload to IAnalyticsUI.
                return (adpAnalyticsUI as ADPAnalyticsUI ).IsOptedIn(30000);
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
        public static void ShowADPConsetDialog(IntPtr? host)
        {
            adpAnalyticsUI.ShowOptInDialog(System.Threading.Thread.CurrentThread.CurrentUICulture.Name, false, host);
        }

        /// <summary>
        /// Check if this event will be logged to ADP, or if it will be filtered out
        /// by optin status or ADP allow listing.
        /// </summary>
        public static bool IsADPCollectionEnabled(Actions action, Categories category)
        {   
            if(adpAnalyticsUI != null)
            {
                return (adpAnalyticsUI as ADPAnalyticsUI).IsCollectionEnabled(action.ToString(), category.ToString());

            }
            return false;
        }
    }
}
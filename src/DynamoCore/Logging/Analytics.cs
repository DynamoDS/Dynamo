using System;
using System.Diagnostics;
using Analytics.NET.Google;
using Autodesk.Analytics.Core;
using Autodesk.Analytics.Events;
using Dynamo.Interfaces;
using Dynamo.Models;
using Microsoft.Win32;

namespace Dynamo.Logging
{
    /// <summary>
    /// Categories for analytics tracking.
    /// </summary>
    public enum Categories
    {
        ApplicationLifecycle,
        Stability,
        NodeOperations,
        Performance,
        SearchUX,
    }

    /// <summary>
    /// Actions for analytics tracking.
    /// </summary>
    public enum Actions
    {
        Start,
        End,
        Create,
        Delete,
        Move,
        Copy,
        EngineFailure,
        FilterButtonClicked,
    }

    /// <summary>
    /// Utility class to support analytics tracking.
    /// </summary>
    public class Analytics
    {
        private static Analytics instance = null;
        private Heartbeat heartbeat = null;
        private Log piiLogger = null;
        private IPreferences preferences = null;

#if DEBUG
        private const string ANALYTICS_PROPERTY = "UA-78361914-1";
#else
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
#endif

        /// <summary>
        /// Gets unique annonymous user id
        /// </summary>
        internal string UserId { get; private set; }

        /// <summary>
        /// Gets unique session id
        /// </summary>
        internal string SessionId { get; private set; }

        /// <summary>
        /// Checks if analytics tracking is enabled
        /// </summary>
        public static bool IsAnalyticsTrackingEnabled { get { return instance != null && instance.preferences.IsAnalyticsReportingApproved; } }

        /// <summary>
        /// Checks if usgae reporting is enabled.
        /// </summary>
        public static bool IsUsageTrackingEnabled { get { return instance != null && instance.preferences.IsUsageReportingApproved; } }

        private Analytics(string userId, DynamoModel dynamoModel)
        {
            UserId = userId;
            SessionId = Guid.NewGuid().ToString();
            preferences = dynamoModel.PreferenceSettings;
            heartbeat = Heartbeat.GetInstance(dynamoModel);

            piiLogger = new Log("Dynamo", userId, SessionId);
        }

        private void Dispose()
        {
            Heartbeat.DestroyInstance();
            heartbeat = null;

            piiLogger.Dispose();
            piiLogger = null;
        }

        internal static String GetUserID()
        {
            // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\DynamoUXG";
            const string keyName = userRoot + "\\" + subkey;

            // An int value can be stored without specifying the
            // registry data type, but long values will be stored
            // as strings unless you specify the type. Note that
            // the int is stored in the default name/value
            // pair.

            var tryGetValue = Registry.GetValue(keyName, "InstrumentationGUID", null) as string;

            if (tryGetValue != null)
            {
                Debug.WriteLine("User id found: " + tryGetValue);
                return tryGetValue;
            }

            String newGUID = Guid.NewGuid().ToString();
            Registry.SetValue(keyName, "InstrumentationGUID", newGUID);
            Debug.WriteLine("New User id: " + newGUID);
            return newGUID;
        }

        public static void Start(DynamoModel model, bool enable)
        {
            //Whether enabled or not, we still record the startup.
            Service.Instance.Register(new GATrackerFactory(ANALYTICS_PROPERTY));
            var userId = GetUserID();
            var appversion = model.AppVersion;

            //If not enabled set the idle time as infinite so idle state is not recorded.
            Service.StartUp(new ProductInfo() { Name = "Dynamo", VersionString = appversion },
                new UserInfo(userId), enable ? TimeSpan.FromMinutes(30) : TimeSpan.MaxValue);

            StabilityCookie.Startup();

            instance = new Analytics(userId, model);
        }

        public static void ShutDown()
        {
            //Are we shutting down clean if so write 'nice shutdown' cookie
            if (DynamoModel.IsCrashing)
                StabilityCookie.WriteCrashingShutdown();
            else
                StabilityCookie.WriteCleanShutdown();

            Service.ShutDown();
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }
        }

        public static void TrackEvent(Actions action, Categories category, string description = "", int? value = null)
        {
            if (!Analytics.IsAnalyticsTrackingEnabled)
                return;

            var e = AnalyticsEvent.Create(category.ToString(), action.ToString(), description, value);
            e.Track();
        }

        public static void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            if (!Analytics.IsAnalyticsTrackingEnabled) return;

            var e = new TimedEvent(time) { Category = category.ToString(), VariableName = variable, Description = description };
            e.Track();
        }

        public static void TrackScreenView(string viewName)
        {
            if (!Analytics.IsAnalyticsTrackingEnabled) return;

            var e = new ScreenViewEvent(viewName);
            e.Track();
        }

        public static void TrackException(Exception ex, bool isFatal)
        {
            //Continue recording exception in all scenarios.
            Service.TrackException(ex, isFatal);
        }

        public static void LogPiiInfo(string tag, string data)
        {
            if (!IsUsageTrackingEnabled) return;

            instance.piiLogger.Info(tag, data);
        }
    }
}

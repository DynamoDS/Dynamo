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
    class DynamoAnalyticsSession : IAnalyticsSession
    {
        private Heartbeat heartbeat;
        private UsageLog logger;

#if DEBUG
        private const string ANALYTICS_PROPERTY = "UA-78361914-2";
#else
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
#endif

        public DynamoAnalyticsSession()
        {
            UserId = GetUserID();
            SessionId = Guid.NewGuid().ToString();
        }

        public void Start(DynamoModel model)
        {
            //Whether enabled or not, we still record the startup.
            var service = Service.Instance;
            
            //Some clients such as Revit may allow start/close Dynamo multiple times
            //in the same session so register only if the factory is not registered.
            if(service.GetTrackerFactory(GATrackerFactory.Name) == null)
                service.Register(new GATrackerFactory(ANALYTICS_PROPERTY));

            StabilityCookie.Startup();

            heartbeat = Heartbeat.GetInstance(model);

            logger = new UsageLog("Dynamo", UserId, SessionId);
        }

        public void Dispose()
        {
            //Are we shutting down clean if so write 'nice shutdown' cookie
            if (DynamoModel.IsCrashing)
                StabilityCookie.WriteCrashingShutdown();
            else
                StabilityCookie.WriteCleanShutdown();

            Service.ShutDown();
            //Unregister the GATrackerFactory only after shutdown is recorded.
            //Unregister is required, so that the host app can re-start Analytics service.
            Service.Instance.Unregister(GATrackerFactory.Name);
            
            if (null != heartbeat)
                Heartbeat.DestroyInstance();
            heartbeat = null;

            if (null != logger)
                logger.Dispose();
            logger = null;
        }

        public ILogger Logger
        {
            get { return logger; }
        }

        public string UserId { get; private set; }

        public string SessionId { get; private set; }

        public static String GetUserID()
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
    }

    /// <summary>
    /// Dynamo specific implementation of IAnalyticsClient
    /// </summary>
    class DynamoAnalyticsClient : IAnalyticsClient, IDisposable
    {
        /// <summary>
        /// A dummy IDisposable class
        /// </summary>
        class Dummy : IDisposable
        {
            public void Dispose() { }
        }

        private IPreferences preferences = null;

        public static IDisposable Disposable { get { return new Dummy(); } }

        private ProductInfo product;

        public virtual IAnalyticsSession Session { get; private set; }

        public bool ReportingAnalytics
        {
            get { return preferences != null && preferences.IsAnalyticsReportingApproved; }
        }

        public bool ReportingUsage
        {
            get { return preferences != null && preferences.IsUsageReportingApproved; }
        }

        /// <summary>
        /// Constructs DynamoAnalyticsClient with given DynamoModel
        /// </summary>
        /// <param name="dynamoModel">DynamoModel</param>
        public DynamoAnalyticsClient(DynamoModel dynamoModel)
        {
            //Set the preferences, so that we can get live value of analytics 
            //reporting approved status.
            preferences = dynamoModel.PreferenceSettings;

            if (Session == null) Session = new DynamoAnalyticsSession();

            //Setup Analytics service, StabilityCookie, Heartbeat and UsageLog.
            Session.Start(dynamoModel);

            //Dynamo app version.
            var appversion = dynamoModel.AppVersion;

            product = new ProductInfo() { Name = "Dynamo", VersionString = appversion };
        }

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        public void Start()
        {
            //If not ReportingAnalytics, then set the idle time as infinite so idle state is not recorded.
            Service.StartUp(product,
                new UserInfo(Session.UserId), ReportingAnalytics ? TimeSpan.FromMinutes(30) : TimeSpan.MaxValue);

            TrackPreferenceInternal("ReportingAnalytics", "", ReportingAnalytics ? 1 : 0);
            TrackPreferenceInternal("ReportingUsage", "", ReportingUsage ? 1 : 0);
        }

        public void ShutDown()
        {
            Dispose();
        }

        public void TrackEvent(Actions action, Categories category, string description, int? value)
        {
            if (!ReportingAnalytics) return;

            var e = AnalyticsEvent.Create(category.ToString(), action.ToString(), description, value);
            e.Track();
        }

        public void TrackPreference(string name, string stringValue, int? metricValue)
        {
            if (ReportingAnalytics) TrackPreferenceInternal(name, stringValue, metricValue);
        }

        private void TrackPreferenceInternal(string name, string stringValue, int? metricValue)
        {
            var e = AnalyticsEvent.Create(Categories.Preferences.ToString(), name, stringValue, metricValue);
            e.Track();
        }

        public void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            if (!ReportingAnalytics) return;

            var e = new TimedEvent(time)
            {
                Category = category.ToString(),
                VariableName = variable,
                Description = description
            };
            e.Track();
        }

        public void TrackScreenView(string viewName)
        {
            if (!ReportingAnalytics) return;

            var e = new ScreenViewEvent(viewName);
            e.Track();
        }

        public void TrackException(Exception ex, bool isFatal)
        {
            //Continue recording exception in all scenarios.
            Service.TrackException(ex, isFatal);
        }

        public IDisposable CreateTimedEvent(Categories category, string variable, string description, int? value)
        {
            if (!ReportingAnalytics) return Disposable;

            var e = new TimedEvent()
            {
                Category = category.ToString(),
                VariableName = variable,
                Description = description,
                Value = value
            };
            //Timed event does not need startup tracking.
            return e;
        }

        public IDisposable CreateCommandEvent(string name, string description, int? value)
        {
            if (!ReportingAnalytics) return Disposable;

            var e = new CommandEvent(name) { Description = description, Value = value };
            e.Track();
            return e;
        }

        public IDisposable TrackFileOperationEvent(string filepath, Actions operation, int size, string description)
        {
            if (!ReportingAnalytics) return Disposable;

            var e = new FileOperationEvent()
            {
                FilePath = filepath,
                FileSize = size,
                FileAction = FileAction(operation),
                Description = description
            };
            e.Track();
            return e;
        }

        private FileOperationEvent.Actions FileAction(Actions operation)
        {
            switch (operation)
            {
                case Actions.Delete:
                    return FileOperationEvent.Actions.FileDelete;
                case Actions.Open:
                    return FileOperationEvent.Actions.FileOpen;
                case Actions.Close:
                    return FileOperationEvent.Actions.FileClose;
                case Actions.Read:
                    return FileOperationEvent.Actions.FileRead;
                case Actions.Write:
                    return FileOperationEvent.Actions.FileWrite;
                case Actions.Save:
                    return FileOperationEvent.Actions.FileSave;
                case Actions.SaveAs:
                    return FileOperationEvent.Actions.FileSaveAs;
                case Actions.New:
                    return FileOperationEvent.Actions.FileNew;
                default:
                    break;
            }
            throw new ArgumentException("Invalid action for FileOperation.");
        }

        public void LogPiiInfo(string tag, string data)
        {
            if (!ReportingUsage) return;

            if (Session != null && Session.Logger != null)
            {
                Session.Logger.Log(tag, data);
            }
        }

        public void Dispose()
        {
            if (Session != null)
            {
                Session.Dispose();
                Session = null;
            }
        }
    }
}

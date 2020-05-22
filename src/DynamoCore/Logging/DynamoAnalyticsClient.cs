using System;
using System.Diagnostics;
using Analytics.NET.Google;
using Analytics.NET.ADP;
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

        public DynamoAnalyticsSession()
        {
            UserId = GetUserID();
            SessionId = Guid.NewGuid().ToString();
        }

        public void Start(DynamoModel model)
        {
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

#if DEBUG
        private const string ANALYTICS_PROPERTY = "UA-78361914-2";
#else
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
#endif

        private IPreferences preferences = null;

        public static IDisposable Disposable { get { return new Dummy(); } }

        private ProductInfo product;

        public virtual IAnalyticsSession Session { get; private set; }

        /// <summary>
        /// Return if Analytics Client is allowed to send any analytics information (Google, ADP etc.)
        /// </summary>
        public bool ReportingAnalytics
        {
            get
            {
                return Service.IsInitialized && (ReportingGoogleAnalytics || ReportingADPAnalytics);
            }
        }

        /// <summary>
        /// Return if Google Analytics Client is allowed to send analytics info
        /// </summary>
        private bool ReportingGoogleAnalytics
        {
            get
            {
                return preferences != null
                    && Service.IsInitialized
                    && preferences.IsAnalyticsReportingApproved;
            }
        }

        /// <summary>
        /// Return if ADP Analytics Client is allowed to send analytics info
        /// </summary>
        private bool ReportingADPAnalytics
        {
            get 
            {
                return preferences != null
                    && Service.IsInitialized
                    && preferences.IsADPAnalyticsReportingApproved; 
            }
        }

        /// <summary>
        /// Return if Analytics Client is allowed to send instrumentation info
        /// </summary>
        public bool ReportingUsage
        {
            get { return preferences != null
                    && Service.IsInitialized
                    && preferences.IsUsageReportingApproved; }
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
            
            var hostName = string.IsNullOrEmpty(dynamoModel.HostName) ? "Dynamo" : dynamoModel.HostName;

            string buildId = "", releaseId = "";
            Version version;
            if (Version.TryParse(dynamoModel.Version, out version))
            {
                buildId = $"{version.Major}.{version.Minor}.{version.Build}"; // BuildId has the following format major.minor.build, ex: 2.5.1
                releaseId = $"{version.Major}.{version.Minor}.0"; // ReleaseId has the following format: major.minor.0; ex: 2.5.0
            }
            product = new ProductInfo() { Id = "DYN", Name = hostName, VersionString = appversion, AppVersion = appversion, BuildId = buildId, ReleaseId = releaseId };
        }

        private void RegisterGATracker(Service service)
        {
            //Some clients such as Revit may allow start/close Dynamo multiple times
            //in the same session so register only if the factory is not registered.
            if (service.GetTrackerFactory(GATrackerFactory.Name) == null)
                service.Register(new GATrackerFactory(ANALYTICS_PROPERTY));

            Service.Instance.AddTrackerFactoryFilter(GATrackerFactory.Name, () => ReportingGoogleAnalytics);
        }

        private void RegisterADPTracker(Service service)
        {
            //Some clients such as Revit may allow start/close Dynamo multiple times
            //in the same session so register only if the factory is not registered.
            if (service.GetTrackerFactory(ADPTrackerFactory.Name) == null)
                service.Register(new ADPTrackerFactory());

            Service.Instance.AddTrackerFactoryFilter(ADPTrackerFactory.Name, () => ReportingADPAnalytics);
        }

        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        public void Start()
        {
            if (preferences != null && 
                (preferences.IsAnalyticsReportingApproved || preferences.IsADPAnalyticsReportingApproved))
            {
                //Register trackers
                var service = Service.Instance;

                // Use separate functions to avoid loading the tracker dlls if they are not opted in (as an extra safety measure).
                // ADP will be loaded because opt-in/opt-out is handled/serialized exclusively by the ADP module.
                
                // Register Google Tracker only if the user is opted in.
                if (preferences.IsAnalyticsReportingApproved)
                    RegisterGATracker(service);

                // Register ADP Tracker only if the user is opted in.
                if (preferences.IsADPAnalyticsReportingApproved)
                    RegisterADPTracker(service);

                //If not ReportingAnalytics, then set the idle time as infinite so idle state is not recorded.
                Service.StartUp(product, new UserInfo(Session.UserId), TimeSpan.FromMinutes(30));
                TrackPreferenceInternal("ReportingAnalytics", "", ReportingAnalytics ? 1 : 0);
                TrackPreferenceInternal("ReportingADPAnalytics", "", ReportingADPAnalytics ? 1 : 0);
            }
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
            if (!ReportingAnalytics) return;

            TrackPreferenceInternal(name, stringValue, metricValue);
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
            if (!ReportingAnalytics) return;

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
            // If the Analytics Client was initialized, shut it down.
            // Otherwise skip this step because it would cause an exception.
            if (Service.IsInitialized)
                Service.ShutDown();

            if (Session != null)
            {
                Session.Dispose();
                Session = null;
            }
        }
    }
}

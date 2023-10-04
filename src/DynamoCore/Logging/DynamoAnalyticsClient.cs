using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Analytics.ADP;
using Autodesk.Analytics.Core;
using Autodesk.Analytics.Events;
using Dynamo.Interfaces;
using Dynamo.Models;
using Microsoft.Win32;

namespace Dynamo.Logging
{
    class DynamoAnalyticsSession : IAnalyticsSession
    {
        public DynamoAnalyticsSession()
        {
            UserId = GetUserID();
            SessionId = Guid.NewGuid().ToString();
        }

        public void Start()
        {
            StabilityCookie.Startup();
        }

        public void Dispose()
        {
            //Are we shutting down clean if so write 'nice shutdown' cookie
            if (DynamoModel.IsCrashing)
                StabilityCookie.WriteCrashingShutdown();
            else
                StabilityCookie.WriteCleanShutdown();
        }

        public string UserId { get; private set; }

        public string SessionId { get; private set; }
        [Obsolete("Do not use, will be removed, was only used by legacy instrumentation.")]
        public ILogger Logger => throw new NotImplementedException();

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
                Debug.WriteLine("Unique User id for Analytics found: " + tryGetValue);
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
        private readonly ManualResetEventSlim waitInit = new ManualResetEventSlim(false);
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

        private readonly IPreferences preferences = null;

        public static IDisposable Disposable { get { return new Dummy(); } }

        private readonly ProductInfo product;

        private readonly HostContextInfo hostInfo;

        public virtual IAnalyticsSession Session { get; private set; }

        /// <summary>
        /// Return if Analytics Client is allowed to send any analytics information
        /// </summary>
        public bool ReportingAnalytics
        {
            get
            {
                return !Analytics.DisableAnalytics &&
                    Service.IsInitialized;
            }
        }

        /// <summary>
        /// Return if Analytics Client is allowed to send instrumentation info
        /// </summary>
        public bool ReportingUsage
        {
            get
            {
                return preferences != null
                  && Service.IsInitialized
                  && !Analytics.DisableAnalytics
                  && preferences.IsUsageReportingApproved;
            }
        }

        /// <summary>
        /// Constructs DynamoAnalyticsClient for the current host.
        /// </summary>
        public DynamoAnalyticsClient()
        {
            //Set the preferences, so that we can get live value of analytics 
            //reporting approved status.
            preferences = Configuration.PreferenceSettings.Instance;

            if (Session == null) Session = new DynamoAnalyticsSession();

            //Setup Analytics service, and StabilityCookie.
            Session.Start();

            //Dynamo app version.
            var appversion = DynamoModel.AppVersion;

            var hostName = string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName) ? "Dynamo" : DynamoModel.HostAnalyticsInfo.HostName;

            hostInfo = new HostContextInfo() { ParentId = DynamoModel.HostAnalyticsInfo.ParentId, SessionId = DynamoModel.HostAnalyticsInfo.SessionId };

            string buildId = String.Empty, releaseId = String.Empty;
            if (Version.TryParse(DynamoModel.Version, out Version version))
            {
                buildId = $"{version.Major}.{version.Minor}.{version.Build}"; // BuildId has the following format major.minor.build, ex: 2.5.1
                releaseId = $"{version.Major}.{version.Minor}.0"; // ReleaseId has the following format: major.minor.0; ex: 2.5.0
            }
            product = new ProductInfo() { Id = "DYN", Name = hostName, VersionString = appversion, AppVersion = appversion, BuildId = buildId, ReleaseId = releaseId };
        }

        private void RegisterADPTracker(Service service)
        {
            //Some clients such as Revit may allow start/close Dynamo multiple times
            //in the same session so register only if the factory is not registered.
            if (service.GetTrackerFactory(ADPTrackerFactory.Name) == null)
            {
                service.Register(new ADPTrackerFactory());
                service.AddTrackerFactoryFilter(ADPTrackerFactory.Name, () => ReportingAnalytics);
            }
        }

        private void StartInternal()
        {
            if (preferences != null && !Analytics.DisableAnalytics)
            {
                //Register trackers
                var service = Service.Instance;

                // Always register ADP Tracker.
                // ADP manages opt in status internally.
                RegisterADPTracker(service);

                //If not ReportingAnalytics, then set the idle time as infinite so idle state is not recorded.
                Service.StartUp(product, new UserInfo(Session.UserId), hostInfo, TimeSpan.FromMinutes(30));
            }

            waitInit.Set();
        }
        /// <summary>
        /// Starts the client when DynamoModel is created. This method initializes
        /// the Analytics service and application life cycle start is tracked.
        /// </summary>
        public void Start()
        {
            // Start Analytics service regardless of optin status.
            // Each track event will be enabled/disabled based on the corresponding optin status.
            // Ex. ADP will manage optin status internally
            Task.Run(() => StartInternal());

            TrackPreferenceInternal("ReportingAnalytics", "", ReportingAnalytics ? 1 : 0);
        }

        public void ShutDown()
        {
            waitInit.Wait();
            Dispose();
        }

        public void TrackEvent(Actions action, Categories category, string description, int? value)
        {
            Task.Run(() =>
            {
                waitInit.Wait();
                {
                    if (!ReportingAnalytics) return;

                    var e = AnalyticsEvent.Create(category.ToString(), action.ToString(), description, value);
                    e.Track();
                }
            });
        }

        public void TrackPreference(string name, string stringValue, int? metricValue)
        {
            Task.Run(() =>
            {
                waitInit.Wait();
                {
                    if (!ReportingAnalytics) return;

                    TrackPreferenceInternal(name, stringValue, metricValue);
                }
            });
        }

        private void TrackPreferenceInternal(string name, string stringValue, int? metricValue)
        {
            Task.Run(() =>
            {
                waitInit.Wait();
                var e = AnalyticsEvent.Create(Categories.Preferences.ToString(), name, stringValue, metricValue);
                e.Track();                
            });
        }

        public void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            Task.Run(() =>
            {
                waitInit.Wait();
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
            });
        }

        public void TrackScreenView(string viewName)
        {
            Task.Run(() =>
            {
                waitInit.Wait();
                {
                    if (!ReportingAnalytics) return;

                    var e = new ScreenViewEvent(viewName);
                    e.Track();
                }
            });
        }

        public void TrackException(Exception ex, bool isFatal)
        {
            Task.Run(() =>
            {
                waitInit.Wait();
                {
                    if (!ReportingAnalytics) return;

                    Service.TrackException(ex, isFatal);
                }
            });
        }

        [Obsolete("Method will become private in Dynamo 3.0, please use CreateTaskTimedEvent")]
        public IDisposable CreateTimedEvent(Categories category, string variable, string description, int? value)
        {
            waitInit.Wait();                
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

        public Task<IDisposable> CreateTaskTimedEvent(Categories category, string variable, string description, int? value)
        {
            return Task.Run(() => CreateTimedEvent(category, variable, description, value));
        }

        [Obsolete("Property will become private in Dynamo 3.0, please use CreateTaskCommandEvent")]
        public IDisposable CreateCommandEvent(string name, string description, int? value)
        {
            waitInit.Wait();

            if (!ReportingAnalytics) return Disposable;

            var e = new CommandEvent(name) { Description = description, Value = value };
            e.Track();
            return e;
        }

        public Task<IDisposable> CreateTaskCommandEvent(string name, string description, int? value)
        {
            return Task.Run(() => CreateCommandEvent(name, description, value));
        }

        public void EndEventTask(Task<IDisposable> taskToEnd)
        {
            Task.Run(() =>
            {
                taskToEnd.Wait();
                taskToEnd.Result.Dispose();
            });
        }

        [Obsolete("Property will become private in Dynamo 3.0, please use TrackTaskFileOperationEvent")]
        public IDisposable TrackFileOperationEvent(string filepath, Actions operation, int size, string description)
        {
            waitInit.Wait();
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

        public Task<IDisposable> TrackTaskFileOperationEvent(string filepath, Actions operation, int size, string description)
        {
            return Task.Run(() => TrackFileOperationEvent(filepath, operation, size, description));
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

        [Obsolete("Function will be removed in Dynamo 3.0 as Dynamo will no longer support GA instrumentation.")]
        public void LogPiiInfo(string tag, string data)
        {
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

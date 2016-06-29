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
        private Heartbeat heartbeat = null;
        private Log piiLogger = null;

#if DEBUG
        private const string ANALYTICS_PROPERTY = "UA-78361914-1";
#else
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
#endif
        public static IDisposable Disposable { get { return new Dummy(); } }

        public DynamoAnalyticsClient()
        {
            UserId = GetUserID();
            SessionId = Guid.NewGuid().ToString();
        }

        public string UserId { get; private set; }

        public string SessionId { get; private set; }

        public bool ReportingAnalytics
        {
            get { return preferences != null && preferences.IsAnalyticsReportingApproved; }
        }

        public bool ReportingUsage
        {
            get { return preferences != null && preferences.IsUsageReportingApproved; }
        }

        public void Start(DynamoModel dynamoModel)
        {
            //Whether enabled or not, we still record the startup.
            Service.Instance.Register(new GATrackerFactory(ANALYTICS_PROPERTY));
            var appversion = dynamoModel.AppVersion;
            preferences = dynamoModel.PreferenceSettings;

            //If not enabled set the idle time as infinite so idle state is not recorded.
            Service.StartUp(new ProductInfo() { Name = "Dynamo", VersionString = appversion },
                new UserInfo(UserId), ReportingAnalytics ? TimeSpan.FromMinutes(30) : TimeSpan.MaxValue);

            StabilityCookie.Startup();

            heartbeat = Heartbeat.GetInstance(dynamoModel);

            piiLogger = new Log("Dynamo", UserId, SessionId);
        }

        public void ShutDown()
        {
            //Are we shutting down clean if so write 'nice shutdown' cookie
            if (DynamoModel.IsCrashing)
                StabilityCookie.WriteCrashingShutdown();
            else
                StabilityCookie.WriteCleanShutdown();

            Service.ShutDown();

            Dispose();
        }

        public void TrackEvent(Actions action, Categories category, string description, int? value)
        {
            if (!ReportingAnalytics) return;

            var e = AnalyticsEvent.Create(category.ToString(), action.ToString(), description, value);
            e.Track();
        }

        public void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            if (!ReportingAnalytics) return;

            var e = new TimedEvent(time) { Category = category.ToString(), VariableName = variable, Description = description };
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

        public IDisposable CreateTimedEvent(Categories category, string variable, string description)
        {
            if (!ReportingAnalytics) return Disposable;

            return new TimedEvent() { Category = category.ToString(), VariableName = variable, Description = description };
        }

        public IDisposable CreateCommandEvent(string name)
        {
            if (!ReportingAnalytics) return Disposable;

            return new CommandEvent(name);
        }

        public IDisposable CreateFileOperationEvent(string filepath, Actions operation, int size)
        {
            if (!ReportingAnalytics) return Disposable;

            return new FileOperationEvent() { FilePath = filepath, FileSize = size, FileAction = FileAction(operation) };
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

            piiLogger.Info(tag, data);
        }

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

        public void Dispose()
        {
            if (null != heartbeat)
                Heartbeat.DestroyInstance();
            heartbeat = null;

            if (null != piiLogger)
                piiLogger.Dispose();
            piiLogger = null;
        }
    }
}

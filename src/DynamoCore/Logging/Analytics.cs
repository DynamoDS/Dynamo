using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Analytics.NET.Google;
using Autodesk.Analytics.Core;
using Autodesk.Analytics.Events;
using Dynamo.Models;
using Microsoft.Win32;

namespace Dynamo.Logging
{
    public enum Categories
    {
        ApplicationLifecycle,
        Stability,
        NodeOperations,
        Performance,
        SearchUX,
    }

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

    class Analytics
    {
        private static Analytics instance = null;
        private string userId;

#if DEBUG
        private const string ANALYTICS_PROPERTY = "UA-78361914-1";
#else
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
#endif

        public static bool Enabled { get { return instance != null; } }
        
        private Analytics(string user) 
        {
            userId = user;
        }

        private static String GetUserID()
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

            if (enable)
            {
                instance = new Analytics(userId);
            }
        }

        public static void ShutDown()
        {
            Service.ShutDown();
        }

        public static void TrackEvent(Actions action, Categories category, string description = "", int? value = null)
        {
            if(!Analytics.Enabled)
                return;

            var e = AnalyticsEvent.Create(category.ToString(), action.ToString(), description, value);
            e.Track();
        }

        public static void TrackTimedEvent(Categories category, string variable, TimeSpan time, string description = "")
        {
            if (!Analytics.Enabled) return;

            var e = new TimedEvent(time) { Category = category.ToString(), VariableName = variable, Description = description };
            e.Track();
        }

        public static void TrackScreenView(string viewName)
        {
            if (!Analytics.Enabled) return;

            var e = new ScreenViewEvent(viewName);
            e.Track();
        }

        public static void TrackException(Exception ex, bool isFatal)
        {
            //Continue recording exception in all scenarios.
            Service.TrackException(ex, isFatal);
        }
    }
}

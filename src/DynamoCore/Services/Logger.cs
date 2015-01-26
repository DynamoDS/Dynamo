﻿using System;
using System.Diagnostics;

using CSharpAnalytics;
using CSharpAnalytics.Protocols.Measurement;

using Dynamo.Models;

using Microsoft.Win32;
using net.riversofdata.dhlogger;

namespace Dynamo.Services
{
    /// <summary>
    /// Interception class to handle whether logging is called or not
    /// </summary>
    public class InstrumentationLogger
    {
        private const bool IS_VERBOSE_DIAGNOSTICS = false;

        private static readonly string userID = GetUserID();
        private static string sessionID = Guid.NewGuid().ToString();
        private static Log loggerImpl;
        private static DynamoModel dynamoModel;

        //Analytics components
        private const string ANALYTICS_PROPERTY = "UA-52186525-1";
        private static MeasurementAnalyticsClient client;

        private static bool started = false;

        static InstrumentationLogger()
        {
            userID = GetUserID();
            StabilityTracking.GetInstance();
        }

        //Service start
        public static void Start(DynamoModel dynamoModel)
        {
            InstrumentationLogger.dynamoModel = dynamoModel;

            if (IsAnalyticsEnabled)
            {
                string appVersion = dynamoModel.AppVersion;

                var mc = new MeasurementConfiguration(ANALYTICS_PROPERTY,
                    "Dynamo", appVersion);

                sessionID = Guid.NewGuid().ToString();
                loggerImpl = new Log("Dynamo", userID, sessionID);

            
                AutoMeasurement.Start(mc);
                client = AutoMeasurement.Client;

                if (IS_VERBOSE_DIAGNOSTICS)
                    AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);

                started = true;
            }


            // The following starts the heartbeat, do not remove this 
            // because of the unreferenced "heartbeat" variable.

// ReSharper disable UnusedVariable
            var heartbeat = Heartbeat.GetInstance(dynamoModel);
// ReSharper restore UnusedVariable

        }

        public static void End()
        {
            // Heartbeat internally refers to the InstrumentationLogger (hence,
            // the "loggerImpl"), so we must destroy the heartbeat thread before
            // the rest of clean-up happens.
            Heartbeat.DestroyInstance();

            sessionID = null;

            if (loggerImpl != null)
            {
                loggerImpl.Dispose();
                loggerImpl = null;
            }

            started = false;
        }

        private static bool IsAnalyticsEnabled
        {
            get
            {
                if (DynamoModel.IsTestMode)
                    return false;

                if (dynamoModel != null)
                    return dynamoModel.PreferenceSettings.IsAnalyticsReportingApproved;

                return false;
            }

        }

        private static bool IsPIILoggingEnabled
        {
            get
            {
                if (DynamoModel.IsTestMode) // Do not want logging in unit tests.
                    return false;

                if (dynamoModel != null)
                    return dynamoModel.PreferenceSettings.IsUsageReportingApproved;

                return false;
            }
        }

        public static bool IsTestMode { get; set; }

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



        #region Analytics only methods
        
        public static void LogAnonymousTimedEvent(string category, string variable, TimeSpan time, string label = null)
        {
            if (IsTestMode)
                return;

            if (!IsAnalyticsEnabled)
                return;

            if (!started)
                return;

            client.TrackTimedEvent(category, variable, time, label);
        }

        public static void LogAnonymousEvent(string action, string category, string label = null)
        {
            if (IsTestMode)
                return;

            if (!IsAnalyticsEnabled)
                return;

            if (!started)
                return;

            AutoMeasurement.Client.TrackEvent(action, category, label);
        }

        public static void LogAnonymousScreen(string screenName)
        {
            if (IsTestMode)
                return;

            if (!IsAnalyticsEnabled)
                return;

            if (!started)
                return;

            AutoMeasurement.Client.TrackScreenView(screenName);
        }

        #endregion

        public static void LogException(Exception e)
        {
            if (IsTestMode)
                return;

            if (!started)
                return;

            if (IsAnalyticsEnabled)
            {
                //Log anonymous version
                AutoMeasurement.Client.TrackException(e.GetType().ToString());
            }

            //Protect PII
            if (!IsPIILoggingEnabled)
                return;

            //Log PII containing version
            loggerImpl.Error("StackTrace", e.ToString());
        }

        public static void FORCE_LogInfo(string tag, string data)
        {
            if (IsTestMode)
                return;

            if (!started)
                return;

            loggerImpl.Info(tag, data);
        }

        public static void LogPiiInfo(string tag, string data)
        {
            if (IsTestMode)
                return;

            if (!started)
                return;

            if (!IsPIILoggingEnabled)
                return;

            loggerImpl.Info(tag, data);
        }

    }

}

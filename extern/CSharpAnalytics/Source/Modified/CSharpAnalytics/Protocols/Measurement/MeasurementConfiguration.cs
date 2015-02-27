﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpAnalytics
{
    /// <summary>
    /// Configuration settings for analytics.
    /// </summary>
    public class MeasurementConfiguration
    {
        private static readonly Regex accountIdMatch = new Regex(@"^UA-\d+-\d+$");

        private readonly string accountId;
        private readonly string applicationName;
        private readonly string applicationVersion;

        private double sampleRate = 100.0; // Track all visitors by default

        /// <summary>
        /// Google Analytics provided property id in the format UA-XXXX-Y.
        /// </summary>
        public string AccountId { get { return accountId; } }

        /// <summary>
        /// Name of the application.
        /// </summary>
        public string ApplicationName { get { return applicationName; } }

        /// <summary>
        /// Version of the application.
        /// </summary>
        public string ApplicationVersion { get { return applicationVersion; } }

        /// <summary>
        /// Anonymize the tracking by scrubbing the last octet of the IP address.
        /// </summary>
        public bool AnonymizeIp { get; set; }

        /// <summary>
        /// Send analytics requests over HTTPS/SSL if true, over HTTP if not.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Sample rate percentage to determine how likely new visitors will be tracked.
        /// </summary>
        /// <remarks>
        /// Defaults to 100 meaning all visitors will be tracked.
        /// Setting this to other values means you need to manually adjust all figures in
        /// the analytics reports. If you are going to change the value you should bump the
        /// app version number and be prepared to run reports multiple times, adjust each reports
        /// figures for that revisions sample rate and then combine them together.
        /// Only use this if you really need sample rates because you will have too much data
        /// for your analytics account to handle otherwise.
        /// </remarks>
        public double SampleRate
        {
            get { return sampleRate; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value", "SampleRate is a percentage and must be between 0 and 100");
                sampleRate = value;
            }
        }

        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">Version of the application.</param>
        public MeasurementConfiguration(string accountId, string applicationName, string applicationVersion)
        {
            if (!accountIdMatch.IsMatch(accountId))
                throw new ArgumentException("accountID must be in the format UA-XXXX-Y.");

            this.accountId = accountId;
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            AnonymizeIp = true;
        }

#if WINDOWS_STORE
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        public MeasurementConfiguration(string accountId)
            : this(accountId, Windows.ApplicationModel.Package.Current.Id.Name, FormatVersion(Windows.ApplicationModel.Package.Current.Id.Version))
        {
        }

        /// <summary>
        /// Format the application version number to be sent to analytics.
        /// </summary>
        /// <param name="version">Version of the application.</param>
        /// <returns>Formatted string containing the version number.</returns>
        internal static string FormatVersion(Windows.ApplicationModel.PackageVersion version)
        {
            return String.Join(".", version.Major, version.Minor, version.Build, version.Revision);
        }
#endif

#if WINFORMS
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        public MeasurementConfiguration(string accountId)
            : this(accountId,
                    GetAttribute<System.Reflection.AssemblyTitleAttribute>(t => t.Title),
                    GetAttribute<System.Reflection.AssemblyVersionAttribute>(v => v.Version))
        {
        }

        private static string GetAttribute<T>(Func<T, string> selector) where T : Attribute
        {
            var attribute = System.Reflection.Assembly.GetEntryAssembly()
                .GetCustomAttributes(true)
                .OfType<T>()
                .FirstOrDefault();

            return attribute == null
                ? ""
                : selector(attribute);
        }
#endif

#if WINDOWS_PHONE
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        public MeasurementConfiguration(string accountId)
            : this(accountId, SystemInfo.WindowsPhoneSystemInfo.ApplicationName, SystemInfo.WindowsPhoneSystemInfo.ApplicationVersion)
        {
        }
#endif
    }
}
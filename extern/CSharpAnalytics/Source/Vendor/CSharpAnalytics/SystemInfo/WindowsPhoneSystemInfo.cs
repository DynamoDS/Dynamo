﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Phone.Info;
using System;
using System.Linq;
using System.Xml.Linq;

namespace CSharpAnalytics.SystemInfo
{
    public static class WindowsPhoneSystemInfo
    {
        private static readonly string applicationName;
        private static readonly string applicationVersion;

        static WindowsPhoneSystemInfo()
        {
            var manifest = XDocument.Load("WMAppManifest.xml");
            if (manifest.Root == null) return;
            
            var app = manifest.Root.Element("App");
            if (app == null) return;

            applicationName = (string)app.Attribute("Title");
            applicationVersion = (string)app.Attribute("Version");
        }

        /// <summary>
        /// Get the Windows version number and processor architecture and cache it
        /// as a user agent string so it can be sent with HTTP requests.
        /// </summary>
        /// <returns>String containing formatted system parts of the user agent.</returns>
        public static string GetSystemUserAgent()
        {
            try
            {
                var osVersion = System.Environment.OSVersion.Version;
                var minor = osVersion.Minor;
                if (minor > 9) minor /= 10;

                var parts = new[] {
                    "Windows Phone " + osVersion.Major + "." + minor,
                    "ARM",
                    "Touch",
                    DeviceStatus.DeviceManufacturer,
                    DeviceStatus.DeviceName
                };

                return "(" + String.Join("; ", parts.Where(e => !String.IsNullOrEmpty(e))) + ")";
            }
            catch
            {
                return "";
            }
        }

        public static string ApplicationName { get { return applicationName; } }
        public static string ApplicationVersion { get { return applicationVersion; } }
    }
}
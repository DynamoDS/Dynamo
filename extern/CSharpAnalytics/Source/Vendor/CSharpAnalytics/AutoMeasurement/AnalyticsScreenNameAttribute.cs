﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics
{
    /// <summary>
    /// Allow a Page class to be decorated with an alternative name to be used by AutoMeasurement
    /// instead of using the default of the page class name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AnalyticsScreenNameAttribute : Attribute
    {
        private readonly string screenName;

        /// <summary>
        /// Name of the screen as it should display in analytics.
        /// </summary>
        public string ScreenName { get { return screenName; } }
 
        /// <summary>
        /// Create an instance of the AnalyticsScreenNameAttribute with a given screen name.
        /// </summary>
        /// <param name="screenName">Name of the screen as it should display in analytics.</param>
        public AnalyticsScreenNameAttribute(string screenName)
        {
            this.screenName = screenName;
        }
    }
}
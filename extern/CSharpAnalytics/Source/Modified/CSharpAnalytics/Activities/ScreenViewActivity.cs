﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a screen displayed within an application.
    /// </summary>
    [DebuggerDisplay("ScreenView {ScreenName}]")]
    public class ScreenViewActivity : MeasurementActivity
    {
        private readonly string screenName;

        /// <summary>
        /// Create a new ScreenViewActivity to capture details of this screen view.
        /// </summary>
        /// <param name="screenName">Name of the screen being viewed.</param>
        public ScreenViewActivity(string screenName)
        {
            this.screenName = screenName;
        }

        /// <summary>
        /// Name of the screen being viewed.
        /// </summary>
        public string ScreenName { get { return screenName; } }
    }

    // For backward compatibility only
    public class AppViewActivity : ScreenViewActivity
    {
        public AppViewActivity(string screenName)
            : base(screenName)
        {
        }   
    }
}
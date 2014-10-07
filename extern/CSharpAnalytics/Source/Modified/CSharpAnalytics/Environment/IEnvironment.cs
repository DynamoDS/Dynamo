﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Environment
{
    /// <summary>
    /// Interface to obtain details about the current environment for analytics.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Current character set of the system. e.g. utf-8
        /// </summary>
        string CharacterSet { get; }

        /// <summary>
        /// Current language code of the system. e.g. en-us
        /// </summary>
        string LanguageCode { get; }
 
        /// <summary>
        /// Version number of Adobe Flash if installed or null if not installed.
        /// </summary>
        string FlashVersion { get; }

        /// <summary>
        /// Whether Java is currently installed or null if not able to detect.
        /// </summary>
        bool? JavaEnabled { get; }

        /// <summary>
        /// Number of bits-per-pixel for this display. e.g. 32, 24 or 8.
        /// </summary>
        uint ScreenColorDepth { get; }

        /// <summary>
        /// Height of the current primary display in pixels. e.g. 768
        /// </summary>
        uint ScreenHeight { get; }

        /// <summary>
        /// Width of the current primary display in pixels. e.g. 1024
        /// </summary>
        uint ScreenWidth { get; }

        /// <summary>
        /// Height of the current window or viewport in pixels. e.g. 980
        /// </summary>
        uint ViewportHeight { get; }

        /// <summary>
        /// Width of the current window or viewport in pixels. e.g. 1000
        /// </summary>
        uint ViewportWidth { get; }
    }
}
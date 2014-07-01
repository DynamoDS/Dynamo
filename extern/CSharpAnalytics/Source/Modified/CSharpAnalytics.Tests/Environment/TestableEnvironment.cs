﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Environment;

namespace CSharpAnalytics.Test.Environment
{
    /// <summary>
    /// Captures details of the environment to be recorded by analytics.
    /// </summary>
    public class TestableEnvironment : IEnvironment
    {
        /// <summary>
        /// Current character set of the system.
        /// </summary>
        /// <example>UTF-8</example>
        public string CharacterSet { get; set; }

        /// <summary>
        /// Current language code of the system.
        /// </summary>
        /// <example>en-us</example>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Version number of Adobe Flash if installed or null if not installed.
        /// </summary>
        /// <example>11.5 r31</example>
        public string FlashVersion { get; set; }

        /// <summary>
        /// Whether Java is currently installed or null if not able to detect.
        /// </summary>
        public bool? JavaEnabled { get; set; }

        /// <summary>
        /// Number of bits-per-pixel for this display.
        /// </summary>
        /// <example>32</example>
        /// <example>24</example>
        public uint ScreenColorDepth { get; set; }

        /// <summary>
        /// Height of the current primary display in pixels.
        /// </summary>
        /// <example>768</example>
        public uint ScreenHeight { get; set; }

        /// <summary>
        /// Width of the current primary display in pixels.
        /// </summary>
        /// <example>1024</example>
        public uint ScreenWidth { get; set; }

        /// <summary>
        /// Height of the current window or viewport in pixels.
        /// </summary>
        /// <example>980</example>
        public uint ViewportHeight { get; set; }

        /// <summary>
        /// Width of the current window or viewport in pixels.
        /// </summary>
        /// <example>1000</example>
        public uint ViewportWidth { get; set; }

        /// <summary>
        /// Create a new environment with a given language code.
        /// </summary>
        /// <param name="languageCode">Language code, e.g. en-us</param>
        public TestableEnvironment(string languageCode)
        {
            LanguageCode = languageCode;
        }
    }
}
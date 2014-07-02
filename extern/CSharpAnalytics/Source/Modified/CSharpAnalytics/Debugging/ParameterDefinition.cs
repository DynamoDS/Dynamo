﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Debugging
{
    /// <summary>
    /// Defines the parameter name, human-readable label and decoder used to implement
    /// debugging and decoding of Google Analytics requests.
    /// </summary>
    class ParameterDefinition
    {
        private static readonly Func<string, string> defaultFormatter = s => s;

        private readonly Func<string, string> formatter;
        private readonly string label;
        private readonly string name;
        private readonly bool isRegexMatch;

        /// <summary>
        /// Creates a new parameter with a given name, label and optional formatter.
        /// </summary>
        /// <param name="name">Name of this parameter as set in the url.</param>
        /// <param name="label">Human-readable label for this parameter.</param>
        /// <param name="formatter">Optional formatter to decode and format the value from the url to a human-readable format.</param>
        /// <param name="isRegexMatch">Whether this parameter is matched against parameter keys using a regex or not.</param>
        public ParameterDefinition(string name, string label, Func<string, string> formatter = null, bool isRegexMatch = false)
        {
            this.name = name;
            this.label = label;
            this.formatter = formatter ?? defaultFormatter;
            this.isRegexMatch = isRegexMatch;
        }

        /// <summary>
        /// Function used to decode and format the URI request value back to human-readable format.
        /// </summary>
        public Func<string, string> Formatter { get { return formatter; } }

        /// <summary>
        /// Label to display for this parameter on debug output.
        /// </summary>
        public string Label { get { return label; } }

        /// <summary>
        /// Name of this parameter when found in an analytics URI request.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Whether this parameter is matched against URI request using a regex.
        /// </summary>
        public bool IsRegexMatch { get { return isRegexMatch; } }
    }
}
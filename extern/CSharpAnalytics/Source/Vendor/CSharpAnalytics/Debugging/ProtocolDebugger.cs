﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpAnalytics.Debugging
{
    /// <summary>
    /// Provide debugging of tracking requests by decomposing such request back into their parts.
    /// </summary>
    /// <remarks>
    /// Output is similar to ga_debug.js.
    /// </remarks>
    internal class ProtocolDebugger
    {
        private readonly ParameterDefinition[] parameterDefinitions;
        private static readonly Action<string> defaultWriter = s => { };

        /// <summary>
        /// Create a new ProtocolDebugger with a given action to receive debugger output.
        /// </summary>
        /// <param name="parameterDefinitions">Array of ParameterDefinitions valid for this debugger.</param>
        public ProtocolDebugger(ParameterDefinition[] parameterDefinitions)
        {
            this.parameterDefinitions = parameterDefinitions;
        }

        /// <summary>
        /// Examine URI and break down into constituent parts via the writer
        /// for this debugger.
        /// </summary>
        /// <param name="uri">Analytics tracking URI to examine.</param>
        /// <param name="writer">Action that takes a string to receive output, defaults to debug window.</param>
        public void Dump(Uri uri, Action<string> writer)
        {
            writer = writer ?? defaultWriter;
            writer(uri.ToString());

            var parameters = ExtractParameters(uri);
            foreach (var parameterDefinition in parameterDefinitions)
            {
                if (parameterDefinition.IsRegexMatch)
                    DumpRegexParameters(parameterDefinition, parameters, writer);
                else
                    DumpExactParameter(parameterDefinition, parameters, writer);
            }
        }

        /// <summary>
        /// Dump any parameters that match the given parameter definition's regular expression.
        /// </summary>
        /// <param name="parameterDefinition">Parameter definition to regex match with parameters.</param>
        /// <param name="parameters">All parameters to consider for a regex match.</param>
        /// <param name="writer">Action that takes a string to receive output.</param>
        private void DumpRegexParameters(ParameterDefinition parameterDefinition, Dictionary<string, string> parameters, Action<string> writer)
        {
            foreach (var pair in parameters)
            {
                var match = Regex.Match(pair.Key, parameterDefinition.Name);
                if (match.Success)
                {
                    var label = parameterDefinition.Label.Replace("$1", match.Groups[1].Captures[0].Value);
                    DumpParameter(parameterDefinition, label, pair.Value, writer);
                }
            }
        }

        /// <summary>
        /// Dump any parameter exactly matching the given parameter definition.
        /// </summary>
        /// <param name="parameterDefinition">Parameter definition to exactly match with parameters.</param>
        /// <param name="parameters">All parameters to consider for an exact match.</param>
        /// <param name="writer">Action that takes a string to receive debugger output.</param>
        private void DumpExactParameter(ParameterDefinition parameterDefinition, Dictionary<string, string> parameters, Action<string> writer)
        {
            string rawValue;
            if (parameters.TryGetValue(parameterDefinition.Name, out rawValue))
                DumpParameter(parameterDefinition, parameterDefinition.Label, rawValue, writer);
        }

        /// <summary>
        /// Extract the query string parameters from a URI into a dictionary of keys and values.
        /// </summary>
        /// <param name="uri">URI to extract the parameters from.</param>
        /// <returns>Dictionary of keys and values representing the parameters.</returns>
        private static Dictionary<string, string> ExtractParameters(Uri uri)
        {
            return uri.GetComponents(UriComponents.Query, UriFormat.SafeUnescaped)
                .Split('&')
                .Select(kv => kv.Split('='))
                .ToDictionary(k => k[0], v => Uri.UnescapeDataString(v[1]));
        }

        /// <summary>
        /// Format and write a parameter to the current writer.
        /// </summary>
        /// <param name="parameterDefinition">Parameter to write out.</param>
        /// <param name="label">Human-readable label to prefix before the value.</param>
        /// <param name="rawValue">Raw value of the parameter to format before writing.</param>
        /// <param name="writer">Action that takes a string to receive output.</param>
        private void DumpParameter(ParameterDefinition parameterDefinition, string label, string rawValue, Action<string> writer)
        {
            var formattedValue = parameterDefinition.Formatter(rawValue);
            if (!String.IsNullOrWhiteSpace(formattedValue))
                writer(label.PadRight(24) + ": " + formattedValue);
        }
    }
}
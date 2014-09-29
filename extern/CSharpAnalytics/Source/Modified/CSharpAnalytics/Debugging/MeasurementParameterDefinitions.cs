﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Debugging
{
    /// <summary>
    /// List of all known parameters that may be found in a Google Analytics Measurement Protocol URI.
    /// </summary>
    /// <remarks>
    /// Used to facilitate the debugging of such requests.
    /// </remarks>
    static class MeasurementParameterDefinitions
    {
        /// <summary>
        /// All parameters that may be found in a Google Analytics Measurement Protocol request.
        /// </summary>
        public static readonly ParameterDefinition[] All = 
        {
            // General
            new ParameterDefinition("v",        "Protocol Version"),
            new ParameterDefinition("tid",      "Tracking ID"),
            new ParameterDefinition("aip",      "Anonymize IP", FormatBoolean),
            new ParameterDefinition("qt",       "Queue Time", s => s + " ms"),
            new ParameterDefinition("z",        "Cache Buster"),

            // Visitor
            new ParameterDefinition("cid",      "Client ID"),
            new ParameterDefinition("uid",      "User ID"),

            // Session
            new ParameterDefinition("sc",       "Session Control"),
            new ParameterDefinition("uip",      "IP Override"),
            new ParameterDefinition("ua",       "User Agent Override"),

            // Traffic Sources
            new ParameterDefinition("dr",       "Document Referrer"),

            new ParameterDefinition("cn",       "Campaign Name"),
            new ParameterDefinition("cs",       "Campaign Source"),
            new ParameterDefinition("cm",       "Campaign Medium"),
            new ParameterDefinition("ck",       "Campaign Keyword"),
            new ParameterDefinition("cc",       "Campaign Content"),
            new ParameterDefinition("ci",       "Campaign ID"),

            new ParameterDefinition("gclid",    "Google Adwords"),
            new ParameterDefinition("dclid",    "Google Display Ads ID"),

            // System Info
            new ParameterDefinition("sr",       "Screen Resolution"),
            new ParameterDefinition("vp",       "Viewport Size"),
            new ParameterDefinition("de",       "Document Encoding"),
            new ParameterDefinition("sd",       "Screen Colors"),
            new ParameterDefinition("ul",       "User Language"),
            new ParameterDefinition("je",       "Java Enabled", FormatBoolean),
            new ParameterDefinition("fl",       "Flash Version"),

            // Hit
            new ParameterDefinition("t",        "Hit Type"),
            new ParameterDefinition("ni",       "Non-Interaction Hit", FormatBoolean),

            // Content Information
            new ParameterDefinition("dl",       "Document Location URL"),
            new ParameterDefinition("dh",       "Document Host Name"),
            new ParameterDefinition("dp",       "Document Path"),
            new ParameterDefinition("dt",       "Document Title"),
            new ParameterDefinition("cd",       "Screen Name"),
            new ParameterDefinition("linkid",   "Link ID"),

            // App Tracking
            new ParameterDefinition("an",       "Application Name"),
            new ParameterDefinition("aid",      "Application ID"),
            new ParameterDefinition("av",       "Application Version"),
            new ParameterDefinition("aiid",     "Application Installer ID"),

            // Event Tracking
            new ParameterDefinition("ec",       "Event Category"),
            new ParameterDefinition("ea",       "Event Action"),
            new ParameterDefinition("el",       "Event Label"),
            new ParameterDefinition("ev",       "Event Value"),

            // E-Commerce
            new ParameterDefinition("ti",       "Transaction ID"),
            new ParameterDefinition("ta",       "Transaction Affilliation"),
            new ParameterDefinition("tr",       "Transaction Revenue"),
            new ParameterDefinition("ts",       "Transaction Shipping"),
            new ParameterDefinition("tt",       "Transaction Tax"),

            new ParameterDefinition("in",       "Item Name"),
            new ParameterDefinition("ip",       "Item Price"),
            new ParameterDefinition("iq",       "Item Quantity"),
            new ParameterDefinition("ic",       "Item Code"),
            new ParameterDefinition("iv",       "Item Category"),
            new ParameterDefinition("cu",       "Currency Code"),

            // Social Interactions
            new ParameterDefinition("sn",       "Social Network"),
            new ParameterDefinition("sa",       "Social Action"),
            new ParameterDefinition("st",       "Social Action Target"),

            // Timing
            new ParameterDefinition("utc",      "User Timing Category"),
            new ParameterDefinition("utv",      "User Timing Variable"),
            new ParameterDefinition("utt",      "User Timing Time"),
            new ParameterDefinition("utl",      "User Timing Label"),

            new ParameterDefinition("plt",      "Page Load Time"),
            new ParameterDefinition("dns",      "DNS Time"),
            new ParameterDefinition("pdt",      "Page Download Time"),
            new ParameterDefinition("rrt",      "Redirect Response Time"),
            new ParameterDefinition("tcp",      "TCP Connect Time"),
            new ParameterDefinition("srt",      "Server Response Time"),

            // Exceptions
            new ParameterDefinition("exd",      "Exception Description"),
            new ParameterDefinition("exf",      "Is Exception Fatal", FormatBoolean),

            // Custom Dimensions / Metrics
            new ParameterDefinition("cd([0-9]+)", "Custom Dimension $1", isRegexMatch:true),
            new ParameterDefinition("cm([0-9]+)", "Custom Metric $1", isRegexMatch:true),

            // Experiments
            new ParameterDefinition("xid",      "Experiment ID"),
            new ParameterDefinition("xvar",     "Experiment Variant"),

            // Undocumented
            new ParameterDefinition("_v",       "Library Version")
        };

        /// <summary>
        /// Convert a boolean 1 or 0 back to True or False.
        /// </summary>
        /// <param name="input">Input string to convert.</param>
        /// <returns>True for 1, False for 0 and - for all other values.</returns>
        private static string FormatBoolean(string input)
        {
            switch (input)
            {
                case "1": return "True";
                case "0": return "False";
                default: return "-";
            }
        }
    }
}
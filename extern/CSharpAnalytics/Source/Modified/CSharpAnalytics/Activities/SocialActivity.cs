﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an social action that has been performed.
    /// </summary>
    [DebuggerDisplay("Social {Action} on {Network}")]
    public class SocialActivity : MeasurementActivity
    {
        private readonly string action;
        private readonly string network;
        private readonly string target;

        /// <summary>
        /// Type of social action performed. e.g. Like, Share, Tweet
        /// </summary>
        public string Action
        {
            get { return action; }
        }

        /// <summary>
        /// Social network the action was performed on. e.g. Facebook, Twitter, LinkedIn
        /// </summary>
        public string Network
        {
            get { return network; }
        }

        /// <summary>
        /// Optional resource being acted upon. e.g. Page being shared.
        /// </summary>
        public string Target
        {
            get { return target; }
        }

        /// <summary>
        /// Create a new SocialActivity with various parameters to be captured.
        /// </summary>
        /// <param name="action">Social action being performed.</param>
        /// <param name="network">Name of the social network being acted upon.</param>
        /// <param name="target">Optional target resource being acted upon.</param>
        public SocialActivity(string action, string network, string target = null)
        {
            this.network = network;
            this.action = action;
            this.target = target;
        }
    }
}
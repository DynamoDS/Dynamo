﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an individual event to be recorded in analytics.
    /// See https://developers.google.com/analytics/devguides/collection/gajs/eventTrackerGuide Event Tracking Guide
    /// </summary>
    [DebuggerDisplay("Event {Category}, {Action}, {Label}")]
    public class EventActivity : MeasurementActivity
    {
        private readonly string action;
        private readonly string category;
        private readonly string label;
        private readonly int? value;

        /// <summary>
        /// Name to represent what this event did. Uniquely paired with <seealso cref="Category"/>.
        /// </summary>
        /// <example>Play</example>
        public virtual string Action
        {
            get { return action; }
        }

        /// <summary>
        /// Name used to group various related actions together.
        /// </summary>
        /// <example>Videos</example>
        public string Category
        {
            get { return category; }
        }

        /// <summary>
        /// Optional on-screen label associated with this event.
        /// </summary>
        /// <example>Gone With the Wind</example>
        public string Label
        {
            get { return label; }
        }

        /// <summary>
        /// Optional numerical value associated with this event often used for timing or ratings.
        /// </summary>
        public int? Value
        {
            get { return value; }
        }

        /// <summary>
        /// Create a new EventActivity with various parameters to be captured.
        /// </summary>
        /// <param name="action">Action name to be assigned to the Action property.</param>
        /// <param name="category">Category name to be assigned to Category property.</param>
        /// <param name="label">Optional label to be assigned to the Label property.</param>
        /// <param name="value">Optional value to be assigned to the Value property.</param>
        /// <param name="nonInteraction">Optional boolean value to be assigned to the NonInteraction property.</param>
        public EventActivity(string action, string category, string label = null, int? value = null, bool nonInteraction = false)
        {
            this.category = category;
            this.action = action;
            this.label = label;
            this.value = value;
            NonInteraction = nonInteraction;
        }
    }
}
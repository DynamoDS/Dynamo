﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an automatically timed event to be recorded in analytics.
    /// </summary>
    /// <remarks>
    /// This activities time is the time between StartedAt and EndedAt which defaults to
    /// when it was created and when it was tracked respectively.
    /// </remarks>
    [DebuggerDisplay("AutoTimedEvent {Category}, {Variable}, {Time}, {Label}")]
    public class AutoTimedEventActivity : TimedEventActivity
    {
        /// <summary>
        /// When this event ended.
        /// </summary>
        public DateTimeOffset? EndedAt { get; set; }

        /// <summary>
        /// When this event started.
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary>
        /// How long the event took.
        /// </summary>
        public override TimeSpan Time
        {
            get { return (EndedAt ?? DateTimeOffset.Now) - StartedAt; }
        }

        /// <summary>
        /// Create a new AutoTimedEventActivity to capture details of a timed event.
        /// </summary>
        /// <param name="category">Category of this event.</param>
        /// <param name="variable">Variable name of this event.</param>
        /// <param name="label">Label for this event.</param>
        public AutoTimedEventActivity(string category, string variable, string label = null)
            : base(category, variable, TimeSpan.Zero, label)
        {
            StartedAt = DateTimeOffset.Now;
        }

        /// <summary>
        /// Mark the event as ended.
        /// </summary>
        public void End()
        {
            EndedAt = DateTimeOffset.Now;
        }
    }
}
﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a timed event to be recorded in analytics.
    /// </summary>
    [DebuggerDisplay("TimedEvent {Category}, {Variable}, {Time}, {Label}")]
    public class TimedEventActivity : MeasurementActivity
    {
        private readonly string category;
        private readonly string label;
        private readonly TimeSpan time;
        private readonly string variable;

        /// <summary>
        /// Logical group name for categorized reporting.
        /// </summary>
        /// <example>jQuery</example>
        public string Category
        {
            get { return category; }
        }

        /// <summary>
        /// Optional further break-down within the variable.
        /// </summary>
        /// <example>Google CDN</example>
        public string Label
        {
            get { return label; }
        }

        /// <summary>
        /// How long the event took.
        /// </summary>
        public virtual TimeSpan Time
        {
            get { return time; }
        }

        /// <summary>
        /// Name of the event within the category.
        /// </summary>
        /// <example>Load Library</example>
        public string Variable
        {
            get { return variable; }
        }

        /// <summary>
        /// Create a new EventActivity with various parameters to be captured.
        /// </summary>
        /// <param name="category">Category name to be assigned to Category property.</param>
        /// <param name="variable">Variable name to be assigned to Variable property.</param>
        /// <param name="time">Optional time to be assigned to the Time property, will default to time between being started and ended.</param>
        /// <param name="label">Optional label to be assigned to the Label property.</param>
        public TimedEventActivity(string category, string variable, TimeSpan time, string label = null)
        {
            this.category = category;
            this.variable = variable;
            this.label = label;
            this.time = time;
        }
    }
}
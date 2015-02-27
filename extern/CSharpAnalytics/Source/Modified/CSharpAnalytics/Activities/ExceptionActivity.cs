﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an application exception to be recorded in analytics.
    /// </summary>
    [DebuggerDisplay("Exception {Description}")]
    public class ExceptionActivity : MeasurementActivity
    {
        private readonly string description;
        private readonly bool isFatal;

        /// <summary>
        /// Description of this exception.
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// Whether this exception was fatal (caused the application to stop) or not.
        /// </summary>
        public bool IsFatal
        {
            get { return isFatal; }
        }

        /// <summary>
        /// Create a new ExceptionActivity with various parameters to be captured.
        /// </summary>
        /// <param name="description">Description of the exception.</param>
        /// <param name="isFatal">Whether the exception was fatal (caused the app to crash).</param>
        public ExceptionActivity(string description, bool isFatal)
        {
            this.description = description;
            this.isFatal = isFatal;
        }
    }
}
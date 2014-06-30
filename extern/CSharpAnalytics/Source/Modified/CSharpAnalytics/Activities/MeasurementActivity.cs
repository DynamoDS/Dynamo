﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Globalization;

namespace CSharpAnalytics.Activities
{
    public abstract class MeasurementActivity
    {
        internal readonly string[] CustomDimensions = new string[200];
        internal readonly object[] CustomMetrics = new object[200];

        /// <summary>
        /// Whether this hit should end the session.
        /// </summary>
        public bool EndSession { get; set; }

        /// <summary>
        /// Specifies where a hit be considered non-interactive.
        /// </summary>
        /// <remarks>Is used in calculation of bounce rates etc.</remarks>
        public bool NonInteraction { get; set; }

        /// <summary>
        /// Set the value of a custom dimension to be sent with this activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(int index, string value)
        {
            CustomDimensions[index] = value;
        }

        /// <summary>
        /// Set the value of a custom dimension to be sent with this activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// This overide allows you to use an enum instead of integers for the index.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(Enum index, string value)
        {
            ValidateEnum(index);
            SetCustomDimension(Convert.ToInt32(index, CultureInfo.InvariantCulture), value);
        }

        /// <summary>
        /// Set the integer value of a custom metric to be sent with this activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Integer value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, long value)
        {
            CustomMetrics[index] = value;
        }

        /// <summary>
        /// Set the time value of a custom metric to be sent with this activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Time value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, TimeSpan value)
        {
            CustomMetrics[index] = value;
        }

        /// <summary>
        /// Set the financial value of a custom metric to be sent with this activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Financial value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, decimal value)
        {
            CustomMetrics[index] = value;
        }

        /// <summary>
        /// Validate an enum to ensure it is defined and has an underlying int type throwing
        /// an exception if it does not.
        /// </summary>
        /// <param name="index">Enum to check.</param>
        private static void ValidateEnum(Enum index)
        {
            if (Enum.GetUnderlyingType(index.GetType()) != typeof(int))
                throw new ArgumentException("Enum must be of type int", "index");

            if (!Enum.IsDefined(index.GetType(), index))
                throw new ArgumentOutOfRangeException("index", "Enum value is not defined");
        }
    }
}
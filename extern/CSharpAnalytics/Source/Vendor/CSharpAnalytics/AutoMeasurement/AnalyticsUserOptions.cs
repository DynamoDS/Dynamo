﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Sessions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSharpAnalytics
{
    /// <summary>
    /// User options helper for AutoMeasurement.
    /// </summary>
    /// <remarks>
    /// You can bind to this class from XAML to easily allow users to change AutoMeasurement options.
    /// </remarks>
    public class AnalyticsUserOptions : INotifyPropertyChanged
    {
        /// <summary>
        /// Event that fires when a property has changed to allow XAML two-way binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Whether a user has allowed usage data collection or not.
        /// </summary>
        public bool AllowUsageDataCollection
        {
            get { return AutoMeasurement.VisitorStatus == VisitorStatus.Active; }
            set
            {
                var isAlreadyActive = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
                if ((value && !isAlreadyActive) || (!value && isAlreadyActive))
                {
                    AutoMeasurement.SetOptOut(!value);
                    FirePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fires the property changed event if there are interested listeners.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed, is set by compiler.</param>
        private void FirePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var safePropertyChanged = PropertyChanged;
            if (safePropertyChanged != null && propertyName != null)
                safePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
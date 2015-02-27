﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.Environment;
using CSharpAnalytics.Sessions;
using System;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// MeasurementTracker coordinates the tracking of IMeasurementActivity with the environment, session and sender.
    /// </summary>
    internal class MeasurementTracker
    {
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;
        private readonly MeasurementUriBuilder uriBuilder;

        private TransactionActivity lastTransaction;

        /// <summary>
        /// Create a new MeasurementTracker with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics Measurement Protocol client.</param>
        /// <param name="sessionManager">Session manager with visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public MeasurementTracker(MeasurementConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            this.sessionManager = sessionManager;
            this.sender = sender;
            uriBuilder = new MeasurementUriBuilder(configuration, sessionManager, environment);
        }

        /// <summary>
        /// Track an activity in analytics.
        /// </summary>
        /// <param name="activity">MeasurementActivity to track in analytics.</param>
        public void Track(MeasurementActivity activity)
        {
            if (sessionManager.VisitorStatus != VisitorStatus.Active) return;

            CarryForwardLastTransaction(activity);

            sessionManager.Hit();
            if (activity.EndSession)
                sessionManager.End();

            var trackingUri = uriBuilder.BuildUri(activity);
            sender(trackingUri);
        }

        /// <summary>
        /// Preserves the last transaction seen and carries it forward to subsequent
        /// transaction items.
        /// </summary>
        /// <param name="activity">Current activity being tracked.</param>
        private void CarryForwardLastTransaction(MeasurementActivity activity)
        {
            if (activity is TransactionActivity)
                lastTransaction = (TransactionActivity) activity;

            if (activity is TransactionItemActivity)
                ((TransactionItemActivity) activity).Transaction = lastTransaction;
        }
    }
}
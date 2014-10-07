﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Represents a Visitor or user of the application.
    /// </summary>
    public class Visitor
    {
        private readonly Guid clientId;

        /// <summary>
        /// Create a brand-new Visitor.
        /// </summary>
        internal Visitor()
            : this(Guid.NewGuid())
        {
        }

        /// <summary>
        /// Create an existing Visitor.
        /// </summary>
        /// <param name="clientId">Unique Id of the existing Visitor.</param>
        internal Visitor(Guid clientId)
        {
            this.clientId = clientId;
        }

        /// <summary>
        /// Unique Id of this client.
        /// </summary>
        public Guid ClientId { get { return clientId; } }
    }
}
﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Runtime.Serialization;

namespace CSharpAnalytics.Sessions
{
    /// <summary>
    /// Memento to capture all SessionManager details for persisting between application runs.
    /// </summary>
    [DataContract(Namespace="CSharpAnalytics")]
    public class SessionState
    {
        // Visitor
        [DataMember] public Guid VisitorId { get; set; }
        [DataMember] public VisitorStatus VisitorStatus { get; set; }
        
        // Session
        [DataMember] public SessionStatus SessionStatus { get; set; }

        // Internal
        [DataMember] public DateTimeOffset LastActivityAt { get; set; }
        [DataMember] public Uri Referrer { get; set; }
    }
}
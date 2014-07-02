﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics
{
    /// <summary>
    /// Implement this interface on any Pages in your application where you want
    /// to override the page titles or paths generated for that page by emitting them yourself at
    /// the end of the page's LoadState method.
    /// </summary>
    /// <remarks>
    /// This is especially useful for a page that obtains its content from a data source to
    /// track it as seperate virtual pages.
    /// </remarks>
    public interface ITrackOwnView
    {
    }
}
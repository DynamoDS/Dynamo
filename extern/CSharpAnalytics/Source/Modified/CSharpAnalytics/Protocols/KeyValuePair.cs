﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace CSharpAnalytics.Protocols
{
    /// <summary>
    /// Simple static class to provide a static constructor on KeyValuePair to allow type inference.
    /// </summary>
    internal static class KeyValuePair
    {
        /// <summary>
        /// Create a new KeyValuePair with type inference.
        /// </summary>
        /// <typeparam name="TKey">Type of key.</typeparam>
        /// <typeparam name="TValue">Type of value.</typeparam>
        /// <param name="key">The key for this pair..</param>
        /// <param name="value">The value for this pair.</param>
        /// <returns></returns>
        public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }
    }
}
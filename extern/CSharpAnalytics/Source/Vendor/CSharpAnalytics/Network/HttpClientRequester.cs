﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Net.Http;
using System.Threading;

namespace CSharpAnalytics.Network
{
    public class HttpClientRequester : IDisposable
    {
        /// <summary>
        /// Create a new HttpClientRequester that can request a Uri using HttpClient.
        /// </summary>
        public HttpClientRequester()
        {
            HttpClient = new HttpClient();
        }

        public HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Request the URI with retry logic using HttpClient.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="cancellationToken">Token to indicate if request should be abandoned.</param>
        public bool Request(Uri requestUri, CancellationToken cancellationToken)
        {
            var message = CreateRequest(requestUri);
            var request = HttpClient.SendAsync(message, cancellationToken).GetAwaiter().GetResult();
            return request.IsSuccessStatusCode;
        }

        /// <summary>
        /// Creates the HttpRequestMessage for a URI taking into consideration the length.
        /// For URIs over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="uri">URI to request.</param>
        /// <returns>HttpRequestMessage for this URI.</returns>
        internal static HttpRequestMessage CreateRequest(Uri uri)
        {
            if (!uri.ShouldUsePostForRequest())
                return new HttpRequestMessage(HttpMethod.Get, uri);

            var uriWithoutQuery = new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));
            return new HttpRequestMessage(HttpMethod.Post, uriWithoutQuery) { Content = new StringContent(uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped)) };
        }

        /// <summary>
        /// Dispose this instance and release any resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            var safeHttpClient = HttpClient;
            if (isDisposing && safeHttpClient != null)
            {
                safeHttpClient.Dispose();
                HttpClient = null;
            }
        }
    }
}
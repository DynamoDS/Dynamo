// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Copyright (c) Autodesk Inc. All rights reserved.

// LC: Modified to work in .NET 4.0, work with older Web API
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs over HTTP or HTTPS in background using HttpWebRequest.
    /// </summary>
    public class HttpWebRequester
    {
        private readonly string userAgent;

        /// <summary>
        /// Create a new HttpWebRequester.
        /// </summary>
        /// <param name="userAgent">User agent string.</param>
        public HttpWebRequester(string userAgent)
        {
            this.userAgent = userAgent;
        }

        /// <summary>
        /// Request the URI with retry logic using HttpWebRequest.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="cancellationToken">CancellationToken to indicate if the request should be cancelled.</param>
        public bool Request(Uri requestUri, CancellationToken cancellationToken)
        {
            var request = CreateRequest(requestUri);
            request.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
            var response = (HttpWebResponse)request.GetResponse();
            return response.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Creates the HttpWebRequest for a URI taking into consideration the length.
        /// For URIs over 2000 bytes it will be a GET otherwise it will become a POST
        /// with the query payload moved to the POST body.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="writePostBody">Whether to open the http request and write the POST body.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        internal static HttpWebRequest CreateRequest(Uri requestUri, bool writePostBody = true)
        {
            return requestUri.ShouldUsePostForRequest()
                       ? CreatePostRequest(requestUri, writePostBody)
                       : CreateGetRequest(requestUri);
        }

        /// <summary>
        /// Create a HttpWebRequest using the HTTP GET method.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        private static HttpWebRequest CreateGetRequest(Uri requestUri)
        {
            var getRequest = WebRequest.Create(requestUri);
            getRequest.Method = "GET";
            
            //LC: Add explicit cast
            return (HttpWebRequest)getRequest;
        }

        /// <summary>
        /// Create a HttpWebRequest using the HTTP POST method.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="writeBody">Whether it should write the contents of the body.</param>
        /// <returns>HttpWebRequest for this URI.</returns>
        private static HttpWebRequest CreatePostRequest(Uri requestUri, bool writeBody)
        {
            var uriWithoutQuery = new Uri(requestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped));

            //LC - Version adjust
            //var postRequest = WebRequest.CreateHttp(uriWithoutQuery);
            var postRequest = WebRequest.Create(uriWithoutQuery);

            postRequest.Method = "POST";
            
            var bodyWithQuery = requestUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyWithQuery);

            postRequest.ContentLength = bodyBytes.Length;

            if (writeBody)
            {
                var stream = postRequest.GetRequestStream();
                stream.Write(bodyBytes, 0, bodyBytes.Length);
                stream.Close();
            }

            //LC: Add explicit cast
            return (System.Net.HttpWebRequest)postRequest;
        }
    }

#if WINDOWS_PHONE

    internal static class HttpWebRequestExtensions
    {
        internal static void Add(this WebHeaderCollection collection, HttpRequestHeader header, string value)
        {
            collection[header] = value;
        }

        internal static HttpWebResponse GetResponse(this HttpWebRequest request)
        {
            return request.GetResponseAsync().GetAwaiter().GetResult();
        }

        internal static Stream GetRequestStream(this HttpWebRequest request)
        {
            return request.GetRequestStreamAsync().GetAwaiter().GetResult();
        }

        internal static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<HttpWebResponse>();
            request.BeginGetResponse(asyncResponse =>
            {
                try
                {
                    var asyncState = (HttpWebRequest)asyncResponse.AsyncState;
                    var response = (HttpWebResponse)asyncState.EndGetResponse(asyncResponse);
                    tcs.TrySetResult(response);
                }
                catch (WebException ex)
                {
                    var failedResponse = (HttpWebResponse)ex.Response;
                    tcs.TrySetResult(failedResponse);
                }
            }, request);
            return tcs.Task;
        }

        internal static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
        {
            var tcs = new TaskCompletionSource<Stream>();
            request.BeginGetRequestStream(asyncResponse =>
            {
                try
                {
                    var asyncState = (HttpWebRequest)asyncResponse.AsyncState;
                    var stream = asyncState.EndGetRequestStream(asyncResponse);
                    tcs.TrySetResult(stream);
                }
                catch (WebException ex)
                {
                    tcs.SetException(ex);
                }
            }, request);
            return tcs.Task;
        }
        
    }

#endif
}
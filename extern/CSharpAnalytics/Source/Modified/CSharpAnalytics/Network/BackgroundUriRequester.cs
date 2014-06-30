// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Copyright (c) Autodesk Inc. All rights reserved.

// LC: Modified to work in .NET 4.0, removal of async style
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpAnalytics.Network
{
    /// <summary>
    /// Responsible for requesting a queue of URIs in background.
    /// </summary>
    public class BackgroundUriRequester : IDisposable
    {
        protected static readonly TimeSpan NetworkRetryWaitStep = TimeSpan.FromSeconds(5);
        protected static readonly TimeSpan NetworkRetryWaitMax = TimeSpan.FromMinutes(10);

        private readonly Queue<Uri> priorRequests = new Queue<Uri>();
        private readonly Queue<Uri> currentRequests = new Queue<Uri>();

        private CancellationTokenSource cancellationTokenSource;

        private Task backgroundTask;
        private TimeSpan currentUploadInterval;
        private Uri currentlySending;

        private readonly Func<bool> checkInternetAvailable;
        private readonly Func<Uri, CancellationToken, bool> requester;

        public BackgroundUriRequester(Func<Uri, CancellationToken, bool> requester, Func<bool> checkInternetAvailable = null)
        {
            this.requester = requester;
            this.checkInternetAvailable = checkInternetAvailable ?? (() => true);
        }

        /// <summary>
        /// Determines whether the BackgroundHttpRequester is currently started.
        /// </summary>
        public bool IsStarted { get { return cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested; } }

        /// <summary>
        /// Add a URI to be requested to the queue.
        /// </summary>
        /// <param name="uri">URI to be requested.</param>
        public void Add(Uri uri)
        {
            currentRequests.Enqueue(uri);
        }

        /// <summary>
        /// Start the BackgroundHttpRequester with a given upload interval and a list of previously unrequested URIs.
        /// </summary>
        /// <param name="uploadInterval">How often to send the contents of the queue.</param>
        /// <param name="previouslyUnrequested">List of previously unrequested URIs obtained last time the requester was stopped.</param>
        public void Start(TimeSpan uploadInterval, IEnumerable<Uri> previouslyUnrequested = null)
        {
            if (IsStarted)
                throw new InvalidOperationException(String.Format("Cannot start a {0} when already started", GetType().Name));

            if (previouslyUnrequested != null)
                foreach (var request in previouslyUnrequested)
                    priorRequests.Enqueue(request);

            cancellationTokenSource = new CancellationTokenSource();
            currentUploadInterval = uploadInterval;

            backgroundTask = Task.Factory.StartNew(RequestLoop, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>
        /// Stop the BackgroundHttpRequester and return a list of URIs that were not requested.
        /// </summary>
        /// <returns>List of unrequested URIs that should be passed into Start next time.</returns>
        public List<Uri> Stop()
        {
            if (!IsStarted)
                throw new InvalidOperationException(String.Format("Cannot stop a {0} when already stopped", GetType().Name));

            cancellationTokenSource.Cancel();
            backgroundTask.Wait();

            return priorRequests
                .Concat(new[] { currentlySending })
                .Concat(currentRequests)
                .Where(r => r != null)
                .ToList();
        }

        /// <summary>
        /// Loop that keeps requesting URIs in the queue until there are none left, then sleeps.
        /// </summary>
        private void RequestLoop()
        {
            using (var queueEmptyWait = new ManualResetEventSlim())
            {
                while (IsStarted)
                {
                    try
                    {
                        if (checkInternetAvailable())
                        {
                            while (GetNextQueueEntry(out currentlySending))
                            {
                                RequestWithFailureRetry(currentlySending, cancellationTokenSource.Token);
                                currentlySending = null;
                            }
                        }

                        queueEmptyWait.Wait(currentUploadInterval, cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("RequestLoop failing with {0}", ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Request the URI with retry logic using HttpClient.
        /// </summary>
        /// <param name="requestUri">URI to request.</param>
        /// <param name="cancellationToken">Cancellation token that indicates if a request should be cancelled.</param>
        private void RequestWithFailureRetry(Uri requestUri, CancellationToken cancellationToken)
        {
            var retryDelay = TimeSpan.Zero;
            var successfullySent = false;

            do
            {
                try
                {
                    successfullySent = requester(requestUri, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                        ex = ex.GetInnermostException();

                    Debug.WriteLine("RequestWithFailureRetry failing with {0}", ex.Message);
                }
                finally
                {
                    if (!successfullySent)
                        WaitBetweenFailedRequests(ref retryDelay);
                }
            } while (!successfullySent);
        }

        /// <summary>
        /// Get the next entry from the queue.
        /// </summary>
        /// <param name="entry">Entry obtained from the queue.</param>
        /// <returns>True if an entry was available, false otherwise.</returns>
        private bool GetNextQueueEntry(out Uri entry)
        {
            if (priorRequests.Count > 0)
            {
                entry = priorRequests.Dequeue();
                return true;
            }

            if (currentRequests.Count > 0)
            {
                entry = currentRequests.Dequeue();
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>
        /// Delay for a period of time between failed network requests.
        /// </summary>
        /// <param name="previousRetryDelay">Previous retry delay value to base delay on.</param>
        protected void WaitBetweenFailedRequests(ref TimeSpan previousRetryDelay)
        {
            previousRetryDelay = previousRetryDelay + NetworkRetryWaitStep;
            if (previousRetryDelay > NetworkRetryWaitMax)
                previousRetryDelay = NetworkRetryWaitMax;

            using (var failedRequestWait = new ManualResetEventSlim())
                failedRequestWait.Wait(previousRetryDelay, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Total count of all remaining items in the queue.
        /// </summary>
        internal int QueueCount
        {
            get
            {
                return priorRequests.Count
                            + currentRequests.Count
                            + (currentlySending == null ? 0 : 1);
            }
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
            var safeCancellationTokenSource = cancellationTokenSource;
            if (isDisposing && safeCancellationTokenSource != null)
            {
                safeCancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
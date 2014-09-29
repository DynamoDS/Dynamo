﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Environment;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Serializers;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.SystemInfo;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in WindowsStore applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public class WindowsStoreAutoMeasurement : BaseAutoMeasurement
    {
        private DataTransferManager attachedDataTransferManager;
        private Frame attachedFrame;

        /// <summary>
        /// Attach to the root frame, hook into the navigation event and track initial screen view.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        public void Attach(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            if (frame != attachedFrame)
            {
                if (attachedFrame != null)
                    attachedFrame.Navigated -= FrameNavigated;
                frame.Navigated += FrameNavigated;
                attachedFrame = frame;
            }

            var content = frame.Content;
            if (content != null)
                TrackScreenView(content.GetType());
        }

        /// <summary>
        /// Get the environment details for this system.
        /// </summary>
        /// <returns>
        /// IEnvironment implementation for getting screen, language and other system details.
        /// </returns>
        protected override IEnvironment GetEnvironment()
        {
            return new WindowsStoreEnvironment();
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        protected override void HookEvents()
        {
            var application = Application.Current;
            application.Resuming += ApplicationOnResuming;
            application.Suspending += ApplicationOnSuspending;

            attachedDataTransferManager = DataTransferManager.GetForCurrentView();
            attachedDataTransferManager.TargetApplicationChosen += SocialShare;
        }

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        /// <remarks>
        /// Not actually used in AutoMeasurement but here to show you what to do if you wanted to.
        /// </remarks>
        protected override void UnhookEvents()
        {
            var application = Application.Current;
            application.Resuming -= ApplicationOnResuming;
            application.Suspending -= ApplicationOnSuspending;
            attachedDataTransferManager.TargetApplicationChosen -= SocialShare;

            if (attachedFrame != null)
                attachedFrame.Navigated -= FrameNavigated;
            attachedFrame = null;
        }

        /// <summary>
        /// Setup the Uri requester complete with user agent etc.
        /// </summary>
        /// <returns>Task that completes when the requester is ready to use.</returns>
        protected override async Task SetupRequesterAsync()
        {
            var systemUserAgent = await WindowsStoreSystemInfo.GetSystemUserAgentAsync();

            var httpClientRequester = new HttpClientRequester();
            httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            if (!String.IsNullOrEmpty(systemUserAgent))
                httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(systemUserAgent);

            Requester = httpClientRequester.Request;
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        protected override bool IsInternetAvailable()
        {
            var internetProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetProfile == null) return false;

            switch (internetProfile.GetNetworkConnectivityLevel())
            {
                case NetworkConnectivityLevel.None:
                case NetworkConnectivityLevel.LocalAccess:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Load the data object from storage with the given name.
        /// </summary>
        /// <typeparam name="T">Type of data to load from storage.</typeparam>
        /// <param name="name">Name of the data in storage.</param>
        /// <returns>Instance of T containing the loaded data or null if did not exist.</returns>
        protected override async Task<T> Load<T>(string name)
        {
            return await LocalFolderContractSerializer<T>.RestoreAsync(name);
        }

        /// <summary>
        /// Save the data object to storage with the given name overwriting if required.
        /// </summary>
        /// <typeparam name="T">Type of data object to persist.</typeparam>
        /// <param name="data">Data object to persist.</param>
        /// <param name="name">Name to give to the object in storage.</param>
        /// <returns>Task that is complete when the data has been saved to storage.</returns>
        protected override async Task Save<T>(T data, string name)
        {
            await LocalFolderContractSerializer<T>.SaveAsync(data, name);
        }

        /// <summary>
        /// Handle application resuming from suspend without shutdown.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="o">Undocumented event parameter that is null.</param>
        private async void ApplicationOnResuming(object sender, object o)
        {
            await StartRequesterAsync();
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="suspendingEventArgs">Details about the suspending event.</param>
        private async void ApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();
            await StopRequesterAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Receive navigation events to translate them into analytics page views.
        /// </summary>
        /// <remarks>
        /// Implement IAnalyticsPageView if your pages look up content so you can
        /// track better detail from the end of your LoadState method.
        /// </remarks>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">NavigationEventArgs for the event.</param>
        private void FrameNavigated(object sender, NavigationEventArgs e)
        {
            TrackScreenView(e.SourcePageType);
        }

        /// <summary>
        /// Record an event that the social sharing charm has been completed and
        /// which application data was shared with.
        /// </summary>
        /// <param name="sender">DataTransferManager responsible for the share.</param>
        /// <param name="e">Event containing details of which target application was chosen.</param>
        private void SocialShare(DataTransferManager sender, TargetApplicationChosenEventArgs e)
        {
            Client.TrackEvent("Share", "Charms", e.ApplicationName);
        }
    }

    /// <summary>
    /// AutoMeasurement static wrapper to make it easier to use across a Windows Store application.
    /// </summary>
    public static class AutoMeasurement
    {
        private static readonly WindowsStoreAutoMeasurement instance = new WindowsStoreAutoMeasurement();

        public static VisitorStatus VisitorStatus
        {
            get { return instance.VisitorStatus; }
        }

        public static MeasurementAnalyticsClient Client
        {
            get { return instance.Client; }
        }

        public static Action<string> DebugWriter
        {
            set { instance.DebugWriter = value; }
        }

        public static void Attach(Frame rootFrame)
        {
            instance.Attach(rootFrame);
        }

        public static void SetOptOut(bool optOut)
        {
            instance.SetOptOut(optOut);
        }

        public static void Start(MeasurementConfiguration measurementConfiguration, LaunchActivatedEventArgs args, TimeSpan? uploadInterval = null)
        {
            instance.Start(measurementConfiguration, args.Kind.ToString(), uploadInterval);
        }
    }
}
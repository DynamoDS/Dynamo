﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Linq;
using Windows.Foundation;
using Windows.Globalization;
using Windows.UI.Xaml;

namespace CSharpAnalytics.Environment
{
    /// <summary>
    /// Implements the IEnvironment interface required by analytics to track details of the machine
    /// in a WindowsStore application.
    /// </summary>
    internal class WindowsStoreEnvironment : IEnvironment
    {
        private Rect lastGoodScreen = new Rect(0, 0, 0, 0);

        public string CharacterSet { get { return "UTF-8"; } }
        public string LanguageCode { get { return ApplicationLanguages.Languages.First(); } }

        public string FlashVersion { get { return null; } }
        public bool? JavaEnabled { get { return null; } }

        public uint ScreenColorDepth { get { return 32; } }

        public uint ScreenHeight
        {
            get { return (uint)Screen.Height; }
        }

        public uint ScreenWidth
        {
            get { return (uint)Screen.Width; }
        }

        public uint ViewportHeight
        {
            get { return (uint)Screen.Height; }
        }

        public uint ViewportWidth
        {
            get { return (uint)Screen.Width; }
        }

        private Rect Screen
        {
            get
            {
                var currentWindow = Window.Current;
                if (currentWindow != null)
                    lastGoodScreen = currentWindow.CoreWindow.Bounds;
                return lastGoodScreen;
            }
        }
    }
}
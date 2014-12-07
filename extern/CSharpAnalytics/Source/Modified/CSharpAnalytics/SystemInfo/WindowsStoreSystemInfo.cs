﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration.Pnp;

namespace CSharpAnalytics.SystemInfo
{
    /// <summary>
    /// Obtain system information not conveniently exposed by WinRT APIs.
    /// </summary>
    /// <remarks>
    /// Microsoft doesn't really want you getting this information and makes it difficult.
    /// The techniques used here are not bullet proof but are good enough for analytics.
    /// Do not use these methods or techniques for anything more important than that.
    /// (Note that this class was also published as SystemInfoEstimate on our blog)
    /// </remarks>
    public static class WindowsStoreSystemInfo
    {
        private const string ModelNameKey = "System.Devices.ModelName";
        private const string ManufacturerKey = "System.Devices.Manufacturer";
        private const string DisplayPrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        private const string DeviceDriverKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6}";
        private const string DeviceDriverVersionKey = DeviceDriverKey + ",3";
        private const string DeviceDriverProviderKey = DeviceDriverKey + ",9";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + RootContainer + "\"";

        /// <summary>
        /// Build a system user agent string that contains the Windows version number
        /// and CPU architecture.
        /// </summary>
        /// <returns>String containing formatted system parts of the user agent.</returns>
        public static async Task<string> GetSystemUserAgentAsync()
        {
            try
            {
                var parts = new[] {
                    "Windows NT " + await GetWindowsVersionAsync(),
                    FormatForUserAgent(GetProcessorArchitecture())
                };

                return "(" + String.Join("; ", parts.Where(e => !String.IsNullOrEmpty(e))) + ")";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Format a ProcessorArchitecture as it would be expected in a user agent of a browser.
        /// </summary>
        /// <returns>String containing the format processor architecture.</returns>
        static string FormatForUserAgent(ProcessorArchitecture architecture)
        {
            switch (architecture)
            {
                case ProcessorArchitecture.AMD64:
                    return "x64";
                case ProcessorArchitecture.ARM:
                    return "ARM";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the processor architecture of this computer.
        /// </summary>
        /// <returns>The processor architecture of this computer.</returns>
        public static ProcessorArchitecture GetProcessorArchitecture()
        {
            try
            {
                var sysInfo = new _SYSTEM_INFO();
                GetNativeSystemInfo(ref sysInfo);

                return Enum.IsDefined(typeof(ProcessorArchitecture), sysInfo.wProcessorArchitecture)
                    ? (ProcessorArchitecture)sysInfo.wProcessorArchitecture
                    : ProcessorArchitecture.UNKNOWN;
            }
            catch
            {
            }

            return ProcessorArchitecture.UNKNOWN;
        }

        /// <summary>
        /// Get the name of the manufacturer of this computer.
        /// </summary>
        /// <example>Microsoft Corporation</example>
        /// <returns>The name of the manufacturer of this computer.</returns>
        public static async Task<string> GetDeviceManufacturerAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ManufacturerKey });
            return (string)rootContainer.Properties[ManufacturerKey];
        }

        /// <summary>
        /// Get the name of the model of this computer.
        /// </summary>
        /// <example>Surface with Windows 8</example>
        /// <returns>The name of the model of this computer.</returns>
        public static async Task<string> GetDeviceModelAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { ModelNameKey });
            return (string)rootContainer.Properties[ModelNameKey];
        }

        /// <summary>
        /// Get the device category this computer belongs to.
        /// </summary>
        /// <example>Computer.Desktop, Computer.Tablet</example>
        /// <returns>The category of this device.</returns>
        public static async Task<string> GetDeviceCategoryAsync()
        {
            var rootContainer = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer, RootContainer, new[] { DisplayPrimaryCategoryKey });
            return (string)rootContainer.Properties[DisplayPrimaryCategoryKey];
        }

        /// <summary>
        /// Get the version of Windows for this computer.
        /// </summary>
        /// <example>6.2</example>
        /// <returns>Version number of Windows running on this computer.</returns>
        public static async Task<string> GetWindowsVersionAsync()
        {
            // There is no good place to get this so we're going to use the most popular
            // Microsoft driver version number from the device tree.
            var requestedProperties = new[] { DeviceDriverVersionKey, DeviceDriverProviderKey };

            var microsoftVersionedDevices = (await PnpObject.FindAllAsync(PnpObjectType.Device, requestedProperties, RootContainerQuery))
                .Select(d => new { Provider = (string)d.Properties.GetValueOrDefault(DeviceDriverProviderKey),
                                    Version = (string)d.Properties.GetValueOrDefault(DeviceDriverVersionKey) })
                .Where(d => d.Provider == "Microsoft" && d.Version != null)
                .ToList();

            var versionNumbers = microsoftVersionedDevices
                .GroupBy(d => d.Version.Substring(0, d.Version.IndexOf('.', d.Version.IndexOf('.') + 1)))
                .OrderByDescending(d => d.Count())
                .ToList();

            var confidence = (versionNumbers[0].Count() * 100 / microsoftVersionedDevices.Count);
            return versionNumbers.Count > 0 ? versionNumbers[0].Key : "";
        }

        static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref _SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct _SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };
    }

    public enum ProcessorArchitecture : ushort
    {
        INTEL = 0,
        MIPS = 1,
        ALPHA = 2,
        PPC = 3,
        SHX = 4,
        ARM = 5,
        IA64 = 6,
        ALPHA64 = 7,
        MSIL = 8,
        AMD64 = 9,
        IA32_ON_WIN64 = 10,
        UNKNOWN = 0xFFFF
    }
}
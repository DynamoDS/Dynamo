﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace CSharpAnalytics.SystemInfo
{
    /// <summary>
    /// Obtains system information from a regular Windows .NET environment.
    /// </summary>
    public static class WindowsSystemInfo
    {
        /// <summary>
        /// Build a system user agent string that contains the Windows version number
        /// and CPU architecture.
        /// </summary>
        /// <returns>String containing formatted system parts of the user agent.</returns>
        public static string GetSystemUserAgent()
        {
            try
            {
                var parts = new[] {
                    System.Environment.OSVersion.VersionString,
                    GetPlatform()
                };

                return "(" + String.Join("; ", parts.Where(e => !String.IsNullOrEmpty(e))) + ")";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Retrieves information about the current system.
        /// </summary>
        /// <param name="systemInfo">A pointer to a <see cref="SystemInfo"/> structure that receives the information.</param>
        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(ref SystemInfo systemInfo);

        /// <summary>
        /// Gets the current processor architecture.
        /// </summary>
        private static string GetPlatform()
        {
            var sysInfo = new SystemInfo();
            GetNativeSystemInfo(ref sysInfo);

            switch (sysInfo.ProcessorArchitecture)
            {
                case 0:
                    return "x86";
                case 6:
                    return "IA64";
                case 9:
                    return "x64";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Contains information about the current computer system. This includes the architecture and type of the processor, 
        /// the number of processors in the system, the page size, and other such information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SystemInfo
        {
            public ushort ProcessorArchitecture;
            public ushort Reserved;
            public uint PageSize;
            public IntPtr MinimumApplicationAddress;
            public IntPtr MaximumApplicationAddress;
            public UIntPtr ActiveProcessorMask;
            public uint NumberOfProcessors;
            public uint ProcessorType;
            public uint AllocationGranularity;
            public ushort ProcessorLevel;
            public ushort ProcessorRevision;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamo.Utils
{
    public class ManagedMemoryCounter : IDisposable
    {
        private Process _process;
        private PerformanceCounter _memoryCounter;
        private PerformanceCounter _privateWorkingSet;
        private const string CATEGORY_NET_CLR_MEMORY = ".NET CLR Memory";
        private const string CATEGORY_PROCESS = "Process";
        private const string PROCESS_ID = "Process ID";

        public ManagedMemoryCounter(int processId)
            : this(Process.GetProcessById(processId))
        {
        }

        public ManagedMemoryCounter(Process p)
        {
            if (p == null)
            {
                throw new ArgumentNullException("Process is null");
            }

            _process = p;
            string processInstanceName = GetInstanceNameForProcess(p);
            _memoryCounter = new PerformanceCounter(CATEGORY_NET_CLR_MEMORY, "# Bytes in all Heaps", processInstanceName);
            _privateWorkingSet = new PerformanceCounter(CATEGORY_PROCESS, "Working Set - Private", processInstanceName);
        }

        public long BytesInAllHeaps
        {
            get
            {
                return _memoryCounter.NextSample().RawValue;
            }
        }

        public long PrivateWorkingSet
        {
            get
            {
                return _privateWorkingSet.NextSample().RawValue;
            }
        }
        
        public bool HasProcessExited()
        {
            if (_process == null)
                return true;

            try
            {
                return _process.HasExited;
            }
            catch
            {
                return Process.GetProcessById(_process.Id) == null;
            } 
        }

        private string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    var mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        } 

        private string GetInstanceNameForProcess(Process p)
        {
            string modulePath = GetMainModuleFilepath(p.Id);
            string instanceName = Path.GetFileNameWithoutExtension(modulePath);
            if (instanceName.Equals("nunit-console", StringComparison.CurrentCultureIgnoreCase))
            {
                instanceName = "nunit-agent";
            }
            return instanceName;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _memoryCounter.Dispose();
            _privateWorkingSet.Dispose();
        }

        #endregion
    }

    class MemoryUsageLogger
    {
        static void Main(string[] args)
        {
            string processFilePath = args[0];
            if (!File.Exists(processFilePath))
            {
                Console.WriteLine("File {0} does not exist.", processFilePath);
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(args[0]);
            processStartInfo.Arguments = String.Join(" ", args.Skip(1)); 
            var process = Process.Start(processStartInfo);
            long maxNetMemory = 0;
            long maxPrivateWorkingSet = 0;

            using (ManagedMemoryCounter counter = new ManagedMemoryCounter(process))
            {
                Stopwatch timer = Stopwatch.StartNew(); 
                timer.Start();

                while (!counter.HasProcessExited())
                {
                    var bytesInHeap = counter.BytesInAllHeaps / 1024;
                    maxNetMemory = Math.Max(maxNetMemory, bytesInHeap);

                    var privateWorkingSet = counter.PrivateWorkingSet / 1024;
                    maxPrivateWorkingSet = Math.Max(maxPrivateWorkingSet, privateWorkingSet);
                    Thread.Sleep(100);
                }

                timer.Stop();
                Console.WriteLine("{0},{1},{2}", timer.ElapsedMilliseconds, maxNetMemory, maxPrivateWorkingSet);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Dynamo.Applications;
using Dynamo.Logging;
using Dynamo.Models;
using NUnit.Framework;
using static Dynamo.Models.DynamoModel;

namespace IntegrationTests
{
    public class DynamoApplicationTests
    {
        [Test]
        public void DynamoSandboxLoadsASMFromValidPath()
        {
            var versions = new List<Version>(){
                new Version(232, 0, 0),
                new Version(231, 0, 0),
            };


            //go get a valid asm path.
            var locatedPath = string.Empty;
            var coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Process dynamoSandbox = null;

            DynamoShapeManager.Utilities.GetInstalledAsmVersion2(versions, ref locatedPath, coreDirectory);
            try
            {
                Assert.DoesNotThrow(() =>
                {
                    // we use a new process to avoid checking against previously loaded
                    // asm modules in the nunit-agent process.
                    dynamoSandbox = System.Diagnostics.Process.Start(new ProcessStartInfo(Path.Combine(coreDirectory, "DynamoSandbox.exe"), $"-gp \"{locatedPath}\""){ UseShellExecute = true });
                    dynamoSandbox.WaitForInputIdle();

                    var firstASMmodulePath = string.Empty;
                    foreach (ProcessModule module in dynamoSandbox.Modules)
                    {
                        if (module.FileName.Contains("ASMAHL"))
                        {
                            firstASMmodulePath = module.FileName;
                            break;
                        }
                    }
                    // TODO: This test need to be updated somehow to bypass splash screen
                    if (!string.IsNullOrEmpty(firstASMmodulePath))
                    {
                        //assert that ASM is really loaded from exactly where we specified.
                        Assert.AreEqual(Path.GetDirectoryName(firstASMmodulePath), locatedPath);
                    }
                });
            }
            finally
            {

                dynamoSandbox?.Kill();

            }
        }

        [Test]
        public void DynamoMakeModelWithHostName()
        {
            var model = Dynamo.Applications.StartupUtils.MakeModel(false, string.Empty, "DynamoFormIt");
            Assert.AreEqual(DynamoModel.HostAnalyticsInfo.HostName, "DynamoFormIt");
        }
        [Test]
        public void DynamoModelStartedWithNoNetworkMode_AlsoDisablesAnalytics()
        {
            var startConfig = new DefaultStartConfiguration() { NoNetworkMode = true };
            var model = DynamoModel.Start(startConfig);
            Assert.AreEqual(true, Analytics.DisableAnalytics);
            model.ShutDown(false);
        }
        [Test]
        public void DynamoModelStartedWithNoNetworkModeFalse_DisablesAnalyticsCanBeTrue()
        {
            var startConfig = new DefaultStartConfiguration() { NoNetworkMode = false };
            Analytics.DisableAnalytics = true;
            var model = DynamoModel.Start(startConfig);
            Assert.AreEqual(true, Analytics.DisableAnalytics);
            model.ShutDown(false);
        }

        [Test]
        public void IfASMPathInvalidExceptionNotThrown()
        {
            var asmMockPath = @"./doesNotExist/";
            Assert.DoesNotThrow(() =>
            {
                var model = StartupUtils.MakeModel(true, asmMockPath);
                Assert.IsNotNull(model);
            });
        }

        [Test]
        public void GetVersionFromASMPath_returnsFileVersionForMockdll()
        {
            var version = DynamoShapeManager.Utilities.GetVersionFromPath(@"./", "DynamoCore*.dll");
            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Assert.AreEqual(version.Major, thisVersion.Major);
            Assert.AreEqual(version.Minor, thisVersion.Minor);
            Assert.AreEqual(version.Build, thisVersion.Build);
        }

        [Test]
        public void ASMVersionIsLoggedWhenSuccessfullyLoaded()
        {
            // Arrange
            var model = StartupUtils.MakeModel(CLImode: true, asmPath: string.Empty);

            // Assert - Verify that either a specific version is logged or the generic success message
            var logText = model.Logger.LogText;
            Assert.IsTrue(
                (logText.Contains("ASM version") && logText.Contains("loaded from:")) ||
                logText.Contains("ASM loaded successfully"),
                "Expected ASM logging to appear in logger output"
            );

            model.ShutDown(false);
        }

        [Test]
        public void ASMWarningLoggedWhenLoadFails()
        {
            // Arrange - use invalid path to force ASM load failure
            var invalidPath = @"./invalid/asm/path/";
            var model = StartupUtils.MakeModel(CLImode: true, asmPath: invalidPath);

            // Assert - Invalid path should log a warning
            var logText = model.Logger.LogText;
            Assert.IsTrue(
                logText.Contains("ASM could not be loaded"),
                "Expected ASM load failure warning to be logged"
            );

            model.ShutDown(false);
        }

        [Test]
        public void ASMMessageLoggedWhenVersionUndetermined()
        {
            // Arrange - create model with default ASM
            var model = StartupUtils.MakeModel(CLImode: true, asmPath: string.Empty);

            // Assert
            var logText = model.Logger.LogText;

            // Should have some ASM-related log message
            Assert.IsTrue(
                logText.Contains("ASM version") ||
                logText.Contains("ASM loaded successfully") ||
                logText.Contains("ASM could not be loaded"),
                "Expected some ASM status message in logger output"
            );

            model.ShutDown(false);
        }

        // This test is Explicit because it launches the full DynamoSandbox process tree (including
        // WebView2/Edge child processes) and inspects live OS TCP tables. That is environment
        // sensitive: corporate proxies, VPNs, endpoint-protection agents and OS-level services can
        // open connections attributed to child processes, producing false positives on shared CI.
        // Run it on a clean, isolated machine when auditing the --NoNetworkMode guarantee.
        [Test, Explicit("Network-egress audit; run on a clean, isolated machine. See comment above.")]
        [Category("NetworkAudit")]
        public void WhenDynamoStartsWithNoNetworkModeThenNoOutboundConnectionsAreOpened()
        {
            var coreDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Process dynamoSandbox = null;

            try
            {
                dynamoSandbox = Process.Start(new ProcessStartInfo(
                    Path.Combine(coreDirectory, "DynamoSandbox.exe"), "--NoNetworkMode")
                { UseShellExecute = true });

                // Give startup (splash + home page + WebView2 children) time to settle.
                dynamoSandbox.WaitForInputIdle();
                Thread.Sleep(TimeSpan.FromSeconds(30));

                var treePids = GetProcessTreePids(dynamoSandbox.Id);
                var offenders = GetNonLoopbackTcpConnections(treePids).ToList();

                Assert.IsEmpty(offenders,
                    "Dynamo opened outbound connections in --NoNetworkMode: " +
                    string.Join(", ", offenders));
            }
            finally
            {
                dynamoSandbox?.Kill(entireProcessTree: true);
            }
        }

        /// <summary>
        /// Returns the set of process ids belonging to <paramref name="rootPid"/> and its descendants
        /// (e.g. the msedgewebview2.exe children spawned by Dynamo's WebView2 surfaces).
        /// </summary>
        private static HashSet<int> GetProcessTreePids(int rootPid)
        {
            var pids = new HashSet<int> { rootPid };
            var all = Process.GetProcesses();

            // Build a parent-map by walking every process's parent id via WMI-free heuristic:
            // repeatedly add processes whose parent is already in the set until it stabilizes.
            var parentById = new Dictionary<int, int>();
            foreach (var p in all)
            {
                try { parentById[p.Id] = GetParentProcessId(p.Id); }
                catch { /* process may have exited; ignore */ }
            }

            bool added = true;
            while (added)
            {
                added = false;
                foreach (var kvp in parentById)
                {
                    if (!pids.Contains(kvp.Key) && pids.Contains(kvp.Value))
                    {
                        pids.Add(kvp.Key);
                        added = true;
                    }
                }
            }
            return pids;
        }

        /// <summary>
        /// Snapshots the IPv4 TCP table and returns any connection owned by a process in
        /// <paramref name="treePids"/> whose remote endpoint is not loopback and is in a state that
        /// indicates an actual outbound connection attempt.
        /// </summary>
        private static IEnumerable<string> GetNonLoopbackTcpConnections(HashSet<int> treePids)
        {
            foreach (var row in GetTcpTable())
            {
                if (!treePids.Contains(row.OwningPid)) continue;

                var remote = row.RemoteEndPoint.Address;
                if (IPAddress.IsLoopback(remote)) continue;
                if (remote.Equals(IPAddress.Any)) continue; // 0.0.0.0 == listening, not outbound

                // Only flag states that represent a real outbound connection.
                if (row.State == MibTcpState.Established ||
                    row.State == MibTcpState.SynSent ||
                    row.State == MibTcpState.SynReceived)
                {
                    yield return $"pid {row.OwningPid} -> {row.RemoteEndPoint} ({row.State})";
                }
            }
        }

        #region Win32 interop for per-PID TCP ownership

        private enum MibTcpState
        {
            Closed = 1, Listen = 2, SynSent = 3, SynReceived = 4, Established = 5,
            FinWait1 = 6, FinWait2 = 7, CloseWait = 8, Closing = 9, LastAck = 10,
            TimeWait = 11, DeleteTcb = 12
        }

        private readonly struct TcpRow
        {
            public TcpRow(MibTcpState state, IPEndPoint remote, int pid)
            {
                State = state; RemoteEndPoint = remote; OwningPid = pid;
            }
            public MibTcpState State { get; }
            public IPEndPoint RemoteEndPoint { get; }
            public int OwningPid { get; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public uint localPort;
            public uint remoteAddr;
            public uint remotePort;
            public uint owningPid;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize,
            bool bOrder, int ulAf, int tableClass, int reserved);

        private const int AF_INET = 2;
        private const int TCP_TABLE_OWNER_PID_ALL = 5;

        private static IEnumerable<TcpRow> GetTcpTable()
        {
            var rows = new List<TcpRow>();
            int size = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr table = Marshal.AllocHGlobal(size);
            try
            {
                if (GetExtendedTcpTable(table, ref size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0) != 0)
                {
                    return rows;
                }

                int rowCount = Marshal.ReadInt32(table);
                IntPtr rowPtr = IntPtr.Add(table, sizeof(int));
                int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();

                for (int i = 0; i < rowCount; i++)
                {
                    var r = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                    var remoteAddress = new IPAddress(r.remoteAddr);
                    // Ports are stored in network byte order in the low 16 bits.
                    int remotePort = ((int)(r.remotePort & 0xFF) << 8) | (int)((r.remotePort >> 8) & 0xFF);
                    rows.Add(new TcpRow((MibTcpState)r.state,
                        new IPEndPoint(remoteAddress, remotePort), (int)r.owningPid));
                    rowPtr = IntPtr.Add(rowPtr, rowSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(table);
            }
            return rows;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        private static int GetParentProcessId(int pid)
        {
            using var process = Process.GetProcessById(pid);
            var pbi = new PROCESS_BASIC_INFORMATION();
            if (NtQueryInformationProcess(process.Handle, 0, ref pbi, Marshal.SizeOf(pbi), out _) != 0)
            {
                return 0;
            }
            return pbi.InheritedFromUniqueProcessId.ToInt32();
        }

        #endregion
    }
}

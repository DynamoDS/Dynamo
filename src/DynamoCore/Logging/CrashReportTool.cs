using Dynamo.Core;
using Dynamo.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo.Core
{
    public class CrashReportArgs : EventArgs
    {
        public bool SendLogFile = true;
        public bool SendSettingsFile = true;
        public bool SendDynFile = true;

        internal DynamoModel model;

        internal CrashReportArgs(DynamoModel dynamoModel)
        {
            model = dynamoModel;
        }
    }

    internal class CrashReportTool
    {
        private static string CerToolLocation = @"";

        internal enum MINIDUMP_TYPE
        {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpFilterMemory = 0x00000008,
            MiniDumpScanMemory = 0x00000010,
            MiniDumpWithUnloadedModules = 0x00000020,
            MiniDumpWithIndirectlyReferencedMemory = 0x00000040,
            MiniDumpFilterModulePaths = 0x00000080,
            MiniDumpWithProcessThreadData = 0x00000100,
            MiniDumpWithPrivateReadWriteMemory = 0x00000200,
            MiniDumpWithoutOptionalData = 0x00000400,
            MiniDumpWithFullMemoryInfo = 0x00000800,
            MiniDumpWithThreadInfo = 0x00001000,
            MiniDumpWithCodeSegs = 0x00002000,
            MiniDumpWithoutAuxiliaryState = 0x00004000,
            MiniDumpWithFullAuxiliaryState = 0x00008000,
            MiniDumpWithPrivateWriteCopyMemory = 0x00010000,
            MiniDumpIgnoreInaccessibleMemory = 0x00020000,
            MiniDumpWithTokenInformation = 0x00040000,
            MiniDumpWithModuleHeaders = 0x00080000,
            MiniDumpFilterTriage = 0x00100000,
            MiniDumpWithAvxXStateContext = 0x00200000,
            MiniDumpWithIptTrace = 0x00400000,
            MiniDumpScanInaccessiblePartialPages = 0x00800000,
            MiniDumpFilterWriteCombinedMemory,
            MiniDumpValidTypeFlags = 0x01ffffff
        };

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("dbghelp.dll")]
        public static extern bool MiniDumpWriteDump(
            IntPtr hProcess,
            UInt32 ProcessId,
            SafeHandle hFile,
            MINIDUMP_TYPE DumpType,
            ref MINIDUMP_EXCEPTION_INFORMATION ExceptionParam,
            IntPtr UserStreamParam,
            IntPtr CallbackParam);


        /// <summary>
        /// Struct mapping to MINIDUMP_EXCEPTION_INFORMATION for Win32 API
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MINIDUMP_EXCEPTION_INFORMATION
        {
            /// <summary>
            /// The thread id
            /// </summary>
            public uint ThreadId;

            /// <summary>
            /// The exception pointers
            /// </summary>
            public IntPtr ExceptionPointers;

            /// <summary>
            /// The client pointers
            /// </summary>
            public int ClientPointers;
        }

        // If input argument "outputDir" is null or empty,
        // the crash report file (name = %processName%_%processId%.dmp) will be created
        // in the temp folder path (%TMP% on windows)
        internal static string CreateMiniDumpFile(string outputDir = null)
        {
            Process process = Process.GetCurrentProcess();
            MINIDUMP_TYPE DumpType = MINIDUMP_TYPE.MiniDumpNormal;

            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Path.GetTempPath();
            }

            var outputFile = Path.Combine(outputDir, process.ProcessName + "_" + process.Id + ".dmp");

            MINIDUMP_EXCEPTION_INFORMATION info = new MINIDUMP_EXCEPTION_INFORMATION
            {
                ClientPointers = 1,
                ExceptionPointers = Marshal.GetExceptionPointers(),
                ThreadId = GetCurrentThreadId()
            };

            using (FileStream dmpFileStream = File.Create(outputFile))
            {
                if (MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), dmpFileStream.SafeFileHandle, DumpType, ref info, IntPtr.Zero, IntPtr.Zero))
                {
                    return outputFile;
                }
            }
            return null;
        }

        internal static void OnCrashReportWindow(CrashReportArgs args)
        {
            try
            {
                DynamoModel.IsCrashing = true;

                var cerDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "DynamoCER_Report_" +
                    DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd-HH-mm-ss")));

                using (Scheduler.Disposable.Create(() => { cerDir.Delete(); }) )
                {
                    var filesToSend = new List<string>();
                    if (args.SendLogFile && args.model != null)
                    {
                        string logFile = Path.Combine(cerDir.FullName, "DynamoLog.log");

                        File.Copy(args.model.Logger.LogPath, logFile);
                        // might be usefull to dump all loaded Packages into
                        // the log at this point.
                        filesToSend.Add(logFile);
                    }

                    if (args.SendSettingsFile && args.model != null)
                    {
                        string settingsFile = Path.Combine(cerDir.FullName, "DynamoSettings.xml");
                        File.Copy(args.model.PathManager.PreferenceFilePath, settingsFile);

                        filesToSend.Add(settingsFile);
                    }

                    if (args.SendDynFile && args.model != null)
                    {
                        var dynFilePath = Path.Combine(cerDir.FullName, "DynamoModel.dyn");
                        args.model.CurrentWorkspace.Save(dynFilePath);

                        filesToSend.Add(dynFilePath);
                    }

                    var extras = string.Join(" ", filesToSend.Select(f => "/EXTRA " + f));

                    string appConfig = "";
                    if (args.model != null)
                    {
                        var appName = string.IsNullOrEmpty(args.model.HostAnalyticsInfo.HostName) ? Process.GetCurrentProcess().ProcessName :
                            args.model.HostAnalyticsInfo.HostName;
                        appConfig = $@"<ProductInformation name=\""{appName}\"" build_version=\""{args.model.Version}\"" " +
                        $@"registry_version=\""{args.model.Version}\"" registry_localeID=\""{CultureInfo.CurrentCulture.LCID}\"" uptime=\""0\"" " +
                        $@"session_start_count=\""0\"" session_clean_close_count=\""0\"" current_session_length=\""0\"" />";
                    }

                    var miniDumpFilePath = CreateMiniDumpFile(cerDir.FullName);
                    var upiConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upiconfig.xml");

                    var cerArgs = $"/UPITOKEN {upiConfigFilePath} /DMP {miniDumpFilePath} /APPXML \"{appConfig}\" {extras}";
                    Process.Start(new ProcessStartInfo(CerToolLocation, cerArgs)).WaitForExit();
                }
            }
            catch(Exception ex)
            {
                args.model?.Logger?.LogError($"Failed to invoke CER with the following error : {ex.Message}");
            }
        }
    }
}

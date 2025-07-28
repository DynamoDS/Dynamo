using Dynamo.Core;
using Dynamo.Models;
using Dynamo.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Dynamo.Wpf.Utilities
{
    internal class CrashReportTool
    {
        private static List<string> ProductsWithCER => new List<string>() { "Revit", "Civil", "Robot Structural Analysis" };
        private static readonly string CERDllName = "cer.dll";

        private static string CERInstallLocation = null;

        private enum MINIDUMP_TYPE
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
        private static extern uint GetCurrentThreadId();

        [DllImport("dbghelp.dll")]
        private static extern bool MiniDumpWriteDump(
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
        private struct MINIDUMP_EXCEPTION_INFORMATION
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

            var outputFile = Path.Combine(outputDir, $"{process.ProcessName}_{process.Id}.dmp"); ;

            MINIDUMP_EXCEPTION_INFORMATION info = new MINIDUMP_EXCEPTION_INFORMATION
            {
                ClientPointers = 1,
                ExceptionPointers = Marshal.GetExceptionPointers(),
                ThreadId = GetCurrentThreadId()//The windows thread id (as opposed to Thread.CurrentThread.ManagedThreadId)
            };

            using (FileStream dmpFileStream = File.Create(outputFile))
            {
                if (MiniDumpWriteDump(process.Handle, (uint)process.Id, dmpFileStream.SafeFileHandle, DumpType, ref info, IntPtr.Zero, IntPtr.Zero))
                {
                    return outputFile;
                }
            }
            return null;
        }

        private static string FindCERToolInInstallLocations()
        {
            if (CERInstallLocation != null) return CERInstallLocation;

            try
            {

                // Try to directly get the type for the detective class
                // (this will in many cases also load the assembly)
                var type = Type.GetType("DynamoInstallDetective.Utilities, DynamoInstallDetective", false);
                if (type == null)
                {
                    // fallback, load the assembly and get the type using LoadFrom
                    string rootFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var assemblyPath = Path.Combine(rootFolder, "DynamoInstallDetective.dll");
                    if (!File.Exists(assemblyPath))
                        throw new FileNotFoundException(assemblyPath);

                    var assembly = Assembly.LoadFrom(assemblyPath);
                    type = assembly.GetType("DynamoInstallDetective.Utilities");
                }

                var installationsMethod = type.GetMethod(   
                    "FindMultipleProductInstallations",
                    BindingFlags.Public | BindingFlags.Static);

                if (installationsMethod == null)
                {
                    throw new MissingMethodException("Method 'DynamoInstallDetective.Utilities.FindProductInstallations' not found");
                }

                var methodParams = new object[] { ProductsWithCER, CERDllName };
                var installs = installationsMethod.Invoke(null, methodParams) as IEnumerable;

                CERInstallLocation = installs.Cast<KeyValuePair<string, Tuple<int, int, int, int>>>().Select(x => x.Key).LastOrDefault() ?? string.Empty;
                return CERInstallLocation;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static void ShowCrashWindow(object sender, CrashPromptArgs args)
        {
            if (CrashReportTool.ShowCrashErrorReportWindow(sender,
                (args is CrashErrorReportArgs cerArgs) ? cerArgs :
                new CrashErrorReportArgs(args.Details)))
            {
                return;
            }
            // Backup crash reporting dialog (in case ADSK CER is not found)
            var prompt = new Nodes.Prompts.CrashPrompt(sender, args);
            prompt.ShowDialog();
        }

        /// <summary>
        /// Calls external CER tool (with UI)
        /// </summary>
        /// <param name="sender">Either a DynamoViewModel or a DynamoModel object</param>
        /// <param name="args">Arguments consumed by the Crash report UI</param>
        /// <returns>True if the CER tool process was successfully started. False otherwise</returns>
        private static bool ShowCrashErrorReportWindow(object sender, CrashErrorReportArgs args)
        {
            DynamoModel model = null;
            var viewModel = sender as DynamoViewModel;
            if (viewModel != null)
            {
                model = viewModel.Model;
            }
            else if (sender is DynamoModel dm)
            {
                model = dm;
            }

            string cerToolDir = !string.IsNullOrEmpty(model?.CERLocation) ?
                model?.CERLocation : FindCERToolInInstallLocations();

            var cerToolPath = !string.IsNullOrEmpty(cerToolDir) ? Path.Combine(cerToolDir, CERDllName) : string.Empty;

            if (string.IsNullOrEmpty(cerToolPath) || !File.Exists(cerToolPath))
            {
                model?.Logger?.LogError($"The CER tool was not found at location {cerToolPath}");
                return false;
            }

            try
            {
                DynamoModel.IsCrashing = true;

                var cerDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "DynamoCER_Report_" +
                    DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd-HH-mm-ss")));

                var filesToSend = new List<string>();
                if (args.SendLogFile && model != null)
                {
                    string logFile = Path.Combine(cerDir.FullName, "DynamoLog.log");

                    if(File.Exists(model.Logger.LogPath))
                    {
                        File.Copy(model.Logger.LogPath, logFile);
                        filesToSend.Add(logFile);
                    }
                    else
                    {
                        model?.Logger?.LogError($"Failed to add DynamoLog.log to CER with the following error : {model.Logger.LogPath} Not Found");
                    }
                }

                if (args.SendSettingsFile && model != null)
                {
                    string settingsFile = Path.Combine(cerDir.FullName, "DynamoSettings.xml");

                    if (File.Exists(model.PathManager.PreferenceFilePath))
                    {
                        File.Copy(model.PathManager.PreferenceFilePath, settingsFile);

                        filesToSend.Add(settingsFile);
                    }
                    else
                    {
                        model?.Logger?.LogError($"Failed to add DynamoSettings.xml to CER with the following error : {model.PathManager.PreferenceFilePath} Not Found");
                    }
                }

                if (args.HasDetails())
                {
                    var stackTracePath = Path.Combine(cerDir.FullName, "StackTrace.log");
                    try
                    {
                        File.WriteAllText(stackTracePath, args.Details);
                        filesToSend.Add(stackTracePath);
                    }
                    catch (Exception ex)
                    {
                        model?.Logger?.LogError($"Failed to add StackTrace.log to CER with the following error : {ex.Message}");
                    }
                }

                if (args.SendRecordedCommands && viewModel != null)
                {
                    try
                    {
                        filesToSend.Add(viewModel.DumpRecordedCommands());
                    }
                    catch (Exception ex)
                    {
                        model?.Logger?.LogError($"Failed to add recorded commands to CER with the following error : {ex.Message}");
                    }
                }

                string appConfig = "";
                if (model != null)
                {
                    var appName = GetHostAppName();
                    appConfig = $"<ProductInformation name=\"{appName}\" build_version=\"{DynamoModel.Version}\" " +
                                $"registry_version=\"{DynamoModel.Version}\" registry_localeID=\"{CultureInfo.CurrentCulture.LCID}\" uptime=\"0\" " +
                                $"session_start_count=\"0\" session_clean_close_count=\"0\" current_session_length=\"0\" />";
                }

                string dynName = model?.CurrentWorkspace.Name;

                var miniDumpFilePath = string.Empty;
                try
                {
                    miniDumpFilePath = CreateMiniDumpFile(cerDir.FullName);
                }
                catch (Exception ex)
                {
                    model?.Logger?.LogError($"Failed to generate minidump file for CER due to the following error : {ex.Message}");
                }

                if (string.IsNullOrEmpty(miniDumpFilePath))
                {
                    return false;
                }

                var upiConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upiconfig.xml");

                using (var cerDLL = new CerDLL(cerToolPath)) 
                {
                    cerDLL.ToggleCER(true);
                    cerDLL.RegisterUPI(upiConfigFilePath);

                    if (!string.IsNullOrEmpty(dynName))
                    {
                        cerDLL.SetStringParam(ReportStringParamKey.StringKeyDwg, dynName);
                    }

                    foreach (var file in filesToSend)
                    {
                        cerDLL.SetMultiStringParam(ReportMultiStringParamKey.MultiStringKeyExtraFile, file, filesToSend.Count);
                    }

                    cerDLL.SetMultiStringParam(ReportMultiStringParamKey.MultiStringKeyAppXML, appConfig, 1);
                    cerDLL.SetBoolParam(ReportBoolParamKey.BoolKeyUseExceptionTrace, true);
                    var success = cerDLL.SendReportWithDump(miniDumpFilePath, true);
                    model?.Logger?.LogError(success
                        ? $"Successfully sent CER error report"
                        : $"Failed to send CER error report");
                }
                
                return true;
            }
            catch(Exception ex)
            {
                model?.Logger?.LogError($"Failed to invoke CER with the following error : {ex.Message}");
            }
            return false;
        }

        internal static string GetHostAppName()
        {
            //default to app name being process name, but prefer HostAnalyticsInfo.HostName
            //then legacy Model.HostName
            var appName = Process.GetCurrentProcess().ProcessName;
            if (!string.IsNullOrEmpty(DynamoModel.HostAnalyticsInfo.HostName))
            {
                appName = DynamoModel.HostAnalyticsInfo.HostName;
            }

            return appName;
        }
    }
}

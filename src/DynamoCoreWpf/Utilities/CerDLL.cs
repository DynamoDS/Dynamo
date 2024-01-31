using System;
using System.Runtime.InteropServices;

namespace Dynamo.Wpf.Utilities
{
    internal class DLL
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    internal class CerDLL : IDisposable
    {
        private IntPtr m_dll = IntPtr.Zero;

        public string DllFilePath { get; }

        public CerDLL(string dllFilePath)
        {
            DllFilePath = dllFilePath;
        }

        public void Dispose()
        {
            if (m_dll != IntPtr.Zero)
            {
                DLL.FreeLibrary(m_dll);
                m_dll = IntPtr.Zero;
            }
            Initialized = false;
        }

        private static bool Initialized;
        private static readonly object InitLocker = new();
        private bool Init()
        {
            if (!Initialized)
            {
                lock (InitLocker)
                {
                    if (!Initialized)
                    {
                        if (m_dll == IntPtr.Zero)
                            m_dll = DLL.LoadLibrary(DllFilePath);
                        Initialized = true;
                    }
                }
            }

            return m_dll != IntPtr.Zero;
        }

        private TDelegate GetDelegate<TDelegate>() where TDelegate : Delegate

        {
            IntPtr funcAddr = DLL.GetProcAddress(m_dll, typeof(TDelegate).Name);
            if (funcAddr == IntPtr.Zero)
                return null;

            return Marshal.GetDelegateForFunctionPointer<TDelegate>(funcAddr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_EnableUnhandledExceptionFilter();
        public void EnableUnhandledExceptionFilter()
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_EnableUnhandledExceptionFilter>();
            if (func == null)
                return;
            func();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_ToggleCER(bool enable);
        public void ToggleCER(bool enable)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_ToggleCER>();
            if (func == null)
                return;
            func(enable);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CER_IsCEREnabled();
        public bool IsCEREnabled()
        {
            if (!Init())
                return false;

            var func = GetDelegate<CER_IsCEREnabled>();
            if (func == null)
                return false;
            return func();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetSenddmpPath(string senddmpExePath);
        public void SetSenddmpPath(string senddmpExePath)
        {
            if (!Init())
                return;


            var func = GetDelegate<CER_SetSenddmpPath>();
            if (func == null)
                return;
            func(senddmpExePath);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_RegisterUPI(string upiConfigFilePath);
        public void RegisterUPI(string upiConfigFilePath)
        {
            if (!Init())
                return;


            var func = GetDelegate<CER_RegisterUPI>();
            if (func == null)
                return;
            func(upiConfigFilePath);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CER_SendReport(IntPtr exceptionPointers, bool suspendProcess);
        public bool SendReport(Exception e, bool suspendProcess)
        {
            if (!Init())
                return false;

            var exceptionPointers = Marshal.GetExceptionPointers();
            if (exceptionPointers == IntPtr.Zero)
            {
                try
                {
                    var exInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e);
                    exInfo.Throw();
                }
                catch
                {
                    exceptionPointers = Marshal.GetExceptionPointers();
                }
            }

            var func = GetDelegate<CER_SendReport>();
            if (func == null)
                return false;
            return func(exceptionPointers, suspendProcess);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CER_SendReportWithDump(string dumpFile, bool suspendProcess);
        public bool SendReportWithDump(string dumpFile, bool suspendProcess)
        {
            if (!Init())
                return false;

            var func = GetDelegate<CER_SendReportWithDump>();
            if (func == null)
                return false;
            return func(dumpFile, suspendProcess);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetTheme(int theme);
        public void SetTheme(int theme)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetTheme>();
            if (func == null)
                return;
            func(theme);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetMultiStringParam(int key, string value, int maxCount);
        public void SetMultiStringParam(ReportMultiStringParamKey key, string value, int maxCount)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetMultiStringParam>();
            if (func == null)
                return;
            func((int)key, value, maxCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetStringParam(int key, string value);
        public void SetStringParam(ReportStringParamKey key, string value)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetStringParam>();
            if (func == null)
                return;
            func((int)key, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetIntParam(int key, int value);
        public void SetIntParam(ReportIntParamKey key, int value)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetIntParam>();
            if (func == null)
                return;
            func((int)key, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetBoolParam(int key, bool value);
        public void SetBoolParam(ReportBoolParamKey key, bool value)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetBoolParam>();
            if (func == null)
                return;
            func((int)key, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetAdditionalStringParam(string key, string value, int maxCount);
        public void SetAdditionalStringParam(string key, string value, int maxCount)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetAdditionalStringParam>();
            if (func == null)
                return;
            func(key, value, maxCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetAdditionalIntParam(string key, int value, int maxCount);
        public void SetAdditionalIntParam(string key, int value, int maxCount)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetAdditionalIntParam>();
            if (func == null)
                return;
            func(key, value, maxCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetAdditionalBoolParam(string key, bool value, int maxCount);
        public void SetAdditionalBoolParam(string key, bool value, int maxCount)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetAdditionalBoolParam>();
            if (func == null)
                return;
            func(key, value, maxCount);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_RemoveAdditionalParam(string key);
        public void RemoveAdditionalParam(string key)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_RemoveAdditionalParam>();
            if (func == null)
                return;
            func(key);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CER_SetLegacyOptionsByString(string str);
        public void SetLegacyOptionsByString(string str)
        {
            if (!Init())
                return;

            var func = GetDelegate<CER_SetLegacyOptionsByString>();
            if (func == null)
                return;
            func(str);
        }
    }

    internal enum ThemeType : int
    {
        Light,
        Dark,
        Blue,
    };

    internal enum ReportStringParamKey : int
    {
        StringKeyBegin = 10000,
        StringKeyUpiToken, // will be deprecated, please call RegisterUPI function as instead
        StringKeyCalUptime,
        StringKeyProductKey,
        StringKeySerialNum,
        StringKeyFeatureName,
        StringKeyFeatureVer,
        StringKeyLicenseBehavior,
        StringKeyLicExp,
        StringKeyLicUsage,
        StringKeyDwg,
        StringKeyAutoSend,
        StringKeyAppXMLFile,
        StringKeyAvailPhysicalMem,
        StringKeyAvailPageFile,
        StringKeyAvailVirtualMem,
        StringKeyLastError,
        StringKeyErrNo,
        StringKeyUserSubscriptionEmail,
        StringKeyErrorDescription,
        StringKeyGraphicsDriver,
        StringKeyCadSettingsRegPath,
        StringKeyOptionsFile,
        StringKeyMc3SessionId,
        StringKeyMc3UserId,
        StringKeyAppIcon,
        StringKeyGdiObjects,
        StringKeyGdiUserObjects,
        StringKeyGdiObjectsPeak,
        StringKeyGdiUserObjectsPeak,
        StringKeyUnknown = 19998,
        StringKeyEnd = 19999
    };

    internal enum ReportMultiStringParamKey
    {
        MultiStringKeyBegin = 20000,
        MultiStringKeyAppCData,
        MultiStringKeyAppName,
        MultiStringKeyAppXML,
        MultiStringKeyExtraFile,
        MultiStringKeyLastCommand,
        MultiStringKeyUnknown = 29998,
        MultiStringKeyEnd = 29999
    };

    internal enum ReportIntParamKey : int
    {
        IntKeyBegin = 30000,
        IntKeyCrashCount,
        IntKeyAppLocaleId,
        IntKeyAppIconId,
        IntKeyUnknown = 39998,
        IntKeyEnd = 39999
    };

    internal enum ReportBoolParamKey : int
    {
        BoolKeyBegin = 40000,
        BoolKeyAltForceSend,
        BoolKeyUseExceptionTrace,
        BoolKeyUnknown = 49998,
        BoolKeyEnd = 49999
    };
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUtilities
{
    internal static class WinTrustWrapper
    {
        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        private static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

        private static uint WinVerifyTrust(string fileName)
        {
            Guid wintrust_action_generic_verify_v2 = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
            uint result = 0;
            using (WINTRUST_FILE_INFO fileInfo = new WINTRUST_FILE_INFO(fileName, Guid.Empty))
            using (UnmanagedPointer guidPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid))), AllocMethod.HGlobal))
            using (UnmanagedPointer wvtDataPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_DATA))), AllocMethod.HGlobal))
            {
                WINTRUST_DATA data = new WINTRUST_DATA(fileInfo);
                IntPtr pGuid = guidPtr;
                IntPtr pData = wvtDataPtr;
                Marshal.StructureToPtr(wintrust_action_generic_verify_v2, pGuid, true);
                Marshal.StructureToPtr(data, pData, true);
                result = WinVerifyTrust(IntPtr.Zero, pGuid, pData);
            }

            return result;
        }

        public static bool IsTrusted(string fileName)
        {
            return WinVerifyTrust(fileName) == 0;
        }
    }

    internal struct WINTRUST_FILE_INFO : IDisposable
    {
        public WINTRUST_FILE_INFO(string fileName, Guid subject)
        {
            cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO));

            pcwszFilePath = fileName;

            if (subject != Guid.Empty)
            {
                pgKnownSubject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));

                Marshal.StructureToPtr(subject, pgKnownSubject, true);
            }
            else
            {
                pgKnownSubject = IntPtr.Zero;
            }

            hFile = IntPtr.Zero;
        }

        public uint cbStruct;

        [MarshalAs(UnmanagedType.LPTStr)]

        public string pcwszFilePath;

        public IntPtr hFile;

        public IntPtr pgKnownSubject;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (pgKnownSubject != IntPtr.Zero)
            {
                Marshal.DestroyStructure(this.pgKnownSubject, typeof(Guid));

                Marshal.FreeHGlobal(this.pgKnownSubject);
            }
        }

        #endregion
    }

    enum AllocMethod
    {
        HGlobal,
        CoTaskMem
    };

    enum UnionChoice
    {
        File = 1,
        Catalog,
        Blob,
        Signer,
        Cert
    }

    enum UiChoice
    {
        All = 1,
        NoUI,
        NoBad,
        NoGood
    }

    enum RevocationCheckFlags
    {
        None = 0,
        WholeChain
    }

    enum StateAction
    {
        Ignore = 0,
        Verify,
        Close,
        AutoCache,
        AutoCacheFlush
    }

    enum TrustProviderFlags
    {
        UseIE4Trust = 1,
        NoIE4Chain = 2,
        NoPolicyUsage = 4,
        RevocationCheckNone = 16,
        RevocationCheckEndCert = 32,
        RevocationCheckChain = 64,
        RecovationCheckChainExcludeRoot = 128,
        Safer = 256,
        HashOnly = 512,
        UseDefaultOSVerCheck = 1024,
        LifetimeSigning = 2048
    }

    enum UIContext
    {
        Execute = 0,
        Install
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINTRUST_DATA : IDisposable
    {
        public WINTRUST_DATA(WINTRUST_FILE_INFO fileInfo)
        {
            this.cbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA));

            pInfoStruct = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));

            Marshal.StructureToPtr(fileInfo, pInfoStruct, false);

            this.dwUnionChoice = UnionChoice.File;

            pPolicyCallbackData = IntPtr.Zero;

            pSIPCallbackData = IntPtr.Zero;

            dwUIChoice = UiChoice.NoUI;

            fdwRevocationChecks = RevocationCheckFlags.None;

            dwStateAction = StateAction.Ignore;

            hWVTStateData = IntPtr.Zero;

            pwszURLReference = IntPtr.Zero;

            dwProvFlags = TrustProviderFlags.Safer;

            dwUIContext = UIContext.Execute;
        }

        public uint cbStruct;

        public IntPtr pPolicyCallbackData;

        public IntPtr pSIPCallbackData;

        public UiChoice dwUIChoice;

        public RevocationCheckFlags fdwRevocationChecks;

        public UnionChoice dwUnionChoice;

        public IntPtr pInfoStruct;

        public StateAction dwStateAction;

        public IntPtr hWVTStateData;

        private IntPtr pwszURLReference;

        public TrustProviderFlags dwProvFlags;

        public UIContext dwUIContext;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (dwUnionChoice == UnionChoice.File)
            {
                WINTRUST_FILE_INFO info = new WINTRUST_FILE_INFO();

                Marshal.PtrToStructure(pInfoStruct, info);

                info.Dispose();

                Marshal.DestroyStructure(pInfoStruct, typeof(WINTRUST_FILE_INFO));
            }

            Marshal.FreeHGlobal(pInfoStruct);
        }

        #endregion
    }

    internal sealed class UnmanagedPointer : IDisposable
    {
        private IntPtr m_ptr;

        private AllocMethod m_meth;

        internal UnmanagedPointer(IntPtr ptr, AllocMethod method)
        {
            m_meth = method;
            m_ptr = ptr;
        }

        ~UnmanagedPointer()
        {
            Dispose(false);
        }

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (m_ptr != IntPtr.Zero)
            {
                if (m_meth == AllocMethod.HGlobal)
                {
                    Marshal.FreeHGlobal(m_ptr);
                }
                else if (m_meth == AllocMethod.CoTaskMem)
                {
                    Marshal.FreeCoTaskMem(m_ptr);
                }

                m_ptr = IntPtr.Zero;
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public static implicit operator IntPtr(UnmanagedPointer ptr)
        {
            return ptr.m_ptr;
        }
    }
    
    public class CertificateVerification
    {
        /// <summary>
        /// Check if a .NET assembly can be loaded and has a valid certificate
        /// </summary>
        /// <param name="assemblyPath">Path of the assembly file</param>
        /// <returns></returns>
        public static bool CheckAssemblyForValidCertificate(string assemblyPath)
        {
            //Verify the assembly exists
            if (!File.Exists(assemblyPath))
            {
                throw new Exception(String.Format(
                    "A dll file was not found at {0}. No certificate was able to be verified.", assemblyPath));
            }

            //Verify the node library file has a verified signed certificate
            try
            {
                var validCert = WinTrustWrapper.IsTrusted(assemblyPath);
                if (validCert)
                {
                    return true;
                }
            }
            catch
            {
                throw new Exception(String.Format(
                    "A dll file found at {0} did not have a signed certificate.", assemblyPath));
            }

            throw new Exception(String.Format(
                "A dll file found at {0} did not have a signed certificate.", assemblyPath));
        }
    }
}

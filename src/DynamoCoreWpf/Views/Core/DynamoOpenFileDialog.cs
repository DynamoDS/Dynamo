using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Microsoft.Win32;

namespace Dynamo.UI
{
    class DynamoOpenFileDialog
    {
        private NativeFileOpenDialog _dialog;
        private DynamoViewModel model;
        private string _fileName = string.Empty;
        private bool _runManualMode = false;
        private readonly bool enableCustomDialog; // Used in 'Insert' - optional boolean parameter to skip addition of the check button
        private const int RunManualCheckboxId = 0x1001;

        public string Filter
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string[] filterElements = value.Split(new char[] { '|' });
                    COMDLG_FILTERSPEC[] filter = new COMDLG_FILTERSPEC[filterElements.Length / 2];
                    for (int x = 0; x < filterElements.Length; x += 2)
                    {
                        filter[x / 2].pszName = filterElements[x];
                        filter[x / 2].pszSpec = filterElements[x + 1];
                    }
                    _dialog.SetFileTypes((uint)filter.Length, filter);
                }
            }
        }

        public string Title
        {
            set { _dialog.SetTitle(value); }
        }

        public string InitialDirectory
        {
            set
            {
                object item;
                // IShellItem GUID
                Guid guid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
                int hresult = SHCreateItemFromParsingName(value, IntPtr.Zero, ref guid, out item);
                if (hresult != 0)
                    throw new System.ComponentModel.Win32Exception(hresult);

                _dialog.SetFolder((IShellItem) item);
            }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public bool RunManualMode
        {
            get { return _runManualMode; }
        }

        public DynamoOpenFileDialog(DynamoViewModel model, bool enableCustomDialog = true)
        {
            this.model = model;
            this.enableCustomDialog = enableCustomDialog;
            _dialog = new NativeFileOpenDialog();
            if (!enableCustomDialog) return;
            IFileDialogCustomize customize = (IFileDialogCustomize) _dialog;
            customize.AddCheckButton(RunManualCheckboxId,
                Dynamo.Wpf.Properties.Resources.FileDialogManualMode,
                model.PreferenceSettings.OpenFileInManualExecutionMode);
        }

        public DialogResult ShowDialog()
        {
            try
            {
                int result = _dialog.Show(GetActiveWindow());
                if (result < 0)
                {
                    if ((uint)result == (uint)HRESULT.E_CANCELLED)
                        return DialogResult.Cancel;
                    throw Marshal.GetExceptionForHR(result);
                }

                IShellItem dialogResult;
                _dialog.GetResult(out dialogResult);
                dialogResult.GetDisplayName(SIGDN.SIGDN_DESKTOPABSOLUTEEDITING, out _fileName);

                IFileDialogCustomize customize = (IFileDialogCustomize)_dialog;
                if (!enableCustomDialog) return DialogResult.OK;

                customize.GetCheckButtonState(RunManualCheckboxId, out _runManualMode);
                model.PreferenceSettings.OpenFileInManualExecutionMode = _runManualMode;

                return DialogResult.OK;
            }
            catch(Exception ex)
            {
                model.Model.Logger.Log(ex.Message);
                return DialogResult.Cancel;
            }
        }
        /// <summary>
        /// The method is used to get the last accessed path by the user
        /// </summary>
        /// <returns>The last accessed path by the user, can be null</returns>
        internal string GetLastAccessedPath()
        {
            return ApplicationGetLastOpenSavePath(Path.GetFileName(Environment.ProcessPath));
        }
        /// <summary>
        /// Fetches last accessed location from Windows registry
        /// Solution taken from : https://stackoverflow.com/a/61583119
        /// </summary>
        private string ApplicationGetLastOpenSavePath(string executableName)
        {
            if (string.IsNullOrEmpty(executableName)) return null;
            string lastVisitedPath = string.Empty;
            try
            {
                var lastVisitedKey = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU", false);

                string[] values = lastVisitedKey.GetValueNames();
                foreach (string value in values)
                {
                    if (value == "MRUListEx") continue;
                    var keyValue = (byte[])lastVisitedKey.GetValue(value);

                    string appName = Encoding.Unicode.GetString(keyValue, 0, executableName.Length * 2);
                    if (!appName.Equals(executableName)) continue;

                    int offset = executableName.Length * 2 + "\0\0".Length;  // clearly null terminated :)
                    lastVisitedPath = GetPathFromIDList(keyValue, offset);
                    break;
                }
            }
            catch (Exception)
            {
                //let the method return empty string in case of exception
            }
            return lastVisitedPath;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        private string GetPathFromIDList(byte[] idList, int offset)
        {
            try
            {
                int buffer = 520;  // 520 = MAXPATH * 2
                var sb = new StringBuilder(buffer);

                IntPtr ptr = Marshal.AllocHGlobal(idList.Length);
                Marshal.Copy(idList, offset, ptr, idList.Length - offset);

                // or -> bool result = SHGetPathFromIDListW(ptr, sb);
                bool result = SHGetPathFromIDListEx(ptr, sb, buffer, GPFIDL_FLAGS.GPFIDL_UNCPRINTER);
                Marshal.FreeHGlobal(ptr);
                return result ? sb.ToString() : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        [DllImport("shell32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern bool SHGetPathFromIDListW(
            IntPtr pidl,
            [MarshalAs(UnmanagedType.LPTStr)]
    StringBuilder pszPath);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        internal static extern bool SHGetPathFromIDListEx(
            IntPtr pidl,
            [MarshalAs(UnmanagedType.LPTStr)]
    [In,Out] StringBuilder pszPath,
            int cchPath,
            GPFIDL_FLAGS uOpts);

        internal enum GPFIDL_FLAGS : uint
        {
            GPFIDL_DEFAULT = 0x0000,
            GPFIDL_ALTNAME = 0x0001,
            GPFIDL_UNCPRINTER = 0x0002
        }
    }

    public class DynamoFolderBrowserDialog
    {
        public string Title { get; set; }
        public Window Owner { get; set; }
        public string SelectedPath { get; set; }

        public DialogResult ShowDialog()
        {
            NativeFileOpenDialog dialog = null;

            try
            {
                // If the caller did not specify a starting path, or set it to null,
                // it is not healthy as it causes SHCreateItemFromParsingName to
                // throw E_INVALIDARG (0x80070057). Setting it to an empty string.
                //
                if (SelectedPath == null)
                    SelectedPath = string.Empty;

                dialog = new NativeFileOpenDialog();

                dialog.SetTitle(Title);

                object shellItem;
                // IShellItem GUID
                Guid guid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE");
                int hresult = SHCreateItemFromParsingName(SelectedPath, IntPtr.Zero, ref guid, out shellItem);
                if ((uint)hresult != (uint)HRESULT.S_OK)
                    throw Marshal.GetExceptionForHR(hresult);
                dialog.SetFolder((IShellItem)shellItem);

                dialog.SetOptions(FOS.FOS_PICKFOLDERS | FOS.FOS_FORCEFILESYSTEM | FOS.FOS_FILEMUSTEXIST);

                IntPtr hWnd = new WindowInteropHelper(Owner).Handle;
                hresult = dialog.Show(hWnd);
                if (hresult < 0)
                {
                    if ((uint)hresult == (uint)HRESULT.E_CANCELLED)
                        return DialogResult.Cancel;
                    throw Marshal.GetExceptionForHR(hresult);
                }

                string path;
                IShellItem item;
                dialog.GetResult(out item);
                item.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out path);
                SelectedPath = path;

                return DialogResult.OK;
            }
            finally
            {
                if (dialog != null)
                    Marshal.FinalReleaseComObject(dialog);
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);
    }

    [ComImport]
    [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
    [CoClass(typeof(FileOpenDialogRCW))]
    internal interface NativeFileOpenDialog : IFileOpenDialog { }

    [ComImport]
    [ClassInterface(ClassInterfaceType.None)]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
    internal class FileOpenDialogRCW { }
}

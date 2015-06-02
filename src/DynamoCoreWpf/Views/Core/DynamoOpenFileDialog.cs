using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Dynamo.Wpf.Interfaces;
using Dynamo.ViewModels;

namespace Dynamo.UI
{
    class DynamoOpenFileDialog
    {
        private NativeFileOpenDialog _dialog;
        private DynamoViewModel model;
        private string _fileName = string.Empty;
        private bool _runManualMode = false;
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

        public DynamoOpenFileDialog(DynamoViewModel model)
        {
            this.model = model;
            _dialog = new NativeFileOpenDialog();
            IFileDialogCustomize customize = (IFileDialogCustomize) _dialog;
            customize.AddCheckButton(RunManualCheckboxId, 
                Dynamo.Wpf.Properties.Resources.FileDialogManualMode,
                model.PreferenceSettings.OpenFileInManualExecutionMode);
        }

        public DialogResult ShowDialog()
        {
            int result = _dialog.Show(GetActiveWindow());
            if (result < 0)
            {
                if ((uint) result == (uint) HRESULT.E_CANCELLED)
                    return DialogResult.Cancel;
                throw Marshal.GetExceptionForHR(result);
            }

            IShellItem dialogResult;
            _dialog.GetResult(out dialogResult);
            dialogResult.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out _fileName);

            IFileDialogCustomize customize = (IFileDialogCustomize) _dialog;
            customize.GetCheckButtonState(RunManualCheckboxId, out _runManualMode);
            model.PreferenceSettings.OpenFileInManualExecutionMode = _runManualMode;

            return DialogResult.OK;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();
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

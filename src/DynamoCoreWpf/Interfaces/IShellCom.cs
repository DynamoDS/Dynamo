using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Dynamo.Wpf.Interfaces
{
    // https://msdn.microsoft.com/en-us/library/windows/desktop/bb762502(v=vs.85).aspx
    public enum FDAP
    {
        FDAP_BOTTOM = 0x00000000,
        FDAP_TOP = 0x00000001,
    }

    // http://www.pinvoke.net/default.aspx/Enums/FDE_SHAREVIOLATION_RESPONSE.html
    public enum FDE_SHAREVIOLATION_RESPONSE
    {
        FDESVR_DEFAULT = 0x00000000,
        FDESVR_ACCEPT = 0x00000001,
        FDESVR_REFUSE = 0x00000002
    }

    // http://www.pinvoke.net/default.aspx/Enums.FDE_OVERWRITE_RESPONSE
    public enum FDE_OVERWRITE_RESPONSE
    {
        FDEOR_DEFAULT = 0x00000000,
        FDEOR_ACCEPT = 0x00000001,
        FDEOR_REFUSE = 0x00000002
    }

    // http://www.pinvoke.net/default.aspx/Enums.SIATTRIBFLAGS
    public enum SIATTRIBFLAGS
    {
        SIATTRIBFLAGS_AND = 0x00000001,
        SIATTRIBFLAGS_OR = 0x00000002,
        SIATTRIBFLAGS_APPCOMPAT = 0x00000003,
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/bb762544(v=vs.85).aspx
    public enum SIGDN : uint
    {
        SIGDN_NORMALDISPLAY = 0x00000000,
        SIGDN_PARENTRELATIVEPARSING = 0x80018001,
        SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
        SIGDN_PARENTRELATIVEEDITING = 0x80031001,
        SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
        SIGDN_FILESYSPATH = 0x80058000,
        SIGDN_URL = 0x80068000,
        SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
        SIGDN_PARENTRELATIVE = 0x80080001,
        SIGDN_PARENTRELATIVEFORUI = 0x80094001
    }

    // http://www.pinvoke.net/default.aspx/Enums.FOS
    [Flags]
    public enum FOS : uint
    {
        FOS_OVERWRITEPROMPT = 0x00000002,
        FOS_STRICTFILETYPES = 0x00000004,
        FOS_NOCHANGEDIR = 0x00000008,
        FOS_PICKFOLDERS = 0x00000020,
        FOS_FORCEFILESYSTEM = 0x00000040,
        FOS_ALLNONSTORAGEITEMS = 0x00000080,
        FOS_NOVALIDATE = 0x00000100,
        FOS_ALLOWMULTISELECT = 0x00000200,
        FOS_PATHMUSTEXIST = 0x00000800,
        FOS_FILEMUSTEXIST = 0x00001000,
        FOS_CREATEPROMPT = 0x00002000,
        FOS_SHAREAWARE = 0x00004000,
        FOS_NOREADONLYRETURN = 0x00008000,
        FOS_NOTESTFILECREATE = 0x00010000,
        FOS_HIDEMRUPLACES = 0x00020000,
        FOS_HIDEPINNEDPLACES = 0x00040000,
        FOS_NODEREFERENCELINKS = 0x00100000,
        FOS_DONTADDTORECENT = 0x02000000,
        FOS_FORCESHOWHIDDEN = 0x10000000,
        FOS_DEFAULTNOMINIMODE = 0x20000000
    }

    // http://www.pinvoke.net/default.aspx/Enums.CDCONTROLSTATE
    public enum CDCONTROLSTATE
    {
        CDCS_INACTIVE = 0x00000000,
        CDCS_ENABLED = 0x00000001,
        CDCS_VISIBLE = 0x00000002
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/bb762505(v=vs.85).aspx
    public enum FFFP_MODE
    {
        FFFP_EXACTMATCH = 0,
        FFFP_NEARESTPARENTMATCH = 1
    }

    // http://www.pinvoke.net/default.aspx/Enums.KF_CATEGORY
    public enum KF_CATEGORY
    {
        KF_CATEGORY_VIRTUAL = 0x00000001,
        KF_CATEGORY_FIXED = 0x00000002,
        KF_CATEGORY_COMMON = 0x00000003,
        KF_CATEGORY_PERUSER = 0x00000004
    }

    // http://www.pinvoke.net/default.aspx/Enums.KF_DEFINITION_FLAGS
    [Flags]
    public enum KF_DEFINITION_FLAGS
    {
        KFDF_PERSONALIZE = 0x00000001,
        KFDF_LOCAL_REDIRECT_ONLY = 0x00000002,
        KFDF_ROAMABLE = 0x00000004,
    }

    // http://www.pinvoke.net/default.aspx/Enums/HRESULT.html
    public enum HRESULT : long
    {
        S_FALSE = 0x0001,
        S_OK = 0x0000,
        E_INVALIDARG = 0x80070057,
        E_OUTOFMEMORY = 0x8007000E,
        E_CANCELLED = 0x800704C7
    }

    // http://www.pinvoke.net/default.aspx/Structures.COMDLG_FILTERSPEC
    [StructLayout ( LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4 )]
    struct COMDLG_FILTERSPEC
    {
        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszName;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszSpec;
    }

    // http://www.pinvoke.net/default.aspx/Interfaces/KNOWNFOLDER_DEFINITION.html
    [StructLayout ( LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4 )]
    struct KNOWNFOLDER_DEFINITION
    {
        public KF_CATEGORY category;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszName;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszCreator;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszDescription;

        public Guid fidParent;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszRelativePath;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszParsingName;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszToolTip;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszLocalizedName;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszIcon;

        [MarshalAs ( UnmanagedType.LPWStr )]
        public string pszSecurity;

        public uint dwAttributes;
        public KF_DEFINITION_FLAGS kfdFlags;
        public Guid ftidType;
    }

    // http://www.pinvoke.net/default.aspx/Structures.PROPERTYKEY
    [StructLayout ( LayoutKind.Sequential, Pack = 4 )]
    struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
    }

    // http://www.pinvoke.net/default.aspx/Interfaces.IModalWindow
    [ComImport]
    [Guid("B4DB1657-70D7-485E-8E3E-6FCB5A5C1802")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IModalWindow
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
        int Show([In] IntPtr parent);
    }


    // http://www.pinvoke.net/default.aspx/Interfaces.IFileDialog
    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileDialog : IModalWindow
    {
        // Defined on IModalWindow - repeated here due to requirements of COM interop layer
        // --------------------------------------------------------------------------------
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
        int Show ( [In] IntPtr parent );

        // IFileDialog-Specific interface members
        // --------------------------------------------------------------------------------
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileTypes ( [In] uint cFileTypes,
                    [In, MarshalAs ( UnmanagedType.LPArray )] COMDLG_FILTERSPEC[] rgFilterSpec );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileTypeIndex ( [In] uint iFileType );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFileTypeIndex ( out uint piFileType );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Advise ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialogEvents pfde, out uint pdwCookie );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Unadvise ( [In] uint dwCookie );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetOptions ( [In] FOS fos );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetOptions ( out FOS pfos );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, FDAP fdap );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetClientGuid ( [In] ref Guid guid );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void ClearClientData ( );

        // Not supported:  IShellItemFilter is not defined, converting to IntPtr
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );
    }

    // http://www.pinvoke.net/default.aspx/Interfaces.IFileOpenDialog
    [ComImport]
    [Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileOpenDialog : IFileDialog
    {
        // Defined on IModalWindow - repeated here due to requirements of COM interop layer
        // --------------------------------------------------------------------------------
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
        int Show ( [In] IntPtr parent );

        // Defined on IFileDialog - repeated here due to requirements of COM interop layer
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileTypes ( [In] uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileTypeIndex ( [In] uint iFileType );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFileTypeIndex ( out uint piFileType );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Advise ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialogEvents pfde, out uint pdwCookie );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Unadvise ( [In] uint dwCookie );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetOptions ( [In] FOS fos );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetOptions ( out FOS pfos );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, FDAP fdap );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetClientGuid ( [In] ref Guid guid );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void ClearClientData ( );

        // Not supported:  IShellItemFilter is not defined, converting to IntPtr
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );

        // Defined by IFileOpenDialog
        // ---------------------------------------------------------------------------------
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetResults ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppenum );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetSelectedItems ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppsai );
    }


    // http://www.pinvoke.net/default.aspx/Interfaces.IFileDialogEvents
    [ComImport]
    [Guid("973510DB-7D7F-452B-8975-74A85828D354")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileDialogEvents
    {
        // NOTE: some of these callbacks are cancelable - returning S_FALSE means that 
        // the dialog should not proceed (e.g. with closing, changing folder); to 
        // support this, we need to use the PreserveSig attribute to enable us to return
        // the proper HRESULT
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
        HRESULT OnFileOk ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ), PreserveSig]
        HRESULT OnFolderChanging ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
                       [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psiFolder );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void OnFolderChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void OnSelectionChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void OnShareViolation ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
                    [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi,
                    out FDE_SHAREVIOLATION_RESPONSE pResponse );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void OnTypeChange ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void OnOverwrite ( [In, MarshalAs ( UnmanagedType.Interface )] IFileDialog pfd,
                   [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi,
                   out FDE_OVERWRITE_RESPONSE pResponse );
    }


    // http://www.pinvoke.net/default.aspx/Interfaces.IShellItem
    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItem {
        void BindToHandler(IntPtr pbc, 
            [MarshalAs(UnmanagedType.LPStruct)]Guid bhid, 
            [MarshalAs(UnmanagedType.LPStruct)]Guid riid, 
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);

        void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);

        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

        void Compare(IShellItem psi, uint hint, out int piOrder);
    }


    // http://www.pinvoke.net/default.aspx/Interfaces.IShellItemArray
    [ComImport]
    [Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItemArray
    {
        // Not supported: IBindCtx
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void BindToHandler ( [In, MarshalAs ( UnmanagedType.Interface )] IntPtr pbc, [In] ref Guid rbhid,
                     [In] ref Guid riid, out IntPtr ppvOut );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetPropertyStore ( [In] int Flags, [In] ref Guid riid, out IntPtr ppv );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetPropertyDescriptionList ( [In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetAttributes ( [In] SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetCount ( out uint pdwNumItems );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetItemAt ( [In] uint dwIndex, [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );

        // Not supported: IEnumShellItems (will use GetCount and GetItemAt instead)
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void EnumItems ( [MarshalAs ( UnmanagedType.Interface )] out IntPtr ppenumShellItems );
    }


    // http://www.pinvoke.net/default.aspx/Interfaces.IFileDialogCustomize
    [ComImport]
    [Guid("8016B7B3-3D49-4504-A0AA-2A37494E606F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IFileDialogCustomize : IFileDialog
    {
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void EnableOpenDropDown ( [In] int dwIDCtl );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddMenu ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddPushButton ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddComboBox ( [In] int dwIDCtl );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddRadioButtonList ( [In] int dwIDCtl );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddCheckButton ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel,
                      [In] bool bChecked );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddEditBox ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddSeparator ( [In] int dwIDCtl );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddText ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetControlLabel ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetControlState ( [In] int dwIDCtl, [Out] out CDCONTROLSTATE pdwState );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetControlState ( [In] int dwIDCtl, [In] CDCONTROLSTATE dwState );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetEditBoxText ( [In] int dwIDCtl, [Out] IntPtr ppszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetEditBoxText ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetCheckButtonState ( [In] int dwIDCtl, [Out] out bool pbChecked );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetCheckButtonState ( [In] int dwIDCtl, [In] bool bChecked );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void AddControlItem ( [In] int dwIDCtl, [In] int dwIDItem,
                      [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void RemoveControlItem ( [In] int dwIDCtl, [In] int dwIDItem );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void RemoveAllControlItems ( [In] int dwIDCtl );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetControlItemState ( [In] int dwIDCtl, [In] int dwIDItem, [Out] out CDCONTROLSTATE pdwState );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetControlItemState ( [In] int dwIDCtl, [In] int dwIDItem, [In] CDCONTROLSTATE dwState );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void GetSelectedControlItem ( [In] int dwIDCtl, [Out] out int pdwIDItem );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void SetSelectedControlItem ( [In] int dwIDCtl, [In] int dwIDItem ); // Not valid for OpenDropDown
        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void StartVisualGroup ( [In] int dwIDCtl, [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void EndVisualGroup ( );

        [MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
        void MakeProminent ( [In] int dwIDCtl );
    }
}

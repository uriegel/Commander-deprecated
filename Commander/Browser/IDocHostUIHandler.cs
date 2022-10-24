using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Commander
{
    [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IDocHostUIHandler
    {
        [PreserveSig]
        uint ShowContextMenu(int dwID, POINT pt, [MarshalAs(UnmanagedType.Interface)] object pcmdtReserved, [MarshalAs(UnmanagedType.Interface)] object pdispReserved);

        [PreserveSig]
        uint GetHostInfo(ref DOCHOSTUIINFO info);

        [PreserveSig]
        uint ShowUI(int dwID, [MarshalAs(UnmanagedType.Interface)] object activeObject, [MarshalAs(UnmanagedType.Interface)] object commandTarget, [MarshalAs(UnmanagedType.Interface)] object frame, [MarshalAs(UnmanagedType.Interface)] object doc);

        [PreserveSig]
        uint HideUI();

        [PreserveSig]
        uint UpdateUI();

        [PreserveSig]
        uint EnableModeless(bool fEnable);

        [PreserveSig]
        uint OnDocWindowActivate(bool fActivate);

        [PreserveSig]
        uint OnFrameWindowActivate(bool fActivate);

        [PreserveSig]
        uint ResizeBorder(COMRECT rect, [MarshalAs(UnmanagedType.Interface)] object doc, bool fFrameWindow);

        [PreserveSig]
        uint TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID);

        [PreserveSig]
        uint GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, int dw);

        [PreserveSig]
        uint GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] object pDropTarget, [MarshalAs(UnmanagedType.Interface)] out object ppDropTarget);

        [PreserveSig]
        uint GetExternal([MarshalAs(UnmanagedType.IDispatch)] out object ppDispatch);

        [PreserveSig]
        uint TranslateUrl(int dwTranslate, [MarshalAs(UnmanagedType.LPWStr)] string strURLIn, [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);

        [PreserveSig]
        uint FilterDataObject(IDataObject pDO, out IDataObject ppDORet);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DOCHOSTUIINFO
    {
        public int cbSize;
        public int dwFlags;
        public int dwDoubleClick;
        public IntPtr dwReserved1;
        public IntPtr dwReserved2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct COMRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class POINT
    {
        public int x;
        public int y;
    }

    [ComImport, Guid("3050F3F0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICustomDoc
    {
        [PreserveSig]
        int SetUIHandler(IDocHostUIHandler pUIHandler);
    }

    [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleServiceProvider
    {
        [PreserveSig]
        uint QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Navigation;

namespace Commander
{
    public class WebBrowserHostUIHandler : IDocHostUIHandler
    {
        private const uint E_NOTIMPL = 0x80004001;
        private const uint S_OK = 0;
        private const uint S_FALSE = 1;

        public WebBrowserHostUIHandler(System.Windows.Controls.WebBrowser browser)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            Browser = browser;
            browser.LoadCompleted += OnLoadCompleted;
            browser.Navigating += browser_Navigating;
            browser.Navigated += OnNavigated;
            IsWebBrowserContextMenuEnabled = true;
            Flags |= HostUIFlags.ENABLE_REDIRECT_NOTIFICATION | HostUIFlags.DPI_AWARE | HostUIFlags.THEME;
            ICustomDoc doc = Browser.Document as ICustomDoc;
            if (doc != null)
            {
                doc.SetUIHandler(this);
            }
        }


        void browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            ICustomDoc doc = Browser.Document as ICustomDoc;
            if (doc != null)
            {
                doc.SetUIHandler(this);
            }
        }

        public System.Windows.Controls.WebBrowser Browser { get; private set; }
        public HostUIFlags Flags { get; set; }
        public bool IsWebBrowserContextMenuEnabled { get; set; }
        public bool ScriptErrorsSuppressed { get; set; }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            ICustomDoc doc = Browser.Document as ICustomDoc;
            if (doc != null)
            {
                doc.SetUIHandler(this);
            }
            SetSilent(Browser, ScriptErrorsSuppressed);
        }

        private void OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            ICustomDoc doc = Browser.Document as ICustomDoc;
            if (doc != null)
            {
                doc.SetUIHandler(this);
            }
        }

        uint IDocHostUIHandler.ShowContextMenu(int dwID, POINT pt, object pcmdtReserved, object pdispReserved)
        {
            return IsWebBrowserContextMenuEnabled ? S_FALSE : S_OK;
        }

        uint IDocHostUIHandler.GetHostInfo(ref DOCHOSTUIINFO info)
        {
            info.dwFlags = (int)Flags;
            info.dwDoubleClick = 0;
            return S_OK;
        }

        uint IDocHostUIHandler.ShowUI(int dwID, object activeObject, object commandTarget, object frame, object doc)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.HideUI()
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.UpdateUI()
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.EnableModeless(bool fEnable)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.ResizeBorder(COMRECT rect, object doc, bool fFrameWindow)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.TranslateAccelerator(ref System.Windows.Forms.Message msg, ref Guid group, int nCmdID)
        {
            return S_FALSE;
        }

        uint IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw)
        {
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.GetDropTarget(object pDropTarget, out object ppDropTarget)
        {
            ppDropTarget = null;
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.GetExternal(out object ppDispatch)
        {
            ppDispatch = Browser.ObjectForScripting;
            return S_OK;
        }

        uint IDocHostUIHandler.TranslateUrl(int dwTranslate, string strURLIn, out string pstrURLOut)
        {
            pstrURLOut = null;
            return E_NOTIMPL;
        }

        uint IDocHostUIHandler.FilterDataObject(IDataObject pDO, out IDataObject ppDORet)
        {
            ppDORet = null;
            return E_NOTIMPL;
        }

        public static void SetSilent(System.Windows.Controls.WebBrowser browser, bool silent)
        {
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }
    }
}

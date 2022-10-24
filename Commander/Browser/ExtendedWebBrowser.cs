using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Commander
{
    public static class ExtendedWebBrowser
    {
        public static void Initialize(this WebBrowser browser, Window window, Uri source)
        {
            browser.PreviewKeyDown += Browser_PreviewKeyDown;
            ExtendedWebBrowser.window = window;
            wbHandler = new WebBrowserHostUIHandler(browser);
            wbHandler.IsWebBrowserContextMenuEnabled = false;
            wbHandler.ScriptErrorsSuppressed = true;
            wbHandler.Flags |= HostUIFlags.ENABLE_REDIRECT_NOTIFICATION | HostUIFlags.DPI_AWARE;
            browser.Navigated += browser_Navigated;
            ExtendedWebBrowser.source = source;
            ExtendedWebBrowser.browser = browser;
            browser.Source = new Uri("about:blank");
        }

        static async void Browser_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    e.Handled = true;
                    await Task.Factory.StartNew(() => browser.InvokeScript("controlTab"), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch { }
        }
        
        static void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                if (first)
                {
                    first = false;
                    ICustomDoc doc = browser.Document as ICustomDoc;
                    if (doc != null)
                        doc.SetUIHandler(wbHandler);
                    browser.Source = source;
                }

                WebBrowserHostUIHandler.SetSilent(browser, wbHandler.ScriptErrorsSuppressed);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        // TODO: AttachedProperty von WebBrowser
        static Window window;
        static WebBrowserHostUIHandler wbHandler;
        static WebBrowser browser;
        static Uri source;
        static bool first = true;
    }
}

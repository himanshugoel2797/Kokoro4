using CefSharp;
using CefSharp.OffScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIRenderer
    {
        static UIRenderer()
        {
            Cef.Initialize(new CefSettings() { UserAgent = EngineManager.EngineName });
        }

        private ChromiumWebBrowser browser;

        public UIRenderer(string url)
        {
            browser = new ChromiumWebBrowser(url,
                new BrowserSettings()
                {
                    JavascriptOpenWindows = CefState.Disabled,
                    JavascriptCloseWindows = CefState.Disabled,
                    OffScreenTransparentBackground = true,
                    WebGl = CefState.Disabled,
                    Plugins = CefState.Disabled,
                    TabToLinks = CefState.Disabled
                });

        }
    }
}

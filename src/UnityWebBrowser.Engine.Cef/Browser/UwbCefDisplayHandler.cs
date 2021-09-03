using System;
using UnityWebBrowser.Engine.Shared;
using UnityWebBrowser.Shared;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Browser
{
    /// <summary>
    ///     <see cref="CefDisplayHandler" /> implementation
    /// </summary>
    public class UwbCefDisplayHandler : CefDisplayHandler
    {
        private readonly IClient client;

        public UwbCefDisplayHandler(UwbCefClient client)
        {
            this.client = client.client;
        }

        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url)
        {
            Logger.Debug($"URL Change: {url}");
            client.UrlChange(url);
        }

        protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen)
        {
            client.Fullscreen(fullscreen);
        }

        protected override bool OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type,
            CefCursorInfo customCursorInfo)
        {
            //TODO: Possibly implement events for this
            return base.OnCursorChange(browser, cursorHandle, type, customCursorInfo);
        }

        protected override void OnTitleChange(CefBrowser browser, string title)
        {
            client.TitleChange(title);
        }

        protected override void OnFaviconUrlChange(CefBrowser browser, string[] iconUrls)
        {
            //TODO: Implement events for this
            base.OnFaviconUrlChange(browser, iconUrls);
        }

        protected override void OnLoadingProgressChange(CefBrowser browser, double progress)
        {
            client.ProgressChange(progress);
        }

        protected override bool OnConsoleMessage(CefBrowser browser, CefLogSeverity level, string message,
            string source, int line)
        {
            switch (level)
            {
                case CefLogSeverity.Disable:
                    break;
                case CefLogSeverity.Default:
                case CefLogSeverity.Info:
                    Logger.Info($"CEF: {message}");
                    break;
                case CefLogSeverity.Warning:
                    Logger.Warn($"CEF: {message}");
                    break;
                case CefLogSeverity.Error:
                case CefLogSeverity.Fatal:
                    Logger.Error($"CEF: {message}");
                    break;
                case CefLogSeverity.Verbose:
                    Logger.Debug($"CEF: {message}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            return true;
        }
    }
}
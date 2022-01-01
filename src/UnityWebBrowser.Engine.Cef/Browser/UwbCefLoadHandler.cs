using System.Linq;
using UnityWebBrowser.Engine.Cef.Core;
using UnityWebBrowser.Engine.Shared;
using UnityWebBrowser.Engine.Shared.Core.Logging;
using Xilium.CefGlue;

namespace UnityWebBrowser.Engine.Cef.Browser
{
    /// <summary>
    ///     <see cref="CefLoadHandler" /> implementation
    /// </summary>
    public class UwbCefLoadHandler : CefLoadHandler
    {
        private readonly UwbCefClient client;
        private readonly string[] ignoredLoadUrls = { "about:blank" };

        internal UwbCefLoadHandler(UwbCefClient client)
        {
            this.client = client;
        }

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            string url = frame.Url;
            if (ignoredLoadUrls.Contains(url))
                return;

            CefLoggerWrapper.Debug($"Loading: {url}");

            if (frame.IsMain) client.client.LoadStart(url);
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            string url = frame.Url;
            if (ignoredLoadUrls.Contains(url))
                return;

            CefLoggerWrapper.Debug($"Loaded: {url}");

            if (frame.IsMain) client.client.LoadFinish(url);
        }

        protected override void OnLoadError(CefBrowser browser, CefFrame frame, CefErrorCode errorCode,
            string errorText, string failedUrl)
        {
            if (errorCode is CefErrorCode.Aborted
                or CefErrorCode.BLOCKED_BY_RESPONSE
                or CefErrorCode.BLOCKED_BY_CLIENT
                or CefErrorCode.BLOCKED_BY_CSP)
                return;

            //TODO: We should move this to an internal scheme page thingy
            string html =
                $@"<style>
@import url('https://fonts.googleapis.com/css2?family=Ubuntu&display=swap');
body {{
font-family: 'Ubuntu', sans-serif;
}}
</style>
<h2>An error occurred while trying to load '{failedUrl}'!</h2>
<p>Error: {errorText}<br>(Code: {(int)errorCode})</p>";
            client.LoadHtml(html);

            CefLoggerWrapper.Error(
                $"An error occurred while trying to load '{failedUrl}'! Error: {errorText} (Code: {errorCode})");
        }
    }
}
﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using Xilium.CefGlue;

namespace UnityWebBrowserServer
{
	public class OffscreenCEFClient : CefClient, IDisposable
	{
		private CefBrowserHost host;
		private OffscreenLoadHandler loadHandler;
		private OffscreenRenderHandler renderHandler;
		private CefSize size;

		private static readonly object sPixelLock = new object();
		private byte[] sPixelBuffer;

		public OffscreenCEFClient(CefSize size)
		{
			loadHandler = new OffscreenLoadHandler(this);
			renderHandler = new OffscreenRenderHandler(this);

			sPixelBuffer = new byte[size.Width * size.Height * 4];

			this.size = size;
		}

		public void Dispose()
		{
			host?.CloseBrowser(true);
			host?.Dispose();
		}

		public byte[] GetPixels()
		{
			if(host == null)
				return new byte[0];

			byte[] pixelBytes = new byte[sPixelBuffer.Length];
			lock (sPixelLock)
			{
				Array.Copy(sPixelBuffer, pixelBytes, sPixelBuffer.Length);
			}
			return pixelBytes;
		}

		protected override CefLoadHandler GetLoadHandler()
		{
			return loadHandler;
		}

		protected override CefRenderHandler GetRenderHandler()
		{
			return renderHandler;
		}

		internal class OffscreenLoadHandler : CefLoadHandler
		{
			private readonly OffscreenCEFClient client;

			internal OffscreenLoadHandler(OffscreenCEFClient client)
			{
				this.client = client;
			}

			protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
			{
				if (browser != null)
					client.host = browser.GetHost();

				if(frame.IsMain)
					Console.WriteLine($"START: {browser?.GetMainFrame().Url}");
			}

			protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
			{
				if(frame.IsMain)
					Console.WriteLine($"END: {browser.GetMainFrame().Url}, {httpStatusCode}");
			}
		}

		internal class OffscreenRenderHandler : CefRenderHandler
		{
			private OffscreenCEFClient client;

			internal OffscreenRenderHandler(OffscreenCEFClient client)
			{
				this.client = client;
			}

			protected override CefAccessibilityHandler GetAccessibilityHandler()
			{
				return null;
			}

			protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
			{
				GetViewRect(browser, out CefRectangle newRect);
				rect = newRect;
				return true;
			}

			protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
			{
				screenX = viewX;
				screenY = viewY;
				return true;
			}

			protected override void GetViewRect(CefBrowser browser, out CefRectangle rect)
			{
				rect = new CefRectangle(0, 0, client.size.Width, client.size.Height);
			}

			[SecurityCritical]
			protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width,
				int height)
			{
				if (browser != null)
				{
					lock (sPixelLock)
					{
						Marshal.Copy(buffer, client.sPixelBuffer, 0, client.sPixelBuffer.Length);
					}
				}
			}

			protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
			{
				return false;
			}

			protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
			{
			}

			protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr sharedHandle)
			{
			}

			protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
			{
			}

			protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds)
			{
			}
		}

		public class OffscreenCEFApp : CefApp
		{
		}
	}
}